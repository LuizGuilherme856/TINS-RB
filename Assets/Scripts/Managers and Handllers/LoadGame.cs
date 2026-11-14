using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGame : MonoBehaviour
{

    [SerializeField] private PlayerData playerData;

    private void Start()
    {
        Debug.Log("loading save...");
        LoadSave();
    }

    public void LoadSave()
    {
        playerData.LoadPlayerData();
    }
}
