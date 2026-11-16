using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HighScoreDisplay : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameObject tableRow;
    [SerializeField] private Transform rowParent;
    [SerializeField] private int maxRows = 50; //Limit instantiation to avoid frame-time spikes.

    [SerializeField] private ScoreType scoreType = ScoreType.local;


    private List<Score> scores;

    private void OnEnable()
    {

        if (scoreType != ScoreType.local || playerData?.scores == null) return;
        scores = playerData.scores.Where(score => score != null).OrderByDescending(score => score.score).ToList();

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
            row.GetComponent<HighScoreTableRow>().SetScoreInfoToRow(score, count + 1);

            count++;
            if (count >= maxRows-1) return;
        }
    }

    private enum ScoreType { local, topGlobal, friends, aroundPlayer}

}
