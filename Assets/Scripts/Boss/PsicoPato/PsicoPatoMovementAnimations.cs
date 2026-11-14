using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PsicoPatoBoss;

public class PsicoPatoMovementAnimations : EnemyMovementAnimation
{
    
    [SerializeField] protected PsicoPatoBoss psicoAI;
    protected PsicoPatoStates lastStage = PsicoPatoStates.seeking;

    [SerializeField] protected float velocityTolerance = 0.1f;
    [SerializeField] public bool lockDirection = false;
    //Subscribe as a listener.
    protected override void Awake()
    {
        base.Awake();
        psicoAI.OnChangeState += TriggerChangeStateAnimation;
    }

    //TODO: Lock the dash direction after applying the impulse.
    /// <summary>
    /// Reset Animator triggers on state changes so only one trigger remains active.
    /// </summary>
    public void TriggerChangeStateAnimation()
    {
        //Review this section.
        switch (psicoAI.GetCurrentState())
        {
            case PsicoPatoStates.idle:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Idle");
                break;
            case PsicoPatoStates.seeking:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Seek");
                break;
            case PsicoPatoStates.dash:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Dash");
                break;
            case PsicoPatoStates.egg:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Egg");
                break;
            case PsicoPatoStates.laser:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Laser");
                break;
            case PsicoPatoStates.shoot:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Shoot");
                break;
            case PsicoPatoStates.scream:
                Aux.ResetAllAnimationTriggersButDirectional(animator);
                animator.SetTrigger("Scream");
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
            // Debug.LogWarning("Enemy speed: " + myRigidbody.velocity.magnitude);
            //If the object should move,
            if (psicoAI.GetCurrentState() == PsicoPatoStates.seeking || psicoAI.GetCurrentState() == PsicoPatoStates.dash)
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
