using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DamageOnRadius : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;

    [Header("Damage settings")]
    [SerializeField] private float radius;
    [SerializeField] private DamageData damageData;
    [Tooltip("Numero maximo de objetos/inimigos que vai atingir a cada vez")]
    [SerializeField] private int maxEffects = 99;
    [Tooltip("Ataque simples so causa o dano e mostra o texto com o numero de dano. Nao causa status, nem momemtum nem feedbacks")]
    [SerializeField] private bool simpleAttack = false;

    [Header("Auto play settings")]
    [SerializeField] private bool autoPlayOnEnable;
    [SerializeField] private float delayToStart = 0f;

    [Header("Damage rate settings")]
    [SerializeField] private DamageRate damageRate;
    [SerializeField] private float damageTickRate = 0f;

    [Space(10)]
    [Header("Animation")]
    [SerializeField] private bool triggerAnimator = false;
    [SerializeField] private string animatorTriggerName;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }


    private int effectsCount = 0;


    private void OnEnable()
    {
        effectsCount = 0;
        if(autoPlayOnEnable)
        {
            switch (damageRate)
            {
                case DamageRate.Once:
                    Invoke(nameof(TriggerDamage), delayToStart);
                    break;
                case DamageRate.Repeat:
                    InvokeRepeating(nameof(TriggerDamage), delayToStart, damageTickRate);
                    break;
                default:
                    break;
            }
        }
    }
    private void OnDisable()
    {
        CancelInvoke();
    }

    public void TriggerDamage()
    {
        effectsCount = 0;

        Vector3 epicenter = transform.position;
        Collider2D[] targetsHit = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);
        foreach (Collider2D target in targetsHit)
        {
            effectsCount++;
            if (effectsCount > maxEffects) return;

            if (target.CompareTag("Enemy"))
            {
                try
                {
                    var targetHealth = target.GetComponent<EnemyHealth>();
                    float damage = damageData.damage;
                    
                    //Critical damage.
                    float randomNumber_crit = Random.Range(0f, 1f);
                    if(randomNumber_crit < damageData.criticalChance)
                    {
                        damage *= damageData.criticalMultiplier;
                        if(simpleAttack)
                        {
                            target.GetComponent<HandleAttackOnEnemy>().SimpleAttack(Vector3.zero, damageData.statusDuration, damageData.statusDamagePerSec, damageData.statusChance, damageData.damageType, (damage, CriticalDamage.critical));
                        }
                        else
                        {
                            target.GetComponent<HandleAttackOnEnemy>().AttackEnemy(Vector3.zero, damageData.statusDuration, damageData.statusDamagePerSec,damageData.statusChance ,damageData.damageType, (damage, CriticalDamage.critical));
                        }
                    }
                    else
                    {
                        if (simpleAttack)
                        {
                            target.GetComponent<HandleAttackOnEnemy>().SimpleAttack(Vector3.zero, damageData.statusDuration, damageData.statusDamagePerSec, damageData.statusChance, damageData.damageType, (damage, CriticalDamage.no));
                        }
                        else
                        {
                            target.GetComponent<HandleAttackOnEnemy>().AttackEnemy(Vector3.zero, damageData.statusDuration, damageData.statusDamagePerSec, damageData.statusChance, damageData.damageType, (damage, CriticalDamage.no));
                        }
                    }
                    
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Erro ao aplicar dano no inimigo.\nException: " + e);
                }


            }
            else if (target.CompareTag("Player"))
            {

                target.GetComponent<HandleAttackOnPlayer>().DamagePlayer(1);

            }
            else if (target.CompareTag("Destructible"))
            {
                var breakScript = target.GetComponent<BreakOnTouch>();
                if (breakScript != null)
                {
                    breakScript.Break();
                }
            }

            if(triggerAnimator)
            {
                animator.SetTrigger(animatorTriggerName);
            }

        }
    }


    private void OnDrawGizmosSelected() //Visualize the attack hitbox.
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

public enum DamageRate
{
    Once,
    Repeat
}
