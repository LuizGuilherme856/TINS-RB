using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FloorExplosions;

public class AlkarionBoss : MonoBehaviour
{


    [SerializeField] private float laughCoolDownTime = 5f;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private FloorExplosions floorExplosions;
    [SerializeField] private MMF_Player laughFeedbacks, wakeUpFeedback, instantiateProjectileFeedback;


    [Header("Alkarion head stuff")]
    public EnemyHealth headEnemyPrefab;
    public Transform headSpawnPoint; // May reference the object containing the trigger itself.
    public List<EnemyHealth> activeHeads = new List<EnemyHealth>();

    private bool canAttack = true;

    private AttackPlayer attackPlayer;
    private MusicManager musicManager;
    private Animator animator;
    private float laughTimer = 0;
    public bool isSleeping = true;

    [Header("Minion  Spawner settings")]
    public MinionSpawner[] minionSpawners;
    public float minionSpawnRate = 15f;
    [Range(0,1)] public float spawnChance = 0.5f;



    private void Awake()
    {
        musicManager = FindFirstObjectByType<MusicManager>();
        attackPlayer = GetComponent<AttackPlayer>();
        animator = GetComponent<Animator>();
        laughTimer = laughCoolDownTime;
        //playerTransform = playerHolder.GetPlayerTransform();
    }
    private void OnEnable()
    {
        enemyHealth.OnTakeDamage += WakeUp;
    }

    private void Update()
    {
        if (isSleeping || enemyHealth.currentHealth <= 0) return;

        TriggerLaugh();
    }

    private void TriggerLaugh()
    {
        if(canAttack == false) return;

        if (laughTimer > 0)
        {
            laughTimer -= Time.deltaTime;
        }
        else
        {
            animator.SetTrigger("Laugh");
            laughTimer = laughCoolDownTime;
        }
    }
  
        
    private void WakeUp()
    {
        //if (isPlayerInsideArea.IsInside == false) return;

        enemyHealth.OnTakeDamage -= WakeUp;
        if (!isSleeping) return;
        InvokeRepeating(nameof(TryToSpawnMinion), 2, minionSpawnRate);
        animator.SetTrigger("Wake up");
        wakeUpFeedback.PlayFeedbacks();
        //enemyHealth.UpdateHealth();
        isSleeping = false;

    }

    private void TryToSpawnMinion()
    {
        if (minionSpawners == null) return;

        for (int i = 0; i < minionSpawners.Length; i++)
        {
            if (Random.Range(0f, 1f) < spawnChance)
            {
                StartCoroutine(minionSpawners[i].SpawnEnemies());
            }
        }


    }


    public void TriggerLaughExplosionsAnimator(floorExplosionMode mode) //Adapter needed because the Animator cannot call FloorExplosion directly.
    {
        if (canAttack == false) return;
        floorExplosions.TriggerExplosionMode(mode);
        laughFeedbacks.PlayFeedbacks();
    }

    public void InstantiateProjectileAnimator()
    {
        if(canAttack)
            instantiateProjectileFeedback.PlayFeedbacks();
    }

    public void SetCanAttack()
    {
        canAttack= true;

    }
    public void SetCannotAttack()
    {
        canAttack= false;
    }

    public void SpawnHead_Animation()
    {
        SpawnHead(headSpawnPoint.position);
    }

    //alkarion head 
    private void SpawnHead(Vector3 position)
    {
        EnemyHealth newEnemy = Instantiate(headEnemyPrefab, position, Quaternion.identity);
        //activeHeads.Add(newEnemy);
        //newEnemy.OnZeroHealth += () => OnEnemyDeath(newEnemy);
    }

    private void OnEnemyDeath(EnemyHealth deadEnemy)
    {
        if (activeHeads.Contains(deadEnemy))
            activeHeads.Remove(deadEnemy);

        // Spawn two enemies near the defeated enemy's position with a small offset.
        Vector3 basePos = deadEnemy.transform.position;
        SpawnHead(basePos + Vector3.left * 0.5f);
        SpawnHead(basePos + Vector3.right * 0.5f);

        // Remove the listener to prevent a memory leak.
        deadEnemy.OnZeroHealth -= () => OnEnemyDeath(deadEnemy);
    }


    
}
