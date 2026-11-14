using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HandAI;

public class HandMovementAnimation : EnemyMovementAnimation
{
    [SerializeField] protected HandAI handAI;
    //protected handStates lastStage = handStates.seeking;

    //Subscribe as a listener.
    protected override void Awake()
    {
        base.Awake();
        try
        {
            handAI = GetComponent<HandAI>();
        }
        catch (System.Exception)
        {
            Debug.LogError("Base Hand não possui HandAi");

        }
        handAI.OnChangeState += TriggerChangeStateAnimation;
    }


    /// <summary>
    /// Reset Animator triggers on state changes so only one trigger remains active.
    /// </summary>
    protected void TriggerChangeStateAnimation()
    {
        switch (handAI.GetCurrentState())
        {
            case HandAI.handStates.seeking:
                animator.SetTrigger("Seeking");
                animator.ResetTrigger("Escaping");
                animator.ResetTrigger("Alert");
                animator.ResetTrigger("Jumping");
                animator.ResetTrigger("Recovering");
                break;
            case HandAI.handStates.escaping:
                animator.ResetTrigger("Seeking");
                animator.SetTrigger("Escaping");
                animator.ResetTrigger("Alert");
                animator.ResetTrigger("Jumping");
                animator.ResetTrigger("Recovering");
                break;
            case HandAI.handStates.alert:
                animator.ResetTrigger("Seeking");
                animator.ResetTrigger("Escaping");
                animator.SetTrigger("Alert");
                animator.ResetTrigger("Jumping");
                animator.ResetTrigger("Recovering");
                break;
            case HandAI.handStates.jumping:
                animator.ResetTrigger("Seeking");
                animator.ResetTrigger("Escaping");
                animator.ResetTrigger("Alert");
                animator.SetTrigger("Jumping");
                animator.ResetTrigger("Recovering");
                break;
            case HandAI.handStates.recovering:
                animator.ResetTrigger("Seeking");
                animator.ResetTrigger("Escaping");
                animator.ResetTrigger("Alert");
                animator.ResetTrigger("Jumping");
                animator.SetTrigger("Recovering");
                break;
            default:
                break;
        }
    }
}
