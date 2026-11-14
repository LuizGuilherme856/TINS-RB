using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneManager : MonoBehaviour
{

    public GameObject debugGameObj;
    public List<GameObject> objsToDisable;
    public PlayerInputHandler playerInputHandler;

    void OnEnable()
    {
        playerInputHandler.OnCheat += EnableDebug;
    }

    private void EnableDebug()
    {
        foreach (var obj in objsToDisable)
        {
            if (obj == null) continue;
            obj.SetActive(false);
        }

        debugGameObj.SetActive(true);
    }

}
