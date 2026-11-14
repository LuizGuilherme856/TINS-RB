using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlayStage_conversation : HowToPlayStage
{

    [SerializeField] private TalkToNpc dialogueSystem;
    
    //TODO: Automatically advance conversation-only stages,
    //for example every five seconds, and allow them to complete automatically.

    private void Start()
    {
        changeLineAutomaticallyWhenComplete = false;
    }
    public override void DialogueIsFinished()
    {
        base.DialogueIsFinished();
        SetCompleted();
    }



}
