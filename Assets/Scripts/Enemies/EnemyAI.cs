using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using TINS.Campaign;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] protected float nextWaypointDistance = 1f;
    [SerializeField] protected EnemyData enemyData;
    [SerializeField] protected bool isNPC = false, onMenu = false;
    [SerializeField] protected float walkSpeed = 0;

    [Header("Melee protection movement")]
    [Tooltip("Movement speed multiplier after the player's melee hit breaks this enemy's protection.")]
    [SerializeField, Range(0.1f, 1f)] private float speedMultiplierAfterProtectionBreak = 0.5f;
    private bool protectionBroken;
    private bool pausedForTowerAttack;
    private float externalKnockbackTime;
    private float towerLightSpeedMultiplier = 1f;
    private EnemyHealth enemyHealth;
    private PurificationReveal purificationReveal;

    [SerializeField]  protected Transform target;//, playerTransform;
    protected Seeker seeker;
    protected Path path;
    protected Rigidbody2D myRigidbody;
    protected int currentWayPoint = 0;
    public Vector2 desiredDirection { get; protected set;}
    //bool reachedEndOfPath = false;

    protected StatusEffect currentStatusEffect = StatusEffect.None;
    protected StatusEffectOnEnemy statusEffectOnEnemy;

    //Some enemies operate outside A* regions and must disable pathfinding.
    [SerializeField] protected bool usesAStar = true;


    protected virtual void Awake()
    {
        seeker = GetComponent<Seeker>();
        myRigidbody = GetComponent<Rigidbody2D>();
        statusEffectOnEnemy = GetComponent<StatusEffectOnEnemy>();
        enemyHealth = GetComponent<EnemyHealth>();
        purificationReveal = FindFirstObjectByType<PurificationReveal>();

        InvokeRepeating(nameof(UpdatePath), 0, enemyData.updatePathInterval);
    }

    protected virtual void OnEnable()
    {
        protectionBroken = false;
        towerLightSpeedMultiplier = 1f;
        if (enemyHealth != null) enemyHealth.OnMeleeProtectionBroken += HandleMeleeProtectionBroken;
        if(onMenu) return; //Do nothing in menus.
        //Observe state changes and update the cached value.
        if (isNPC == false) statusEffectOnEnemy.OnStatusChange += () => currentStatusEffect = statusEffectOnEnemy.GetCurrentStatus(); 
    }

    protected virtual void OnDisable()
    {
        if (enemyHealth != null) enemyHealth.OnMeleeProtectionBroken -= HandleMeleeProtectionBroken;
    }

    private void HandleMeleeProtectionBroken()
    {
        protectionBroken = true;
    }

    protected virtual void Start()
    {
        if (isNPC == false)
            target = (CampaignMissionRuntime.Active != null ? CampaignMissionRuntime.Active.DefenseCore : GameObject.FindGameObjectWithTag("Player")?.transform);
        else
            target = transform;
        //playerTransform = target;
    }

    protected virtual void Update()
    { 
    
    }


    protected virtual void UpdatePath()
    {
        if (!CampaignCombatRules.IsCampaignMission && usesAStar && target != null && seeker != null)
        {
            seeker.StartPath(myRigidbody.position, target.position, OnPathComplete);
        }
        else
        {
            return;
        }


    }


    protected virtual void FixedUpdate()
    {
        UpdateTowerLightSpeedMultiplier();

        // Let external impulses move the enemy without A* immediately counteracting them.
        if (externalKnockbackTime > 0f)
        {
            externalKnockbackTime -= Time.fixedDeltaTime;
            return;
        }

        if (CampaignCombatRules.IsCampaignMission && enemyHealth != null && enemyHealth.IsCampaignFlashStunned)
        {
            if (myRigidbody != null && !enemyHealth.IsCampaignFlashRepelled) myRigidbody.linearVelocity = Vector2.zero;
            return;
        }

        if (pausedForTowerAttack)
        {
            if (myRigidbody != null && myRigidbody.bodyType != RigidbodyType2D.Static)
                myRigidbody.linearVelocity = Vector2.zero;
            return;
        }

        if (CampaignCombatRules.IsCampaignMission && !isNPC && !onMenu)
        {
            if (target == null || myRigidbody == null || currentStatusEffect == StatusEffect.Frozen ||
                currentStatusEffect == StatusEffect.Electrocuted) return;
            Vector2 offset = (Vector2)target.position - myRigidbody.position;
            if (offset.sqrMagnitude <= 0.64f) return;
            desiredDirection = offset.normalized;
            walkSpeed = GetWalkSpeed() * CampaignCombatRules.EnemySpeedMultiplier * towerLightSpeedMultiplier;
            if (currentStatusEffect == StatusEffect.Concussion) walkSpeed *= 0.2f;
            float mass = enemyData.ignoreMassWhenMoving ? myRigidbody.mass : 1f;
            myRigidbody.AddForce(desiredDirection * walkSpeed * Time.fixedDeltaTime * mass);
            return;
        }

        if (target == null || path == null) return;

        if (currentStatusEffect == StatusEffect.Frozen) return;

        if (isNPC && Vector3.Distance(target.position, transform.position) < 1) return; //Stop when the destination is reached.

        if (currentWayPoint >= path.vectorPath.Count) return; //The end of the path was reached.


        desiredDirection = GetDesiredDirection();

        walkSpeed = GetWalkSpeed();
        if (CampaignCombatRules.IsCampaignMission)
            walkSpeed *= CampaignCombatRules.EnemySpeedMultiplier;
        walkSpeed *= towerLightSpeedMultiplier;

        //Apply the concussion movement slowdown.
        if (currentStatusEffect == StatusEffect.Concussion)
        {
            walkSpeed *= 0.2f;
        }
        else if (currentStatusEffect == StatusEffect.Electrocuted)
        {
            walkSpeed = 0;
        }

        Vector2 force = desiredDirection * walkSpeed * Time.fixedDeltaTime;

        //Keep movement speed independent of Rigidbody mass.
        if (enemyData.ignoreMassWhenMoving)
        {
            force *= myRigidbody.mass;
        }

        myRigidbody.AddForce(force, ForceMode2D.Force);

        float distance = Vector2.Distance(myRigidbody.position, path.vectorPath[currentWayPoint]);

        if (distance < nextWaypointDistance)
        {
            currentWayPoint++;
        }

    }

    protected virtual Vector2 GetDesiredDirection()
    {
        return  ((Vector2)path.vectorPath[currentWayPoint] - myRigidbody.position).normalized;
    }

    protected virtual float GetWalkSpeed()
    {
        float speed;
        if (Vector2.Distance(transform.position, target.position) < enemyData.minDistanceToIncreaseSpeed)
        {
            speed = enemyData.GetSpeed();
        }
        else
        {
            speed = enemyData.GetSpeed() * enemyData.speedMultiplierWhenNotVisible;
        }

        return protectionBroken ? speed * speedMultiplierAfterProtectionBreak : speed;
    }

    private void UpdateTowerLightSpeedMultiplier()
    {
        if (purificationReveal == null)
            purificationReveal = FindFirstObjectByType<PurificationReveal>();

        if (purificationReveal == null)
        {
            towerLightSpeedMultiplier = 1f;
            return;
        }

        Vector2 offset = (Vector2)transform.position - (Vector2)purificationReveal.transform.position;
        towerLightSpeedMultiplier = offset.sqrMagnitude <= purificationReveal.Radius * purificationReveal.Radius
            ? purificationReveal.EnemySpeedMultiplierInsideLight
            : 1f;
    }

    protected virtual void OnPathComplete(Path path)
    {
        if(path.error == false)
        {
            this.path = path;
            currentWayPoint = 0;
        }
    }

    public Transform GetCurrentTargetTransform()
    {
        return target;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void PauseForTowerAttack(bool paused)
    {
        pausedForTowerAttack = paused;
        if (paused && myRigidbody != null && myRigidbody.bodyType != RigidbodyType2D.Static)
            myRigidbody.linearVelocity = Vector2.zero;
    }

    public void ApplyExternalKnockback(Vector2 impulse, float movementLockDuration)
    {
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        if (myRigidbody == null)
            return;

        externalKnockbackTime = Mathf.Max(externalKnockbackTime, movementLockDuration);
        myRigidbody.AddForce(impulse, ForceMode2D.Impulse);
    }

    protected void ChangePathUpdateTime(float targetTime)
    {
        CancelInvoke(nameof(UpdatePath));
        InvokeRepeating(nameof(UpdatePath), 0, targetTime);
    }
}
