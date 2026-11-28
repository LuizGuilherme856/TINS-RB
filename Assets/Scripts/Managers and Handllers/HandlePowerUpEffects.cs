using UnityEngine;
using static PowerUp;
using System;

//Communicates with components on the player GameObject.
public class HandlePowerUpEffects : MonoBehaviour
{
    [SerializeField] private GunManager gunManager;
    [SerializeField] private GunInventory gunInventory;
    [SerializeField] private Health health;
    [SerializeField] private PointManager pointManager;
    [SerializeField] private Shield_PowerUp shield_PowerUp;
    [SerializeField] private MeleeAttackHandler meleeAttackHandler;
    [SerializeField] private ShootingHandler shootingHandler;

    [Space(15)]

    [SerializeField] private PowerUpData powerUpData;
    [SerializeField] private GlobalStats globalStats;
    [SerializeField] private RunData runData;

    [Space(50)]
    [Header("    ------- SURVIVAL MODE -----    ")]
    [SerializeField] private SurvivalModeManager survivalManager;
    [SerializeField] private XpManager_SurvivalMode xpManager;



    private PlayerData playerData;
    private PowerUpDurationHandler powerUpDurationHandler;



    private void Awake()
    {
        powerUpDurationHandler = FindFirstObjectByType<PowerUpDurationHandler>();

        playerData = GetComponent<PlayerDataHolder>().GetPlayerData();
        if (pointManager == null) pointManager = FindFirstObjectByType<PointManager>();

        if (globalStats == null)
        {
            globalStats = AssetFinder.GetGlobalStats();
            Debug.LogError($"O obj {gameObject.name} nao tem global stats. Procurando manualmente. Por favor" +
                $"coloque pelo inspector pra poupar performance");
        }
    }

    /// <summary>
    /// Mirror this information into RunData.
    /// </summary>
    public void AddPowerUpPickUpCount()
    {
        runData.numberOfPowerUpsCollected++;
    }

    public void RefillAmmo()
    {

        var guns = gunInventory.GetGuns();

        if (guns.Count <= 0) return;

        foreach (var gun in guns)
        {
            gunInventory.RefillReserve(gun);
        }

        //gunManager.GetEquipedGun()?.SetMaxSpareAmmo();
    }

    public void ExtraLife()
    {
        health.ReplenishHealth(globalStats.ForCurrentRun.AddToHpHealedByHearts);
    }

    public void ExtraPoints()
    {
        pointManager.AddPoint(powerUpData.pointPerDrop);
    }

    public void AddHorn()
    {
        playerData.AddHorns(1);
    }

    public void Shield()
    {
        shield_PowerUp.ActivateShield(powerUpData.shieldDuration);
    }

    public void InstaKillMelee()
    {
        powerUpDurationHandler.SetIconDuration(powerUpData.intaKillMeleeDuration, PowerUpType.instaKillMelee);
        meleeAttackHandler.SetDamageMultiplierDuration(powerUpData.intaKillMeleeDamageMultiplier, powerUpData.intaKillMeleeDuration);
    }

    public void InstaKill()
    {
        powerUpDurationHandler.SetIconDuration(powerUpData.intaKillDuration, PowerUpType.instaKill);
        shootingHandler.SetDamageMultiplierDuration(powerUpData.intaKillDamageMultiplier, powerUpData.intaKillDuration);
    }

    public void DoublePoints()
    {
        powerUpDurationHandler.SetIconDuration(25, PowerUpType.doublePoints);
        pointManager.SetPointMultiplier(2, powerUpData.doublePointsDuration);

    }

    public void SurvivalXpDrop()
    {
        xpManager.AddXpPoint();
    }
}
