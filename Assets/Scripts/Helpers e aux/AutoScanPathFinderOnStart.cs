using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

/// <summary>
/// Scan the environment when gameplay starts.
/// Scanning too early missed obstacles
/// placed during startup, such as Kaishen.
/// </summary>
public class AutoScanPathFinderOnStart : MonoBehaviour
{

    private void Start()
    {
        //Scan();
        Invoke(nameof(Scan),0.1f);
    }

    public void Scan()
    {
        AstarPath.active.Scan();
    }

}
