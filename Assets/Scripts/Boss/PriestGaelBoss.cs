using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestGaelBoss : MonoBehaviour
{
    [SerializeField] private Transform topPoint; // Upper movement target for the boss.
    [SerializeField] private Transform leftPoint; // Left movement target for the boss.
    [SerializeField] private Transform rightPoint; // Right movement target for the boss.
    [SerializeField] private Transform farRightPoint; // Far-right movement target for the boss.
    [SerializeField] private Transform farLeftPoint; // Far-left movement target for the boss.
    [SerializeField] private float idleWaitTime = 2f; // Time spent in idle.
    [SerializeField] private float moveSpeed = 2f; // Boss movement speed.

    [SerializeField] private Transform finalFightPoint; // Final retreat destination.

    [SerializeField] private List<PriestGaelState> stateOrder = new List<PriestGaelState> { PriestGaelState.Idle, PriestGaelState.EnergyBall, PriestGaelState.Laugh, PriestGaelState.Claws, PriestGaelState.LaughSpawnMinion, PriestGaelState.MultipleEnergyBalls };

    public PriestGaelState currentState; // Current boss state.
    public Animator escapeAnimator; // Animator used for the retreat animation.

    [HideInInspector] public bool escapeMode = false; // Boss retreat mode.
    [HideInInspector] public bool readyToEscape = false; // Helper flag indicating readiness to retreat.
    [HideInInspector] public bool transformationMode = false; // Ready to transform.
    ///[HideInInspector] public bool readyToTransform = false; // Ready to transform.
    public event Action OnReadyToTransform;
    [Space(30)]
    [Header("2 fase e transformação")]
    [SerializeField] private Transform transformationPoint_1;
    [SerializeField] private Transform transformationPoint_2_HighestPoint, transformationPoint_3_lowestPoint;

    [SerializeField] MMF_Player dashFeedback;

    #region private variables
    private int currentStateIndex = 0; // Current state index in the state list.
    private bool isEnergyBallComplete, isLaughComplete, isClawsComplete, isHorizontalClawComplete;
    private bool isLaughSpawnMinionComplete, isMultipleEnergyBallsComplete;
    private PriestGaelBossAttacks bossAttacks; // Boss attack controller reference.
    private EnemyHealth enemyHealth;
    private bool isTransformSequenceRunning = false;
    #endregion

    private void Awake()
    {
        bossAttacks = GetComponent<PriestGaelBossAttacks>(); // Get the boss attack component.
        enemyHealth = GetComponent<EnemyHealth>();
        //Assign the health reference to the UI.
        FindFirstObjectByType<BossFightUiInfo>().SetBossHealth(enemyHealth);
    }

    void Start()
    {
        currentState = stateOrder[currentStateIndex]; // Set the initial state.
        StartCoroutine(StateMachine()); // Start the state machine.
    }

    /// <summary>
    /// Coroutine state machine: remain in a state until its coroutine completes.
    /// </summary>
    private IEnumerator StateMachine()
    {
        yield return new WaitForSeconds(1f); // Wait one second before starting the state machine.

        while (true)
        {

            #region Escape And Transform


            if (escapeMode)
            {
                //Handle Gael's first-phase retreat; see GaelEscapeManager.cs.
                moveSpeed *= 1.5f; //Increase movement speed.
                yield return StartCoroutine(MoveToPosition(topPoint.position));
                readyToEscape = true;
                yield return StartCoroutine(IdleState());
                escapeAnimator.SetTrigger("Escape");
                dashFeedback.PlayFeedbacks();
                yield return new WaitForSeconds(5f);
                //moveSpeed *= 3f; //Increase movement speed.
                //yield return StartCoroutine(MoveToPosition(finalFightPoint.position));

                StopAllCoroutines(); //Stop all coroutines.
                gameObject.SetActive(false); //Deactivate the boss.

                break;
                //yield return null; //Stop the state machine.
            }
            else if(transformationMode)
            {
                try
                {
                    if (isTransformSequenceRunning == false)
                    {
                        StartCoroutine(ForceTransform());
                    }

                }
                catch (Exception e)
                {
                    OnReadyToTransform?.Invoke();
                    gameObject.SetActive(false); 
                    Debug.LogError("Erro ao iniciar a transformação do boss: \n" + e.Message);

                }

                break;

            }

            #endregion


            //state machine
            switch (currentState)
            {
                case PriestGaelState.Idle:
                    yield return StartCoroutine(IdleState());
                    break;
                case PriestGaelState.EnergyBall:
                    yield return StartCoroutine(EnergyBallState());
                    break;
                case PriestGaelState.Laugh:
                    yield return StartCoroutine(LaughState());
                    break;
                case PriestGaelState.Claws:
                    yield return StartCoroutine(ClawsState());
                    break;
                case PriestGaelState.LaughSpawnMinion:
                    yield return StartCoroutine(LaughSpawnMinionState());
                    break;
                case PriestGaelState.MultipleEnergyBalls:
                    yield return StartCoroutine(MultipleEnergyBallsState());
                    break;
                case PriestGaelState.HorizontalClaws:
                    yield return StartCoroutine(HorizontalClawState());
                    break;
            }

            // Use modulo to wrap the next state index within the list.
            currentStateIndex = (currentStateIndex + 1) % stateOrder.Count;
            currentState = stateOrder[currentStateIndex];
        }

        
    }


    public IEnumerator ForceTransform()
    {
        if (isTransformSequenceRunning)
        {
            yield break;
        }

        isTransformSequenceRunning = true;
        transformationMode = true;
        moveSpeed *= 1.5f; //Increase movement speed.
        yield return StartCoroutine(MoveToPosition(topPoint.position));

        yield return StartCoroutine(IdleState());
        OnReadyToTransform?.Invoke();
        //Move upward.
        yield return StartCoroutine(MoveToPosition(transformationPoint_1.position));
        //Move slowly for a short distance to smooth the transition.
        moveSpeed *= 0.4f;
        yield return StartCoroutine(MoveToPosition(transformationPoint_2_HighestPoint.position));

        //Disable the collider and fall quickly.
        GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        GetComponent<CapsuleCollider2D>().enabled = false;
        dashFeedback.PlayFeedbacks(); //Reuse dash movement here, although this is not a full dash.
        moveSpeed *= 5f;
        yield return StartCoroutine(MoveToPosition(transformationPoint_3_lowestPoint.position));
        yield return new WaitForSeconds(10f);
        StopAllCoroutines(); //Stop all coroutines.
        gameObject.SetActive(false); //Deactivate the boss.
    }


    private IEnumerator IdleState()
    {
        // Move toward the idle position before stopping there.
        Vector3 pos = topPoint.position;
        yield return StartCoroutine(MoveToPosition(pos));
        yield return new WaitForSeconds(idleWaitTime);
    }

    private IEnumerator EnergyBallState()
    {
        // Wait until the destination is reached, then attack.
        var pos = leftPoint.position;
        yield return StartCoroutine(MoveToPosition(pos));

        isEnergyBallComplete = false;

        StartCoroutine(TriggerEnergyBallAttack());

        yield return new WaitUntil(() => isEnergyBallComplete);
    }

    private IEnumerator LaughState()
    {
        // Wait until the destination is reached, then start the attack animation.
        var pos = topPoint.position;
        yield return StartCoroutine(MoveToPosition(pos));
        // Mark the attack incomplete before starting its coroutine.
        isLaughComplete = false;
        StartCoroutine(TriggerLaughAttack());
        // WaitUntil completion before leaving the state.
        yield return new WaitUntil(() => isLaughComplete);
    }

    private IEnumerator ClawsState()
    {
        // Wait until the destination is reached, then start the attack animation.
        var pos = topPoint.position;
        yield return StartCoroutine(MoveToPosition(pos));
        isClawsComplete = false;
        StartCoroutine(TriggerClawsAttack());
        yield return new WaitUntil(() => isClawsComplete);
    }

    /// <summary>
    /// Move right to left while launching the claw-animation slash.
    /// </summary>
    private IEnumerator HorizontalClawState()
    {
        // Number of back-and-forth passes.
        int iterations = 1; //Currently one pass, but the count may be increased.

        for (int i = 0; i < iterations; i++)
        {
            // Move right and wait until the destination is reached.
            yield return StartCoroutine(MoveToPosition(farRightPoint.position));

            // Activate the attack.
            StartCoroutine(TriggerHorizontalClawAttack());

            //moveSpeed *= 1.5f; // Increase boss movement speed.
            // Continue moving left until the attack finishes.
            while (!isHorizontalClawComplete)
            {
                transform.position = Vector2.MoveTowards(transform.position, farLeftPoint.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            // Clamp to the destination to ensure movement stops there.
            yield return StartCoroutine(MoveToPosition(farLeftPoint.position));

            // Activate the attack.
            StartCoroutine(TriggerHorizontalClawAttack());
            // Continue moving right until the attack finishes.
            while (!isHorizontalClawComplete)
            {
                transform.position = Vector2.MoveTowards(transform.position, farRightPoint.position, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        //moveSpeed /= 1.5f; // Restore the original speed.
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator LaughSpawnMinionState()
    {
        // Wait until the destination is reached, then start the attack animation.
        var pos = topPoint.position;
        yield return StartCoroutine(MoveToPosition(pos));

        isLaughSpawnMinionComplete = false;
        StartCoroutine(TriggerLaughSpawnMinionAttack());
        yield return new WaitUntil(() => isLaughSpawnMinionComplete);
    }

    private IEnumerator MultipleEnergyBallsState()
    {
        // Wait until the destination is reached, then start the attack animation.
        var pos = rightPoint.position;
        yield return StartCoroutine(MoveToPosition(pos));
        isMultipleEnergyBallsComplete = false;
        StartCoroutine(TriggerMultipleEnergyBallsAttack());
        yield return new WaitUntil(() => isMultipleEnergyBallsComplete);
    }

    /// <summary>
    /// Move the boss to the requested position.
    /// </summary>
    private IEnumerator MoveToPosition(Vector3 pos)
    {
        while (Vector2.Distance(pos, transform.position) > 1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, pos, moveSpeed * Time.deltaTime);
            yield return null;
        }


    }



    #region Attack Triggers

    // These wrappers run attack coroutines and mark completion when they finish.

    private IEnumerator TriggerEnergyBallAttack()
    {
        yield return StartCoroutine(bossAttacks.EnergyBallAttack());
        isEnergyBallComplete = true;
    }

    private IEnumerator TriggerLaughAttack()
    {
        yield return StartCoroutine(bossAttacks.LaughAttack());
        isLaughComplete = true;
    }

    private IEnumerator TriggerClawsAttack()
    {
        yield return StartCoroutine(bossAttacks.ClawsAttack());
        isClawsComplete = true;
    }

    private IEnumerator TriggerHorizontalClawAttack()
    {
        isHorizontalClawComplete = false;
        yield return StartCoroutine(bossAttacks.HorizontalClawAttack());
        isHorizontalClawComplete = true;
    }

    private IEnumerator TriggerLaughSpawnMinionAttack()
    {
        yield return StartCoroutine(bossAttacks.LaughSpawnMinionAttack());
        isLaughSpawnMinionComplete = true;
    }

    private IEnumerator TriggerMultipleEnergyBallsAttack()
    {
        yield return StartCoroutine(bossAttacks.MultipleEnergyBallsAttack());
        isMultipleEnergyBallsComplete = true;
    }

    #endregion
}


public enum PriestGaelState
{
    Idle,
    EnergyBall,
    Laugh,
    Claws,
    LaughSpawnMinion,
    MultipleEnergyBalls,
    HorizontalClaws
}
