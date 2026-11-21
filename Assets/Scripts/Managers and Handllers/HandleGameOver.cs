using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Opens the death and statistics screens.
/// </summary>
public class HandleGameOver : MonoBehaviour
{

    [SerializeField] private MMF_Player endScreenFeedback, statisticsScreenFeedback;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerHolder playerHolder;

    [field: SerializeField] public float statisticsScreenDelay { get; private set; }
    private ScoreManager scoreManager;
    private DeathStatistics deathStatistics;

    private bool hasPlayedDeathScreen = false;
    private Health playerHealth;
    private void Awake()
    {
        deathStatistics = FindFirstObjectByType<DeathStatistics>();
        scoreManager = GetComponent<ScoreManager>();

        playerHealth = playerHolder.GetPlayerHealth();
    }
    private static void DestroyAllEnemies() //TODO: Confirm whether this helper is still needed.
    {
        Debug.Log("Destroy all enemies");
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemies)
        {
            Destroy(enemy);
        }
    }

    private void OnEnable()
    {
        hasPlayedDeathScreen = false;
        playerHealth.OnZeroHealth += OnPlayerDeath;
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.OnZeroHealth -= OnPlayerDeath;
    }

    private void OnPlayerDeath()
    {
        // By the time the player dies, the campaign has taken ownership of the result.
        if (TINS.Campaign.CampaignCombatRules.IsCampaignMission) return;
        PlayEndScreen();
        playerData.AddDeath();
        scoreManager.NewScore();
    }

    public void PlayEndScreen()
    {
        hasPlayedDeathScreen = true;
        Invoke(nameof(PlayStatisticsScreen), statisticsScreenDelay);
        endScreenFeedback.PlayFeedbacks();
    }

    private void PlayStatisticsScreen()
    {
        statisticsScreenFeedback.PlayFeedbacks();
        try
        {
            deathStatistics.RefreshStatistics();

        }
        catch (System.Exception e)
        {
            Debug.Log("erro ao refresh statistics: \n" + e);
        }
    }

/// <summary>
/// Fallback to display the death screen if the normal zero-health event is missed.
/// Check player health once per second.
/// </summary>
/// <returns></returns>
    private IEnumerator DoubleCheckPlayerHealth()
    {
        while (true)
        {
            if (playerHealth.GetCurrentHealth() <= 0)//If health is zero,
            {
                //Wait one second.
                yield return new WaitForSeconds(1);
                //retrigger death if the death screen has not started.
                if (hasPlayedDeathScreen == false) playerHealth.InstaKill();
            }

            yield return new WaitForSeconds(1);
        }

    }

}
