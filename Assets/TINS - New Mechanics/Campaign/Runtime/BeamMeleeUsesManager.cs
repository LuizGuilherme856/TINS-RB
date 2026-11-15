using System;
using UnityEngine;
using GraveyardShift.Gameplay;

/// <summary>Tracks Flash charges and restores them automatically, one at a time.</summary>
public sealed class BeamMeleeUsesManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxUses = 8;
    [SerializeField, Min(0.1f)] private float rechargeDurationPerCharge = 5f;
    [SerializeField, Range(0f, 1f)] private float skillCheckTarget = 0.8f;
    [Header("Player head charge animation")]
    [SerializeField] private Animator headAnimator;
    [SerializeField] private string[] headStateNames = { "PlayerHeadState1", "PlayerHeadState2", "PlayerHeadState3", "PlayerHeadState4" };

    public event Action<int, int> OnUsesChanged;
    public int UsesRemaining { get; private set; }
    public int MaxUses => Mathf.Max(1, maxUses);
    public bool LastNoChargeAttemptResolvedSkillCheck { get; private set; }
    public bool LastNoChargeAttemptSucceeded { get; private set; }
    public bool IsRecharging => UsesRemaining < MaxUses;
    public float RechargeProgress => UsesRemaining >= MaxUses || rechargeDurationPerCharge <= 0f
        ? 1f : 1f - Mathf.Clamp01(rechargeTimer / rechargeDurationPerCharge);

    private SkillCheckBar observedSkillCheck;
    private float nextSkillCheckSearchTime;
    private float rechargeTimer;
    private bool skillCheckFailedThisCharge;
    private bool skillCheckStartedThisCharge;

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
        skillCheckStartedThisCharge = false;
        UpdateHeadAnimation();
        OnUsesChanged?.Invoke(UsesRemaining, MaxUses);
        BindSkillCheck();
    }

    private void OnDisable()
    {
        if (observedSkillCheck != null)
            observedSkillCheck.ResultResolved -= HandleSkillCheckResult;
        observedSkillCheck = null;
    }

    private void Update()
    {
        if (observedSkillCheck == null && Time.unscaledTime >= nextSkillCheckSearchTime)
        {
            nextSkillCheckSearchTime = Time.unscaledTime + 0.5f;
            BindSkillCheck();
        }

        if (UsesRemaining >= MaxUses) return;
        if (rechargeTimer <= 0f)
            BeginChargeRecharge();

        if (UsesRemaining == 0 && !skillCheckFailedThisCharge && !skillCheckStartedThisCharge && observedSkillCheck != null)
        {
            skillCheckStartedThisCharge = true;
            observedSkillCheck.BeginSkillCheckAt(skillCheckTarget, rechargeDurationPerCharge, RechargeProgress);
        }

        rechargeTimer -= Time.deltaTime;
        if (rechargeTimer <= 0f)
            FinishOneCharge();
    }

    public bool TryConsumeUse()
    {
        if (UsesRemaining <= 0)
        {
            LastNoChargeAttemptResolvedSkillCheck = observedSkillCheck != null && observedSkillCheck.IsRunning;
            LastNoChargeAttemptSucceeded = false;
            if (LastNoChargeAttemptResolvedSkillCheck)
            {
                int beforeResolve = UsesRemaining;
                observedSkillCheck.ResolveSkillCheck();
                LastNoChargeAttemptSucceeded = UsesRemaining > beforeResolve;
            }
            return false;
        }

        LastNoChargeAttemptResolvedSkillCheck = false;
        LastNoChargeAttemptSucceeded = false;
        UsesRemaining--;
        UpdateHeadAnimation();
        OnUsesChanged?.Invoke(UsesRemaining, MaxUses);
        if (rechargeTimer <= 0f)
            BeginChargeRecharge();
        if (UsesRemaining == 0 && observedSkillCheck != null)
        {
            skillCheckFailedThisCharge = false;
            skillCheckStartedThisCharge = false;
            observedSkillCheck.BeginSkillCheckAt(skillCheckTarget, rechargeDurationPerCharge, RechargeProgress);
            skillCheckStartedThisCharge = true;
        }
        return true;
    }

    private void BeginChargeRecharge()
    {
        rechargeTimer = Mathf.Max(0.1f, rechargeDurationPerCharge);
        skillCheckFailedThisCharge = false;
        skillCheckStartedThisCharge = false;
    }

    private void HandleSkillCheckResult(SkillCheckBar.SkillCheckResult result)
    {
        if (UsesRemaining != 0) return;
        if (!result.hitTarget)
        {
            skillCheckFailedThisCharge = true;
            if (observedSkillCheck != null) observedSkillCheck.CancelSkillCheck();
            return;
        }

        // A clean timing hit completes the current recharge early; it never grants an extra charge.
        rechargeTimer = 0f;
        FinishOneCharge();
    }

    private void FinishOneCharge()
    {
        if (UsesRemaining >= MaxUses) return;
        UsesRemaining++;
        rechargeTimer = 0f;
        skillCheckFailedThisCharge = false;
        skillCheckStartedThisCharge = false;
        if (observedSkillCheck != null) observedSkillCheck.CancelSkillCheck();
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
        skillCheckStartedThisCharge = false;
        if (observedSkillCheck != null) observedSkillCheck.CancelSkillCheck();
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

    public void ConfigureRechargeDuration(float duration)
    {
        rechargeDurationPerCharge = Mathf.Max(0.1f, duration);
    }

    private void BindSkillCheck()
    {
        if (observedSkillCheck != null)
            observedSkillCheck.ResultResolved -= HandleSkillCheckResult;
        observedSkillCheck = null;
        SkillCheckBar[] skillChecks = Resources.FindObjectsOfTypeAll<SkillCheckBar>();
        for (int i = 0; i < skillChecks.Length; i++)
        {
            if (skillChecks[i] == null || !skillChecks[i].gameObject.scene.IsValid()) continue;
            observedSkillCheck = skillChecks[i];
            break;
        }
        if (observedSkillCheck != null)
            observedSkillCheck.ResultResolved += HandleSkillCheckResult;
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
