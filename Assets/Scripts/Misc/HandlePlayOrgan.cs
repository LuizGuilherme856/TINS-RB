using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlePlayOrgan : InteractWithPlayer
{

    protected override void InteractionAction()
    {
        base.InteractionAction();
        Debug.Log("action");
    }
}
