using System;
using UnityEngine;

public class JumpingEnemy : MonoBehaviour
{


    //TODO: Generalize this script and specialize it for the hand enemy.

    [SerializeField] protected JumpingEnemyData jumpingEnemyData;

    [SerializeField] protected EnemyAI enemyAI;

    //Health Related
    protected EnemyHealth jumperHealth;
    [SerializeField]  protected GameObject baseEnemy; //Assigned in the Editor: the grounded enemy before jumping.
    protected EnemyHealth baseEnemyHealth;
    //Event related
    public event Action OnLand;

    [SerializeField] protected GameObject OnLandObject;// May spawn an explosion after landing instead of restoring the grounded enemy.

    [Tooltip("True = Ativara o objeto" +
        "\n False = Spawnará como novo objeto (sem ser filho deste)")]
    [SerializeField] protected bool activateOnLand = true;


    //Leaping realated
    //Animation duration.
    [SerializeField]  protected float currentLeapingDuration = 2.0f;
    //Interface that synchronizes the landing point with the grounded enemy.
    [SerializeField] protected ILander landerHolder;

    [SerializeField] protected float currentLeapTimer;

    //Trajectory calculation related
    [SerializeField] protected Vector2 playerPos;//, playerTransform;
    [Tooltip("Tempo ao longo da interpolação ou o rate que caminhamos na interpolação,Tem que ser nomalizado entre 0 e 1")]
    [SerializeField] protected float functionT;
    //Distance beetwen player and enemy

    protected Vector2 vectorThisToTarget;

    [SerializeField]  protected Vector2 midPoint;

    [SerializeField] protected Vector2 initialPosition;

    protected Vector2 targetPos;

    //Scale Related
    protected float baseScaleMul = 1;
    protected Vector3 baseScale;

    //Tracer Related
    //Object containing TracerManager and LineRenderer.
    [SerializeField] protected GameObject tracerHolder;//Assigned in the Editor.
    protected TracerManager tracerManager;
    [Tooltip("Quantidade de keyPoints no preview de Trajetoria do line render")]
    [SerializeField] protected int stepQuantity = 100; //Consider connecting this to graphics-quality settings.
    [SerializeField] protected int lookAtPointsQuantity = 4;
    Vector2 currentPosition;
    Vector3[] LookAtPoints;
    private double lastLookPassedStep = -1;

    //Colission Related
    //Collider used to grab the player's head.
    [SerializeField] protected GameObject grabCollision; //Assigned in the Editor.
    //Tracks ToggleGrabCollisionWithTime.
    protected bool canGrab;
    // When true, the enemy never attaches to the player's head.
    [Tooltip("true = nunca irá para a cabeça do player, setado pelo jumpingEnemyData")]
    [SerializeField] protected bool neverGrab = false;
    //JumpingManager Related
    public event Action OnPrepareToDie;

    protected void Awake()
    {
        
        try
        {
            landerHolder = (ILander)enemyAI;
        }
        catch (Exception)
        {

            Debug.LogError("O inimigo base não implementa a interface ILander");
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
            jumperHealth = this.gameObject.GetComponent<EnemyHealth>();
            jumperHealth.OnInstaDieWithNoCorpse += PreparingToDie;
            jumperHealth.OnZeroHealth += PreparingToDie;
        }
        catch (Exception)
        {
            Debug.LogError("O prefab associado ao Jumper Enemy não possui um script EnemyHealth");
        }
        try
        {
            baseEnemyHealth = baseEnemy.gameObject.GetComponent<EnemyHealth>();
        }
        catch (Exception)
        {
            Debug.LogError("O prefab associado ao Base Enemy do Jumper Enemy não possui um script EnemyHealth");
        }

        baseScale = this.transform.localScale;

        neverGrab = jumpingEnemyData.neverGrab;
    }


    protected void OnEnable()
    {
        
        //Copy health from the grounded enemy that spawned this jumping version.
        jumperHealth.SetMaxHealth(baseEnemyHealth.maxHealth);
        jumperHealth.SetCurrentHealth(baseEnemyHealth.currentHealth);
        //Reset the trajectory before resetting the tracer.
        TrajetoryReset();
        TracerReset();
        //Disable the grab collider initially.
        ToggleGrabCollisionWithTime(0);
        //After spawning, notify HordeManager that pooled reactivation must not update this again.
        //hordeManagerCommunicator.SetUpdateRunData(false);

    }

    protected void OnDisable()
    {
        // TechBooh: death, a wave change, or pooling may disable the enemy
        // before landing. Its tracer may not be a child and would otherwise remain in the scene.
        if (tracerHolder != null)
        {
            tracerHolder.SetActive(false);
        }
    }

    protected void FixedUpdate()
    {
        currentLeapTimer += Time.fixedDeltaTime;
        functionT = currentLeapTimer / currentLeapingDuration;


        //Calculate the Bezier curve from the start, midpoint, and landing point.
        targetPos = AuxInterpolator.QuadraticCurve(initialPosition, midPoint, playerPos, functionT);
        //Adjust scale according to trajectory progress.
        AuxInterpolator.AdjustScale(this.gameObject,baseScale,baseScaleMul,jumpingEnemyData.midPointScaleMul, functionT);

        ToggleGrabCollisionWithTime(functionT);

        PlotTrajectory(functionT);
        //The shadow could instead use linear interpolation without a midpoint.
        this.gameObject.transform.position = new Vector3(targetPos.x, targetPos.y, this.gameObject.transform.position.z);
        //After landing, reactivate the grounded enemy and trigger its next action.
        if (currentLeapTimer / currentLeapingDuration >= 1)
        {
            // True activates the existing object; false spawns a separate unparented instance.
            if (activateOnLand)
            {
                OnLandObject.gameObject.transform.position = new Vector3(playerPos.x, playerPos.y, OnLandObject.gameObject.transform.position.z);
                OnLandObject.SetActive(true);
            }
            else 
            {
                Instantiate(OnLandObject, new Vector3(playerPos.x, playerPos.y, OnLandObject.gameObject.transform.position.z),OnLandObject.transform.rotation);
            }
            tracerHolder.SetActive(false);
            OnLand?.Invoke();
            this.gameObject.SetActive(false);
        }
    }



    /// <summary>
    ///  Reset state, rebuild the tracer trajectory, and generate the configured number of look-at points.
    /// </summary>
    private void TracerReset()
    {
        //Enable the tracer to display the trajectory.
        lastLookPassedStep = -1;
        tracerHolder.SetActive(true);
        tracerManager.ResetLastPassedStep();
        SetupLookAtPoints();
        tracerManager.UpdateGlobalKeyPoints(initialPosition, midPoint, playerPos, stepQuantity);
    }

    /// <summary>
    /// Reset trajectory state and recalculate the enemy's movement path.
    /// </summary>
    private void TrajetoryReset()
    {
        //Reset the starting position and timers.
        currentLeapTimer = 0;

        this.transform.localScale = baseScale;

        initialPosition = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
        //Calculate the landing point.
        playerPos = landerHolder.GetLandingSpot();
        //Calculate jump duration.
        currentLeapingDuration = CalculateNewLeapDuration(Vector2.Distance(initialPosition, playerPos));
        //Calculate the midpoint.
        midPoint = AuxInterpolator.CalculateMidPoint(initialPosition, playerPos);
    }

    /// <summary>
    /// Generate the sequence of points the enemy faces during the jump.
    /// </summary>
    private void SetupLookAtPoints()
    {
        LookAtPoints = tracerManager.GetLookAtKeyPoints(initialPosition, midPoint, playerPos, lookAtPointsQuantity, stepQuantity); 
    }
    /// <summary>
    /// Generate a trajectory without modifying the state currently moving the enemy.
    /// </summary>
    protected void PlotTrajectory(float t)
    {

        currentPosition = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
        AdjustLookAt(currentLeapTimer / currentLeapingDuration);
        // Potential optimization: reduce stepQuantity from its configured value toward 1 as the jump progresses.
        tracerManager.RemoveTrajectoryWithTime(stepQuantity,t);
    }


    /// <summary>
    /// 
    /// Requires an up-to-date currentPosition.
    /// </summary>
    protected void AdjustLookAt(float t)
    {
        
        double tempDiv = Math.Floor((t / ((1.0f / (float)lookAtPointsQuantity))));//Current trajectory point index.
        if (tempDiv <= lookAtPointsQuantity)// Keep the point index within bounds.
        {
            if (tempDiv > lastLookPassedStep) // The current point changed since the last update.
            {
                //Movement may cross multiple points between frames;
                //advance the look-at target accordingly.
                for (int i = 0; i < (tempDiv - lastLookPassedStep); i++) 
                {
                    if (i == (tempDiv - lastLookPassedStep) - 1)
                    {
                        Vector2 dir = playerPos - currentPosition;
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90);
                    }
                    else // At the last point, face the player.
                    {
                        
                        Vector2 dir = (Vector2)LookAtPoints[i] - currentPosition;
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90);

                    }
                }
                lastLookPassedStep = (int)tempDiv; // Update the last observed point.
            }
        }     
    }


    /// <summary>
    /// The grounded enemy must assign this before spawning its jumping version.
    /// </summary>
    /// <param name="targetEnemyData"></param>
    public void SetEnemyData(JumpingEnemyData targetEnemyData)
    {
        this.jumpingEnemyData = targetEnemyData;
    }

    /// <summary>
    /// Calculate jump duration from current distance, maximum distance, and maximum jump time.
    /// </summary>
    protected float CalculateNewLeapDuration(float distance)
    {
        return ((jumpingEnemyData.maxTimeLeaping*distance)/jumpingEnemyData.maxLeapingDistance);
    }

    //Colission Related
    /// <summary>
    /// Enable the head-grabber collider according to normalized time between 0 and 1.
    /// </summary>
    /// <param name="t">Normalized time between 0 and 1.</param>
    protected void ToggleGrabCollisionWithTime(float t) 
    {
        if (neverGrab)
        {
            return;
        }
        if (0.75f < t)
        {
            grabCollision.SetActive(true);
            canGrab = true;
        }
        else 
        {
            grabCollision.SetActive(false);
            canGrab = false;
        }
    
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.CompareTag("PlayerHead"))
        {
            if(canGrab)
            {
                //Check player invulnerability in HandleAttackOnPlayer.
                bool headIsGrabable = collision.gameObject.GetComponentInParent<HandleAttackOnPlayer>().OnHeadEffect(jumpingEnemyData.enemyType,jumperHealth.currentHealth,jumpingEnemyData);
                //Deactivate the enemy only if attaching to the head is allowed.
                if (headIsGrabable)
                {
                    tracerHolder.gameObject.SetActive(false);
                    //If integrated with wave spawning, apply the full death bookkeeping used by the bomber.
                    //Consider EnemyHealth.InstaDieWithNoCorpse.
                    jumperHealth.InstaDieWithNoCorpse(); // TODO: Confirm with Luiz whether a new wave may begin while the hand remains on the player's head.
                    this.gameObject.SetActive(false);
                }

            }
            
        }
    }

    //Prepare state before enemy death.
    protected void PreparingToDie()
    {
        tracerHolder.SetActive(false);
        //hordeManagerCommunicator.SetUpdateRunData(true);
        OnPrepareToDie?.Invoke();
    }


}
