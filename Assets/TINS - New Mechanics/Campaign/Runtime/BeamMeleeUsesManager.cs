using System;
using UnityEngine;

/// <summary>Tracks Flash charges and restores them automatically, one at a time.</summary>
public sealed class BeamMeleeUsesManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxUses = 8;
    [SerializeField, Min(0.1f)] private float rechargeDurationPerCharge = 5f;
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
    public bool IsRecharging => UsesRemaining < MaxUses;
    public float RechargeProgress => !IsRecharging ? 1f : rechargeTimer <= 0f
        ? 0f : 1f - Mathf.Clamp01(rechargeTimer / rechargeDurationPerCharge);

    public float SkillCheckTarget => Mathf.Clamp(skillCheckTarget, skillCheckWidth * 0.5f, 1f - skillCheckWidth * 0.5f);
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
        if (rechargeTimer <= 0f) BeginChargeRecharge();

        rechargeTimer -= Time.deltaTime;
        if (rechargeTimer <= 0f) FinishOneCharge();
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
            if (LastNoChargeAttemptSucceeded) FinishOneCharge();
            else
            {
                skillCheckFailedThisCharge = true;
                TowerAudioFeedbacks.Instance?.PlaySkillCheckError();
            }
            return false;
        }

        UsesRemaining--;
        if (rechargeTimer <= 0f) BeginChargeRecharge();
        UpdateHeadAnimation();
        OnUseSpent?.Invoke(UsesRemaining);
        OnUsesChanged?.Invoke(UsesRemaining, MaxUses);
        return true;
    }

    private void BeginChargeRecharge()
    {
        rechargeTimer = rechargeDurationPerCharge;
        skillCheckFailedThisCharge = false;
    }

    private void FinishOneCharge()
    {
        if (!IsRecharging) return;
        UsesRemaining++;
        rechargeTimer = 0f;
        skillCheckFailedThisCharge = false;
        if (IsRecharging) BeginChargeRecharge();
        UpdateHeadAnimation();
        OnUsesChanged?.Invoke(UsesRemaining, MaxUses);
        TowerAudioFeedbacks.Instance?.PlayBeamRecharge();
    }

    public void RestoreUses(int amount)
    {
        if (amount <= 0) return;
        int previous = UsesRemaining;
        UsesRemaining = Mathf.Clamp(UsesRemaining + amount, 0, MaxUses);
        if (UsesRemaining == previous) return;
        rechargeTimer = 0f;
        if (IsRecharging) BeginChargeRecharge();
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
        UsesRemaining = Mathf.Clamp(UsesRemaining + maxUses - previousMaximum, 0, MaxUses);
        UpdateHeadAnimation();
        OnUsesChanged?.Invoke(UsesRemaining, MaxUses);
    }

    public float RechargeDurationPerCharge => rechargeDurationPerCharge;

    public void ConfigureRechargeDuration(float duration)
    {
        rechargeDurationPerCharge = Mathf.Max(0.1f, duration);
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
