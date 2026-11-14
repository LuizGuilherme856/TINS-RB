using UnityEngine;
using System.Collections.Generic;

public class DestroyOnDisable : MonoBehaviour
{
    [SerializeField] private List<GameObject> gameObjectsToDestroy = new List<GameObject>();

    void OnDisable()
    {
        foreach (GameObject obj in gameObjectsToDestroy)
        {   
            if (obj != null)
                Destroy(obj);
        }
    }
}
