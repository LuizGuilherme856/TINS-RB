using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousDamageOnCollision : MonoBehaviour
{
    [SerializeField] private StatsCalculator statsCalculator;
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private WeaponData weaponData;
    private List<HandleAttackOnEnemy> handleAttackOnEnemyList = new List<HandleAttackOnEnemy>();

    private enum WeaponType { gun, melee}

    private void OnEnable()
    {
        switch (weaponType)
        {
            case WeaponType.gun:
                var gunData = (GunData)weaponData;
                InvokeRepeating(nameof(DamageOnArea), 0, gunData.fireRate);
                break;
            case WeaponType.melee:
                var meleeData = (MeleeData)weaponData;
                InvokeRepeating(nameof(DamageOnArea), 0, meleeData.attackRate);
                break;

        }
        
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void DamageOnArea()
    {
        if (handleAttackOnEnemyList.Count == 0) return;

        try
        {
            foreach (var enemy in handleAttackOnEnemyList)
            {
                (float, CriticalDamage) damage = (0, CriticalDamage.no);

                switch (weaponType)
                {
                    case WeaponType.gun:
                        damage = Damage2.GetDamageAndCritical(weaponData.GetDamage(), (GunData)weaponData, statsCalculator);
                        break;
                    case WeaponType.melee:
                        damage = Damage2.GetDamageAndCritical(weaponData.GetDamage(), (MeleeData)weaponData, statsCalculator);
                        break;

                }

                enemy.AttackEnemy(Vector3.zero, weaponData.statusDuration, weaponData.statusDamagePerSec, weaponData.statusChance, weaponData.damageType, damage);
            }

        }
        catch (System.Exception)
        {
            Debug.Log("Tem algo errado acontecendo ao aplicar o dano continuo. Verifique");
        }
        
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        var attackOnEnemy = collision.GetComponent<HandleAttackOnEnemy>();
        if (attackOnEnemy)
        {
            handleAttackOnEnemyList.Add(attackOnEnemy);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var enemyHealth = collision.GetComponent<HandleAttackOnEnemy>();
        if (enemyHealth)
        {
            handleAttackOnEnemyList.Remove(enemyHealth);
        }
    }

}
