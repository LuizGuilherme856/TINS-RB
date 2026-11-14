using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayerOnContact : MonoBehaviour
{
    [SerializeField] private List<Transform> teleportPositions = new List<Transform>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (teleportPositions == null || teleportPositions.Count == 0)
        {
            Debug.LogWarning("TeleportPlayerOnContact: nenhuma posição de teleport configurada.");
            return;
        }

        var playerPos = collision.gameObject.transform.position;
        Transform nearest = null;
        float minSqrDist = float.MaxValue;

        foreach (var t in teleportPositions)
        {
            if (t == null) continue;
            float sqrDist = (t.position - playerPos).sqrMagnitude;
            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                nearest = t;
            }
        }

        if (nearest != null)
        {
            collision.gameObject.transform.position = nearest.position;
        }
    }
}
