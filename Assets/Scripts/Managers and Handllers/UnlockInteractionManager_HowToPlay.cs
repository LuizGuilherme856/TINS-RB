using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// Gate actions according to the current stage.
/// For example, prevent opening a door before the tutorial permits it.


public class UnlockInteractionManager_HowToPlay : MonoBehaviour
{
    //These paired arrays must have equal lengths.
    [SerializeField] private int[] stages;
    [SerializeField] private UnlockInteraction_HowToPlay[] interactionToUnlock;

    private HowToPlayManager howToPlayManager;
    private int currentStage = 0;

    private void Awake()
    {
        howToPlayManager = GetComponent<HowToPlayManager>();
    }

    void Start()
    {
        howToPlayManager.OnNewStage += CheckNewStage;
    }


    private void CheckNewStage()
    {
        currentStage = howToPlayManager.currentStage;

        for (int i = 0; i < stages.Length; i++)
        {
            if(currentStage == stages[i]) //If the stage requires unlocking an action,
            {
                interactionToUnlock[i].Unlock();
                return;
            }

        }

    }
}
