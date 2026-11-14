using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyTrap : InteractWithPlayer
{
    [SerializeField] private int price = 300;
   // [SerializeField] private int opensRegion = 1; //index of the region this door unlocks 

    [SerializeField] private string openText = "detonate";
    private Animator animator;

    private bool isActive = false;
    public event Action OnActivateTrap;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }


    protected override void StartInteraction()
    {
        if (isActive) return; //its already open so you cant open it again

        InputPromptDeviceType deviceType = playerInputHandler != null
            ? playerInputHandler.CurrentPromptDevice
            : InputPromptDeviceType.KeyboardMouse;
        string buttonPrompt = buttons != null
            ? buttons.GetInteractionButton(deviceType)
            : deviceType == InputPromptDeviceType.KeyboardMouse ? "E" : "A";
        generalTextBoxUI.SetText("Press " + buttonPrompt + " To " + openText + " for " + price);
    }

    protected override void InteractionAction()
    {
        if (isActive) return; //its already open so you cant open it again

        if (pointManager.currentPoints >= price)
        {
            pointManager.RemovePoints(price);
            buyFeedback.PlayFeedbacks();
            TriggerTrap();
        }
        else
        {
            cantBuyFeedback.PlayFeedbacks();
        }
    }


    private void TriggerTrap()
    {
        Invoke(nameof(ScanPathFinding), 0);

        isActive = true;
        EndInteraction();
        animator.SetTrigger("Activate");
        OnActivateTrap?.Invoke();
        //Destroy(gameObject);
    }


    private void ScanPathFinding()
    {
        AstarPath.active.Scan();
    }


}
