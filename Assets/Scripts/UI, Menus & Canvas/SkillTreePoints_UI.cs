using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreePoints_UI : MonoBehaviour
{
    [SerializeField] private MMF_Player levelUpFeedbacks;
    [SerializeField] private float delayToUpdateLevel = 2.3f;

    [Space(15)]
    [SerializeField] private TextMeshProUGUI levelTxt;
    [SerializeField] private TextMeshProUGUI pointsAvailableTxt, levelProgressionTxt;
    [SerializeField] private Slider levelProgressionSlider;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private LanguageData languageData;

    private void OnEnable()
    {
        UpdateText();
        playerData.OnSkillPointsChange += UpdateAvailableSkillPoints;
    }

    private void UpdateText(bool checkLevelUp = true)
    {
        string levelText = "";
        switch (languageData.currentLanguage)
        {
            case Language.english:
                levelText = "Level ";
                break;
            case Language.chinese:
                levelText = "等级 ";
                break;
            case Language.portuguese:
                levelText = "Nível ";
                break;
            case Language.spanish:
                levelText = "Nivel ";
                break;
            default:
                break;
        }
        levelTxt.text = levelText + playerData.level.ToString();

        if (playerData.level >= playerData.maxLevel) //Maximum level reached.
        {
            levelProgressionTxt.text = "MAX LEVEL";
            levelProgressionSlider.value = 1;
        }
        else
        {
            levelProgressionTxt.text = 0 +"/" + playerData.CalculateXpToNextLevel();
            levelProgressionSlider.value = 0;
        }

        pointsAvailableTxt.text = playerData.GetSkillPoints().ToString();


        if (checkLevelUp && playerData.level < playerData.maxLevel)
        {
            StartCoroutine(UpdateSliderValue(PercentageToNextLevel(), delayToUpdateLevel - 0.3f));
            //Invoke(nameof(CheckLevelUp), delayToUpdateLevel);
        }
    }

    private IEnumerator UpdateSliderValue(float targetValue, float duration)
    {
        levelProgressionSlider.value = 0;
        float startValue = levelProgressionSlider.value;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            levelProgressionSlider.value = Mathf.Lerp(startValue, targetValue, elapsedTime / duration);

            levelProgressionTxt.text = Mathf.FloorToInt(levelProgressionSlider.value * playerData.xp) + "/" + playerData.CalculateXpToNextLevel();
            yield return null;
        }

        levelProgressionTxt.text = playerData.xp.ToString() + "/" + playerData.CalculateXpToNextLevel();
        levelProgressionSlider.value = targetValue;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (playerData.level >= playerData.maxLevel) return;

        if (playerData.xp >= playerData.CalculateXpToNextLevel())
        {
            playerData.xp -= playerData.CalculateXpToNextLevel();
            playerData.level++;
            playerData.AddSkillPoints(1);

            UpdateText(false);
            StartCoroutine(UpdateSliderValue(PercentageToNextLevel(), 1f));
            levelUpFeedbacks.PlayFeedbacks();
        }
    }

    private void UpdateAvailableSkillPoints()
    {
        pointsAvailableTxt.text = playerData.GetSkillPoints().ToString();
    }

    private float PercentageToNextLevel()
    {
        return (float) playerData.xp / (float)playerData.CalculateXpToNextLevel();
    }
}

