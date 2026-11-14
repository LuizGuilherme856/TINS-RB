using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MeleeStats;

/// <summary>
/// Base class for MeleeStats and BulletStats.
/// </summary>
public abstract class DamageObjectStats : MonoBehaviour
{
    public WeaponData weaponData;
    protected float damageMultiplier_powerUp = 1; //Used by the fivefold-damage power-up.

    public abstract (float damage, CriticalDamage critical) GetDamage();

    public  void SetDamageMultiplier_PowerUp(float damageMultiplier = 1)
    {
        this.damageMultiplier_powerUp = damageMultiplier;
    }

    public float GetKnockBackForce()
    {
        return weaponData.knockBackForce;
    }

    public virtual DamageType GetDamageType()
    {
        return weaponData.damageType;
    }


    public float GetStatusDuration()
    {
        return weaponData.statusDuration;
    }

    public float GetStatusChance()
    {
        return weaponData.statusChance;
    }

    public float GetStatusDamage()
    {
        return weaponData.statusDamagePerSec;
    }

    public float GetProjectileSize()
    {
        return weaponData.projectileSize;
    }
}

public enum CriticalDamage { no, critical, superCritical, instaKill }