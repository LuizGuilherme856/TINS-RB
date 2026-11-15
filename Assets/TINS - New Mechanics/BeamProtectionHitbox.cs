using UnityEngine;

/// <summary>Lets the beam break enemy melee protection without applying damage.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class BeamProtectionHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        PlayerMeleeAttackSource source = GetComponent<PlayerMeleeAttackSource>();
        if (enemy == null || source == null || !source.CanBreakEnemyProtection)
            return;

        enemy.RegisterBeamIllumination(source);
    }
}
