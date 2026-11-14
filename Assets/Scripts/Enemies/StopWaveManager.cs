using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopWaveManager : MonoBehaviour
{
    [SerializeField] EnemyWaveManager enemyWaveManager;
    public RunData runData;
    private CountEnemies_Helper[] enemiesDisabled;

    public IEnumerator StopWave(float delay)
    {
        enemyWaveManager.canEnemySpawn = false;

        yield return new WaitForSeconds(delay);

        enemiesDisabled = FindObjectsByType<CountEnemies_Helper>(FindObjectsSortMode.None);
        foreach (var enemy in enemiesDisabled)
        {
            enemy.gameObject.SetActive(false);
        }
    }

    public IEnumerator ResumeWave_Routine(float delay)
    {
        yield return new WaitForSeconds(delay);
        enemyWaveManager.canEnemySpawn = true;

        

        runData.enemiesAlive = 0;
        //Reactivate previously disabled enemies.
        foreach (var enemy in enemiesDisabled)
        {
            enemy.gameObject.SetActive(true);
        }

        //var enemiesAlive = FindObjectsOfType<CountEnemies_Helper>().Length;

    }

}
