using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[CreateAssetMenu(fileName = "new enemy", menuName = "floor")]
public class FloorExplosionData : ScriptableObject
{

    [field: SerializeField] public Vector3[] linearOffSet { get; private set; }
    [field: SerializeField] public int[] maxExplosions { get; private set; }

    [field: SerializeField] public float[] explosionRate { get; private set; }
    [field: SerializeField] public float[] sinOffset { get; private set; }

    [field: SerializeField] public float[] timeBetweenExplosionTriggers { get; private set; }

    [field: SerializeField] public Vector3 limitPos { get; private set; }

}
