using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using TechBooh;
using Unity.VisualScripting;
using UnityEngine;
using static PsicoPatoData;
using Random = UnityEngine.Random;
using Transform = UnityEngine.Transform;

public class PsicoPatoBoss : EnemyAI
{
    //State Related
    public enum PsicoPatoStates { idle, seeking, scream, dash, egg, laser ,shoot };
    [SerializeField] protected PsicoPatoStates currentState = PsicoPatoStates.seeking;
    //Select the next attack and prepare the state machine.
    protected PsicoPatoStates nextAttackState = PsicoPatoStates.idle;
    protected Queue<PsicoPatoStates> lastAttackStates = new Queue<PsicoPatoStates>(3);

    //Data Related
    protected PsicoPatoData psicoData;

    [Header("  Feedbacks  ")]
    [SerializeField] protected MMF_Player dashFeedback;
    [SerializeField] protected MMF_Player dashPrepareFeedback;
    [SerializeField] protected MMF_Player laserFeedback, laserEndFeedback;
    [SerializeField] protected MMF_Player  shootingEndFeedback;
    [SerializeField] protected MMF_Player eggFeedback;

    //Maps each boss phase to its available attacks.
    Dictionary<int, (int, PsicoPatoStates)[]> unlockedAttacks = new Dictionary<int, (int, PsicoPatoStates)[]>();

    #region debug

    //Timer Related
    [Header("Timers")]
    [SerializeField] protected float timeInIdle = 0f;
    [SerializeField] protected float timeInSeeking = 0f;
    [SerializeField] protected float timeInShoot = 0f;
    [SerializeField] protected float timeInDash = 0f;
    [SerializeField] protected float timeInLaser = 0f;
    [SerializeField] protected float timeInEgg = 0f;
    [SerializeField] protected float timeInScream = 0f;


    [SerializeField] protected int iterationsAtCurrentState = 0;

    [SerializeField] protected float maxAtThisState = 0f;

    [SerializeField] protected int currentLaserIteration = 0;
    [SerializeField] protected int currentDashIteration = 0;
    [SerializeField] protected int currentEggteration = 0;

    protected bool firstTimeEnteringThisState = true;

    #endregion

    //Boos Scripts Related
    [SerializeField] protected BossEnemy bossScript;
    [SerializeField] protected BossNextForm nextFormScript;

    //Targeting Related
    [SerializeField] protected Transform playerTransform;
    [SerializeField] protected Transform currentTargetTransform;
    [SerializeField] protected bool targetingPlayer = false;

    //Events
    public event Action OnChangeState;

    //Animator
    [SerializeField] protected PsicoPatoMovementAnimations PsicoAnim;

    //Health Related
    [SerializeField] protected EnemyHealth PsicoHealth;


    //Not Varia Attacking Related
    protected AttackPlayer attackPlayer;

    //Dash Related
    //Impulse related
    protected Impulse impulseScript;
    protected bool dashed = false;

    //Preview Related
    //Experimental Sprite Shape implementation.
    //[SerializeField] protected SpriteShapeManager spriteShapeManager; // Assigned in the Editor.
    [SerializeField] protected GameObject dashPreviewObject; // Assigned in the Editor.

    //Laser Related
    //[SerializeField] protected GameObject laserGameObject; //Assigned in the Editor.
    [SerializeField] protected MMF_Player laserAttack_2, laserAttack_3; //Assigned in the Editor.
    private MMF_Rotation mmfRotationLaser_2, mmfRotationLaser_3;
    private MMF_Scale mmfScaleLaser_3;

    //Shoot Related
    //0 = Esquerda, 1= direita
    protected int chosenSpawn = 0;
    [SerializeField] protected Transform leftShootLocation, rightShootLocation, centerOfArenaLocation;
    protected MoveObjectToPos moveObjToPosScript;

    //Egg Related
    protected PlayMMF_PlayerFeedbacks PlayMMF_Player;
    protected float timeOfLastEgg = 0;
    
    protected override void Awake()
    {
        
        base.Awake();
        try
        {
            psicoData = (PsicoPatoData)enemyData;
        }
        catch (Exception)
        {
            Debug.LogError("enemyData n�o � uma instancia de PsicoPatoData! Coloque um PsicoPatorData na referencia 'Enemy Data'");
        }

        try
        {
            PsicoHealth = this.gameObject.GetComponent<EnemyHealth>();
            PsicoHealth.OnZeroHealth += ResetVariables;
        }
        catch (Exception)
        {
            Debug.LogError("PsicoPato n�o possui um script EnemyHealth");
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
            Debug.LogError("PsicoPato  n�o tem um AttackPlayer.cs");
        }
        try
        {
            PsicoAnim = GetComponent<PsicoPatoMovementAnimations>();
        }
        catch (System.Exception)
        {
            Debug.LogError("PsicoPato  n�o tem um PsicoPatoMovementAnimation.cs");
        }
        try
        {
            impulseScript = GetComponent<Impulse>();
        }
        catch (System.Exception)
        {
            Debug.LogError("PsicoPato  n�o tem um Impulse.cs");
        }

        try
        {
            mmfRotationLaser_2 = laserAttack_2.GetFeedbackOfType<MMF_Rotation>();
            mmfRotationLaser_3 = laserAttack_3.GetFeedbackOfType<MMF_Rotation>();
            mmfScaleLaser_3 = laserAttack_3.GetFeedbackOfType<MMF_Scale>();
        }
        catch (System.Exception)
        {
            Debug.LogError("Feel Laser n�o tem MMF_Rotation");
        }

        try
        {
            moveObjToPosScript = GetComponent<MoveObjectToPos>();
        }
        catch (System.Exception)
        {
            Debug.LogError("PsicoPato n�o tem MoveObjectToPos");
        }

        try
        {
            PlayMMF_Player = GetComponent<PlayMMF_PlayerFeedbacks>();
        }
        catch (System.Exception)
        {
            Debug.LogError("PsicoPato n�o tem PlayMMF_PlayerFeedbacks");
        }


        InitiateDictionary();
        //psicoData.CheckStatesStructs();
        nextFormScript.OnChangeForm += ChangedForm;
        //currentTargetTransform.parent = null;
        //spriteShapeManager.gameObject.transform.parent = null;
        //Remove parents to prevent bugs related to directional and flips
        dashPreviewObject.transform.parent = null;
        bossScript.GetAttackByName("Laser").attackGameObject.transform.parent = null;
        //Set laser velocity of rotation

    }

    protected override void OnEnable()
    {
        //Return to seeking and reset timers.
        currentState = PsicoPatoStates.idle;
        ResetVariables();
        OnChangeState?.Invoke();
        base.OnEnable();
    }

    protected override void Start()
    {
        playerTransform = Try.FindGameObjectWithTag("Player").transform;
                
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
            case PsicoPatoStates.idle:
                  timeInIdle += Time.deltaTime;
            break;

            case PsicoPatoStates.seeking:
                    timeInSeeking += Time.deltaTime;
            break;

            case PsicoPatoStates.dash:
                    timeInDash += Time.deltaTime;
                break;

            case PsicoPatoStates.laser:
                    timeInLaser += Time.deltaTime;
                break;

            case PsicoPatoStates.shoot:
                    timeInShoot += Time.deltaTime;
                break;

            case PsicoPatoStates.egg:
                 timeInEgg += Time.deltaTime;
                break;

            case PsicoPatoStates.scream:
                 timeInScream += Time.deltaTime;
                break;
            default:
                break;

        }
    }

    // Start is called before the first frame update
    protected override void FixedUpdate()
    {

        if(currentState != PsicoPatoStates.laser && laserFeedback.IsPlaying)
        {
            laserFeedback.StopFeedbacks();
        }


        if (currentStatusEffect == StatusEffect.Frozen) return;
        switch (currentState)
        {
            case PsicoPatoStates.idle:
                //Convert the maximum to an integer when using iteration counts.
                if (psicoData.idle.usesIterationCount)
                {
                    if (iterationsAtCurrentState <= 0 || firstTimeEnteringThisState)
                    {
                        //Known issue: fixed duration causes one extra iteration.
                        maxAtThisState = (int)Math.Round(Random.Range(psicoData.idle.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                            psicoData.idle.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]));
                        firstTimeEnteringThisState = false;
                    }
                    if (iterationsAtCurrentState >= maxAtThisState && timeInIdle >= psicoData.minimumTimeOnState) 
                    {
                        firstTimeEnteringThisState = true;
                        timeInIdle = 0;
                        iterationsAtCurrentState = 0;
                        //Choose the next attack now to determine the required distance.
                        nextAttackState = GetNextAttackWithoutRepetition();
                        currentState = PsicoPatoStates.seeking;
                        //Set the Animator to the new state.
                        //TODO: Connect this to the Animator.
                        OnChangeState?.Invoke();
                        //ActivateAnimationByState(currentState);
                    }
                }
                else 
                {
                    if (timeInIdle <= 0 || firstTimeEnteringThisState)
                    {

                        maxAtThisState = Random.Range(psicoData.idle.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                            psicoData.idle.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]);


                        //Read the state's range for the current form and requested upper or lower limit.
                        firstTimeEnteringThisState = false;
                    }
                    if (timeInIdle >= maxAtThisState && timeInIdle >= psicoData.minimumTimeOnState)
                    {

                        firstTimeEnteringThisState = true;
                        timeInIdle = 0;
                        //Choose the next attack now to determine the required distance.
                        nextAttackState = GetNextAttackWithoutRepetition();
                        currentState = PsicoPatoStates.seeking;
                        //Set the Animator to the new state.
                        OnChangeState?.Invoke();
                        //ActivateAnimationByState(currentState);
                    }

                }
                

                
                break;
            case PsicoPatoStates.seeking:
                if (firstTimeEnteringThisState)
                {
                    if (nextAttackState == PsicoPatoStates.shoot)
                    {
                        // During the spawner attack, target the nearest spawner's position.
                        CalculateAndSetNeareastSpawner();
                    }
                    firstTimeEnteringThisState = false;
                }

                if (Math.Abs(Vector2.Distance(transform.position, target.position)) < GetNextAttackDistance(nextAttackState) && timeInSeeking >= psicoData.minimumTimeOnState)
                {
                    firstTimeEnteringThisState = true;
                    timeInSeeking = 0;
                    currentState = nextAttackState;

                    //Set the Animator to the new state.
                    //TODO: Connect this to the Animator.
                    TargetPlayer();
                    OnChangeState?.Invoke();
                    //ActivateAnimationByState(currentState);
                }
                break;
            case PsicoPatoStates.dash:
                if (psicoData.dash.usesIterationCount)
                {
                    Debug.LogError("Dash n�o pode ser usado com count!");
                    if (iterationsAtCurrentState <= 0 || firstTimeEnteringThisState)
                    {
                        //Convert the maximum to an integer when using iteration counts.
                        if (psicoData.dash.usesIterationCount)
                        {
                            //Known issue: fixed duration causes one extra iteration.
                            maxAtThisState = (int)Math.Round(Random.Range(psicoData.dash.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                                psicoData.dash.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]));
                        }
                        else
                        {
                            maxAtThisState = Random.Range(psicoData.dash.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                                psicoData.dash.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]);

                        }
                        firstTimeEnteringThisState = false;
                    }
                    if (iterationsAtCurrentState >= maxAtThisState && timeInDash >= psicoData.minimumTimeOnState)
                    {
                        firstTimeEnteringThisState = true;
                        iterationsAtCurrentState = 0;
                        timeInDash = 0;
                        currentState = PsicoPatoStates.idle;
                        //Set the Animator to the new state.
                        OnChangeState?.Invoke();
                        //ActivateAnimationByState(currentState);
                    }
                }
                else 
                {
                    
                    if (timeInDash<= 0 || firstTimeEnteringThisState)
                    {
                        dashPrepareFeedback.PlayFeedbacks();

                        //Read the state's range for the current form and requested upper or lower limit.
                        maxAtThisState = Random.Range(psicoData.dash.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                            psicoData.dash.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]);

                        //Clear velocity before dashing.
                        myRigidbody.linearVelocity = Vector2.zero;
                        //Show the trajectory preview.
                        dashPreviewObject.SetActive(true);

                        firstTimeEnteringThisState = false;
                    }
                    if (timeInDash >= maxAtThisState - psicoData.dashImpulseTiming[bossScript.getCurrentForm()] && !dashed)
                    {
                        dashPrepareFeedback.StopFeedbacks();
                        dashFeedback.PlayFeedbacks();
                        //Debug.LogWarning("Current pato vel" + myRigidbody.velocity);
                        //Calculate the dash angle.
                        Vector2 dir = target.position - this.gameObject.transform.position;
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                        //Hide the preview.
                        dashPreviewObject.SetActive(false);
                        //Apply the impulse.
                        impulseScript.TriggerImpulse(angle, psicoData.dashImpulseStrength[bossScript.getCurrentForm()],false,psicoData.ignoreMassWhenMoving);
                        dashed = true;
                        //Lock directional animations.
                        PsicoAnim.lockDirection = true;
                    }
                    if (timeInDash >= maxAtThisState && timeInDash >= psicoData.minimumTimeOnState)
                    {

                        firstTimeEnteringThisState = true;
                        timeInDash = 0;
                        iterationsAtCurrentState = 0;
                        dashed = false;
                        PsicoAnim.lockDirection = false;
                        //Choose the next attack now to determine the required distance.
                        
                        currentState = PsicoPatoStates.idle;
                        //Set the Animator to the new state.
                        //TODO: Connect this to the Animator.
                        OnChangeState?.Invoke();
                        //ActivateAnimationByState(currentState);
                    }

                }
                
                break;
            case PsicoPatoStates.laser:
                //Prevent facing-direction changes.

                PsicoAnim.lockDirection = true;

                if (psicoData.laser.usesIterationCount)
                {
                    if (iterationsAtCurrentState <= 0 || firstTimeEnteringThisState)
                    {
                        laserFeedback.PlayFeedbacks();

                        //Convert the maximum to an integer when using iteration counts.
                        if (psicoData.laser.usesIterationCount)
                        {
                            //Known issue: fixed duration causes one extra iteration.
                            maxAtThisState = (int)Math.Round(Random.Range(psicoData.laser.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                                psicoData.laser.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]));
                        }
                        else
                        {
                            maxAtThisState = Random.Range(psicoData.laser.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                                psicoData.laser.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]);

                        }
                        firstTimeEnteringThisState = false;
                    }
                    if (iterationsAtCurrentState >= maxAtThisState && timeInLaser >= psicoData.minimumTimeOnState)
                    {
                        firstTimeEnteringThisState = true;
                        iterationsAtCurrentState = 0;
                        timeInLaser= 0;
                        currentState = PsicoPatoStates.idle;
                        DisableLaserByAnim();
                        //Set the Animator to the new state.
                        OnChangeState?.Invoke();
                        //ActivateAnimationByState(currentState);

                        laserFeedback.StopFeedbacks();
                        laserEndFeedback.PlayFeedbacks();
                    }
                }
                else
                {
                    if (timeInLaser <= 0 || firstTimeEnteringThisState)
                    {
                        laserFeedback.PlayFeedbacks();

                        maxAtThisState = Random.Range(psicoData.laser.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                            psicoData.laser.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]);

                        //Read the state's range for the current form and requested upper or lower limit.
                        
                        firstTimeEnteringThisState = false;
                    }
                    if (timeInLaser >= maxAtThisState && timeInLaser >= psicoData.minimumTimeOnState)
                    {

                        firstTimeEnteringThisState = true;
                        timeInLaser = 0;
                        iterationsAtCurrentState = 0;
                        //Reset the laser.
                        DisableLaserByAnim();

                        currentState = PsicoPatoStates.idle;
                        //Set the Animator to the new state.
                        //TODO: Connect this to the Animator.
                        OnChangeState?.Invoke();

                        laserFeedback.StopFeedbacks();
                        laserEndFeedback.PlayFeedbacks();
                    }

                }
                break;
            case PsicoPatoStates.shoot:
                if (psicoData.shoot.usesIterationCount)
                {
                    if (iterationsAtCurrentState <= 0 || firstTimeEnteringThisState)
                    {
                        //Convert the maximum to an integer when using iteration counts.

                        //Known issue: fixed duration causes one extra iteration.
                        maxAtThisState = (int)Math.Round(Random.Range(psicoData.shoot.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                            psicoData.shoot.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]));

                        // CalculateAndSetNeareastSpawner();
                        target = centerOfArenaLocation;

                            firstTimeEnteringThisState = false;
                    }
                    if (iterationsAtCurrentState >= maxAtThisState && timeInShoot >= psicoData.minimumTimeOnState)
                    {
                        shootingEndFeedback.PlayFeedbacks();

                        firstTimeEnteringThisState = true;
                        iterationsAtCurrentState = 0;
                        timeInShoot= 0;
                        currentState = PsicoPatoStates.idle;
                        TargetPlayer();
                        //Set the Animator to the new state.
                        OnChangeState?.Invoke();
                        //ActivateAnimationByState(currentState);
                    }
                }
                else
                {
                    if (timeInShoot <= 0 || firstTimeEnteringThisState)
                    {

                        maxAtThisState = Random.Range(psicoData.shoot.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                            psicoData.shoot.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]);

                        //CalculateAndSetNeareastSpawner();
                        target = centerOfArenaLocation;
                        //Read the state's range for the current form and requested upper or lower limit.
                        firstTimeEnteringThisState = false;
                    }
                    if (timeInShoot >= maxAtThisState && timeInShoot >= psicoData.minimumTimeOnState)
                    {
                        shootingEndFeedback.PlayFeedbacks();

                        firstTimeEnteringThisState = true;
                        timeInShoot = 0;
                        iterationsAtCurrentState = 0;
                        //Choose the next attack now to determine the required distance.
                        
                        currentState = PsicoPatoStates.idle;
                        TargetPlayer();
                        //Set the Animator to the new state.
                        //TODO: Connect this to the Animator.
                        OnChangeState?.Invoke();
                        //ActivateAnimationByState(currentState);
                    }

                }
                break;
            case PsicoPatoStates.egg:
                if (psicoData.egg.usesIterationCount)
                {
                    if (iterationsAtCurrentState <= 0 || firstTimeEnteringThisState)
                    {
                        eggFeedback.PlayFeedbacks();
                        //Convert the maximum to an integer when using iteration counts.
                        if (psicoData.egg.usesIterationCount)
                        {
                            //Known issue: fixed duration causes one extra iteration.
                            maxAtThisState = (int)Math.Round(Random.Range(psicoData.egg.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                                psicoData.egg.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]));
                        }
                        else
                        {
                            maxAtThisState = Random.Range(psicoData.egg.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                                psicoData.egg.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]);

                        }
                        firstTimeEnteringThisState = false;
                    }
                    if (iterationsAtCurrentState >= maxAtThisState && timeInEgg >= psicoData.minimumTimeOnState)
                    {
                        firstTimeEnteringThisState = true;
                        iterationsAtCurrentState = 0;
                        timeInEgg = 0;
                        currentState = PsicoPatoStates.idle;
                        //Set the Animator to the new state.
                        OnChangeState?.Invoke();
                        //ActivateAnimationByState(currentState);
                    }
                }
                else
                {
                    if (timeInEgg <= 0 || firstTimeEnteringThisState)
                    {
                        eggFeedback.PlayFeedbacks();

                        maxAtThisState = Random.Range(psicoData.egg.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                            psicoData.egg.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]);


                        //Read the state's range for the current form and requested upper or lower limit.
                        firstTimeEnteringThisState = false;
                    }
                    if (timeInEgg >= maxAtThisState && timeInEgg >= psicoData.minimumTimeOnState)
                    {

                        firstTimeEnteringThisState = true;
                        timeInEgg = 0;
                        iterationsAtCurrentState = 0;
                        //Choose the next attack now to determine the required distance.
                        
                        currentState = PsicoPatoStates.idle;
                        //Set the Animator to the new state.
                        //TODO: Connect this to the Animator.
                        OnChangeState?.Invoke();
                        //ActivateAnimationByState(currentState);
                    }

                }
                break;
            case PsicoPatoStates.scream:
                if (iterationsAtCurrentState <= 0 || firstTimeEnteringThisState)
                {
                    //Convert the maximum to an integer when using iteration counts.
                    if (psicoData.scream.usesIterationCount)
                    {
                        //Known issue: fixed duration causes one extra iteration.
                        maxAtThisState = (int)Math.Round(Random.Range(psicoData.scream.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                            psicoData.scream.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]));
                    }
                    else
                    {
                        maxAtThisState = Random.Range(psicoData.scream.RangesPerForm[bossScript.getCurrentForm()].Ranges[0],
                            psicoData.scream.RangesPerForm[bossScript.getCurrentForm()].Ranges[1]);

                    }
                    firstTimeEnteringThisState = false;
                }
                if (iterationsAtCurrentState >= maxAtThisState && timeInScream >= psicoData.minimumTimeOnState)
                {
                    firstTimeEnteringThisState = true;
                    iterationsAtCurrentState = 0;
                    timeInScream = 0;
                    currentState = PsicoPatoStates.idle;
                    //Set the Animator to the new state.
                    OnChangeState?.Invoke();
                    //ActivateAnimationByState(currentState);
                }
                break;
            default:
                break;
        }


        //MoveAndTarget();
        base.FixedUpdate();
    }
    protected void MoveAndTarget()
    {
        if (path == null) return;

        if (currentStatusEffect == StatusEffect.Frozen) return;

        if (isNPC && Vector3.Distance(target.position, transform.position) < 1) return; //Stop when the target is reached.

        if (currentWayPoint >= path.vectorPath.Count) //The end of the path was reached.
        {
            //reachedEndOfPath = true;
            return;
        }
        else
        {
            desiredDirection = GetDesiredDirection();

            //Debug.Log(desiredDirection);


            walkSpeed = GetWalkSpeed();

            Vector2 force = desiredDirection * walkSpeed * Time.fixedDeltaTime;

            myRigidbody.AddForce(force, ForceMode2D.Force);


            float distance = Vector2.Distance(myRigidbody.position, path.vectorPath[currentWayPoint]);

            if (distance < nextWaypointDistance)
            {
                currentWayPoint++;
            }
        }

    }

    protected override float GetWalkSpeed()
    {
        if (Vector2.Distance(transform.position, target.position) < enemyData.minDistanceToIncreaseSpeed)
        {
            return psicoData.GetSpeed(currentState, nextAttackState);

        }
        else
        {
            return psicoData.GetSpeed(currentState, nextAttackState) * enemyData.speedMultiplierWhenNotVisible;
        }
    }

    protected override Vector2 GetDesiredDirection()
    {
        //Face the player or next path point according to the current state.

        if (currentState == PsicoPatoStates.seeking || currentState == PsicoPatoStates.dash)
        {
            return  ((Vector2)path.vectorPath[currentWayPoint] - myRigidbody.position).normalized;


        }
        else
        {
            return  ((Vector2)target.position - myRigidbody.position).normalized;
        }   
    }


    /// <summary>
    /// Initialize the mapping between boss forms and available attacks.
    /// </summary>
    protected void InitiateDictionary()
    {
        unlockedAttacks = new Dictionary<int, (int, PsicoPatoStates)[]>();
        (int, PsicoPatoStates)[] form1 =
{
            (psicoData.dash.WeightsPerForm[0], psicoData.dash.state),
            (psicoData.shoot.WeightsPerForm[0], psicoData.shoot.state),
        };
        unlockedAttacks.Add(0, form1);
        (int, PsicoPatoStates)[] form2 =
{           (psicoData.dash.WeightsPerForm[1], psicoData.dash.state),
            (psicoData.shoot.WeightsPerForm[1], psicoData.shoot.state),
            (psicoData.laser.WeightsPerForm[1], psicoData.laser.state),
        };
        unlockedAttacks.Add(1, form2);
        (int, PsicoPatoStates)[] form3 =
{           (psicoData.dash.WeightsPerForm[2], psicoData.dash.state),
            (psicoData.shoot.WeightsPerForm[2], psicoData.shoot.state),
            (psicoData.laser.WeightsPerForm[2], psicoData.laser.state),
            (psicoData.egg.WeightsPerForm[2], psicoData.egg.state),
        };
        unlockedAttacks.Add(2, form3);
    }


    /// <summary>
    /// Select a state using its configured weights.
    /// </summary>
    /// <param name="currentState">Current boss state.</param>
    /// <returns></returns>
    protected PsicoPatoStates NextWeightedRandomState()
    {
        //unlockedAttacks[nextFormScript.currentForm];
        // Step 1: Normalize probabilities.
        float totalProbability = 0f;
        foreach ((int, PsicoPatoStates) targetState in unlockedAttacks[nextFormScript.currentForm])
        {

            totalProbability += targetState.Item1;
        }

        // Ensure the total is greater than zero.
        if (totalProbability <= 0)
        {
            Debug.LogError("As probabilidades de estados devem ser maiores que 0.");
            return PsicoPatoStates.idle;
        }

        // Step 2: Draw a random value within the total probability weight.
        float randomValue = Random.Range(0, totalProbability);

        // Step 3: Find the corresponding attack.
        // Repeated attacks may depend on the order in which they are added here.
        //TODO: Track recent attacks and reject repeated selections or remove them from the pool.
        float cumulativeProbability = 0f;
        foreach ((int, PsicoPatoStates) targetState in unlockedAttacks[nextFormScript.currentForm]) 
        {
    
                cumulativeProbability += targetState.Item1;
            
            if (randomValue <= cumulativeProbability)
            {
                return targetState.Item2;
            }
        }

        // Return null if selection fails.
        Debug.LogError("Nenhum estado foi selecionado. Verifique as probabilidades :c.");
        return PsicoPatoStates.idle;
    }

    protected float GetNextAttackDistance(PsicoPatoStates targetState) 
    {
        switch (targetState)
        {
            case PsicoPatoStates.dash:
                return psicoData.dash.DistancePerForm[nextFormScript.currentForm];
            case PsicoPatoStates.egg:
                return psicoData.egg.DistancePerForm[nextFormScript.currentForm];
            case PsicoPatoStates.laser:
                return psicoData.laser.DistancePerForm[nextFormScript.currentForm];
            case PsicoPatoStates.shoot:
                return psicoData.shoot.DistancePerForm[nextFormScript.currentForm];
            default:
                return 0;
        }
    }

    public PsicoPatoStates GetCurrentState()
    {
        return currentState;
    }

    public void AddIterationByAnimation()
    {
        iterationsAtCurrentState += 1;
    }

    /// <summary>
    /// Enter scream when the boss changes form; apply form-specific stat changes here.
    /// </summary>
    public void ChangedForm()
    {
        ResetVariables();
        currentState = PsicoPatoStates.scream;
        //PsicoAnim.TriggerChangeStateAnimation();
        OnChangeState?.Invoke();
    }


    /// <summary>
    ///Also used when taking damage while preparing to fire.
    ///Enter seeking and restart the cooldown timer.
    /// </summary>
    public void GoToIdle()
    {
        ResetVariables();
        timeInIdle = 0;
        currentState= PsicoPatoStates.idle;
        //TargetPath();
        OnChangeState?.Invoke();
    }

    //These target helpers switch between looking at the next path point (targetPath)
    //and facing the player while aiming or shooting.
    protected void TargetPlayer()
    {
        target = playerTransform;
        UpdatePath();
        //targetingPlayer = true;

    }

    //Experimental Sprite Shape implementation.
    protected void ShowPreview()
    {
        //Vector2 dir = target.position - this.gameObject.transform.position;
        //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //Vector2 targetPosInter = (Vector2)target.position + 10 * (-dir);
        //Vector3 targetPos = new Vector3(targetPosInter.x, targetPosInter.y, 0f);
        //spriteShapeManager.UpdateKeyPoint(1, playerTransform.position);
    }

    /// <summary>
    /// Reset all state-related variables.
    /// </summary>
    protected void ResetVariables()
    {
        //currentState = PsicoPatoStates.idle;
        timeInIdle = 0f;
        timeInSeeking = 0f;
        timeInShoot = 0f;
        timeInDash = 0f;
        timeInLaser = 0f;
        timeInEgg = 0f;
        //laserGameObject.SetActive(false);
        DisableLaserByAnim();
        target = playerTransform;
        dashPreviewObject.SetActive(false);
        targetingPlayer = false;
        firstTimeEnteringThisState = true;
        iterationsAtCurrentState = 0;
        dashed = false;
        PsicoAnim.lockDirection = false; // Consider moving this to the base class.
        StopAllFeedbacks();
        bossScript.DeactivateAllAttacks();
    }

    /// <summary>
    /// Stop all attack feedbacks, including sounds.
    /// </summary>
    protected void StopAllFeedbacks() 
    {
        dashFeedback.StopFeedbacks();
        dashPrepareFeedback.StopFeedbacks();
        laserFeedback.StopFeedbacks(); 
        laserEndFeedback.StopFeedbacks();
        shootingEndFeedback.StopFeedbacks();
        eggFeedback.StopFeedbacks();
    }

    //Laser Related
    //Helpers below activate the laser or change its direction.
    protected void TriggerLaserByAnim()
    {
        bossScript.AttackByIndex(0);
        //if (bossScript.getCurrentForm() == 1)
        //{
            laserAttack_2.PlayFeedbacks();
        //}
        //else
        //{
        // Laser feedback 2 replaced feedback 3 because the latter was unreliable.
        //    laserAttack_2.PlayFeedbacks();
        //}
    }

    protected void DisableLaserByAnim()
    {
        //bossScript.DeactivateAttackByIndex(0);
        //if (bossScript.getCurrentForm() == 1)
        //{
            laserAttack_2.StopFeedbacks();
            laserAttack_2.RestoreInitialValues();
        //}
        //else 
        //{
        //    //Laser feedback 2 replaced feedback 3 because the latter was unreliable.
        //    laserAttack_2.StopFeedbacks();
        //    laserAttack_2.RestoreInitialValues();
        //}
        PsicoAnim.lockDirection = false;
    }

    protected void TriggerAndChangeLaserToHorizontal() 
    {
        //currentForm(0,1,2) -> form "1" = segunda forma
        //if (bossScript.getCurrentForm() == 1)
        //{
            mmfRotationLaser_2.RemapCurveZero = -90;
            mmfRotationLaser_2.RemapCurveOne = 90;
        //}
        //else
        //{
        //    mmfRotationLaser_3.RemapCurveZero = -90;
        //    mmfRotationLaser_3.RemapCurveOne = 90;
        //}

        GameObject targetAttack = bossScript.GetAttackByName("Laser").attackGameObject;
        //Match the attack's flip state to the object.
        if ((this.transform.localScale.x < 0 && targetAttack.transform.localScale.x >0) || (this.transform.localScale.x > 0 && targetAttack.transform.localScale.x < 0))
        {

            targetAttack.transform.localScale = new Vector3(-targetAttack.transform.localScale.x,
                        targetAttack.transform.localScale.y, targetAttack.transform.localScale.z);
            if (bossScript.getCurrentForm() != 1)
            {
                mmfScaleLaser_3.RemapCurveOne = mmfScaleLaser_3.RemapCurveOne * -1;
                mmfScaleLaser_3.RemapCurveZero = mmfScaleLaser_3.RemapCurveZero * -1;
            }
        }
        PsicoAnim.lockDirection = true;
        ToggleLaserbyAnimation();
    }

    protected void TriggerAndChangeLaserToUp()
    {
        GameObject targetAttack = bossScript.GetAttackByName("Laser").attackGameObject;
        if (targetAttack.transform.localScale.x < 0)
        {

            targetAttack.transform.localScale = new Vector3(Math.Abs(targetAttack.transform.localScale.x),
                        targetAttack.transform.localScale.y, targetAttack.transform.localScale.z);
            if (bossScript.getCurrentForm() != 1)
            {
                mmfScaleLaser_3.RemapCurveOne = mmfScaleLaser_3.RemapCurveOne * -1;
                mmfScaleLaser_3.RemapCurveZero = mmfScaleLaser_3.RemapCurveZero * -1;
            }
        }

        mmfRotationLaser_2.RemapCurveZero = 0;
        mmfRotationLaser_2.RemapCurveOne = 180;

        //currentForm(0,1,2) -> form "1" = segunda forma
        /* //Legacy two-laser implementation.
        if (bossScript.getCurrentForm() == 1)
        {
            mmfRotationLaser_2.RemapCurveZero = 0;
            mmfRotationLaser_2.RemapCurveOne = 180;
        }
        else
        {
            mmfRotationLaser_3.RemapCurveZero = 0;
            mmfRotationLaser_3.RemapCurveOne = 180;
        }
        */
        PsicoAnim.lockDirection = true;
        ToggleLaserbyAnimation();
    }

    protected void TriggerAndChangeLaserToDown()
    {
        GameObject targetAttack = bossScript.GetAttackByName("Laser").attackGameObject;
        if (targetAttack.transform.localScale.x < 0)
        {

            targetAttack.transform.localScale = new Vector3(Math.Abs(targetAttack.transform.localScale.x),
                        targetAttack.transform.localScale.y, targetAttack.transform.localScale.z);
            if (bossScript.getCurrentForm() != 1)
            {
                mmfScaleLaser_3.RemapCurveOne = mmfScaleLaser_3.RemapCurveOne * -1;
                mmfScaleLaser_3.RemapCurveZero = mmfScaleLaser_3.RemapCurveZero * -1;
            }
        }
        //currentForm(0,1,2) -> form "1" = segunda forma
        mmfRotationLaser_2.RemapCurveZero = 180;
        mmfRotationLaser_2.RemapCurveOne = 360;
        /*
         //Legacy two-laser implementation.
         if (bossScript.getCurrentForm() == 1)
        {
            mmfRotationLaser_2.RemapCurveZero = 180;
            mmfRotationLaser_2.RemapCurveOne = 360;
        }
        else
        {
            mmfRotationLaser_3.RemapCurveZero = 180;
            mmfRotationLaser_3.RemapCurveOne = 360;
        }

         */

        PsicoAnim.lockDirection = true;
        ToggleLaserbyAnimation();
    }

    /// <summary>
    /// Activate the laser for the current boss form.
    /// </summary>
    protected void ToggleLaserbyAnimation()
    {
        
        //Do not replay feedbacks if already active.
        if(bossScript.GetAttackByName("Laser").attackGameObject.activeInHierarchy == true)
        {

            
        }else 
        {
            bossScript.AttackByIndex(0);
            ////currentForm(0,1,2) -> form "1" = segunda forma
            //if (bossScript.getCurrentForm() == 1)
            //{
                laserAttack_2.PlayFeedbacks();
            //}
            //else 
            //{
            // Laser feedback 2 replaced feedback 3 because the latter was unreliable.
            //    laserAttack_2.PlayFeedbacks();
            //}
                
        }

    }

    //End Laser Related

    /// <summary>
    /// Find and activate the nearest spawner.
    /// </summary>
    protected void CalculateAndSetNeareastSpawner() 
    {
        if (Vector2.Distance(this.transform.position, leftShootLocation.position) >=
            Vector2.Distance(this.transform.position, rightShootLocation.position))
        {
            //0 = Esquerda, 1= direita
            chosenSpawn = 1;
            target = rightShootLocation; 

        }
        else
        {
            //0 = Esquerda, 1= direita
            chosenSpawn = 0;
            target = leftShootLocation;
        }
        UpdatePath();
    }

    /// <summary>
    /// Activate the arena spawner selected as nearest.
    /// </summary>
    protected void TriggerShootWithAnimation()
    {
        target = centerOfArenaLocation;
        if (bossScript.GetAttackByName("ShootLeft").attackGameObject.activeInHierarchy == false && bossScript.GetAttackByName("ShootLeft").attackGameObject.activeInHierarchy == false)
        {   //If the left spawner was selected,
            if (chosenSpawn == 0 )
            {
                moveObjToPosScript.StartMove(0);
                bossScript.AttackByIndex(1);
            }
            //Otherwise, activate the right side.
            else
            {
                moveObjToPosScript.StartMove(1);
                bossScript.AttackByIndex(2);
            }
        }
    }

    /// <summary>
    /// Spawn an egg attack when allowed by eggSpawnRate.
    /// Keep the object-spawn feedback at index 4 in PlayMMF_Player.
    /// </summary>
    protected void TriggerEggWithAnimation()
    {
        if (Time.time - timeOfLastEgg > psicoData.eggSpawnRate)
        {
            try
            {
                timeOfLastEgg = Time.time;
                eggFeedback.PlayFeedbacks();
            }
            catch (Exception)
            {

                Debug.LogError("Psicopato n�o tem o feedback de ovo no espa�o 4");
            }
        }
        return;
    }

    //Attack Queue related
    /// <summary>
    /// Enqueue up to three states.
    /// </summary>
    /// <param name="targetState"></param>
    public void MaxEnqueue(PsicoPatoStates targetState)
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

        PsicoPatoStates[] tempStates = lastAttackStates.ToArray();

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
    protected PsicoPatoStates GetNextAttackWithoutRepetition() 
    {
        PsicoPatoStates possibleNextState = NextWeightedRandomState();
        PsicoPatoStates[] tempStates = lastAttackStates.ToArray();
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
}
