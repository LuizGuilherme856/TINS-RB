using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class NecromancerAI : EnemyAI
{
    #region Variables

    //State and data
    public enum NecromancerStates{seeking, escaping, alert, firing}

    [SerializeField] protected NecromancerStates state = NecromancerStates.seeking;
    protected NecromancerData necromancerData;
    protected bool firstTimeEnteringAlert = true;


    //Target Related
    [SerializeField] protected bool targetingPlayer = false;

    //Timer Related
    [SerializeField] private float currentAlertTimer = 0;
    [SerializeField] private float currentSeekingTimer = 0;
    private float currentSpawnLockStateTimer = 0;


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
    [SerializeField] protected EnemyHealth necromancerHealth;

    //Tracer Related
    //protected Vector2 initialPosition;
    // protected Vector2 midPoint;


    //Events
    public event Action OnChangeState;//, OnPrepareToDie;

    //Die Comunication related
    protected HordeManagerCommunicator hordeManagerCommunicator;

    //MeleeDamage Related
    protected AttackPlayer attackPlayer;

    //SpawnRelated
    protected SpawnManager spawnManager;
    [SerializeField] protected List<EnemyData> spawnableEnemies;
    protected Vector2 targetSpawnPostion;

    //Preview And Spawn Related
    [SerializeField] protected GameObject spawnSpotObject; // Assigned in the Editor.
    [SerializeField] protected bool usesSpawnPreview = false;

    //Animation related
    [SerializeField] protected NecromancerMovementAnimation necromancerAnim;

    /// <summary>
    /// Current number of minions.
    /// </summary>
    private int minionsAlive = 0;
    [SerializeField] protected int maxMinions = 9;

    #endregion

    protected override void Awake()
    {
        base.Awake();
        try
        {
            necromancerData = (NecromancerData)enemyData;
        }
        catch (Exception)
        {
            Debug.LogError("enemyData não é uma instancia de shooterArcherData! Coloque um shooterArcherData na referencia 'Enemy Data'");
        }

        try
        {
            invocationFXHandler = this.gameObject.GetComponent<HandleInvocationFX>();
        }
        catch (Exception)
        {
            Debug.LogError("Necromante não possui um script HandleInvocationFX");
        }
        try
        {
            necromancerHealth = this.gameObject.GetComponent<EnemyHealth>();
            necromancerHealth.OnTakeDamage += MakeItConfused;
            // Apply the same cleanup to the jumping hand when it dies.
        }
        catch (Exception)
        {
            Debug.LogError("Necromante não possui um script EnemyHealth");
        }
        try
        {
            changeTargetScript = this.gameObject.GetComponent<ChangeTarget>();
        }
        catch (Exception)
        {
            Debug.LogError("Necromante não possui um script ChangeTarget");
        }
        try
        {
            attackPlayer = GetComponent<AttackPlayer>();
        }
        catch (System.Exception)
        {
            Debug.LogError("Necromante não tem um AttackPlayer.cs");
        }
        try
        {
            necromancerAnim = GetComponentInChildren<NecromancerMovementAnimation>();
        }
        catch (System.Exception)
        {
            Debug.LogError("Necromante não tem um ShooterMovementAnimation.cs em um dos seus filhos");
        }
        try
        {
            spawnManager = AssetFinder.GetSpawnManager();
        }
        catch (System.Exception)
        {
            Debug.LogError("Necromante  não tem um ShooterMovementAnimation.cs em um dos seus filhos");
        }


    }

    protected override void OnEnable()
    {
        //Return to seeking and reset timers.
        state = NecromancerStates.seeking;
        currentAlertTimer = 0;
        currentSeekingTimer = 0;
        currentSpawnLockStateTimer = 0;
        firstTimeEnteringAlert = true;
        targetingPlayer = false;
        spawnSpotObject.SetActive(false);
        OnChangeState?.Invoke();
        base.OnEnable();

        minionsAlive = 0;
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

        if (state == NecromancerStates.seeking)
        {
            currentSpawnLockStateTimer += Time.deltaTime;
            currentSeekingTimer += Time.deltaTime;
        }
        else if (state == NecromancerStates.alert)
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
            case NecromancerStates.seeking:
                SeekingState();
                break;
            case NecromancerStates.escaping:
                EscapingState();
                break;
            case NecromancerStates.alert:
                AlertState();
                break;
            case NecromancerStates.firing:
                //Animation events spawn minions and reset seeking.
                break;
            default:
                break;
        }

        base.FixedUpdate();

    }

    #region States

    private void AlertState()
    {

        #region Obsolete
        //Debug.LogWarning("Alert!");

        /*
        // Restore this section if shooters should stop aiming when the player leaves range.
        //Return to seeking when distance exceeds distToAlert.
        if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) > NecromancerData.minDistToAim)
        {
            tracerHolder.SetActive(false);
            state = NecromancerStates.seeking;
            OnChangeState?.Invoke();
        }
        */

        #endregion

        if(minionsAlive >= maxMinions)
        {
            GoToSeeking();
        }

        //Restore this condition to calculate only once.
        float targetT = 0.5f;
        if (firstTimeEnteringAlert)
        {
            targetT = Random.Range(necromancerData.minDisOfSpawn, necromancerData.limitDisOfSpawn);
            firstTimeEnteringAlert = false;
        }
        targetSpawnPostion = CalculateSpawnPosition(targetT);
        spawnSpotObject.transform.position = new Vector3(targetSpawnPostion.x, targetSpawnPostion.y, spawnSpotObject.transform.position.z);

        // Cache the player position for the jumping enemy.
        if (currentAlertTimer >= necromancerData.timeInAlert  )
        {
            // Cache the firing destination here.

            state = NecromancerStates.firing;
            OnChangeState?.Invoke();
        }
    }

    private void EscapingState()
    {
        //Debug.LogWarning("Escaping!");

        //Calculate the best place to move to aim from
        BestPlaceToShoot();
        //Enter alert when distance is at most distToAlert.
        if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) < necromancerData.minDistToAim && minionsAlive < maxMinions)
        {
            //If the player is within aiming range,
            if (Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) > necromancerData.limitDistToAim)
            {
                TargetPlayer();
                currentAlertTimer = 0;
                if (usesSpawnPreview)
                {
                    spawnSpotObject.SetActive(true);
                }
                firstTimeEnteringAlert = true;
                state = NecromancerStates.alert;
                OnChangeState?.Invoke();
            }
        }
        else
        {
            spawnSpotObject.SetActive(false);
            state = NecromancerStates.seeking;
            OnChangeState?.Invoke();
        }
    }

    private void SeekingState()
    {
        //Hide the shooting preview.
        //Debug.LogWarning("Seeking!");
        //Calculate the best place to move to aim from
        BestPlaceToShoot();

        //Retreat if the player is too close.
        if ((Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) <= necromancerData.limitDistToAim))
        {
            currentSeekingTimer = 0;
            TargetPath();
            state = NecromancerStates.escaping;
            OnChangeState?.Invoke();
        }


        bool spawnLockTimeHasPassed = currentSpawnLockStateTimer >= necromancerData.timeInSpawnLockState;
        bool CooldownEnded = currentSeekingTimer >= necromancerData.timeInCooldown;
        bool minDistanceToAim = Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) < necromancerData.minDistToAim;
        bool playerIsInRange = Math.Abs(Vector2.Distance(transform.position, playerTransform.position)) > necromancerData.limitDistToAim;
        bool hasMinionToSpawn = minionsAlive < maxMinions;

        if (spawnLockTimeHasPassed && CooldownEnded && minDistanceToAim && playerIsInRange && hasMinionToSpawn)
        {
            //Initialize the alert timer.
            currentSeekingTimer = 0;
            TargetPlayer();
            currentAlertTimer = 0;
            if (usesSpawnPreview)
            {
                spawnSpotObject.SetActive(true);
            }
            firstTimeEnteringAlert = true;
            state = NecromancerStates.alert;
            OnChangeState?.Invoke();
        }
    }

    #endregion

    #region Movement Stuff
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

        Vector2 newLocation = thisPos + relationThisTarget.normalized * (necromancerData.limitDistToAim - distance);

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
        if (state == NecromancerStates.alert || state == NecromancerStates.firing)
        {
            return  ((Vector2)target.position - myRigidbody.position).normalized;

        }
        else
        {
            return ((Vector2)path.vectorPath[currentWayPoint] - myRigidbody.position).normalized;
        }
    }

    protected override float GetWalkSpeed()
    {
        if (Vector2.Distance(transform.position, target.position) < enemyData.minDistanceToIncreaseSpeed)
        {
            return  necromancerData.GetSpeed(state);

        }
        else
        {
            return  necromancerData.GetSpeed(state) * enemyData.speedMultiplierWhenNotVisible;
        }
    }

    #endregion

    public NecromancerStates GetCurrentState()
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
        state = NecromancerStates.seeking;
        spawnSpotObject.SetActive(false);
        TargetPath();
        OnChangeState?.Invoke();
    }

    public void MakeItConfused()
    {
        if (state == NecromancerStates.alert)
        {
            GoToSeeking();
        }

    }

    /// <summary>
    /// Fire through an animation event.
    /// </summary>
    public void SpawnEnemyByAnimation()
    {
        //Disable Spawn Preview
        spawnSpotObject.SetActive(false);

        foreach (EnemyData targetEnemyData in spawnableEnemies)
        {
            if(minionsAlive >= maxMinions)
            {
                Debug.Log("Maximo de minions atingido");
                return;
            }

            var enemy = spawnManager.SpawnEnemyAtPosition(targetEnemyData.enemyType, spawnSpotObject.transform);
            minionsAlive++;
            //Decrease the count when a minion dies.
            enemy.GetComponent<EnemyHealth>().OnZeroHealth += () => minionsAlive--;
        }
        
    }

    // Helpers that calculate necromancer minion spawn positions.
   
    public Vector2 CalculateSpawnPosition()
    {
        thisPos = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
        targetPos = new Vector2(playerTransform.position.x, playerTransform.position.y);
        float targetT= Random.Range(necromancerData.minDisOfSpawn, necromancerData.limitDisOfSpawn);
        
        return AuxInterpolator.Lerp(thisPos,targetPos, targetT);
    }

    public Vector2 CalculateSpawnPosition(float targetT)
    {
        thisPos = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
        targetPos = new Vector2(playerTransform.position.x, playerTransform.position.y);
        //float targetT = Random.Range(necromancerData.minDisOfSpawn, necromancerData.limitDisOfSpawn);

        return AuxInterpolator.Lerp(thisPos, targetPos, targetT);
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
