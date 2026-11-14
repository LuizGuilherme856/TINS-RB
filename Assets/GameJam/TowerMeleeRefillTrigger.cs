using UnityEngine;

/// <summary>Restores the player's limited melee uses through the Tower's 2D trigger.</summary>
[RequireComponent(typeof(Collider2D))]
public sealed class TowerMeleeRefillTrigger : MonoBehaviour
{
    private Collider2D towerCollider;

    private void Awake()
    {
        towerCollider = GetComponent<Collider2D>();
        towerCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BeamMeleeUsesManager player = other.GetComponentInParent<BeamMeleeUsesManager>();
        if (player != null && player.isActiveAndEnabled)
            player.RefillUses();
    }
}
