using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Remove enemies after player death to stop further damage and sounds.
/// </summary>
public class KillEnemiesOnPlayerDeath : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;
    private Health playerHealth;


    private void Awake()
    {
        playerHealth = playerHolder.GetPlayerHealth();
    }


    private void OnEnable()
    {
        playerHealth.OnZeroHealth += WaitAndKillEnemies;
        
    }

    private void OnDisable()
    {
        playerHealth.OnZeroHealth -= WaitAndKillEnemies;
    }

    private void WaitAndKillEnemies()
    {
        Invoke(nameof(KillEnemies), 3f);
    }

    private void KillEnemies()
    {
        var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        foreach (EnemyHealth enemy in enemies)
        {
            enemy.gameObject.SetActive(false);
        }

        //Some enemies, including bosses, are nested under manager objects.
        var remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in remainingEnemies) { enemy.gameObject.SetActive(false);}
    }

}
