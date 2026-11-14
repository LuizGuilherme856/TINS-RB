using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// Replaces HordeManager in Survival mode
/// and coordinates enemy spawning.
/// responsible for initiating new waves of enemies
/// </summary>
public class SurvivalModeManager : MonoBehaviour
{
    [Header("       NUMBER OF ENEMIES")]
    [Space(5)]
    [SerializeField] private float healthIncreaseCoef = 0.2f; // Usage: currentHealth += startHealth * HealthIncreaseCoef.

    [Header("       SPAWN TIME")]
    [Space(5)]
    //[SerializeField] private float spawnRate = 0.5f;
    //[SerializeField] private float delayToStartSpawningEnemies = 10f;
    //[SerializeField] private float spawnRateDecreaser = 0.01f;
    [SerializeField] private float minSpawnRate = 0.1f;
    //[SerializeField] private float delayToStartFirstWave = 5f;

    [field: SerializeField] public int startRainingWave { get; private set; }
    [field: SerializeField] public int maxRainWave { get; private set; }

    [Header("       ENEMY TYPES")]
    [Space(5)]

    [SerializeField] private EnemyType[] enemyTypes;
    [Tooltip("Tem que ta em ordem crescente e o ultimo tem que ser 100")]
    [SerializeField][Range(0, 100)] private float[] enemyTypeSpawnChance;
    [SerializeField] private EnemyType wave5EnemyType, wave5EnemyTypeRain;



    [Space(20)]
    [Header("       OUTROS  ")]
    //[SerializeField] private float level1Xp = 100f;
    //[SerializeField][Range(0,2)] private float  xpIncreaser = 1.2f;

    [Space(10)]
    [SerializeField] private PlayerHolder playerHolder;

    [Space(20)]
    [SerializeField] private bool spawnManually = false;
    [SerializeField] private bool showDebug = true;



    //[field: SerializeField] public int currentWave { get; private set; }
    //private int enemiesAlive;
    private float timer = 0;


    //private float timeSinceBossSpawn = 0; //Prevent Gromsch from spawning more often than intended.

    public event Action OnEnemyDied;//, OnPickUpPoint;

    private SpawnManager spawnManager;
    private RunData runData;
    //private int xpPoints = 0;

    private bool isPlayerDead = false;

    void Awake()
    {
        spawnManager = GetComponent<SpawnManager>();

        runData = playerHolder.GetRunDataManager().GetRunData();
        //runData = FindObjectOfType<RunDataManager>().GetRunData();

       // defaultSpawnRate = spawnRate;
    }

    private void OnEnable()
    {
        isPlayerDead = false;
        playerHolder.GetPlayerHealth().OnZeroHealth += () => isPlayerDead = true;
    }

    private void OnDisable()
    {
        playerHolder.GetPlayerHealth().OnZeroHealth -= () => isPlayerDead = true;
    }

    void Update()
    {

        if (spawnManually || isPlayerDead) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            timer = CalculateSpawnRate();

            SpawnAnEnemie();
        }

    }

    private void TryToSpawnBosses()
    {
        //Allow five seconds for bosses to spawn; on scene changes, respawn only surviving bosses.
        //After five seconds, treat this as a map transition.
        bool timeToSpawnBossHasPassed = runData.timeElapsed_currentWave > 5f ? true : false;

        if (runData.currentWaveNumber % 11 == 0) //Gromsch spawn wave.
        {
            if (timeToSpawnBossHasPassed == false) //Spawn the required count.
            {
                for (int i = 0; i < runData.currentWaveNumber / 11; i++) //Add another Gromsch every eleven waves.
                {
                    Debug.Log("spawn gromsch");
                    SpawnBossGromsch();
                }
            }
            else if (timeToSpawnBossHasPassed && runData.numberOfGromschsAlive <= 0)
            {
                Debug.Log("Gromsch ja foi spawnado e ja foi morto.nao é mais necessario nessa onda");
            }
            else
            {
                for (int i = 0; i < runData.numberOfGromschsAlive; i++) //Add another Gromsch every eleven waves.
                {
                    SpawnBossGromsch(false); //Do not increment the Gromsch count.
                }

            }
        }


        //Same handling as Gromsch above.
        if (runData.currentWaveNumber >= 20 && runData.currentWaveNumber % 5 == 0) //Rain waves are multiples of five and trigger Cthulhu.
        {
            if (runData.currentWaveNumber == 55) return; //Skip Cthulhu on wave 55 to avoid too many simultaneous bosses.

            if (timeToSpawnBossHasPassed == false)
            {
                int numberOfCthulhus = runData.currentWaveNumber / 5 - 3;
                SpawnBossCthulhu(numberOfCthulhus);
            }
            else if (timeToSpawnBossHasPassed && runData.numberOfCthulhusAlive <= 0)
            {
                Debug.Log("Cthulhu ja foi spawnado e ja foi morto.nao é mais necessario nessa onda");
            }
            else
            {
                int numberOfCthulhus = runData.numberOfCthulhusAlive;
                SpawnBossCthulhu(numberOfCthulhus);
            }
        }
    }

    /// <summary>
    /// Use linear regression to reduce the spawn interval over time.
    /// </summary>
    private float CalculateSpawnRate()
    {
        float t = runData.timeElapsed / 60; //Time in minutes.
        float newSpawnRate = 0.0004f * t * t - 0.02468f * t + 0.48385f;
        if (newSpawnRate < minSpawnRate)
        {
            return minSpawnRate;
        }

        return newSpawnRate;


    }


    private void SpawnBossGromsch(bool increaseNumberOfGromschs = true)
    {
        if (increaseNumberOfGromschs) runData.numberOfGromschsAlive++;
        spawnManager.SpawnEnemy(EnemyType.gromsch, false);
        //runData.numberOfEnemiesToSpawn--;
    }

    private void SpawnBossCthulhu(int number)
    {
        for (int i = 0; i < number; i++)
        {
            spawnManager.InstantiateEnemy(EnemyType.cthulhu);
        }
    }


    private void SpawnAnEnemie()
    {
        var enemy = RandomizeEnemyToSpawn();
        spawnManager.SpawnEnemy(enemy, false); //Pass false to spawn outside the map.
        PrintEnemiesAlive();
    }

    private EnemyType RandomizeEnemyToSpawn()
    {
        float randomNumber;
        randomNumber = Random.Range(enemyTypeSpawnChance[0], enemyTypeSpawnChance[5]);

        for (int i = 0; i < enemyTypes.Length; i++)
        {
            if (randomNumber <= enemyTypeSpawnChance[i])
            {
                return enemyTypes[i];
            }
        }

        Debug.LogError("Nao conseguiu sortear nenhum inimigo!!! Sorteando demoninho");
        return EnemyType.demoninho;
    }

    //public int GetCurrentWave()
    //{
    //    return runData.currentWave;
    //}

    public void EnemySpawned()
    {
        runData.enemiesAlive++;
        PrintEnemiesAlive();
    }

    public void EnemyDied()
    {
        runData.enemiesAlive--;
        OnEnemyDied?.Invoke();
        PrintEnemiesAlive();
    }

    private void PrintEnemiesAlive()
    {
        Aux.Print("Number of enemies Alive: " + runData.enemiesAlive, showDebug);
    }

    public float GetHealthIncreaseCoef()
    {
        return healthIncreaseCoef;
    }




}
