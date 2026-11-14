using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Display training-dummy information.
/// </summary>
public class TrainingDummy : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMesh;
    EnemyData enemyData;

    void Start()
    {
        enemyData = GetComponent<EnemyHealth>().enemyData;
        textMesh.text = enemyData.enemyElement.ToString();

    }

 
}
