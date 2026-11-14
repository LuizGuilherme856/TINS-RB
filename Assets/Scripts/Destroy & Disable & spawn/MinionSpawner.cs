using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionSpawner : MonoBehaviour
{
    public enum SpawnMode
    {
        Sequential,
        Random
    }

    [SerializeField] protected EnemyType enemyToSpawn;
    [SerializeField] protected int numberOfEnemies = 1;
    [SerializeField] protected List<Transform> spawnPositions;
    [SerializeField] protected float delayBetweenSpawns = 1f;
    [SerializeField] protected bool hasFeedbacks = false;
    [SerializeField] protected MMF_Player spawnFeedbacks;

    [Tooltip("Sequential ele spawna em ordem na lista de posições. Primeiro na pos 1 depois 2, etc." +
        "Random é aleatório. Qualquer posição")]
    [SerializeField] protected SpawnMode spawnMode = SpawnMode.Sequential;

    protected SpawnManager spawnManager;

    public virtual event Action FirstSpawnComplete;

    private bool stopSpawn = false;

    protected void Awake()
    {
        spawnManager = AssetFinder.GetSpawnManager();
    }


    public virtual IEnumerator SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            int positionIndex;

            if (spawnMode == SpawnMode.Sequential)
            {
                positionIndex = i % spawnPositions.Count;
            }
            else // Random
            {
                positionIndex = UnityEngine.Random.Range(0, spawnPositions.Count);
            }

            spawnManager.SpawnEnemyAtPosition(enemyToSpawn, spawnPositions[positionIndex]);
            if (hasFeedbacks)
            {
                spawnFeedbacks.PlayFeedbacks();
            }
            //On the first spawn:
            if (i == 0)
            {
                InvokeFistSpawn();
            }
            yield return new WaitForSeconds(delayBetweenSpawns);

            if (stopSpawn)
            {
                yield break; // Exit when stopSpawn is true.
            }
        }
    }

    public void StopSpawn()
    {
        stopSpawn = true;
    }

    protected void InvokeFistSpawn()
    {
        FirstSpawnComplete?.Invoke();
    }
}
