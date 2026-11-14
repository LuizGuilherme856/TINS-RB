using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LuciferBoss : MonoBehaviour
{
    [SerializeField] protected LuciferData luciData; //Assigned in the Editor.

    protected EnemyHealth luciHealth;

    /// <summary>
    /// stateTree contains branches, each holding a list of nodes.
    /// Each node represents one state in the sequence.
    /// </summary>
    [SerializeField] private List<StateBranch> statesTree;

    /// <summary>
    /// A branch is a list of states.
    /// </summary>
    [Serializable]
    private class StateBranch
    {
        [HideInInspector]
        public string name = "State Branch";
        public List<LuciferS1> stateNodes;

    }


    private LuciferS1 currentState;
    private LuciferS1? previousState;
    private int branchIndex; //Current branch, containing a list of states.
    private int nodeIndex; //Current node/state.

    #region AttackStuff

    [Header("Attack Stuff")]

    [SerializeField] protected LuciAttacks luciAttacksScript;

    [SerializeField] protected LuciBossAttack followBulletAttack; // Assigned in the Editor.

    [System.Serializable]
    public class LuciBossAttack
    {
        [SerializeField] private GameObject attackPrefab;
        [SerializeField] private Transform attackSpawnPoint;
        [SerializeField] private int maxAttacks = 3;
        [HideInInspector] public int currentAttacks = 0;

        public GameObject AttackPrefab => attackPrefab;
        public Transform AttackSpawnPoint => attackSpawnPoint;
        public int MaxAttacks => maxAttacks;
    }




    #endregion

    #region AnimatorStuff

    private Animator animator;

    //Animation names in the Animator.
    private readonly string IDLE_NAME = "Idle";
    private readonly string SHOOT_NAME = "Shoot";
    private readonly string SHOOT_LASER_NAME = "ShootLaser";
    private readonly string GAS_NAME = "Gas";
    private readonly string GAS_EXPLODE_NAME = "GasExplode";
    private readonly string HEAL_NAME = "HealPillar";
    private readonly string WINDL_NAME = "WindLeft";
    private readonly string WINDR_NAME = "WindRight";
    private readonly string SPAWN_NAME = "Spawn";

    #endregion

    #region AuxStateVariables

    //Timers controlling state duration.

    protected bool castAnimationComplete = false;



    public void SetCastComplete() => castAnimationComplete = true;

    private enum LuciferS1
    {
        Idle,miniIdle, shoot, shootLaser, spawn, gas, explode, pillar,
        windLeft, windRight
    }

    #endregion

    public virtual event Action Stage1Death;

    private void Awake()
    {
        animator = GetComponent<Animator>();


        luciHealth = gameObject.GetComponent<EnemyHealth>();
        luciHealth.OnZeroHealth += EndBossFight;

        luciAttacksScript = gameObject.GetComponent<LuciAttacks>();


    }

    void Start()
    {
        StartCoroutine(StateMachine());

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
                case LuciferS1.Idle:
                    yield return StartCoroutine(IdleState());
                    break;
                case LuciferS1.miniIdle:
                    yield return StartCoroutine(miniIdleState());
                    break;
                case LuciferS1.shoot:
                    yield return StartCoroutine(ShootState());
                    break;
                case LuciferS1.shootLaser:
                    yield return StartCoroutine(ShootLaserState());
                    break;
                case LuciferS1.spawn:
                    yield return StartCoroutine(SpawnState());
                    break;
                case LuciferS1.gas:
                    yield return StartCoroutine(GasState());
                    break;
                case LuciferS1.explode:
                    yield return StartCoroutine(ExplodeState());
                    break;
                case LuciferS1.pillar:
                    yield return StartCoroutine(PillarState());
                    break;
                case LuciferS1.windLeft:
                    yield return StartCoroutine(WindLeftState());
                    break;
                case LuciferS1.windRight:
                    yield return StartCoroutine(WindRightState());
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


    private IEnumerator IdleState()
    {
        animator.Play(IDLE_NAME);
        yield return new WaitForSeconds(luciData.idleWaitTime);
    }

    private IEnumerator miniIdleState()
    {
        animator.Play(IDLE_NAME);
        yield return new WaitForSeconds(luciData.miniIdleWaitTime);
    }

    private IEnumerator ShootState()
    {
        castAnimationComplete = false;

        animator.Play(SHOOT_NAME);

        luciAttacksScript.followBulletAttack.currentAttacks = Random.Range(0, luciAttacksScript.followBulletAttack.MaxAttacks);

        while (luciAttacksScript.followBulletAttack.currentAttacks < luciAttacksScript.followBulletAttack.MaxAttacks)
        {
            yield return null;
        }

    }

    private IEnumerator ShootLaserState()
    {
        castAnimationComplete = false;

        animator.Play(SHOOT_LASER_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    private IEnumerator SpawnState()
    {
        castAnimationComplete = false;

        animator.Play(SPAWN_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    private IEnumerator GasState()
    {
        castAnimationComplete = false;

        animator.Play(GAS_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    private IEnumerator ExplodeState()
    {
        castAnimationComplete = false;

        animator.Play(GAS_EXPLODE_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    private IEnumerator PillarState()
    {
        castAnimationComplete = false;

        animator.Play(HEAL_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    private IEnumerator WindLeftState()
    {
        castAnimationComplete = false;

        animator.Play(WINDL_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    private IEnumerator WindRightState()
    {
        castAnimationComplete = false;

        animator.Play(WINDR_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }



    /// <summary>
    /// Keep this transition hook for future boss-switch effects.
    /// </summary>
    protected void EndBossFight() 
    {
        luciAttacksScript.EndBossFight();
        Stage1Death?.Invoke(); //Raise the death event to notify LuciferFightManager.

    }







}
