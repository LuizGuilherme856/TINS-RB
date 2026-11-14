using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakOnTouchManager : MonoBehaviour
{
    [SerializeField]private List<BreakOnTouch> breakables = new List<BreakOnTouch>();
    private int brokenCount = 0;
    [SerializeField] private bool usesNumberToBrake = false;
    [SerializeField] private int numberToBrake = 20;
    public Action OnBreakAll, OnBreakMin;
    void Start()
    {

        foreach (var breakable in breakables)
        {
            breakable.OnBreak += OnBreakableBroken;
        }
    }

    private void OnBreakableBroken()
    {
        brokenCount++;
       // Debug.Log($"Quebrou {brokenCount}/{breakables.Count}");

        if (brokenCount >= breakables.Count)
        {
            // Debug.Log("All BreakOnTouch objects have been broken!");
            // Additional events or method calls may be triggered here.
            OnBreakAll?.Invoke();
        }

        if (usesNumberToBrake)
        {
            if (brokenCount >= numberToBrake)
            {
                // Debug.Log("All BreakOnTouch objects have been broken!");
                // Additional events or method calls may be triggered here.
                OnBreakMin?.Invoke();
            }
        }

    }
}
