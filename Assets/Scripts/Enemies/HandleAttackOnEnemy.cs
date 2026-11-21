using MoreMountains.Feedbacks;
using ND_VariaBULLET;
using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class HandleAttackOnEnemy : MonoBehaviour
{
    //[SerializeField] MMFeedbacks hitFeedback;
    [SerializeField] MMF_Player hitFeedback;

    protected FloatingTextManager floatinTextManager;

    [SerializeField] protected bool moveParticleSystemToImpact = false;
    [SerializeField] protected GameObject particleSystemGameObj;

    protected EnemyHealth enemyHealth;
    protected Rigidbody2D rb;
    protected Collider2D enemyCollider;
    protected StatusEffectOnEnemy statusEffectOnEnemy;
    protected EnemyData enemyData;

    [Header("TechBooh - Knockback Safety")]
    [SerializeField] protected float wallDetectionPadding_TechBooh = 0.08f;
    [SerializeField] protected float minimumWallDetectionDistance_TechBooh = 0.3f;
    [SerializeField] protected float maximumWallDetectionDistance_TechBooh = 1.5f;
    [SerializeField] protected float knockbackWallProtectionDuration_TechBooh = 0.2f;
    [SerializeField][Range(0.1f, 1f)] protected float enemyHitKnockbackMultiplier_TechBooh = 0.85f;

    protected readonly RaycastHit2D[] knockbackCastHits_TechBooh = new RaycastHit2D[8];
    protected ContactFilter2D knockbackContactFilter_TechBooh;
    protected float knockbackWallProtectionTimer_TechBooh = 0f;

    public event Action OnTakeMeleeDamage, OnTakeMeleeOrbitalDamage;


    protected virtual void Awake()
    {
        floatinTextManager = FindFirstObjectByType<FloatingTextManager>();

        if(enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
        enemyData = enemyHealth.enemyData;
        statusEffectOnEnemy = GetComponent<StatusEffectOnEnemy>();
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();

        knockbackContactFilter_TechBooh = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = false
        };
    }

    protected virtual void FixedUpdate()
    {
        ProtectKnockbackAgainstWallsOnFixedUpdate_TechBooh();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (enemyHealth.invulnerableTimer > 0 && !enemyHealth.IsCampaignFlashStunned) return;

        GameObject collisionGameObject = collision.gameObject;
        if (collisionGameObject.CompareTag("Bullet"))
        {
            var _momemtum = collisionGameObject.GetComponent<Projectile>().GetMomentumBeforeCollision();
            var _bulletStats = collisionGameObject.GetComponent<DamageObjectStats>();
            var _damageType = _bulletStats.GetDamageType();
            var _damage = _bulletStats.GetDamage();
            //Ignore instant-kill damage on enemies immune to it.
            if (_damage.critical == CriticalDamage.instaKill && enemyData.immuneToInstaKill) return;
            
            AttackEnemy(collisionGameObject, _momemtum, _bulletStats, _damageType, _damage);

        }

    }

   
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemyHealth.invulnerableTimer > 0 && !enemyHealth.IsCampaignFlashStunned) return;

        if (collision.gameObject.CompareTag("Melee") || collision.gameObject.CompareTag("Bullet"))
        {
            if (collision.gameObject.CompareTag("Melee")) 
            {
                if (collision.gameObject.GetComponent<OrbitalUpgradeHolder>() != null)
                {
                    OnTakeMeleeOrbitalDamage?.Invoke();
                }
                else
                {
                    OnTakeMeleeDamage?.Invoke();
                }
            }
            //Read the damage parameters.
            var _meleeObject = collision.gameObject;
            var _damageObjStats = collision.gameObject.GetComponent<DamageObjectStats>();
            var _damage = _damageObjStats.GetDamage();

            //Ignore instant-kill damage on enemies immune to it.
            if (_damage.critical == CriticalDamage.instaKill && enemyData.immuneToInstaKill) return;

            var _damageType = _damageObjStats.GetDamageType();
            var _momemtum = _meleeObject.GetComponent<Projectile>().GetMomentumBeforeCollision();

            AttackEnemy(_meleeObject, _momemtum, _damageObjStats, _damageType, _damage);

        }
    }

    public void AttackEnemy(GameObject collisionGameObject, Vector3 _momemtum, DamageObjectStats _bulletStats, DamageType _damageType, (float damage, CriticalDamage critical) _damage)
    {
        if (!enemyHealth.IsCampaignFlashStunned && IsImmuneToTheDamage(_damageType)) return;

        ApplyDamage(_momemtum, _damageType, _damage);
        ApplyMomentum(collisionGameObject, _bulletStats);
        ApplyStatusEffect(_bulletStats.GetStatusDuration(), _bulletStats.GetStatusDamage(), _bulletStats.GetStatusChance(), _damageType);
    }


    /// <summary>
    /// Damage an enemy without a projectile.
    /// </summary>
    public void AttackEnemy(Vector3 _momemtum, float statusDuration, float statusDamage, float statusChance, DamageType _damageType,
        (float damage, CriticalDamage critical) _damage)
    {
        ApplyDamage(_momemtum, _damageType, _damage);
        ApplyStatusEffect(statusDuration, statusDamage, statusChance, _damageType);
    }

    public void AttackEnemy_SimpleVersion( DamageType _damageType, float damage)
    {
        AttackEnemy(Vector3.zero, 0, 0, 0, _damageType, ( damage, CriticalDamage.no));
    }

    public void InstaKill(DamageType _damageType)
    {
        AttackEnemy(Vector3.zero, 0, 0, 0, _damageType, (99999, CriticalDamage.instaKill));
    }
    /// <summary>
    /// Lightweight AttackEnemy variant without status effects, momentum, or feedbacks.
    /// </summary>
    public void SimpleAttack(Vector3 _momemtum, float statusDuration, float statusDamage, float statusChance, DamageType _damageType,
        (float damage, CriticalDamage critical) _damage)
    {
        ApplyDamage(_momemtum, _damageType, _damage, false);
    }

    public void ApplyStatusEffect(float statusDuration, float statusDamage, float statusChance, DamageType _damageType)
    {
        if (statusEffectOnEnemy == null) return;

        // Reactions can still apply independently of the weapon's base status chance.
        var newStatus = Damage2.StatusEffectInteraction(_damageType, statusEffectOnEnemy.GetCurrentStatus());

        if (newStatus != StatusEffect.None)
        {
            statusEffectOnEnemy.SetStatus(newStatus, statusDuration, statusDamage);
        }
        else
        {
            StatusEffect status = Damage2.GetStatusEffect(_damageType);
            if (status == StatusEffect.None || statusChance <= 0f) return;

            statusChance += statusChance * Damage2.GetStatusChance(enemyData.enemyElement, _damageType);
            if (statusChance > 0f && Random.Range(0f, 1f) <= statusChance)
                statusEffectOnEnemy.SetStatus(status, statusDuration, statusDamage);

        }
    }

    public EnemyHealth SlashTarget => enemyHealth;

    public bool TrySlashHit(MeleeData weapon, SlashStep step, Vector2 direction, float multiplier, bool finisher)
    {
        if (!enemyHealth.isActiveAndEnabled || enemyHealth.currentHealth <= 0f ||
            (enemyHealth.invulnerableTimer > 0f && !enemyHealth.IsCampaignFlashStunned) || enemyHealth.IsProtectedUntilPlayerMelee ||
            IsImmuneToTheDamage(weapon.damageType)) return false;

        var damage = MeleeStats.RollDamage(weapon, step.damage * multiplier);
        if (enemyData.immuneToInstaKill && damage.critical == CriticalDamage.instaKill) return false;

        float before = enemyHealth.currentHealth;
        Vector2 impulse = direction * (weapon.knockBackForce * step.push * enemyData.pushDamping);
        bool resistsStagger = enemyData.bossOrElite || enemyHealth is BossHealth;
        if (resistsStagger) impulse *= 0.15f;
        impulse = ClampKnockbackAgainstWalls_TechBooh(impulse, weapon.projectileSize);
        ApplyDamage(impulse, weapon.damageType, damage, comboFinisher: finisher);
        if (enemyHealth.currentHealth >= before) return false;

        OnTakeMeleeDamage?.Invoke();
        if (enemyHealth.currentHealth <= 0f) return true;
        ApplyStatusEffect(weapon.statusDuration, weapon.statusDamagePerSec, weapon.statusChance, weapon.damageType);
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            if (!resistsStagger && TryGetComponent(out EnemyAI ai))
                ai.ApplyExternalKnockback(impulse, finisher ? 0.28f : 0.12f);
            else
                rb.AddForce(impulse, ForceMode2D.Impulse);
            knockbackWallProtectionTimer_TechBooh = knockbackWallProtectionDuration_TechBooh;
        }
        return true;
    }

    private void ApplyMomentum(GameObject collisionGameObject, DamageObjectStats _bulletStats)
    {
        var _bulletRigidBody = collisionGameObject.GetComponent<Rigidbody2D>();
        if (moveParticleSystemToImpact) particleSystemGameObj.transform.position = collisionGameObject.transform.position;
        
        //Calculate projectile knockback on the enemy.
        Vector2 forceDirection = collisionGameObject.GetComponent<Projectile>().GetVelocityBeforeCollision();
        forceDirection *= _bulletStats.GetKnockBackForce() * _bulletRigidBody.mass * _bulletStats.GetProjectileSize();
        Vector2 finalImpulse = forceDirection * enemyData.pushDamping;
        finalImpulse = ClampKnockbackAgainstWalls_TechBooh(finalImpulse, _bulletStats.GetProjectileSize());

        if (finalImpulse.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        rb.AddForce(finalImpulse, ForceMode2D.Impulse);
        knockbackWallProtectionTimer_TechBooh = knockbackWallProtectionDuration_TechBooh;
    }

    /// <summary>
    /// TechBooh:
    /// Strong knockback could push enemies through walls when physics
    /// advanced too far in one frame. Cast briefly in the push direction
    /// and reduce or cancel knockback when a solid obstacle is ahead.
    /// </summary>
    protected Vector2 ClampKnockbackAgainstWalls_TechBooh(Vector2 targetImpulse, float projectileSize)
    {
        if (enemyCollider == null || targetImpulse.sqrMagnitude <= 0.0001f)
        {
            return targetImpulse;
        }

        Vector2 knockbackDirection = targetImpulse.normalized;
        float detectionDistance = Mathf.Clamp(
            targetImpulse.magnitude * 0.02f + projectileSize * 0.25f,
            minimumWallDetectionDistance_TechBooh,
            maximumWallDetectionDistance_TechBooh);

        int hitCount = enemyCollider.Cast(
            knockbackDirection,
            knockbackContactFilter_TechBooh,
            knockbackCastHits_TechBooh,
            detectionDistance);

        if (hitCount <= 0)
        {
            return targetImpulse;
        }

        float closestHitDistance = float.MaxValue;
        int enemyHitsInPath = 0;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = knockbackCastHits_TechBooh[i];
            if (hit.collider == null || hit.collider.isTrigger)
            {
                continue;
            }

            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform) || transform.IsChildOf(hit.collider.transform))
            {
                continue;
            }

            // TechBooh:
            // Other enemies must not block this check because that would break
            // existing chain knockback; only solid level geometry should stop it.
            if (hit.collider.GetComponentInParent<EnemyHealth>() != null)
            {
                enemyHitsInPath++;
                continue;
            }

            if (hit.distance < closestHitDistance)
            {
                closestHitDistance = hit.distance;
            }
        }

        targetImpulse = ApplyEnemyKnockbackAttenuation_TechBooh(targetImpulse, enemyHitsInPath);

        if (closestHitDistance == float.MaxValue)
        {
            return targetImpulse;
        }

        float allowedDistance = Mathf.Max(0f, closestHitDistance - wallDetectionPadding_TechBooh);
        if (allowedDistance <= 0.01f)
        {
            return Vector2.zero;
        }

        float allowedRatio = Mathf.Clamp01(allowedDistance / detectionDistance);
        return targetImpulse * allowedRatio;
    }

    /// <summary>
    /// TechBooh:
    /// Preserve chain knockback with gradually reduced force.
    /// Each enemy in the knockback direction applies another multiplier.
    /// </summary>
    protected Vector2 ApplyEnemyKnockbackAttenuation_TechBooh(Vector2 targetImpulse, int enemyHitsInPath)
    {
        if (enemyHitsInPath <= 0)
        {
            return targetImpulse;
        }

        float attenuationMultiplier = Mathf.Pow(enemyHitKnockbackMultiplier_TechBooh, enemyHitsInPath);
        return targetImpulse * attenuationMultiplier;
    }

    /// <summary>
    /// TechBooh:
    /// Even reduced initial impulses can produce velocity high enough
    /// to tunnel through a wall. Briefly after knockback,
    /// cast along current velocity and stop before the obstacle.
    /// </summary>
    protected void ProtectKnockbackAgainstWallsOnFixedUpdate_TechBooh()
    {
        if (enemyCollider == null || rb == null)
        {
            return;
        }

        if (knockbackWallProtectionTimer_TechBooh <= 0f)
        {
            return;
        }

        knockbackWallProtectionTimer_TechBooh -= Time.fixedDeltaTime;

        Vector2 currentVelocity = rb.linearVelocity;
        if (currentVelocity.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector2 moveDirection = currentVelocity.normalized;
        float castDistance = currentVelocity.magnitude * Time.fixedDeltaTime + wallDetectionPadding_TechBooh;
        castDistance = Mathf.Clamp(castDistance, wallDetectionPadding_TechBooh, maximumWallDetectionDistance_TechBooh * 2f);

        int hitCount = enemyCollider.Cast(
            moveDirection,
            knockbackContactFilter_TechBooh,
            knockbackCastHits_TechBooh,
            castDistance);

        if (hitCount <= 0)
        {
            return;
        }

        float closestHitDistance = float.MaxValue;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = knockbackCastHits_TechBooh[i];
            if (hit.collider == null || hit.collider.isTrigger)
            {
                continue;
            }

            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform) || transform.IsChildOf(hit.collider.transform))
            {
                continue;
            }

            // TechBooh:
            // Preserve enemy-to-enemy collision and pushing; stop movement only
            // when a wall or other level obstacle is ahead.
            if (hit.collider.GetComponentInParent<EnemyHealth>() != null)
            {
                continue;
            }

            if (hit.distance < closestHitDistance)
            {
                closestHitDistance = hit.distance;
            }
        }

        if (closestHitDistance == float.MaxValue)
        {
            return;
        }

        float safeDistance = Mathf.Max(0f, closestHitDistance - wallDetectionPadding_TechBooh);
        Vector2 safePosition = rb.position + moveDirection * safeDistance;

        rb.position = safePosition;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        knockbackWallProtectionTimer_TechBooh = 0f;
    }

    private void ApplyDamage(Vector3 _momemtum, DamageType _damageType, (float damage, CriticalDamage critical) _damage, bool triggerHitFeedback = true, bool? comboFinisher = null)
    {
        //Calculate damage using the enemy's elemental resistance to this damage type.
        float damageCalculation = _damage.damage + _damage.damage * Damage2.GetResistance(enemyData.enemyElement, _damageType);

        //Modify damage based on the enemy's active status effect.
        var damageOnStatus = Damage2.GetDamageOnStatus(_damageType, statusEffectOnEnemy.GetCurrentStatus());
        if (damageOnStatus == DamageOnStatus.resistant)
        {
            damageCalculation /= 3;
        }
        else if (damageOnStatus == DamageOnStatus.vulnerable)
        {
            damageCalculation *= 3;
        }

        _damage = (damageCalculation, _damage.critical);

        if (comboFinisher.HasValue)
        {
            enemyHealth.SetLastHitInfo(_momemtum, _damageType, true);
            enemyHealth.ApplyHealthDamage(_damage.damage, comboFinisher.Value);
        }
        else DamageHealth(_momemtum, _damageType, _damage);


        //TriggerAttackEnemyFeedback(_damage.damage, _damageType, _damage.critical, damageOnStatus);

        floatinTextManager?.TriggerHitFeedback(_damage.damage, transform.position, _damageType, _damage.critical, damageOnStatus);

        if (triggerHitFeedback) hitFeedback?.PlayFeedbacks();
    }

    /// <summary>
    /// Apply the resulting health damage.
    /// </summary>
    protected virtual void DamageHealth(Vector3 _momemtum, DamageType _damageType, (float damage, CriticalDamage critical) _damage)
    {
        enemyHealth.SetDamageType(_damageType);
        enemyHealth.SetLastHitInfo(_momemtum, _damageType, false);
        enemyHealth.SetDamage(_damage.damage);
    }

    private bool IsImmuneToTheDamage(DamageType _damageType)
    {
        if (_damageType == DamageType.Slash && enemyData.immuneToSlash)
        {
            return true;
        }
        else if (_damageType == DamageType.Impact && enemyData.immuneToImpact)
        {
            return true;
        }
        else if (_damageType == DamageType.Poison && enemyData.immuneToPoison)
        {
            return true;
        }
        else if (_damageType == DamageType.Explosive && enemyData.immuneToExplosion)
        {
            return true;
        }
        else if (_damageType == DamageType.Ice && enemyData.immuneToIce)
        {
            return true;
        }
        else if (_damageType == DamageType.Fire && enemyData.immuneToFire)
        {
            return true;
        }
        else
        {
            return false;
        }
    }




    public void TriggerAttackEnemyFeedback(float damage, DamageType damageType, CriticalDamage critical, DamageOnStatus damageOnStatus)
    {

        floatinTextManager?.TriggerHitFeedback(damage, transform.position, damageType, critical, damageOnStatus);
        hitFeedback.PlayFeedbacks();
    }



}
