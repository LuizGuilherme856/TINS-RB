using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NPC_Dialogue : MonoBehaviour
{
    [SerializeField] private LanguageData languageData;

    [Header("English")]
    [SerializeField] private string[] lineEnglish;

    [Header("Chinese")]
    [SerializeField] private string[] lineChinese;
    [Header("Portuguese")]
    [SerializeField] private string[] linePortuguese;
    [Header("Spanish")]
    [SerializeField] private string[] lineSpanish;


    public string GetRandomDialogueLine()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                return lineEnglish[Random.Range(0, lineEnglish.Length)];
            case Language.chinese:
                return lineChinese[Random.Range(0, lineChinese.Length)];
            case Language.portuguese:
                return linePortuguese[Random.Range(0, linePortuguese.Length)];
            case Language.spanish:
                return lineSpanish[Random.Range(0, lineSpanish.Length)];
            default:
                Debug.LogWarning("language not found");
                return lineEnglish[Random.Range(0, lineEnglish.Length)];

        }
    }

    public int GetNumberOfLines() //When multiple lines exist, avoid repeating the same line twice.
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                return lineEnglish.Length;
            case Language.chinese:
                return lineChinese.Length;
            case Language.portuguese:
                return linePortuguese.Length;
            case Language.spanish:
                return lineSpanish.Length;
            default:
                Debug.LogWarning("language not found");
                return lineEnglish.Length;

        }

    }
}
