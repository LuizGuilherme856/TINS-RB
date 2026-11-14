using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Stores possible spawn positions for NPCs, the player, and weapons.

public class PossibleSpawnPos_randomLevels : MonoBehaviour
{
    [SerializeField] private Transform[] playerPos, npcPos, gunPos;

    //Keep the duck and Kaishen from spawning at the same position.
    //Known limitation: the array may contain multiple positions that are not all checked.
    public bool isUsingNpcPos { get; private set; } 

    public Transform GetNpcPos()
    {
        isUsingNpcPos = true;
        return npcPos[Random.Range(0, npcPos.Length)];
    }

    public Transform GetPlayerPos()
    {
        return playerPos[Random.Range(0, playerPos.Length)];
    }

    public Transform GetGunPos()
    {
        return gunPos[Random.Range(0, gunPos.Length)];
    }
}
