using UnityEngine;

/// <summary>Identifies the owner and activation of a pooled player melee attack.</summary>
public sealed class PlayerMeleeAttackSource : MonoBehaviour
{
    public Health Owner { get; private set; }
    public uint AttackId { get; private set; }
    private PlayerAction playerAction;

    public bool CanBreakEnemyProtection => Owner != null
        && Owner.CompareTag("Player") && playerAction == PlayerAction.melee;

    public void Configure(Health owner, PlayerAction action)
    {
        Owner = owner;
        playerAction = action;
        unchecked { AttackId++; }
    }

    private void OnDisable()
    {
        Owner = null;
    }
}
