using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SwordBellBossAI;

public class SwordBellMovementAnimations : EnemyMovementAnimation
{
    [SerializeField] protected SwordBellBossAI BellAI;
    protected SwordBellStates lastStage = SwordBellStates.seeking;

    [SerializeField] protected float velocityTolerance = 0.1f;
    [SerializeField] public bool lockDirection = false;
    //Subscribe as a listener.
    protected override void Awake()
    {
        base.Awake();
        try
        {
            BellAI = GetComponent<SwordBellBossAI>();
        }
        catch (Exception)
        {
            Debug.LogError("Objeto que possiu spearbellAnimations n�o possui um SpearBellAi");
        }
        BellAI.OnChangeState += TriggerChangeStateAnimation;
    }

    //TODO: Lock the dash direction after applying the impulse.
    /// <summary>
    /// Reset Animator triggers on state changes so only one trigger remains active.
    /// </summary>
    public void TriggerChangeStateAnimation()
    {
        switch (BellAI.GetCurrentState())
        {
            case SwordBellStates.idle:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Idle");
                break;
            case SwordBellStates.seeking:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Seeking");
                break;
            case SwordBellStates.alert:
                AlertTriggerChangeStateAnimation(BellAI.GetNextAttackState());
                break;
            case SwordBellStates.spin:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Spin");
                break;
            case SwordBellStates.stun:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Stun");
                break;
            case SwordBellStates.shoot:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Shoot");
                break;
            case SwordBellStates.ring:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Ring");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// In alert, inspect the next attack and trigger it.
    /// </summary>
    /// <param name="nextAttackState"></param>
    public void AlertTriggerChangeStateAnimation(SwordBellStates nextAttackState)
    {
        //Review this section.
        switch (BellAI.GetNextAttackState())
        {
            case SwordBellStates.spin:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Spin");
                break;
            case SwordBellStates.shoot:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Shoot");
                break;
            case SwordBellStates.ring:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Ring");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Updating lastDirection without refreshing the animation previously caused direction changes to be missed.
    /// </summary>
    /// <param name="currentAIDirection"></param>
    protected override void TriggerChangeAnimation(Direction currentAIDirection)
    {
        if (lockDirection)
        {
            return;
        }

        if (currentAIDirection != lastDirection || triggerAnimationEveryFrame)
        {
            Aux.Print("" + currentAIDirection, showDebug);
            UpdateFourDirectionAnimation(currentAIDirection);
        }

        lastDirection = currentAIDirection;
    }


    /// <summary>
    /// Specialized so the enemy can determine its facing direction while stationary.
    /// </summary>
    protected override void CheckFourDirectionChange()
    {
        if (lockDirection)
        {
            return;
        }
        Direction currentAIDirection;
        Direction currentVelocityDirection = CurrentDirection(myRigidbody.linearVelocity);
        if (movementAnimation == MovementAnimation.fourDirections)
        {

            currentAIDirection = CurrentDirection(enemyAI.desiredDirection);
            //If the object should move,
            if (BellAI.GetCurrentState() == SwordBellStates.seeking)
            {
                /*update facing only when the AI direction agrees with velocity; sudden AI direction changes otherwise cause animation jitter.
                Face forward when moving forward, but keep facing the player when pushed backward.
                This delays animation changes slightly and can briefly make the enemy walk backward.*/
                if (currentAIDirection == currentVelocityDirection)
                {
                    TriggerChangeAnimation(currentAIDirection);
                }

            }
            else
            {
                TriggerChangeAnimation(currentAIDirection);
            }

        }
        else //if(movementAnimation == MovementAnimation.fourDirectionsFlyier)
        {
            Vector3 direction = (targetTransform.transform.position - myRigidbody.transform.position).normalized;
            currentAIDirection = ConvertVectorToDirection(direction);
            TriggerChangeAnimation(currentAIDirection);
        }


    }
}
