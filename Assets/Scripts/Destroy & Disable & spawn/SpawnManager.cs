//Spawns enemies at positions belonging to unlocked regions.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    //Enemy spawn positions become available as the player unlocks regions.
    [SerializeField] private RegionSpawnPositions[] regionSpawnPositions;
    [SerializeField] private EnemySpawn[] enemySpawns;
    [SerializeField] private Transform[] outOfTheMapSpawn;

    [Header("   Enemies prefabs")] //For enemies that do not use pooling.
    [SerializeField] private GameObject cthulhuPrefab;

    /// <summary>
    /// Activate the player's starting region; disabled only in safe zones.
    /// </summary>
    [SerializeField] private bool autoActivateRegion = true;

    private List<int> activeRegions = new List<int>(); //TODO: Verify whether this code has any effect.
    private int currentPlayerRegion; //region that the player is in. 

    public event Action OnChangePlayerRegion;


    private void Awake()
    {
        if (autoActivateRegion)
        {
            SetRegionActive(0); //Region 0 remains active because the player starts there.
        }
    }

    public void SetRegionActive(int region)
    {
        regionSpawnPositions[region].isActive = true;

        //Refresh the cached list of unlocked region indices.
        for (int i = 0; i < regionSpawnPositions.Length; i++) 
        {
            if (regionSpawnPositions[i].isActive)
            {
                activeRegions.Add(i);
            }
        }


    }

    public void InstantiateEnemy(EnemyType enemyToSpawn) //Does not use pooling.
    {
        Transform spawnTransform = outOfTheMapSpawn[Random.Range(0, outOfTheMapSpawn.Length)];
        if (enemyToSpawn == EnemyType.cthulhu)
        {
            Instantiate(cthulhuPrefab, spawnTransform.position, spawnTransform.rotation);
        }
        else
        {
            Debug.LogError("Tipo de inimigo nao suportado ainda pra instanciação sem pool... Spawnando um demoninho");
            SpawnEnemy(EnemyType.demoninho, true);
        }
    }


    public GameObject SpawnEnemyAtPosition(EnemyType enemyToSpawn, Transform position)
    {
        //Find the EnemySpawner for the requested enemy type.
        foreach (EnemySpawn spawn in enemySpawns) 
        {
            if (spawn.GetEnemyType() == enemyToSpawn)
            {
                var enemyGameObj =  spawn.SpawnAndGet(position);
                return enemyGameObj;
            }

        }

        Debug.LogError("Tipo de inimigo nao encontrado. Nada foi spawnado");
        return null;

    }

    /// <summary>
    /// Activate the enemy and return its reference.
    /// </summary>
    /// <returns></returns>
    public GameObject SpawnEnemyAtPostionAndGet(EnemyType enemyToSpawn, Transform position)
    {
        //Find the EnemySpawner for the requested enemy type.
        foreach (EnemySpawn spawn in enemySpawns)
        {
            if (spawn.GetEnemyType() == enemyToSpawn)
            {


                GameObject targetEnemy = spawn.SpawnAndGet(position);
                return targetEnemy;
            }
        }
        return null;

    }


    public void SpawnEnemy(EnemyType enemyToSpawn, bool spawnWithinTheMap = true)
    {

        foreach(EnemySpawn spawn in enemySpawns) //Find the EnemySpawner for the requested enemy type.
       {
            if(spawn.GetEnemyType() == enemyToSpawn)
            {
                if (spawnWithinTheMap)
                {
                    int region = currentPlayerRegion;
                    spawn.Spawn(regionSpawnPositions[region].GetRandomSpawnPositionOnRegion(false));
                }
                else //Spawn outside the map for flying enemies.
                {
                    spawn.Spawn(outOfTheMapSpawn[Random.Range(0, outOfTheMapSpawn.Length)]);
                }

                return;
            }
        }

    }

    public void SetCurrentPlayerRegion(int i)
    {
        currentPlayerRegion = i;
        OnChangePlayerRegion?.Invoke();
    }

    public int GetCurrentPlayerRegion()
    {
        return currentPlayerRegion;
    }

}

