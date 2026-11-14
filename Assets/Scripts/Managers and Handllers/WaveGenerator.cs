using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

public class WaveGenerator : MonoBehaviour
{
    [SerializeField] private EnemyWave enemyWaveDefault;
    [SerializeField] private int numberOfWavesToCreate = 5; // Number of waves to create.
    [SerializeField] private int addToIndex = 0;

    

    // Start is called before the first frame update
    void Start()
    {
        CreateNewEnemyWaves();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CreateNewEnemyWaves()
    {
        for (int i = 0; i < numberOfWavesToCreate; i++)
        {
            EnemyWave newWave = ScriptableObject.CreateInstance<EnemyWave>();

            newWave.enemies = enemyWaveDefault.enemies;
            newWave.minEnemies = enemyWaveDefault.minEnemies + 5 * (i+1);
            newWave.maxEnemies = enemyWaveDefault.maxEnemies + 5 * (i + 1);
            newWave.spawnPoints = enemyWaveDefault.spawnPoints + 10 * (i + 1);
            

            string assetPath = $"Assets/Wave {i + addToIndex}.asset";
            UnityEditor.AssetDatabase.CreateAsset(newWave, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }
}

#endif