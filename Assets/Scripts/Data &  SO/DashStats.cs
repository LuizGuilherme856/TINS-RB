using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashStats : MeleeStats
{
    public StatsCalculator statsCalculator;
    public GlobalStats globalStats;
    public override (float damage, CriticalDamage critical) GetDamage()
    {
        float damage =  statsCalculator.CalculateDashDamage( weaponData.GetDamage());

        if (Random.Range(0.01f, 100f) < weaponData.instaKillChance)
        {
            return (float.MaxValue, CriticalDamage.instaKill);
        }

        if (Random.Range(0, 100f) <= weaponData.criticalChance)
        {
            // Debug.Log("Critical damage");
            return (damage * weaponData.criticalMultiplier * damageMultiplier_powerUp, CriticalDamage.critical);
        }
        else
        {
            return (damage * damageMultiplier_powerUp, CriticalDamage.no);
        }
    }

    public override DamageType GetDamageType()
    {
        return globalStats.ForCurrentRun.dashDamageType;
    }
}
