using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public bool UpdateAnimation { get;  set; } = true;

    private Animator animator;
    private PlayerMovement playerMovement;
    [SerializeField] private Aim aim;

    private bool wasAimingRight = true;
    //private AnimatorClipInfo[] animatorinfo;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();

        if (aim == null) aim = FindFirstObjectByType<Aim>();
    }

    void Update()
    {
        CheckAnimationState();
    }

    private void CheckAnimationState() //Potential optimization opportunity.
    {
        if (!UpdateAnimation)
        {
            // if(playerMovement.GetIsMoving())
            // {
            //     Debug.LogWarning("Player is moving but UpdateAnimation is false. Animation will not update.");
            // }
            return;
        }


        //animatorinfo = animator.GetCurrentAnimatorClipInfo(0);
        //string currentAnimation = animatorinfo[0].clip.name;
        Direction currentDirection;
        try
        {
            currentDirection = aim.AimDirection();
        }
        catch
        {
            Debug.LogError("Erro ao ler a direção da mira.");
            currentDirection = Direction.West;
        }

        if (playerMovement.GetIsMoving())
        {

            if(currentDirection == Direction.South)
            {
                //animator.Play("player down movement");
                PlayDownMovement();
            }
            else if(currentDirection == Direction.North)
            {
                //animator.Play("player up movement");
                PlayUpMovement();
            }
            else
            {
                //animator.Play("player horizontal movement");
                PlayHorizontalMovement();
            }

        }
        else
        {
            if (currentDirection == Direction.South)
            {
                //animator.Play("Player idle down");
                PlayIdleDown();
            }
            else if (currentDirection == Direction.North)
            {
                //animator.Play("Player idle up");
                PlayIdleUp();
            }
            else
            {
                //animator.Play("Player idle");
                PlayIdle();
            }
        }
    }

    public void SetFlipSprite(int direction)
    {
        Vector3 scale = transform.localScale;
        bool isAimingRight = aim.GetIsAimingRight();

        if (direction == 1 && wasAimingRight == false)
        {
            transform.localScale = new Vector2(Mathf.Abs(scale.x), scale.y);
        }
        else if (direction == -1 && wasAimingRight == true)
        {
            transform.localScale = new Vector2(-Mathf.Abs(scale.x), scale.y);
        }
        wasAimingRight = isAimingRight;
    }

    public bool GetIsFacingRight()
    {
        if (transform.localScale.x > 0) return true;
        else return false;
    }


    public void PlayAnimation(PlayerAnimationName animationName)
    {
        switch (animationName)
        {
            case PlayerAnimationName.PlayerDownMovement:
                animator.Play("player down movement");
                break;
            case PlayerAnimationName.PlayerUpMovement:
                animator.Play("player up movement");
                break;
            case PlayerAnimationName.PlayerHorizontalMovement:
                animator.Play("player horizontal movement");
                break;
            case PlayerAnimationName.PlayerIdleDown:
                animator.Play("Player idle down");
                break;
            case PlayerAnimationName.PlayerIdleUp:
                animator.Play("Player idle up");
                break;
            case PlayerAnimationName.PlayerIdle:
                animator.Play("Player idle");
                break;
        }
    }

    public void PlayDownMovement() => PlayAnimation(PlayerAnimationName.PlayerDownMovement);
    public void PlayUpMovement() => PlayAnimation(PlayerAnimationName.PlayerUpMovement);
    public void PlayHorizontalMovement() => PlayAnimation(PlayerAnimationName.PlayerHorizontalMovement);
    public void PlayIdleDown() => PlayAnimation(PlayerAnimationName.PlayerIdleDown);
    public void PlayIdleUp() => PlayAnimation(PlayerAnimationName.PlayerIdleUp);
    public void PlayIdle() => PlayAnimation(PlayerAnimationName.PlayerIdle);

}

public enum PlayerAnimationName
{
    PlayerDownMovement,         // "player down movement"
    PlayerUpMovement,           // "player up movement"
    PlayerHorizontalMovement,   // "player horizontal movement"
    PlayerIdleDown,             // "Player idle down"
    PlayerIdleUp,               // "Player idle up"
    PlayerIdle                  // "Player idle"
}