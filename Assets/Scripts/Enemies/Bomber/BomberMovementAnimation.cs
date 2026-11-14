using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BomberAI;


public class BomberMovementAnimation : EnemyMovementAnimation
{
    [SerializeField] protected BomberAI bomberAI;
    protected bomberStates lastStage = bomberStates.seeking;

    //Subscribe as a listener.
    protected override void Awake()
    {
        base.Awake();
        bomberAI.OnChangeState += TriggerChangeStateAnimation;
    }


    /// <summary>
    /// Reset Animator triggers on state changes so only one trigger remains active.
    /// </summary>
    protected void TriggerChangeStateAnimation()
    {
        switch (bomberAI.GetCurrentState())
        {
            case BomberAI.bomberStates.seeking:
                animator.SetTrigger("Seeking");
                animator.ResetTrigger("Alert");
                animator.ResetTrigger("Running");
                animator.ResetTrigger("Exploding");
                break;
            case BomberAI.bomberStates.alert:
                animator.ResetTrigger("Seeking");
                animator.SetTrigger("Alert");
                animator.ResetTrigger("Running");
                animator.ResetTrigger("Exploding");
                break;
            case BomberAI.bomberStates.running:
                animator.ResetTrigger("Seeking");
                animator.ResetTrigger("Alert");
                animator.SetTrigger("Running");
                animator.ResetTrigger("Exploding");
                break;
            case BomberAI.bomberStates.exploding:
                animator.ResetTrigger("Seeking");
                animator.ResetTrigger("Alert");
                animator.ResetTrigger("Running");
                animator.SetTrigger("Exploding");
                break;
            default:
                break;
        }
    }
}
