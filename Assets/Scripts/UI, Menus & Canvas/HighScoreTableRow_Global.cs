using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreTableRow_Global : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI posTMP;
    [SerializeField] private TextMeshProUGUI scoreTMP, playerNameTMP;


    private Score score;
    public void SetScoreInfoToRow(Score score)
    {
        this.score = score;
        posTMP.text = score.globalPos.ToString();

        scoreTMP.text = score.score.ToString();
        playerNameTMP.text = score.playerName;

    }

}
