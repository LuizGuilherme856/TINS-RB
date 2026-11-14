using System.Collections;
using UnityEngine;

public class EggSpawnerAI : MonoBehaviour
{
    protected MinionSpawner minionSpawner;

    [SerializeField] protected EggSpawnerData eggSpawnData; //Assigned in the Editor.

    protected bool eggGrowAnimationComplete = false;
    public void SetEggGrowComplete() => eggGrowAnimationComplete = true;

    protected bool spawnComplete = false;

    private readonly string GROW_ENEMY_LOOP_NAME = "GrowEnemyLoop";
    private readonly string FINISH_GROW_ENEMY_NAME = "FinishGrow";


    protected Animator animator;

    protected void Awake()
    {
        try
        {
            minionSpawner = GetComponentInChildren<MinionSpawner>();    
        }
        catch (System.Exception)
        {
            Debug.LogWarning("EggSpawner não tem eggSpawnerAI");
        }

        try
        {
            animator = GetComponent<Animator>();
        }
        catch (System.Exception)
        {
            Debug.LogWarning("EggSpawner não tem animatorController");
            throw;
        }
    }


    void Start()
    {
        StartCoroutine(StateMachine());
    }

    public void OnEnable()
    {
        minionSpawner.FirstSpawnComplete += SetSpawnComplete;
    }

    protected void SetSpawnComplete() 
    {
        spawnComplete = true;
    }

    /// <summary>
    /// Boss state machine.
    /// </summary>
    private IEnumerator StateMachine()
    {
        eggGrowAnimationComplete = false;

        while (!eggGrowAnimationComplete)
        {
            yield return null;
        }

        animator.Play(GROW_ENEMY_LOOP_NAME);
        yield return new WaitForSeconds(eggSpawnData.timeToSpawnEnemy);

        animator.Play(FINISH_GROW_ENEMY_NAME);
    }

    public void SpawnEnemyAndDeactivate_AnimationEvent()
    {
        StartCoroutine(minionSpawner.SpawnEnemies());
        
        StartCoroutine(WaitSpawn());
        
    }

    private IEnumerator WaitSpawn()
    {
        while (!spawnComplete)
        {
            yield return null;
        }
        //Reset();
        //yield return new WaitForSeconds(0.8f); //Delay disappearance until the enemy appears; timing may vary between machines and needs verification.
        this.gameObject.SetActive(false);
    }

    protected void Reset()
    {
        minionSpawner.FirstSpawnComplete -= SetSpawnComplete;
        spawnComplete = false;
    }

    public void OnDisable()
    {
        Reset();
    }
}
