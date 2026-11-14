using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Show cinematic text at the start of the tutorial.
/// </summary>
public class CinematicTextSequence : MonoBehaviour
{


    public LanguageData languageData;
    public Image backgroundImage;
    public MMF_Player sfxFeedback;

    public List<Phrase> phrases;
    public TextMeshProUGUI uiText;
    public CanvasGroup canvasGroup;
    public float fadeTime = 1.5f;

    public event Action OnSequenceEnd;


    void Start()
    {
        canvasGroup.alpha = 0;
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
// #if UNITY_EDITOR
//         Time.timeScale = 25f;
//         #endif

        foreach (var phrase in phrases)
        {
            sfxFeedback?.PlayFeedbacks();
            uiText.text = GetTextByLanguage(phrase);
            yield return Fade(0, 1);
            yield return new WaitForSeconds(phrase.duration);
            yield return Fade(1, 0);
        }

        #if UNITY_EDITOR
        Time.timeScale = 1;
        #endif

        EndSequence();
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = to;
    }

    string GetTextByLanguage(Phrase phrase)
    {
        switch (languageData.currentLanguage)
        {
            case Language.english: return phrase.english;
            case Language.spanish: return phrase.spanish;
            case Language.chinese: return phrase.chinese;
            case Language.portuguese:
            default: return phrase.portuguese;
        }
    }

    void EndSequence()
    {
        canvasGroup.alpha = 0;
        OnSequenceEnd?.Invoke();
        StartCoroutine(FadeOutBackground(1f));
    }

    private IEnumerator FadeOutBackground(float duration)
    {
        if (backgroundImage == null)
            yield break;

        Color originalColor = backgroundImage.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsed / duration);
            backgroundImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Set alpha to exactly zero at completion.
        backgroundImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        backgroundImage.gameObject.SetActive(false); // Deactivate the background image after fading out.
    }


}

[System.Serializable]
public class Phrase
{

    public string english;
    public string chinese;
    public string portuguese;
    public string spanish;
    [Space(10)]
    public float duration = 4f;

    public string GetTextByLanguage(Language language)
    {
        switch (language)
        {
            case Language.english: return english;
            case Language.spanish: return spanish;
            case Language.chinese: return chinese;
            case Language.portuguese:
            default: return portuguese;
        }
    }
}
