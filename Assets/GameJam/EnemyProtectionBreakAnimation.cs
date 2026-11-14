using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>Plays the one-shot protection-break sprite clip on an enemy.</summary>
public sealed class EnemyProtectionBreakAnimation : MonoBehaviour
{
    [SerializeField] private AnimationClip protectionBreakClip;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private GameObject vulnerabilityFx;
    [SerializeField] private GameObject crystalObject;
    [SerializeField] private PurificationReveal purificationReveal;
    [SerializeField, Min(0f)] private float illuminationReentryPadding = 0.2f;

    private const string GlitchProperty = "_EnableGlitch";
    private const string GlitchKeyword = "_ENABLEGLITCH_ON";
    private readonly List<GameObject> tentacleObjects = new List<GameObject>();
    private Coroutine playback;
    private Sprite spriteBeforePlayback;
    private bool animatorWasEnabled;
    private Material runtimeMaterial;
    private bool lastProtectedState;
    private bool hasAppliedProtectionState;
    private bool wasInsidePurificationLight;

    private void Awake()
    {
        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
        if (animator == null) animator = GetComponent<Animator>();
        if (targetRenderer == null) targetRenderer = GetComponent<SpriteRenderer>();
        if (purificationReveal == null)
            purificationReveal = FindFirstObjectByType<PurificationReveal>();

        Transform[] descendants = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < descendants.Length; i++)
        {
            Transform child = descendants[i];
            if (crystalObject == null && child != transform &&
                child.name.Equals("cristal", System.StringComparison.OrdinalIgnoreCase))
                crystalObject = child.gameObject;

            if (child != transform && child.name.ToLowerInvariant().Contains("tentacle"))
                tentacleObjects.Add(child.gameObject);
        }
    }

    private void OnEnable()
    {
        if (vulnerabilityFx != null)
            vulnerabilityFx.SetActive(false);
        if (crystalObject != null)
            crystalObject.SetActive(false);

        wasInsidePurificationLight = false;

        if (enemyHealth != null)
            enemyHealth.OnMeleeProtectionBroken += HandleProtectionBroken;

        hasAppliedProtectionState = false;
        SynchronizeProtectionState();
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnMeleeProtectionBroken -= HandleProtectionBroken;

        StopPlayback(restoreState: true);
        hasAppliedProtectionState = false;
    }

    private void LateUpdate()
    {
        // EnemyHealth and pooled-object OnEnable order is not guaranteed; resync after all OnEnable calls.
        TrackPurificationExposure();
        SynchronizeProtectionState();
        SynchronizeCrystalVisibility();
    }

    private void TrackPurificationExposure()
    {
        if (purificationReveal == null)
            return;

        float distance = Vector2.Distance(transform.position, purificationReveal.transform.position);
        float lightRadius = purificationReveal.Radius;
        bool isInsideLight = distance <= lightRadius;

        if (!wasInsidePurificationLight && isInsideLight)
        {
            wasInsidePurificationLight = true;
            if (enemyHealth != null && enemyHealth.IsProtectedUntilPlayerMelee)
            {
                enemyHealth.SetIlluminationsRequiredToBecomeVulnerable(purificationReveal.towerLightExpose);
                enemyHealth.RegisterPurificationIllumination();
            }
        }
        else if (wasInsidePurificationLight && distance > lightRadius + illuminationReentryPadding)
        {
            // Require a clear exit before counting another entry, avoiding duplicate
            // exposures when the light's edge shimmers during tower growth.
            wasInsidePurificationLight = false;
        }
    }

    private void HandleProtectionBroken()
    {
        ApplyProtectionState(false);

        if (vulnerabilityFx != null)
            vulnerabilityFx.SetActive(true);

        PlayProtectionBreakAnimation();
    }

    private void SynchronizeProtectionState()
    {
        if (enemyHealth != null)
            ApplyProtectionState(enemyHealth.IsProtectedUntilPlayerMelee);
    }

    private void ApplyProtectionState(bool isProtected)
    {
        if (hasAppliedProtectionState && lastProtectedState == isProtected)
            return;

        hasAppliedProtectionState = true;
        lastProtectedState = isProtected;
        SetGlitchEnabled(isProtected);

        for (int i = 0; i < tentacleObjects.Count; i++)
        {
            if (tentacleObjects[i] != null)
                tentacleObjects[i].SetActive(isProtected);
        }
    }

    private void SynchronizeCrystalVisibility()
    {
        if (crystalObject == null || enemyHealth == null || purificationReveal == null)
            return;

        Vector2 offset = (Vector2)transform.position - (Vector2)purificationReveal.transform.position;
        bool outsideLight = offset.sqrMagnitude > purificationReveal.Radius * purificationReveal.Radius;
        bool shouldShow = !enemyHealth.IsProtectedUntilPlayerMelee && outsideLight;
        if (crystalObject.activeSelf != shouldShow)
            crystalObject.SetActive(shouldShow);
    }

    private void SetGlitchEnabled(bool enabled)
    {
        if (targetRenderer == null)
            return;

        if (runtimeMaterial == null)
            runtimeMaterial = targetRenderer.material;

        if (runtimeMaterial == null || !runtimeMaterial.HasProperty(GlitchProperty))
            return;

        runtimeMaterial.SetFloat(GlitchProperty, enabled ? 1f : 0f);
        if (enabled)
            runtimeMaterial.EnableKeyword(GlitchKeyword);
        else
            runtimeMaterial.DisableKeyword(GlitchKeyword);
    }

    private void PlayProtectionBreakAnimation()
    {
        if (protectionBreakClip == null || targetRenderer == null)
        {
            Debug.LogWarning("Protection-break animation is not configured.", this);
            return;
        }

        StopPlayback(restoreState: true);
        spriteBeforePlayback = targetRenderer.sprite;
        animatorWasEnabled = animator != null && animator.enabled;
        if (animator != null) animator.enabled = false;
        playback = StartCoroutine(PlayClip());
    }

    private IEnumerator PlayClip()
    {
        float elapsed = 0f;
        float clipLength = Mathf.Max(protectionBreakClip.length, 0.01f);

        while (elapsed < clipLength)
        {
            protectionBreakClip.SampleAnimation(gameObject, elapsed);
            yield return null;
            elapsed += Time.deltaTime;
        }

        FinishPlayback();
    }

    private void StopPlayback(bool restoreState)
    {
        if (playback != null)
        {
            StopCoroutine(playback);
            playback = null;
            if (restoreState) FinishPlayback();
        }
    }

    private void FinishPlayback()
    {
        if (targetRenderer != null && spriteBeforePlayback != null)
            targetRenderer.sprite = spriteBeforePlayback;

        if (animator != null)
            animator.enabled = animatorWasEnabled;

        playback = null;
    }

    private void OnDestroy()
    {
        if (runtimeMaterial == null)
            return;

        if (Application.isPlaying)
            Destroy(runtimeMaterial);
        else
            DestroyImmediate(runtimeMaterial);
    }
}
