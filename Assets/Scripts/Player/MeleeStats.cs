///Duplicates logic from BulletStats.cs; consider sharing it.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using Random = UnityEngine.Random;

public class MeleeStats : DamageObjectStats
{

    private bool hasHitAnEnemy;

    public event Action OnHitEnemy;

    private void OnEnable()
    {
        hasHitAnEnemy = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy") && hasHitAnEnemy == false)
        {
            hasHitAnEnemy = true;
            OnHitEnemy?.Invoke();
        }
    }

    public override (float damage, CriticalDamage critical) GetDamage()
    {
        return RollDamage(weaponData, damageMultiplier_powerUp);
    }

    public static (float damage, CriticalDamage critical) RollDamage(WeaponData weaponData, float multiplier)
    {
        if (Random.Range(0.01f, 100f) < weaponData.instaKillChance)
        {
            return ( float.MaxValue, CriticalDamage.instaKill);
        }

        if (Random.Range(0, 100f) <= weaponData.criticalChance)
        {
            // Debug.Log("Critical damage");
            return (weaponData.GetDamage() * weaponData.criticalMultiplier * multiplier, CriticalDamage.critical);
        }
        else
        {
            return (weaponData.GetDamage() * multiplier, CriticalDamage.no);
        }
    }



}
