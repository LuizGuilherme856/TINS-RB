using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Ask whether to revive when the player dies with enough horns.
/// </summary>
public class HandlePlayerReviveScreen : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private GameObject reviveScreen;
    [SerializeField] private MMF_Player timeFeedbacks;
    [SerializeField] private PauseMenuManager pausesMenuManager;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GlobalStats globalStats;
    
    [SerializeField] private SelectEventSystemButton selectEventSystemButton;
    [SerializeField] private GameObject reviveButton;
    [SerializeField] private TextMeshProUGUI priceTxt;
    private int hornCost = 5;

    int timesRevived = 0;
    void Start()
    {
       playerHolder.GetPlayerHealth().OnKnockedDown += ShowReviveScreen;
    }

    public void ShowReviveScreen()
    {
        if(globalStats.FreeRevives > 0) //Skill allowing one revival after death.
        {
            globalStats.ChangeFreeRevives(-1, OperationType.AddUp);
            playerHolder.GetPlayerDeathScript().ReEnablePlayerSprites();
            playerHolder.GetRevivePlayer().Revive();
            timesRevived++;
            return;
        }

        LetPlayerDie();

    }

    private void ActivateReviveScreen()
    {
        selectEventSystemButton.SetButtonToSelect(reviveButton);
        reviveScreen.SetActive(true);
        priceTxt.text = (hornCost * timesRevived).ToString();
    }

    public void RevivePlayerMethod()
    {
        playerHolder.GetPlayerDeathScript().ReEnablePlayerSprites();
        playerHolder.GetRevivePlayer().Revive();
        playerHolder.GetGunInventory().ResetGuns();
        reviveScreen.SetActive(false);
        pausesMenuManager.ChangeTimeScale(1f);
        playerData.AddHorns(-hornCost*timesRevived);
        timesRevived++;

    }


    public void LetPlayerDie()
    {
        playerHolder.GetPlayerHealth().Kill();
        reviveScreen.SetActive(false);
        pausesMenuManager.ChangeTimeScale(1f);
    }

}
