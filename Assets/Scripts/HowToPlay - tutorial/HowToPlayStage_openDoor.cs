using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HowToPlayStage_openDoor : HowToPlayStage
{
    [SerializeField] private ClosedDoor closedDoorToObserve;

    //public override event Action OnComplete;

    private void Start()
    {
        closedDoorToObserve.OnOpenDoor += SetCompleted;
    }

}
