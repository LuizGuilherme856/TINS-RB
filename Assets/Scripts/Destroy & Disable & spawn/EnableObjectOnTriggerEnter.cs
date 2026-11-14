using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableObjectOnTriggerEnter : MonoBehaviour
{
    [Header("Objects to disable")]
    public List<GameObject> objectsToDisable;

    [Header("Objects to enable")]
    public List<GameObject> objectsToEnable;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object that entered the trigger has the "Player" tag
        if (collision.CompareTag("Player"))
        {
            // Disable all objects in the disable list
            foreach (var obj in objectsToDisable)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            // Enable all objects in the enable list
            foreach (var obj in objectsToEnable)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
    }
}
