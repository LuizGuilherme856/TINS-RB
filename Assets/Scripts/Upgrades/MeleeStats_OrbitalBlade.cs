using UnityEngine;

/// <summary>
/// MeleeStats specialization for orbital blades.
/// </summary>
public class MeleeStats_OrbitalBlade : MeleeStats
{
    [SerializeField] private StatsCalculator statsCalculator;
    public override (float damage, CriticalDamage critical) GetDamage()
    {
        if (Random.Range(0.01f, 100f) < statsCalculator.CalculateOrbitalBladeInstaKillChance( weaponData.instaKillChance))
        {
            return (float.MaxValue, CriticalDamage.instaKill);
        }

        return (statsCalculator.CalculateOrbitalBladeDamage(weaponData.GetDamage()), CriticalDamage.no);
    }

}
