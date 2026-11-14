using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlayStage_SpecialAbility : HowToPlayStage
{

    [SerializeField] private PlayerInputHandler playerInputHandler;
    
    void Start()
    {
        playerInputHandler.OnSpecialAttackButtonDown += SetCompleted;    
    }

}
