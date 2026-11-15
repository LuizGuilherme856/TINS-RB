using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TINS.Campaign;

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

    private CampaignBaseController campaignController;
    private bool campaignSoulsMode;

    private void OnEnable()
    {
        campaignController = CampaignBaseController.Active;
        campaignSoulsMode = campaignController != null && campaignController.IsReady;
        SetCampaignHeaderVisible(!campaignSoulsMode);

        if (campaignSoulsMode)
        {
            campaignController.SoulsChanged += UpdateCampaignSouls;
            UpdateCampaignSouls(campaignController.Profile.souls);
            return;
        }

        if (playerData != null)
            playerData.OnSkillPointsChange += UpdateAvailableSkillPoints;
        UpdateText();
    }

    private void OnDisable()
    {
        if (campaignController != null)
            campaignController.SoulsChanged -= UpdateCampaignSouls;
        if (playerData != null)
            playerData.OnSkillPointsChange -= UpdateAvailableSkillPoints;
    }

    private void SetCampaignHeaderVisible(bool visible)
    {
        if (levelTxt != null) levelTxt.gameObject.SetActive(visible);
        if (levelProgressionTxt != null) levelProgressionTxt.gameObject.SetActive(visible);
        if (levelProgressionSlider != null) levelProgressionSlider.gameObject.SetActive(visible);
    }

    private void UpdateCampaignSouls(int souls)
    {
        if (pointsAvailableTxt != null)
            pointsAvailableTxt.text = souls.ToString("N0");
    }

    private void UpdateText(bool checkLevelUp = true)
    {
        if (campaignSoulsMode)
        {
            SetCampaignHeaderVisible(false);
            if (campaignController != null && campaignController.Profile != null)
                UpdateCampaignSouls(campaignController.Profile.souls);
            return;
        }
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
        if (campaignSoulsMode && campaignController != null && campaignController.Profile != null)
            UpdateCampaignSouls(campaignController.Profile.souls);
        else if (playerData != null && pointsAvailableTxt != null)
            pointsAvailableTxt.text = playerData.GetSkillPoints().ToString();
    }

    private float PercentageToNextLevel()
    {
        return (float) playerData.xp / (float)playerData.CalculateXpToNextLevel();
    }
}

