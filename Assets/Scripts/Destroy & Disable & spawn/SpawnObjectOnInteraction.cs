using Pathfinding.RVO;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;

public class SpawnObjectOnInteraction : InteractWithPlayer
{
    public GameObject objToSpawn;
    public float delayToSpawn = 0.5f;
    public bool canSpawn = true;
    private bool interactionFinished = false;
    public bool useAnimator = false;
    public bool showTextOnInteraction = true;
    private Animator animator;

    public Action OnInteraction;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    protected override void StartInteraction()
    {
        if (interactionFinished) return; 

        base.StartInteraction();
        if (showTextOnInteraction == false) return;

        switch (languageData.currentLanguage)
        {
            case Language.english:
                generalTextBoxUI.SetText(interactionButton + " Open ");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText(interactionButton + " 开启 ");
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText(interactionButton + " Abrir ");
                break;
            case Language.spanish:
                generalTextBoxUI.SetText(interactionButton + " Abrir ");
                break;
            default:
                break;
        }
        

        
    }

    protected override void InteractionAction()
    {
        if (interactionFinished) return; //its already open so you cant open it again
        interactionFinished = true;
        StartCoroutine(SpawnObject_Routine());
        //TODO: Raise an event when opened.
        OnInteraction?.Invoke();
    }

    private IEnumerator SpawnObject_Routine()
    {
        buyFeedback.PlayFeedbacks();
        if (useAnimator && animator != null)
        {
            animator.SetTrigger("Interaction");
        }

        yield return new WaitForSeconds(delayToSpawn);
        if(canSpawn) objToSpawn.SetActive(true);
    }

    public void ResetAchEvent()
    {
        OnInteraction = null;
    }

    public bool GetInteractionFinished()
    {
        return interactionFinished;
    }
}
