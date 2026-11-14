using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Change material properties.
/// Originally used for wave-transition effects.
/// </summary>
public class HandleWaveTextMaterialChange : ScrambleText
{

    [SerializeField] private EnemyWaveManager enemyWaveManager;

    private void OnEnable()
    {
        UpdateWaveNumber_Animation();
    }


    public void UpdateWaveNumber_Animation()
    {
        int currentWave = enemyWaveManager.GetCurrentWave();
        if(currentWave < 16)
        {
            textMesh.text = Aux.ToRoman(currentWave);
        }
        else
        {
            textMesh.text = currentWave.ToString();
        }

    }

   

}
