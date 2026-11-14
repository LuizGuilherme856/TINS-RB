using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ShooterAI;


public class ShooterMovementAnimation : EnemyMovementAnimation
{
    [SerializeField] protected ShooterAI ShooterAI;
    protected ShooterStates lastStage = ShooterStates.seeking;

    [SerializeField] protected float velocityTolerance = 0.1f;
    //Subscribe as a listener.
    protected override void Awake()
    {
        base.Awake();
        ShooterAI.OnChangeState += TriggerChangeStateAnimation; 
    }


    /// <summary>
    /// Reset Animator triggers on state changes so only one trigger remains active.
    /// </summary>
    protected void TriggerChangeStateAnimation()
    {
        switch (ShooterAI.GetCurrentState())
        {
            case ShooterAI.ShooterStates.seeking:
                animator.SetTrigger("Seeking");
                animator.ResetTrigger("Alert");
                animator.ResetTrigger("Fire");
                break;
            case ShooterAI.ShooterStates.alert:
                animator.ResetTrigger("Seeking");
                animator.SetTrigger("Alert");
                animator.ResetTrigger("Fire");
                break;
            case ShooterAI.ShooterStates.firing:
                animator.ResetTrigger("Seeking");
                animator.ResetTrigger("Alert");
                animator.SetTrigger("Fire");
                break;
            default:
                break;
        }
    }

     /// <summary>
     /// Specialized so the enemy can determine its facing direction while stationary.
     /// </summary>
     protected override void CheckFourDirectionChange() 
     {
        Direction currentAIDirection;
        Direction currentVelocityDirection = CurrentDirection(myRigidbody.linearVelocity);
        if (movementAnimation == MovementAnimation.fourDirections)
        {

            currentAIDirection = CurrentDirection(enemyAI.desiredDirection);
            // Debug.LogWarning("Enemy speed: " + myRigidbody.velocity.magnitude);
            //If the object is stationary,
            if (ShooterAI.GetCurrentState()==ShooterStates.alert || ShooterAI.GetCurrentState() == ShooterStates.firing)
            {
                TriggerChangeAnimation(currentAIDirection);
            }
            else
            {
                /*update facing only when the AI direction agrees with velocity; sudden AI direction changes otherwise cause animation jitter.
                Face forward when moving forward, but keep facing the player when pushed backward.
                This delays animation changes slightly and can briefly make the enemy walk backward.*/
                if (currentAIDirection == currentVelocityDirection)
                {
                    TriggerChangeAnimation(currentAIDirection);
                }
            }

        }
        else //if(movementAnimation == MovementAnimation.fourDirectionsFlyier)
        {
            Vector3 direction = (targetTransform.transform.position - myRigidbody.transform.position).normalized;
            currentAIDirection = ConvertVectorToDirection(direction);
            TriggerChangeAnimation(currentAIDirection);
        }


    }


    //Keep the pool from flipping with the enemy sprite.
    public void SpawnBulletsByAnimation()
    {
        ShooterAI.SpawnBulletsByAnimation();
    }
    public void SetAIToSeeking() 
    {
        ShooterAI.GoToSeeking();
    }
}
