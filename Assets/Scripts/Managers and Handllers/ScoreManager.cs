using UnityEngine;

//Persist completed-run scores.

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private RunData runData;
    private StatisticsManager statisticsManager;
    private Health playerHealth;
    private Score newScore;

    private void Awake()
    {
        statisticsManager = GetComponent<StatisticsManager>();
        playerHealth = FindFirstObjectByType<Health>();
    }

    public void NewScore()
    {
        FinalizeCurrentRunScore();
    }

    // Finalize once per RunData attempt, including explicit exits as well as death. Preserve this guard
    // across level/safe-zone transitions so one run cannot produce duplicate leaderboard entries.
    public void FinalizeCurrentRunScore()
    {
        if (runData == null)
        {
            return;
        }

        if (runData.hasFinalizedCurrentRunScore)
        {
            // The previous attempt already ended; allow registration of the next run
            // without adding a duplicate score entry.
            playerData?.ResetRunStartRegistration_TechBooh();
            return;
        }

        if (!HasRunProgressToSave())
        {
            return;
        }

        playerData.AddToTotalKills(runData.enemiesKilled);

        string steamUser = "Nameless Hunter";

        try
        {
            steamUser = Steamworks.SteamClient.Name;
        }
        catch (System.Exception)
        {
            Debug.LogError("Erro ao ler o nome do usario Steam");
        }

        newScore = new Score(
            statisticsManager.GetScore(), steamUser, runData.currentCharacter.character, runData.currentWaveNumber,
            runData.timeElapsed, playerData.GetAttemptNumber(), runData.enemiesKilled);

        SaveNewScore();
        SaveSystem.SaveData(playerData);
        TryUploadFinalizedScoreToLeaderboard_TechBooh();
        runData.MarkCurrentRunScoreAsFinalized();
        playerData.ResetRunStartRegistration_TechBooh();
    }

    public void UpdateName(string newName)
    {
        newScore.playerName = newName;
    }

    public void SaveNewScore()
    {
        playerData.scores.Add(newScore);
    }

    public void BeginNewRunScoreTracking()
    {
        if (runData != null)
        {
            runData.ResetCurrentRunScoreFinalization();
        }

        newScore = null;
    }

    private bool HasRunProgressToSave()
    {
        // TechBooh: wave zero represents the tutorial, Hub, or a run
        // that has not started combat; none should produce leaderboard entries.
        if (runData == null || runData.currentWaveNumber <= 0)
        {
            return false;
        }

        if (playerHealth != null && playerHealth.IsTutorial())
        {
            return false;
        }

        return runData.hasSpawnedGameplayEnemy
            || runData.enemiesKilled > 0
            || runData.pointsInThisRun > 0
            || runData.numberOfPowerUpsCollected > 0;
    }

    private void TryUploadFinalizedScoreToLeaderboard_TechBooh()
    {
        if (newScore == null)
        {
            return;
        }

        // TechBooh:
        // Update the leaderboard only when the run actually ends,
        // not when opening a menu, to avoid partial scores.
        SteamLeaderBoardSystem.UploadFinalizedRunScore_TechBooh(newScore);
    }
}

[System.Serializable]
public class Score
{
    public int score;
    public string playerName;
    public Character character;
    public int wave;
    public int kills;
    public float time;
    public int attempt;
    public int globalPos = -1;

    public Score(int score, string playerName, Character character, int wave, float time, int attempt, int kills)
    {
        this.score = score;
        this.playerName = playerName;
        this.character = character;
        this.wave = wave;
        this.time = time;
        this.attempt = attempt;
        this.kills = kills;
    }
}
