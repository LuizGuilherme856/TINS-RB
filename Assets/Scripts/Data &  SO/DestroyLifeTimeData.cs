using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "new data", menuName = "destroy life time data")]
/// <summary>
/// Shared corpse lifetime limits, allowing cleanup settings to be changed centrally.
/// </summary>
public class DestroyLifeTimeData : ScriptableObject
{

    [field: SerializeField] public float corpseLifeTime = 50f;
    [field: SerializeField] public float corpseMaxLifeTime = 120f;
}
