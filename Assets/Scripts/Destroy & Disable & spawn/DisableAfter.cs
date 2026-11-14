using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//disables the game object after some time. Used in MMF object pool
public class DisableAfter : MonoBehaviour
{
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private bool waitDisableCall = false;
    private void OnEnable()
    {
        if (waitDisableCall) return;
        Invoke(nameof(TriggerDisable), waitTime);
    }

    private void TriggerDisable()
    {
        gameObject.SetActive(false);
    }

    public void DisableNow()
    {
        gameObject.SetActive(false);
    }
}
