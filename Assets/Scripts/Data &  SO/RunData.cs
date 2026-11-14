using System.Collections.Generic;
using UnityEngine;
using System;

//[CreateAssetMenu(fileName = "runData", menuName = "runData")]
/// <summary>
/// Stores the current run state so player equipment and progress survive scene transitions.
/// </summary>
// In-memory attempt state shared between scenes. Scene transitions preserve it; starting a new run
// resets it through RunDataManager. It does not provide resume-after-relaunch persistence.
public class RunData : ScriptableObject
{
    public CharacterData currentCharacter;// = Character.azaus;
    public float currentHp;
    public int currentpoints;
    public int pointsInThisRun { get; private set; }
    public List<GunData> guns;
    public MeleeData meleeWeapon, specialAbility, dashData;
    public int enemiesAlive;
    public EnemyWave currentEnemyWave;

     [HideInInspector] public bool survivalMode = false;

    [field: SerializeField] public int currentWaveNumber { get; private set; }

    public int xp { get; private set; }
    public int enemiesKilled { get; private set; }
    public float timeElapsed, timeElapsed_currentWave;
    public int numberOfPowerUpsCollected;
    public bool hasFinalizedCurrentRunScore { get; private set; }
    public bool hasSpawnedGameplayEnemy { get; private set; }

    public int numberOfGromschsAlive = 0;
    public int numberOfCthulhusAlive = 0;

    public List<WaveStatistics> waveStatistics = new();

    public event Action OnChangeWave;

    /// <summary>
    /// Change the wave and reset its elapsed time.
    /// </summary>
    /// <param name="newWave"></param>
    public void SetCurrentWave(int newWave)
    {
        currentWaveNumber = newWave;
        OnChangeWave?.Invoke();
        timeElapsed_currentWave = 0;

        if(newWave <=2) //Reset these values so they do not carry into the next run.
        {
            numberOfCthulhusAlive = 0;
            numberOfGromschsAlive = 0;
        }
    }


    public void AddXp(int xp)
    {
        this.xp += xp;
        EnsureWaveStatisticsIndex(currentWaveNumber);
        waveStatistics[currentWaveNumber].xpCollected += xp ; 
    }

    public void SetXp(int newXp)
    {
        xp = newXp;
    }

    public void AddEnemiesKilled(int enemiesKilled)
    {
        this.enemiesKilled += enemiesKilled;
        EnsureWaveStatisticsIndex(currentWaveNumber);
        waveStatistics[currentWaveNumber].enemiesKilled += enemiesKilled;
    }

    public void SetEnemiesKilled(int newEnemiesKilled)
    {
        enemiesKilled = newEnemiesKilled;
    }

    public void AddPointsInThisRun(int points)
    {
        currentpoints += points;
        pointsInThisRun += points;
        EnsureWaveStatisticsIndex(currentWaveNumber);
        waveStatistics[currentWaveNumber].bloodPointsCollected += points;
    }

    public void SetPointsInThisRun(int points)
    {
        pointsInThisRun = points;
        EnsureWaveStatisticsIndex(currentWaveNumber);
        waveStatistics[currentWaveNumber].bloodPointsCollected += points;
    }

    public void EnsureWaveStatisticsIndex(int index)
    {
        while (waveStatistics.Count <= index)
        {
            // Create WaveStatistics with default values; adjust as needed.
            waveStatistics.Add(new WaveStatistics(index, 0f, 0, 0, 0));
        }
    }

    public void ResetAchivementsEvents()
    {
        OnChangeWave = null;
    }

    public void MarkCurrentRunScoreAsFinalized()
    {
        hasFinalizedCurrentRunScore = true;
    }

    public void ResetCurrentRunScoreFinalization()
    {
        hasFinalizedCurrentRunScore = false;
    }

    public void MarkGameplayEnemySpawned()
    {
        hasSpawnedGameplayEnemy = true;
    }

    public void ResetGameplayEnemySpawned()
    {
        hasSpawnedGameplayEnemy = false;
    }
}

public enum Character { azaus, nina, hanba,deNiro, death, dog , yodista}

