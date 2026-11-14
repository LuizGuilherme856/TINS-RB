using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    [SerializeField] private LanguageData languageData;
    [SerializeField] private TextMeshProUGUI textTMP, textTMP_shadow; //The shadow is a duplicate text element underneath.
    [SerializeField] private string[] englishSubtitles, portugueseSubtitles, spanishSubtitles, chineseSubtitles;
    



    private int currentIndex = -1;//Start at -1 because the first iteration increments immediately.

    /// <summary>
    /// Animation event that displays the next dialogue line.
    /// </summary>
    public void SetNextLine()
    {
        string line = GetNextLine();
        textTMP.text = line;
        textTMP_shadow.text = line;

    }

    /// <summary>
    /// Advance the counter and return the next dialogue line.
    /// </summary>
    /// <returns></returns>
    private string GetNextLine()
    {

        currentIndex++;
        if(currentIndex >= englishSubtitles.Length)
        {
            Debug.Log("chegou ao fim do vetor de falas");
            return "";
        }

        switch (languageData.currentLanguage)
        {
            case Language.english:
                return englishSubtitles[currentIndex];
            case Language.chinese:
                return chineseSubtitles[currentIndex];
            case Language.portuguese:
                return portugueseSubtitles[currentIndex];
            case Language.spanish:
                return spanishSubtitles[currentIndex];
            default:
                Debug.LogError("algo saiu errado na legenda!!! Retornando em ingles");
                return englishSubtitles[currentIndex];
        }


    }

}
