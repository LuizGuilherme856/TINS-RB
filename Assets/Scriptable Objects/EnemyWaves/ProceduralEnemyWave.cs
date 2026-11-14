using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[CreateAssetMenu(fileName = "ProceduralWave ", menuName = "Others/Procedural Wave")]
public class ProceduralEnemyWave : EnemyWave
{
    /*
     This script uses the formulas defined in the balancing spreadsheet:
    https://docs.google.com/spreadsheets/d/1juCJlSUPsdTJv3xO2g8QeUKD61oU-Kwwcy3YoxvSbCU/edit?gid=287508753#gid=287508753
     
     */
    public ProceduralEnemyWave(List<EnemyToSpawn> enemies, int minEnemies, int maxEnemies, int spawnPoints) : base(enemies, minEnemies, maxEnemies, spawnPoints)
    { 
        this.enemies = enemies;
        this.minEnemies = minEnemies;
        this.maxEnemies = maxEnemies;
        this.spawnPoints = spawnPoints;
    }
    public float mod5Mult = 0.25f; //Elite wave multiplier.
    public int getMinEnemies(int waveN)
    {
        if (waveN % 5 == 0) // Whether this is an elite wave.
        {
            return (int)Mathf.Ceil(5 * waveN - 10 + mod5Mult*(5 * waveN - 10));
        }
        else 
        {
            return 5 * waveN - 10;
        }
    }

    public int getMaxEnemies(int waveN)
    {
        if (waveN % 5 == 0) // Whether this is an elite wave.
        {
            return (int)Mathf.Ceil(5.2f * waveN + 6 + mod5Mult * (5.2f * waveN + 6));
        }
        else
        {
            return (int)Mathf.Ceil(5.2f * waveN + 6);
        }
    }

    public int getWavePoints(int waveN)
    {
        if (waveN % 5 == 0) // Whether this is an elite wave.
        {
            return (int)Mathf.Ceil(10 * waveN + mod5Mult * (10 * waveN));
        }
        else
        {
            return 10 * waveN;
        }
    }
}
