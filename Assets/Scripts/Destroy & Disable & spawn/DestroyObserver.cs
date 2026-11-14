using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//Destroy this GameObject when the observed Destroy components raise their events.
public class DestroyObserver : MonoBehaviour
{
    [SerializeField] private Destroy[] destroyScripts;
    [SerializeField] private DestroyMethod destroyMethod = DestroyMethod.deactivateGameObject;

    //private int destroyedObj = 0;

    private void OnEnable()
    {
        foreach (var destroy in destroyScripts)
        {
            destroy.OnTriggerDestroy += HandleDestroy;
        }
       // destroyedObj = 0;
    }


    private void HandleDestroy()
    {
        if (destroyMethod == DestroyMethod.deactivateGameObject)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
