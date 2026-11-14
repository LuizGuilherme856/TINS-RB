using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Track run statistics such as kills and elapsed time.
/// </summary>
public class StatisticsManager : MonoBehaviour
{
    private EnemyWaveManager enemyWaveManager;
    private SurvivalModeManager survivalModeEnemySpawner;
    [SerializeField] private RunData runData;
    public PlayerHolder playerHolder;
    private Health playerHealth;

    [Header(" Score calculation parameters")]
    [SerializeField] private float waveMultiplier = 100f; //Award more points for later waves.
    [SerializeField] private float exponentWave = 1.5f; //Larger values increase score growth in later waves.
    [SerializeField] private float timePenalty = 0.01f; //Reduce score as completion time increases.
    [SerializeField] private float enemyMultiplier = 10f; //Award more points for additional kills.
    [SerializeField] private float bossKilledMultiplier = 5000;

    [SerializeField] private bool isSurvivalMode = false;

    private float timeUpdateRate = 1f; //Update once per second rather than every frame.

    private void Awake()
    {
        if(isSurvivalMode)
        {
            survivalModeEnemySpawner = FindFirstObjectByType<SurvivalModeManager>();
        }
        else
        {
            enemyWaveManager = FindFirstObjectByType<EnemyWaveManager>();

        }

        if (playerHolder != null)
        {
            playerHealth = playerHolder.GetPlayerHealth();
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnZeroHealth += StopTrackingTimeOnPlayerDeath_TechBooh;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnZeroHealth -= StopTrackingTimeOnPlayerDeath_TechBooh;
        }

        CancelInvoke(nameof(UpdateTime));
        CancelInvoke(nameof(UpdateWaveTime));
    }

    private void Start()
    {

        if (isSurvivalMode)
        {
            survivalModeEnemySpawner.OnEnemyDied += () => runData.AddEnemiesKilled(1); 
        }
        else
        {
            enemyWaveManager.OnEnemyDied += () => runData.AddEnemiesKilled(1);
        }

        InvokeRepeating(nameof(UpdateTime), 0, timeUpdateRate);
        InvokeRepeating(nameof(UpdateWaveTime), 0, timeUpdateRate);
    }

    /// <summary>
    /// TechBooh:
    /// Freeze elapsed time at player death so the saved score
    /// matches the death-screen time displayed
    /// a few seconds later.
    /// </summary>
    private void StopTrackingTimeOnPlayerDeath_TechBooh()
    {
        CancelInvoke(nameof(UpdateTime));
        CancelInvoke(nameof(UpdateWaveTime));
    }

    private void UpdateTime()
    {
        runData.timeElapsed += timeUpdateRate;
    }

    private void UpdateWaveTime()
    {
        runData.timeElapsed_currentWave += timeUpdateRate;
        //Debug.Log("Wave time: " + runData.timeElapsed_currentWave);
    }

    public int GetEnemiesKilled()
    {
        return runData.enemiesKilled;
    }
    public string GetTimeElapsed()
    {
        return MMTime.FloatToTimeString(runData.timeElapsed);
    }
    public int GetTotalBloodPoints()
{
    if (runData == null || runData.waveStatistics == null)
        return 0;

    int totalBloodPoints = 0;
    foreach (var wave in runData.waveStatistics)
    {
        totalBloodPoints += wave.bloodPointsCollected;
    }
    return totalBloodPoints;
}

public int GetTotalXp()
{
    if (runData == null || runData.waveStatistics == null)
        return 0;

    int totalXp = 0;
    foreach (var wave in runData.waveStatistics)
    {
        totalXp += wave.xpCollected;
    }
    return totalXp;
}

    public int GetScore()
    {
        float score;
        score = (Mathf.Pow(runData.currentWaveNumber, exponentWave) * waveMultiplier) - (runData.timeElapsed * timePenalty) + (runData.enemiesKilled * enemyMultiplier);

        //TODO: Trigger this calculation from boss-death events instead.
        int numberOfBossWaves = (int)runData.currentWaveNumber / 11;
        score += numberOfBossWaves * bossKilledMultiplier;

        if (score < 0)
        {
            return UnityEngine.Random.Range(1, 11);
        }

        return (int)score;
    }

    //private void SaveStatistics()
    //{
    //    string dateTimeString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    //    SaveWaveStatisticsToFile(SaveSystem.GetDirectoryPath(isBeta) + "/wave_statistics_" + dateTimeString + ".csv");
    //}

    public void SaveWaveStatisticsToFile(string filePath)
    {
        if (runData == null || runData.waveStatistics == null || runData.waveStatistics.Count == 0)
            return;

        StringBuilder sb = new StringBuilder();
        int totalWaveNumber = 0, totalEnemiesKilled = 0, totalBloodPoints = 0, totalXp = 0;
        float totalTimeElapsed = 0f;

        sb.AppendLine("WaveNumber;TimeElapsed;EnemiesKilled;BloodPointsCollected;XpCollected");

        foreach (var wave in runData.waveStatistics)
        {
            sb.AppendLine($"{wave.waveNumber};{wave.timeElapsed};{wave.enemiesKilled};{wave.bloodPointsCollected};{wave.xpCollected}");
            totalWaveNumber += wave.waveNumber;
            totalTimeElapsed += wave.timeElapsed;
            totalEnemiesKilled += wave.enemiesKilled;
            totalBloodPoints += wave.bloodPointsCollected;
            totalXp += wave.xpCollected;
        }

        sb.AppendLine();
        sb.AppendLine($"TotalWaveNumber: {totalWaveNumber}");
        sb.AppendLine($"TotalTimeElapsed: {totalTimeElapsed}");
        sb.AppendLine($"TotalEnemiesKilled: {totalEnemiesKilled}");
        sb.AppendLine($"TotalBloodPointsCollected: {totalBloodPoints}");
        sb.AppendLine($"TotalXpCollected: {totalXp}");

        File.WriteAllText(filePath, sb.ToString());
    }


}

    public class WaveStatistics
    {
        public int waveNumber;
        public float timeElapsed;
        public int enemiesKilled;
        public int bloodPointsCollected;
        public int xpCollected;

        public WaveStatistics(int waveNumber, float timeElapsed, int enemiesKilled, int bloodPointsCollected, int xpCollected)
        {
            this.waveNumber = waveNumber;
            this.timeElapsed = timeElapsed;
            this.enemiesKilled = enemiesKilled;
            this.bloodPointsCollected = bloodPointsCollected;
            this.xpCollected = xpCollected;
        }
    }
