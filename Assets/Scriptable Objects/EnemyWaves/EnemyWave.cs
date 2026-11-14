using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Wave ", menuName = "Enemy Wave")]
public class EnemyWave : ScriptableObject
{
    [SerializeField] public List<EnemyToSpawn> enemies;
    /// <summary>
    /// Spawn priority: lowest-cost enemies first, highest-cost first, or random.
    /// </summary>
   // [Tooltip("NOT IMPLEMENTED YET")]
   // [SerializeField] public SpawnPriority spawnPriority;
    [SerializeField] public int minEnemies, maxEnemies;
    [SerializeField] public int spawnPoints;

    [Space(15)]
    [SerializeField] public List<EnemyData> elitesToSpawn;
    //protected bool spawnedElites = false;

    protected int pointsRemaining;
    protected int enemiesSpawned;


    /// <summary>
    /// Minimum cost of an enemy in this wave, usually 1 point.
    /// </summary>
    protected int minPointsToSpawn = 1;
    protected int maxPointsToSpawn = 1;

    public EnemyWave(List<EnemyToSpawn> enemies, int minEnemies, int maxEnemies, int spawnPoints)
    {
        this.enemies = enemies;
        this.minEnemies = minEnemies;
        this.maxEnemies = maxEnemies;
        this.spawnPoints = spawnPoints;
    }

    public void StartWave()
    {
        pointsRemaining = spawnPoints;
        enemiesSpawned = 0;
        minPointsToSpawn = int.MaxValue;
        //spawnedElites = false;

        //Cache the minimum and maximum enemy costs for this wave.
        foreach (var enemyItem in enemies)
        {
            if (enemyItem.enemyData.spawnPoints < minPointsToSpawn)
            {
                minPointsToSpawn = enemyItem.enemyData.spawnPoints;
            }
            if (enemyItem.enemyData.spawnPoints > maxPointsToSpawn)
            {
                maxPointsToSpawn = enemyItem.enemyData.spawnPoints;
            }
        }

    }

    public EnemyData GetEliteEnemy (int index)
    {
        pointsRemaining -= elitesToSpawn[index].spawnPoints;
        return elitesToSpawn[index];
    }

    public EnemyData GetEnemy()
    {
        if(HasPointsToSpawn() == false)
        {
            throw new System.Exception("Não tem pontos suficientes para spawnar nenhum inimigo e mesmo assim esta tentando spawnar");
        }

        

        //Filter enemies eligible to spawn.
        List<EnemyToSpawn> enemiesAvailableToSpawn = new List<EnemyToSpawn>();
        
        if(pointsRemaining - maxPointsToSpawn >=0) //The remaining budget can afford any enemy.
        { 
            enemiesAvailableToSpawn = enemies;
        }
        else  //Limit the selection to enemies the remaining budget can afford.
        {
            enemiesAvailableToSpawn = enemies.Where(x => x.enemyData.spawnPoints <= pointsRemaining).ToList();
        }

        enemiesSpawned++;


        //Currently, spawning uses random selection only.
        var enemyToSpawn = SelectRandomEnemy(enemiesAvailableToSpawn);

        pointsRemaining -= enemyToSpawn.spawnPoints;

        //Discard leftover points after reaching the spawn limit.
        if (enemiesSpawned >= maxEnemies)
        {
            pointsRemaining = 0;
        }
        //If the budget runs out before the required count, add enough points for one more enemy and repeat as needed.
        else if (pointsRemaining < minPointsToSpawn && enemiesSpawned < minEnemies)
        {
            pointsRemaining = minPointsToSpawn;
        }

        return enemyToSpawn;

    }

    public bool HasPointsToSpawn()
    {
        if (pointsRemaining == 0 || pointsRemaining < minPointsToSpawn)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    public EnemyData SelectRandomEnemy(List<EnemyToSpawn> enemiesToSpawn)
    {
        // Step 1: Normalize probabilities.
        float totalProbability = 0f;
        foreach (var enemy in enemiesToSpawn)
        {
            totalProbability += enemy.probability;
        }

        // Ensure the total is greater than zero.
        if (totalProbability <= 0)
        {
            Debug.LogError("As probabilidades devem ser maiores que 0.");
            return null;
        }

        // Step 2: Draw a random value within the total probability weight.
        float randomValue = Random.Range(0, totalProbability);

        // Step 3: Find the corresponding enemy.
        float cumulativeProbability = 0f;
        foreach (var enemy in enemiesToSpawn)
        {
            cumulativeProbability += enemy.probability;
            if (randomValue <= cumulativeProbability)
            {
                return enemy.enemyData;
            }
        }

        // Return null if selection fails.
        Debug.LogError("Nenhum inimigo foi selecionado. Verifique as probabilidades.");
        return null;
    }


}

[Serializable]
public struct EnemyToSpawn
{
    public EnemyData enemyData;
    [Range(0,1)] public float probability;
    [Range(0, 1)] public float elementalProbability;
}
