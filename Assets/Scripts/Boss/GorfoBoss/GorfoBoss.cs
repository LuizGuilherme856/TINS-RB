using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;
using UnityEngine;
using static SwordBellBossAI;
using Random = UnityEngine.Random;

public class GorfoBoss : BossEnemy
{

    #region Structs
    //Stores an enemy state, its probability, and its ranges.
    [Serializable]
    public struct StatesAndWeight
    {
        //State.
        public gorfoStates state;

        [Tooltip("O quanto a probabildiade normal desse estado occorrer sera multiplicado caso ele for ocorrer novamente")]
        [SerializeField][Range(0, 1)] public float sameStateMod;
        //Attack probability weight for this boss form.
        [Tooltip("Peso da probabilidade desse estado em relação a forma do boss")]
        [SerializeField][Range(0, 50)] public List<int> WeightsPerForm;

        //The following matrix defines ranges for each form, for example:
        /*     Start     End
         form1 [  0    ,   5 ]
         form2 [  2    ,   7 ]
         form3 [  4    ,   9 ]
         */
        [Tooltip("Lista abaixo diz os ranges de acordo com a forma exemplo:\r\n                Inicio    Fim \r\n         form1 [  0    ,   5 ]\r\n         form2 [  2    ,   7 ]\r\n         form3 [  4    ,   9 ]")]
        [SerializeField] public List<Range> RangesPerForm;
        //When enabled, limits count iterations; otherwise, they measure time.
        // When enabled, call AddIterationToAttack at the end of the animation.
        [SerializeField] public bool usesIterationCount;
        [SerializeField] public bool copyFirstRangeToAll;
        public void CopyFirstRangeToAll()
        {
            if (copyFirstRangeToAll)
            {
                for (int i = 1; i < RangesPerForm.Count; i++)
                {
                    RangesPerForm[i] = RangesPerForm[0];
                }
            }
        }
    }

    //Exposes the data for editing and inspection in the Editor.
    [Serializable]
    public struct Range
    {
        [SerializeField][Range(0, 50)] public List<float> Ranges;
    }


    //Bullets Related
    [Serializable]
    public struct BulletStatsPerForm
    {
        [Tooltip("Status Da Bala em relação a forma do boss")]
        [SerializeField][Range(0, 50)] public List<int> bulletsPerShot, projectileSpeed, multipleBulletAngleOffset;

    }

    //Scream Related
    [Serializable]
    public struct ScreamStatsPerFormStruct
    {
        [Tooltip("Velocidade da passada do angulo do ScreamAttack em relação a forma do boss")]
        [SerializeField][Range(0, 50)] public List<int> speedPerForm;
    }

    #endregion

    //Editor related
    [SerializeField] protected bool updateStructs = false;
    //State Related
    public enum gorfoStates { idle, light, mediumStomp, medium, heavy, biceps }

    [SerializeField][Range(0, 1)] protected float mediumLoopProb = 0.3f;

    //Prevents repeated boss attacks.
    protected int maxAttackRepetition = 3;

    [SerializeField] protected Queue<gorfoStates> lastAttackStates = new Queue<gorfoStates>(3);

    protected int attacksSinceLastIdle = 0;

    [Tooltip("Maximo de ataques seguidos em relação a Forma")]
    [SerializeField][Range(0, 50)] public List<int> maxAttackWithoutIdle;


    [Header("Attack Structs")]
    //StatesAndWeights structures for each state.
    [SerializeField] protected StatesAndWeight idle = new StatesAndWeight();
    [SerializeField] protected StatesAndWeight biceps = new StatesAndWeight();
    [SerializeField] protected StatesAndWeight lightAttack = new StatesAndWeight();
    [SerializeField] protected StatesAndWeight mediumStompAttack = new StatesAndWeight();
    [SerializeField] protected StatesAndWeight mediumAttack = new StatesAndWeight();
    [SerializeField] protected StatesAndWeight heavyAttack = new StatesAndWeight();



    protected bool firstTimeEnteringThisState = true;

    [SerializeField] protected gorfoStates currentState = gorfoStates.idle;

    //Maps each boss phase to its available attacks.
    Dictionary<int, (int, gorfoStates)[]> unlockedAttacks = new Dictionary<int, (int, gorfoStates)[]>();


    //Status Related
    
    [Header("Bullets Related")]
    [SerializeField] protected BulletStatsPerForm bulletStatsPerForm = new BulletStatsPerForm();

    //ScreamAttack Related
    
    [Header("ScreamAttack Related")]
    [SerializeField] protected ScreamStatsPerFormStruct ScreamStatsPerForm = new ScreamStatsPerFormStruct();

    //Linear attack automator used to change the attack's rotation speed.
    //[SerializeField] protected AutomateLinear variaAutomate;


    [SerializeField] protected EnemyData gorfoData; // Assigned in the Editor.



    //Spawn projectile with target on player related
    protected AttackPlayer attackPlayer;
    [SerializeField] protected bool useEnemyDataForBullets = false;
    public MMF_Player throwFeedback;



    //Timer Related
    [Header("Timers")]
    [SerializeField] protected float timeInIdle = 0f;
    [SerializeField] protected float timeInBiceps = 0f;
    [SerializeField] protected float timeInLight = 0f;
    [SerializeField] protected float timeInMedium = 0f;
    [SerializeField] protected float timeInMediumStomp = 0f;
    [SerializeField] protected float timeInHeavy = 0f;

    [SerializeField] protected int iterationsAtCurrentState = 0;

    [SerializeField] protected float maxAtThisState = 0f;

    [SerializeField] protected int currentLightIteration = 0;
    [SerializeField] protected int currentMediumIteration = 0;
    [SerializeField] protected int currentMediumStompIteration = 0;
    [SerializeField] protected int currentHeavyIteration = 0;


    //Animation Related
    protected Animator animator;

    //Explosion Related
    [SerializeField] protected Explosion explosion; // Assigned in the Editor.



    protected override void Awake()
    {
        base.Awake();
        //Try catchers

        try
        {
            animator = GetComponent<Animator>();
        }
        catch (System.Exception)
        {
            Debug.LogError("Gorfo boss não tem um animator");
        }
        try
        {
            attackPlayer = GetComponent<AttackPlayer>();
        }
        catch (System.Exception)
        {
            Debug.LogError("Gorfo boss não tem um AttackPlayer");
        }

        //Dictionary related
        InitiateDictionary();

        CheckStatesStructs();
        nextFormScript.OnChangeForm += ChangedForm;

    }

    protected void Update()
    {
        switch (currentState)
        {
            case gorfoStates.idle:
                if (!idle.usesIterationCount)
                    timeInIdle += Time.deltaTime;
                break;
            case gorfoStates.light:
                if (!lightAttack.usesIterationCount)
                    timeInLight += Time.deltaTime;
                break;
            case gorfoStates.mediumStomp:
                if (!mediumStompAttack.usesIterationCount)
                    timeInMediumStomp += Time.deltaTime;
                break;
            case gorfoStates.medium:
                if (!mediumAttack.usesIterationCount)
                    timeInMedium += Time.deltaTime;
                break;
            case gorfoStates.heavy:
                if (!heavyAttack.usesIterationCount)
                    timeInHeavy += Time.deltaTime;
                break;
            case gorfoStates.biceps:
                if (!biceps.usesIterationCount)
                    timeInBiceps += Time.deltaTime;
                break;
            default:
                break;
        }
    }
    protected void FixedUpdate()
    {
        StateMachine();
        if (updateStructs)
        {
            InitiateDictionary();
            CheckStatesStructs();
            updateStructs = false;
        }
    }

    //Reset timers whenever leaving the state.
    override protected void StateMachine()
    {
        switch (currentState)
        {
            case gorfoStates.idle:
                HandleIdleState();
                break;
            case gorfoStates.light:
                HandleLightState();

                break;
            case gorfoStates.mediumStomp:

                HandleMediumStompState();

                break;
            case gorfoStates.medium:
                HandleMediumState();
                break;
            case gorfoStates.heavy:
                HandleHeavyState();

                break;
            case gorfoStates.biceps:

                HandleBicepsState();
                break;
            default:
                break;
        }

    }


    protected void HandleIdleState()
    {
        if (timeInIdle <= 0 || firstTimeEnteringThisState)
        {
            //Read the state's range for the current form and the requested upper or lower limit.
            maxAtThisState = Random.Range(idle.RangesPerForm[currentForm].Ranges[0], idle.RangesPerForm[currentForm].Ranges[1]);
            firstTimeEnteringThisState = false;
        }
        if (timeInIdle >= maxAtThisState)
        {
            firstTimeEnteringThisState = true;
            timeInIdle = 0;
            //currentState = NextWeightedRandomState(idle); // OLD
            currentState = GetNextAttackWithoutRepetition(idle);
            //Set the Animator to the new state.
            ActivateAnimationByState(currentState);
        }
    }

    protected void HandleLightState()
    {
        //If enabled, use iteration counts.
        if (lightAttack.usesIterationCount)
        {

            if (iterationsAtCurrentState <= 0 || firstTimeEnteringThisState)
            {
                maxAtThisState = (int)Math.Round(Random.Range(lightAttack.RangesPerForm[currentForm].Ranges[0], lightAttack.RangesPerForm[currentForm].Ranges[1]));
                firstTimeEnteringThisState = false;
            }
            if (iterationsAtCurrentState >= maxAtThisState)
            {
                firstTimeEnteringThisState = true;
                iterationsAtCurrentState = 0;
                timeInLight = 0;
                currentState = gorfoStates.idle;
                //Set the Animator to the new state.
                ActivateAnimationByState(currentState);
            }

        }
        else
        {
            if (timeInLight <= 0 || firstTimeEnteringThisState)
            {
                maxAtThisState = Random.Range(lightAttack.RangesPerForm[currentForm].Ranges[0], lightAttack.RangesPerForm[currentForm].Ranges[1]);
                firstTimeEnteringThisState = false;
            }
            if (timeInLight >= maxAtThisState)
            {
                firstTimeEnteringThisState = true;
                iterationsAtCurrentState = 0;
                timeInLight = 0;
                currentState = gorfoStates.idle;
                //Set the Animator to the new state.
                ActivateAnimationByState(currentState);
            }
        }
    }

    protected void HandleMediumStompState()
    {
        if (mediumStompAttack.usesIterationCount)
        {
            if (iterationsAtCurrentState <= 0 && firstTimeEnteringThisState)
            {
                //TODO: Review this section; a known error remains.
                maxAtThisState = (int)Math.Round(Random.Range(mediumStompAttack.RangesPerForm[currentForm].Ranges[0], mediumStompAttack.RangesPerForm[currentForm].Ranges[1])); //Known issue: fixed duration causes one extra iteration; discuss with Luiz.
                firstTimeEnteringThisState = false;
            }
            if (iterationsAtCurrentState >= maxAtThisState)
            {
                firstTimeEnteringThisState = true;
                iterationsAtCurrentState = 0;
                timeInMediumStomp = 0;
                currentState = gorfoStates.medium;
                //Set the Animator to the new state.
                ActivateAnimationByState(currentState);
            }


        }
        else
        {
            if (timeInMediumStomp <= 0 || firstTimeEnteringThisState)
            {
                maxAtThisState = Random.Range(mediumStompAttack.RangesPerForm[currentForm].Ranges[0], mediumStompAttack.RangesPerForm[currentForm].Ranges[1]);
                firstTimeEnteringThisState = false;
            }
            if (timeInMediumStomp >= maxAtThisState)
            {
                firstTimeEnteringThisState = true;
                timeInMediumStomp = 0;
                iterationsAtCurrentState = 0;
                currentState = gorfoStates.medium;
                //Set the Animator to the new state.
                ActivateAnimationByState(currentState);
            }
        }
    }

    protected void HandleMediumState()
    {
        if (mediumAttack.usesIterationCount)
        {
            if (iterationsAtCurrentState <= 0 || firstTimeEnteringThisState)
            {
                maxAtThisState = (int)Math.Round(Random.Range(mediumAttack.RangesPerForm[currentForm].Ranges[0], mediumAttack.RangesPerForm[currentForm].Ranges[1])); //Known issue: fixed duration causes one extra iteration; discuss with Luiz.
                firstTimeEnteringThisState = false;
            }
            if (iterationsAtCurrentState >= maxAtThisState)
            {
                firstTimeEnteringThisState = true;
                iterationsAtCurrentState = 0;
                timeInMedium = 0;
                //Set the Animator to the new state.

                if (Random.Range(0f, 1f) <= mediumLoopProb)
                {
                    currentState = gorfoStates.mediumStomp;
                }
                else
                {
                    currentState = gorfoStates.idle;
                }

                ActivateAnimationByState(currentState);
            }

        }
        else
        {
            if (timeInMedium <= 0 || firstTimeEnteringThisState)
            {
                maxAtThisState = Random.Range(mediumAttack.RangesPerForm[currentForm].Ranges[0], mediumAttack.RangesPerForm[currentForm].Ranges[1]);
                firstTimeEnteringThisState = false;
            }
            if (timeInMedium >= maxAtThisState)
            {
                firstTimeEnteringThisState = true;
                iterationsAtCurrentState = 0;
                timeInMedium = 0;

                if (Random.Range(0f, 1f) <= mediumLoopProb)
                {
                    currentState = gorfoStates.mediumStomp;
                }
                else
                {
                    currentState = gorfoStates.idle;
                }
                //Set the Animator to the new state.
                ActivateAnimationByState(currentState);
            }
        }
    }

    protected void HandleHeavyState()
    {
        if (heavyAttack.usesIterationCount)
        {
            if (iterationsAtCurrentState <= 0 || firstTimeEnteringThisState)
            {
                maxAtThisState = (int)Math.Round(Random.Range(heavyAttack.RangesPerForm[currentForm].Ranges[0], heavyAttack.RangesPerForm[currentForm].Ranges[1])); //Known issue: fixed duration causes one extra iteration; discuss with Luiz.
                firstTimeEnteringThisState = false;
            }
            if (iterationsAtCurrentState >= maxAtThisState)
            {
                firstTimeEnteringThisState = true;
                iterationsAtCurrentState = 0;
                timeInHeavy = 0;
                currentState = gorfoStates.idle;
                //Set the Animator to the new state.
                ActivateAnimationByState(currentState);
            }


        }
        else
        {
            if (timeInHeavy <= 0 || firstTimeEnteringThisState)
            {
                maxAtThisState = Random.Range(heavyAttack.RangesPerForm[currentForm].Ranges[0], heavyAttack.RangesPerForm[currentForm].Ranges[1]);
                firstTimeEnteringThisState = false;
            }
            if (timeInHeavy >= maxAtThisState)
            {
                firstTimeEnteringThisState = true;
                timeInHeavy = 0;
                currentState = gorfoStates.idle;
                //Set the Animator to the new state.
                ActivateAnimationByState(currentState);
            }
        }
    }

    protected void HandleBicepsState()
    {
        if (timeInBiceps <= 0 || firstTimeEnteringThisState)
        {
            maxAtThisState = Random.Range(biceps.RangesPerForm[currentForm].Ranges[0], biceps.RangesPerForm[currentForm].Ranges[1]);
            firstTimeEnteringThisState = false;
        }
        if (timeInBiceps >= maxAtThisState)
        {
            firstTimeEnteringThisState = true;
            timeInBiceps = 0;
            //currentState = NextWeightedRandomState(biceps); //OLD
            currentState = GetNextAttackWithoutRepetition(biceps);
            //Set the Animator to the new state.
            ActivateAnimationByState(currentState);


        }
    }





    /// <summary>
    /// Reset all state-related variables.
    /// </summary>
    protected void ResetVariables()
    {
        //currentState = SwordBellStates.idle;
        //Timers
        timeInIdle = 0f;
        timeInBiceps = 0f;
        timeInLight = 0f;
        timeInMedium = 0f;
        timeInMediumStomp = 0f;
        timeInHeavy = 0f;

        //Iterations
        iterationsAtCurrentState = 0;
        currentLightIteration = 0;
        currentMediumIteration = 0;
        currentMediumStompIteration = 0;
        currentHeavyIteration = 0;

        DeactivateAllAttacks();
        firstTimeEnteringThisState = true;

    }

    /// <summary>
    /// Initialize the mapping between boss forms and available attacks.
    /// </summary>
    protected void InitiateDictionary()
    {
        unlockedAttacks = new Dictionary<int, (int, gorfoStates)[]>();
        (int, gorfoStates)[] form1 =
{
            //(idle.WeightsPerForm[0], idle.state),
            //(biceps.WeightsPerForm[0], biceps.state),
            (lightAttack.WeightsPerForm[0], lightAttack.state),
            (mediumStompAttack.WeightsPerForm[0], mediumStompAttack.state),
        };
        unlockedAttacks.Add(0, form1);
        (int, gorfoStates)[] form2 =
{           //(idle.WeightsPerForm[1], idle.state),
            //(biceps.WeightsPerForm[1], biceps.state),
            (lightAttack.WeightsPerForm[1], lightAttack.state),
            (mediumStompAttack.WeightsPerForm[1], mediumStompAttack.state),
        };
        unlockedAttacks.Add(1, form2);
        (int, gorfoStates)[] form3 =
{           //(idle.WeightsPerForm[2], idle.state),
            //(biceps.WeightsPerForm[2], biceps.state),
            (lightAttack.WeightsPerForm[2], lightAttack.state),
            (mediumStompAttack.WeightsPerForm[2], mediumStompAttack.state),
            (heavyAttack.WeightsPerForm[2], heavyAttack.state),
        };
        unlockedAttacks.Add(2, form3);
    }


    /// <summary>
    /// Check whether it is time to enter idle.
    /// </summary>
    /// <returns></returns>
    protected bool CheckIsTimeToIdle()
    {
        if (maxAttackWithoutIdle[currentForm] > attacksSinceLastIdle)
        {
            return false;
        }
        else
        {
            return true;
        }

    }

    /// <summary>
    /// Choose biceps or idle after an attack state finishes.
    /// Also select the next attack now.
    /// </summary>
    protected gorfoStates NextState(StatesAndWeight targetState)
    {
        //An attack just ended; increment the attack count.
        ++attacksSinceLastIdle;
        if (CheckIsTimeToIdle())
        {
            attacksSinceLastIdle = 0;
            if (Random.Range(0, 2) > 0)
            {
                return gorfoStates.idle;
            }
            else 
            {
                return gorfoStates.biceps;
            }
            
        }
        else
        {
            return GetNextAttackWithoutRepetition(targetState);
        }

    }



    /// <summary>
    /// Copy the first form's time or iteration limits to the other boss forms.
    /// </summary>
    protected void CheckStatesStructs()
    {
        idle.CopyFirstRangeToAll();
        biceps.CopyFirstRangeToAll();
        lightAttack.CopyFirstRangeToAll();
        mediumStompAttack.CopyFirstRangeToAll();
        mediumAttack.CopyFirstRangeToAll();
        heavyAttack.CopyFirstRangeToAll();
    }


    //Attack Queue related
    /// <summary>
    /// Enqueue up to three states.
    /// </summary>
    /// <param name="targetState"></param>
    public void MaxEnqueue(gorfoStates targetState)
    {
        if (lastAttackStates.Count == 2)
        {
            lastAttackStates.Dequeue(); // Remove the oldest entry to keep two items.
        }
        lastAttackStates.Enqueue(targetState);
    }

    /// <summary>
    /// Check whether the last two attacks were identical.
    /// </summary>
    /// <returns></returns>
    protected bool CheckStateRepetion()
    {


        if (lastAttackStates.Count < 2) return false; // Require two entries.

        gorfoStates[] tempStates = lastAttackStates.ToArray();

        if (tempStates[0] == tempStates[1]) 
        {
            //Debug.LogWarning("True");
            return true;
        }
        return false;

    }

    /// <summary>
    /// Choose the next attack without repeating one already used three consecutive times.
    /// </summary>
    /// <returns></returns>
    protected gorfoStates GetNextAttackWithoutRepetition(StatesAndWeight targetState)
    {
        gorfoStates possibleNextState = NextWeightedRandomState(targetState);
        gorfoStates[] tempStates = lastAttackStates.ToArray();
        int iterationCount = 0;

        //For Debug
        /*
        if (lastAttackStates.Count >= 2)
        {
            tempStates = lastAttackStates.ToArray();
            //Debug.LogWarning("Attacks in queue: " + tempStates[0] + " " + tempStates[1] + " " + tempStates[2]+" Repetition ? "+ CheckStateRepetion());
            Debug.LogWarning("Attacks in queue: " + tempStates[0] + " " + tempStates[1] + " Repetition ? " + CheckStateRepetion());
        }
        */

        //If a state has occurred three times in a row,
        if (CheckStateRepetion())
        {
            //keep drawing until a different state is selected.
            while (possibleNextState == tempStates[0])
            {
                possibleNextState = NextWeightedRandomState(targetState);
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
    /// Select a state using its configured weights.
    /// </summary>
    /// <param name="currentState">Current boss state.</param>
    /// <returns></returns>
    protected gorfoStates NextWeightedRandomState(StatesAndWeight currentState)
    {
        //unlockedAttacks[nextFormScript.currentForm];
        // Step 1: Normalize probabilities.
        float totalProbability = 0f;
        foreach ((int, gorfoStates) targetState in unlockedAttacks[nextFormScript.currentForm])
        {
            if (targetState.Item2 == currentState.state)
            {
                totalProbability += targetState.Item1 * currentState.sameStateMod;
            }
            else
            {
                totalProbability += targetState.Item1;
            }
        }

        // Ensure the total is greater than zero.
        if (totalProbability <= 0)
        {
            Debug.LogError("As probabilidades de estados devem ser maiores que 0.");
            return gorfoStates.idle;
        }

        // Step 2: Draw a random value within the total probability weight.
        float randomValue = Random.Range(0, totalProbability);

        // Step 3: Find the corresponding enemy.
        float cumulativeProbability = 0f;
        foreach ((int, gorfoStates) targetState in unlockedAttacks[nextFormScript.currentForm])
        {
            if (targetState.Item2 == currentState.state)
            {
                cumulativeProbability += targetState.Item1 * currentState.sameStateMod;
            }
            else
            {
                cumulativeProbability += targetState.Item1;
            }
            if (randomValue <= cumulativeProbability)
            {
                return targetState.Item2;
            }
        }

        // Return null if selection fails.
        Debug.LogError("Nenhum estado foi selecionado. Verifique as probabilidades :c.");
        return gorfoStates.idle;


    }


    /// <summary>
    /// Clear all animation triggers, then activate the requested one.
    /// </summary>
    /// <param name="targetState">The state that was just entered.</param>
    protected void ActivateAnimationByState(gorfoStates targetState)
    {
        //Try setting only the requested trigger without resetting the others first.
        switch (targetState)
        {
            case gorfoStates.idle:
                Aux.ResetAllAnimationTriggers(animator);
                animator.SetTrigger("Idle");
                break;
            case gorfoStates.light:
                Aux.ResetAllAnimationTriggers(animator);
                animator.SetTrigger("Light");
                break;
            case gorfoStates.mediumStomp:
                Aux.ResetAllAnimationTriggers(animator);
                animator.SetTrigger("MediumStomp");
                break;
            case gorfoStates.medium:
                Aux.ResetAllAnimationTriggers(animator);
                animator.SetTrigger("Medium");
                break;
            case gorfoStates.heavy:
                Aux.ResetAllAnimationTriggers(animator);
                animator.SetTrigger("Heavy");
                break;
            case gorfoStates.biceps:
                Aux.ResetAllAnimationTriggers(animator);
                animator.SetTrigger("PrepareBiceps");
                break;
            default:
                break;
        }
    }

    public void TriggerExplosionByAnimation()
    {
        explosion.TriggerExplosion();
    }

    public void SpawnBulletsByAnimation()
    {

        if (useEnemyDataForBullets)
        {
            attackPlayer.InstantiateProjectile();
        }
        else
        {
            attackPlayer.InstantiateProjectile(false, bulletStatsPerForm.bulletsPerShot[currentForm], 
                bulletStatsPerForm.projectileSpeed[currentForm], bulletStatsPerForm.multipleBulletAngleOffset[currentForm]);
        }

        throwFeedback.PlayFeedbacks();
    }
    public void AddIterationByAnimation()
    {
        iterationsAtCurrentState += 1;
    }

    /// <summary>
    /// Enter biceps when the boss changes form; apply form-specific stat changes here.
    /// </summary>
    public void ChangedForm()
    {
        ResetVariables();
        firstTimeEnteringThisState = true;
        currentState = gorfoStates.biceps;

        animator.SetTrigger("PrepareBiceps");
        ResetTriggersButTranformAndBiceps();

        //Increase stats here.
    }
    protected void ResetTriggersButTranformAndBiceps()
    {
        animator.ResetTrigger("Idle");

        animator.ResetTrigger("Light");

        animator.ResetTrigger("MediumStomp");

        animator.ResetTrigger("Medium");

        animator.ResetTrigger("Heavy");
    }
    public void TriggerMediumByFormOnAnimation()
    {
        if (currentForm >= 1)
        {
            AttackByName("RockMediumFase3");
        }
        else
        {
            AttackByName("RockMediumFase2");
        }
    }



}
