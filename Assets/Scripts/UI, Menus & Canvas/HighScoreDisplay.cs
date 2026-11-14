using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class HighScoreDisplay : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameObject tableRow;
    [SerializeField] private Transform rowParent;
    [SerializeField] private int maxRows = 50; //Limit instantiation to avoid frame-time spikes.

    [SerializeField] private ScoreType scoreType = ScoreType.local;
    public SteamLeaderBoard leaderBoard;


    private List<Score> scores;

    private void OnEnable()
    {

        switch (scoreType)
        {
            case ScoreType.local:
                scores = playerData.scores;
                break;
            case ScoreType.topGlobal:
                scores = leaderBoard.GetTopGlobal();
                break;
            case ScoreType.friends:
                scores = leaderBoard.GetFriendsScore();
                break;
            case ScoreType.aroundPlayer:
                scores = leaderBoard.GetSurroundScores();
                break;
            default:
                break;
        }


        if (scores == null) return;

        scores = scores.OrderByDescending(x => x.score).ToList();

        SetRowsToTable();
    }

    private void SetRowsToTable()
    {
        // Remove existing rowParent children before populating.
        // Use DestroyImmediate in the Editor for immediate updates.
#if UNITY_EDITOR
        for (int i = rowParent.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(rowParent.GetChild(i).gameObject);
        }
#else
        // Use deferred Destroy during gameplay.
        for (int i = rowParent.childCount - 1; i >= 0; i--)
        {
            Destroy(rowParent.GetChild(i).gameObject);
        }
#endif

        int count = 0;
        foreach (var score in scores)
        {
            var row = Instantiate(tableRow, rowParent);
            if(scoreType == ScoreType.local)
            {
                row.GetComponent<HighScoreTableRow>().SetScoreInfoToRow(score, count+1);
            }
            else
            {
                row.GetComponent<HighScoreTableRow_Global>().SetScoreInfoToRow(score);
            }

                count++; 
            if (count >= maxRows-1) return;
        }
    }

    private enum ScoreType { local, topGlobal, friends, aroundPlayer}

}
