using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using MoreMountains.Feedbacks; // Required for Action.

public class ObjectiveManager : MonoBehaviour
{
    public bool showOnlyObjective = false;
    public DialogueLine objective;
    public DialogueLine firstRitual, secondRitual, thirdRitual, fourthRitual;
    public List<ManageBloodRitual> bloodRituals;
    public TextMeshProUGUI objectiveTMP;
    public LanguageData languageData;
    public float delayToShowObjective = 5f;
    public float delayBetweenCharacter = 0.05f;
    public float delayToFade = 1f, fadeDuration = 2f;
    public MMF_Player showCharacterFeedback, startSentenceFeedback;
    private int completedCount = 0;

    void Start()
    {
        StartCoroutine(ShowObjective());

        if (showOnlyObjective) return;
        // Subscribe to every ritual's events.
        foreach (var ritual in bloodRituals)
        {
            ritual.OnCompleted += OnRitualCompleted;
        }
    }

    // Called whenever a ritual completes.
    void OnRitualCompleted()
    {
        completedCount++;

        DialogueLine ritualLine = null;
        switch (completedCount)
        {
            case 1: ritualLine = firstRitual; break;
            case 2: ritualLine = secondRitual; break;
            case 3: ritualLine = thirdRitual; break;
            case 4: ritualLine = fourthRitual; break;
        }
        if (ritualLine != null)
            StartCoroutine(TypeSentence(ritualLine.GetLine(languageData.currentLanguage)));
    }

    private IEnumerator ShowObjective()
    {
        yield return new WaitForSeconds(delayToShowObjective);
        StartCoroutine(TypeSentence(objective.GetLine(languageData.currentLanguage)));
    }

    // Typewriter-text coroutine.
    IEnumerator TypeSentence(string sentence)
    {
        startSentenceFeedback.PlayFeedbacks();

        objectiveTMP.text = "";
        objectiveTMP.alpha = 1f;
        foreach (char letter in sentence)
        {
            objectiveTMP.text += letter;
            showCharacterFeedback.PlayFeedbacks();
            yield return new WaitForSeconds(delayBetweenCharacter);
        }


        // Wait briefly before fading.
        yield return new WaitForSeconds(delayToFade);

        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            objectiveTMP.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        objectiveTMP.alpha = 0f;
    }
}