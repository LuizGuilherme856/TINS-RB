using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NecromancerAI;


public class NecromancerMovementAnimation : EnemyMovementAnimation
{
    [SerializeField] protected NecromancerAI necromancerAi;
    protected NecromancerStates lastStage = NecromancerStates.seeking;
    protected float timeSincelastFlip = 10f;
    [SerializeField] protected float timeToChangeTolerance = 0.1f;
    //Subscribe as a listener.
    protected override void Awake()
    {
        base.Awake();
        necromancerAi.OnChangeState += TriggerChangeStateAnimation; 
    }
    protected override void LateUpdate()
    {
        timeSincelastFlip += Time.deltaTime;
        base.LateUpdate();
    }

    /// <summary>
    /// Reset Animator triggers on state changes so only one trigger remains active.
    /// </summary>
    protected void TriggerChangeStateAnimation()
    {
        switch (necromancerAi.GetCurrentState())
        {
            case NecromancerStates.seeking:
                animator.SetTrigger("Seeking");
                animator.ResetTrigger("Alert");
                animator.ResetTrigger("Fire");
                break;
            case NecromancerStates.alert:
                animator.ResetTrigger("Seeking");
                animator.SetTrigger("Alert");
                animator.ResetTrigger("Fire");
                break;
            case NecromancerStates.firing:
                animator.ResetTrigger("Seeking");
                animator.ResetTrigger("Alert");
                animator.SetTrigger("Fire");
                break;
            default:
                break;
        }
    }

    
    /// <summary>
    /// Override to prevent rapid repeated flipping while escaping or aiming.
    /// </summary>
    protected override void FlipAnimation()
    {
        Direction currentAIDirection = CurrentDirectionXOnly(necromancerAi.desiredDirection); 
        Direction currentVelocityDirection = CurrentDirectionXOnly(myRigidbody.linearVelocity);
        //Prevent repeated sprite flipping.
        if (timeSincelastFlip >= timeToChangeTolerance)
        {
            if (necromancerAi.GetCurrentState() == NecromancerStates.alert || necromancerAi.GetCurrentState() == NecromancerStates.firing)
            {
                Flip();
            }
            else
            {
                /*update facing only when the AI direction agrees with velocity; sudden AI direction changes otherwise cause animation jitter.
                Face forward when moving forward, but keep facing the player when pushed backward.
                This delays animation changes slightly and can briefly make the enemy walk backward.*/
                if (currentAIDirection == currentVelocityDirection)
                {
                    Flip();
                }
            }
        }



    }

    //Extracted for reuse.
    protected override void Flip() 
    {
        Vector3 direction = (targetTransform.transform.position - myRigidbody.transform.position).normalized;

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(originalLocalScale.x, originalLocalScale.y, originalLocalScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-originalLocalScale.x, originalLocalScale.y, originalLocalScale.z);
        }
    }
}
