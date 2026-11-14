using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using MoreMountains.Tools;
//Display statistics on the death screen.

public class DeathStatistics : MonoBehaviour
{
    private EnemyWaveManager hordeManager;
    private StatisticsManager statisticsManager;

    public float timeBetweenText = 1;


    [SerializeField] private LanguageData languageData;

    [Space(10)]
[Header("ENGLISH")]
[SerializeField] private string waveTextEnglish;
[SerializeField] private string killsTextEnglish, timeTextEnglish, scoreTextEnglish;
[SerializeField] private string bloodTextEnglish, xpTextEnglish;
[Header("CHINESE")]
[SerializeField] private string waveTextChinese;
[SerializeField] private string killsTextChinese, timeTextChinese, scoreTextChinese;
[SerializeField] private string bloodTextChinese, xpTextChinese;
[Header("PORTUGUESE")]
[SerializeField] private string waveTextPortuguese;
[SerializeField] private string killsTextPortuguese, timeTextPortuguese, scoreTextPortuguese;
[SerializeField] private string bloodTextPortuguese, xpTextPortuguese;
[Header("SPANISH")]
[SerializeField] private string waveTextSpanish;
[SerializeField] private string killsTextSpanish, timeTextSpanish, scoreTextSpanish;
    [SerializeField] private string bloodTextSpanish, xpTextSpanish;

    [Space(10)]
    [Header("Animation Durations")]
[SerializeField] private float minDuration = 0.3f;
    [SerializeField] private float maxDuration = 1.5f;
[SerializeField] private float maxKillsDuration = 5f;
[SerializeField] private int referenceWaveValue = 10;
[SerializeField] private int referenceKillsValue = 100;
[SerializeField] private int referenceBloodValue = 500;
[SerializeField] private int referenceXpValue = 500;
[SerializeField] private int referenceScoreValue = 10000;


    [Space(10)]
[SerializeField] private TextMeshProUGUI waveTMP, killsTMP, timeTMP, scoreTMP;
[SerializeField] private TextMeshProUGUI bloodTMP, xpTMP;
[Space(10)]
[SerializeField] private CheckNewHighScore checkNewHighScore;

    [Space(10)]
    [Header("Feedbacks")]

    public MMF_Player startFeedback;
    [SerializeField] private float rollingFeedbackInterval = 0.05f; // Configure in the Inspector.

    public MMF_Player rollingFeedback, finishLineFeedback, endFeedback;
    [SerializeField] private MMF_Player waveFinishFeedback, timeFinishFeedbacks;
[SerializeField] private MMF_Player killsFinishFeedback;
[SerializeField] private MMF_Player bloodFinishFeedback;
[SerializeField] private MMF_Player xpFinishFeedback;
    [SerializeField] private MMF_Player scoreFinishFeedback;
    [SerializeField] private MMF_Player spawnBodyFeedback;
    public List<GameObject> corpses;
    public int killCountCoeficient = 1;

[Header("Event System stuff")]
    public GameObject continueButton;
    public SelectEventSystemButton selectEventSystemButton;


    private void Awake()
    {
        corpses.MMShuffle();
        statisticsManager = FindFirstObjectByType<StatisticsManager>();
        hordeManager = FindFirstObjectByType<EnemyWaveManager>();
    }


public void RefreshStatistics()
{
    StartCoroutine(ShowStatisticsSequence());
}

private IEnumerator ShowStatisticsSequence()
{
    waveTMP.text = "";
    killsTMP.text = "";
    timeTMP.text = "";
    scoreTMP.text = "";
    bloodTMP.text = "";
    xpTMP.text = "";

        startFeedback.PlayFeedbacks();


    yield return new WaitForSeconds(timeBetweenText);
    int wave = hordeManager.GetCurrentWave();
    yield return StartCoroutine(AnimateCount(waveTMP, GetWaveText(), wave, GetDynamicDuration(wave, referenceWaveValue)));
    waveFinishFeedback.PlayFeedbacks();

      selectEventSystemButton.SetButtonToSelect(continueButton);

    yield return new WaitForSeconds(timeBetweenText);
    var time = statisticsManager.GetTimeElapsed();
    timeTMP.text = GetTimeText() + " " + ColorHelper.FormatTextWithColor(time, "b10e25");
    timeFinishFeedbacks.PlayFeedbacks();
    yield return new WaitForSeconds(0.3f);

    yield return new WaitForSeconds(timeBetweenText);
    int kills = statisticsManager.GetEnemiesKilled();
    yield return StartCoroutine(AnimateCount(killsTMP, GetKillsText(), kills, GetDynamicDuration(kills, referenceKillsValue, true))); // True for kill counts.
    killsFinishFeedback.PlayFeedbacks();

    yield return new WaitForSeconds(timeBetweenText);
    int blood = statisticsManager.GetTotalBloodPoints();
    yield return StartCoroutine(AnimateCount(bloodTMP, GetBloodText(), blood, GetDynamicDuration(blood, referenceBloodValue)));
    bloodFinishFeedback.PlayFeedbacks();

    yield return new WaitForSeconds(timeBetweenText);
    int xp = statisticsManager.GetTotalXp();
    yield return StartCoroutine(AnimateCount(xpTMP, GetXpText(), xp, GetDynamicDuration(xp, referenceXpValue)));
    xpFinishFeedback.PlayFeedbacks();

    yield return new WaitForSeconds(timeBetweenText);
    int score = statisticsManager.GetScore();
    yield return StartCoroutine(AnimateCount(scoreTMP, GetScoreText(), score, GetDynamicDuration(score, referenceScoreValue)));
    scoreFinishFeedback.PlayFeedbacks();

    yield return new WaitForSeconds(1);

    selectEventSystemButton.SetButtonToSelect(continueButton);

    endFeedback.PlayFeedbacks();

    checkNewHighScore.CheckScore();
}
private float GetDynamicDuration(int value, int reference, bool isKills = false)
{
    // At the reference value, use the midpoint between minimum and maximum.
    // Lower values approach minDuration; higher values approach maxDuration.
    float t = Mathf.Clamp01((float)value / reference);
    if (isKills)
        return Mathf.Lerp(minDuration, maxKillsDuration, t);
    else
        return Mathf.Lerp(minDuration, maxDuration, t);
}

 private IEnumerator AnimateCount(TextMeshProUGUI tmp, string label, int finalValue, float duration)
{
    float elapsed = 0f;
    int currentValue = 0;
    int startValue = 0;
    int lastValue = -1;
    float lastFeedbackTime = 0f;

        int count = 0;

    while (elapsed < duration)
        {
            count++;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, finalValue, t));
            tmp.text = $"{label} {currentValue}";

            // Play feedback only after the value changes and the minimum interval elapses.
            if (currentValue != lastValue && rollingFeedback != null && (Time.time - lastFeedbackTime) >= rollingFeedbackInterval)
            {
                rollingFeedback.PlayFeedbacks();

                // For kill counts, trigger spawnBodyFeedback every ten kills.
                if (tmp == killsTMP  && currentValue != 0 && count >= killCountCoeficient)
                {
                    count = 0;
                    try
                    {
                        spawnBodyFeedback.PlayFeedbacks();
                        var corpse = corpses.Where(x => x.activeSelf == false).FirstOrDefault();
                        corpse.SetActive(true);
                    // if (corpse != null)
                    //     {
                    //         corpse.SetActive(true);
                    //     }
                    //     else
                    //     {
                    //         // Duplicate the first corpse in the list, or use a prefab instead.
                    //         var newCorpse = Instantiate(corpses[0], corpses[0].transform.parent);
                    //         newCorpse.SetActive(true);
                    //         corpses.Add(newCorpse);
                    //     }

                    }
                    catch
                    {

                    }
                }

                lastValue = currentValue;
                lastFeedbackTime = Time.time;
            }

            yield return null;
        }
    tmp.text = $"{label} {ColorHelper.FormatTextWithColor(finalValue.ToString(), "b10e25")}";

    finishLineFeedback.PlayFeedbacks();
}


#region TextGet/Translation

    private string GetWaveText()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                return waveTextEnglish;
            case Language.chinese:
                return waveTextChinese;
            case Language.portuguese:
                return waveTextPortuguese;
            case Language.spanish:
                return waveTextSpanish;
            default:
                Debug.LogWarning("language not found");
                return waveTextEnglish;
        }
    }

    private string GetBloodText()
{
    switch (languageData.currentLanguage)
    {
        case Language.english:
            return bloodTextEnglish;
        case Language.chinese:
            return bloodTextChinese;
        case Language.portuguese:
            return bloodTextPortuguese;
        case Language.spanish:
            return bloodTextSpanish;
        default:
            Debug.LogWarning("language not found");
            return bloodTextEnglish;
    }
}

private string GetXpText()
{
    switch (languageData.currentLanguage)
    {
        case Language.english:
            return xpTextEnglish;
        case Language.chinese:
            return xpTextChinese;
        case Language.portuguese:
            return xpTextPortuguese;
        case Language.spanish:
            return xpTextSpanish;
        default:
            Debug.LogWarning("language not found");
            return xpTextEnglish;
    }
}

    private string GetScoreText()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                return scoreTextEnglish;
            case Language.chinese:
                return scoreTextChinese;
            case Language.portuguese:
                return scoreTextPortuguese;
            case Language.spanish:
                return scoreTextSpanish;
            default:
                Debug.LogWarning("language not found");
                return waveTextEnglish;
        }
    }

    private string GetKillsText()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                return killsTextEnglish;
            case Language.chinese:
                return killsTextChinese;
            case Language.portuguese:
                return killsTextPortuguese;
            case Language.spanish:
                return killsTextSpanish;
            default:
                Debug.LogWarning("language not found");
                return killsTextEnglish;
        }
    }

    private string GetTimeText()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                return timeTextEnglish;
            case Language.chinese:
                return timeTextChinese;
            case Language.portuguese:
                return timeTextPortuguese;
            case Language.spanish:
                return timeTextSpanish;
            default:
                Debug.LogWarning("language not found");
                return timeTextEnglish; ;
        }
    }

#endregion

}
