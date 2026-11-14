using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack handler for enemies with multiple colliders.
/// For example, attach one to each separate part of a giant boss;
/// all handlers forward damage to the shared EnemyHealth.
/// </summary>
public class HandleAttackOnEnemy_MultipleParts : HandleAttackOnEnemy
{

    [SerializeField] private EnemyHealth newEnemyHealth;

    protected override void Awake()
    {
        floatinTextManager = FindFirstObjectByType<FloatingTextManager>();

        enemyHealth = newEnemyHealth;
        enemyData = newEnemyHealth.enemyData;
        statusEffectOnEnemy = GetComponent<StatusEffectOnEnemy>();
        rb = GetComponent<Rigidbody2D>();
    }


    protected override void DamageHealth(Vector3 _momemtum, DamageType _damageType, (float damage, CriticalDamage critical) _damage)
    {
        newEnemyHealth.SetDamageType(_damageType);
        newEnemyHealth.SetLastHitInfo(_momemtum, _damageType, false);
        newEnemyHealth.SetDamage(_damage.damage);
    }
}
