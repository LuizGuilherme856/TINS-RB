///Update enemy animations and
///disable the Animator while frozen.

using UnityEngine;


public class EnemyMovementAnimation : MonoBehaviour
{
    [SerializeField] protected MovementAnimation movementAnimation = MovementAnimation.fourDirections;
    [SerializeField] protected bool showDebug = true;
    
    [SerializeField] protected bool hasIdle = false;
    [SerializeField] protected float minDist = 0.5f; //Enter idle inside this minimum distance.
    [SerializeField] protected bool triggerAnimationEveryFrame = false; //Workaround for Cthulhu's animation.
    // [Tooltip("Sprite sorting update interval to avoid drawing over the player")]
   // [SerializeField] protected float spriteLayerUpdateRate = 1f;

    protected Rigidbody2D myRigidbody;
    protected Animator animator;
    protected Vector3 originalLocalScale;
    protected Direction lastDirection;
    protected EnemyAI enemyAI;

    //protected SpriteRenderer spriteRenderer;
    protected Transform targetTransform;


    protected StatusEffect currentStatusEffect = StatusEffect.None;
    protected StatusEffectOnEnemy statusEffectOnEnemy;

    protected float lastHorizontalDirection;

    [SerializeField] protected bool usesParentRigidBodyAndAI = false;

    [SerializeField] protected bool usesMomentumInFlipAnimation = false;
    [SerializeField] private bool faceCurrentTarget;



    protected virtual void Awake()
    {
        statusEffectOnEnemy = GetComponent<StatusEffectOnEnemy>();
        if (usesParentRigidBodyAndAI)
        {
            myRigidbody = GetComponentInParent<Rigidbody2D>();
        }
        else 
        {
            myRigidbody = GetComponent<Rigidbody2D>();
        }

        
        //spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        originalLocalScale = transform.localScale;

        if (movementAnimation != MovementAnimation.fourDirections)
        {
            targetTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
        // Keep these checks separate: flip-only mode must execute both paths.
        if (movementAnimation == MovementAnimation.fourDirections || movementAnimation == MovementAnimation.flipOnly)
        {
            if (usesParentRigidBodyAndAI)
            {
                enemyAI = GetComponentInParent<EnemyAI>();
            }
            else
            {
                enemyAI = GetComponent<EnemyAI>();
            }
        }
    }

    protected virtual void OnEnable()
    {
        try
        {
            //Observe state changes and update the cached value.
            statusEffectOnEnemy.OnStatusChange += UpdateStatusEffect;

        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Nao encontrou o statusEffect: " + e);
        }
    }

    protected virtual void LateUpdate()
    {
        //Debug.Log("distance: " + Vector3.Distance(enemyAI.GetCurrentTargetTransform().position, transform.position));

        if (hasIdle)
        {
            float distance = Vector3.Distance(enemyAI.GetCurrentTargetTransform().position, transform.position);
            if(distance <= minDist)
            {
                animator.SetTrigger("Idle");
                return;
            }

        }

        if (currentStatusEffect == StatusEffect.Frozen)
        {
            animator.enabled = false;
        }
        else if (movementAnimation == MovementAnimation.flipOnly)
        {
            FlipAnimation();

        }
        else if (movementAnimation == MovementAnimation.flipPlusTurn)
        {
            FlipAnimation();

            TurnAnimation();
        }
        else
        {
            CheckFourDirectionChange();

        }
    }

    protected virtual void TurnAnimation()
    {
        Vector3 direction = (targetTransform.transform.position - myRigidbody.transform.position).normalized;

        if (lastHorizontalDirection > 0 && direction.x < 0) //Check whether the direction changed.
        {
            animator.SetTrigger("Turn");
        }
        else if (direction.x > 0 && lastHorizontalDirection < 0)
        {
            animator.SetTrigger("Turn");
        }

        lastHorizontalDirection = direction.x;
    }

    protected virtual void FlipAnimation()
    {
        //Vector3 direction = (targetTransform.transform.position - myRigidbody.transform.position).normalized;
        if (usesMomentumInFlipAnimation)
        {
            Direction currentAIDirection = CurrentDirectionXOnly(enemyAI.desiredDirection); 
            Direction currentVelocityDirection = CurrentDirectionXOnly(myRigidbody.linearVelocity);

            /*update facing only when the AI direction agrees with velocity; sudden AI direction changes otherwise cause animation jitter.
            Face forward when moving forward, but keep facing the player when pushed backward.
            This delays animation changes slightly and can briefly make the enemy walk backward.*/
            if (currentAIDirection == currentVelocityDirection)
            {
                //Aux.Print("Ai direction: " + currentAIDirection + "aivalues:" + enemyAI.desiredDirection + "|| RigidBodyDirection" + currentVelocityDirection + "RigidBodyValues:" + myRigidbody.velocity,showDebug);
                Flip();
            }
        }
        else 
        {   //If momentum is disabled, only flip the sprite.
            Flip();
        }

    }

    //Extracted for reuse.
    protected virtual void Flip()
    {
        Transform facingTarget = faceCurrentTarget && enemyAI != null ? enemyAI.GetCurrentTargetTransform() : targetTransform;
        if (facingTarget == null || myRigidbody == null) return;
        Vector3 direction = (facingTarget.position - myRigidbody.transform.position).normalized;

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(originalLocalScale.x, originalLocalScale.y, originalLocalScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-originalLocalScale.x, originalLocalScale.y, originalLocalScale.z);
        }
    }


    protected virtual void CheckFourDirectionChange()
    {
        Direction currentAIDirection;
        Direction currentVelocityDirection = CurrentDirection(myRigidbody.linearVelocity);
        if(movementAnimation == MovementAnimation.fourDirections)
        {
            /*update facing only when the AI direction agrees with velocity; sudden AI direction changes otherwise cause animation jitter.
            Face forward when moving forward, but keep facing the player when pushed backward.
            This delays animation changes slightly and can briefly make the enemy walk backward.*/
             currentAIDirection = CurrentDirection(enemyAI.desiredDirection);
            if (currentAIDirection == currentVelocityDirection)
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

    protected virtual void TriggerChangeAnimation(Direction currentAIDirection)
    {
        if (currentAIDirection != lastDirection || triggerAnimationEveryFrame)
        {
            Aux.Print("" + currentAIDirection, showDebug);
            UpdateFourDirectionAnimation(currentAIDirection);
        }

        lastDirection = currentAIDirection;
    }

    protected Direction ConvertVectorToDirection(Vector3 direction)
    {
        if(direction.y >= 0.8f )
        {
            //Debug.Log("Retorma norte");
            return Direction.North;  
        }
        else if (direction.y < 0.8f && direction.y >= -0.8f)
        {
            if (direction.x >=0)
            {
                //Debug.Log("Retorma leste");
                return Direction.East;
            }
            else
            {
                return Direction.West;
            }
        }
        else 
        {
            return Direction.South;
        }

    }

    protected virtual void UpdateFourDirectionAnimation(Direction currentDirection)
    {
        //Reset all animation triggers before activating another.
        //Useful for long animations such as the bomber's.
        //Aux.ResetAllAnimationTriggers(animator);
        Aux.ResetOnlyDirectionalTriggers(animator);

        if (currentDirection == Direction.East)
        {
            animator.SetTrigger("Horizontal");
            transform.localScale = new Vector3(originalLocalScale.x, originalLocalScale.y, originalLocalScale.z);
        }
        else if (currentDirection == Direction.West)
        {
            animator.SetTrigger("Horizontal");
            transform.localScale = new Vector3(-originalLocalScale.x, originalLocalScale.y, originalLocalScale.z);
        }
        else if (currentDirection == Direction.North)
        {
            animator.SetTrigger("Up");
        }
        else if (currentDirection == Direction.South)
        {
            animator.SetTrigger("Down");
        }

    }

    protected Direction CurrentDirection(Vector2 desiredDirection)
    {

       // PrintClass.Print(""+desiredDirection, showDebug);
        if( Mathf.Abs(desiredDirection.x) > Mathf.Abs(desiredDirection.y)) //Horizontal movement.
        {
            if(desiredDirection.x > 0)
            {
                return Direction.East;
            }
            else
            {
                return Direction.West;
            }
        }
        else //Vertical movement.
        {
            if(desiredDirection.y > 0)
            {
                return Direction.North;
            }
            else
            {
                return Direction.South;
            }
        }

        
    }


    protected Direction CurrentDirectionXOnly(Vector2 desiredDirection)
    {

        // PrintClass.Print(""+desiredDirection, showDebug);
        if (desiredDirection.x > 0)
        {
            return Direction.East;
        }
        else
        {
            return Direction.West;
        }

    }


    public void SetTarget(Transform target)
    {
        this.targetTransform = target;
    }

    protected void UpdateStatusEffect()
    {
        currentStatusEffect = statusEffectOnEnemy.GetCurrentStatus();

        if(currentStatusEffect == StatusEffect.Frozen)
        {
            animator.enabled = false;
        }
        else
        {
            animator.enabled = true;
        }
    }
    public Direction GetDirection()
    {
        return lastDirection;
    }


    protected enum MovementAnimation {flipOnly, flipPlusTurn, fourDirections, fourDirectionsFlyier }
}
