using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Notify PowerUpDropManager when this enemy should drop a power-up.
public class DropPowerUpComunicator : MonoBehaviour
{
    
    [SerializeField] [Range(0,100)] private float spawnChance = 0.5f;
    
    
    [SerializeField] private bool bossPowerup = false;
    [SerializeField] private int powerUpIndex = 0;
    [SerializeField] private GlobalStats globalStats;
    private RunData runData;
    
    private EnemyHealth enemyHealth;

    private PowerUpDropManager powerUpDropManager;

    private void Awake()
    {
        enemyHealth= GetComponent<EnemyHealth>();
        runData = GetComponent<EnemyHealth>().GetRunData();
        powerUpDropManager = FindFirstObjectByType<PowerUpDropManager>();

        if(globalStats == null)
        {
            globalStats = AssetFinder.GetGlobalStats();
            Debug.LogWarning($"O inimigo {gameObject.name} nao tem global stats. Procurando manualmente. Por favor" +
                $"coloque pelo inspector pra poupar performance");
        }
    }

    private void OnEnable()
    {
        enemyHealth.OnZeroHealth += SpawnPowerUp;
    }

    private void SpawnPowerUp()
    {
        if (bossPowerup)
        {
            powerUpDropManager.SpawnBossPowerUp(transform.position, powerUpIndex);
            return;
        }
        var randomNumber = Random.Range(0, 100);

        if(randomNumber < spawnChance + globalStats.AddTodropChance) 
        {
            powerUpDropManager.SpawnPowerUp(transform.position);
            return;
         
        }

        if(runData.survivalMode)
        {
            powerUpDropManager.SpawnSurvivalModeDrop(transform.position);
            return;
        }


    }

}
