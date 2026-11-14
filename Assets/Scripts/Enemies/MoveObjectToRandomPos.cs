using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjectToRandomPos : MonoBehaviour
{
    [SerializeField] protected Transform objToMove;
    [SerializeField] protected Transform[] possiblePositions;

    protected virtual void  Awake()
    {
        MoveObject();
    }

    protected virtual void MoveObject()
    {
        objToMove.position = possiblePositions[Random.Range(0, possiblePositions.Length)].position;
    }
}
