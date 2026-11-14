using UnityEngine;

/// <summary>Plays the tower's starN animation state as its purification radius grows.</summary>
[DisallowMultipleComponent]
public sealed class TowerRadiusStageAnimation : MonoBehaviour
{
    [SerializeField] private PurificationReveal purificationReveal;
    [SerializeField] private Animator towerAnimator;
    [SerializeField, Min(1)] private int stageCount = 7;
    [SerializeField, Min(0.01f)] private float firstStageRadius = 3.5f;
    [SerializeField, Min(0.01f)] private float finalStageRadius = 28f;
    [SerializeField] private string statePrefix = "star";

    private int currentStage;

    private void Awake()
    {
        ResolveReferences();
        UpdateStage(true);
    }

    private void OnEnable()
    {
        ResolveReferences();
        UpdateStage(true);
    }

    private void Start()
    {
        // Re-apply after all scene components, including the child Animator, have initialized.
        ResolveReferences();
        UpdateStage(true);
    }

    private void Update()
    {
        if (purificationReveal != null)
            UpdateStage(false);
    }

    private void OnValidate()
    {
        stageCount = Mathf.Max(1, stageCount);
        firstStageRadius = Mathf.Max(0.01f, firstStageRadius);
        finalStageRadius = Mathf.Max(firstStageRadius + 0.01f, finalStageRadius);
        statePrefix = string.IsNullOrWhiteSpace(statePrefix) ? "star" : statePrefix.Trim();
    }

    private void ResolveReferences()
    {
        if (purificationReveal == null)
            purificationReveal = GetComponentInChildren<PurificationReveal>(true);
        if (purificationReveal == null)
            purificationReveal = FindFirstObjectByType<PurificationReveal>(FindObjectsInactive.Include);

        if (towerAnimator == null)
            towerAnimator = GetComponentInChildren<Animator>(true);
    }

    private void UpdateStage(bool force)
    {
        if (purificationReveal == null || towerAnimator == null || towerAnimator.runtimeAnimatorController == null)
            return;

        int stage = CalculateStage(purificationReveal.Radius);
        if (!force && stage == currentStage)
            return;

        string animationName = statePrefix + stage;
        currentStage = stage;
        towerAnimator.Play(animationName);
    }

    private int CalculateStage(float radius)
    {
        if (stageCount <= 1 || radius <= firstStageRadius)
            return 1;
        if (radius >= finalStageRadius)
            return stageCount;

        float progress = Mathf.InverseLerp(firstStageRadius, finalStageRadius, radius);
        return Mathf.Clamp(Mathf.FloorToInt(progress * (stageCount - 1)) + 1, 1, stageCount);
    }
}
