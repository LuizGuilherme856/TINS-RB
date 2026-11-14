using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GlobalStats globalStats;
    private Rigidbody2D rb;
    private PlayerData playerData;

    private Vector2 lastVelocity;
    private float campaignSpeedMultiplier = 1f;

    private void Awake()
    {
        playerData = GetComponent<PlayerDataHolder>().GetPlayerData();
        rb = GetComponent<Rigidbody2D>();
        if(globalStats == null)
        {
            globalStats = AssetFinder.GetGlobalStats();
            Debug.LogWarning($"O obj {gameObject.name} nao tem global stats. Procurando manualmente. Por favor" +
                $"coloque pelo inspector pra poupar performance");
        }
    }

    public void Move(Vector2 velocity)
    {

        velocity= velocity.normalized;

        if(velocity.magnitude > Mathf.Epsilon) //check if its moving
        {
            StopOnDirectionChange(velocity);
            rb.AddForce(velocity * playerData.walkSpeed * globalStats.playerWalkSpeedMultiplier * campaignSpeedMultiplier);

        }

        lastVelocity = velocity;
    }

    public void StopImmediately()
    {
        lastVelocity = Vector2.zero;
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void SetCampaignSpeedMultiplier(float multiplier)
    {
        campaignSpeedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    private void FixedUpdate()
    {
        StopOnMinVelocity();
    }

    private void StopOnMinVelocity()
    {
        ///Stop movement on an axis when no input is present
        ///and its velocity is below the minimum threshold.
        if (lastVelocity.x == 0)
        {
            if (Mathf.Abs(rb.linearVelocity.x) < playerData.minVelocity)
            {
                if(rb.bodyType == RigidbodyType2D.Dynamic)
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            }

        }
        if (lastVelocity.y == 0)
        {
            if (Mathf.Abs(rb.linearVelocity.y) < playerData.minVelocity)
            {
                if (rb.bodyType == RigidbodyType2D.Dynamic)
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }

        }

    }

    private void StopOnDirectionChange(Vector2 velocity)
    {
        if (lastVelocity.x > 0 && velocity.x < 0) //If direction changed,
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else if (lastVelocity.x < 0 && velocity.x > 0)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if (lastVelocity.y > 0 && velocity.y < 0) //If direction changed,
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }
        else if (lastVelocity.y < 0 && velocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }
    }

    public bool GetIsMoving()
    {
        if (rb.linearVelocity.magnitude > Mathf.Epsilon) 
        {

            return true;
        }
        return false;

    }
    public Direction GetDirection()
    {
        if (rb.linearVelocity.x > 0)
        {
            return Direction.East;
        }
        else if (rb.linearVelocity.x < 0)
        {
            return Direction.West;
        }
        else if (rb.linearVelocity.y > 0)
        {
            return Direction.North;
        }
        else
        {
            return Direction.South;
        }
    }

}

public enum Direction { North, East, South, West };
