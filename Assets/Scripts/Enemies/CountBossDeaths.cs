using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Boss corpse components register a boss death when the corpse spawns.

public class CountBossDeaths : MonoBehaviour
{
    [SerializeField] private RunData runData;
    [SerializeField] private BossType bossType;

    private void OnEnable()
    {
        switch (bossType)
        {
            case BossType.gromsch:
                runData.numberOfGromschsAlive--;
                break;
            case BossType.cthulhu:
                runData.numberOfCthulhusAlive--;
                break;
            default:
                break;
        }
    }
}

public enum BossType { gromsch, cthulhu};