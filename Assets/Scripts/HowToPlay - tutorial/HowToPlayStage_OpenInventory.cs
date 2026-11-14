using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlayStage_OpenInventory : HowToPlayStage
{
    [SerializeField] private PlayerInputHandler playerInputHandler;
    void Start()
    {
        playerInputHandler.OnInventory += SetCompleted;
    }


}
