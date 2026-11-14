using UnityEngine;
using System;
using System.Linq;
using static UnityEngine.EventSystems.EventTrigger;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controls the game's waves.
/// Spawns enemies according to the current wave.
/// </summary>
public class EnemyWaveManager : MonoBehaviour
{
    [SerializeField] private int maxEnemiesAlive = 50;
    [Header("       ENEMY WAVES")]
    [Space(5)]
    [SerializeField] private EnemyWave[] waves;

    [Space(10)]
    [Header("       PROCEDURAL WAVES")]
    [Space(5)]
    ///Procedural waves grow harder over time, including a higher elite spawn probability.

    [SerializeField] private ProceduralEnemyWave proceduralWaveTemplate; //Base configuration for procedural waves.
    [SerializeField] private ProceduralEnemyWave proceduralEliteWaveTemplate; //Base configuration for procedural elite waves.
    [SerializeField] private int additionalEnemiesPerWave = 2;
    //[SerializeField] private int waveCounter = 1;
    [SerializeField] private bool usesFormula = true;
    [SerializeField] private int additionalPointsPerWave = 5;
    [SerializeField] private int maxNumberOfEnemiesPerWave = 100;
    [SerializeField] private float healthIncreaseCoef = 0.2f; // Usage: currentHealth += startHealth * HealthIncreaseCoef.

    [Space(10)]
    [Header("       SPAWN TIME")]
    [Space(5)]
    [SerializeField] private float spawnRate = 0.5f;
    [SerializeField] private float delayToStartSpawningEnemies = 10f, spawnRate5Wave = 0.7f;
    [SerializeField] private float playStartSpawningFeedbackNsecondsBefore = 3f;
    [SerializeField] private float spawnRateDecreaser = 0.01f, spawnRateDecreaser5Wave = 0.01f;
    [SerializeField] private float minSpawnRate = 0.1f, minSpawnRate5Wave;
    [SerializeField] private float delayToStartFirstWave = 5f;

    [field: SerializeField] public int startRainingWave { get; private set; }
    [field: SerializeField] public int maxRainWave {  get; private set; }


    [Space(10)]
    [SerializeField] private PlayerHolder playerHolder;

    [Space(20)]
    [SerializeField] public bool canEnemySpawn = true;
    [SerializeField] public bool autoStartWave = true;
    [SerializeField] private bool showDebug = true;


    private float timer = 0;
    private int currentPlayerRegion;
    private float defaultSpawnRate, defaultSpawnRate5Wave;


    /// <summary>
    /// OnStartNewWave fires when the previous wave's last enemy dies.
    /// OnEnemyCanStartSpawning fires when spawning actually begins.
    /// </summary>
    public event Action OnStartNewWave, OnEnemyCanStartSpawning;
    public event Action OnEnemyDied;

    private SpawnManager spawnManager;
    private RunData runData;

    private bool isPlayerDead = false;
    private EnemyWave currentWave;

    void Awake()
    {
        spawnManager = GetComponent<SpawnManager>();

        if (runData == null)
        {
            runData = playerHolder.GetRunDataManager().GetRunData();
        }

        timer = delayToStartFirstWave +1;
        if(autoStartWave)
        {
            Invoke(nameof(StartNewWave), delayToStartFirstWave);
        }

        defaultSpawnRate = spawnRate;
        defaultSpawnRate5Wave= spawnRate5Wave;
    }

    private void OnEnable()
    {
        isPlayerDead = false;
        playerHolder.GetPlayerHealth().OnZeroHealth += () => isPlayerDead = true;
        spawnManager.OnChangePlayerRegion += UpdatePlayerRegion;
        runData.enemiesAlive = 0;
        UpdatePlayerRegion();
    }

    private void OnDisable()
    {
        playerHolder.GetPlayerHealth().OnZeroHealth -= () => isPlayerDead = true;
    }

    void Update()
    {
        if (canEnemySpawn == false || isPlayerDead  || runData.currentWaveNumber == 0) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
             timer = spawnRate;
             SpawnAnEnemie();
        }
        
    }


    // Wave setup and delayed spawning are separate phases. Consumers of the wave-start event must
    // not assume enemies are already active; the spawn-ready event represents that later point.
    public void StartNewWave()
    {
        if (runData.currentWaveNumber == 0) runData.SetCurrentWave(1); //Initialize at wave 1.

        timer = delayToStartSpawningEnemies;
        //Play the feedback shortly before enemies spawn.
        Invoke(nameof(StartSpawningEnemiesFeedback), delayToStartSpawningEnemies - playStartSpawningFeedbackNsecondsBefore);

        OnStartNewWave?.Invoke();

        if(runData.currentWaveNumber <=  waves.Count()) 
        {
            currentWave = waves[runData.currentWaveNumber - 1];
        }
        else //Procedurally generated endless waves.
        {
            if (usesFormula) //When using formula-based spawning, read the ScriptableObject directly.
            {
                if (runData.currentWaveNumber % 5 == 0) // Elite wave.
                {
                    //Create an EnemyWave using the ScriptableObject's formula.
                    currentWave = new EnemyWave(proceduralEliteWaveTemplate.enemies,
                        proceduralEliteWaveTemplate.getMinEnemies(runData.currentWaveNumber), proceduralEliteWaveTemplate.getMaxEnemies(runData.currentWaveNumber),
                        proceduralEliteWaveTemplate.getWavePoints(runData.currentWaveNumber));

                    currentWave.elitesToSpawn = proceduralEliteWaveTemplate.elitesToSpawn; //Also include the mini-boss spawns.
                }
                else // Non-elite wave.
                {
                    //Create an EnemyWave using the ScriptableObject's formula.
                    currentWave = new EnemyWave(proceduralWaveTemplate.enemies,
                        proceduralWaveTemplate.getMinEnemies(runData.currentWaveNumber), proceduralWaveTemplate.getMaxEnemies(runData.currentWaveNumber),
                        proceduralWaveTemplate.getWavePoints(runData.currentWaveNumber));
                    currentWave.elitesToSpawn = new List<EnemyData>(); //Initialize the list even when empty.
                }
            }
            else //Otherwise, use the legacy parameters on this component.
            {
                int maxEnemies = proceduralWaveTemplate.maxEnemies + runData.currentWaveNumber * additionalEnemiesPerWave;
                if (maxEnemies > maxNumberOfEnemiesPerWave) maxEnemies = maxNumberOfEnemiesPerWave;

                int spawnPoints = proceduralWaveTemplate.spawnPoints + additionalPointsPerWave;

                //Create an EnemyWave with progressively scaled parameters.
                currentWave = new EnemyWave(proceduralWaveTemplate.enemies,
                    proceduralWaveTemplate.minEnemies, maxEnemies, spawnPoints);

            }


        }
        runData.currentEnemyWave = currentWave;
        currentWave.StartWave();

        ChangeSpawnRate();

        //Spawn elite enemies.
        int eliteCountInThisWave = currentWave.elitesToSpawn.Count();
        if(eliteCountInThisWave > 0)
        {
            for (int i = 0; i < eliteCountInThisWave; i++)
            {
                spawnManager.SpawnEnemy(currentWave.GetEliteEnemy(i).enemyType, false);
            }
        }

    }

    /// <summary>
    /// Actually called slightly before spawning starts.
    /// </summary>
    private void StartSpawningEnemiesFeedback()
    {
        OnEnemyCanStartSpawning?.Invoke();
    }


    private void ChangeSpawnRate()
    {
        spawnRate = defaultSpawnRate - (spawnRateDecreaser) * runData.currentWaveNumber;
        spawnRate5Wave = defaultSpawnRate5Wave - (spawnRateDecreaser5Wave)*runData.currentWaveNumber;
        if (spawnRate < minSpawnRate) spawnRate = minSpawnRate;
        if (spawnRate5Wave < minSpawnRate5Wave) spawnRate5Wave = minSpawnRate5Wave;
    }

    private void SpawnAnEnemie()
    {
        //Limit simultaneous living enemies to 50.
        if (runData.enemiesAlive > maxEnemiesAlive) return;

        if (currentWave.HasPointsToSpawn())
        {
            var enemy = currentWave.GetEnemy();

            //Flying enemies spawn outside the map.
            if (enemy.isFlier)
            {
                spawnManager.SpawnEnemy(enemy.enemyType,false);
            }
            else
            {
                spawnManager.SpawnEnemy(enemy.enemyType, true);
            }
        }
        else
        {
            CheckIfTheEnemiesAreDead();
        }
        
        PrintEnemiesAlive();
    }

    private void CheckIfTheEnemiesAreDead()
    {
        if (runData.enemiesAlive <= 0)
        {
            runData.SetCurrentWave(runData.currentWaveNumber + 1);
            StartNewWave();
            Aux.Print("wave " + runData.currentWaveNumber + " started", showDebug);

            if (runData.enemiesAlive < 0)
            {
                runData.enemiesAlive = 0;
                Debug.Log("Error! ENEMY COUNT SHOULD NOT BE NEGATIVE");
            }
        }



    }

    public void ForceNextWave()
    {
        runData.SetCurrentWave(runData.currentWaveNumber + 1);
        StartNewWave();
    }

    public int GetCurrentWave()
    {
        if(runData == null)
        {
            runData = playerHolder.GetRunDataManager().GetRunData();
        }

        return runData.currentWaveNumber;
    }

    public void EnemySpawned()
    {
        runData.MarkGameplayEnemySpawned();
        runData.enemiesAlive++;
        PrintEnemiesAlive();
    }

    public void EnemyDied()
    {
        runData.enemiesAlive--;
        OnEnemyDied?.Invoke();
        PrintEnemiesAlive();
    }

    private void UpdatePlayerRegion()
    {
        currentPlayerRegion = spawnManager.GetCurrentPlayerRegion();
    }

    private void PrintEnemiesAlive()
    {
        Aux.Print("Number of enemies Alive: " + runData.enemiesAlive, showDebug);
    }

    public float GetHealthIncreaseCoef()
    {
        return healthIncreaseCoef;
    }


    public void EnableEnemySpawn(bool enable)
    {
        canEnemySpawn = enable;
    }

    public void ToggleEnemySpawn()
    {
        canEnemySpawn = !canEnemySpawn;
    }

    #region CHEATS

    public void CHEAT_NextWave()
    {
        runData.SetCurrentWave(runData.currentWaveNumber + 1);
        StartNewWave();
    }

    public void CHEAT_PreviousWave()
    {
        runData.SetCurrentWave(runData.currentWaveNumber - 1);
        StartNewWave();
    }

    public void CHEAT_SetWave(int wave)
    {
        runData.SetCurrentWave(wave);
        StartNewWave();
    }



    #endregion
}

