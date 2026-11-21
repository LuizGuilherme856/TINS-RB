using UnityEngine;
using TINS.Campaign;
//using MoreMountains.Feedbacks;
using System;
using Random = UnityEngine.Random;


public class MeleeManager : MonoBehaviour
{
    [SerializeField] private MeleeData meleeData;
    [SerializeField] private PlayerInputHandler playerInput;
    

    //[SerializeField] private MMMiniObjectPooler objectPool;

    [SerializeField] private MeleeAttackHandler meleeAttackHandler;
    [SerializeField] private PlayerAction playerAction;

    [SerializeField] private PlayerData playerData;
    
    //TODO: Add a GlobalStats reference populated by UpgradeManager when the system is ready.
    public event Action OnChangeMeleeWeapon;

    [field:SerializeField] public float currentAttackRateTimer { get; private set; }

    private HandleSpecialAttack handleSpecialAttack;
    private SlashCombo combo;

    //Dash-related.
    //public event Action OnChangeAction;

    //Upgrade Related
    /// <summary>
    /// Shared player stats.
    /// </summary>
    [SerializeField] private GlobalStats globalStats;
    private UpgradeManager upgradeManager;
    [SerializeField] private StatsCalculator statsCalculator;

[Tooltip("ConfiguraÃ§Ã£o usada no hub pra impedir o player de usar as habilidades no hub. Gambiarra pq buga as skills quando troca de personagem")]
    [SerializeField] private bool disableAttacks = false;
     private EnemyWaveManager enemyWaveManager;

    private void Awake()
    {
        if (playerAction == PlayerAction.melee) combo = new SlashCombo(meleeAttackHandler, playerInput);
        if(playerAction == PlayerAction.special || playerAction == PlayerAction.dash) //Apply additional settings for special attacks and dashes.
        {
            handleSpecialAttack = GetComponent<HandleSpecialAttack>();
        }
        //Try to resolve UpgradeManager.
        try
        {
            upgradeManager = FindFirstObjectByType<UpgradeManager>();
            upgradeManager.OnChangeStats += UpdateTimerOnUpgrade;
        }
        catch (Exception)
        {
            Debug.Log("Nï¿½o encontrou o upgradeManager nessa scene");
        }

        enemyWaveManager = AssetFinder.GetEnemyWaveManager();

    }

    private void OnEnable()
    {
        //Initialize empty.
        currentAttackRateTimer = statsCalculator.CalculateMaxTimeCoolDown(meleeData.attackRate, playerAction);

        if (playerAction == PlayerAction.special)
        {
            //Skill that resets special cooldown on an enemy kill.
            enemyWaveManager.OnEnemyDied += TryToResetCooldown;
        }

        /*
         * Initialize action-specific values so the remaining logic is shared.
         */
        switch (playerAction)
        {
            case PlayerAction.melee:
                playerInput.OnMeleeButtonDown += Attack;
                playerInput.DashPerformed += CancelCombo;
                break;
            case PlayerAction.special:
                playerInput.OnSpecialAttackButtonDown += Attack;
                break;
            case PlayerAction.dash:
                meleeAttackHandler.UpdateMeleeData(meleeData);
                playerInput.OnDash += Attack;
                break;
            default:
                Debug.LogError("Uma aï¿½ï¿½o nï¿½o registrada foi pedida");
                break;
        }
    }

    private void OnDisable()
    {
        playerInput.OnMeleeButtonDown -= Attack;
        playerInput.OnSpecialAttackButtonDown -= Attack;
        playerInput.OnDash -= Attack;
        playerInput.DashPerformed -= CancelCombo;
        CancelCombo();
        if (playerAction == PlayerAction.special)
        {
            //Skill that resets special cooldown on an enemy kill.
            enemyWaveManager.OnEnemyDied -= TryToResetCooldown;
        }
    }

    private void Start()
    {
        // Initialize cooldown in Start to respect component initialization order.
        currentAttackRateTimer =statsCalculator.CalculateMaxTimeCoolDown(meleeData.attackRate, playerAction);

    }

    public int MaxDashActions => statsCalculator.MaxDashActions;

    public void RefillCampaignDash()
    {
        if (playerAction == PlayerAction.dash && meleeData != null)
            UpdateTimerOnUpgrade();
    }

    /// <summary>
    /// Recalculate timers when MaxAction or cooldown changes
    /// so stored charge time stays within the new limits. For example:
    /// Timer = 10 (2 segundos por dash * 5 dashs)
    /// Cooldown changes to one second.
    /// Five one-second charges allow five stored seconds, not the previous ten.
    /// </summary>
    public void UpdateTimerOnUpgrade()
    {
        currentAttackRateTimer = statsCalculator.CalculateMaxTimeCoolDown(meleeData.attackRate, playerAction);
    }

    private void Update()
    {
        if (combo != null && combo.Active)
        {
            if (disableAttacks || !playerInput.CanPerformMelee) CancelCombo();
            else combo.Tick();
        }
        /*
          * Accumulate time up to the action interval times maximum charges and the relevant multiplier.
          */
        if (currentAttackRateTimer < statsCalculator.CalculateMaxTimeCoolDown(meleeData.attackRate, playerAction))
        {
            currentAttackRateTimer += Time.deltaTime;
        }
    }
    private void Attack()
    {
        if (meleeData == null || disableAttacks)
        {
            meleeAttackHandler.PlayCantAttackFeedback();
            return;
        }

        if (combo != null && combo.Active)
        {
            combo.Press();
            return;
        }

        //Is enough charge available for an action within MaxAction?
        if (statsCalculator.CalculatePerActionCoolDown(meleeData.attackRate, playerAction) <= currentAttackRateTimer)
        {
            if (playerAction == PlayerAction.special || playerAction == PlayerAction.dash) //Apply additional settings for special attacks and dashes.
            {
                // If the special handler rejects the ability,
                // return before consuming a charge or cooldown.
                if (handleSpecialAttack != null && handleSpecialAttack.TriggerSpecialAttack(meleeData) == false)
                {
                    meleeAttackHandler.PlayCantAttackFeedback();
                    return;
                }
            }

            Vector3 dashOrigin = playerInput.transform.position;
            if (meleeData.instantiateGameObjectOnAttack)
            {
                if (combo != null && meleeData.useSlashCombo) combo.Start(meleeData);
                else if (playerAction == PlayerAction.dash) // For dashes, calculate direction from player input.
                {
                    meleeAttackHandler.InstantiateMeleeAttack(playerInput.GetMovementAngle());
                }
                else
                {
                    meleeAttackHandler.InstantiateMeleeAttack(playerInput.GetAimAngle());
                }

            }
            currentAttackRateTimer -= statsCalculator.CalculatePerActionCoolDown(meleeData.attackRate, playerAction);
            if (playerAction == PlayerAction.dash)
            {
                playerInput.NotifyDashPerformed();
                CampaignMissionRuntime.Active?.DeployDashDecoy(dashOrigin);
            }
        }
        else
        {
            meleeAttackHandler.PlayCantAttackFeedback();
        }
        
    }

    public void SetMeleeWeapon(MeleeData newMelee)
    {
        CancelCombo();
        meleeData = newMelee;
        meleeAttackHandler.UpdateMeleeData(meleeData);
        OnChangeMeleeWeapon?.Invoke();
    }

    public void TryToResetCooldown()
    {
        if(Random.Range(0f, 1f) < globalStats.resetSpecialAbilityOnEnemyDeathChance)
        {
            currentAttackRateTimer = statsCalculator.CalculateMaxTimeCoolDown(meleeData.attackRate, playerAction);
        }
    }

    public MeleeData GetMeleeData()
    {
        return meleeData;
    }

    public void SetAttacksEnabled(bool enabled)
    {
        disableAttacks = !enabled;
        if (!enabled) CancelCombo();
    }

    private void CancelCombo() => combo?.Cancel();

    private void OnDestroy()
    {
        if (upgradeManager != null) upgradeManager.OnChangeStats -= UpdateTimerOnUpgrade;
    }

    public void RefillActions()
    {
        if (meleeData == null)
        {
            return;
        }

        currentAttackRateTimer = statsCalculator.CalculateMaxTimeCoolDown(meleeData.attackRate, playerAction);
    }
    

    public float GetTotalActionCooldown_Percentage(int maxActionCap)
    {
        return statsCalculator.GetTotalActionCooldown_Percentage(meleeData.attackRate,currentAttackRateTimer, playerAction, maxActionCap);
    }

    /// <summary>
    /// Return the currently available action count.
    /// </summary>
    public int GetCurrentActionQuantity()
    {
        return (int)Math.Floor(currentAttackRateTimer / (statsCalculator.CalculatePerActionCoolDown(meleeData.attackRate, playerAction)));
    }

    /// <summary>
    /// Return fractional progress toward the next charge: 2.7 charges
    /// means two ready charges and 70% progress, so return 0.7.
    /// </summary>
    public float CalculateNextActionPercentageCompleted()
    {
        return currentAttackRateTimer / (statsCalculator.CalculatePerActionCoolDown(meleeData.attackRate, playerAction)) - GetCurrentActionQuantity();
    }

    public PlayerAction GetPlayerAction()
    {
        return playerAction;
    }

}
//An action is a dash, melee attack, or special attack.
public enum PlayerAction { melee, special, dash}
