using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TINS.Campaign;

/// <summary>
/// Move toward the player, emit particles, and flip the sprite.
/// Simplified enemy AI without pathfinding.
/// </summary>
public class MoveTowardsPlayer : MonoBehaviour
{
    private Transform target;
    private Rigidbody2D rb2d;
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private  float minDistance = 0.09f;
    [SerializeField] private ParticleSystem particles;

    [SerializeField] private bool  isEnemy = true, hasparticles = true; //Also used for projectiles.
    [SerializeField] private bool hasSecondForm;

        private BossNextForm bossNextForm;

    private StatusEffect currentStatusEffect = StatusEffect.None;
    private StatusEffectOnEnemy statusEffectOnEnemy;
    private bool canMove = true;
    private bool pausedForTowerAttack;
    private EnemyHealth enemyHealth;
    private PurificationReveal purificationReveal;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        GameObject targetObject = GameObject.FindGameObjectWithTag("Player");
        target = isEnemy && CampaignMissionRuntime.Active != null ? CampaignMissionRuntime.Active.DefenseCore : targetObject?.transform;
        statusEffectOnEnemy = GetComponent<StatusEffectOnEnemy>();
        enemyHealth = GetComponent<EnemyHealth>();
        purificationReveal = CampaignMissionRuntime.Active?.TowerLight;

        if (hasSecondForm) bossNextForm = GetComponent<BossNextForm>();

    }
    private void OnEnable()
    {
        //Observe state changes and update the cached value.
        if (isEnemy && statusEffectOnEnemy != null)
        {
            currentStatusEffect = statusEffectOnEnemy.GetCurrentStatus();
            statusEffectOnEnemy.OnStatusChange += RefreshStatus;
        }

        if(hasparticles) particles.Play();//Emit a particle trail.

    }

    private void OnDisable()
    {
        if (statusEffectOnEnemy != null) statusEffectOnEnemy.OnStatusChange -= RefreshStatus;
        pausedForTowerAttack = false;
    }

    private void RefreshStatus() => currentStatusEffect = statusEffectOnEnemy.GetCurrentStatus();

    private void FixedUpdate()
    {
        if (CampaignCombatRules.IsCampaignMission && enemyHealth != null)
        {
            if (enemyHealth.IsCampaignRepelled) return;
            if (enemyHealth.IsCampaignFlashStunned)
            {
                if (rb2d != null) rb2d.linearVelocity = Vector2.zero;
                return;
            }
        }
        if (pausedForTowerAttack) return;
        if (target == null || rb2d == null || enemyData == null) return;
        if (CampaignCombatRules.IsCampaignMission && purificationReveal == null)
            purificationReveal = CampaignMissionRuntime.Active?.TowerLight;
        if (currentStatusEffect == StatusEffect.Frozen || canMove == false || currentStatusEffect == StatusEffect.Electrocuted) return;
        
            //Find direction
        Vector2 direction = (target.transform.position - rb2d.transform.position).normalized;
        //Use Vector2 to ignore Z distance.

        float walkSpeed = 0;

        bool isOnSecondForm = false;
        if(hasSecondForm)
        {
            if (bossNextForm.currentForm > 0) isOnSecondForm = true;
        }


        //Debug.Log("Target distance: " + Vector2.Distance(transform.position, target.position));
        if (Vector2.Distance(transform.position, target.position) < enemyData.minDistanceToIncreaseSpeed)
        {
            walkSpeed = enemyData.GetSpeed(isOnSecondForm);
        }
        else
        {
            walkSpeed = enemyData.GetSpeed(isOnSecondForm) * enemyData.speedMultiplierWhenNotVisible;
        }

        if (CampaignCombatRules.IsCampaignMission)
            walkSpeed *= CampaignCombatRules.EnemySpeedMultiplier;

        if(currentStatusEffect == StatusEffect.Concussion)
        {
            walkSpeed *= 0.2f;
        }

        if (CampaignCombatRules.IsCampaignMission && purificationReveal != null &&
            Vector2.Distance(transform.position, purificationReveal.transform.position) <= purificationReveal.Radius)
            walkSpeed *= purificationReveal.EnemySpeedMultiplierInsideLight;


        //Check if we need to follow object then do so 
        if (Vector3.Distance(target.transform.position, rb2d.transform.position) > minDistance)
        {
            rb2d.AddForce(direction * walkSpeed, ForceMode2D.Force);
        }

    }

    public Transform CurrentTarget => target;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }

    public void PauseForTowerAttack(bool paused) => pausedForTowerAttack = paused;

    //Animation-event adapters without parameters.
    public void CanMove()
    {
        canMove = true;
    }
    public void CannotMove()
    {
        canMove = false;
    }
}
