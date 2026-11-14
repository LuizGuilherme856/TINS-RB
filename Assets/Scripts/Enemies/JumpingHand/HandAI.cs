using System;
using UnityEngine;


public class HandAI : EnemyAI, ILander
{
    // Potential easing curve for quadratic movement through MMTween.

    public enum handStates { seeking, escaping, alert, jumping, recovering }

    protected handStates state = handStates.seeking;
    protected HandData handData;




    //Timer Related
    private float currentAlertTimer = 0;
    private float currentRecoverTimer = 0;
    private float currentSpawnLockStateTimer = 0;

    //Leap Related
    [SerializeField] protected GameObject jumpingHand;
    [SerializeField] protected Vector2 landingSpot;
    //Assign an empty target object that moves to the currently selected target.
    [SerializeField] protected Transform currentTargetTransform;
    [SerializeField] protected Transform playerTransform;
    [SerializeField] protected ChangeTarget changeTargetScript;

    //Movement Calculation Related
    protected Vector2 thisPos;
    protected Vector2 targetPos;
    protected Vector2 distance;


    //Fx Related
    protected HandleInvocationFX invocationFXHandler;
    //Health Related
    [SerializeField] protected EnemyHealth handHealth;
    protected EnemyHealth jumpingHandHealth;
    //Tracer Related
    [SerializeField] protected GameObject tracerHolder;//Assigned in the Editor.
    protected TracerManager tracerManager;
    protected Vector2 initialPosition;
    protected Vector2 jumpingTargetPosition;
    protected Vector2 midPoint;
    [Tooltip("Quantidade de keyPoints no preview de Trajetoria do line render")]
    [SerializeField] protected int stepQuantity = 100;

    //Events
    public event Action OnChangeState, OnPrepareToDie;

    //Die Comunication related
    protected HordeManagerCommunicator hordeManagerCommunicator;


    protected override void Awake()
    {
        base.Awake();
        try
        {
            handData = (HandData)enemyData;
            //Set JumpingDataEnemy.maxDistanceToLeap to minDistToJump for jump-duration calculations.
            handData.maxLeapingDistance = handData.minDistToJump;
        }
        catch (Exception)
        {
            Debug.LogError("enemyData não é uma instancia de handData! Coloque um handData na referencia 'Enemy Data'");
        }
        try
        {
            jumpingHand.GetComponent<JumpingEnemy>().OnLand += Landed;
            jumpingHand.GetComponent<JumpingEnemy>().SetEnemyData(this.handData);
        }
        catch (Exception)
        {
            Debug.LogError("Jumping hand não possui um script jumping Enemy");
        }
        try
        {
            invocationFXHandler = this.gameObject.GetComponent<HandleInvocationFX>();
        }
        catch (Exception)
        {
            Debug.LogError("Jumping hand não possui um script HandleInvocationFX");
        }
        try
        {
            handHealth = this.gameObject.GetComponent<EnemyHealth>();
            handHealth.OnZeroHealth += PreparingToDie;
            handHealth.OnTakeDamage += MakeItConfused;
            // Apply the same cleanup to the jumping hand when it dies.
        }
        catch (Exception)
        {
            Debug.LogError("Jumping hand não possui um script EnemyHealth");
        }
        try
        {
            jumpingHandHealth = jumpingHand.gameObject.GetComponent<EnemyHealth>();

            // Apply the same cleanup to the jumping hand when it dies.
        }
        catch (Exception)
        {
            Debug.LogError("Jumping hand não possui um script EnemyHealth");
        }
        try
        {
            changeTargetScript = this.gameObject.GetComponent<ChangeTarget>();
        }
        catch (Exception)
        {
            Debug.LogError("Jumping hand não possui um script ChangeTarget");
        }
        try
        {
            tracerManager = tracerHolder.gameObject.GetComponent<TracerManager>();
        }
        catch (Exception)
        {
            Debug.LogError("Tracer Holder não possui um script TracerManager");
        }
        try
        {
            hordeManagerCommunicator = GetComponent<HordeManagerCommunicator>();
        }
        catch (Exception)
        {
            Debug.LogError("HandAi não possui um script HordeManagerCommunicator");
        }

    }

    protected override void OnEnable()
    {
        //Return to seeking and reset timers.
        state = handStates.seeking;
        currentAlertTimer = 0;
        currentRecoverTimer = 0;
        currentSpawnLockStateTimer = 0;
        tracerHolder.SetActive(false);
        OnChangeState?.Invoke();
        base.OnEnable();
    }
    protected override void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (isNPC == false)
        {
            currentTargetTransform.position = playerTransform.position;
            target = currentTargetTransform;

        }
        else
        {
            target = transform;
        }
        //After spawning, notify HordeManager that pooled reactivation must not update this again.
        hordeManagerCommunicator.SetUpdateRunData(false);
    }

    protected override void Update()
    {
        base.Update();

        if (state == handStates.seeking)
        {
            currentSpawnLockStateTimer += Time.deltaTime;
        }
        else if (state == handStates.alert)
        {
            currentAlertTimer += Time.deltaTime;
        }
        else if (state == handStates.recovering)
        {
            currentRecoverTimer += Time.deltaTime;
        }

    }
    protected override void FixedUpdate()
    {
        //Keep the jumping-hand object at the current enemy position.
        jumpingHand.gameObject.transform.position = this.gameObject.transform.position;

        switch (state)
        {
            //Seek while outside distToAlert; enter alert once close enough.
            case handStates.seeking:
                tracerHolder.SetActive(false);
                //Debug.LogWarning("Seeking!");
                //Calculate the best place to move to jump from
                BestPlaceToJump();
                //Enter alert when distance is at most distToAlert.
                if (currentSpawnLockStateTimer >= handData.timeInSpawnLockState)
                {
                    if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) < handData.minDistToJump)
                    {
                        //Retreat if the player is too close.
                        if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) > handData.limitDistToJump)
                        {
                            //Initialize the alert timer.
                            currentAlertTimer = 0;
                            state = handStates.alert;
                            OnChangeState?.Invoke();

                        }
                        else
                        {
                            state = handStates.escaping;
                            OnChangeState?.Invoke();
                        }
                    }
                }



                break;
            case handStates.escaping:
                tracerHolder.SetActive(false);
                //Debug.LogWarning("Escaping!");

                //Calculate the best place to move to jump from
                BestPlaceToJump();
                //Enter alert when distance is at most distToAlert.
                if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) < handData.minDistToJump)
                {
                    //Retreat if the player is too close.
                    if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) > handData.limitDistToJump)
                    {
                        state = handStates.alert;
                        OnChangeState?.Invoke();
                    }
                }
                else
                {
                    state = handStates.seeking;
                    OnChangeState?.Invoke();
                }




                break;
            //Remain still until timeInAlert elapses, then change speed and enter jumping.
            case handStates.alert:
                //Debug.LogWarning("Alert!");
                //Return to seeking when distance exceeds distToAlert.
                if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) > handData.minDistToJump)
                {
                    tracerHolder.SetActive(false);
                    state = handStates.seeking;
                    OnChangeState?.Invoke();
                }
                PlotTrajectory();
                // Cache the player position for the jumping enemy.
                if (currentAlertTimer >= handData.timeInAlert)
                {

                    landingSpot = new Vector2(playerTransform.position.x, playerTransform.position.y);

                    state = handStates.jumping;
                    OnChangeState?.Invoke();
                }
                break;
            //Activate Jumping Hand, configure its path, and deactivate the grounded object.
            case handStates.jumping:
                //Debug.LogWarning("Jumping!");


                try
                {
                    //Disable the invocation effect.
                    invocationFXHandler.enabled = false;
                    //Preserve this order so Jumping Hand can read the required data.
                    jumpingHand.SetActive(true);

                    this.gameObject.SetActive(false);

                    //TODO: Activate Jumping Hand, configure its path, and deactivate the grounded object.
                }
                catch (Exception)
                {
                    Debug.LogError("JumperHand não tem script JumpingHandAI, verifique o prefab JumperHand'");
                }




                break;
            //After missing the player, recover until timeInRecover elapses.
            //TODO: Consider a separate landing state.
            case handStates.recovering:
                //Debug.LogWarning("Recovering!");
                tracerHolder.SetActive(false);
                //invocationFXHandler.enabled = true;

                if (currentRecoverTimer >= handData.timeInRecover)
                {
                    state = handStates.seeking;
                    OnChangeState?.Invoke();
                }
                break;
            default:
                break;
        }


        //MoveAndTarget();
        base.FixedUpdate();

    }

    /// <summary>
    /// Recalculate and display the trajectory while tracking the player.
    /// </summary>
    private void PlotTrajectory()
    {
        tracerHolder.SetActive(true);

        initialPosition = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
        //Calculate the landing point.
        jumpingTargetPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);

        midPoint = AuxInterpolator.CalculateMidPoint(initialPosition, jumpingTargetPosition);
        tracerManager.UpdateKeyPoints(initialPosition, midPoint, jumpingTargetPosition, stepQuantity);
    }



    protected void BestPlaceToJump()
    {


        thisPos = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
        targetPos = new Vector2(playerTransform.position.x, playerTransform.position.y);

        float distance = Vector2.Distance(thisPos, targetPos);

        Vector2 relationThisTarget = thisPos - targetPos;

        Vector2 newLocation = thisPos + relationThisTarget.normalized * (handData.limitDistToJump - distance);

        //currentTargetTransform = new GameObject().transform;
        currentTargetTransform.position = new Vector3(newLocation.x, newLocation.y, playerTransform.position.z);

        //target = currentTargetTransform;

        changeTargetScript.SetNewTarget(currentTargetTransform);
        //UpdatePath();
    }

    /// <summary>
    /// Override because the base FixedUpdate used the wrong GetSpeed implementation for state-based movement.
    /// </summary>
    protected void MoveAndTarget()
    {
        if (path == null) return;

        if (currentStatusEffect == StatusEffect.Frozen) return;

        if (isNPC && Vector3.Distance(target.position, transform.position) < 1) return; //Stop when the target is reached.

        if (currentWayPoint >= path.vectorPath.Count) //The end of the path was reached.
        {
            //reachedEndOfPath = true;
            //return;
        }
        else
        {
            desiredDirection = ((Vector2)path.vectorPath[currentWayPoint] - myRigidbody.position).normalized;
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
            return  handData.GetSpeed(state);

        }
        else
        {
            return  handData.GetSpeed(state) * enemyData.speedMultiplierWhenNotVisible;
        }
    }

    public handStates GetCurrentState()
    {
        return state;
    }
    // Reactivate the invocation effect when the enemy dies.
    protected void ReactivateInvocationFx()
    {
        invocationFXHandler.enabled = true;
    }
    //Prepare state before enemy death.
    protected void PreparingToDie()
    {
        tracerHolder.SetActive(false);
        hordeManagerCommunicator.SetUpdateRunData(true);
        OnPrepareToDie?.Invoke();
        ReactivateInvocationFx();
    }
    public void SetInvocationFxActive(bool target)
    {
        invocationFXHandler.enabled = target;
    }
    /// <summary>
    /// Subscribe this handler to the spawned jumping enemy's OnLand event
    /// to reactivate the grounded object after landing.
    ///Also used when taking damage while preparing to jump.
    ///Put the hand into recovering.
    /// </summary>
    protected void GoToRecovering()
    {
        currentRecoverTimer = 0;
        state = handStates.recovering;
        OnChangeState?.Invoke();
    }
    //ILander-related helpers.
    public Vector2 GetLandingSpot()
    {
        return landingSpot;
    }

    public float GetMaxTimeLeaping()
    {
        return handData.maxTimeLeaping;
    }

    public void MakeItConfused()
    {
        if (state == handStates.alert)
        {
            GoToRecovering();
        }

    }

    public void Landed()
    {
        //Read health from the jumping hand.
        handHealth.SetMaxHealth(jumpingHandHealth.maxHealth);
        handHealth.SetCurrentHealth(jumpingHandHealth.currentHealth);
        GoToRecovering();


    }







}
