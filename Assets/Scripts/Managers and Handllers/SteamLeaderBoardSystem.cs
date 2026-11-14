using System;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

// Steam-backed leaderboard boundary. Keep completed-run submission separate from menu refreshes
// when replacing the service or defining offline behavior on another platform.
public static class SteamLeaderBoardSystem
{
    public const string DefaultLeaderboardName_TechBooh = "Top Hunters";

    /// <summary>
    ///     This function will only replace your last score if the new one is better.
    /// </summary>
    public static async Task SubmitLeaderboard(Steamworks.Data.Leaderboard leaderboard, int value,
        int[] details = null)
    {
        var leaderboardUpdate = await leaderboard.SubmitScoreAsync(value, details ?? Array.Empty<int>());
        if (!leaderboardUpdate.HasValue)
        {
            Debug.LogError("leaderboardUpdate is null");
            return;
        }

        Debug.Log(leaderboardUpdate.Value);
    }

    /// <summary>
    ///     Force your score to be replaced
    /// </summary>
    public static async Task ReplaceLeaderboard(Steamworks.Data.Leaderboard leaderboard, int value,
        int[] details = null)
    {
        var leaderboardUpdate = await leaderboard.ReplaceScore(value, details ?? Array.Empty<int>());
        if (!leaderboardUpdate.HasValue)
        {
            Debug.LogError("leaderboardUpdate is null");
            return;
        }

        Debug.Log(leaderboardUpdate.Value);
    }

    public static async void UploadFinalizedRunScore_TechBooh(Score finalizedScore)
    {
        if (finalizedScore == null)
        {
            return;
        }

        try
        {
            var leaderboard = await GetLeaderBoards(DefaultLeaderboardName_TechBooh);
            if (!leaderboard.HasValue)
            {
                Debug.LogError("Nao foi possivel obter o leaderboard para enviar o score finalizado.");
                return;
            }

            int[] details =
            {
                finalizedScore.wave,
                finalizedScore.kills,
                Mathf.RoundToInt(finalizedScore.time),
                finalizedScore.attempt
            };

            await ReplaceLeaderboard(leaderboard.Value, finalizedScore.score, details);
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao enviar score finalizado para o leaderboard: {e.Message}");
        }
    }

    public static async Task<Steamworks.Data.Leaderboard?> GetLeaderBoards(string lbName)
    {
        try
        {
            return await FindOrCreateLeaderboardAsync(lbName);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        return null;
    }

    private static async Task<Steamworks.Data.Leaderboard?> FindOrCreateLeaderboardAsync(string leaderboardName)
    {
        try
        {
            return await SteamUserStats.FindOrCreateLeaderboardAsync(leaderboardName,
                LeaderboardSort.Ascending,
                LeaderboardDisplay.Numeric);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        return null;
    }
}
