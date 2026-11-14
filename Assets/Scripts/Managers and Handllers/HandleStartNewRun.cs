using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reset upgrades and health when a new run begins.
/// </summary>
public class HandleStartNewRun : MonoBehaviour
{
    [SerializeField] private RuneInventory runeInventory;
    private Health playerHealth;
    private RunDataManager runDataManager;
    private CharacterDataManager characterDataManager;
    private SkillManager skillManager;

    [SerializeField] private GlobalStats globalStats;
    [SerializeField] private bool callOnStart = false;
    [SerializeField] private bool isOnMenu = false;
    [SerializeField] private PlayerData playerData;

    private void Awake()
    {
        playerHealth = FindFirstObjectByType<Health>();
        skillManager = GetComponent<SkillManager>();

        runDataManager = FindFirstObjectByType<RunDataManager>();
        characterDataManager = FindFirstObjectByType<CharacterDataManager>();
    }

    private void Start()
    {
        playerHealth = FindFirstObjectByType<Health>();

        if (playerHealth == null)
        {
            Debug.LogWarning("nao achou o playerHealth");
            return;
        }

        if (playerHealth.GetCurrentHealth() <= 0 || callOnStart)
        {
            StartNewRun();
        }
    }

    // This boundary finishes the previous attempt before resetting temporary state. Tutorial completion
    // uses ResetAfterTutorial_TechBooh so it neither submits a score nor consumes a gameplay attempt.
    public void StartNewRun()
    {
        StartNewRunInternal_TechBooh(true, true);
    }

    /// <summary>
    /// TechBooh: used only after completing the tutorial. Discard its temporary data;
    /// entering the Hub must not consume a gameplay attempt.
    /// </summary>
    public void ResetAfterTutorial_TechBooh()
    {
        StartNewRunInternal_TechBooh(false, false);
    }

    private void StartNewRunInternal_TechBooh(bool finalizePreviousRun, bool registerRunStart)
    {
        if (finalizePreviousRun)
        {
            TryFinalizeCurrentRunScore();
        }
        else
        {
            playerData.ResetRunStartRegistration_TechBooh();
        }

        if (registerRunStart)
        {
            playerData.RegisterRunStart_TechBooh();
        }
        globalStats.ResetValues();

        try
        {
            runeInventory.ResetUpgrades();
        }
        catch (System.Exception e)
        {
            Debug.LogError("erro no runeUpgradeManager: \n" + e);
            throw;
        }

        if (!isOnMenu)
        {
            skillManager.ExecuteSkillUpgrades();
        }

        try
        {
            runDataManager.ResetRunData();
        }
        catch (System.Exception e)
        {
            Debug.LogError("erro no runDataManager: \n" + e);
            throw;
        }

        try
        {
            playerHealth.ResetHealth();
        }
        catch (System.Exception e)
        {
            Debug.LogError("erro no playerHealth.ResetHealth();: \n" + e);
            throw;
        }

        TryBeginNewRunScoreTracking();

        if (globalStats.numberOfRandomRunesToStartWith > 0)
        {
            for (int i = 0; i < globalStats.numberOfRandomRunesToStartWith; i++)
            {
                runeInventory.AddUpgrade(globalStats.GetRandomRune());
            }
        }
    }

    private void TryFinalizeCurrentRunScore()
    {
        var scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.FinalizeCurrentRunScore();
        }
    }

    private void TryBeginNewRunScoreTracking()
    {
        var scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.BeginNewRunScoreTracking();
        }
    }
}
