using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pause on Survival-mode level-up,
/// show upgrade choices, and apply the selected ability upgrade.
/// </summary>
public class UpgradeManager_SurvivalMode : MonoBehaviour
{
    [SerializeField] GameObject upgradesScreen;
    [SerializeField] private XpManager_SurvivalMode xpManager;
    [SerializeField] private PauseMenuManager pauseMenuManager;
    [SerializeField] private List<RuneData> upgradeDataList;
    [SerializeField] private GameObject[] upgradeCardList;

    private UpgradeCardHandler_SurvivalMode upgradeCardHandler;

    private void OnEnable()
    {
        xpManager.OnLevelUp += ShowUpgradesScreen;
    }

    private void ShowUpgradesScreen()
    {
        ShuffleCards();
        upgradesScreen.SetActive(true);
        pauseMenuManager.ChangeTimeScale(0.02f);

       // Invoke(nameof(ChooseAnUpgrade), 2f);

    }

    /// <summary>
    /// Choose an upgrade and close UpgradesScreen.
    /// Called by UI buttons.
    /// </summary>
    public void TriggerCloseScreen()
    {
        Invoke(nameof(CloseScreen), 0.5f);
        pauseMenuManager.ChangeTimeScale(1);
    }

    private void CloseScreen()
    {
        upgradesScreen.SetActive(false);
    }

    /// <summary>
    /// Randomly select purchasable upgrades
    /// and populate their cards.
    /// </summary>
    private void ShuffleCards()
    {
        upgradeDataList.MMShuffle();

        for (int i = 0; i < upgradeCardList.Length; i++)
        {
            var card = upgradeCardList[i].GetComponent<UpgradeCardHandler_SurvivalMode>();
            card.SetNewUpgrade(upgradeDataList[i]);
            card.RefreshUpgradeInfo();
        }
    }

}
