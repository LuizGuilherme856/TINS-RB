using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LuciferStage2Boss;
using Random = UnityEngine.Random;

public class LuciferStage2Boss : MonoBehaviour
{
    [SerializeField] protected LuciferData luciData; //Assigned in the Editor.

    protected EnemyHealth luciHealth;



    /// <summary>
    /// stateTree contains branches, each holding a list of nodes.
    /// Each node represents one state in the sequence.
    /// </summary>
    [SerializeField] protected List<StateBranch> statesTree;

    /// <summary>
    /// A branch is a list of states.
    /// </summary>
    [Serializable]
    protected class StateBranch
    {
        [HideInInspector]
        public string name = "State Branch";
        public List<LuciferS2> stateNodes;

    }


    [SerializeField]  protected LuciferS2 currentState;
    protected LuciferS2? previousState;
    protected int branchIndex; //Current branch, containing a list of states.
    protected int nodeIndex; //Current node/state.

    #region AttackStuff

    [Header("Attack Stuff")]
    [SerializeField] protected LuciStage2Attacks luciAttacksScript;

    [SerializeField] protected LuciBossAttack followBulletAttack; // Assigned in the Editor.


    [System.Serializable]
    public class LuciBossAttack
    {
        [SerializeField] protected GameObject attackPrefab;
        [SerializeField] protected Transform attackSpawnPoint;
        [SerializeField] protected int maxAttacks = 3;
        [HideInInspector] public int currentAttacks = 0;

        public GameObject AttackPrefab => attackPrefab;
        public Transform AttackSpawnPoint => attackSpawnPoint;
        public int MaxAttacks => maxAttacks;
    }




    public virtual event Action CloneCopyState; //Action invoked when requested by the encounter.



    #endregion

    #region AnimatorStuff

    protected Animator animator;

    //Animation names in the Animator.
    protected readonly string IDLE_NAME = "Idle";
    protected readonly string SHOOT_NAME = "Shoot";
    private readonly string SHOOT_LASER_NAME = "ShootLaser";
    protected readonly string GAS_NAME = "Gas";
    protected readonly string GAS_EXPLODE_NAME = "GasExplode";
    protected readonly string HEAL_NAME = "HealPillar";
    protected readonly string WINDL_NAME = "WindLeft";
    protected readonly string WINDR_NAME = "WindRight";
    protected readonly string SPAWN_NAME = "Spawn";
    protected readonly string LASER_NAME = "Laser";
    protected readonly string VORDEM_NAME = "VOrdem";
    protected readonly string VCAOS_NAME = "VCaos";
    protected readonly string VTENTACLES_NAME = "VTentacles";
    protected readonly string THUNDER_NAME = "Thunder";
    protected readonly string CLONE_NAME = "Clone";

    #endregion

    #region AuxStateVariables


    protected bool castAnimationComplete = false;


    //Attack-related.
    protected bool isShootingLaser = false;


    public void SetCastComplete() => castAnimationComplete = true;

    public enum LuciferS2
    {
        Idle,miniIdle, shoot, shootLaser, spawn, gas, explode, pillar,
        windLeft, windRight,laser,vOrdem,vCaos,vTentacles,thunder,clone
    }

    #endregion



    protected virtual void Awake()
    {

        try
        {
            animator = GetComponent<Animator>();
        }
        catch (Exception)
        {

            Debug.LogError("Lucifer não possui um script Animator");
        }
        
        try
        {
            luciHealth = this.gameObject.GetComponent<EnemyHealth>();
            luciHealth.OnZeroHealth += EndBossFight;

        }
        catch (Exception)
        {
            Debug.LogError("Lucifer não possui um script EnemyHealth");
        }
        try
        {
            luciAttacksScript = this.gameObject.GetComponent<LuciStage2Attacks>();
        }
        catch (Exception)
        {
            Debug.LogError("Lucifer não possui um script luciAttack");
        }

    }

    protected virtual void Start()
    {
        StartCoroutine(StateMachine());

    }

    /// <summary>
    /// Boss state machine.
    /// </summary>
    protected virtual IEnumerator StateMachine()
    {
        yield return new WaitForSeconds(1f); // Wait one second before starting the state machine; no specific dependency is documented.

        nodeIndex = 0;

        branchIndex = Random.Range(0, statesTree.Count);
        previousState = null;
        currentState = statesTree[branchIndex].stateNodes[nodeIndex];

        while (true)
        {
            switch (currentState)
            {
                case LuciferS2.Idle:
                    CloneCopyState?.Invoke();
                    yield return StartCoroutine(IdleState());
                    break;
                case LuciferS2.miniIdle:
                    yield return StartCoroutine(miniIdleState());
                    break;
                case LuciferS2.shoot:
                    CloneCopyState?.Invoke();
                    yield return StartCoroutine(ShootState());
                    break;
                case LuciferS2.shootLaser:
                    CloneCopyState?.Invoke();
                    yield return StartCoroutine(ShootLaserState());
                    break;
                case LuciferS2.spawn:
                    yield return StartCoroutine(SpawnState());
                    break;
                case LuciferS2.gas:
                    yield return StartCoroutine(GasState());
                    break;
                case LuciferS2.explode:
                    yield return StartCoroutine(ExplodeState());
                    break;
                case LuciferS2.pillar:
                    yield return StartCoroutine(PillarState());
                    break;
                case LuciferS2.windLeft:
                    yield return StartCoroutine(WindLeftState());
                    break;
                case LuciferS2.windRight:
                    yield return StartCoroutine(WindRightState());
                    break;
                case LuciferS2.laser:
                    CloneCopyState?.Invoke();
                    yield return StartCoroutine(LaserState());
                    break;
                case LuciferS2.vOrdem:
                    yield return StartCoroutine(VOrdemState());
                    break;
                case LuciferS2.vCaos:
                    yield return StartCoroutine(VCaosState());
                    break;
                case LuciferS2.vTentacles:
                    yield return StartCoroutine(VTentaclesState());
                    break;
                case LuciferS2.thunder:
                    yield return StartCoroutine(ThunderState());
                    break;
                case LuciferS2.clone:
                    yield return StartCoroutine(CloneState());
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


    protected virtual IEnumerator IdleState()
    {
        animator.Play(IDLE_NAME);
        yield return new WaitForSeconds(luciData.idleWaitTimeS2);
    }

    private IEnumerator miniIdleState()
    {
        animator.Play(IDLE_NAME);
        yield return new WaitForSeconds(luciData.miniIdleWaitTimeS2);
    }

    protected virtual IEnumerator ShootState()
    {
        castAnimationComplete = false;

        animator.Play(SHOOT_NAME);

        luciAttacksScript.followBulletAttack.currentAttacks = Random.Range(0, luciAttacksScript.followBulletAttack.MaxAttacks);

        while (luciAttacksScript.followBulletAttack.currentAttacks < luciAttacksScript.followBulletAttack.MaxAttacks)
        {
            yield return null;
        }

    }


    protected IEnumerator ShootLaserState()
    {
        castAnimationComplete = false;

        animator.Play(SHOOT_LASER_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator SpawnState()
    {
        castAnimationComplete = false;

        animator.Play(SPAWN_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator GasState()
    {
        castAnimationComplete = false;

        animator.Play(GAS_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator ExplodeState()
    {
        castAnimationComplete = false;

        animator.Play(GAS_EXPLODE_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator PillarState()
    {
        castAnimationComplete = false;

        animator.Play(HEAL_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator WindLeftState()
    {
        castAnimationComplete = false;

        animator.Play(WINDL_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator WindRightState()
    {
        castAnimationComplete = false;

        animator.Play(WINDR_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator LaserState()
    {
        castAnimationComplete = false;

        animator.Play(LASER_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator VOrdemState()
    {
        castAnimationComplete = false;

        animator.Play(VORDEM_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator VCaosState()
    {
        castAnimationComplete = false;

        animator.Play(VCAOS_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator VTentaclesState()
    {
        castAnimationComplete = false;

        animator.Play(VTENTACLES_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator ThunderState()
    {
        castAnimationComplete = false;

        animator.Play(THUNDER_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }

    protected virtual IEnumerator CloneState()
    {
        castAnimationComplete = false;

        animator.Play(CLONE_NAME);

        while (!castAnimationComplete)
        {
            yield return null;
        }
    }



    /// <summary>
    /// Keep this transition hook for future boss-switch effects.
    /// </summary>
    protected virtual void EndBossFight()
    {
        luciAttacksScript.EndBossFight();
    }

    public LuciferS2 GetCurrentState() 
    {
        return currentState;
    }
}
