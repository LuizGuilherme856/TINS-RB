using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HealerBellBossAI : BellBossAI
{
    //Manager Related
    [SerializeField] protected BellsManager bellManager;


    public enum HealerBellStates { idle, seeking, alert, stun, heal, tp, ring }
    [SerializeField] protected HealerBellStates currentState = HealerBellStates.seeking;
    //Select the next attack and prepare the state machine.
    [SerializeField] protected HealerBellStates nextAttackState = HealerBellStates.idle;
    protected Queue<HealerBellStates> lastAttackStates = new Queue<HealerBellStates>(3);
    //[SerializeField] protected int maxAttackWithoutIdle = 2; //Moved to the boss data.
    protected int attacksSinceLastIdle = 0;
    //Data Related
    protected HealerBellBossData healerBellData;

    protected float localSpeedFormMod = 1.0f;


    //Maps each boss phase to its available attacks.
    Dictionary<int, (int, HealerBellStates)[]> unlockedAttacks = new Dictionary<int, (int, HealerBellStates)[]>();

    //Timer Related
    [Header("Timers")]
    [SerializeField] protected float timeInIdle = 0f;
    [SerializeField] protected float timeInSeeking = 0f;
    [SerializeField] protected float timeInTp = 0f;
    [SerializeField] protected float timeInRing = 0f;
    [SerializeField] protected float timeInStun = 0f;
    [SerializeField] protected float timeInHeal = 0f;
    [SerializeField] protected float timeInAlert = 0f;

    [SerializeField] protected float maxAtThisState = 0f;

    protected bool firstTimeEnteringThisState = true;  

    //Targeting Related
    [SerializeField] protected Transform playerTransform;
    [SerializeField] protected Transform currentTargetTransform;
    [SerializeField] protected bool targetingPlayer = false;

    //Events
    public event Action OnChangeState;

    //Animator
    [SerializeField] protected HealerBellMovementAnimations BellAnim;

    //Health Related
    [SerializeField] protected EnemyHealth BellHealth;


    //Not Varia Attacking and Tp related
    [SerializeField] protected AttackPlayer attackPlayer; // Assigned in the Editor.
    // 0 = AP1, 1 = AP2
    [SerializeField] protected int lastAttackPlayerUsed = 1;


    //Ring Related
    [SerializeField] protected Transform RingLocation;
    protected MoveObjectToPos moveObjToPosScript;
    protected bool needsToRing = false;



    //Stun Related
    protected HandleAttackOnEnemy handleAttackOnBell;
    protected float timeOfLastStun = 0;

    //Tp Related
    [SerializeField] protected List<GameObject>  tpPositions; //Assigned in the Editor.
    [SerializeField] protected float minimumTeleportDistanceFromPlayer_TechBooh = 2.5f;
    [Header("TechBooh - Teleport Failsafe")]
    [SerializeField] protected float repeatedTeleportWindow_TechBooh = 6f;
    [SerializeField] protected int repeatedTeleportLimitBeforeReset_TechBooh = 3;
    [SerializeField] protected bool forceReturnToArenaCenterOnRepeatedTeleport_TechBooh = true;

    protected float repeatedTeleportWindowTimer_TechBooh = 0f;
    protected int repeatedTeleportCount_TechBooh = 0;


    


    protected override void Awake()
    {

        base.Awake();
        try
        {
            healerBellData = (HealerBellBossData)enemyData;
        }
        catch (Exception)
        {
            Debug.LogError("enemyData não é uma instancia de HealerBellBossData! Coloque um HealerBellBossData na referencia 'Enemy Data'");
        }
        try
        {
            bellManager = GetComponentInParent<BellsManager>();
            bellManager.OnTimeToRing += NeedsToRing;
            bellManager.OnChangeForm += ChangedForm;
        }
        catch (Exception)
        {
            Debug.LogError("Pai na hierarquia de HealerBell não tem um bellManager");
        }
        try
        {
            BellHealth = this.gameObject.GetComponent<EnemyHealth>();
            BellHealth.OnZeroHealth += ResetVariables;
            BellHealth.OnZeroHealth += bellManager.BellDeath;
        }
        catch (Exception)
        {
            Debug.LogError("HealerBell não possui um script EnemyHealth");
        }


        try
        {
            attackPlayer = GetComponent<AttackPlayer>();
        }
        catch (System.Exception)
        {
            Debug.LogError("HealerBell  não tem um AttackPlayer.cs");
        }

        try
        {
            BellAnim = GetComponent<HealerBellMovementAnimations>();
        }
        catch (System.Exception)
        {
            Debug.LogError("HealerBell  não tem um HealerBellMovementAnimations.cs");
        }

        try
        {
            moveObjToPosScript = GetComponent<MoveObjectToPos>();
        }
        catch (System.Exception)
        {
            Debug.LogError("HealerBell não tem MoveObjectToPos");
        }

        try
        {
            handleAttackOnBell = GetComponent<HandleAttackOnEnemy>();
            handleAttackOnBell.OnTakeMeleeDamage += TookMeleeDamage;
        }
        catch (System.Exception)
        {
            Debug.LogError("Bell não tem HandleAttackOnEnemy");
        }


        bellManager.OnChangeForm += ChangedForm;
        ConfigureArenaRecoveryPosition_TechBooh(RingLocation);



    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //Return to seeking and reset timers.
        currentState = HealerBellStates.idle;
        ResetVariables();
        repeatedTeleportWindowTimer_TechBooh = 0f;
        repeatedTeleportCount_TechBooh = 0;
        OnChangeState?.Invoke();
        base.OnEnable();
    }

    protected override void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (isNPC == false)
        {
            //currentTargetTransform.position = playerTransform.position;
            //target = currentTargetTransform;
            target = playerTransform;

        }
        else
        {
            target = transform;
        }

    }

    protected override void Update()
    {
        base.Update();

        switch (currentState)
        {
            case HealerBellStates.idle:
                timeInIdle += Time.deltaTime;
                break;

            case HealerBellStates.seeking:
                timeInSeeking += Time.deltaTime;
                break;

            case HealerBellStates.ring:
                timeInRing += Time.deltaTime;
                break;

            case HealerBellStates.stun:
                timeInStun += Time.deltaTime;
                break;

            case HealerBellStates.tp:
                timeInTp += Time.deltaTime;
                break;

            case HealerBellStates.heal:
                timeInHeal += Time.deltaTime;
                break;
            case HealerBellStates.alert:
                timeInAlert += Time.deltaTime;
                break;
            default:
                break;

        }
    }

    // Start is called before the first frame update
    protected override void FixedUpdate()
    {
        if (currentStatusEffect == StatusEffect.Frozen) return;

        UpdateRepeatedTeleportWindow_TechBooh();

        switch (currentState)
        {
            case HealerBellStates.idle:
                HandleIdleState();
                break;

            case HealerBellStates.seeking:
                HandleSeekingState();
                break;

            case HealerBellStates.ring:
                HandleRingState();
                break;

            case HealerBellStates.stun:
                HandleStunState();
                break;

            case HealerBellStates.tp:
                HandleTpState();
                break;

            case HealerBellStates.heal:
                HandleHealState();
                break;

            case HealerBellStates.alert:
                HandleAlertState();
                break;

            default:
                break;
        }

        bool shouldMonitorConfinement = currentState == HealerBellStates.seeking &&
            target != null &&
            Vector2.Distance(transform.position, target.position) > GetNextAttackDistance(nextAttackState) + 0.5f;

        if (RecoverIfConfinedNearObstacle_TechBooh(shouldMonitorConfinement))
        {
            ResetVariables();
            currentState = HealerBellStates.idle;
            OnChangeState?.Invoke();
            return;
        }

        base.FixedUpdate();
    }

    private void HandleAlertState()
    {
        if (timeInAlert <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = GetNextAttackAlertTime(nextAttackState);


            //Read the state's range for the current form and the requested upper or lower limit.
            firstTimeEnteringThisState = false;
        }
        if (timeInAlert >= maxAtThisState && timeInAlert >= healerBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInAlert = 0;
            //Choose the next attack now to determine the required distance.

            currentState = nextAttackState;
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
        }
    }

    private void HandleHealState()
    {
        //Return to idle if healing is unavailable.
            
        if (timeInHeal <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = Random.Range(healerBellData.heal.RangesPerForm[currentLocalForm].Ranges[0],
                healerBellData.heal.RangesPerForm[currentLocalForm].Ranges[1]);


            //Read the state's range for the current form and the requested upper or lower limit.
            firstTimeEnteringThisState = false;
        }

        if ((timeInHeal >= maxAtThisState && timeInHeal >= healerBellData.minimumTimeOnState )) 
        {

            firstTimeEnteringThisState = true;
            timeInHeal = 0;
            ChangePathUpdateTime(healerBellData.updatePathInterval);
            //Choose the next attack now to determine the required distance.
            NextNonAttackState();

            bossScript.DeactivateAttackByIndex(1);
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
        }
    }

    private void HandleTpState()
    {
        if (timeInTp <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = Random.Range(healerBellData.tp.RangesPerForm[currentLocalForm].Ranges[0],
                healerBellData.tp.RangesPerForm[currentLocalForm].Ranges[1]);

            //Read the state's range for the current form and the requested upper or lower limit.
            firstTimeEnteringThisState = false;
        }
        if (timeInTp >= maxAtThisState && timeInTp >= healerBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInTp = 0;

            //Choose the next attack now to determine the required distance.


            NextNonAttackState();

            //Set the Animator to the new state.
            OnChangeState?.Invoke();
        }
    }

    private void HandleStunState()
    {
        if (timeInStun <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = Random.Range(healerBellData.stun.RangesPerForm[currentLocalForm].Ranges[0],
                healerBellData.stun.RangesPerForm[currentLocalForm].Ranges[1]);

            firstTimeEnteringThisState = false;
        }
        if (timeInStun >= maxAtThisState && timeInStun >= healerBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInStun = 0;

            currentState = HealerBellStates.idle;

            OnChangeState?.Invoke();

        }
    }

    private void HandleRingState()
    {
        if (timeInRing <= 0 || firstTimeEnteringThisState)
        {
            needsToRing = false;
            //Read the state's range for the current form and the requested upper or lower limit.
            maxAtThisState = Random.Range(healerBellData.ring.RangesPerForm[currentLocalForm].Ranges[0],
                    healerBellData.ring.RangesPerForm[currentLocalForm].Ranges[1]);



            firstTimeEnteringThisState = false;
        }
        if (timeInRing >= maxAtThisState && timeInRing >= healerBellData.minimumTimeOnState)
        {
            //Deactivate the ring attack.
            bossScript.DeactivateAllAttacks();

            firstTimeEnteringThisState = true;
            timeInRing = 0;
            BellAnim.lockDirection = false;
            //Choose the next attack now to determine the required distance.
            attacksSinceLastIdle = 0;
            currentState = HealerBellStates.idle;
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
        }
    }

    private void HandleSeekingState()
    {
        if (firstTimeEnteringThisState)
        {
            if (nextAttackState == HealerBellStates.ring)
            {
                //Target the arena center during the spawner ring attack.
                target = RingLocation;
                UpdatePath();
            }
            else if (nextAttackState == HealerBellStates.heal || nextAttackState == HealerBellStates.idle)
            {
                target = bellManager.GetTargetBell().transform;
            }
            firstTimeEnteringThisState = false;
        }
        bool distAtualMenorDistTarget = Math.Abs(Vector2.Distance(transform.position, target.position)) < GetNextAttackDistance(nextAttackState);
        if (distAtualMenorDistTarget && timeInSeeking >= healerBellData.minimumTimeOnState)
        {
            firstTimeEnteringThisState = true;
            timeInSeeking = 0;
            currentState = HealerBellStates.alert;

            
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
        }
    }

    private void HandleIdleState()
    {
        if (timeInIdle <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = Random.Range(healerBellData.idle.RangesPerForm[currentLocalForm].Ranges[0],
                healerBellData.idle.RangesPerForm[currentLocalForm].Ranges[1]);


            //Read the state's range for the current form and the requested upper or lower limit.
            firstTimeEnteringThisState = false;
        }
        if (timeInIdle >= maxAtThisState && timeInIdle >= healerBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInIdle = 0;
            //Choose the next attack now to determine the required distance.
            if (!needsToRing)
            {
                nextAttackState = GetNextAttack();
            }
            else
            {
                nextAttackState = HealerBellStates.ring;
            }

            currentState = HealerBellStates.seeking;
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
        }
    }


    protected override Vector2 GetDesiredDirection()
    {
        if (currentState == HealerBellStates.seeking)
        {
            return ((Vector2)path.vectorPath[currentWayPoint] - myRigidbody.position).normalized;


        }
        else
        {
            return ((Vector2)target.position - myRigidbody.position).normalized;
        }
    }

    protected override float GetWalkSpeed()
    {
        if (Vector2.Distance(transform.position, target.position) < enemyData.minDistanceToIncreaseSpeed)
        {
            //Increase movement speed when the next attack is the ring so the bell reaches its position sooner.
            if (currentState == HealerBellStates.seeking && nextAttackState == HealerBellStates.ring)
            {
                return healerBellData.GetSpeed(currentState, nextAttackState) * healerBellData.ringSpeedMultiplier * localSpeedFormMod;
            }
            else
            {
                return healerBellData.GetSpeed(currentState, nextAttackState) * localSpeedFormMod;

            }


        }
        else
        {
            return healerBellData.GetSpeed(currentState, nextAttackState) * enemyData.speedMultiplierWhenNotVisible;
        }
    }


    /// <summary>
    /// The healer handles this differently from the other bells:
    /// teleport if the player is close; otherwise, heal the living bell with the lowest health.
    /// </summary>
    /// <param name="currentState">Current boss state.</param>
    /// <returns></returns>
    protected HealerBellStates GetNextAttack()
    {
        //Check whether the player is closer than the teleport threshold.
        if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) < healerBellData.tp.DistancePerForm[currentLocalForm])
        {
            return HealerBellStates.tp;
        }
        else 
        {
            if (bellManager.CanHeal())
            {
                return HealerBellStates.heal;
            }
            else
            {
                return HealerBellStates.idle;
            }
            
        }
    }

    protected float GetNextAttackDistance(HealerBellStates targetState)
    {
        switch (targetState)
        {
            case HealerBellStates.ring:
                return healerBellData.ring.DistancePerForm[currentLocalForm];
            case HealerBellStates.heal:
                return healerBellData.heal.DistancePerForm[currentLocalForm];
            case HealerBellStates.tp:
                return healerBellData.tp.DistancePerForm[currentLocalForm];
            case HealerBellStates.idle:
                return healerBellData.tp.DistancePerForm[currentLocalForm];
            default:
                return 0;
        }
    }

    /// <summary>
    /// Read each attack's alert time from healerBellData.
    /// </summary>
    /// <returns></returns>
    protected float GetNextAttackAlertTime(HealerBellStates targetState)
    {
        switch (targetState)
        {
            case HealerBellStates.ring:
                return healerBellData.ring.AntecipationTimePerForm[currentLocalForm];
            case HealerBellStates.heal:
                return healerBellData.heal.AntecipationTimePerForm[currentLocalForm];
            case HealerBellStates.tp:
                return healerBellData.tp.AntecipationTimePerForm[currentLocalForm];
            default:
                return 0;

        }
    }

    /// <summary>
    /// Check whether it is time to enter idle.
    /// </summary>
    /// <returns></returns>
    protected bool CheckIsTimeToIdle()
    {
        if (healerBellData.maxAttackWithoutIdle[currentLocalForm] > attacksSinceLastIdle)
        {
            return false;
        }
        else
        {
            return true;
        }

    }

    /// <summary>
    /// Choose seeking or idle after an attack state finishes.
    /// Then determine whether the next attack is healing or teleporting.
    /// </summary>
    protected void NextNonAttackState()
    {
        //An attack just ended; increment the attack count.
        ++attacksSinceLastIdle;
        if (CheckIsTimeToIdle())
        {
            attacksSinceLastIdle = 0;
            currentState = HealerBellStates.idle;
        }
        else
        {
            if (!needsToRing)
            {
                nextAttackState = GetNextAttack();
            }
            else
            {
                nextAttackState = HealerBellStates.ring;
            }
            currentState = HealerBellStates.seeking;
        }

    }

    /// <summary>
    /// Notify the bell to move to the arena center for the ring attack.
    /// </summary>
    protected void NeedsToRing()
    {
        needsToRing = true;
    }

    public HealerBellStates GetCurrentState()
    {
        return currentState;
    }

    public HealerBellStates GetNextAttackState()
    {
        return nextAttackState;
    }


    /// <summary>
    /// Enter the scream state when the boss changes form; apply form-specific stat changes here.
    /// </summary>
    public void ChangedForm()
    {
        currentLocalForm = bellManager.currentBellForm;
        localSpeedFormMod = (float)Math.Pow(bellManager.GetSpeedIncrease(), currentLocalForm);
    }


    /// <summary>
    ///Also used when taking damage while preparing to fire.
    ///Enter seeking and restart the cooldown timer.
    /// </summary>
    public void GoToIdle()
    {
        ResetVariables();
        timeInIdle = 0;
        currentState = HealerBellStates.idle;
        OnChangeState?.Invoke();
    }

    //These target helpers switch between looking at the next path point (targetPath)
    //and facing the player while aiming or shooting.
    protected void TargetPlayer()
    {
        target = playerTransform;
        UpdatePath();
    }


    /// <summary>
    /// Reset all state-related variables.
    /// </summary>
    protected void ResetVariables()
    {
        //currentState = HealerBellStates.idle;
        timeInIdle = 0f;
        timeInSeeking = 0f;
        timeInTp = 0f;
        timeInRing = 0f;
        timeInStun = 0f;
        timeInHeal = 0f;
        bossScript.DeactivateAllAttacks();
        ChangePathUpdateTime(healerBellData.updatePathInterval);

        targetingPlayer = false;
        firstTimeEnteringThisState = true;
        BellAnim.lockDirection = false; // Consider moving this to the base class.
    }



    //Attack Queue related
    /// <summary>
    /// Enqueue up to three states.
    /// </summary>
    /// <param name="targetState"></param>
    public void MaxEnqueue(HealerBellStates targetState)
    {
        if (lastAttackStates.Count == 3)
        {
            lastAttackStates.Dequeue(); // Remove the oldest entry to keep three items.
        }
        lastAttackStates.Enqueue(targetState);
    }

    /// <summary>
    /// Check whether the last three attacks were identical.
    /// </summary>
    /// <returns></returns>
    protected bool CheckStateRepetion()
    {


        if (lastAttackStates.Count < 3) return false; // Require three entries.

        HealerBellStates[] tempStates = lastAttackStates.ToArray();

        if (tempStates[0] == tempStates[1] && tempStates[1] == tempStates[2])
        {
            return true;
        }
        return false;

    }

    /// <summary>
    /// Choose the next attack without repeating one already used three consecutive times.
    /// </summary>
    /// <returns></returns>
    protected HealerBellStates GetNextAttackWithoutRepetition()
    {
        HealerBellStates possibleNextState = GetNextAttack();
        HealerBellStates[] tempStates = lastAttackStates.ToArray();
        int iterationCount = 0;

        //For Debug
        /*
        if (lastAttackStates.Count >= 3)
        {
            tempStates = lastAttackStates.ToArray();
            Debug.LogWarning("Attacks in queue: " + tempStates[0] + " " + tempStates[1] + " " + tempStates[2]+" Repetition ? "+ CheckStateRepetion());
        }
        */

        //If a state has occurred three times in a row,
        if (CheckStateRepetion())
        {
            //keep drawing until a different state is selected.
            while (possibleNextState == tempStates[0])
            {
                possibleNextState = GetNextAttack();
                iterationCount++;
                if (iterationCount > 50)
                {
                    MaxEnqueue(possibleNextState);
                    return possibleNextState;
                }
            }
            MaxEnqueue(possibleNextState);
            return possibleNextState;
        }
        else // Without repeated attacks, return the original selection.
        {
            MaxEnqueue(possibleNextState);
            return possibleNextState;
        }

    }

    /// <summary>
    /// On melee damage, enter stun if the current attack permits interruption.
    /// </summary>
    protected void TookMeleeDamage()
    {
        if (healerBellData.stunCoolDown[currentLocalForm] <= (Time.time - timeOfLastStun))
        {
            //If currently in alert,
            if (currentState == HealerBellStates.alert)
            {
                switch (nextAttackState)
                {
                    case HealerBellStates.ring:

                        if (healerBellData.ring.canBeStunned)
                        {
                            timeOfLastStun = Time.time;
                            ResetVariables();
                            currentState = HealerBellStates.stun;
                            OnChangeState?.Invoke();
                        }

                        break;
                    case HealerBellStates.heal:
                        if (healerBellData.heal.canBeStunned)
                        {
                            timeOfLastStun = Time.time;
                            ResetVariables();
                            currentState = HealerBellStates.stun;
                            OnChangeState?.Invoke();
                        }
                        break;
                    case HealerBellStates.tp:
                        if (healerBellData.tp.canBeStunned)
                        {
                            timeOfLastStun = Time.time;
                            ResetVariables();
                            currentState = HealerBellStates.stun;
                            OnChangeState?.Invoke();
                        }
                        break;
                    default:

                        break;

                }
            }
        }
    }


    public void ToogleTpAndHeal_Animation()
    {
        if (currentState == HealerBellStates.heal)
        {
            HealAttack_Animation();
        }
        else
        {
            TpAttack_Animation();
        }
    }

    /// <summary>
    /// perform the healer's teleport.
    /// </summary>
    protected void TpAttack_Animation()
    {
        //Add teleport effects here.
        GameObject tpLocation = GetTpLocation();
        if (tpLocation != null)
        {
            RepositionBellSafely_TechBooh(tpLocation.transform.position);
            RegisterTeleportFailsafe_TechBooh();
            return;
        }

        Vector2 emergencyDirection = (Vector2)(transform.position - playerTransform.position);
        Vector2 emergencyPosition = FindNearbySafePosition_TechBooh(transform.position, emergencyDirection);
        RepositionBellSafely_TechBooh(emergencyPosition);
        RegisterTeleportFailsafe_TechBooh();
    }

    /// <summary>
    /// TechBooh:
    /// The healer can repeatedly teleport into unsuitable positions
    /// near pillars. This short window detects repeated attempts
    /// and enables forced recovery at the arena center.
    /// </summary>
    protected void UpdateRepeatedTeleportWindow_TechBooh()
    {
        if (repeatedTeleportWindowTimer_TechBooh <= 0f)
        {
            return;
        }

        repeatedTeleportWindowTimer_TechBooh -= Time.fixedDeltaTime;
        if (repeatedTeleportWindowTimer_TechBooh > 0f)
        {
            return;
        }

        repeatedTeleportWindowTimer_TechBooh = 0f;
        repeatedTeleportCount_TechBooh = 0;
    }

    /// <summary>
    /// TechBooh:
    /// Repeated teleports within a short window indicate failed recovery;
    /// move the healer to the arena center/origin to stabilize the fight.
    /// </summary>
    protected void RegisterTeleportFailsafe_TechBooh()
    {
        repeatedTeleportWindowTimer_TechBooh = repeatedTeleportWindow_TechBooh;
        repeatedTeleportCount_TechBooh++;

        if (repeatedTeleportCount_TechBooh < repeatedTeleportLimitBeforeReset_TechBooh)
        {
            return;
        }

        repeatedTeleportCount_TechBooh = 0;
        repeatedTeleportWindowTimer_TechBooh = 0f;

        if (forceReturnToArenaCenterOnRepeatedTeleport_TechBooh)
        {
            RepositionBellSafely_TechBooh(GetArenaRecoveryPosition_TechBooh());
        }

        ResetVariables();
        currentState = HealerBellStates.idle;
        OnChangeState?.Invoke();
    }

    protected GameObject GetTpLocation() 
    {
        if (tpPositions == null || tpPositions.Count == 0)
        {
            return null;
        }

        float maxDistance = float.MinValue;
        GameObject targetL = tpPositions[0];
        GameObject fallbackTarget = tpPositions[0];

        for (int i = 0; i < tpPositions.Count; i++)
        {
            GameObject candidate = tpPositions[i];
            if (candidate == null)
            {
                continue;
            }

            float playerDistance = Math.Abs(Vector2.Distance(candidate.transform.position, playerTransform.position));

            if (playerDistance > Math.Abs(Vector2.Distance(fallbackTarget.transform.position, playerTransform.position)))
            {
                fallbackTarget = candidate;
            }

            // TechBooh:
            // Teleporting previously selected only the point farthest from the player.
            // In Cathedral Inside, that point could overlap a pillar or wall and trap the boss.
            // Keep moving away from the player, but accept only unobstructed destinations.
            if (playerDistance >= minimumTeleportDistanceFromPlayer_TechBooh &&
                IsPositionFree_TechBooh(candidate.transform.position) &&
                playerDistance > maxDistance)
            {
                maxDistance = playerDistance;
                targetL = candidate;
            }
        }

        if (maxDistance > float.MinValue)
        {
            return targetL;
        }

        if (fallbackTarget != null && IsPositionFree_TechBooh(fallbackTarget.transform.position))
        {
            return fallbackTarget;
        }

        return null;
    }    


    /// <summary>
    /// Start a coroutine to heal the lowest-health bell over time.
    /// </summary>
    protected void HealAttack_Animation()
    {
        bellManager.Heal(healerBellData.healIntensityPerForm[currentLocalForm]);
    }

}
