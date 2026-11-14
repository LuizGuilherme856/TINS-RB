using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateFloatingText : MonoBehaviour
{
    [SerializeField] private LanguageData languageData;
    private MMF_Player mmf_player;
    private MMF_FloatingText feedbackFloatingText;
    
    [SerializeField] private string englishText, chineseText, portugueseText, spanishText;

    private void Awake()
    {
        mmf_player= GetComponent<MMF_Player>();
        feedbackFloatingText = mmf_player.GetFeedbackOfType<MMF_FloatingText>();
    }

    private void OnEnable()
    {
        SetText();
    }
    private void SetText()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                feedbackFloatingText.Value = englishText;
                break;
            case Language.chinese:
                feedbackFloatingText.Value = chineseText;
                break;
            case Language.portuguese:
                feedbackFloatingText.Value = portugueseText;
                break;
            case Language.spanish:
                feedbackFloatingText.Value = spanishText;
                break;
            default:
                break;
        }


    }

}
