using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuneInfoPanel : MonoBehaviour
{

    [SerializeField] private Image runeIcon, pathIcon;
    [SerializeField] private TextMeshProUGUI runeName, runeDescription, pathName, numberOfTimesCanBeBought;
    [SerializeField] private LanguageData languageData;
    [SerializeField] private PathsOfPowerData pathsData;

     public RuneData upgradeData { get; private set; }
    public void SetRuneInfo(RuneData runeUpgradeData)
    {

        upgradeData = runeUpgradeData;
        runeName.text = runeUpgradeData.GetName(languageData.currentLanguage);
        runeDescription.text = runeUpgradeData.GetDescription(languageData.currentLanguage, true);
        runeIcon.sprite = runeUpgradeData.inventoryIcon;

        pathIcon.sprite = pathsData.GetIcon(runeUpgradeData.path);
        pathName.text = pathsData.GetName(runeUpgradeData.path, languageData.currentLanguage);
        numberOfTimesCanBeBought.text = (runeUpgradeData.numberOfTimesCanBeBought - 1).ToString();

    }

}
