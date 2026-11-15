using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

/// <summary>Simple generated blue-crystal pickup; no external art asset required.</summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public sealed class BlueCrystalPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int value = 1;
    [SerializeField, Min(0.1f)] private float pickupRadius = 0.8f;
    [SerializeField, Min(0.1f)] private float followLerpSpeed = 12f;
    [SerializeField] private float rotationSpeed = 35f;
    private static Sprite crystalSprite;
    private SpriteRenderer spriteRenderer;
    private Collider2D pickupCollider;
    private Rigidbody2D body;
    private MMAutoRotate playerOrbit;
    private BlueCrystalInventory carrier;
    private int followIndex;
    private bool isCarried;
    private bool isOrbiting;
    private Vector3 orbitCenter;
    private float orbitRadius;
    private float orbitSpeed;
    private float orbitPhase;
    private bool isAbsorbing;
    private float absorbElapsed;
    private float absorbDuration;
    private Vector3 absorbStart;
    private Vector3 absorbTarget;
    private Vector3 absorbStartScale;
    private MMF_Player skillCheckFailurePlayer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        pickupCollider = GetComponent<Collider2D>();
        ApplyPickupRadius();
        body = GetComponent<Rigidbody2D>();
        skillCheckFailurePlayer = GetComponent<MMF_Player>();
        playerOrbit = GetComponent<MMAutoRotate>();

        if (spriteRenderer != null)
        {
            // Keep artwork assigned on the prefab. Generate a simple fallback only
            // when no sprite was assigned, and preserve the Inspector tint.
            if (spriteRenderer.sprite == null)
                spriteRenderer.sprite = GetCrystalSprite();
            spriteRenderer.sortingOrder = 20;
        }
    }

    private void OnValidate()
    {
        pickupRadius = Mathf.Max(0.1f, pickupRadius);
        ApplyPickupRadius();
    }

    private void ApplyPickupRadius()
    {
        CircleCollider2D circle = pickupCollider as CircleCollider2D;
        if (circle == null)
            circle = GetComponent<CircleCollider2D>();
        if (circle != null)
            circle.radius = Mathf.Max(0.1f, pickupRadius);
    }

    public static void Spawn(Vector3 position, int value = 1)
    {
        BlueCrystalPickup prefab = Resources.Load<BlueCrystalPickup>("BlueCrystal");
        if (prefab == null)
        {
            Debug.LogError("Blue crystal prefab is missing. Expected Assets/TINS - New Mechanics/Resources/BlueCrystal.prefab.");
            return;
        }

        BlueCrystalPickup pickup = Instantiate(prefab, position, prefab.transform.rotation);
        pickup.value = Mathf.Max(1, value);
        pickup.name = "Blue Crystal";
    }

    private static Sprite GetCrystalSprite()
    {
        if (crystalSprite == null)
            crystalSprite = Sprite.Create(Texture2D.whiteTexture,
                new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f), 1f);
        return crystalSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCarried) return;

        var inventory = other.GetComponentInParent<BlueCrystalInventory>();
        if (inventory == null) return;
        inventory.Collect(this, value);
    }

    public void SetAsCarried(BlueCrystalInventory inventory, int index)
    {
        carrier = inventory;
        followIndex = index;
        isCarried = true;
        isOrbiting = false;
        isAbsorbing = false;

        if (pickupCollider != null)
            pickupCollider.enabled = false;
        if (body != null)
            body.simulated = false;
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 25 + index;

        ConfigurePlayerOrbit();
    }

    private void ConfigurePlayerOrbit()
    {
        if (carrier == null) return;

        if (playerOrbit == null)
            playerOrbit = gameObject.AddComponent<MMAutoRotate>();

        float phase = carrier.GetPlayerOrbitPhase(followIndex);
        float angleRadians = phase * Mathf.Deg2Rad;
        float radius = carrier.PlayerOrbitRadius;
        Vector3 center = carrier.transform.position;

        // MMAutoRotate advances from the crystal's current position, so place it
        // on its orbit first to give every collected crystal a distinct phase.
        transform.position = center + new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians), 0f) * radius;

        playerOrbit.Rotating = true;
        playerOrbit.RotationSpace = Space.Self;
        playerOrbit.RotationSpeed = new Vector3(0f, 0f, rotationSpeed);
        playerOrbit.UpdateMode = MMAutoRotate.UpdateModes.Update;
        playerOrbit.OrbitCenterTransform = carrier.transform;
        playerOrbit.OrbitCenterOffset = Vector3.zero;
        playerOrbit.OrbitRotationAxis = Vector3.forward;
        playerOrbit.OrbitRadius = radius;
        playerOrbit.OrbitRotationSpeed = carrier.GetPlayerOrbitSpeed(followIndex);
        playerOrbit.OrbitCorrectionSpeed = 12f;
        playerOrbit.AdditiveOrbitRotation = false;
        playerOrbit.Orbit(true);
    }

    public void SetOrbit(Vector3 center, float radius, float degreesPerSecond, float phase)
    {
        if (playerOrbit != null)
            playerOrbit.Orbit(false);

        isOrbiting = true;
        orbitCenter = center;
        orbitRadius = Mathf.Max(0f, radius);
        orbitSpeed = degreesPerSecond;
        orbitPhase = phase;
    }

    public void BeginAbsorption(Vector3 center, float duration)
    {
        if (playerOrbit != null)
            playerOrbit.Orbit(false);

        isOrbiting = false;
        isAbsorbing = true;
        absorbElapsed = 0f;
        absorbDuration = Mathf.Max(0.01f, duration);
        absorbStart = transform.position;
        absorbTarget = center;
        absorbStartScale = transform.localScale;
    }

    public void PlaySkillCheckFailureFeedback()
    {
        if (skillCheckFailurePlayer != null)
            skillCheckFailurePlayer.PlayFeedbacks(transform.position);
    }

    public void PlayCollectSound()
    {
        TowerAudioFeedbacks.Instance?.PlayCrystalPickup();
    }

    private void LateUpdate()
    {
        if (!isCarried || carrier == null)
            return;

        if (isAbsorbing)
        {
            absorbElapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(absorbElapsed / absorbDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.position = Vector3.LerpUnclamped(absorbStart, absorbTarget, eased);
            transform.localScale = Vector3.LerpUnclamped(absorbStartScale, Vector3.zero, eased);
            return;
        }

        if (isOrbiting)
        {
            float angle = (Time.unscaledTime * orbitSpeed + orbitPhase) * Mathf.Deg2Rad;
            Vector3 orbitTarget = orbitCenter + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * orbitRadius;
            transform.position = InterpolateTowards(transform.position, orbitTarget, Time.unscaledDeltaTime);
            transform.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);
            return;
        }

        // The More Mountains MMAutoRotate component owns the normal carried
        // motion. The fallback only covers an unexpected missing component.
        if (playerOrbit == null || !playerOrbit.Orbiting)
            ConfigurePlayerOrbit();
    }

    private Vector3 InterpolateTowards(Vector3 current, Vector3 target, float deltaTime)
    {
        float t = 1f - Mathf.Exp(-Mathf.Max(0.1f, followLerpSpeed) * deltaTime);
        return Vector3.Lerp(current, target, t);
    }
}
