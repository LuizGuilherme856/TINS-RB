// TECHBOOH_YODINHA_CONSOLE_REMOVED_V1: applied to a dedicated CONSOLE project copy.
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreTableRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI posTMP;
    [SerializeField] private Image IconImage;
    [SerializeField] private TextMeshProUGUI scoreTMP, playerNameTMP, waveTMP, killsTMP, timeTMP, attemptTMP;
    
    [SerializeField] private Sprite[] charIcons;
    private Score score;
    public void SetScoreInfoToRow(Score score, int pos)
    {
        this.score = score;
        posTMP.text = pos.ToString();

        switch (score.character)
        {
            case Character.azaus:
                IconImage.sprite = charIcons[0];
                break;
            case Character.nina:
                IconImage.sprite = charIcons[1];
                break;
            case Character.hanba:
                IconImage.sprite = charIcons[2];
                break;
            case Character.deNiro:
                IconImage.sprite = charIcons[3];
                break;
            case Character.yodista:
                // TechBooh: keep historical enum values, but use a supported portrait.
                IconImage.sprite = charIcons[0];
                break;
            case Character.dog:
                IconImage.sprite = charIcons[5];
                break;
            default:
                break;
        }

        scoreTMP.text = score.score.ToString();
        playerNameTMP.text = score.playerName;
        waveTMP.text = score.wave.ToString();
        killsTMP.text = score.kills.ToString();
        timeTMP.text = MMTime.FloatToTimeString(score.time);
        attemptTMP.text = score.attempt.ToString();
    }

}
