using UnityEngine;

/// <summary>Drops configurable crystals only when the enemy dies outside the tower's light.</summary>
public sealed class CrystalDropOnEnemyDeath : MonoBehaviour
{
    [SerializeField, Min(0)] private int crystalDropCount = 1;
    [SerializeField, Min(0f)] private float dropChance = 1f;

    private void OnValidate()
    {
        crystalDropCount = Mathf.Max(0, crystalDropCount);
        dropChance = Mathf.Clamp01(dropChance);
    }

    public void DropIfOutsideLight()
    {
        var reveal = FindFirstObjectByType<PurificationReveal>();
        if (reveal == null) return;

        Vector2 lightCenter = reveal.transform.position;
        float radius = reveal.Radius;
        if (((Vector2)transform.position - lightCenter).sqrMagnitude <= radius * radius) return;
        if (Random.value > dropChance) return;

        for (int i = 0; i < crystalDropCount; i++)
            BlueCrystalPickup.Spawn(transform.position);
    }
}
