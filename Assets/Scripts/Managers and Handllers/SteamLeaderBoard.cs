using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class SteamLeaderBoard : MonoBehaviour
{
    public PlayerData playerData;

    private const string LeaderboardName = "Top Hunters";
    private Steamworks.Data.Leaderboard _lb;

    LeaderboardEntry[] globalScores;
    LeaderboardEntry[] friendScores;
    LeaderboardEntry[] surroundScores;

    private async void Awake()
    {
        Aux.TryToConnectToSteam();
        try
        {
            var lb = await SteamLeaderBoardSystem.GetLeaderBoards(LeaderboardName);
            if (lb.HasValue) _lb = lb.Value;
            else Debug.LogError("leaderboard did not have value");

            // Get top 20 scores
            globalScores = await lb.Value.GetScoresAsync(20);

            foreach (LeaderboardEntry e in globalScores)
            {
                Console.WriteLine($"{e.GlobalRank}: {e.Score} {e.User}");
            }

            // Get scores from friends
            friendScores = await lb.Value.GetScoresFromFriendsAsync();

            foreach (LeaderboardEntry e in friendScores)
            {
                Console.WriteLine($"{e.GlobalRank}: {e.Score} {e.User}");
            }

            // Get scores around current user
            surroundScores = await lb.Value.GetScoresAroundUserAsync(-4, 4);

            foreach (LeaderboardEntry e in surroundScores)
            {
                Console.WriteLine($"{e.GlobalRank}: {e.Score} {e.User}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading leaderboards: {e.Message}");
            // Optionally, you could handle the exception, e.g., retry logic or fallback behavior
        }
    }

    #region TEST

    [ContextMenu("ReplaceScoreTest")]
    private void ReplacePlayerHighestScore()
    {
        var entry = new LeaderboardScore(GetPlayerHighestScore(), 0);
        ReplacePointScore(entry);
    }

    #endregion

    #region ReplaceScores

    private void ReplacePointScore(LeaderboardScore entry)
    {
        _ = SteamLeaderBoardSystem.ReplaceLeaderboard(_lb, entry.Score, entry.Details);
    }

    #endregion

    // Return the highest score in playerData.scores.
    // Return zero when playerData or its score history is missing.
    public int GetPlayerHighestScore()
    {
        if (playerData == null || playerData.scores == null || playerData.scores.Count == 0)
        {
            Debug.LogWarning("Player data is null or has no scores.");
            return 0;
        }

        return playerData.scores.Max(s => s != null ? s.score : int.MinValue);
    }

    public List<Score> GetTopGlobal()
    {
        List<Score> scoreList = new List<Score>();

        foreach (LeaderboardEntry e in globalScores)
        {
            AddScoreToList(scoreList, e);
        }

        return scoreList;
    }

    public List<Score> GetFriendsScore()
    {
        List<Score> scoreList = new List<Score>();

        foreach (LeaderboardEntry e in friendScores)
        {
            AddScoreToList(scoreList, e);
        }

        return scoreList;
    }

    private static void AddScoreToList(List<Score> scoreList, LeaderboardEntry e)
    {
        Score score = new Score(e.Score, e.User.Name, Character.azaus, 0, 0, 0, 0);
        score.globalPos = e.GlobalRank;
        scoreList.Add(score);
    }

    public List<Score> GetSurroundScores()
    {
        List<Score> scoreList = new List<Score>();

        foreach (LeaderboardEntry e in surroundScores)
        {
            AddScoreToList(scoreList, e);
        }

        return scoreList;
    }
}

// Struct to hold data for ease-of-use and re-use-ability
public struct LeaderboardScore
{
    public readonly int Score;
    public readonly int SomeOtherValue;
    public readonly int[] Details;

    public LeaderboardScore(int score, int someOtherValue)
    {
        Score = score;
        SomeOtherValue = someOtherValue;
        Details = new[] { someOtherValue };
    }
}
