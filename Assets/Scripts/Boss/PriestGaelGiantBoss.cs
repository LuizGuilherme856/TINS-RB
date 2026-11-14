using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PriestGaelGiantBoss : MonoBehaviour
{
    [SerializeField] private float idleWaitTime = 3f;

    /// <summary>
    /// stateTree contains branches, each holding a list of nodes.
    /// Each node represents one state in the sequence.
    /// </summary>
    [SerializeField] private List<StateBranch> statesTree;

    /// <summary>
    /// A branch is a list of states.
    /// </summary>
    [Serializable] private class StateBranch 
    {
        [HideInInspector]
        public string name = "State Branch";
        public List<PriestGaelGiantStates> stateNodes; 
    
    }

    
    private PriestGaelGiantStates currentState;
    private PriestGaelGiantStates? previousState;
    private int branchIndex; //Current branch, containing a list of states.
    private int nodeIndex; //Current node/state.

    #region AttackStuff

    [Header("Attack Stuff")]
    public GameObject leftExplosionStrongPunch;
    public GameObject rightExplosionStrongPunch;
    //Side-mounted Varia objects emit radial bullet bursts during fastPunch.
    public VariaShootingManager leftVariaFastPunch, rightVariaFastPunch;
    public ObjectShooter laserLeftEye, laserRightEye;
    public MeteorManager meteorManager;
    public Animator[] eyeShotAnimators;
    private MinionSpawner minionSpawner;

    #endregion

    #region AnimatorStuff

    private Animator animator;
    
    //Animation names in the Animator.
   // private readonly string IDLE_NAME = "final boss idle hands down";
    private readonly string LEFT_STRONG_PUNCH_NAME = "final boss - hands down one hand attack left";
    private readonly string RIGHT_STRONG_PUNCH_NAME = "final boss - hands down one hand attack";
    private readonly string LAUGHING_FIREBALLS_NAME = "final boss hands on chest fireball";
    private readonly string LAUGHING_SPAWNING_NAME = "final boss hands on chest";
    private readonly string LAUGHING_DOWN_NAME = "final boss hands on chest then down";
    private readonly string LASER_SIDES_NAME = "final boss looking sides";
    //private readonly string FAST_PUNCH_START_NAME = "final boss punch sides - start";
    private readonly string FAST_PUNCH_RIGHT_NAME = "final boss punch sides Right loop";
    private readonly string FAST_PUNCH_LEFT_NAME = "final boss punch sides - loop";
    private readonly string FAST_PUNCH_BOTH_NAME = "final boss punch sides Both hands loop";
    private readonly string FAST_PUNCH_END_NAME = "final boss punch sides - end";
    #endregion

    #region AuxStateVariables

    //Timers controlling state duration.
    //private bool isIdleComplete;
    private bool isLeftStrongPunchComplete;
    private bool isRightStrongPunchComplete;
    private bool isLaughingFireballsComplete;
    private bool isLaughingSpawningComplete;
    private bool isLaughingDownComplete;
    private bool isLaserSidesComplete;
    //private bool isFastPunchStartComplete;
    private bool isFastPunchRightComplete;
    private bool isFastPunchLeftComplete;
    private bool isFastPunchBothComplete;
    private bool isFastPunchEndComplete;

    //Attack-related.
    private bool isShootingLaser = false;

    //Animation-event setters mark the current state as completed.
    //public void SetIsIdleComplete() => isIdleComplete = true;
    public void SetIsLeftStrongPunchComplete() => isLeftStrongPunchComplete = true;
    public void SetIsRightStrongPunchComplete() => isRightStrongPunchComplete = true;
    public void SetIsLaughingFireballsComplete() => isLaughingFireballsComplete = true;
    public void SetIsLaughingSpawningComplete() => isLaughingSpawningComplete = true;
    public void SetIsLaughingDownComplete() => isLaughingDownComplete = true;
    public void SetIsLaserSidesComplete() => isLaserSidesComplete = true;
    public void SetIsFastPunchRightComplete() => isFastPunchRightComplete = true;
    public void SetIsFastPunchLeftComplete() => isFastPunchLeftComplete = true;
    public void SetIsFastPunchBothComplete() => isFastPunchBothComplete = true;
    //public void SetIsFastPunchStartComplete() => isFastPunchStartComplete = true;
    public void SetIsFastPunchEndComplete() => isFastPunchEndComplete = true;

    private enum PriestGaelGiantStates
    {
        Idle, leftStrongPunch, rightStrongPunch, laughingFireballs, laughingSpawning, laughingDown,
        laserSides_NotWorking, fastPunchRight, fastPunchLeft, fastPunchBoth
    }

    #endregion


    private void Awake()
    {
        animator = GetComponent<Animator>();
        minionSpawner = GetComponent<MinionSpawner>();
    }

    void Start()
    {
        StartCoroutine(StateMachine());
        //Configure Varia to use an elliptical firing pattern.
        leftVariaFastPunch.SetEllipticalPattern();
        rightVariaFastPunch.SetEllipticalPattern();
    }

    /// <summary>
    /// Boss state machine.
    /// </summary>
    private IEnumerator StateMachine()
    {
        yield return new WaitForSeconds(3.5f); // Wait one second before starting the state machine; no specific dependency is documented.

         nodeIndex = 0;

        branchIndex = Random.Range(0, statesTree.Count);
        previousState = null;
        currentState = statesTree[branchIndex].stateNodes[nodeIndex];

        while (true)
        {
            switch (currentState)
            {
                case PriestGaelGiantStates.Idle:
                    yield return StartCoroutine(IdleState());
                    break;
                case PriestGaelGiantStates.leftStrongPunch:
                    yield return StartCoroutine(LeftStrongPunchState());
                    break;
                case PriestGaelGiantStates.rightStrongPunch:
                    yield return StartCoroutine(RightStrongPunchState());
                    break;
                case PriestGaelGiantStates.laughingFireballs:
                    yield return StartCoroutine(LaughingFireballsState());
                    break;
                case PriestGaelGiantStates.laughingSpawning:
                    yield return StartCoroutine(LaughingSpawningState());
                    break;
                case PriestGaelGiantStates.laughingDown:
                    yield return StartCoroutine(LaughingDownState());
                    break;
                case PriestGaelGiantStates.laserSides_NotWorking:
                    yield return StartCoroutine(LaserSidesState());
                    break;
                case PriestGaelGiantStates.fastPunchRight:
                    yield return StartCoroutine(FastPunchRightState());
                    break;
                case PriestGaelGiantStates.fastPunchLeft:
                    yield return StartCoroutine(FastPunchLeftState());
                    break;
                case PriestGaelGiantStates.fastPunchBoth:
                    yield return StartCoroutine(FastPunchBothState());
                    break;
                default:
                    break;
            }

            //Continue while nodes remain in the current branch.
            if (nodeIndex < statesTree[branchIndex].stateNodes.Count - 1)
            {
                nodeIndex++;
            }
            else
            {
                //After the last node, choose another random branch.
                nodeIndex = 0;
                branchIndex = Random.Range(0, statesTree.Count);
            }

            previousState = currentState;
            currentState = statesTree[branchIndex].stateNodes[nodeIndex];
        }
    }

    #region Strong Punch

    private IEnumerator LeftStrongPunchState()
    {
        isLeftStrongPunchComplete = false;

        animator.Play(LEFT_STRONG_PUNCH_NAME);
        while(isLeftStrongPunchComplete == false)
        {
            yield return null;
        }

        //animator.SetTrigger("Idle");

        //yield return new WaitForSeconds(0.5f);

        //if(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != IDLE_NAME)
        //{
        // Debug.LogWarning("Idle was expected; forcing the idle state");
        //    animator.Play(IDLE_NAME);
        //}
    }

    public void LeftStrongPunch_AnimationEvent()
    {
        leftExplosionStrongPunch.SetActive(true);
    }

    private IEnumerator RightStrongPunchState()
    {
        isRightStrongPunchComplete = false;

        animator.Play(RIGHT_STRONG_PUNCH_NAME);
        while (!isRightStrongPunchComplete)
        {
            yield return null;
        }
    }

    public void RightStrongPunch_AnimationEvent()
    {
        rightExplosionStrongPunch.SetActive(true);
    }

    #endregion

    #region FastPunch

    private IEnumerator FastPunchRightState()
    {
        isFastPunchRightComplete = false;

        //If a fastPunch clip is already playing, continue without replaying its start animation.
        //Otherwise, trigger fastPunch so it can reach its ending animation.
        if(CurrentAnimationIsFastPunch() == false)
        {
            animator.SetTrigger("FastPunchRight");
        }
        else if (previousState != null && previousState != PriestGaelGiantStates.fastPunchRight) //Play the animation unless the current attack is specifically fast RIGHT.
        {
            animator.Play(FAST_PUNCH_RIGHT_NAME);
        }

        while (!isFastPunchRightComplete)
        {
            yield return null;
        }

        yield return StartCoroutine(TestFastPunchEnd_Routine());

    }

    /// <summary>
    /// Called by the animation to start firing.
    /// </summary>
    public void FastPunchRight_AnimationEvent()
    {
        StartCoroutine(rightVariaFastPunch.ForceVariaShoot_Routine(1, 0));
    }

    private IEnumerator FastPunchLeftState()
    {
        isFastPunchLeftComplete = false;

        //If a fastPunch clip is already playing, continue without replaying its start animation.
        //Otherwise, trigger fastPunch so it can reach its ending animation.
        if (CurrentAnimationIsFastPunch() == false)
        {
            animator.SetTrigger("FastPunchLeft");
        }
        else if(previousState != null && previousState != PriestGaelGiantStates.fastPunchLeft) //Play the animation unless the current attack is specifically fast left.
        {
            animator.Play(FAST_PUNCH_LEFT_NAME);
        }

        while (!isFastPunchLeftComplete)
        {
            yield return null;
        }


        yield return StartCoroutine(TestFastPunchEnd_Routine());

        Debug.Log("Acabou o left");
    }


    /// <summary>
    /// Called by the animation to start firing.
    /// </summary>
    public void FastPunchLeft_AnimationEvent()
    {
        StartCoroutine(leftVariaFastPunch.ForceVariaShoot_Routine(1,0));
    }

    private IEnumerator FastPunchBothState()
    {
        isFastPunchBothComplete = false;

        //If a fastPunch clip is already playing, continue without replaying its start animation.
        //Otherwise, trigger fastPunch so it can reach its ending animation.
        if (CurrentAnimationIsFastPunch() == false)
        {
            animator.SetTrigger("FastPunchBoth");
        }
        else if (previousState != null && previousState != PriestGaelGiantStates.fastPunchBoth) //Play the animation unless the current attack is specifically fast both.
        {
            animator.Play(FAST_PUNCH_BOTH_NAME);
        }

        while (!isFastPunchBothComplete) yield return null;

        yield return StartCoroutine(TestFastPunchEnd_Routine());
    }

    /// <summary>
    /// Called by the animation to start firing.
    /// </summary>
    public void FastPunchBoth_AnimationEvent()
    {
        StartCoroutine(leftVariaFastPunch.ForceVariaShoot_Routine(1, 0));
        StartCoroutine(rightVariaFastPunch.ForceVariaShoot_Routine(1, 0));
    }

    #region Fast punch Aux

    private bool CurrentAnimationIsFastPunch()
    {
        string clipName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        return clipName == FAST_PUNCH_RIGHT_NAME || clipName == FAST_PUNCH_LEFT_NAME || clipName == FAST_PUNCH_BOTH_NAME;
    }
    private bool StateIsFastPunch(PriestGaelGiantStates state)
    {
        return state == PriestGaelGiantStates.fastPunchRight
            || state == PriestGaelGiantStates.fastPunchLeft
            || state == PriestGaelGiantStates.fastPunchBoth;
    }

    private bool NextStateIsFastPunch()
    {
        //Keep the index within the list bounds.
        if (nodeIndex + 1 >= statesTree[branchIndex].stateNodes.Count)
        {
            return false;
        }

        return statesTree[branchIndex].stateNodes[nodeIndex + 1] == PriestGaelGiantStates.fastPunchRight
            || statesTree[branchIndex].stateNodes[nodeIndex + 1] == PriestGaelGiantStates.fastPunchLeft
            || statesTree[branchIndex].stateNodes[nodeIndex + 1] == PriestGaelGiantStates.fastPunchBoth;
    }

    /// <summary>
    /// Check whether the fastPunch ending animation is required.
    /// </summary>
    private IEnumerator TestFastPunchEnd_Routine()
    {
        //Inspect the next state; if it is not a fastPunch state,
        if (NextStateIsFastPunch() == false)
        {
            isFastPunchEndComplete = false;
            //play the fastPunch ending animation and
            animator.Play(FAST_PUNCH_END_NAME);
            while (!isFastPunchEndComplete)
            {
                // wait for it to finish.
                yield return null;
            }

            Debug.Log("Acabou o fast punch");
        }
    }

    #endregion

    #endregion

    #region Laughing

    private IEnumerator LaughingFireballsState()
    {
        isLaughingFireballsComplete = false;

        animator.Play(LAUGHING_FIREBALLS_NAME);
        while (!isLaughingFireballsComplete)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Called by the animation to start meteor falls.
    /// </summary>
    public void LaughingFireballs_Animation()
    {
        StartCoroutine(meteorManager.ActivateMeteorFall_Routine());
    }

    private IEnumerator LaughingSpawningState()
    {
        isLaughingSpawningComplete = false;

        animator.Play(LAUGHING_SPAWNING_NAME);
        while (!isLaughingSpawningComplete)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Called by the animation to spawn minions.
    /// </summary>
    public void SpawnMinions_Animation()
    {
        StartCoroutine(minionSpawner.SpawnEnemies());
    }

    private IEnumerator LaughingDownState()
    {
        isLaughingDownComplete = false;

        animator.Play(LAUGHING_DOWN_NAME);
        while (!isLaughingDownComplete)
        {
            yield return null;
        }
    }

    public void StartShootingLaser_Animation()
    {
        isShootingLaser = true;
        StartCoroutine(ShootLaser_Routine());
    }

    private IEnumerator ShootLaser_Routine(float delayBetweenShots = 0.4f)
    {
        while (isShootingLaser)
        {

            foreach (var anim in eyeShotAnimators) anim.SetTrigger("Shoot");

            yield return new WaitForSeconds(0.1f);

            laserLeftEye.LaunchObject();
            laserRightEye.LaunchObject();

            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    public void StopShootingLaser_Animation()
    {
        isShootingLaser = false;
    }




    #endregion

    private IEnumerator LaserSidesState()
    {
        isLaserSidesComplete = false;

        animator.Play(LASER_SIDES_NAME);
        while (!isLaserSidesComplete)
        {
            yield return null;
        }
    }

    private IEnumerator IdleState()
    {
        yield return new WaitForSeconds(idleWaitTime);
    }





}
