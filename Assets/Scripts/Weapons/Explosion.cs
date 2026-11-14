using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using TechBooh;

//[RequireComponent(typeof(ManageExplosionCameraShake))]

public class Explosion : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] private float explosionRadius = 1f, impulseRadius = 5f;
    [SerializeField] private ExplosiveData explosiveData;
    [SerializeField] private bool enemyExplosion = false;

    [Space(10)]
    [Header("Visual settings")]
    [SerializeField] private MMF_Player feedbacks;
    [SerializeField] private bool hasSmoke = true, hasParticles = true, hasHoleOnTheGround = true;
    [SerializeField] private GameObject HoleOnGround;
    [SerializeField] private ParticleSystem particles, smoke;

   
    [Space(10)]
    [Header("Other settings")]
    [SerializeField] private bool rotateImpactOnTheFloor = false;
    [SerializeField] private bool autoExplodeOnEnable = false;
    [SerializeField] private float delayToExplode = 0;
    /// <summary>
    /// Percentage of delayToExplode added as random explosion delay.
    /// </summary>
    [SerializeField] [Range(0,1)] private float randomizeDelayBy = 0;
    [SerializeField] private bool pushPlayer = true;
    [Space(10)]
    [Header("Animation")]
    [SerializeField] private bool triggerAnimator = false;
    [SerializeField] private string animatorTriggerName;


    private Animator animator;
    private void Awake()
    {
        if (triggerAnimator) animator = Try.GetComponent<Animator>(gameObject);
    }

    private void OnEnable()
    {
        if(autoExplodeOnEnable)
        {
            TriggerExplosion(false);
        }

        if(feedbacks == null)
        {
            Debug.LogError($"Explosion: falta o MMF_Player no objeto: {gameObject.name}. " +
                $"Provavelmente não foi atualizado na troca do MMFeedbacks para o MMF_Player ");
        }
    }

    /// <summary>
    /// Apply area damage; call manually or enable autoExplodeOnEnable.
    /// </summary>
    /// <param name="forceExplosionNow">Skip explosion delay and timing randomization.</param>
    public void TriggerExplosion(bool forceExplosionNow = true)
    {
        if(forceExplosionNow)
        {
            Explode();
        }
        else
        {
            float delay = delayToExplode + Random.Range(-delayToExplode * randomizeDelayBy, delayToExplode * randomizeDelayBy);
            Invoke(nameof(Explode), delay);
        }

    }

    private void Explode()
    {

        if(triggerAnimator)
        {
            animator.SetTrigger(animatorTriggerName);
        }

        Vector3 epicenter = transform.position;
        Collider2D[] targetsHit = Physics2D.OverlapCircleAll(transform.position, impulseRadius, targetLayer);

        foreach (Collider2D target in targetsHit)
        {

            if (impulseRadius < explosionRadius)
            {
                Debug.Log("impulseRadius < explosionRadius. Deveria ser o contrário");
                impulseRadius = explosionRadius * 2;
            }

            float targetDistanceFromEpicenter = Mathf.Abs(Vector2.Distance(epicenter, target.transform.position));
            var impulseDirection = target.transform.position - epicenter;

            //Reduce force with distance from the explosion center.
            float forceMultiplier = Mathf.Abs((impulseRadius - targetDistanceFromEpicenter) / impulseRadius);

            if (target.CompareTag("Enemy"))
            {
                try
                {
                    //Reduce damage with distance from the center.
                    float damageMultiplier = Mathf.Abs((explosionRadius - targetDistanceFromEpicenter) / explosionRadius);

                    target.GetComponent<Rigidbody2D>().AddForce(impulseDirection * explosiveData.impulseStrength * forceMultiplier, ForceMode2D.Impulse);

                    //Damage only enemies inside the explosion radius.
                    if (targetDistanceFromEpicenter <= explosionRadius)
                    {
                        var targetHealth = target.GetComponent<EnemyHealth>();
                        float damage = Mathf.FloorToInt(explosiveData.damage * damageMultiplier);
                        
                        //Critical damage.
                        float randomNumber_crit = Random.Range(0f, 1f);
                        if (randomNumber_crit < explosiveData.criticalChance)
                        {
                            damage *= explosiveData.criticalMultiplier;
                            //Vector3 _momemtum, float statusDuration, float statusDamage, float statusChance, DamageType _damageType, (float damage, CriticalDamage critical) _damage
                            target.GetComponent<HandleAttackOnEnemy>().AttackEnemy(Vector3.zero, explosiveData.statusDuration, explosiveData.statusDamagePerSec, explosiveData.statusChance, explosiveData.damageType, (damage, CriticalDamage.critical));
                        }
                        else
                        {
                            target.GetComponent<HandleAttackOnEnemy>().AttackEnemy(Vector3.zero, explosiveData.statusDuration, explosiveData.statusDamagePerSec, explosiveData.statusChance, explosiveData.damageType, (damage, CriticalDamage.no));
                        }
                    }


                }
                catch (System.Exception e)
                {
                    Debug.LogError("Erro ao aplicar dano da explosão no inimigo.\nException: " + e);
                }


            }
            else if (target.CompareTag("Player"))
            {
                if(pushPlayer)
                {
                    //Enemy explosions use the normal player-hit radius.
                    if (enemyExplosion && targetDistanceFromEpicenter < impulseRadius)
                    {
                        target.GetComponent<Rigidbody2D>().AddForce(impulseDirection * explosiveData.impulseStrength * forceMultiplier * 0.2f, ForceMode2D.Impulse);
                    }
                    //Player explosions use a smaller self-knockback radius.
                    else if (targetDistanceFromEpicenter < impulseRadius / 2)
                    {
                        target.GetComponent<Rigidbody2D>().AddForce(impulseDirection * explosiveData.impulseStrength * forceMultiplier * 0.2f, ForceMode2D.Impulse);
                    }

                }

                if(explosiveData.damageOnPlayer > 0)
                {
                    //Enemy explosions use the normal player-hit radius.
                    if (enemyExplosion && targetDistanceFromEpicenter < explosionRadius)
                    {
                        target.GetComponent<HandleAttackOnPlayer>().DamagePlayer(explosiveData.damageOnPlayer);
                    }
                    //Player explosions use a smaller self-damage radius.
                    else if (targetDistanceFromEpicenter < explosionRadius / 3)
                    {
                        target.GetComponent<HandleAttackOnPlayer>().DamagePlayer(explosiveData.damageOnPlayer);
                    }
                    Debug.Log("dano de explosao no player");

                }
            }
            else if (target.CompareTag("Destructible"))
            {
                var breakScript = target.GetComponent<BreakOnTouch>();
                if (breakScript != null)
                {
                    breakScript.Break();
                }
            }

        }

        if (hasParticles) particles.Play();
        if (hasSmoke) smoke.Play();

        if (hasHoleOnTheGround)
        {
            Debug.LogWarning("TODO Instanciando HoleOnGround não utiliza pool");

            if (rotateImpactOnTheFloor) Instantiate(HoleOnGround, transform.position, transform.rotation);
            else Instantiate(HoleOnGround, transform.position, Quaternion.identity);
        }

        feedbacks.PlayFeedbacks();
    }

    public void SetExplosionRadius(float targetExplosionRadius)
    {
        explosionRadius = targetExplosionRadius;
    }

    public void SetImpulseRadius(float targetImpulseRadius)
    {
        impulseRadius = targetImpulseRadius;
    }

    public ExplosiveData GetExplosionData()
    {
        return explosiveData;
    }

    public void SetExplosiveData(ExplosiveData newExplosiveData)
    {
        explosiveData = newExplosiveData;
    }

    private void OnDrawGizmosSelected() //Visualize the attack hitbox.
    {
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.DrawWireSphere(transform.position, impulseRadius);
    }
}
