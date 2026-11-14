using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Deactivate the GameObject after the specified wave.
/// </summary>
public class DisableAfterWave : MonoBehaviour
{
    [SerializeField] int waveToDisable;
    [SerializeField] private RunData runData;
    [SerializeField] private EnemyWaveManager hordeManager;


    private void OnEnable()
    {
        hordeManager.OnStartNewWave += CheckCurrentWave;
    }

    private void CheckCurrentWave()
    {
        if(runData.currentWaveNumber >= waveToDisable)
        {
            gameObject.SetActive(false);
        }
    }

}
