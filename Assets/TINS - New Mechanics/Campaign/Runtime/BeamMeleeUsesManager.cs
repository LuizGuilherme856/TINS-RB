using System;
using UnityEngine;
using TINS.Campaign;

/// <summary>Reloads the full Flash supply when its last charge is spent.</summary>
public sealed class BeamMeleeUsesManager : MonoBehaviour
{
    [Header("Player head charge animation")]
    [SerializeField] private Animator headAnimator;
    [SerializeField] private string[] headStateNames = { "PlayerHeadState1", "PlayerHeadState2", "PlayerHeadState3", "PlayerHeadState4" };

    public event Action<int, int> OnUsesChanged;
    public event Action<int> OnUseSpent;
    public int UsesRemaining { get; private set; }
    public int MaxUses => Mathf.Max(1, FlashData.Shared.maxUses + additionalUses);
    public bool LastNoChargeAttemptResolvedSkillCheck { get; private set; }
    public bool LastNoChargeAttemptSucceeded { get; private set; }
    public bool IsRecharging => isActiveAndEnabled && UsesRemaining == 0;
    public float RechargeProgress => IsRecharging ? 1f - Mathf.Clamp01(rechargeTimer / RechargeDuration) : 1f;

    public float SkillCheckWidth => FlashData.Shared.skillCheckWidth;
    public float SkillCheckTarget => Mathf.Clamp(FlashData.Shared.skillCheckTarget, SkillCheckWidth * 0.5f, 1f - SkillCheckWidth * 0.5f);
    public bool IsSkillCheckAvailable => IsRecharging && rechargeTimer > 0f && !skillCheckFailedThisCharge;

    private int additionalUses;
    private float? configuredRechargeDuration;
    private float rechargeTimer;
    private bool skillCheckFailedThisCharge;

    private void Awake()
    {
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

    private void OnDisable()
    {
        UsesRemaining = 0;
        rechargeTimer = 0f;
        OnUsesChanged?.Invoke(0, MaxUses);
    }

    private void Update()
    {
        if (!IsRecharging) return;
        rechargeTimer -= Time.deltaTime;
        if (rechargeTimer <= 0f) RefillUses();
    }

    public bool TryConsumeUse()
    {
        if (!isActiveAndEnabled) return false;
        LastNoChargeAttemptResolvedSkillCheck = false;
        LastNoChargeAttemptSucceeded = false;
        if (UsesRemaining <= 0)
        {
            if (!IsSkillCheckAvailable) return false;

            LastNoChargeAttemptResolvedSkillCheck = true;
            LastNoChargeAttemptSucceeded = Mathf.Abs(RechargeProgress - SkillCheckTarget) <= SkillCheckWidth * 0.5f;
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
        rechargeTimer = RechargeDuration;
        skillCheckFailedThisCharge = false;
    }

    public void RestoreUses(int amount)
    {
        if (!isActiveAndEnabled || amount <= 0) return;
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
        if (!isActiveAndEnabled) return;
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
        int previousMaximum = MaxUses;
        this.additionalUses = additionalUses;
        bool wasRecharging = IsRecharging;
        UsesRemaining = Mathf.Clamp(UsesRemaining + MaxUses - previousMaximum, 0, MaxUses);
        if (IsRecharging && !wasRecharging) BeginRecharge();
        UpdateHeadAnimation();
        OnUsesChanged?.Invoke(UsesRemaining, MaxUses);
    }

    public float RechargeDuration => configuredRechargeDuration ?? FlashData.Shared.rechargeDuration;

    public void ConfigureRechargeDuration(float duration)
    {
        float remainingFraction = 1f - RechargeProgress;
        configuredRechargeDuration = Mathf.Max(0.1f, duration);
        if (IsRecharging) rechargeTimer = RechargeDuration * remainingFraction;
    }

    private void UpdateHeadAnimation()
    {
        if (headAnimator == null || !headAnimator.isActiveAndEnabled || headStateNames == null || headStateNames.Length < 4) return;
        int stateIndex = UsesRemaining <= 2 ? 0 : (UsesRemaining - 1) / 2;
        stateIndex = Mathf.Clamp(stateIndex, 0, 3);
        string stateName = headStateNames[stateIndex];
        if (!string.IsNullOrEmpty(stateName)) headAnimator.Play(stateName, 0, 0f);
    }
}
