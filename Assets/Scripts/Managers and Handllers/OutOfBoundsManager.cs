using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsManager : MonoBehaviour
{
    [SerializeField] private Transform playerRespawnPosition;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = playerRespawnPosition.position;
        }
        else if (other.CompareTag("Enemy"))
        {
            var enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {

                // Fallback: kill the enemy.
                enemyHealth.FallingDeath();
                
            }
        }
    }
}