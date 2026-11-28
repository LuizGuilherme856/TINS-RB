using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Reveals the purified floor inside a circular radius and keeps a 2D point light
/// matched to that area. Put the corrupted SpriteRenderer above the purified one.
/// </summary>
[ExecuteAlways]
public sealed class PurificationReveal : MonoBehaviour
{
    [Min(0.01f)]
    [SerializeField] public float radius = 2f;
    [Min(0.01f)] public float WinRadius = 10f;
    public UnityEvent OnWinGame = new UnityEvent();
    public bool NotifyAtWinRadius { get; set; } = true;
    [Min(0f)]
    [Tooltip("Extra light radius beyond the purification edge, used only for the soft falloff.")]
    [SerializeField] private float glowLightPadding = 0.75f;
    [Header("Enemy effects")]
    [Range(0f, 100f)]
    [Tooltip("Percentage of enemy movement speed removed while inside this tower's purification radius.")]
    public float enemySlowDownPercentage = 30f;
    [Min(1)]
    [Tooltip("Number of separate entries into the tower light required to expose a protected enemy.")]
    public int towerLightExpose = 1;
    [SerializeField] private SpriteMask revealMask;
    [SerializeField] private Light2D purificationLight;
    [SerializeField] private SpriteRenderer corruptedFloor;
    [SerializeField] private SpriteRenderer edgeGlow;
    private bool hasInvokedWinGame;

    public Light2D PurificationLight => purificationLight;

    public float Radius
    {
        get => radius;
        set
        {
            radius = Mathf.Max(0.01f, value);
            ApplyRadius();
        }
    }

    public float EnemySpeedMultiplierInsideLight => 1f - Mathf.Clamp(enemySlowDownPercentage, 0f, 100f) / 100f;

    private void OnEnable()
    {
        ApplyMaskToRoots();
        ApplyRadius();
    }

    private void OnValidate()
    {
        radius = Mathf.Max(0.01f, radius);
        ApplyMaskToRoots();
        ApplyRadius();
    }

    private void ApplyRadius()
    {

        if (corruptedFloor != null)
        {
            corruptedFloor.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        }

        if (revealMask != null)
        {
            if (corruptedFloor != null)
            {
                // Cover the full order range on the corrupted sprite's sorting layer.
                // The default SpriteMask range of order 0 only fails as soon as a
                // scene's floor renderers are drawn at a different order.
                revealMask.backSortingLayerID = corruptedFloor.sortingLayerID;
                revealMask.frontSortingLayerID = corruptedFloor.sortingLayerID;
            }
            revealMask.backSortingOrder = short.MinValue;
            revealMask.frontSortingOrder = short.MaxValue;
            revealMask.isCustomRangeActive = true;
            revealMask.transform.localScale = Vector3.one * (radius * 2f);
        }

        if (edgeGlow != null)
        {
            edgeGlow.transform.localScale = Vector3.one * (radius * 2f);
            if (corruptedFloor != null)
            {
                edgeGlow.sortingLayerID = corruptedFloor.sortingLayerID;
                edgeGlow.sortingOrder = corruptedFloor.sortingOrder + 1;
            }
        }

        if (purificationLight != null)
        {
            // Keep the fully-lit inner area aligned with the reveal, then fade out
            // through a wider outer radius. The inner/outer gap defines the penumbra.
            purificationLight.pointLightOuterRadius = radius + glowLightPadding;
            purificationLight.pointLightInnerRadius = radius;
        }

        if (Application.isPlaying && NotifyAtWinRadius && !hasInvokedWinGame && radius >= Mathf.Max(0.01f, WinRadius))
        {
            hasInvokedWinGame = true;
            OnWinGame?.Invoke();
        }
    }

    private static void ApplyMaskToRoots()
    {
        // Roots are scene decorations rather than purification tiles, so they
        // need to opt into the same SpriteMask explicitly.
        GameObject[] sceneObjects = FindObjectsOfType<GameObject>(true);
        foreach (GameObject sceneObject in sceneObjects)
        {
            if (!sceneObject.name.StartsWith("roots", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            SpriteRenderer[] renderers = sceneObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer renderer in renderers)
            {
                renderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            }
        }
    }
}
