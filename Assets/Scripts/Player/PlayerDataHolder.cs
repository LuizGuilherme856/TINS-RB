using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Central holder for the ScriptableObject reference.

public class PlayerDataHolder : MonoBehaviour
{

    [SerializeField] private PlayerData playerData;

    public PlayerData GetPlayerData() { return playerData; }
    public void SetPlayerData(PlayerData playerData) 
    {
        this.playerData = playerData;
    }
}
