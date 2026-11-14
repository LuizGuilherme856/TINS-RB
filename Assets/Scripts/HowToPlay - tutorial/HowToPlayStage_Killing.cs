using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Spawn enemies and complete the stage after all are dead.

public class HowToPlayStage_Killing : HowToPlayStage
{
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private int enemiesToSpawn;
    [SerializeField] private EnemyType enemyType;

    //public override event Action OnComplete;

    private int enemiesKilled =0;
    public override void StartStage()
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            spawnManager.SpawnEnemy(enemyType, true);
        }

        var enemyHealths = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        foreach (var health in enemyHealths)
        {
            health.OnZeroHealth += () => enemiesKilled++;
        }
    }

    private void Update()
    {
        if(enemiesKilled >= enemiesToSpawn)
        {
            SetCompleted();
        }
    }
}
