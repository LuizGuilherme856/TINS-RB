using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlayStage_FillSacrifice : HowToPlayStage
{
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private float spawnRate = 1f;
    [SerializeField] private EnemyType enemyType;
    [SerializeField] private ManageSacrifice sacrificeToObserve;

    private float timer;
    private bool startStage = false;

    private void Start()
    {
        sacrificeToObserve.OnSacrificeComplete += SetCompleted;
        sacrificeToObserve.OnSacrificeComplete += () => startStage = false;
    }
    public override void StartStage()
    {
        startStage = true;
    }


    private void FixedUpdate()
    {
        if (startStage == false) return;
        
        if(timer <= 0)
        {
            spawnManager.SpawnEnemy(enemyType, true);
            timer = spawnRate;
        }

        timer -= Time.fixedDeltaTime;
    }
}
