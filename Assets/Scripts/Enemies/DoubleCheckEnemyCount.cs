using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Verify enemy deaths so a wave cannot remain stuck after all enemies are gone.
public class DoubleCheckEnemyCount : MonoBehaviour
{
    [SerializeField] private RunData runData;
    [SerializeField] private EnemyWaveManager hordeManager;

    private float  timeElapsedWithoutEnemyDeaths=0;
    private float timeElapsedWithFewEnemies = 0;
    private float timeSinceLastCheck = 0;


    private void Update()
    {
        //Before the first wave begins.
        if (runData.currentEnemyWave == null) return;
        
        timeElapsedWithoutEnemyDeaths += Time.deltaTime;
        
        timeSinceLastCheck += Time.deltaTime;


        if (runData.enemiesAlive < 5 && runData.currentEnemyWave.HasPointsToSpawn() == false)
        {
            if (timeElapsedWithoutEnemyDeaths > 120 && timeSinceLastCheck > 5)
            {
                if (IsAnyBossAlive() == false)
                {
                    ForceNewWave(false);

                }
            }
        }

        if (runData.enemiesAlive < 10 && runData.currentEnemyWave.HasPointsToSpawn() == false)
        {
            timeElapsedWithFewEnemies += Time.deltaTime;

            if (timeElapsedWithFewEnemies > 20 && timeSinceLastCheck > 5)
            {
                CheckEnemyNumber();
            }
        }




        if(timeElapsedWithoutEnemyDeaths > 400)
        {
            ForceNewWave(false);
        }
    }

    private void OnEnable()
    {
        hordeManager.OnStartNewWave += NewWave;
        hordeManager.OnEnemyDied += UpdateEnemyDiedTimer;
    }

    private void OnDisable()
    {
        hordeManager.OnStartNewWave -= NewWave;
        hordeManager.OnEnemyDied -= UpdateEnemyDiedTimer;
    }

    private void NewWave()
    {
        timeElapsedWithFewEnemies = 0;
        timeElapsedWithoutEnemyDeaths = 0;
    }

    private void UpdateEnemyDiedTimer()
    {
        timeElapsedWithoutEnemyDeaths = 0;
    }


    private bool IsAnyBossAlive()
    {
        timeSinceLastCheck = 0;
        var enemies = FindObjectsByType<CountEnemies_Helper>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy.isBoss)
            {
                return true;

            }
        }

        return false;
    }

    private void CheckEnemyNumber()
    {
        // Debug.Log("Counting enemies");
        timeSinceLastCheck = 0;
        var enemies = FindObjectsByType<CountEnemies_Helper>(FindObjectsSortMode.None);
 
        // Debug.Log("Living enemies: " + enemies.Length);

        if(enemies.Length < 1)
        {
            //Intentional second check for consistency.
            enemies = FindObjectsByType<CountEnemies_Helper>(FindObjectsSortMode.None);
            if(enemies.Length < 1)
            {
                ForceNewWave();
            }
            //hordeManager.StartNewWave();
        }

    }

    private void ForceNewWave(bool killEnemies = true)
    {
        if (hordeManager.canEnemySpawn == false) return;

        Debug.LogError("ERROR! Forcing new wave. This should not be happening!");

        if(killEnemies)
        {
            var enemies = FindObjectsByType<CountEnemies_Helper>(FindObjectsSortMode.None);

            Debug.Log("killing enemies = " + enemies.Length);
            foreach (var enemy in enemies)
            {
                enemy.gameObject.GetComponent<EnemyHealth>().InstaKill();
            }

        }

        //runData.timeElapsed_currentWave = 0;
        //runData.currentWave++;
        runData.SetCurrentWave(runData.currentWaveNumber + 1);
        hordeManager.StartNewWave();
    }

}
