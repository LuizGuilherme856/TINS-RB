using UnityEngine;
using UnityEngine.InputSystem;
using GraveyardShift.Gameplay;
using TINS.Campaign;

/// <summary>Starts the tower recharge skill check when the player presses Space inside its light.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SkillCheckBar))]
public sealed class LightRadiusSkillCheckActivator : MonoBehaviour
{
    [SerializeField] private SkillCheckBar skillCheck;
    [SerializeField] private PurificationReveal towerLight;
    [SerializeField] private Transform player;
    [SerializeField] private GeneralTextBoxUI interactionTextBox;
    [SerializeField] private TowerCrystalDeposit crystalDeposit;
    [SerializeField] private string rechargePrompt = "PRESS SPACE TO RECHARGE";
    [SerializeField, Min(0f)] private float resultDisplayDuration = 0.9f;
    private enum InteractionState { Outside, Ready, Running, ShowingResult }
    private InteractionState state;
    private bool waitForSpaceRelease = true;
    private float resultVisibleUntil;

    private void Awake()
    {
        ResolveReferences();
    }

    private void ResolveReferences()
    {
        if (skillCheck == null)
            skillCheck = GetComponent<SkillCheckBar>();
        if (towerLight == null)
            towerLight = FindFirstObjectByType<PurificationReveal>(FindObjectsInactive.Include);
        if (interactionTextBox == null)
            interactionTextBox = FindFirstObjectByType<GeneralTextBoxUI>(FindObjectsInactive.Include);
        if (crystalDeposit == null)
            crystalDeposit = FindFirstObjectByType<TowerCrystalDeposit>();
        if (player == null || !player.gameObject.activeInHierarchy)
            FindPlayer();
    }

    private void OnEnable()
    {
        if (skillCheck == null)
            skillCheck = GetComponent<SkillCheckBar>();
        if (skillCheck != null)
        {
            skillCheck.Cancelled += OnSkillCheckCancelled;
        }
        state = InteractionState.Outside;
        waitForSpaceRelease = true;
    }

    private void OnDisable()
    {
        if (skillCheck != null)
        {
            skillCheck.Cancelled -= OnSkillCheckCancelled;
            skillCheck.HideUntilStarted();
        }
        HideRechargePrompt();
    }

    private void Start()
    {
        if (skillCheck != null)
            skillCheck.HideUntilStarted();
    }

    private void Update()
    {
        if (CampaignCombatRules.IsCampaignMission)
            return;
        // The HUD can enable before the tower/player. Retry missing references
        // rather than remaining permanently inert after an early Awake lookup.
        ResolveReferences();
        if (skillCheck == null || towerLight == null)
            return;

        bool playerInsideLight = towerLight.isActiveAndEnabled &&
            player != null && player.gameObject.activeInHierarchy && IsPlayerInsideLight();
        Keyboard keyboard = Keyboard.current;
        if (!playerInsideLight)
        {
            if (state != InteractionState.Outside)
            {
                state = InteractionState.Outside;
                skillCheck.HideUntilStarted();
                HideRechargePrompt();
                waitForSpaceRelease = true;
            }
            return;
        }

        if (state == InteractionState.Outside)
        {
            skillCheck.HideUntilStarted();
            state = InteractionState.Ready;
            waitForSpaceRelease = keyboard != null && keyboard.spaceKey.isPressed;
        }

        if (state == InteractionState.ShowingResult)
        {
            if (Time.unscaledTime >= resultVisibleUntil && !IsDepositRunning())
            {
                skillCheck.HideUntilStarted();
                state = InteractionState.Ready;
                ShowRechargePrompt();
            }
            return;
        }

        if (state == InteractionState.Ready)
        {
            if (IsDepositRunning())
            {
                HideRechargePrompt();
                return;
            }
            ShowRechargePrompt();
        }

        if (keyboard == null)
            return;
        if (waitForSpaceRelease)
        {
            if (!keyboard.spaceKey.isPressed)
                waitForSpaceRelease = false;
            return;
        }

        if (!keyboard.spaceKey.wasPressedThisFrame)
            return;

        // Consume this press once, even when resolving invokes callbacks immediately.
        waitForSpaceRelease = true;
        if (state == InteractionState.Ready)
        {
            skillCheck.BeginSkillCheck();
            if (skillCheck.IsRunning)
                EnterRunningState();
        }
        else if (state == InteractionState.Running)
        {
            skillCheck.ResolveSkillCheck();
            if (!skillCheck.IsRunning)
                EnterResultState();
        }
    }

    private void EnterRunningState()
    {
        state = InteractionState.Running;
        waitForSpaceRelease = true;
        HideRechargePrompt();
    }

    private void OnSkillCheckCancelled()
    {
        state = InteractionState.Outside;
        waitForSpaceRelease = true;
        HideRechargePrompt();
    }

    private void EnterResultState()
    {
        state = InteractionState.ShowingResult;
        waitForSpaceRelease = true;
        resultVisibleUntil = Time.unscaledTime + resultDisplayDuration;
        HideRechargePrompt();
    }

    private bool IsDepositRunning()
    {
        if (crystalDeposit == null)
            crystalDeposit = FindFirstObjectByType<TowerCrystalDeposit>();
        return crystalDeposit != null && crystalDeposit.IsDepositing;
    }

    private void ShowRechargePrompt()
    {
        if (interactionTextBox == null)
            interactionTextBox = FindFirstObjectByType<GeneralTextBoxUI>();
        if (interactionTextBox != null)
            interactionTextBox.SetText(rechargePrompt);
    }

    private void HideRechargePrompt()
    {
        if (interactionTextBox == null)
            interactionTextBox = FindFirstObjectByType<GeneralTextBoxUI>();
        if (interactionTextBox != null)
            interactionTextBox.RemoveText();
    }

    private bool IsPlayerInsideLight()
    {
        Vector2 offset = (Vector2)player.position - (Vector2)towerLight.transform.position;
        float radius = Mathf.Max(0.01f, towerLight.Radius);
        return offset.sqrMagnitude <= radius * radius;
    }

    private void FindPlayer()
    {
        PlayerInputHandler[] players = FindObjectsByType<PlayerInputHandler>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].isActiveAndEnabled && !players[i].GetIsClone())
            {
                player = players[i].transform;
                return;
            }
        }
    }
}
