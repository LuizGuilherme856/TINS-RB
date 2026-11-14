using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using Random = UnityEngine.Random;
public class SpearBellBossAI : BellBossAI
{
    //Manager Related
    [SerializeField] protected BellsManager bellManager;


    //State Related
    public enum SpearBellStates { idle, seeking, alert, stun, block, shoot, ring }
    [SerializeField] protected SpearBellStates currentState = SpearBellStates.seeking;
    //Select the next attack and prepare the state machine.
    [SerializeField] protected SpearBellStates nextAttackState = SpearBellStates.idle;
    protected Queue<SpearBellStates> lastAttackStates = new Queue<SpearBellStates>(3);
    //[SerializeField] protected int maxAttackWithoutIdle = 2; //Moved to the boss data.
    protected int attacksSinceLastIdle = 0;
    //Data Related
    protected SpearBellBossData spearBellData;

    //Form Related
    [SerializeField]  protected float localSpeedIncreaseMod = 1.0f;


    //Maps each boss phase to its available attacks.
    Dictionary<int, (int, SpearBellStates)[]> unlockedAttacks = new Dictionary<int, (int, SpearBellStates)[]>();

    //Timer Related
    [Header("Timers")]
    [SerializeField] protected float timeInIdle = 0f;
    [SerializeField] protected float timeInSeeking = 0f;
    [SerializeField] protected float timeInShoot = 0f;
    [SerializeField] protected float timeInRing = 0f;
    [SerializeField] protected float timeInStun = 0f;
    [SerializeField] protected float timeInBlock = 0f;
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
    [SerializeField] protected SpearBellMovementAnimations BellAnim;

    //Health Related
    [SerializeField] protected EnemyHealth BellHealth;


    //Not Varia Attacking Related
    protected AttackPlayer attackPlayer;



    //Ring Related
    [SerializeField] protected Transform RingLocation;
    protected MoveObjectToPos moveObjToPosScript;
    protected bool needsToRing = false;



    //Stun Related
    protected HandleAttackOnEnemy handleAttackOnBell;
    protected float timeOfLastStun = 0;

    //Block Related
    [SerializeField] protected GameObject blockGameObject; //Assigned in the Editor.


    protected override void Awake()
    {
        
        base.Awake();
        try
        {
            spearBellData = (SpearBellBossData)enemyData;
        }
        catch (Exception)
        {
            Debug.LogError("enemyData não é uma instancia de SpearBellBossData! Coloque um SpearBellBossData na referencia 'Enemy Data'");
        }
        try
        {
            bellManager = GetComponentInParent<BellsManager>();
            bellManager.OnTimeToRing += NeedsToRing;
            bellManager.OnChangeForm += ChangedForm;
        }
        catch (Exception)
        {
            Debug.LogError("Pai na hierarquia de SpearBell não tem um bellManager");
        }
        try
        {
            BellHealth = this.gameObject.GetComponent<EnemyHealth>();
            BellHealth.OnZeroHealth += ResetVariables;
            BellHealth.OnZeroHealth += bellManager.BellDeath;
        }
        catch (Exception)
        {
            Debug.LogError("SpearBell não possui um script EnemyHealth");
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
            Debug.LogError("SpearBell  não tem um AttackPlayer.cs");
        }
        try
        {
            BellAnim = GetComponent<SpearBellMovementAnimations>();
        }
        catch (System.Exception)
        {
            Debug.LogError("SpearBell  não tem um SpearBellMovementAnimations.cs");
        }

        try
        {
            moveObjToPosScript = GetComponent<MoveObjectToPos>();
        }
        catch (System.Exception)
        {
            Debug.LogError("SpearBell não tem MoveObjectToPos");
        }


        handleAttackOnBell = GetComponent<HandleAttackOnEnemy>();
        handleAttackOnBell.OnTakeMeleeDamage += TookMeleeDamage;


        InitiateDictionary();
        ConfigureArenaRecoveryPosition_TechBooh(RingLocation);

        bellManager.OnChangeForm += ChangedForm;
        //Detach the object so blocking it does not damage the boss.
        blockGameObject.transform.parent = null;


    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //Return to seeking and reset timers.
        currentState = SpearBellStates.idle;
        ResetVariables();
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
            case SpearBellStates.idle:
                timeInIdle += Time.deltaTime;
                break;

            case SpearBellStates.seeking:
                timeInSeeking += Time.deltaTime;
                break;

            case SpearBellStates.ring:
                timeInRing += Time.deltaTime;
                break;

            case SpearBellStates.stun:
                timeInStun += Time.deltaTime;
                break;

            case SpearBellStates.shoot:
                timeInShoot += Time.deltaTime;
                break;

            case SpearBellStates.block:
                timeInBlock += Time.deltaTime;
                break;
            case SpearBellStates.alert:
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
            case SpearBellStates.idle:
                HandleIdleState();
                break;

            case SpearBellStates.seeking:
                HandleSeekingState();
                break;

            case SpearBellStates.ring:
                HandleRingState();
                break;

            case SpearBellStates.stun:
                HandleStunState();
                break;

            case SpearBellStates.shoot:
                HandleShootState();
                break;

            case SpearBellStates.block:
                HandleBlockState();
                break;

            case SpearBellStates.alert:
                HandleAlertState();
                break;

            default:
                break;
        }

        bool shouldMonitorConfinement = currentState == SpearBellStates.seeking &&
            target != null &&
            Vector2.Distance(transform.position, target.position) > GetNextAttackDistance(nextAttackState) + 0.5f;

        if (RecoverIfConfinedNearObstacle_TechBooh(shouldMonitorConfinement))
        {
            ResetVariables();
            currentState = SpearBellStates.idle;
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
        if (timeInAlert >= maxAtThisState && timeInAlert >= spearBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInAlert = 0;
            //Choose the next attack now to determine the required distance.

            currentState = nextAttackState;
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
            //ActivateAnimationByState(currentState);
        }
    }

    private void HandleBlockState()
    {
        if (timeInBlock <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = Random.Range(spearBellData.block.RangesPerForm[currentLocalForm].Ranges[0],
                spearBellData.block.RangesPerForm[currentLocalForm].Ranges[1]);

            //Enable the blocking area.
            blockGameObject.SetActive(true);
            bossScript.AttackByIndex(1);



            //Read the state's range for the current form and the requested upper or lower limit.
            firstTimeEnteringThisState = false;
        }

        //Align the direction with the boss's facing.
        if ((this.gameObject.transform.localScale.x * blockGameObject.transform.localScale.x) < 0)
        {
            blockGameObject.transform.localScale = new Vector3(blockGameObject.transform.localScale.x * -1, blockGameObject.transform.localScale.y, blockGameObject.transform.localScale.z);
        }
        if (timeInBlock >= maxAtThisState && timeInBlock >= spearBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInBlock = 0;
            //Choose the next attack now to determine the required distance.
            NextNonAttackState();
            //Disable the blocking area.
            blockGameObject.SetActive(false);
            bossScript.DeactivateAttackByIndex(1);
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
            //ActivateAnimationByState(currentState);
        }
    }

    private void HandleShootState()
    {
        if (timeInShoot <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = Random.Range(spearBellData.shoot.RangesPerForm[currentLocalForm].Ranges[0],
                spearBellData.shoot.RangesPerForm[currentLocalForm].Ranges[1]);

            //Read the state's range for the current form and the requested upper or lower limit.
            firstTimeEnteringThisState = false;
        }
        if (timeInShoot >= maxAtThisState && timeInShoot >= spearBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInShoot = 0;

            //Choose the next attack now to determine the required distance.


            NextNonAttackState();

            //Set the Animator to the new state.
            OnChangeState?.Invoke();
            //ActivateAnimationByState(currentState);
        }
    }

    private void HandleStunState()
    {
        if (timeInStun <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = Random.Range(spearBellData.stun.RangesPerForm[currentLocalForm].Ranges[0],
                spearBellData.stun.RangesPerForm[currentLocalForm].Ranges[1]);

            firstTimeEnteringThisState = false;
        }
        if (timeInStun >= maxAtThisState && timeInStun >= spearBellData.minimumTimeOnState)
        {

            firstTimeEnteringThisState = true;
            timeInStun = 0;

            currentState = SpearBellStates.idle;

            OnChangeState?.Invoke();

        }
    }

    private void HandleRingState()
    {
        if (timeInRing <= 0 || firstTimeEnteringThisState)
        {
            needsToRing = false;
            //Read the state's range for the current form and the requested upper or lower limit.
            maxAtThisState = Random.Range(spearBellData.ring.RangesPerForm[currentLocalForm].Ranges[0],
                    spearBellData.ring.RangesPerForm[currentLocalForm].Ranges[1]);


            firstTimeEnteringThisState = false;
        }
        if (timeInRing >= maxAtThisState && timeInRing >= spearBellData.minimumTimeOnState)
        {
            //Deactivate the ring attack.
            bossScript.DeactivateAttackByIndex(0);

            firstTimeEnteringThisState = true;
            timeInRing = 0;
            BellAnim.lockDirection = false;
            //Choose the next attack now to determine the required distance.
            attacksSinceLastIdle = 0;
            currentState = SpearBellStates.idle;
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
            //ActivateAnimationByState(currentState);
        }
    }

    private void HandleSeekingState()
    {
        if (firstTimeEnteringThisState)
        {
            if (nextAttackState == SpearBellStates.ring)
            {
                //Target the arena center during the spawner ring attack.
                target = RingLocation;
                UpdatePath();
            }
            firstTimeEnteringThisState = false;
        }

        bool distAtualMenorDistTarget = Math.Abs(Vector2.Distance(transform.position, target.position)) < GetNextAttackDistance(nextAttackState);
        if (distAtualMenorDistTarget && timeInSeeking >= spearBellData.minimumTimeOnState)
        {
            firstTimeEnteringThisState = true;
            timeInSeeking = 0;
            currentState = SpearBellStates.alert;

            //Set the Animator to the new state.

            TargetPlayer();
            OnChangeState?.Invoke();
            //ActivateAnimationByState(currentState);
        }
    }

    private void HandleIdleState()
    {
        if (timeInIdle <= 0 || firstTimeEnteringThisState)
        {

            maxAtThisState = Random.Range(spearBellData.idle.RangesPerForm[currentLocalForm].Ranges[0],
                spearBellData.idle.RangesPerForm[currentLocalForm].Ranges[1]);


            //Read the state's range for the current form and the requested upper or lower limit.
            firstTimeEnteringThisState = false;
        }
        if (timeInIdle >= maxAtThisState && timeInIdle >= spearBellData.minimumTimeOnState)
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
                nextAttackState = SpearBellStates.ring;
            }

            currentState = SpearBellStates.seeking;
            //Set the Animator to the new state.
            OnChangeState?.Invoke();
            //ActivateAnimationByState(currentState);
        }
    }


    protected override Vector2 GetDesiredDirection()
    {
        if (currentState == SpearBellStates.seeking)
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
            if (currentState == SpearBellStates.seeking && nextAttackState == SpearBellStates.ring)
            {
               return spearBellData.GetSpeed(currentState, nextAttackState) * spearBellData.ringSpeedMultiplier * localSpeedIncreaseMod;
            }
            else
            {
                return spearBellData.GetSpeed(currentState, nextAttackState) * localSpeedIncreaseMod;

            }


        }
        else
        {
            return spearBellData.GetSpeed(currentState, nextAttackState) * enemyData.speedMultiplierWhenNotVisible;
        }
    }


    /// <summary>
    /// Initialize the mapping between boss forms and available attacks.
    /// </summary>
    protected void InitiateDictionary()
    {
        unlockedAttacks = new Dictionary<int, (int, SpearBellStates)[]>();
        (int, SpearBellStates)[] form1 =
            {
            (spearBellData.block.WeightsPerForm[0], spearBellData.block.state),
            (spearBellData.shoot.WeightsPerForm[0], spearBellData.shoot.state),
            };
        unlockedAttacks.Add(0, form1);
        (int, SpearBellStates)[] form2 =
            {
            (spearBellData.block.WeightsPerForm[1], spearBellData.block.state),
            (spearBellData.shoot.WeightsPerForm[1], spearBellData.shoot.state),
            };
        unlockedAttacks.Add(1, form2);
        (int, SpearBellStates)[] form3 =
            {                      
            (spearBellData.block.WeightsPerForm[2], spearBellData.block.state),
            (spearBellData.shoot.WeightsPerForm[2], spearBellData.shoot.state),
            };
        unlockedAttacks.Add(2, form3);
        (int, SpearBellStates)[] form4 =
    {
            (spearBellData.block.WeightsPerForm[3], spearBellData.block.state),
            (spearBellData.shoot.WeightsPerForm[3], spearBellData.shoot.state),
            };
        unlockedAttacks.Add(3, form4);
    }


    /// <summary>
    /// Select a state using its configured weights.
    /// </summary>
    /// <param name="currentState">Current boss state.</param>
    /// <returns></returns>
    protected SpearBellStates NextWeightedRandomState()
    {
        //unlockedAttacks[nextFormScript.currentForm];
        // Step 1: Normalize probabilities.
        float totalProbability = 0f;
        foreach ((int, SpearBellStates) targetState in unlockedAttacks[currentLocalForm])
        {

            totalProbability += targetState.Item1;
        }

        // Ensure the total is greater than zero.
        if (totalProbability <= 0)
        {
            Debug.LogError("As probabilidades de estados devem ser maiores que 0.");
            return SpearBellStates.idle;
        }

        // Step 2: Draw a random value within the total probability weight.
        float randomValue = Random.Range(0, totalProbability);

        // Step 3: Find the corresponding attack.
        float cumulativeProbability = 0f;
        foreach ((int, SpearBellStates) targetState in unlockedAttacks[currentLocalForm])
        {

            cumulativeProbability += targetState.Item1;

            if (randomValue <= cumulativeProbability)
            {
                return targetState.Item2;
            }
        }

        // Return null if selection fails.
        Debug.LogError("Nenhum estado foi selecionado. Verifique as probabilidades :c.");
        return SpearBellStates.idle;
    }

    protected float GetNextAttackDistance(SpearBellStates targetState)
    {
        switch (targetState)
        {
            case SpearBellStates.ring:
                return spearBellData.ring.DistancePerForm[currentLocalForm];
            case SpearBellStates.block:
                return spearBellData.block.DistancePerForm[currentLocalForm];
            case SpearBellStates.shoot:
                return spearBellData.shoot.DistancePerForm[currentLocalForm];
            default:
                return 0;
        }
    }

    /// <summary>
    /// Read each attack's alert time from spearBellData.
    /// </summary>
    /// <returns></returns>
    protected float GetNextAttackAlertTime(SpearBellStates targetState)
    {
        switch (targetState)
        {
            case SpearBellStates.ring:
                return spearBellData.ring.AntecipationTimePerForm[currentLocalForm];
            case SpearBellStates.block:
                return spearBellData.block.AntecipationTimePerForm[currentLocalForm];
            case SpearBellStates.shoot:
                return spearBellData.shoot.AntecipationTimePerForm[currentLocalForm];
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
        if (spearBellData.maxAttackWithoutIdle[currentLocalForm] > attacksSinceLastIdle)
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
    /// </summary>
    protected void NextNonAttackState() 
    {
        //An attack just ended; increment the attack count.
        ++attacksSinceLastIdle;
        if (CheckIsTimeToIdle())
        {
            attacksSinceLastIdle = 0;
            currentState = SpearBellStates.idle;
        }
        else
        {
            if (!needsToRing)
            {
                nextAttackState = GetNextAttackWithoutRepetition();
            }
            else
            {
                nextAttackState = SpearBellStates.ring;
            }
            currentState = SpearBellStates.seeking;
        }

    }

    /// <summary>
    /// Notify the bell to move to the arena center for the ring attack.
    /// </summary>
    protected void NeedsToRing() 
    {
        needsToRing = true;
    }

    public SpearBellStates GetCurrentState()
    {
        return currentState;
    }

    public SpearBellStates GetNextAttackState()
    {
        return nextAttackState;
    }


    /// <summary>
    /// Enter the scream state when the boss changes form; apply form-specific stat changes here.
    /// </summary>
    public void ChangedForm()
    {
        currentLocalForm = bellManager.currentBellForm;
        localSpeedIncreaseMod = (float)Math.Pow(bellManager.GetSpeedIncrease(), currentLocalForm);
    }


    /// <summary>
    ///Also used when taking damage while preparing to fire.
    ///Enter seeking and restart the cooldown timer.
    /// </summary>
    public void GoToIdle()
    {
        ResetVariables();
        timeInIdle = 0;
        currentState = SpearBellStates.idle;
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
        //currentState = SpearBellStates.idle;
        timeInIdle = 0f;
        timeInSeeking = 0f;
        timeInShoot = 0f;
        timeInRing = 0f;
        timeInStun = 0f;
        timeInBlock = 0f;
        bossScript.DeactivateAllAttacks();
        //DisableLaserByAnim();
        blockGameObject.SetActive(false);
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
    public void MaxEnqueue(SpearBellStates targetState)
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

        SpearBellStates[] tempStates = lastAttackStates.ToArray();

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
    protected SpearBellStates GetNextAttackWithoutRepetition()
    {
        SpearBellStates possibleNextState = NextWeightedRandomState();
        SpearBellStates[] tempStates = lastAttackStates.ToArray();
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
        if (spearBellData.stunCoolDown[currentLocalForm] <= (Time.time - timeOfLastStun))
        {
            //If currently in alert,
            if (currentState == SpearBellStates.alert)
            {
                switch (nextAttackState)
                {
                    case SpearBellStates.ring:

                        if (spearBellData.ring.canBeStunned)
                        {
                            timeOfLastStun = Time.time;
                            ResetVariables();
                            currentState = SpearBellStates.stun;
                            OnChangeState?.Invoke();
                        }

                        break;
                    case SpearBellStates.block:
                        if (spearBellData.block.canBeStunned)
                        {
                            timeOfLastStun = Time.time;
                            ResetVariables();
                            currentState = SpearBellStates.stun;
                            OnChangeState?.Invoke();
                        }
                        break;
                    case SpearBellStates.shoot:
                        if (spearBellData.shoot.canBeStunned)
                        {
                            timeOfLastStun = Time.time;
                            ResetVariables();
                            currentState = SpearBellStates.stun;
                            OnChangeState?.Invoke();
                        }
                        break;
                    default:

                        break;

                }
            }
        }
        
    }


    /// <summary>
    /// Called on the animation frame that spawns bullets.
    /// </summary>
    public void SpawnBulletsByAnimation()
    {

       attackPlayer.InstantiateProjectile(false,spearBellData.bulletStatsPerForm.bulletsPerShot[currentLocalForm],
           spearBellData.bulletStatsPerForm.projectileSpeed[currentLocalForm], spearBellData.bulletStatsPerForm.multipleBulletAngleOffset[currentLocalForm]);
        
    }
}
