using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ScanPathFinder : MonoBehaviour
{

    public void AsyncScan()
    {
        //AstarPath.active.ScanAsync();
        StartCoroutine(nameof(ScanPathFinding));

    }
    //Workaround: ScanAsync requires this additional call to function correctly.
    IEnumerator ScanPathFinding()
    {

        foreach (Progress progress in AstarPath.active.ScanAsync())
        {
            // Debug.Log("Scanning... " + progress.ToString());
            yield return null;
        }


    }
}
