using System.Collections.Generic;
using UnityEngine;

// Shared modifier state used by StatsCalculator and upgrades. New-run reset and reapplication
// order matter: permanent skills and temporary runes must not accumulate across unrelated runs.
public class GlobalStats : ScriptableObject
{
    private static readonly Dictionary<GlobalStats, GlobalStats> LegacyStats = new();
    private static readonly Dictionary<GlobalStats, GlobalStats> CampaignStats = new();
    [System.NonSerialized] private bool isSessionCopy;

    public GlobalStats ForCurrentRun
    {
        get
        {
            if (!Application.isPlaying || isSessionCopy) return this;
            var session = TINS.Campaign.CampaignCombatRules.IsCampaignMission ? CampaignStats : LegacyStats;
            if (session.TryGetValue(this, out GlobalStats copy) && copy != null) return copy;

            copy = Instantiate(this);
            copy.isSessionCopy = true;
            copy.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            copy.orbitalUpgrades ??= new List<OrbitalUpgrade_ActiveEffect>();
            session[this] = copy;
            return copy;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearSessionStats()
    {
        foreach (GlobalStats stats in LegacyStats.Values)
            if (stats != null) Destroy(stats);
        foreach (GlobalStats stats in CampaignStats.Values)
            if (stats != null) Destroy(stats);
        LegacyStats.Clear();
        CampaignStats.Clear();
    }
    private readonly float MIN_FLOAT = 0.001f;

    public RuneData[] runeDatas;
    public GunData[] gunDatas;

    #region Prices

    [field: SerializeField] public float doorPriceMultiplier { get; private set; } = 1;
    [field: SerializeField] public float meleeWeaponPriceMultiplier { get; private set; } = 1;
    [field: SerializeField] public float gunUpgradePriceMultiplier { get; private set; } = 1;
    [field: SerializeField] public float runePriceMultiplier { get; private set; } = 1;
    [field: SerializeField] public float gunPriceMultiplier { get; private set; } = 1;
    [field: SerializeField] public float globalPriceMultiplier { get; private set; } = 1;

    public void ChangeGlobalPriceMultiplier(float value, OperationType operation)
    {
        globalPriceMultiplier = ApplyOperation(globalPriceMultiplier, value, operation);
        globalPriceMultiplier = Aux.MinValueToReturn(globalPriceMultiplier, 0);
    }

    public void ChangeGunPriceMultiplier(float value, OperationType operation)
    {
        gunPriceMultiplier = ApplyOperation(gunPriceMultiplier, value, operation);
        gunPriceMultiplier = Aux.MinValueToReturn(gunPriceMultiplier, 0);
    }

    public void ChangeDoorPriceMultiplier(float value, OperationType operation)
    {
        doorPriceMultiplier = ApplyOperation(doorPriceMultiplier, value, operation);
        doorPriceMultiplier = Aux.MinValueToReturn(doorPriceMultiplier, 0);
    }

    public void ChangeMeleeWeaponPriceMultiplier(float value, OperationType operation)
    {
        meleeWeaponPriceMultiplier = ApplyOperation(meleeWeaponPriceMultiplier, value, operation);
        meleeWeaponPriceMultiplier = Aux.MinValueToReturn(meleeWeaponPriceMultiplier, 0);
    }

    public void ChangeGunUpgradePriceMultiplier(float value, OperationType operation)
    {
        gunUpgradePriceMultiplier = ApplyOperation(gunUpgradePriceMultiplier, value, operation);
        gunUpgradePriceMultiplier = Aux.MinValueToReturn(gunUpgradePriceMultiplier, 0);
    }

    public void ChangeRunePriceMultiplier(float value, OperationType operation)
    {
        runePriceMultiplier = ApplyOperation(runePriceMultiplier, value, operation);
        runePriceMultiplier = Aux.MinValueToReturn(runePriceMultiplier, 0);
    }

    #endregion

    #region Run Start

    public int numberOfRandomRunesToStartWith { get; private set; } = 0;
    public int numberOfRandomGunsToStartWith { get; private set; } = 0;

    public void ChangeNumberOsRandomGunsToStartWith(int value, OperationType operation)
    {
        numberOfRandomGunsToStartWith = ApplyOperation(numberOfRandomGunsToStartWith, value, operation);
        if (numberOfRandomGunsToStartWith < 0) numberOfRandomGunsToStartWith = 0;
    }

    public GunData GetRandomGun()
    {
        return gunDatas[Random.Range(0, gunDatas.Length)];
    }

    public void ChangeNumberOsRandomRunesToStartWith(int value, OperationType operation)
    {
        numberOfRandomRunesToStartWith = ApplyOperation(numberOfRandomRunesToStartWith, value, operation);
        if (numberOfRandomRunesToStartWith < 0) numberOfRandomRunesToStartWith = 0;
    }

    public RuneData GetRandomRune()
    {
        return runeDatas[Random.Range(0, runeDatas.Length)];
    }

    #endregion

    #region Misc

    [field: SerializeField] public bool RefilAmmoOnOpenDoor { get; private set; } = false;
    [field: SerializeField] public int FreeRevives { get; private set; } = 0;
    [field: SerializeField] public DamageType walkTrailDamageType { get; private set; } = DamageType.None;

    public void ChangeWalkTrailDamageType(DamageType type, OperationType operation)
    {
        if (operation == OperationType.Ignore) return;
        walkTrailDamageType = type;
    }

    public void ChangeRefilAmmoOnOpenDoor(bool value, OperationType operation)
    {
        if(operation == OperationType.Ignore) return;
        RefilAmmoOnOpenDoor = value;
    }

    /// <summary>
    /// Skill that revives once without losing weapons.
    /// </summary>
    public void ChangeFreeRevives(int value, OperationType operation)
    {
        FreeRevives = ApplyOperation(FreeRevives, value, operation);
    }

    #endregion

    #region Points
    [field: SerializeField] public int extraPointsPerKill { get; private set; } = 0;
    [field: SerializeField] public int extraInitialPoints { get; private set; } = 0;

    public void ChangeExtraPointsPerKill(int value, OperationType operation)
    {
        extraPointsPerKill = ApplyOperation(extraPointsPerKill, value, operation);
        //if (extraPointsPerKill < -5) extraPointsPerKill = 0;
    }

    public void ChangeExtraInitialPoints(int value, OperationType operation)
    {
        extraInitialPoints = ApplyOperation(extraInitialPoints, value, operation);
    }
    #endregion

    #region Status
    /// <summary>
    /// Maximum number of other enemies reached by one chain-reaction link.
    /// </summary>
    [field: SerializeField] public int maxChainReactions = 5;
    [field: SerializeField] public float statusChanceMultiplier { get; private set; } = 1;
    [field: SerializeField] public float statusDamageMultiplier { get; private set; } = 1;
    [field: SerializeField] public float statusDurationMultiplier { get; private set; } = 1;

    public void ChangeStatusChanceMultiplier(float value, OperationType operation)
    {
        statusChanceMultiplier = Aux.MinValueToReturn(ApplyOperation(statusChanceMultiplier, value, operation), 0);
    }

    public void ChangeStatusDamageMultiplier(float value, OperationType operation)
    {
        statusDamageMultiplier = Aux.MinValueToReturn(ApplyOperation(statusDamageMultiplier, value, operation), 0);
    }

    public void ChangeStatusDurationMultiplier(float value, OperationType operation)
    {
        statusDurationMultiplier = Aux.MinValueToReturn(ApplyOperation(statusDurationMultiplier, value, operation), 0);
    }
    #endregion

    #region Drops

    [field: SerializeField] public float AddTodropChance { get; set; } = 0;
    [field: SerializeField] public int AddToHpHealedByHearts { get; set; } = 1;
    [field: SerializeField] public float powerUpGoTowardsPlayerMaxDistanceMultiplier { get; set; } = 1;

    public void ChangePowerUpGoTowardsPlayerMaxDistanceMultiplier(float value, OperationType operation)
    {
        powerUpGoTowardsPlayerMaxDistanceMultiplier = ApplyOperation(powerUpGoTowardsPlayerMaxDistanceMultiplier, value, operation);
        powerUpGoTowardsPlayerMaxDistanceMultiplier = Aux.MinValueToReturn(powerUpGoTowardsPlayerMaxDistanceMultiplier, 0);

    }

    public void ChangeAddToDropChance(float value, OperationType operation)
    {
        AddTodropChance = ApplyOperation(AddTodropChance, value, operation);
    }

    public void ChangeAddToHpHealedByHearts(int value, OperationType operation)
    {
        AddToHpHealedByHearts = ApplyOperation(AddToHpHealedByHearts, value, operation);
    }


    #endregion

    #region Player

    [field: SerializeField] public float increaseWeaponCapacityBy { get; set; } = 0;

    [field: SerializeField] public float playerWalkSpeedMultiplier { get; set; } = 1;

    /// <summary>
    /// Maximum damage-ignore probability; 0.5 caps it at 50%.
    /// </summary>
    [field: SerializeField] public float maxIgnoreDamageChance { get; set; } = 0.5f;

    [field: SerializeField] public int AddToPlayerInitialHealth = 0;
    [field: SerializeField] public int IncreasePlayerMaxHealthBy { get; set; } = 0;

    [field: SerializeField] public float IncreaseChanceOfIgnoreDamageBy { get; private set; } = 0;
    [field: SerializeField] public float ChanceOfIgnoreDamage_Multiplier { get; private set; } = 1;

    [field: SerializeField] public float ShieldDurationOnTakingDamage { get; private set; } = 0;

    [field: SerializeField] public int healthPointsToHealOnBuyWeapon = 0;

    public void ChangeAddToPlayerInitialHealth(int value, OperationType operation)
    {
        AddToPlayerInitialHealth = ApplyOperation(AddToPlayerInitialHealth, value, operation);
    }

    public void ChangeHealthPointsToHealOnBuyWeapon(int value, OperationType operation)
    {
        healthPointsToHealOnBuyWeapon = ApplyOperation(healthPointsToHealOnBuyWeapon, value, operation);
    }

    public void ChangeIncreaseWeaponCapacityBy(float value, OperationType operation)
    {
        increaseWeaponCapacityBy = ApplyOperation(increaseWeaponCapacityBy, value, operation);
    }

    public void ChangePlayerWalkSpeedMultiplier(float value, OperationType operation)
    {
        playerWalkSpeedMultiplier = ApplyOperation(playerWalkSpeedMultiplier, value, operation);
        playerWalkSpeedMultiplier = Aux.MinValueToReturn(playerWalkSpeedMultiplier, 0.2f); // Prevent negative or excessively low movement speed.
    }

    /// <summary>
    /// Some skills grant a temporary shield after taking damage.
    /// </summary>
    public void ChangeShieldDurationOnTakingDamage(float value, OperationType operation)
    {
        ShieldDurationOnTakingDamage = ApplyOperation(ShieldDurationOnTakingDamage, value, operation);

        ShieldDurationOnTakingDamage = Aux.ReturnZeroIfNegative(ShieldDurationOnTakingDamage);
    }

    /// <summary>
    /// GenericUpgradeHandler checks every 30 seconds for an extra heal;
    /// this value controls its probability.
    /// </summary>
    public float ChanceOfGain1HpEvery30s { get; private set; } = 0;

    public void ChangeChanceOfGain1HpEvery30s(float value, OperationType operation)
    {
        ChanceOfGain1HpEvery30s = ApplyOperation(ChanceOfGain1HpEvery30s, value, operation);
        ChanceOfGain1HpEvery30s = Aux.ReturnZeroIfNegative(ChanceOfGain1HpEvery30s);
    }
    public void ChangeChanceOfIgnoreDamageMultiplier(float value, OperationType operation)
    {
        ChanceOfIgnoreDamage_Multiplier = ApplyOperation(ChanceOfIgnoreDamage_Multiplier, value, operation);
        ChanceOfIgnoreDamage_Multiplier = Aux.MinValueToReturn(ChanceOfIgnoreDamage_Multiplier, 0);
    }
    public void ChangeIncreaseChanceOfIgnoreDamageBy(float value, OperationType operation)
    {
        IncreaseChanceOfIgnoreDamageBy = ApplyOperation(IncreaseChanceOfIgnoreDamageBy, value, operation);
    }

    #endregion

    #region Enemies

    [field: SerializeField] public float enemyHealthMultiplier { get; private set; } = 1;

    public void ChangeEnemyHealthMultiplier(float value, OperationType operation)
    {
        enemyHealthMultiplier = ApplyOperation(enemyHealthMultiplier, value, operation);
        enemyHealthMultiplier = Aux.MinValueToReturn(enemyHealthMultiplier, 0.1f); // Prevent negative or excessively low enemy health.
    }

    #endregion

    #region OrbitalUpgrades

    /// <summary>
    /// Store acquired orbitals so they can be
    /// recreated after scene changes.
    /// </summary>
    public List<OrbitalUpgrade_ActiveEffect> orbitalUpgrades;

    public float IncreaseOrbitalSpeedBy { get; private set; } = 0;
    public float OrbitalSpeed_Multiplier { get; private set; } = 1;

    public void ChangeIncreaseOrbitalSpeedBy(float value, OperationType operation)
    {
        IncreaseOrbitalSpeedBy = ApplyOperation(IncreaseOrbitalSpeedBy, value, operation);
    }

    public void ChangeOrbitalSpeed_Multiplier(float value, OperationType operation)
    {
        OrbitalSpeed_Multiplier = ApplyOperation(OrbitalSpeed_Multiplier, value, operation);
        OrbitalSpeed_Multiplier = Aux.MinValueToReturn(OrbitalSpeed_Multiplier, 0.1f); // Prevent negative or excessively low orbital speed.
    }


    [field: SerializeField] public float IncreaseOrbitalCorrectionSpeedBy { get; private set; } = 0;
    [field: SerializeField] public float OrbitalCorrectionSpeed_Multiplier { get; private set; } = 1;

    public void ChangeIncreaseOrbitalCorrectionSpeedBy(float value, OperationType operation)
    {
        IncreaseOrbitalCorrectionSpeedBy = ApplyOperation(IncreaseOrbitalCorrectionSpeedBy, value, operation);
    }

    public void ChangeOrbitalCorrectionSpeed_Multiplier(float value, OperationType operation)
    {
        OrbitalCorrectionSpeed_Multiplier = ApplyOperation(OrbitalCorrectionSpeed_Multiplier, value, operation);
    }

    [field: SerializeField] public float OrbitalBladeInstaKillChance { get; private set; } = 0f;
    [field: SerializeField] public float OrbitalBladeInstaKillChance_Multiplier { get; private set; } = 1.2f;

    public void ChangeOrbitalBladeInstaKillChance(float value, OperationType operation)
    {
        OrbitalBladeInstaKillChance = Aux.ReturnZeroIfNegative(ApplyOperation(OrbitalBladeInstaKillChance, value, operation));
    }


    [field: SerializeField] public float orbitalBladeMinRadius = 1.4f;
    [field: SerializeField] public float OrbitalRadius_Multiplier { get; private set; } = 1;

    public void ChangeOrbitalRadiusMultiplier(float value, OperationType operation)
    {
        OrbitalRadius_Multiplier = Aux.ReturnZeroIfNegative(ApplyOperation(OrbitalRadius_Multiplier, value, operation));
    }



    [field: SerializeField] public float OrbitalBladeDamage_Multiplier { get; private set; } = 1;

    [field: SerializeField] public float IncreaseOrbitalBladeDamageBy { get; private set; } = 0;

    public void ChangeIncreaseOrbitalBladeDamageBy(int value, OperationType operation)
    {
        IncreaseOrbitalBladeDamageBy = ApplyOperation(IncreaseOrbitalBladeDamageBy, value, operation);
    }

    public void ChangeOrbitalBladeDamage_Multiplier(float value, OperationType operation)
    {
        OrbitalBladeDamage_Multiplier = Aux.ReturnZeroIfNegative(ApplyOperation(OrbitalBladeDamage_Multiplier, value, operation));
    }






    #endregion

    #region Melee

    [SerializeField] public int maxMeleeActionQuantity = 1;
    [SerializeField] public float MeleeAttackRateMultiplier = 1;

    public void ChangeMaxMeleeActionQuantity(int value, OperationType operation)
    {
        maxMeleeActionQuantity = ApplyOperation(maxMeleeActionQuantity, value, operation);
    }

    public void ChangeMaxSpecialActionQuantity(int value, OperationType operation)
    {
        maxSpecialActionQuantity = ApplyOperation(maxSpecialActionQuantity, value, operation);
    }

    public void ChangeMeleeAttackRateMultiplier(float value, OperationType operation)
    {
        MeleeAttackRateMultiplier = ApplyOperation(MeleeAttackRateMultiplier, value, operation);
        MeleeAttackRateMultiplier = Aux.MinValueToReturn(MeleeAttackRateMultiplier, MIN_FLOAT); // Prevent negative or excessively low attack intervals.
    }



    #endregion

    #region special ability

    [SerializeField] public float SpecialAbilityCooldownMultiplier = 1;
    [SerializeField] public int maxSpecialActionQuantity = 1;

    public float resetSpecialAbilityOnEnemyDeathChance { get; private set; } = 0;

    public void ChangeResetSpecialAbilityOnEnemyDeathChance(float value, OperationType operation)
    {
        resetSpecialAbilityOnEnemyDeathChance = ApplyOperation(resetSpecialAbilityOnEnemyDeathChance, value, operation);
        resetSpecialAbilityOnEnemyDeathChance = Aux.ReturnZeroIfNegative(resetSpecialAbilityOnEnemyDeathChance);
    }

    public void ChangeSpecialAbilityCooldownMultiplier(float value, OperationType operation)
    {
        SpecialAbilityCooldownMultiplier = ApplyOperation(SpecialAbilityCooldownMultiplier, value, operation);
        SpecialAbilityCooldownMultiplier = Aux.MinValueToReturn(SpecialAbilityCooldownMultiplier, 0); // Prevent negative or excessively low cooldowns.
    }
    #endregion

    #region Dash

    [field: SerializeField] public int maxDashActionQuantity { get; private set; } = 1;
     public float DashAbilityCooldownMultiplier = 1;
    [field: SerializeField] public float dashDamageMultiplier { get; private set; } = 1;
    [field: SerializeField] public DamageType dashDamageType { get; private set; } = DamageType.Impact;

    public void ChangeDashDamageMultiplier(float value, OperationType operation)
    {
        dashDamageMultiplier = ApplyOperation(dashDamageMultiplier, value, operation);
        dashDamageMultiplier = Aux.MinValueToReturn(dashDamageMultiplier, 0); // Prevent negative dash damage.
    }

    public void ChangeDashCooldownMultiplier(float value, OperationType operation)
    {
        //if (!ValueIsValid(value)) return;
        DashAbilityCooldownMultiplier = ApplyOperation(DashAbilityCooldownMultiplier, value, operation);
        if (DashAbilityCooldownMultiplier < 0) DashAbilityCooldownMultiplier = 0.05f;
    }

    public void ChangeMaxDashActionQuantity(int value, OperationType operation)
    {
        maxDashActionQuantity = ApplyOperation(maxDashActionQuantity, value, operation);

    }


    public void ChangeDashDamageType(DamageType type)
    {
        dashDamageType = type;
    }

    #endregion

    #region Elemental Damage

    [field: SerializeField] public float fireDamageMultiplier { get; private set; } = 1;
    [field: SerializeField] public float iceDamageMultiplier { get; private set; } = 1;
    [field: SerializeField] public float poisonDamageMultiplier { get; private set; } = 1;
    [field: SerializeField] public float waterDamageMultiplier { get; private set; } = 1;
    [field: SerializeField] public float electricityDamageMultiplier { get; private set; } = 1;
    [field: SerializeField] public float etherDamageMultiplier { get; private set; } = 1;
    [field: SerializeField] public float impactDamageMultiplier { get; private set; } = 1;
    [field: SerializeField] public float punctureDamageMultiplier { get; private set; } = 1;
    [field: SerializeField] public float slashDamageMultiplier { get; private set; } = 1;

    public float GetElementalDamageMultiplier(DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Fire => fireDamageMultiplier,
            DamageType.Ice => iceDamageMultiplier,
            DamageType.Poison => poisonDamageMultiplier,
            DamageType.Water => waterDamageMultiplier,
            DamageType.Electricity => electricityDamageMultiplier,
            DamageType.Ether => etherDamageMultiplier,
            DamageType.Impact => impactDamageMultiplier,
            DamageType.Puncture => punctureDamageMultiplier,
            DamageType.Slash => slashDamageMultiplier,
            _ => 1f, // Default for unrecognized DamageType values.
        };
    }

    public void ChangeFireDamageMultiplier(float value, OperationType operation)
    {
        fireDamageMultiplier = ApplyOperation(fireDamageMultiplier, value, operation);
        fireDamageMultiplier = Aux.MinValueToReturn(fireDamageMultiplier, 0); // Prevent negative fire damage.
    }

    public void ChangeIceDamageMultiplier(float value, OperationType operation)
    {
        iceDamageMultiplier = ApplyOperation(iceDamageMultiplier, value, operation);
        iceDamageMultiplier = Aux.MinValueToReturn(iceDamageMultiplier, 0); // Prevent negative ice damage.
    }

    public void ChangePoisonDamageMultiplier(float value, OperationType operation)
    {
        poisonDamageMultiplier = ApplyOperation(poisonDamageMultiplier, value, operation);
        poisonDamageMultiplier = Aux.MinValueToReturn(poisonDamageMultiplier, 0); // Prevent negative poison damage.
    }

    public void ChangeWaterDamageMultiplier(float value, OperationType operation)
    {
        waterDamageMultiplier = ApplyOperation(waterDamageMultiplier, value, operation);
        waterDamageMultiplier = Aux.MinValueToReturn(waterDamageMultiplier, 0); // Prevent negative water damage.
    }

    public void ChangeElectricityDamageMultiplier(float value, OperationType operation)
    {
        electricityDamageMultiplier = ApplyOperation(electricityDamageMultiplier, value, operation);
        electricityDamageMultiplier = Aux.MinValueToReturn(electricityDamageMultiplier, 0); // Prevent negative electricity damage.
    }

    public void ChangeEtherDamageMultiplier(float value, OperationType operation)
    {
        etherDamageMultiplier = ApplyOperation(etherDamageMultiplier, value, operation);
        etherDamageMultiplier = Aux.MinValueToReturn(etherDamageMultiplier, 0); // Prevent negative ether damage.
    }

    public void ChangeImpactDamageMultiplier(float value, OperationType operation)
    {
        impactDamageMultiplier = ApplyOperation(impactDamageMultiplier, value, operation);
        impactDamageMultiplier = Aux.MinValueToReturn(impactDamageMultiplier, 0); // Prevent negative impact damage.
    }

    public void ChangePunctureDamageMultiplier(float value, OperationType operation)
    {
        punctureDamageMultiplier = ApplyOperation(punctureDamageMultiplier, value, operation);
        punctureDamageMultiplier = Aux.MinValueToReturn(punctureDamageMultiplier, 0); // Prevent negative piercing damage.
    }

    public void ChangeSlashDamageMultiplier(float value, OperationType operation)
    {
        slashDamageMultiplier = ApplyOperation(slashDamageMultiplier, value, operation);
        slashDamageMultiplier = Aux.MinValueToReturn(slashDamageMultiplier, 0); // Prevent negative slash damage.
    }



    #endregion

    #region GlobalDamage

    [field: SerializeField] public float GlobalDamage_Multiplier { get; private set; } = 1;
    [field: SerializeField] public float IncreaseGlobalDamageBy { get; private set; } = 0;


    public void ChangeGlobalDamageMultiplier(float value, OperationType operation)
    {
        GlobalDamage_Multiplier = ApplyOperation(GlobalDamage_Multiplier, value, operation);
        GlobalDamage_Multiplier = Aux.MinValueToReturn(GlobalDamage_Multiplier, MIN_FLOAT); // Prevent negative or excessively low global damage.
    }

    public void ChangeIncreaseGlobalDamageBy(float value, OperationType operation)
    {
        IncreaseGlobalDamageBy = ApplyOperation(IncreaseGlobalDamageBy, value, operation);
    }
    #endregion

    #region GunDamage

    [field: SerializeField] public float IncreaseGunDamageBy { get; private set; } = 0;
    [field: SerializeField] public float GunDamage_Multiplier { get; private set; } = 1;
    [field: SerializeField] public float GunCriticalDamage_Multiplier { get; private set; } = 1;
    [field: SerializeField] public float GunCriticalChance_Multiplier { get; private set; } = 1;

    [field: SerializeField] public float FireRate_Multiplier { get; private set; } = 1;

    [field: SerializeField] public bool doubleDamageOnLastBullet { get; private set; } = false;

    public void ChangeDoubleDamageOnLastBullet(bool doubleDamage, OperationType operation)
    {
        if (operation == OperationType.Ignore) return;
        doubleDamageOnLastBullet = doubleDamage;
    }

    public void ChangeFireRateMultiplier(float value, OperationType operation)
    {
        FireRate_Multiplier = Aux.MinValueToReturn(ApplyOperation(FireRate_Multiplier, value, operation), MIN_FLOAT);
    }

    public void ChangeIncreaseGunDamageBy(float value, OperationType operation)
    {
        IncreaseGunDamageBy = ApplyOperation(IncreaseGunDamageBy, value, operation);
    }

    public void ChangeGunDamageMultiplier(float value, OperationType operation)
    {
        GunDamage_Multiplier = Aux.ReturnZeroIfNegative(ApplyOperation(GunDamage_Multiplier, value, operation));
    }

    public void ChangeGunCriticalDamageMultiplier(float value, OperationType operation)
    {
        GunCriticalDamage_Multiplier = Aux.ReturnZeroIfNegative(ApplyOperation(GunCriticalDamage_Multiplier, value, operation));
    }

    public void ChangeGunCriticalChanceMultiplier(float value, OperationType operation)
    {
        GunCriticalChance_Multiplier = Aux.ReturnZeroIfNegative(ApplyOperation(GunCriticalChance_Multiplier, value, operation));
    }
    #endregion

    #region GunAmmo

    [field: SerializeField] public int increaseMaxSpareAmmoBy { get; set; } = 0;
    [field: SerializeField] public int increaseMaxAmmoOnClipBy { get; set; } = 0;
    [field: SerializeField] public int increaseAmmoConsumedPerShot { get; set; } = 0;

    [field: SerializeField] public bool explosionOnReload { get; private set; } = false;
    [field: SerializeField] public DamageType explosionOnReloadDamageType { get; private set; } = DamageType.Explosive;

    [field: SerializeField] public float IncreaseReloadTimeBy { get; set; } = 0;

    [field: SerializeField] public float ReloadTime_Multiplier { get; private set; } = 1;
    [field: SerializeField] public float GunSpareAmmo_Multiplier { get; private set; } = 1;
    [field: SerializeField] public float GunMaxAmmoOnClip_Multiplier { get; private set; } = 1;
    [field: SerializeField] public float GunAmmoConsumedPerShot_Multiplier { get; private set; } = 1;
    [field: SerializeField] public float ChanceOfNotConsumingAmmo_Multiplier { get; private set; } = 1;
    [field: SerializeField] public float IncreaseChanceOfNotConsumingAmmoBy { get; private set; }

    [field: SerializeField] public float chanceOfFillingAmmoOnKill { get; private set; } = 0;

    public void ChangeExplosionOnReload(bool value, OperationType operation)
    {
        if(operation == OperationType.Ignore) return;
        explosionOnReload = value;
    }

    public void ChangeExplosionOnReloadDamageType(DamageType type, OperationType operation)
    {
        if (operation == OperationType.Ignore) return;
        explosionOnReloadDamageType = type;
    }

    public void ChangeChanceOfFillingAmmoOnKill(float value, OperationType operation)
    {
        chanceOfFillingAmmoOnKill = ApplyOperation(chanceOfFillingAmmoOnKill, value, operation);
        chanceOfFillingAmmoOnKill = Aux.ReturnZeroIfNegative(chanceOfFillingAmmoOnKill);
    }

    public void ChangeIncreaseReloadTimeBy(float value, OperationType operation)
    {
        IncreaseReloadTimeBy = ApplyOperation(IncreaseReloadTimeBy, value, operation);
    }

    public void ChangeReloadTime_Multiplier(float value, OperationType operation)
    {
        //if (!ValueIsValid(value)) return;
        ReloadTime_Multiplier = ApplyOperation(ReloadTime_Multiplier, value, operation);
        ReloadTime_Multiplier = Aux.MinValueToReturn(ReloadTime_Multiplier, 0);

    }

    public void ChangeGunSpareAmmoMultiplier(float value, OperationType operation)
    {
        //if (!ValueIsValid(value)) return;
        
        GunSpareAmmo_Multiplier = ApplyOperation(GunSpareAmmo_Multiplier, value, operation);
        GunSpareAmmo_Multiplier = Aux.MinValueToReturn(GunSpareAmmo_Multiplier, 0);
    }

    public void ChangeGunMaxAmmoOnClipMultiplier(float value, OperationType operation)
    {
        GunMaxAmmoOnClip_Multiplier = ApplyOperation(GunMaxAmmoOnClip_Multiplier, value, operation);
        GunMaxAmmoOnClip_Multiplier = Aux.MinValueToReturn(GunMaxAmmoOnClip_Multiplier, 0);
    }

    public void ChangeGunAmmoConsumedPerShotMultiplier(float value, OperationType operation)
    {
        GunAmmoConsumedPerShot_Multiplier = ApplyOperation(GunAmmoConsumedPerShot_Multiplier, value, operation);
        GunAmmoConsumedPerShot_Multiplier = Aux.MinValueToReturn(GunAmmoConsumedPerShot_Multiplier, 0);
    }

    public void ChangeChanceOfNotConsumingAmmoMultiplier(float chance, OperationType operation)
    {
        //if (!ValueIsValid(chance)) return;
        ChanceOfNotConsumingAmmo_Multiplier = ApplyOperation(ChanceOfNotConsumingAmmo_Multiplier, chance, operation);
        ChanceOfNotConsumingAmmo_Multiplier = Aux.MinValueToReturn(ChanceOfNotConsumingAmmo_Multiplier, 0);
    }

    public void ChangeIncreaseChanceOfNotConsumingAmmoBy(float chance, OperationType operation)
    {
        //if (!ValueIsValid(chance)) return;
        IncreaseChanceOfNotConsumingAmmoBy = ApplyOperation(IncreaseChanceOfNotConsumingAmmoBy, chance, operation);
    }
    #endregion

    #region MeleeDamage

    [field: SerializeField] public int IncreaseMeleeDamageBy { get; private set; } = 0;
    [field: SerializeField] public float MeleeDamage_Multiplier { get; private set; } = 1;
    [field: SerializeField] public float MeleeCriticalDamage_Multiplier { get; private set; } = 1;
    [field: SerializeField] public float MeleeCriticalChance_Multiplier { get; private set; } = 1;

    public void ChangeIncreaseMeleeDamageBy(int value, OperationType operation)
    {
        IncreaseMeleeDamageBy = ApplyOperation(IncreaseMeleeDamageBy, value, operation);
    }

    public void ChangeMeleeDamageMultiplier(float value, OperationType operation)
    {
        MeleeDamage_Multiplier = ApplyOperation(MeleeDamage_Multiplier, value, operation);
        MeleeDamage_Multiplier = Aux.MinValueToReturn(MeleeDamage_Multiplier, 0); // Prevent negative or excessively low melee damage.
    }

    public void ChangeMeleeCriticalDamageMultiplier(float value, OperationType operation)
    {

        MeleeCriticalDamage_Multiplier = ApplyOperation(MeleeCriticalDamage_Multiplier, value, operation);
        MeleeCriticalDamage_Multiplier = Aux.MinValueToReturn(MeleeCriticalDamage_Multiplier, 0); // Prevent negative or excessively low melee critical damage.
    }

    public void ChangeMeleeCriticalChanceMultiplier(float value, OperationType operation)
    {
        MeleeCriticalChance_Multiplier = ApplyOperation(MeleeCriticalChance_Multiplier, value, operation);
        MeleeCriticalChance_Multiplier = Aux.MinValueToReturn(MeleeCriticalChance_Multiplier, 0); // Prevent negative or excessively low melee critical chance.
    }
    #endregion

    #region Thorn Damage

    [field: SerializeField] public int ThornDamage { get; private set; } = 0;
    [field: SerializeField] public DamageType ThornDamageType { get; private set; } = DamageType.Impact;
    [field: SerializeField] public float ThornInstaKillChance { get; private set; } = 0;

    public void ChangeThornInstaKillChance(float value, OperationType operation)
    {
        float chance = ApplyOperation(ThornInstaKillChance, value, operation);
        ThornInstaKillChance = chance;
    }

    public void ChangeThornDamageType(DamageType type, OperationType operation)
    {
        if (operation == OperationType.Ignore) return;
        ThornDamageType = type;
    }

    public void ChangeThornDamageBy(int value, OperationType operation)
    {
        ThornDamage = ApplyOperation(ThornDamage, value, operation);
        ThornDamage = Mathf.FloorToInt( Aux.MinValueToReturn(ThornDamage, 0)); // Prevent negative thorn damage.
    }
    #endregion

    #region MultipleBullets

    /// <summary>
    /// Chance to fire an extra bullet.
    /// </summary>
    [field: SerializeField] public float chanceOfShootingExtraBullet { get; private set; } = 0;
    [field: SerializeField] public int IncreaseScatteredBulletsBy { get; private set; } = 0;
    [field: SerializeField] public int IncreaseLinedUpBulletsBy { get; private set; } = 0;

    public void ChangeChanceOfShootingExtraBullets(float value, OperationType operation)
    {
        chanceOfShootingExtraBullet = ApplyOperation(chanceOfShootingExtraBullet, value, operation);
        chanceOfShootingExtraBullet = Aux.MinValueToReturn(chanceOfShootingExtraBullet, 0); // Prevent negative extra-bullet chance.
    }

    public void ChangeIncreaseScatteredBulletsBy(int value, OperationType operation)
    {
        IncreaseScatteredBulletsBy = ApplyOperation(IncreaseScatteredBulletsBy, value, operation);

    }

    public void ChangeIncreaseLinedUpBulletsBy(int value, OperationType operation)
    {
        IncreaseLinedUpBulletsBy = ApplyOperation(IncreaseLinedUpBulletsBy, value, operation);
    }
    #endregion

    #region AuxMethods

    /// <summary>
    /// Reapply values during level transitions, not on death.
    /// Call ResetValues on death.
    /// </summary>
    public void ActivateOnNewScene()
    {

    }
    /// <summary>
    /// Reset all variables.
    /// </summary>
    [ContextMenu(nameof(ResetValues))]

    
    public void ResetValues()
    {
        //prices
        doorPriceMultiplier = 1;
        meleeWeaponPriceMultiplier = 1;
        gunUpgradePriceMultiplier = 1;
        runePriceMultiplier = 1;
        gunPriceMultiplier = 1;
        globalPriceMultiplier = 1;

        //start run 
        numberOfRandomRunesToStartWith = 0;
        numberOfRandomGunsToStartWith = 0;

        //Misc bool 
        RefilAmmoOnOpenDoor = false;
        FreeRevives = 0;
        walkTrailDamageType = DamageType.None;

        //Points
        extraPointsPerKill = 0;
        extraInitialPoints = 0;

        //status 
        maxChainReactions = 5;
        statusChanceMultiplier = 1;
        statusDamageMultiplier = 1;
        statusDurationMultiplier = 1;

        //drop
        AddTodropChance = 0;
        AddToHpHealedByHearts = 1;
        powerUpGoTowardsPlayerMaxDistanceMultiplier = 1;

        // Player
        maxIgnoreDamageChance = 0.5f;
        increaseWeaponCapacityBy = 0;
        playerWalkSpeedMultiplier = 1;
        AddToPlayerInitialHealth = 0;
        IncreasePlayerMaxHealthBy = 0;
        ChanceOfIgnoreDamage_Multiplier = 0;
        IncreaseChanceOfIgnoreDamageBy = 0;
        ChanceOfIgnoreDamage_Multiplier = 1;
        ChanceOfGain1HpEvery30s = 0;
        ShieldDurationOnTakingDamage = 0;
        healthPointsToHealOnBuyWeapon = 0;

        //enemies 
        enemyHealthMultiplier = 1;

        // OrbitalUpgrades
        orbitalUpgrades.Clear();
        IncreaseOrbitalSpeedBy = 0;
        OrbitalSpeed_Multiplier = 1;

        // Assassin Blade (orbital upgrade)
        OrbitalBladeInstaKillChance = 0f;
        OrbitalBladeInstaKillChance_Multiplier = 1.2f;

        // Swift Blade (orbital upgrade)
        OrbitalRadius_Multiplier = 1;

        // Sharp Blade (orbital upgrade)
        OrbitalBladeDamage_Multiplier = 1;
        IncreaseOrbitalBladeDamageBy = 0;

        // melee
        maxMeleeActionQuantity = 1;
        MeleeAttackRateMultiplier = 1;

        //special ability 
        maxSpecialActionQuantity = 1;
        SpecialAbilityCooldownMultiplier = 1;
        resetSpecialAbilityOnEnemyDeathChance = 0;

        //dash
        dashDamageType = DamageType.Impact;
        maxDashActionQuantity = 1;
        DashAbilityCooldownMultiplier = 1;
        dashDamageMultiplier = 1;

        // GlobalDamage
        GlobalDamage_Multiplier = 1;
        IncreaseGlobalDamageBy = 0;

        // GunDamage
        IncreaseGunDamageBy = 0;
        GunDamage_Multiplier = 1;
        GunCriticalDamage_Multiplier = 1;
        GunCriticalChance_Multiplier = 1;
        FireRate_Multiplier = 1;
        doubleDamageOnLastBullet = false;

        //elemental Damage
        fireDamageMultiplier = 1;
        iceDamageMultiplier  = 1;
        poisonDamageMultiplier  = 1;
        waterDamageMultiplier  = 1;
        electricityDamageMultiplier = 1;
        etherDamageMultiplier= 1;
        impactDamageMultiplier  = 1;
        punctureDamageMultiplier  = 1;
        slashDamageMultiplier = 1;

         // GunAmmo
        chanceOfFillingAmmoOnKill = 0;
        increaseMaxSpareAmmoBy = 0;
        increaseMaxAmmoOnClipBy = 0;
        increaseAmmoConsumedPerShot = 0;
        explosionOnReload = false;
        explosionOnReloadDamageType = DamageType.Explosive;

        IncreaseReloadTimeBy = 0;
        ReloadTime_Multiplier = 1;
        GunSpareAmmo_Multiplier = 1;
        GunMaxAmmoOnClip_Multiplier = 1;
        GunAmmoConsumedPerShot_Multiplier = 1;
        ChanceOfNotConsumingAmmo_Multiplier = 1;
        IncreaseChanceOfNotConsumingAmmoBy = 0;

        // MeleeDamage
        IncreaseMeleeDamageBy = 0;
        MeleeDamage_Multiplier = 1;
        MeleeCriticalDamage_Multiplier = 1;
        MeleeCriticalChance_Multiplier = 1;

        //thorn damage
        ThornDamage = 0;
        ThornDamageType = DamageType.Impact;
        ThornInstaKillChance = 0;


        // MultipleBullets
        chanceOfShootingExtraBullet = 0;
        IncreaseScatteredBulletsBy = 0;
        IncreaseLinedUpBulletsBy = 0;

        Debug.Log("Global Stats reseted");
    }


    private bool ValueIsValid(float value)
    {
        if (value < 0 || value > 1000)
        {
            Debug.LogError($"O valor inserido {value} é invalido. \nObjeto: " + name);
            return false;
        }
        return true;
    }

    private int ApplyOperation(int original, int value, OperationType operation)
    {
        return operation switch
        {
            OperationType.AddUp => original + value,
            OperationType.Multiply => original * value,
            OperationType.Replace => value,
            _ => original,
        };
    }

    private float ApplyOperation(float original, float value, OperationType operation)
    {
        return operation switch
        {
            OperationType.AddUp => original + value,
            OperationType.Multiply => original * value,
            OperationType.Replace => value,
            _ => original,
        };
    }

    #endregion
}

public enum OperationType { Ignore, Multiply, AddUp, Replace }
