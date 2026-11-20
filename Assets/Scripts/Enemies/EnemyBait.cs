using System.Collections.Generic;
using MoreMountains.Feedbacks;
using TINS.Campaign;
using UnityEngine;

public class EnemyBait : MonoBehaviour
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private MMF_Player spawnFeedbacks, destroyFeedbacks;
    private CircleCollider2D attraction;
    private readonly Dictionary<ChangeTarget, Transform> previousTargets = new();

    private void Awake() => attraction = GetComponent<CircleCollider2D>();

    private void OnEnable()
    {
        if (attraction != null) attraction.enabled = true;
        spawnFeedbacks?.PlayFeedbacks();
        Invoke(nameof(Expire), duration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHealth health = collision.GetComponentInParent<EnemyHealth>();
        if (health == null || health.currentHealth <= 0f) return;
        ChangeTarget enemy = health.GetComponent<ChangeTarget>();
        if (enemy == null) return;

        Transform previous = enemy.CurrentTarget;
        if (previous != null && previous.TryGetComponent(out EnemyBait _)) return;
        previousTargets[enemy] = previous;
        enemy.SetNewTarget(transform);
    }

    private void Expire()
    {
        RestoreTargets();
        if (attraction != null) attraction.enabled = false;
        destroyFeedbacks?.PlayFeedbacks();
        Destroy(gameObject, 6f);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Expire));
        RestoreTargets();
    }

    private void RestoreTargets()
    {
        foreach (var entry in previousTargets)
        {
            ChangeTarget enemy = entry.Key;
            if (enemy == null || enemy.CurrentTarget != transform) continue;
            Transform previous = entry.Value;
            if (previous == null) previous = CampaignMissionRuntime.Active?.DefenseCore;
            if (previous != null) enemy.SetNewTarget(previous);
            else enemy.SetPlayerAsTarget();
        }
        previousTargets.Clear();
    }
}
