using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display upgrade descriptions.
/// and invoke the selected upgrade.
/// </summary>
public class UpgradeCardHandler_SurvivalMode : MonoBehaviour
{
    [SerializeField] private LanguageData languageData;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI upgradeName, upgradeDescription;
    [SerializeField] private RuneInventory upgradeManager;
    [SerializeField] private PlayerHolder playerHolder;
    
    private Health playerHealth;

    private RuneData currentUpgrade;


    private void Awake()
    {
        playerHealth = playerHolder.GetPlayerHealth();
    }
    public void SetNewUpgrade(RuneData upgradeData)
    {
        currentUpgrade = upgradeData;
    }

    public void RefreshUpgradeInfo()
    {
        icon.sprite = currentUpgrade.inventoryIcon;
        upgradeName.text = currentUpgrade.GetName(languageData.currentLanguage);
        upgradeDescription.text = currentUpgrade.GetDescription(languageData.currentLanguage);
    }

    public void ConfirmUpgrade()
    {
        upgradeManager.AddUpgrade(currentUpgrade);

        //if (currentUpgrade.upgradePlayer)
        //{
        //    playerHealth.ReplenishHealth(currentUpgrade.IncreaseLife);
        //    playerHealth.UpdateHealth();

        //}

    }


}
