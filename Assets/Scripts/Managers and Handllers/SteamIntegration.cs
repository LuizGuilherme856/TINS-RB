using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Desktop Steam lifecycle entry point. Platform ports must isolate these calls and their native
// libraries; catching initialization failures alone does not remove the compile-time dependency.
public class SteamIntegration : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Aux.TryToConnectToSteam();
    }



    void OnApplicationQuit()
    {
        Steamworks.SteamClient.Shutdown();
        Debug.Log("Steam API shutdown successfully.");
    }
}
