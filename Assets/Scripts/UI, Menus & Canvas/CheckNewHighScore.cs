using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Check for a new high score and play death-screen feedbacks.

public class CheckNewHighScore : MonoBehaviour
{
    [SerializeField] private GameObject newHighScoreText;
    [SerializeField] private MMF_Player newHighScoreFeedbacks;

    [SerializeField] private PlayerData playerData;
    [SerializeField] private StatisticsManager statisticsManager;


    private List<Score> scores = new();

    private void Start()
    {
        scores = playerData.scores;
    }
    public void CheckScore()
    {
        // scores = scores.OrderByDescending(x => x.score).ToList();

        // if (scores.Count == 1) return; //Do not report a new high score when only one score exists.

        // if (statisticsManager.GetScore() == scores[0].score) //The sorted first entry matches this score when it is a new high score.
        // {
        //     Invoke(nameof(NewHighScore), fxDelay);
        // }

    }

    private void NewHighScore()
    {
        newHighScoreText.SetActive(true);
        newHighScoreFeedbacks.PlayFeedbacks();
    }
}
