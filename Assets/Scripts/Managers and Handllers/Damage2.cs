using TINS.Campaign;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Elemental interactions for the Damage 2.0 system.
/// </summary>
public class Damage2 : MonoBehaviour
{
    public static float INSTA_KILL_DAMAGE = 999999;

    public static CriticalDamage GetCritical(float criticalChance)
    {
        float randomNumber = Random.Range(0, 100f);
        if (randomNumber <= criticalChance)
        {
            //Critical chance above 100% can produce a super critical for double critical damage.
            if (criticalChance > 100)
            {
                randomNumber = Random.Range(0, 100f);
                float superCriticalChance = criticalChance - 100;
                if (randomNumber <= superCriticalChance)
                {
                    return  CriticalDamage.superCritical;
                }

            }
            return CriticalDamage.critical;
        }
        else
        {
            return  CriticalDamage.no;
        }
    }


    public static (float, CriticalDamage) GetDamageAndCritical( float baseDamage, GunData GunData, StatsCalculator statsCalculator)
    {
        float criticalChance = statsCalculator.CalculateNewGunCriticalChance(GunData.criticalChance);
        float criticalMultiplier = statsCalculator.CalculateNewGunCriticalMultiplier(GunData.criticalMultiplier);

        CampaignMissionRuntime mission = CampaignMissionRuntime.Active;
        CriticalDamage critical = mission != null ? RollCampaignCritical(criticalChance, mission.SuperCriticalChance) : GetCritical(criticalChance);
        switch (critical)
        {
            case CriticalDamage.no:
                return (baseDamage, CriticalDamage.no);
            case CriticalDamage.critical:
                return (baseDamage * criticalMultiplier, CriticalDamage.critical);
            case CriticalDamage.superCritical:
                return (2 * baseDamage * criticalMultiplier, CriticalDamage.superCritical);
            case CriticalDamage.instaKill:
                return (INSTA_KILL_DAMAGE, CriticalDamage.instaKill);
            default:
                return (baseDamage, CriticalDamage.no);
        }
    }

    private static CriticalDamage RollCampaignCritical(float criticalChance, float superCriticalChance)
    {
        if (criticalChance <= 0f || Random.value * 100f >= criticalChance) return CriticalDamage.no;
        return Random.value < superCriticalChance ? CriticalDamage.superCritical : CriticalDamage.critical;
    }

    public static (float, CriticalDamage) GetDamageAndCritical(float baseDamage, MeleeData meleeData, StatsCalculator statsCalculator)
    {
        float criticalChance = statsCalculator.CalculateNewMeleeCriticalChance(meleeData.criticalChance);
        float criticalMultiplier = statsCalculator.CalculateNewMeleeCriticalMultiplier(meleeData.criticalMultiplier);

        switch (GetCritical(criticalChance))
        {
            case CriticalDamage.no:
                return (baseDamage, CriticalDamage.no);
            case CriticalDamage.critical:
                return (baseDamage * criticalMultiplier, CriticalDamage.no);
            case CriticalDamage.superCritical:
                return (2 * baseDamage * criticalMultiplier, CriticalDamage.no);
            case CriticalDamage.instaKill:
                return (INSTA_KILL_DAMAGE, CriticalDamage.instaKill);
            default:
                return (baseDamage, CriticalDamage.no);
        }
    }


    /// <summary>
    /// Incoming damage may replace the current status effect.
    /// For example, fire melts a frozen enemy and changes its status to wet.
    /// </summary>
    /// <param name="damageType">Incoming damage type.</param>
    /// <param name="currentStatus">Enemy's current status effect.</param>
    public static StatusEffect StatusEffectInteraction(DamageType damageType, StatusEffect currentStatus)
    {
        switch (damageType)
        {
            
            case DamageType.Explosive:
                if (currentStatus == StatusEffect.Poisoned) return StatusEffect.Exploded;
                break;

            case DamageType.Fire:
                if (currentStatus == StatusEffect.Frozen) return StatusEffect.Soaked;
                else if (currentStatus == StatusEffect.Poisoned) return StatusEffect.Exploded;
                break;

            case DamageType.Ice:
                if (currentStatus == StatusEffect.Soaked) return StatusEffect.Frozen;
                break;

            case DamageType.Poison:
                if (currentStatus == StatusEffect.Bleeding) return StatusEffect.Poisoned;
                else if (currentStatus == StatusEffect.Burning) return StatusEffect.Exploded;
                break;
            case DamageType.Electricity:
                if (currentStatus == StatusEffect.Soaked) return StatusEffect.Electrocuted;
                break;

            default:
                return StatusEffect.None;
        }

        return StatusEffect.None;
    }


    /// <summary>
    /// Return status probability for the enemy element and incoming damage type.
    /// </summary>
    public static float GetStatusChance(EnemyElement enemyElement, DamageType damageType)
    {
        return StatusChanceMatrix[enemyElement][damageType];
    }



    /// <summary>
    /// Return resistance for the enemy element and incoming damage type.
    /// </summary>
    public static float GetResistance(EnemyElement enemyElement, DamageType damageType)
    {
        return ResistanceMatrix[enemyElement][damageType];
    }



    /// <summary>
    /// Return the status effect associated with a DamageType.
    /// </summary>
    /// <returns>The corresponding status effect.</returns>
    public static StatusEffect GetStatusEffect(DamageType damageType)
    {
        // Look up the corresponding effect in the damage-type map.
        return StatusEffectMap.TryGetValue(damageType, out var statusEffect)
            ? statusEffect
            : StatusEffect.None; // Return None when the damage type is unmapped.
    }

    /// <summary>
    /// Damage resistance contributed by the current status effect.
    /// </summary>
    public static DamageOnStatus GetDamageOnStatus(DamageType damageType, StatusEffect statusEffect)
    {

        switch (damageType)
        {
            case DamageType.Puncture:
                break;
            case DamageType.Slash:
                if (statusEffect == StatusEffect.Concussion) return DamageOnStatus.vulnerable;
                if (statusEffect == StatusEffect.Frozen) return DamageOnStatus.resistant;
                break;
            case DamageType.Impact:
                if (statusEffect == StatusEffect.Frozen) return DamageOnStatus.vulnerable;
                break;
            case DamageType.Explosive:
                if (statusEffect == StatusEffect.Frozen) return DamageOnStatus.vulnerable;
                break;
            case DamageType.Fire:
                if (statusEffect == StatusEffect.Frozen) return DamageOnStatus.resistant;
                if (statusEffect == StatusEffect.Soaked) return DamageOnStatus.resistant;
                if (statusEffect == StatusEffect.Electrocuted) return DamageOnStatus.vulnerable;
                break;
            case DamageType.Ice:
                if (statusEffect == StatusEffect.Burning) return DamageOnStatus.resistant;
                if (statusEffect == StatusEffect.Soaked) return DamageOnStatus.vulnerable;
                break;
            case DamageType.Poison:
                if (statusEffect == StatusEffect.Bleeding) return DamageOnStatus.vulnerable;
                if (statusEffect == StatusEffect.Burning) return DamageOnStatus.vulnerable;
                if (statusEffect == StatusEffect.Frozen) return DamageOnStatus.resistant;
                break;
            case DamageType.Water:
                if (statusEffect == StatusEffect.Burning) return DamageOnStatus.resistant;
                if (statusEffect == StatusEffect.Frozen) return DamageOnStatus.vulnerable;
                if (statusEffect == StatusEffect.Electrocuted) return DamageOnStatus.vulnerable;
                break;
            case DamageType.Electricity:
                if (statusEffect == StatusEffect.Burning) return DamageOnStatus.vulnerable;
                if (statusEffect == StatusEffect.Frozen) return DamageOnStatus.vulnerable;
                break;
            case DamageType.Ether:
                break;
            case DamageType.None:
                break;
            default:
                break;
        }

        return DamageOnStatus.none;
    }

    /// <summary>
    /// Determine whether the damage applies a status to this enemy element; otherwise return None.
    /// </summary>
    public static StatusEffect TryToApplyStatusEffect(EnemyElement enemyElement, DamageType damageType)
    {
        float chance = GetStatusChance(enemyElement, damageType);

        float randomNumber = Random.Range(0f, 1f);

        if(randomNumber <= chance)
        {
            return GetStatusEffect(damageType);
        }
        else
        {
           return StatusEffect.None;
        }
    }

    #region dictionaries

    // Static status-chance lookup table.
    private static readonly Dictionary<EnemyElement, Dictionary<DamageType, float>> StatusChanceMatrix = new()
    {
        { EnemyElement.Flesh, new Dictionary<DamageType, float>
            {
                { DamageType.Puncture, 0f }, { DamageType.Slash, 0.25f }, { DamageType.Impact, -0.25f }, { DamageType.Explosive, 0.1f },
                { DamageType.Fire, 0f }, { DamageType.Ice, 0f }, { DamageType.Poison, 0.3f }, { DamageType.Water, -0.5f },
                { DamageType.Electricity, 0.3f }, { DamageType.Ether, 0f }
            }
        },
        { EnemyElement.Bone, new Dictionary<DamageType, float>
            {
                { DamageType.Puncture, 0f }, { DamageType.Slash, 0f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0f },
                { DamageType.Fire, 0f }, { DamageType.Ice, 0f }, { DamageType.Poison, 0f }, { DamageType.Water, -0.5f },
                { DamageType.Electricity, 0f }, { DamageType.Ether, 0f }
            }
        },
        { EnemyElement.Metal, new Dictionary<DamageType, float>
            {
                { DamageType.Puncture, 0.25f }, { DamageType.Slash, -0.25f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0.5f },
                { DamageType.Fire, 0f }, { DamageType.Ice, -0.15f }, { DamageType.Poison, 0f }, { DamageType.Water, -0.5f },
                { DamageType.Electricity, 0.6f }, { DamageType.Ether, 0f }
            }
        },
        { EnemyElement.Spectral, new Dictionary<DamageType, float>
            {
                { DamageType.Puncture, 0f }, { DamageType.Slash, -0.5f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0.1f },
                { DamageType.Fire, 0.5f }, { DamageType.Ice, 0f }, { DamageType.Poison, 0f }, { DamageType.Water, -0.5f },
                { DamageType.Electricity, 0.25f }, { DamageType.Ether, 0f }
            }
        },
        { EnemyElement.Fire, new Dictionary<DamageType, float>
            {
                { DamageType.Puncture, 0f }, { DamageType.Slash, 0f }, { DamageType.Impact, 0f }, { DamageType.Explosive, -0.3f },
                { DamageType.Fire, -1f }, { DamageType.Ice, 0.25f }, { DamageType.Poison, -0.15f }, { DamageType.Water, 0.9f },
                { DamageType.Electricity, 0.25f }, { DamageType.Ether, 0f }
            }
        },
        { EnemyElement.Ice, new Dictionary<DamageType, float>
            {
                { DamageType.Puncture, 0f }, { DamageType.Slash, -0.1f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0.1f },
                { DamageType.Fire, 0.5f }, { DamageType.Ice, -1f }, { DamageType.Poison, 0f }, { DamageType.Water, -0.3f },
                { DamageType.Electricity, 0f }, { DamageType.Ether, 0f }
            }
        },
        { EnemyElement.Poison, new Dictionary<DamageType, float>
            {
                { DamageType.Puncture, 0f }, { DamageType.Slash, 0f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0f },
                { DamageType.Fire, 0.5f }, { DamageType.Ice, 0f }, { DamageType.Poison, -1f }, { DamageType.Water, 0f },
                { DamageType.Electricity, 0f }, { DamageType.Ether, 0f }
            }
        },
        { EnemyElement.Water, new Dictionary<DamageType, float>
            {
                { DamageType.Puncture, 0f }, { DamageType.Slash, 0f }, { DamageType.Impact, 0f }, { DamageType.Explosive, -0.4f },
                { DamageType.Fire, -0.5f }, { DamageType.Ice, 0.25f }, { DamageType.Poison, 0f }, { DamageType.Water, -1f },
                { DamageType.Electricity, 0.3f }, { DamageType.Ether, 0f }
            }
        },
        { EnemyElement.Electricity, new Dictionary<DamageType, float>
            {
                { DamageType.Puncture, 0f }, { DamageType.Slash, 0f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0f },
                { DamageType.Fire, 0.1f }, { DamageType.Ice, 0f }, { DamageType.Poison, 0f }, { DamageType.Water, 0f },
                { DamageType.Electricity, -1f }, { DamageType.Ether, 0f }
            }
        }
    };


    private static readonly Dictionary<EnemyElement, Dictionary<DamageType, float>> ResistanceMatrix = new()
{
    { EnemyElement.Flesh, new Dictionary<DamageType, float>
        {
            { DamageType.Puncture, 0f }, { DamageType.Slash, 0.25f }, { DamageType.Impact, -0.25f }, { DamageType.Explosive, 0.1f },
            { DamageType.Fire, 0f }, { DamageType.Ice, 0f }, { DamageType.Poison, 0.3f }, { DamageType.Water, -0.5f },
            { DamageType.Electricity, 0.3f }, { DamageType.Ether, 0f }
        }
    },
    { EnemyElement.Bone, new Dictionary<DamageType, float>
        {
            { DamageType.Puncture, 0f }, { DamageType.Slash, 0f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0f },
            { DamageType.Fire, 0f }, { DamageType.Ice, 0f }, { DamageType.Poison, 0f }, { DamageType.Water, -0.5f },
            { DamageType.Electricity, 0f }, { DamageType.Ether, 0f }
        }
    },
    { EnemyElement.Metal, new Dictionary<DamageType, float>
        {
            { DamageType.Puncture, 0.25f }, { DamageType.Slash, -0.25f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0.5f },
            { DamageType.Fire, 0f }, { DamageType.Ice, -0.15f }, { DamageType.Poison, 0f }, { DamageType.Water, -0.5f },
            { DamageType.Electricity, 0.6f }, { DamageType.Ether, 0f }
        }
    },
    { EnemyElement.Spectral, new Dictionary<DamageType, float>
        {
            { DamageType.Puncture, 0f }, { DamageType.Slash, -0.5f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0.1f },
            { DamageType.Fire, 0.5f }, { DamageType.Ice, 0f }, { DamageType.Poison, 0f }, { DamageType.Water, -0.5f },
            { DamageType.Electricity, 0.25f }, { DamageType.Ether, 0f }
        }
    },
    { EnemyElement.Fire, new Dictionary<DamageType, float>
        {
            { DamageType.Puncture, 0f }, { DamageType.Slash, 0f }, { DamageType.Impact, 0f }, { DamageType.Explosive, -0.3f },
            { DamageType.Fire, -0.9f }, { DamageType.Ice, 0.25f }, { DamageType.Poison, -0.15f }, { DamageType.Water, 0.9f },
            { DamageType.Electricity, 0.25f }, { DamageType.Ether, 0f }
        }
    },
    { EnemyElement.Ice, new Dictionary<DamageType, float>
        {
            { DamageType.Puncture, 0f }, { DamageType.Slash, -0.1f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0.1f },
            { DamageType.Fire, 0.5f }, { DamageType.Ice, -0.9f }, { DamageType.Poison, 0f }, { DamageType.Water, -0.3f },
            { DamageType.Electricity, 0f }, { DamageType.Ether, 0f }
        }
    },
    { EnemyElement.Poison, new Dictionary<DamageType, float>
        {
            { DamageType.Puncture, 0f }, { DamageType.Slash, 0f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0f },
            { DamageType.Fire, 0.5f }, { DamageType.Ice, 0f }, { DamageType.Poison, -0.9f }, { DamageType.Water, 0f },
            { DamageType.Electricity, 0f }, { DamageType.Ether, 0f }
        }
    },
    { EnemyElement.Water, new Dictionary<DamageType, float>
        {
            { DamageType.Puncture, 0f }, { DamageType.Slash, 0f }, { DamageType.Impact, 0f }, { DamageType.Explosive, -0.4f },
            { DamageType.Fire, -0.5f }, { DamageType.Ice, 0.25f }, { DamageType.Poison, 0f }, { DamageType.Water, -0.9f },
            { DamageType.Electricity, 0.3f }, { DamageType.Ether, 0f }
        }
    },
    { EnemyElement.Electricity, new Dictionary<DamageType, float>
        {
            { DamageType.Puncture, 0f }, { DamageType.Slash, 0f }, { DamageType.Impact, 0f }, { DamageType.Explosive, 0f },
            { DamageType.Fire, 0.1f }, { DamageType.Ice, 0f }, { DamageType.Poison, 0f }, { DamageType.Water, 0f },
            { DamageType.Electricity, -0.9f }, { DamageType.Ether, 0f }
        }
    }
};

    // Maps DamageType to its corresponding status effect.
    private static readonly Dictionary<DamageType, StatusEffect> StatusEffectMap = new()
    {
        { DamageType.Puncture, StatusEffect.Punctured },
        { DamageType.Slash, StatusEffect.Bleeding },
        { DamageType.Explosive, StatusEffect.Exploded },
        { DamageType.Fire, StatusEffect.Burning },
        { DamageType.Ice, StatusEffect.Frozen },
        { DamageType.Poison, StatusEffect.Poisoned },
        { DamageType.Water, StatusEffect.Soaked },
        { DamageType.Electricity, StatusEffect.Electrocuted },
        { DamageType.Ether, StatusEffect.None }
    };

    #endregion


}

#region Enums

// Damage and status-element enums.
public enum DamageType
{
    Puncture, Slash, Impact, Explosive, Fire, Ice, Poison, Water, Electricity, Ether, None
}

//public enum DamageType { Slash, Impact, Explosive, Fire, Ice, Poison, Water, Electricity, Ether, Puncture }

/// <summary>
/// Determines whether a status resists or is vulnerable to a DamageType.
/// </summary>
public enum DamageOnStatus { resistant, vulnerable, none}

public enum EnemyElement
{
    Flesh, Bone, Metal, Spectral, Fire, Ice, Poison, Water, Electricity
}

public enum StatusEffect
{
    Punctured, Bleeding, Concussion, Exploded, Burning, Frozen, Poisoned, Soaked, Electrocuted, None
}
#endregion
