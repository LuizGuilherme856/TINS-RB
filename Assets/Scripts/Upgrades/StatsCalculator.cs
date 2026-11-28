using TINS.Campaign;
using System;
using UnityEngine;

//[CreateAssetMenu(fileName = "Stats Calculator", menuName = "Stats Calculator")]

/// <summary>
/// Calculate derived damage, health, and other stats.
/// Read the required modifiers from GlobalStats for each calculation.
/// </summary>
public class StatsCalculator : ScriptableObject
{
    [SerializeField] private bool showDebug;
    [SerializeField] private GlobalStats globalStats;

    public float minAttackRate = 0.1f; //Minimum attack interval; must remain above zero.
    public float minSpecialAbilityCooldown = 2f; //Minimum special-ability cooldown; must remain above zero.
    public float minDashCooldown = 0.2f;

    #region STATUS
    public float CalculateNewStatusChance(float statusChance)
    {
        float value = statusChance * globalStats.ForCurrentRun.statusChanceMultiplier;
        return Aux.ReturnZeroIfNegative(value);
    }

    public float CalculateNewStatusDamage(float statusDamage)
    {
        float value = statusDamage * globalStats.ForCurrentRun.statusDamageMultiplier;
        return Aux.ReturnZeroIfNegative(value);
    }

    public float CalculateNewStatusDuration(float statusDuration)
    {
        float value = statusDuration * globalStats.ForCurrentRun.statusDurationMultiplier;

        return Aux.ReturnZeroIfNegative(value);
    }


    #endregion

    #region Player

    public float CalculatePlayerSpeed(float defaultSpeed)
    {
        float newSpeed = defaultSpeed * globalStats.ForCurrentRun.playerWalkSpeedMultiplier;
        //Minimum speed is 1.
        if (newSpeed < 1) newSpeed = 1;
        return newSpeed;
    }

    public float CalculateIgnoreDamageChance(float baseChance)
    {
        float chance = ChanceMultiplierHandler(baseChance, globalStats.ForCurrentRun.ChanceOfIgnoreDamage_Multiplier) + globalStats.ForCurrentRun.IncreaseChanceOfIgnoreDamageBy;
        Aux.Print("CalculateIgnoreDamageChance: " + chance, showDebug);

        if (chance > globalStats.ForCurrentRun.maxIgnoreDamageChance)
        {
            Debug.Log("Atingiu o máximo de chance de ignorar dano. Setando a chance pro valor máximo de: " + globalStats.ForCurrentRun.maxIgnoreDamageChance);
            return globalStats.ForCurrentRun.maxIgnoreDamageChance;
        }
        else return chance;

    }

    public int CalculatePlayerInitialHealthPoints(int defaultHp)
    {
        var hp = defaultHp + globalStats.ForCurrentRun.AddToPlayerInitialHealth;

        Aux.Print("CalculatePlayerInitialHealthPoints: " + hp, showDebug);
        return Aux.ReturnOneIfLessThanOne(hp);
    }

    public int CalculateMaxHealthpoints(int defaultHp)
    {
        var hp = defaultHp + globalStats.ForCurrentRun.IncreasePlayerMaxHealthBy + globalStats.ForCurrentRun.AddToPlayerInitialHealth;
        Aux.Print("CalculateMaxHealthpoints: " + hp, showDebug);
        return Aux.ReturnOneIfLessThanOne(hp);
    }

    #endregion

    #region OrbitalUpgrade

    public float CalculateOrbitalSpeed(float speed)
    {
        return speed * globalStats.ForCurrentRun.OrbitalSpeed_Multiplier + globalStats.ForCurrentRun.IncreaseOrbitalSpeedBy;
    }


    public float CalculateOrbitalCorrectionSpeed(float speed)
    {
        return speed * globalStats.ForCurrentRun.OrbitalCorrectionSpeed_Multiplier + globalStats.ForCurrentRun.IncreaseOrbitalCorrectionSpeedBy;
    }


    public float CalculateOrbitalBladeRadius(float radius)
    {
        float newRadius = radius * globalStats.ForCurrentRun.OrbitalRadius_Multiplier;

        if (newRadius < globalStats.ForCurrentRun.orbitalBladeMinRadius) newRadius = globalStats.ForCurrentRun.orbitalBladeMinRadius;
        return newRadius;
    }

    public float CalculateOrbitalBladeInstaKillChance(float weaponDefaultChance)
    {
        var gs = globalStats.ForCurrentRun; //Local values keep formulas readable.
        float newChance = (weaponDefaultChance + gs.OrbitalBladeInstaKillChance) * gs.OrbitalBladeInstaKillChance_Multiplier;
        return newChance;
    }

    public float CalculateOrbitalBladeDamage(float damage)
    {
        var gs = globalStats.ForCurrentRun;
        float newDamage = (damage + gs.IncreaseOrbitalBladeDamageBy) * gs.OrbitalBladeDamage_Multiplier;

        newDamage = CalculateGlobalDamage(newDamage);
        return Aux.ReturnZeroIfNegative(MathF.Floor(newDamage));
    }


    #endregion

    #region GlobalDamage
    //Global damage applies to melee, guns, abilities, and other attacks.

    public float CalculateGlobalDamage(float damage)
    {
        float newDamage = damage * globalStats.ForCurrentRun.GlobalDamage_Multiplier + globalStats.ForCurrentRun.IncreaseGlobalDamageBy;

        Aux.Print("CalculateGlobalDamage: " + newDamage, showDebug);
        return Aux.ReturnZeroIfNegative(newDamage);
    }
    #endregion

    public int MaxDashActions => globalStats.ForCurrentRun.maxDashActionQuantity + Mathf.RoundToInt(
        CampaignMissionRuntime.Active?.SkillEffect(CampaignSkillEffectType.ExtraDash).x ?? 0f);

    #region ActionRelated

    public float CalculateMaxTimeCoolDown(float baseAttackRate, PlayerAction playerAction)
    {
        switch (playerAction)
        {
            case PlayerAction.melee:
                return Aux.MinValueToReturn( baseAttackRate * globalStats.ForCurrentRun.maxMeleeActionQuantity * globalStats.ForCurrentRun.MeleeAttackRateMultiplier, minAttackRate);
            case PlayerAction.special:
                return Aux.MinValueToReturn(baseAttackRate * globalStats.ForCurrentRun.maxSpecialActionQuantity * globalStats.ForCurrentRun.SpecialAbilityCooldownMultiplier, minSpecialAbilityCooldown);
            case PlayerAction.dash:
                return Aux.MinValueToReturn(baseAttackRate * MaxDashActions * globalStats.ForCurrentRun.DashAbilityCooldownMultiplier, minDashCooldown);

            default:
                Debug.LogError("No momento do calculo do Tempo Maximo De uma a��o,uma a��o n�o registrada foi pedida");
                return 1;
        }
    }
    public float CalculatePerActionCoolDown(float baseAttackRate, PlayerAction playerAction)
    {
        switch (playerAction)
        {
            case PlayerAction.melee:
                return baseAttackRate * globalStats.ForCurrentRun.MeleeAttackRateMultiplier;
            case PlayerAction.special:
                return baseAttackRate * globalStats.ForCurrentRun.SpecialAbilityCooldownMultiplier;
            case PlayerAction.dash:
                return baseAttackRate * globalStats.ForCurrentRun.DashAbilityCooldownMultiplier;
            default:
                Debug.LogError("No momento do calculo do CoolDown de uma a��o,uma a��o n�o registrada foi pedida");
                return 1;

        }
    }

    /// <summary>
    /// Calculate actionCooldown from attackRate, MaxAction, and the action multiplier.
    /// Cap the value to the maximum number of UI bars.
    /// </summary>
    public float GetTotalActionCooldown_Percentage(float baseAttackRate, float currentAttackTimer, PlayerAction playerAction, int maxActionCap)
    {
        float totalAttackRate;
        switch (playerAction)
        {
            case PlayerAction.melee:
                if (globalStats.ForCurrentRun.maxMeleeActionQuantity > maxActionCap)
                {
                    totalAttackRate = baseAttackRate * maxActionCap * globalStats.ForCurrentRun.MeleeAttackRateMultiplier;
                }
                else
                {
                    totalAttackRate = baseAttackRate * globalStats.ForCurrentRun.maxMeleeActionQuantity * globalStats.ForCurrentRun.MeleeAttackRateMultiplier;
                }
                break;
            case PlayerAction.special:
                if (globalStats.ForCurrentRun.maxSpecialActionQuantity > maxActionCap)
                {
                    totalAttackRate = baseAttackRate * maxActionCap * globalStats.ForCurrentRun.SpecialAbilityCooldownMultiplier;
                }
                else
                {
                    totalAttackRate = baseAttackRate * globalStats.ForCurrentRun.maxSpecialActionQuantity * globalStats.ForCurrentRun.SpecialAbilityCooldownMultiplier;
                }
                break;
            case PlayerAction.dash:
                if (MaxDashActions > maxActionCap)
                {
                    totalAttackRate = baseAttackRate * maxActionCap * globalStats.ForCurrentRun.DashAbilityCooldownMultiplier;
                }
                else
                {
                    totalAttackRate = baseAttackRate * MaxDashActions * globalStats.ForCurrentRun.DashAbilityCooldownMultiplier;
                }
                break;
            default:
                Debug.LogError("No momento do calculo da UI de uma a��o,uma a��o n�o registrada foi pedida");
                return 0;

        }
        return currentAttackTimer / totalAttackRate;
    }


    #endregion

    #region Dash

    public float CalculateDashDamage(float defaultDamage)
    {
        float newDamage = defaultDamage * globalStats.ForCurrentRun.dashDamageMultiplier;
        return CalculateGlobalDamage(newDamage);
    }
    #endregion

    #region GunDamage

    public float CalculateNewGunDamage(float damage, DamageType damageType)
    {

        float elementalMultiplier = globalStats.ForCurrentRun.GetElementalDamageMultiplier(damageType);

        float newDamage = damage * elementalMultiplier * globalStats.ForCurrentRun.GunDamage_Multiplier + globalStats.ForCurrentRun.IncreaseGunDamageBy;

        newDamage = CalculateGlobalDamage(newDamage);

        Aux.Print("CalculateNewGunDamage: " + newDamage, showDebug);

        return Aux.ReturnZeroIfNegative(newDamage);
    }

    public float CalculateNewGunCriticalChance(float chance)
    {
        CampaignMissionRuntime mission = CampaignMissionRuntime.Active;
        return mission != null ? Mathf.Clamp(mission.SkillEffect(CampaignSkillEffectType.GunCritical).x, 0f, 100f) :
            chance * globalStats.ForCurrentRun.GunCriticalChance_Multiplier;
    }

    public float CalculateNewGunCriticalMultiplier(float criticalMultiplier)
    {
        return criticalMultiplier * globalStats.ForCurrentRun.GunCriticalDamage_Multiplier;
    }

    public float CalculateNewFireRate(float fireRate)
    {
        return fireRate * globalStats.ForCurrentRun.GunCriticalDamage_Multiplier;
    }

    #endregion

    #region MeleeDamage

    public float CalculateNewMeleeDamage(float damage, DamageType damageType)
    {
        float elementalMultiplier = globalStats.ForCurrentRun.GetElementalDamageMultiplier(damageType);
        float newDamage = damage * elementalMultiplier;

        newDamage = newDamage * globalStats.ForCurrentRun.MeleeDamage_Multiplier + globalStats.ForCurrentRun.IncreaseMeleeDamageBy;

        newDamage = CalculateGlobalDamage(newDamage);

        return Aux.ReturnZeroIfNegative(newDamage);
    }

    public float CalculateNewMeleeCriticalChance(float chance)
    {
        return chance * globalStats.ForCurrentRun.MeleeCriticalChance_Multiplier;
    }

    public float CalculateNewMeleeCriticalMultiplier(float criticalDamage)
    {
        return criticalDamage * globalStats.ForCurrentRun.MeleeCriticalDamage_Multiplier;
    }

    #endregion

    #region GunAmmo


    public float CalculateNewReloadTime(float defaultTime)
    {
        float newReloadTime = defaultTime * globalStats.ForCurrentRun.ReloadTime_Multiplier + globalStats.ForCurrentRun.IncreaseReloadTimeBy;

        return Aux.ReturnZeroIfNegative(newReloadTime);
    }

    public int CalculateNewSpareAmmo(int spareAmmo)
    {
        float newSpareAmmo = spareAmmo * globalStats.ForCurrentRun.GunSpareAmmo_Multiplier + globalStats.ForCurrentRun.increaseMaxSpareAmmoBy;

        if (newSpareAmmo < 1) newSpareAmmo = 1;
        return Mathf.CeilToInt(newSpareAmmo);
    }

    public int CalculateNewAmmoOnClip(int ammoOnClip)
    {
        float newAmmoOnClip = ammoOnClip * globalStats.ForCurrentRun.GunMaxAmmoOnClip_Multiplier + globalStats.ForCurrentRun.increaseMaxAmmoOnClipBy;

        if (newAmmoOnClip < 1) newAmmoOnClip = 1;
        return Mathf.CeilToInt(newAmmoOnClip);
    }

    public int CalculateNewAmmoConsumedPerShot(int ammoConsumed, int maxSpareAmmo)
    {
        //Limit the burst to available magazine ammunition to avoid trapping the player.
        //TODO: Review the case where a one-bullet weapon fires 50 upgraded projectiles but consumes only one round.
        //if (newConsumedPershot > maxSpareAmmo) return maxSpareAmmo;
        float newAmmoConsumedPerShot = ammoConsumed * globalStats.ForCurrentRun.GunAmmoConsumedPerShot_Multiplier + globalStats.ForCurrentRun.increaseAmmoConsumedPerShot;
        if (newAmmoConsumedPerShot < 1) newAmmoConsumedPerShot = 1;

        return Mathf.CeilToInt(newAmmoConsumedPerShot);
    }

    public float CalculateChanceOfNotConsumingAmmo(float baseChance)
    {
        float chance = baseChance * ChanceMultiplierHandler(baseChance, globalStats.ForCurrentRun.ChanceOfNotConsumingAmmo_Multiplier);
        chance += globalStats.ForCurrentRun.IncreaseChanceOfNotConsumingAmmoBy;

        if (chance < 0) chance = 0;
        else if (chance > 1) chance = 1;

        return chance;
    }



    #endregion

    #region MultipleBullets

    /// <summary>
    /// Calculate scattered bullets per shot, with a minimum of 1.
    /// </summary>

    public int CalculateScatteredBulletsAmount(int weaponScatteredBulletsBaseValue)
    {
        int newValue = weaponScatteredBulletsBaseValue + globalStats.ForCurrentRun.IncreaseScatteredBulletsBy;
        newValue = Aux.MaxValueToReturn(newValue, 80);
        return Aux.ReturnOneIfLessThanOne(newValue);
    }

    /// <summary>
    /// Calculate trailing aligned bullets per shot.
    /// </summary>
    public int CalculateLinedUpBulletsAmount(int weaponLinedUpBulletsBaseValue)
    {
        int newValue = weaponLinedUpBulletsBaseValue + globalStats.ForCurrentRun.IncreaseLinedUpBulletsBy;
        newValue = Aux.MaxValueToReturn(newValue, 30);
        return Aux.ReturnZeroIfNegative(newValue);
    }


    #endregion


    #region AuxMethods

    /// <summary>
    /// Calculate modified probabilities.
    /// Multiply a nonzero base chance normally.
    /// When the base is zero, use minChance as the starting point
    /// so a multiplier upgrade can still grant a chance.
    /// </summary>

    private float ChanceMultiplierHandler(float baseChance, float globalStatsMultiplier)
    {
        if (baseChance > 1 || baseChance < 0)
        {
            Debug.LogWarning("Valores em porcentagem deveriam ficar entre 0 e 1");
        }

        //Use this minimum when multiplying
        //a zero base probability.
        float minChance = 0.05f;
        //Keep zero when base chance is zero and the multiplier is 1,
        //since those defaults mean no chance.
        if (baseChance <= 0 && globalStatsMultiplier == 1)
        {
            return 0;
        }
        //A non-default multiplier indicates an intended change to a zero base chance.
        //Use the multiplier to establish that chance.
        else if (baseChance <= 0 && globalStatsMultiplier != 1)
        {
            return minChance * globalStatsMultiplier;
        }
        else //Multiply the existing nonzero chance.
        {
            return baseChance * globalStatsMultiplier;
        }
    }
    #endregion

}
