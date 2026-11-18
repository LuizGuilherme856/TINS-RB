using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>Reloads the full Flash supply when its last charge is spent.</summary>
public sealed class BeamMeleeUsesManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxUses = 8;
    [FormerlySerializedAs("rechargeDurationPerCharge")]
    [SerializeField, Min(0.1f)] private float rechargeDuration = 5f;
    [SerializeField, Range(0f, 1f)] private float skillCheckTarget = 0.8f;
    [SerializeField, Range(0.02f, 0.5f)] private float skillCheckWidth = 0.296f;
    [Header("Player head charge animation")]
    [SerializeField] private Animator headAnimator;
    [SerializeField] private string[] headStateNames = { "PlayerHeadState1", "PlayerHeadState2", "PlayerHeadState3", "PlayerHeadState4" };

    public event Action<int, int> OnUsesChanged;
    public event Action<int> OnUseSpent;
    public int UsesRemaining { get; private set; }
    public int MaxUses => Mathf.Max(1, maxUses);
    public bool LastNoChargeAttemptResolvedSkillCheck { get; private set; }
    public bool LastNoChargeAttemptSucceeded { get; private set; }
    public bool IsRecharging => UsesRemaining == 0;
    public float RechargeProgress => IsRecharging ? 1f - Mathf.Clamp01(rechargeTimer / rechargeDuration) : 1f;

    public float SkillCheckWidth => skillCheckWidth;
    public float SkillCheckTarget => Mathf.Clamp(skillCheckTarget, SkillCheckWidth * 0.5f, 1f - SkillCheckWidth * 0.5f);
    public bool IsSkillCheckAvailable => UsesRemaining == 0 && rechargeTimer > 0f && !skillCheckFailedThisCharge;

    private float rechargeTimer;
    private bool skillCheckFailedThisCharge;

    private void Awake()
    {
        if (headAnimator == null) headAnimator = FindHeadAnimator();
        UsesRemaining = MaxUses;
        UpdateHeadAnimation();
    }

    private void OnEnable()
    {
        UsesRemaining = MaxUses;
        rechargeTimer = 0f;
        skillCheckFailedThisCharge = false;
        UpdateHeadAnimation();
        OnUsesChanged?.Invoke(UsesRemaining, MaxUses);
    }

    private void Update()
    {
        if (!IsRecharging) return;
        rechargeTimer -= Time.deltaTime;
        if (rechargeTimer <= 0f) RefillUses();
    }

    public bool TryConsumeUse()
    {
        LastNoChargeAttemptResolvedSkillCheck = false;
        LastNoChargeAttemptSucceeded = false;
        if (UsesRemaining <= 0)
        {
            if (!IsSkillCheckAvailable) return false;

            LastNoChargeAttemptResolvedSkillCheck = true;
            LastNoChargeAttemptSucceeded = Mathf.Abs(RechargeProgress - SkillCheckTarget) <= skillCheckWidth * 0.5f;
            if (LastNoChargeAttemptSucceeded) RefillUses();
            else
            {
                skillCheckFailedThisCharge = true;
                TowerAudioFeedbacks.Instance?.PlaySkillCheckError();
            }
            return false;
        }

        UsesRemaining--;
        if (IsRecharging) BeginRecharge();
        UpdateHeadAnimation();
        OnUseSpent?.Invoke(UsesRemaining);
        OnUsesChanged?.Invoke(UsesRemaining, MaxUses);
        return true;
    }

    private void BeginRecharge()
    {
        rechargeTimer = rechargeDuration;
        skillCheckFailedThisCharge = false;
    }

    public void RestoreUses(int amount)
    {
        if (amount <= 0) return;
        int previous = UsesRemaining;
        UsesRemaining = Mathf.Clamp(UsesRemaining + amount, 0, MaxUses);
        if (UsesRemaining == previous) return;
        rechargeTimer = 0f;
        skillCheckFailedThisCharge = false;
        UpdateHeadAnimation();
        OnUsesChanged?.Invoke(UsesRemaining, MaxUses);
        TowerAudioFeedbacks.Instance?.PlayBeamRecharge();
    }

    public void RefillUses()
    {
        bool needed = UsesRemaining < MaxUses;
        UsesRemaining = MaxUses;
        rechargeTimer = 0f;
        skillCheckFailedThisCharge = false;
        UpdateHeadAnimation();
        OnUsesChanged?.Invoke(UsesRemaining, MaxUses);
        if (needed) TowerAudioFeedbacks.Instance?.PlayBeamRecharge();
    }

    public void ConfigureMaximumUses(int additionalUses)
    {
        int previousMaximum = maxUses;
        maxUses = Mathf.Max(1, previousMaximum + additionalUses);
        bool wasRecharging = IsRecharging;
        UsesRemaining = Mathf.Clamp(UsesRemaining + maxUses - previousMaximum, 0, MaxUses);
        if (IsRecharging && !wasRecharging) BeginRecharge();
        UpdateHeadAnimation();
        OnUsesChanged?.Invoke(UsesRemaining, MaxUses);
    }

    public float RechargeDuration => rechargeDuration;

    public void ConfigureRechargeDuration(float duration)
    {
        float remainingFraction = 1f - RechargeProgress;
        rechargeDuration = Mathf.Max(0.1f, duration);
        if (IsRecharging) rechargeTimer = rechargeDuration * remainingFraction;
    }

    private void UpdateHeadAnimation()
    {
        if (headAnimator == null || !headAnimator.isActiveAndEnabled || headStateNames == null || headStateNames.Length < 4) return;
        int stateIndex = UsesRemaining <= 2 ? 0 : (UsesRemaining - 1) / 2;
        stateIndex = Mathf.Clamp(stateIndex, 0, 3);
        string stateName = headStateNames[stateIndex];
        if (!string.IsNullOrEmpty(stateName)) headAnimator.Play(stateName, 0, 0f);
    }

    private Animator FindHeadAnimator()
    {
        Animator[] animators = GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
            if (animators[i] != null && animators[i].name.ToLowerInvariant().Contains("head")) return animators[i];
        return null;
    }
}
