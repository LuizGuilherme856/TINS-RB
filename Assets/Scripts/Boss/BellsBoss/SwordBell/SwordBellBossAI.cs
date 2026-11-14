using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using Random = UnityEngine.Random;
public class SwordBellBossAI : BellBossAI
{
    //Manager Related
    [SerializeField] protected BellsManager bellManager;


    //State Related
    public enum SwordBellStates { idle, seeking, alert, stun, spin, shoot, ring }
    [SerializeField] protected SwordBellStates currentState = SwordBellStates.seeking;

    //Select the next attack and prepare the state machine.
    [SerializeField] protected SwordBellStates nextAttackState = SwordBellStates.idle;

    protected Queue<SwordBellStates> lastAttackStates = new Queue<SwordBellStates>(3);
  
    protected int attacksSinceLastIdle = 0;
    //Data Related
    protected SwordBellBossData swordBellData;

    //Form Related

    protected float localSpeedFormMod = 1.0f;


    //Maps each boss phase to its available attacks.
    Dictionary<int, (int, SwordBellStates)[]> unlockedAttacks = new Dictionary<int, (int, SwordBellStates)[]>();

    //Timer Related
    [Header("Timers")]
    [SerializeField] protected float timeInIdle = 0f;
    [SerializeField] protected float timeInSeeking = 0f;
    [SerializeField] protected float timeInShoot = 0f;
    [SerializeField] protected float timeInRing = 0f;
    [SerializeField] protected float timeInStun = 0f;
    [SerializeField] protected float timeInSpin = 0f;
    [SerializeField] protected float timeInAlert = 0f;

    [SerializeField] protected float maxAtThisState = 0f;

    protected bool firstTimeEnteringThisState = true;


    //[SerializeField] protected BossNextForm nextFormScript;

    //Targeting Related
    [SerializeField] protected Transform playerTransform;
    [SerializeField] protected Transform currentTargetTransform;
    [SerializeField] protected bool targetingPlayer = false;

    //Events
    public event Action OnChangeState;

    //Animator
    [SerializeField] protected SwordBellMovementAnimations BellAnim;

    //Health Related
    [SerializeField] protected EnemyHealth BellHealth;


    //Not Varia Attacking and Shoot related
    [SerializeField] protected AttackPlayer attackPlayer; // Assigned in the Editor.
    [SerializeField] protected AttackPlayerWithGunData attackPlayerWithGunData; // Assigned in the Editor.
    // 0 = AP1, 1 = AP2
    [SerializeField] protected int lastAttackPlayerUsed = 1;


    //Ring Related
    [SerializeField] protected Transform RingLocation;
    protected MoveObjectToPos moveObjToPosScript;
    protected bool needsToRing = false;



    //Stun Related
    protected HandleAttackOnEnemy handleAttackOnBell;
    protected float timeOfLastStun = 0;

    //Spin Related
    [SerializeField] protected GameObject spinGameObject; //Assigned in the Editor.
    [Header("TechBooh - Anti Stuck")]
    [SerializeField] protected float stuckCheckInterval_TechBooh = 0.8f;
    [SerializeField] protected float minimumMovementToAvoidStuck_TechBooh = 0.2f;
    [SerializeField] protected int consecutiveStuckChecksRequired_TechBooh = 3;
    [SerializeField] protected float unstuckCooldown_TechBooh = 2.5f;
    [SerializeField] protected float repeatedUnstuckWindow_TechBooh = 6f;
    [SerializeField] protected int repeatedUnstuckLimitBeforeReset_TechBooh = 2;

    protected Vector2 lastStuckCheckPosition_TechBooh;
    protected float stuckCheckTimer_TechBooh = 0f;
    protected int consecutiveStuckChecks_TechBooh = 0;
    protected float unstuckCooldownTimer_TechBooh = 0f;
    protected float repeatedUnstuckWindowTimer_TechBooh = 0f;
    protected int repeatedUnstuckCount_TechBooh = 0;


    protected override void Awake()
    {

        base.Awake();
        try
        {
            swordBellData = (SwordBellBossData)enemyData;
        }
        catch (Exception)
        {
            Debug.LogError("enemyData nÃ£o Ã© uma instancia de SwordBellBossData! Coloque um SwordBellBossData na referencia 'Enemy Data'");
        }
        try
        {
            bellManager = GetComponentInParent<BellsManager>();
            bellManager.OnTimeToRing += NeedsToRing;
            bellManager.OnChangeForm += ChangedForm;
        }
        catch (Exception)
        {
            Debug.LogError("Pai na hierarquia de SwordBell nÃ£o tem um bellManager");
        }
        try
        {
            BellHealth = this.gameObject.GetComponent<EnemyHealth>();
            BellHealth.OnZeroHealth += ResetVariables;
            BellHealth.OnZeroHealth += bellManager.BellDeath;
        }
        catch (Exception)
        {
            Debug.LogError("SwordBell nÃ£o possui um script EnemyHealth");
        }
        /*
        try
        {
            changeTargetScript = this.gameObject.GetComponent<ChangeTarget>();
        }
        catch (Exception)
        {
            Debug.LogError("PsicoPato has no ChangeTarget component");
        }
        */
        
        try
        {
            attackPlayer = GetComponent<AttackPlayer>();
        }
        catch (System.Exception)
        {
            Debug.LogError("SwordBell  nÃ£o tem um AttackPlayer.cs");
        }

        try
        {
            attackPlayerWithGunData = GetComponent<AttackPlayerWithGunData>();
        }
        catch (System.Exception)
        {
            Debug.LogError("SwordBell  nÃ£o tem um attackPlayerWithGunData.cs");
        }

        try
        {
            BellAnim = GetComponent<SwordBellMovementAnimations>();
        }
        catch (System.Exception)
        {
            Debug.LogError("SwordBell  nÃ£o tem um SwordBellMovementAnimations.cs");
        }

        try
        {
            moveObjToPosScript = GetComponent<MoveObjectToPos>();
        }
        catch (System.Exception)
        {
            Debug.LogError("SwordBell nÃ£o tem MoveObjectToPos");
        }

        try
        {
            handleAttackOnBell = GetComponent<HandleAttackOnEnemy>();
            handleAttackOnBell.OnTakeMeleeDamage += TookMeleeDamage;
        }
        catch (System.Exception)
        {
            Debug.LogError("Bell nÃ£o tem HandleAttackOnEnemy");
        }


        InitiateDictionary();
        ConfigureArenaRecoveryPosition_TechBooh(RingLocation);

        bellManager.OnChangeForm += ChangedForm;
        //Detach the attack so its collision area does not damage the boss.
        spinGameObject.transform.parent = null;


    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //Return to seeking and reset timers.
        currentState = SwordBellStates.idle;
        ResetVariables();
        lastStuckCheckPosition_TechBooh = transform.position;
        stuckCheckTimer_TechBooh = 0f;
        consecutiveStuckChecks_TechBooh = 0;
        unstuckCooldownTimer_TechBooh = 0f;
        repeatedUnstuckWindowTimer_TechBooh = 0f;
        repeatedUnstuckCount_TechBooh = 0;
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
            case SwordBellStates.idle:
                timeInIdle += Time.deltaTime;
                break;

            case SwordBellStates.seeking:
                timeInSeeking += Time.deltaTime;
                break;

            case SwordBellStates.ring:
                timeInRing += Time.deltaTime;
                break;

            case SwordBellStates.stun:
                timeInStun += Time.deltaTime;
                break;

            case SwordBellStates.shoot:
                timeInShoot += Time.deltaTime;
                break;

            case SwordBellStates.spin:
                timeInSpin += Time.deltaTime;
                break;
            case SwordBellStates.alert:
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
        switch (currentState)
        {
            case SwordBellStates.idle:
                HandleIdleState();
                break;

            case SwordBellStates.seeking:
                HandleSeekingState();
                break;

            case SwordBellStates.ring:
                HandleRingState();
                break;

            case SwordBellStates.stun:
                HandleStunState();
                break;

            case SwordBellStates.shoot:
                HandleShootState();
                break;

            case SwordBellStates.spin:
                HandleSpinState();
                break;

            case SwordBellStates.alert:
                HandleAlertState();
                break;

            default:
                break;
        }

        RecoverFromPillarStuck_TechBooh();

        bool shouldMonitorConfinement = currentState == SwordBellStates.spin ||
            (currentState == SwordBellStates.seeking && target != null &&
             Vector2.Distance(transform.position, target.position) > GetNextAttackDistance(nextAttackState) + 0.5f);

        if (RecoverIfConfinedNearObstacle_TechBooh(shouldMonitorConfinement))
        {
            ResetVariables();
            currentState = SwordBellStates.idle;
            OnChangeState?.Invoke();
            return;
        }

        base.FixedUpdate();
    }

    /// <summary>
    /// TechBooh:
    /// The sword bell can keep pressing against a pillar during spin/chase
    /// and stop completely, making the fight trivial.
    /// Monitor short periods without displacement during movement states
    /// and move the boss a few meters to a safe point when stuck.
    /// </summary>
    protected void RecoverFromPillarStuck_TechBooh()
    {
        if (unstuckCooldownTimer_TechBooh > 0f)
        {
            unstuckCooldownTimer_TechBooh -= Time.fixedDeltaTime;
        }

        if (repeatedUnstuckWindowTimer_TechBooh > 0f)
        {
            repeatedUnstuckWindowTimer_TechBooh -= Time.fixedDeltaTime;
            if (repeatedUnstuckWindowTimer_TechBooh <= 0f)
            {
                repeatedUnstuckWindowTimer_TechBooh = 0f;
                repeatedUnstuckCount_TechBooh = 0;
            }
        }

        if (currentState != SwordBellStates.spin && currentState != SwordBellStates.seeking)
        {
            lastStuckCheckPosition_TechBooh = transform.position;
            stuckCheckTimer_TechBooh = 0f;
            consecutiveStuckChecks_TechBooh = 0;
            return;
        }

        if (desiredDirection.sqrMagnitude <= 0.001f)
        {
            lastStuckCheckPosition_TechBooh = transform.position;
            stuckCheckTimer_TechBooh = 0f;
            consecutiveStuckChecks_TechBooh = 0;
            return;
        }

        if (unstuckCooldownTimer_TechBooh > 0f)
        {
            lastStuckCheckPosition_TechBooh = transform.position;
            stuckCheckTimer_TechBooh = 0f;
            consecutiveStuckChecks_TechBooh = 0;
            return;
        }

        stuckCheckTimer_TechBooh += Time.fixedDeltaTime;
        if (stuckCheckTimer_TechBooh < stuckCheckInterval_TechBooh)
        {
            return;
        }

        float movedDistance = Vector2.Distance(lastStuckCheckPosition_TechBooh, transform.position);
        stuckCheckTimer_TechBooh = 0f;
        lastStuckCheckPosition_TechBooh = transform.position;

        if (movedDistance > minimumMovementToAvoidStuck_TechBooh)
        {
            consecutiveStuckChecks_TechBooh = 0;
            return;
        }

        consecutiveStuckChecks_TechBooh++;
        if (consecutiveStuckChecks_TechBooh < consecutiveStuckChecksRequired_TechBooh)
        {
            return;
        }

        consecutiveStuckChecks_TechBooh = 0;

        Vector2 preferredDirection = target != null
            ? ((Vector2)transform.position - (Vector2)target.position).normalized
            : -desiredDirection.normalized;

        Vector2 safePosition = FindNearbySafePosition_TechBooh(transform.position, preferredDirection);
        if (Vector2.Distance(safePosition, transform.position) <= 0.01f)
        {
            return;
        }

        RepositionBellSafely_TechBooh(safePosition);
        unstuckCooldownTimer_TechBooh = unstuckCooldown_TechBooh;
        lastStuckCheckPosition_TechBooh = transform.position;
        RegisterRepeatedUnstuck_TechBooh();
    }

    /// <summary>
    /// TechBooh:
    /// Repeated recovery attempts in a short window indicate
    /// an unsuitable navigation context that would keep teleporting the bell
    /// back toward the pillar. Reset the state machine in this situation to
    /// return to idle and recalculate combat from a clean state.
    /// As a final fallback, move directly to the arena center/origin.
    /// </summary>
    protected void RegisterRepeatedUnstuck_TechBooh()
    {
        repeatedUnstuckWindowTimer_TechBooh = repeatedUnstuckWindow_TechBooh;
        repeatedUnstuckCount_TechBooh++;

        if (repeatedUnstuckCount_TechBooh < repeatedUnstuckLimitBeforeReset_TechBooh)
        {
            return;
        }

        repeatedUnstuckCount_TechBooh = 0;
        repeatedUnstuckWindowTimer_TechBooh = 0f;

        if (forceReturnToArenaCenterOnRepeatedStuck_TechBooh)
        {
            RepositionBellSafely_TechBooh(GetArenaRecoveryPosition_TechBooh());
        }

        // Exit the current state cleanly to break the loop:
        // "move briefly -> teleport -> return to the pillar".
        ResetVariables();
        currentState = SwordBellStates.idle;
        OnChangeState?.Invoke();
    }

    private void HandleAlertState()
    {
        if (timeInAlert <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = GetNextAttackAlertTime(nextAttackState);


            //Read the state's range for the current form and requested upper or lower limit.
            firstTimeEnteringThisState = false;
        }
        if (timeInAlert >= maxAtThisState && timeInAlert >= swordBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInAlert = 0;
            //Choose the next attack now to determine the required distance.

            currentState = nextAttackState;
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
        }
    }

    private void HandleSpinState()
    {
        if (timeInSpin <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = Random.Range(swordBellData.spin.RangesPerForm[currentLocalForm].Ranges[0],
                swordBellData.spin.RangesPerForm[currentLocalForm].Ranges[1]);

            //Enable the spinning attack area.
            //spinGameObject.SetActive(true);
            bossScript.AttackByIndex(1);

            ChangePathUpdateTime(swordBellData.pathUpdateInSpin);

            //Read the state's range for the current form and requested upper or lower limit.
            firstTimeEnteringThisState = false;
        }

        if (timeInSpin >= maxAtThisState && timeInSpin >= swordBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInSpin = 0;
            ChangePathUpdateTime(swordBellData.updatePathInterval);
            //Choose the next attack now to determine the required distance.
            NextNonAttackState();
            //Disable the spinning attack area.
            //spinGameObject.SetActive(false);
            bossScript.DeactivateAttackByIndex(1);
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
        }
    }

    private void HandleShootState()
    {
        if (timeInShoot <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = Random.Range(swordBellData.shoot.RangesPerForm[currentLocalForm].Ranges[0],
                swordBellData.shoot.RangesPerForm[currentLocalForm].Ranges[1]);

            //Read the state's range for the current form and requested upper or lower limit.
            firstTimeEnteringThisState = false;
        }
        if (timeInShoot >= maxAtThisState && timeInShoot >= swordBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInShoot = 0;

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

            maxAtThisState = Random.Range(swordBellData.stun.RangesPerForm[currentLocalForm].Ranges[0],
                swordBellData.stun.RangesPerForm[currentLocalForm].Ranges[1]);

            firstTimeEnteringThisState = false;
        }
        if (timeInStun >= maxAtThisState && timeInStun >= swordBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInStun = 0;

            currentState = SwordBellStates.idle;

            OnChangeState?.Invoke();

        }
    }

    private void HandleRingState()
    {
        if (timeInRing <= 0 || firstTimeEnteringThisState)
        {
            needsToRing = false;
            //Read the state's range for the current form and requested upper or lower limit.
            maxAtThisState = Random.Range(swordBellData.ring.RangesPerForm[currentLocalForm].Ranges[0],
                    swordBellData.ring.RangesPerForm[currentLocalForm].Ranges[1]);

            //Activate the ring attack.
            //bossScript.AttackByIndex(0); //Activation is driven by the animation.

            firstTimeEnteringThisState = false;
        }
        if (timeInRing >= maxAtThisState && timeInRing >= swordBellData.minimumTimeOnState)
        {
            //Deactivate the ring attack.
            bossScript.DeactivateAttackByIndex(0);

            firstTimeEnteringThisState = true;
            timeInRing = 0;
            BellAnim.lockDirection = false;
            //Choose the next attack now to determine the required distance.
            attacksSinceLastIdle = 0;
            currentState = SwordBellStates.idle;
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
        }
    }

    private void HandleSeekingState()
    {
        if (firstTimeEnteringThisState)
        {
            if (nextAttackState == SwordBellStates.ring)
            {
                //Target the arena center during the spawner ring attack.
                target = RingLocation;
                UpdatePath();
            }
            firstTimeEnteringThisState = false;
        }

        bool distAtualMenorDistTarget = Math.Abs(Vector2.Distance(transform.position, target.position)) < GetNextAttackDistance(nextAttackState);
        if (distAtualMenorDistTarget && timeInSeeking >= swordBellData.minimumTimeOnState)
        {
            firstTimeEnteringThisState = true;
            timeInSeeking = 0;
            currentState = SwordBellStates.alert;

            //Set the Animator to the new state.

            TargetPlayer();
            OnChangeState?.Invoke();
        }
    }

    private void HandleIdleState()
    {
        if (timeInIdle <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = Random.Range(swordBellData.idle.RangesPerForm[currentLocalForm].Ranges[0],
                swordBellData.idle.RangesPerForm[currentLocalForm].Ranges[1]);


            //Read the state's range for the current form and requested upper or lower limit.
            firstTimeEnteringThisState = false;
        }
        if (timeInIdle >= maxAtThisState && timeInIdle >= swordBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInIdle = 0;
            //Choose the next attack now to determine the required distance.
            if (!needsToRing)
            {
                nextAttackState = GetNextAttackWithoutRepetition();
            }
            else
            {
                nextAttackState = SwordBellStates.ring;
            }

            currentState = SwordBellStates.seeking;
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
        }
    }

    protected override Vector2 GetDesiredDirection()
    {
        if (currentState == SwordBellStates.seeking)
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
            if (currentState == SwordBellStates.seeking && nextAttackState == SwordBellStates.ring)
            {
                return swordBellData.GetSpeed(currentState, nextAttackState) * swordBellData.ringSpeedMultiplier * localSpeedFormMod;
            }
            else
            {
                return swordBellData.GetSpeed(currentState, nextAttackState) * localSpeedFormMod;

            }


        }
        else
        {
            return swordBellData.GetSpeed(currentState, nextAttackState) * enemyData.speedMultiplierWhenNotVisible;
        }
    }


    /// <summary>
    /// Initialize the mapping between boss forms and available attacks.
    /// </summary>
    protected void InitiateDictionary()
    {
        unlockedAttacks = new Dictionary<int, (int, SwordBellStates)[]>();
        (int, SwordBellStates)[] form1 =
            {
            (swordBellData.spin.WeightsPerForm[0], swordBellData.spin.state),
            (swordBellData.shoot.WeightsPerForm[0], swordBellData.shoot.state),
            };
        unlockedAttacks.Add(0, form1);
        (int, SwordBellStates)[] form2 =
            {
            (swordBellData.spin.WeightsPerForm[1], swordBellData.spin.state),
            (swordBellData.shoot.WeightsPerForm[1], swordBellData.shoot.state),
            };
        unlockedAttacks.Add(1, form2);
        (int, SwordBellStates)[] form3 =
            {
            (swordBellData.spin.WeightsPerForm[2], swordBellData.spin.state),
            (swordBellData.shoot.WeightsPerForm[2], swordBellData.shoot.state),
            };
        unlockedAttacks.Add(2, form3);
        (int, SwordBellStates)[] form4 =
    {
            (swordBellData.spin.WeightsPerForm[3], swordBellData.spin.state),
            (swordBellData.shoot.WeightsPerForm[3], swordBellData.shoot.state),
            };
        unlockedAttacks.Add(3, form4);
    }


    /// <summary>
    /// Select a state using its configured weights.
    /// </summary>
    /// <param name="currentState">Current boss state.</param>
    /// <returns></returns>
    protected SwordBellStates NextWeightedRandomState()
    {
        //unlockedAttacks[nextFormScript.currentForm];
        // Step 1: Normalize probabilities.
        float totalProbability = 0f;
        foreach ((int, SwordBellStates) targetState in unlockedAttacks[currentLocalForm])
        {

            totalProbability += targetState.Item1;
        }

        // Ensure the total is greater than zero.
        if (totalProbability <= 0)
        {
            Debug.LogError("As probabilidades de estados devem ser maiores que 0.");
            return SwordBellStates.idle;
        }

        // Step 2: Draw a random value within the total probability weight.
        float randomValue = Random.Range(0, totalProbability);

        // Step 3: Find the corresponding attack.
        float cumulativeProbability = 0f;
        foreach ((int, SwordBellStates) targetState in unlockedAttacks[currentLocalForm])
        {

            cumulativeProbability += targetState.Item1;

            if (randomValue <= cumulativeProbability)
            {
                return targetState.Item2;
            }
        }

        // Return null if selection fails.
        Debug.LogError("Nenhum estado foi selecionado. Verifique as probabilidades :c.");
        return SwordBellStates.idle;
    }

    protected float GetNextAttackDistance(SwordBellStates targetState)
    {
        switch (targetState)
        {
            case SwordBellStates.ring:
                return swordBellData.ring.DistancePerForm[currentLocalForm];
            case SwordBellStates.spin:
                return swordBellData.spin.DistancePerForm[currentLocalForm];
            case SwordBellStates.shoot:
                return swordBellData.shoot.DistancePerForm[currentLocalForm];
            default:
                return 0;
        }
    }

    /// <summary>
    /// Read each attack's alert time from swordBellData.
    /// </summary>
    /// <returns></returns>
    protected float GetNextAttackAlertTime(SwordBellStates targetState)
    {
        switch (targetState)
        {
            case SwordBellStates.ring:
                return swordBellData.ring.AntecipationTimePerForm[currentLocalForm];
            case SwordBellStates.spin:
                return swordBellData.spin.AntecipationTimePerForm[currentLocalForm];
            case SwordBellStates.shoot:
                return swordBellData.shoot.AntecipationTimePerForm[currentLocalForm];
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
        if (swordBellData.maxAttackWithoutIdle[currentLocalForm] > attacksSinceLastIdle)
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
    /// Also select the next attack now.
    /// </summary>
    protected void NextNonAttackState()
    {
        //An attack just ended; increment the attack count.
        ++attacksSinceLastIdle;
        if (CheckIsTimeToIdle())
        {
            attacksSinceLastIdle = 0;
            currentState = SwordBellStates.idle;
        }
        else
        {
            if (!needsToRing)
            {
                nextAttackState = GetNextAttackWithoutRepetition();
            }
            else
            {
                nextAttackState = SwordBellStates.ring;
            }
            currentState = SwordBellStates.seeking;
        }

    }

    /// <summary>
    /// Notify the bell to move to the arena center for the ring attack.
    /// </summary>
    protected void NeedsToRing()
    {
        needsToRing = true;
    }

    public SwordBellStates GetCurrentState()
    {
        return currentState;
    }

    public SwordBellStates GetNextAttackState()
    {
        return nextAttackState;
    }


    /// <summary>
    /// Enter scream when the boss changes form; apply form-specific stat changes here.
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
        currentState = SwordBellStates.idle;
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
        //currentState = SwordBellStates.idle;
        timeInIdle = 0f;
        timeInSeeking = 0f;
        timeInShoot = 0f;
        timeInRing = 0f;
        timeInStun = 0f;
        timeInSpin = 0f;
        bossScript.DeactivateAllAttacks();
        ChangePathUpdateTime(swordBellData.updatePathInterval);
        //DisableLaserByAnim();
        //spinGameObject.SetActive(false);
        target = playerTransform;
        //dashPreviewObject.SetActive(false);
        targetingPlayer = false;
        firstTimeEnteringThisState = true;
        BellAnim.lockDirection = false; // Consider moving this to the base class.
    }



    //Attack Queue related
    /// <summary>
    /// Enqueue up to three states.
    /// </summary>
    /// <param name="targetState"></param>
    public void MaxEnqueue(SwordBellStates targetState)
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

        SwordBellStates[] tempStates = lastAttackStates.ToArray();

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
    protected SwordBellStates GetNextAttackWithoutRepetition()
    {
        SwordBellStates possibleNextState = NextWeightedRandomState();
        SwordBellStates[] tempStates = lastAttackStates.ToArray();
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
            //Keep drawing until a different state is selected.
            while (possibleNextState == tempStates[0])
            {
                possibleNextState = NextWeightedRandomState();
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
        if (swordBellData.stunCoolDown[currentLocalForm] <= (Time.time - timeOfLastStun))
        {
            //If currently in alert,
            if (currentState == SwordBellStates.alert)
            {
                switch (nextAttackState)
                {
                    case SwordBellStates.ring:

                        if (swordBellData.ring.canBeStunned)
                        {
                            timeOfLastStun = Time.time;
                            ResetVariables();
                            currentState = SwordBellStates.stun;
                            OnChangeState?.Invoke();
                        }

                        break;
                    case SwordBellStates.spin:
                        if (swordBellData.spin.canBeStunned)
                        {
                            timeOfLastStun = Time.time;
                            ResetVariables();
                            currentState = SwordBellStates.stun;
                            OnChangeState?.Invoke();
                        }
                        break;
                    case SwordBellStates.shoot:
                        if (swordBellData.shoot.canBeStunned)
                        {
                            timeOfLastStun = Time.time;
                            ResetVariables();
                            currentState = SwordBellStates.stun;
                            OnChangeState?.Invoke();
                        }
                        break;
                    default:

                        break;

                }
            }
        }
    }



    public void ToogleShoot_Animation() 
    {
        if (lastAttackPlayerUsed == 1)
        {
            attackPlayerWithGunData.InstantiateProjectile(0,0);
            lastAttackPlayerUsed = 0;

        }
        else 
        {
            attackPlayerWithGunData.InstantiateProjectile(1, 1);

            lastAttackPlayerUsed = 1;
        }
    }
}


