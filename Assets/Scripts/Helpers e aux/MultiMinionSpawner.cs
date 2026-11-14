using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MinionSpawner;

public class MultiMinionSpawner : MinionSpawner
{
    [SerializeField] protected List<EnemyType> enemysToSpawn;

    public override IEnumerator SpawnEnemies()
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

            spawnManager.SpawnEnemyAtPosition(enemysToSpawn[Random.Range(0, enemysToSpawn.Count)], spawnPositions[positionIndex]);
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
        }
    }

}

