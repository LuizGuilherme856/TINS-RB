using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Used by the Vampire Survivors-style Survival mode.
/// </summary>
public class UpdateLevelProgressBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private XpManager_SurvivalMode xpManager;
    [SerializeField] private LanguageData languageData;

    private string textTranslated = "Level";

    private void OnEnable()
    {
        TranslateLevelText();
        xpManager.OnLevelUp += UpdateLevelText;
        
    }
    private void TranslateLevelText()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                textTranslated = "Level";
                break;
            case Language.chinese:
                textTranslated = "Level";
                break;
            case Language.portuguese:
                textTranslated = "Nível";
                break;
            case Language.spanish:
                textTranslated = "Nivel";
                break;
            default:
                break;
        }
    }


    private void UpdateLevelText()
    {
        levelText.text = textTranslated + " " + xpManager.GetCurrentLevel();
    }
}
