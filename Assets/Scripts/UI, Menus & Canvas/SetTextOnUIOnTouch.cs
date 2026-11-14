
using UnityEngine;

/// <summary>
/// Display text for objects touched by the player.
/// </summary>

public class SetTextOnUIOnTouch : InteractWithPlayer
{

    [SerializeField] private string englishText, chineseText, portugueseText, spanishText;
    protected override void StartInteraction()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                generalTextBoxUI.SetText(englishText);
                break;
            case Language.chinese:
                generalTextBoxUI.SetText(chineseText);
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText(portugueseText);
                break;
            case Language.spanish:
                generalTextBoxUI.SetText(spanishText);
                break;
            default:
                break;
        }

        
    }
}
