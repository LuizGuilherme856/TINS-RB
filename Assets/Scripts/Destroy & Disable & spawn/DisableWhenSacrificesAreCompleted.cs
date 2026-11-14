using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableWhenSacrificesAreCompleted : MonoBehaviour
{

    [SerializeField] private GameObject[] gameObjectsToDisable;

    [SerializeField] private ManageSacrifice[] sacrificesToObserve;

    private void OnEnable()
    {
        foreach (var sacrifice in sacrificesToObserve)
        {
            sacrifice.OnSacrificeComplete += CheckIfSacrificesAreComplete;
        }

        CheckIfSacrificesAreComplete();
        Invoke(nameof(CheckIfSacrificesAreComplete), 5); //Intentional redundancy as a fallback.

    }

    private void OnDisable()
    {
        foreach (var sacrifice in sacrificesToObserve)
        {
            sacrifice.OnSacrificeComplete -= CheckIfSacrificesAreComplete;
        }
    }


    private void CheckIfSacrificesAreComplete()
    {
        foreach (var sacrifice in sacrificesToObserve)
        {
            if(sacrifice.IsCompleted() == false)
            {
                return;
            }
        }

        DisableGameObjects();
    }

    private void DisableGameObjects()
    {
        foreach (var gameObj in gameObjectsToDisable)
        {
            gameObj.SetActive(false);
        }
    }
}
