using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraveyardShift.Gameplay;

/// <summary>Transfers carried ether to the defense core while the player holds Interact nearby.</summary>
[RequireComponent(typeof(TowerController))]
public sealed class TowerCrystalDeposit : MonoBehaviour
{
    [SerializeField, Min(0f)] private float radiusPerCrystal = 0.75f;
    [SerializeField, Min(0.01f)] private float crystalTravelDuration = 0.34f;
    [SerializeField, Min(0f)] private float accelerationPerDeliveredCrystal = 0.08f;
    [SerializeField, Min(0f)] private float interactionRadius = 2.2f;
    [SerializeField, Min(0f)] private float pulseDuration = 0.25f;
    [SerializeField, Min(0f)] private float pulseAmplitude = 0.06f;
    [SerializeField] private PurificationReveal purificationReveal;
    [SerializeField, Min(0f)] private float knockbackImpulsePerCrystal = 2f;
    [SerializeField, Min(0f)] private float knockbackMovementLockDuration = 0.25f;

    private TowerController tower;
    private PlayerInputHandler nearbyPlayer;
    private BlueCrystalInventory nearbyInventory;
    private bool depositSequenceRunning;
    private Vector3 baseScale;

    public bool IsDepositing => depositSequenceRunning;

    private void Awake()
    {
        tower = GetComponent<TowerController>();
        if (purificationReveal == null)
            purificationReveal = FindFirstObjectByType<PurificationReveal>();
        if (purificationReveal != null)
            purificationReveal.transform.position = transform.position;
        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (nearbyPlayer == null || !nearbyPlayer.isActiveAndEnabled || nearbyPlayer.GetIsClone())
            FindNearbyPlayer();

        if (!depositSequenceRunning && nearbyPlayer != null && nearbyInventory != null &&
            nearbyInventory.Count > 0 && nearbyPlayer.IsInteractHeld && IsPlayerCloseEnough())
            StartCoroutine(DepositWhileHeld());
    }

    private void FindNearbyPlayer()
    {
        nearbyPlayer = null;
        nearbyInventory = null;
        PlayerInputHandler[] players = FindObjectsByType<PlayerInputHandler>(FindObjectsSortMode.None);
        float bestDistance = interactionRadius;
        foreach (PlayerInputHandler candidate in players)
        {
            if (candidate == null || !candidate.isActiveAndEnabled || candidate.GetIsClone()) continue;
            float distance = Vector2.Distance(candidate.transform.position, transform.position);
            if (distance > bestDistance) continue;
            BlueCrystalInventory inventory = candidate.GetComponent<BlueCrystalInventory>();
            if (inventory == null) inventory = candidate.GetComponentInChildren<BlueCrystalInventory>();
            if (inventory == null) continue;
            bestDistance = distance;
            nearbyPlayer = candidate;
            nearbyInventory = inventory;
        }
    }

    private bool IsPlayerCloseEnough() => nearbyPlayer != null &&
        Vector2.Distance(nearbyPlayer.transform.position, transform.position) <= interactionRadius;

    private IEnumerator DepositWhileHeld()
    {
        depositSequenceRunning = true;
        int delivered = 0;
        while (nearbyPlayer != null && nearbyPlayer.IsInteractHeld && IsPlayerCloseEnough() && nearbyInventory != null && nearbyInventory.Count > 0)
        {
            int carriedBefore = nearbyInventory.Count;
            float duration = crystalTravelDuration / (1f + delivered * accelerationPerDeliveredCrystal);
            duration = Mathf.Max(0.055f, duration);
            Vector3 targetPosition = purificationReveal != null ? purificationReveal.transform.position : transform.position;
            yield return nearbyInventory.AbsorbOneAt(targetPosition, duration);
            if (nearbyInventory.Count >= carriedBefore) break;

            delivered++;
            TowerAudioFeedbacks.Instance?.PlayCrystalDepositPitch(Mathf.Min(1.8f, 0.86f + delivered * 0.06f));
            if (purificationReveal != null)
            {
                float oldRadius = purificationReveal.Radius;
                float newRadius = Mathf.Min(purificationReveal.WinRadius, oldRadius + radiusPerCrystal);
                yield return AnimateExpansion(oldRadius, newRadius);
                PushEnemiesAway(newRadius, 1);
            }
        }

        depositSequenceRunning = false;
    }

    private IEnumerator AnimateExpansion(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, pulseDuration));
            purificationReveal.Radius = Mathf.Lerp(from, to, t);
            float pulse = 1f + Mathf.Sin(t * Mathf.PI) * pulseAmplitude;
            transform.localScale = baseScale * pulse;
            yield return null;
        }
        purificationReveal.Radius = to;
        transform.localScale = baseScale;
    }

    private void PushEnemiesAway(float radius, int deliveredCrystals)
    {
        if (radius <= 0f || deliveredCrystals <= 0 || knockbackImpulsePerCrystal <= 0f) return;
        Vector2 center = purificationReveal != null ? purificationReveal.transform.position : transform.position;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, radius);
        HashSet<EnemyAI> affected = new HashSet<EnemyAI>();
        foreach (Collider2D hit in colliders)
        {
            if (hit == null) continue;
            EnemyAI enemy = hit.GetComponentInParent<EnemyAI>();
            if (enemy == null || !affected.Add(enemy)) continue;
            Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
            if (body == null) body = hit.attachedRigidbody;
            if (body == null) continue;
            Vector2 direction = body.worldCenterOfMass - center;
            if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up;
            else direction.Normalize();
            enemy.ApplyExternalKnockback(direction * knockbackImpulsePerCrystal * deliveredCrystals, knockbackMovementLockDuration);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
