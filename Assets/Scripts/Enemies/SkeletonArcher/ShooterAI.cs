using System;
using UnityEngine;


public class ShooterAI : EnemyAI
{
    // Potential easing curve for quadratic movement through MMTween.

    public enum ShooterStates { seeking, escaping, alert, firing}

    protected ShooterStates state = ShooterStates.seeking;
    protected ShooterData shooterData;

    //Target Related
    [SerializeField] protected bool targetingPlayer = false;

    //Timer Related
    [SerializeField] private float currentAlertTimer = 0;
    [SerializeField] private float currentSeekingTimer = 0;
    private float currentSpawnLockStateTimer = 0;

    //Leap Related
    [SerializeField] protected Vector2 shootingSpot;
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
    [SerializeField] protected EnemyHealth shooterHealth;
    //Tracer Related
    protected Vector2 initialPosition;
    protected Vector2 midPoint;


    //Events
    public event Action OnChangeState;//, OnPrepareToDie;

    //Die Comunication related
    protected HordeManagerCommunicator hordeManagerCommunicator;

    //Shooting Related
    protected AttackPlayer attackPlayer;

    //Animation related
    [SerializeField] protected ShooterMovementAnimation shooterAnim;

    //Preview Related
    [SerializeField] protected GameObject shootPreviewObject; // Assigned in the Editor.
    [SerializeField] protected bool usesShootPreview = false;
    protected override void Awake()
    {
        base.Awake();
        try
        {
            shooterData = (ShooterData)enemyData;
        }
        catch (Exception)
        {
            Debug.LogError("enemyData não é uma instancia de PsicoPatoData! Coloque um PsicoPatoData na referencia 'Enemy Data'");
        }

        try
        {
            invocationFXHandler = this.gameObject.GetComponent<HandleInvocationFX>();
        }
        catch (Exception)
        {
            Debug.LogError("PsicoPato não possui um script HandleInvocationFX");
        }
        try
        {
            shooterHealth = this.gameObject.GetComponent<EnemyHealth>();
            shooterHealth.OnTakeDamage += MakeItConfused;
            // Apply the same cleanup to the jumping hand when it dies.
        }
        catch (Exception)
        {
            Debug.LogError("PsicoPato não possui um script EnemyHealth");
        }
        try
        {
            changeTargetScript = this.gameObject.GetComponent<ChangeTarget>();
        }
        catch (Exception)
        {
            Debug.LogError("Shooter não possui um script ChangeTarget");
        }
        try
        {
            attackPlayer = GetComponent<AttackPlayer>();
        }
        catch (System.Exception)
        {
            Debug.LogError("Shooter  não tem um AttackPlayer.cs");
        }
        try
        {
            shooterAnim = GetComponentInChildren<ShooterMovementAnimation>();
        }
        catch (System.Exception)
        {
            Debug.LogError("Shooter  não tem um ShooterMovementAnimation.cs em um dos seus filhos");
        }


    }

    protected override void OnEnable()
    {
        //Return to seeking and reset timers.
        state = ShooterStates.seeking;
        currentAlertTimer = 0;
        currentSeekingTimer = 0;
        currentSpawnLockStateTimer = 0;
        targetingPlayer = false;
        shootPreviewObject.SetActive(false);
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

    }

    protected override void Update()
    {
        base.Update();

        if (state == ShooterStates.seeking)
        {
            currentSpawnLockStateTimer += Time.deltaTime;
            currentSeekingTimer += Time.deltaTime;
        }
        else if (state == ShooterStates.alert)
        {
            currentAlertTimer += Time.deltaTime;
        }


    }
    protected override void FixedUpdate()
    {
        if (currentStatusEffect == StatusEffect.Frozen) return;

        switch (state)
        {
            //Seek while outside distToAlert; enter alert once close enough.
            case ShooterStates.seeking:
                //Hide the shooting preview.
                
                //Debug.LogWarning("Seeking!");
                //Calculate the best place to move to aim from
                BestPlaceToShoot();

                //Retreat if the player is too close.
                if ((Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) <= shooterData.limitDistToAim))
                {
                    currentSeekingTimer = 0;
                    state = ShooterStates.escaping;
                    OnChangeState?.Invoke();
                }


                //If the post-spawn delay and cooldown have elapsed,
                if (currentSpawnLockStateTimer >= shooterData.timeInSpawnLockState && currentSeekingTimer >= shooterData.timeInCooldown)
                {
                    if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) < shooterData.minDistToAim)
                    {
                        //If the player is within aiming range,
                        if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) > shooterData.limitDistToAim)
                        {
                            //Initialize the alert timer.
                            currentSeekingTimer = 0;
                            TargetPlayer();
                            currentAlertTimer = 0;
                            if (usesShootPreview)
                            {
                                shootPreviewObject.SetActive(true);
                            }
                            state = ShooterStates.alert;
                            OnChangeState?.Invoke();

                        }
                    }
                }



                break;
            case ShooterStates.escaping:
                //Debug.LogWarning("Escaping!");

                //Calculate the best place to move to aim from
                BestPlaceToShoot();
                //Enter alert when distance is at most distToAlert.
                if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) < shooterData.minDistToAim)
                {
                    //If the player is within aiming range,
                    if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) > shooterData.limitDistToAim)
                    {
                        TargetPlayer();
                        currentAlertTimer = 0;
                        if (usesShootPreview)
                        {
                            shootPreviewObject.SetActive(true);
                        }
                        state = ShooterStates.alert;
                        OnChangeState?.Invoke();
                    }
                }
                else
                {
                    shootPreviewObject.SetActive(false);
                    state = ShooterStates.seeking;
                    OnChangeState?.Invoke();
                }
                break;
            case ShooterStates.alert:
                //Debug.LogWarning("Alert!");

                /*
                // Restore this section if shooters should stop aiming when the player leaves range.
                //Return to seeking when distance exceeds distToAlert.
                if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) > shooterData.minDistToAim)
                {
                    tracerHolder.SetActive(false);
                    state = ShooterStates.seeking;
                    OnChangeState?.Invoke();
                }
                */

                // Cache the player position for the jumping enemy.
                if (currentAlertTimer >= shooterData.timeInAlert)
                {
                    
                    // Cache the firing destination here.
                    shootingSpot = new Vector2(playerTransform.position.x, playerTransform.position.y);

                    state = ShooterStates.firing;
                    OnChangeState?.Invoke();
                }
                break;
            //Activate Jumping Hand, configure its path, and deactivate the grounded object.
            case ShooterStates.firing:


                //Animation events fire the projectile and reset seeking.


                
                break;
            default:
                break;
        }


        //MoveAndTarget();
        base.FixedUpdate();

    }


    /// <summary>
    /// choose a firing position between the retreat distance
    /// and the maximum approach distance from the player.
    /// </summary>
    protected void BestPlaceToShoot()
    {


        thisPos = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
        targetPos = new Vector2(playerTransform.position.x, playerTransform.position.y);

        float distance = Vector2.Distance(thisPos, targetPos);

        Vector2 relationThisTarget = thisPos - targetPos;

        Vector2 newLocation = thisPos + relationThisTarget.normalized * (shooterData.limitDistToAim - distance);

        currentTargetTransform.position = new Vector3(newLocation.x, newLocation.y, playerTransform.position.z);


        changeTargetScript.SetNewTarget(currentTargetTransform);
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

    protected override Vector2 GetDesiredDirection()
    {
        //Face the player or next path point according to the current state.
        if (state == ShooterStates.alert || state == ShooterStates.firing)
        {
            return desiredDirection = ((Vector2)target.position - myRigidbody.position).normalized;

        }
        else
        {
            return desiredDirection = ((Vector2)path.vectorPath[currentWayPoint] - myRigidbody.position).normalized;
        }
    }

    protected override float GetWalkSpeed()
    {
        if (Vector2.Distance(transform.position, target.position) < enemyData.minDistanceToIncreaseSpeed)
        {
            return walkSpeed = shooterData.GetSpeed(state);

        }
        else
        {
            return walkSpeed = shooterData.GetSpeed(state) * enemyData.speedMultiplierWhenNotVisible;
        }
    }

    public ShooterStates GetCurrentState()
    {
        return state;
    }

    /// <summary>
    ///Also used when taking damage while preparing to fire.
    ///Enter seeking and restart the cooldown timer.
    /// </summary>
    public void GoToSeeking()
    {
        currentSeekingTimer = 0;
        state = ShooterStates.seeking;
        shootPreviewObject.SetActive(false);
        TargetPath();
        OnChangeState?.Invoke();
    }

    public void MakeItConfused()
    {
        if (state == ShooterStates.alert)
        {
            GoToSeeking();
        }

    }

    /// <summary>
    /// Fire through an animation event.
    /// </summary>
    public void SpawnBulletsByAnimation()
    {
        attackPlayer.InstantiateProjectile();
    }


    //These target helpers switch between looking at the next path point (targetPath)
    //and facing the player while aiming or shooting.
    protected void TargetPlayer()
    {
        target = playerTransform;
        changeTargetScript.SetNewTarget(target);
        targetingPlayer = true;

    }
    protected void TargetPath()
    {
        target = currentTargetTransform;
        changeTargetScript.SetNewTarget(currentTargetTransform);
        targetingPlayer = false;

    }








}
