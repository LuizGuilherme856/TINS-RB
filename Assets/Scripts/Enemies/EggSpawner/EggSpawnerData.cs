using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[CreateAssetMenu(fileName = "EggSpawnerData", menuName = "EggSpawner")]
public class EggSpawnerData : EnemyData
{
    [Header("EggSpawnerSpecific")]
    [SerializeField] public float timeToSpawnEnemy = 2.0f;
}
