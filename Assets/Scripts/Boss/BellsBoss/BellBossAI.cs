using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellBossAI : EnemyAI
{
    [SerializeField] protected int currentLocalForm = 0;
    //Boss Scripts Related
    [SerializeField] protected BossEnemy bossScript;
    [SerializeField] protected VariaShootingManager variaShootingManager;

    [Header("TechBooh - Bell Unstuck")]
    [SerializeField] protected float unstuckCheckRadius_TechBooh = 0.75f;
    [SerializeField] protected float unstuckRepositionDistance_TechBooh = 1.75f;
    [SerializeField] protected float unstuckBoundsPadding_TechBooh = 0.15f;
    [SerializeField] protected float confinedMovementRadius_TechBooh = 1.25f;
    [SerializeField] protected float maxConfinedMovementTime_TechBooh = 4f;
    [SerializeField] protected float arenaRecoveryCooldown_TechBooh = 3f;
    [SerializeField] protected bool forceReturnToArenaCenterOnRepeatedStuck_TechBooh = true;

    protected Vector2 originalArenaPosition_TechBooh;

    private Collider2D[] solidBellColliders_TechBooh;
    private Vector2 confinedMovementAnchor_TechBooh;
    private float confinedMovementTimer_TechBooh;
    private float arenaRecoveryCooldownTimer_TechBooh;

    protected override void Awake()
    {
        base.Awake();

        solidBellColliders_TechBooh = GetComponentsInChildren<Collider2D>(true);
        originalArenaPosition_TechBooh = transform.position;
        ResetConfinedMovementWatchdog_TechBooh();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        variaShootingManager.SetAutoFireState(false);
        arenaRecoveryCooldownTimer_TechBooh = 0f;
        ResetConfinedMovementWatchdog_TechBooh();
    }

    /// <summary>
    /// TechBooh: stores the arena center used by the final anti-stuck fallback.
    /// RingLocation is already assigned in the Cathedral scene for all three bells.
    /// </summary>
    protected void ConfigureArenaRecoveryPosition_TechBooh(Transform arenaCenter)
    {
        originalArenaPosition_TechBooh = arenaCenter != null
            ? arenaCenter.position
            : transform.position;
    }


    /// <summary>
    /// Select the spawner for the current boss phase.
    /// </summary>
    public void ToogleRingAttack_Animation()
    {
        switch (currentLocalForm)
        {
            case 0:
                StartCoroutine(variaShootingManager.ForceVariaShoot_Routine());
                break;
            case 1:
                StartCoroutine(variaShootingManager.ForceVariaShoot_Routine());
                break;
            case 2:
                StartCoroutine(variaShootingManager.ForceVariaShoot_Routine());
                break;
            default:
                StartCoroutine(variaShootingManager.ForceVariaShoot_Routine());
                break;
        }
    }

    /// <summary>
    /// TechBooh:
    /// Checks whether a bell can occupy a point without additional Editor setup.
    /// Ignore triggers and the bell's own colliders; solid pillars and walls determine clearance.
    /// </summary>
    protected bool IsPositionFree_TechBooh(Vector2 candidatePosition)
    {
        Vector2 positionDelta = candidatePosition - (Vector2)transform.position;
        bool checkedBellCollider = false;

        if (solidBellColliders_TechBooh != null)
        {
            for (int bellColliderIndex = 0; bellColliderIndex < solidBellColliders_TechBooh.Length; bellColliderIndex++)
            {
                Collider2D bellCollider = solidBellColliders_TechBooh[bellColliderIndex];
                if (bellCollider == null || !bellCollider.enabled || bellCollider.isTrigger ||
                    bellCollider.attachedRigidbody != myRigidbody)
                {
                    continue;
                }

                checkedBellCollider = true;
                Bounds bounds = bellCollider.bounds;
                Vector2 queryCenter = (Vector2)bounds.center + positionDelta;
                Vector2 querySize = (Vector2)bounds.size + Vector2.one * (unstuckBoundsPadding_TechBooh * 2f);
                Collider2D[] overlaps = Physics2D.OverlapBoxAll(queryCenter, querySize, 0f);

                if (ContainsBlockingCollider_TechBooh(overlaps))
                {
                    return false;
                }
            }
        }

        if (checkedBellCollider)
        {
            return true;
        }

        // Keep the old radius check as a fallback for an incorrectly configured prefab.
        return !ContainsBlockingCollider_TechBooh(
            Physics2D.OverlapCircleAll(candidatePosition, unstuckCheckRadius_TechBooh));
    }

    private bool ContainsBlockingCollider_TechBooh(Collider2D[] colliders)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            if (collider == null || collider.isTrigger)
            {
                continue;
            }

            if (collider.transform == transform || collider.transform.IsChildOf(transform) || transform.IsChildOf(collider.transform))
            {
                continue;
            }

            if (collider.attachedRigidbody != null && collider.attachedRigidbody == myRigidbody)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// TechBooh:
    /// Quickly searches for a safe point around the current origin.
    /// Prevents bells from remaining stuck in pillars after teleporting, lunging, or spinning.
    /// </summary>
    protected Vector2 FindNearbySafePosition_TechBooh(Vector2 originPosition, Vector2 preferredDirection)
    {
        if (preferredDirection.sqrMagnitude <= 0.001f)
        {
            preferredDirection = Vector2.right;
        }

        preferredDirection.Normalize();

        Vector2[] directions =
        {
            preferredDirection,
            (Vector2)(Quaternion.Euler(0f, 0f, 45f) * preferredDirection),
            (Vector2)(Quaternion.Euler(0f, 0f, -45f) * preferredDirection),
            (Vector2)(Quaternion.Euler(0f, 0f, 90f) * preferredDirection),
            (Vector2)(Quaternion.Euler(0f, 0f, -90f) * preferredDirection),
            -preferredDirection,
            (Vector2)(Quaternion.Euler(0f, 0f, 135f) * preferredDirection),
            (Vector2)(Quaternion.Euler(0f, 0f, -135f) * preferredDirection)
        };

        float[] distances =
        {
            unstuckRepositionDistance_TechBooh,
            unstuckRepositionDistance_TechBooh * 1.5f,
            unstuckRepositionDistance_TechBooh * 2f
        };

        for (int distanceIndex = 0; distanceIndex < distances.Length; distanceIndex++)
        {
            for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
            {
                Vector2 candidatePosition = originPosition + (directions[directionIndex] * distances[distanceIndex]);
                if (IsPositionFree_TechBooh(candidatePosition))
                {
                    return candidatePosition;
                }
            }
        }

        return originPosition;
    }

    /// <summary>
    /// TechBooh:
    /// Centralizes repositioning so subclasses can request
    /// consistent recovery when a bell gets stuck in arena geometry.
    /// </summary>
    protected void RepositionBellSafely_TechBooh(Vector2 targetPosition)
    {
        myRigidbody.linearVelocity = Vector2.zero;
        myRigidbody.angularVelocity = 0f;
        myRigidbody.position = targetPosition;
        transform.position = targetPosition;
        Physics2D.SyncTransforms();
        ResetConfinedMovementWatchdog_TechBooh();
        UpdatePath();
    }

    /// <summary>
    /// TechBooh: detects the case where a bell still moves a little but remains confined
    /// around the same pillar. The previous displacement-only check considered that movement
    /// valid forever, so this is the final watchdog shared by all three bells.
    /// </summary>
    protected bool RecoverIfConfinedNearObstacle_TechBooh(bool shouldBeMoving)
    {
        if (!forceReturnToArenaCenterOnRepeatedStuck_TechBooh)
        {
            ResetConfinedMovementWatchdog_TechBooh();
            return false;
        }

        if (arenaRecoveryCooldownTimer_TechBooh > 0f)
        {
            arenaRecoveryCooldownTimer_TechBooh -= Time.fixedDeltaTime;
        }

        if (!shouldBeMoving || target == null ||
            currentStatusEffect == StatusEffect.Frozen ||
            currentStatusEffect == StatusEffect.Electrocuted)
        {
            ResetConfinedMovementWatchdog_TechBooh();
            return false;
        }

        if (arenaRecoveryCooldownTimer_TechBooh > 0f)
        {
            ResetConfinedMovementWatchdog_TechBooh();
            return false;
        }

        Vector2 currentPosition = transform.position;
        if (Vector2.Distance(confinedMovementAnchor_TechBooh, currentPosition) > confinedMovementRadius_TechBooh)
        {
            confinedMovementAnchor_TechBooh = currentPosition;
            confinedMovementTimer_TechBooh = 0f;
            return false;
        }

        confinedMovementTimer_TechBooh += Time.fixedDeltaTime;
        if (confinedMovementTimer_TechBooh < maxConfinedMovementTime_TechBooh)
        {
            return false;
        }

        Vector2 recoveryPosition = GetArenaRecoveryPosition_TechBooh();
        RepositionBellSafely_TechBooh(recoveryPosition);
        arenaRecoveryCooldownTimer_TechBooh = arenaRecoveryCooldown_TechBooh;
        return true;
    }

    /// <summary>
    /// TechBooh: prefers the exact arena center, but searches around it when another bell
    /// currently occupies that point. This prevents one recovery from overlapping two bosses.
    /// </summary>
    protected Vector2 GetArenaRecoveryPosition_TechBooh()
    {
        if (IsPositionFree_TechBooh(originalArenaPosition_TechBooh))
        {
            return originalArenaPosition_TechBooh;
        }

        Vector2 preferredDirection = originalArenaPosition_TechBooh - (Vector2)transform.position;
        Vector2 nearbyPosition = FindNearbySafePosition_TechBooh(originalArenaPosition_TechBooh, preferredDirection);
        if (IsPositionFree_TechBooh(nearbyPosition))
        {
            return nearbyPosition;
        }

        // The center is authored as a clear point in the arena. Use it as the last resort
        // instead of allowing a critical boss encounter to remain permanently stuck.
        return originalArenaPosition_TechBooh;
    }

    protected void ResetConfinedMovementWatchdog_TechBooh()
    {
        confinedMovementAnchor_TechBooh = transform.position;
        confinedMovementTimer_TechBooh = 0f;
    }
}
