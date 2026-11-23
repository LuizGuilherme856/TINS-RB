using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GlobalStats globalStats;

    [Header("Obstacle Contact")]
    [SerializeField, Min(0f)] private float cornerCorrectionDistance = 0.15f;
    [SerializeField] private AudioClip obstacleImpactSound;
    [SerializeField] private AudioMixerGroup obstacleImpactMixer;
    [SerializeField, Range(0f, 1f)] private float obstacleImpactVolume = 0.2f;
    [SerializeField, Min(0f)] private float obstacleImpactMinSpeed = 2.5f;

    private const float CornerForwardClearance = 0.18f;
    private const int CornerSamples = 3;
    private const float ImpactCooldown = 0.24f;
    private const float ImpactTintDuration = 0.08f;

    private Rigidbody2D rb;
    private PlayerData playerData;
    private SpriteRenderer bodySprite;
    private AudioSource obstacleAudio;
    private ContactFilter2D obstacleFilter;
    private LayerMask cornerLayers;
    private readonly List<ContactPoint2D> obstacleContacts = new List<ContactPoint2D>(8);
    private readonly List<RaycastHit2D> cornerHits = new List<RaycastHit2D>(8);

    private Vector2 lastInput;
    private Vector2 lastWalkDirection;
    private float campaignSpeedMultiplier = 1f;
    private float lastMoveTime;
    private float nextImpactTime;
    private float impactTintEndTime;
    private bool impactTintActive;
    private Color spriteColorBeforeImpact;
    private Color impactTint;

    private void Awake()
    {
        playerData = GetComponent<PlayerDataHolder>().GetPlayerData();
        rb = GetComponent<Rigidbody2D>();
        bodySprite = GetComponent<SpriteRenderer>();
        obstacleFilter = new ContactFilter2D { useTriggers = false };
        obstacleFilter.SetLayerMask(LayerMask.GetMask("Obstacle", "ClosedDoor", "cliff Obstacle", "Table"));
        cornerLayers = LayerMask.GetMask("Obstacle", "Table");
        if(globalStats == null)
        {
            globalStats = AssetFinder.GetGlobalStats();
            Debug.LogWarning($"O obj {gameObject.name} nao tem global stats. Procurando manualmente. Por favor" +
                $"coloque pelo inspector pra poupar performance");
        }
    }

    public void Move(Vector2 velocity)
    {
        Vector2 input = velocity.normalized;
        Vector2 walkDirection = input;
        if (input != Vector2.zero)
        {
            StopOnDirectionChange(input);
            walkDirection = GetWalkDirection(input);
            rb.AddForce(walkDirection * (playerData.walkSpeed * globalStats.playerWalkSpeedMultiplier * campaignSpeedMultiplier));
        }

        lastInput = input;
        lastWalkDirection = walkDirection;
        lastMoveTime = Time.fixedTime;
    }

    private Vector2 GetWalkDirection(Vector2 input)
    {
        if (rb.bodyType != RigidbodyType2D.Dynamic || rb.GetContacts(obstacleFilter, obstacleContacts) == 0)
            return input;

        Vector2 slide = ProjectAlongObstacles(input);
        if (slide.sqrMagnitude > 0.12f || cornerCorrectionDistance <= 0f)
            return slide;

        // Corner correction is only for a single prop. Multiple contacts can mean a narrow gap.
        Collider2D obstacle = obstacleContacts[0].collider;
        if ((cornerLayers.value & (1 << obstacle.gameObject.layer)) == 0)
            return slide;

        foreach (ContactPoint2D contact in obstacleContacts)
        {
            if (contact.collider != obstacle)
                return slide;
        }

        ContactPoint2D face = obstacleContacts[0];
        if (Vector2.Dot(input, face.normal) > -0.9f)
            return slide;

        Vector2 side = Vector2.Perpendicular(input);
        if (Vector2.Dot(slide, side) < -0.001f)
            side = -side;

        // Start the casts just outside the surface. The body still moves by force in the simulation.
        Vector2 castOrigin = rb.position + face.normal * (Mathf.Max(0f, -face.separation) + 0.005f);
        for (int sample = 1; sample <= CornerSamples; sample++)
        {
            float distance = cornerCorrectionDistance * sample / CornerSamples;
            if (CanRoundCorner(castOrigin, input, side, distance))
                return ProjectAlongObstacles(side * 0.6f);
            if (CanRoundCorner(castOrigin, input, -side, distance))
                return ProjectAlongObstacles(-side * 0.6f);
        }

        return slide;
    }

    private Vector2 ProjectAlongObstacles(Vector2 direction)
    {
        // A second pass also clips motion redirected by a neighbouring surface.
        for (int pass = 0; pass < 2; pass++)
        {
            foreach (ContactPoint2D contact in obstacleContacts)
            {
                float intoWall = Vector2.Dot(direction, contact.normal);
                if (intoWall < 0f)
                    direction -= contact.normal * intoWall;
            }
        }

        foreach (ContactPoint2D contact in obstacleContacts)
        {
            if (Vector2.Dot(direction, contact.normal) < -0.001f)
                return Vector2.zero;
        }

        return direction;
    }

    private bool CanRoundCorner(Vector2 origin, Vector2 forward, Vector2 side, float distance)
    {
        return HasClearPath(origin, side, distance) &&
            HasClearPath(origin + side * distance, forward, CornerForwardClearance);
    }

    private bool HasClearPath(Vector2 origin, Vector2 direction, float distance)
    {
        rb.Cast(origin, rb.rotation, direction, cornerHits, distance);
        foreach (RaycastHit2D hit in cornerHits)
        {
            if (!hit.collider.isTrigger)
                return false;
        }

        return true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (lastInput == Vector2.zero || Time.fixedTime - lastMoveTime > Time.fixedDeltaTime ||
            Time.time < nextImpactTime ||
            obstacleFilter.IsFilteringLayerMask(collision.collider.gameObject))
            return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 normal = collision.GetContact(i).normal;
            if (Vector2.Dot(lastInput, normal) > -0.65f ||
                Mathf.Abs(Vector2.Dot(collision.relativeVelocity, normal)) < obstacleImpactMinSpeed)
                continue;

            nextImpactTime = Time.time + ImpactCooldown;
            PlayObstacleImpactSound();

            if (bodySprite != null)
            {
                spriteColorBeforeImpact = bodySprite.color;
                impactTint = spriteColorBeforeImpact * new Color(1f, 0.78f, 0.66f, 1f);
                bodySprite.color = impactTint;
                impactTintEndTime = Time.time + ImpactTintDuration;
                impactTintActive = true;
            }
            break;
        }
    }

    private void PlayObstacleImpactSound()
    {
        if (obstacleImpactSound == null || obstacleImpactVolume <= 0f)
            return;

        if (obstacleAudio == null)
        {
            obstacleAudio = gameObject.AddComponent<AudioSource>();
            obstacleAudio.playOnAwake = false;
            obstacleAudio.outputAudioMixerGroup = obstacleImpactMixer;
        }

        obstacleAudio.PlayOneShot(obstacleImpactSound, obstacleImpactVolume);
    }

    private void LateUpdate()
    {
        if (impactTintActive && Time.time >= impactTintEndTime)
            ClearImpactTint();
    }

    private void OnDisable()
    {
        lastInput = Vector2.zero;
        lastWalkDirection = Vector2.zero;
        ClearImpactTint();
    }

    private void ClearImpactTint()
    {
        if (impactTintActive && bodySprite != null && bodySprite.color == impactTint)
            bodySprite.color = spriteColorBeforeImpact;
        impactTintActive = false;
    }

    public void StopImmediately()
    {
        lastInput = Vector2.zero;
        lastWalkDirection = Vector2.zero;
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
        if (rb.bodyType != RigidbodyType2D.Dynamic)
            return;

        // Assisted movement can use an axis the player is not pressing.
        Vector2 velocity = rb.linearVelocity;
        Vector2 limitedVelocity = velocity;
        if (lastInput.x == 0f && Mathf.Abs(lastWalkDirection.x) < 0.001f && Mathf.Abs(velocity.x) < playerData.minVelocity)
            limitedVelocity.x = 0f;
        if (lastInput.y == 0f && Mathf.Abs(lastWalkDirection.y) < 0.001f && Mathf.Abs(velocity.y) < playerData.minVelocity)
            limitedVelocity.y = 0f;
        if (!limitedVelocity.Equals(velocity))
            rb.linearVelocity = limitedVelocity;
    }

    private void StopOnDirectionChange(Vector2 velocity)
    {
        if (lastInput.x > 0 && velocity.x < 0)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else if (lastInput.x < 0 && velocity.x > 0)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if (lastInput.y > 0 && velocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }
        else if (lastInput.y < 0 && velocity.y > 0)
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
