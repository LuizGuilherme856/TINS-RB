using UnityEngine;

public class PushEnemiesDebug : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private LayerMask targetLayer;

    [Header("Pulse Settings")]
    [SerializeField] private bool pushOnStart = true;
    [SerializeField] private float pulseInterval = 1f;
    [SerializeField] private float impulseRadius = 2f;
    [SerializeField] private float impulseStrength = 10f;
    [SerializeField] private bool scaleForceByDistance = true;

    private float pulseTimer;

    private void OnEnable()
    {
        pulseTimer = 0f;

        if (pushOnStart)
        {
            TriggerPushPulse_TechBooh();
        }
    }

    private void Update()
    {
        if (pulseInterval <= 0f)
        {
            TriggerPushPulse_TechBooh();
            pulseInterval = 0.01f;
            return;
        }

        pulseTimer += Time.deltaTime;
        if (pulseTimer < pulseInterval)
        {
            return;
        }

        pulseTimer = 0f;
        TriggerPushPulse_TechBooh();
    }

    /// <summary>
    /// TechBooh:
    /// Apply a radial impulse to nearby enemies.
    /// Optionally reduce force with distance to simulate pressure falloff.
    /// </summary>
    public void TriggerPushPulse_TechBooh()
    {
        Vector2 epicenter = transform.position;
        Collider2D[] targetsHit = Physics2D.OverlapCircleAll(epicenter, impulseRadius, targetLayer);

        foreach (Collider2D target in targetsHit)
        {
            if (target == null || !target.CompareTag("Enemy"))
            {
                continue;
            }

            Rigidbody2D targetRb = target.attachedRigidbody != null
                ? target.attachedRigidbody
                : target.GetComponent<Rigidbody2D>();

            if (targetRb == null)
            {
                continue;
            }

            Vector2 impulseDirection = (Vector2)target.transform.position - epicenter;
            if (impulseDirection.sqrMagnitude <= 0.0001f)
            {
                impulseDirection = Random.insideUnitCircle.normalized;
            }
            else
            {
                impulseDirection.Normalize();
            }

            float forceMultiplier = 1f;
            if (scaleForceByDistance)
            {
                float targetDistanceFromEpicenter = Vector2.Distance(epicenter, targetRb.position);
                forceMultiplier = Mathf.Clamp01((impulseRadius - targetDistanceFromEpicenter) / impulseRadius);
            }

            if (forceMultiplier <= 0f)
            {
                continue;
            }

            targetRb.AddForce(impulseDirection * impulseStrength * forceMultiplier, ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, impulseRadius);
    }
}
