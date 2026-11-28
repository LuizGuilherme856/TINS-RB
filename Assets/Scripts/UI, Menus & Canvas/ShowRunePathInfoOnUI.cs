using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Show or hide rune-path selection
/// and populate its information fields.
/// </summary>
public class ShowRunePathInfoOnUI : MonoBehaviour
{
    [SerializeField] private LanguageData languageData;
    [SerializeField] private TextMeshProUGUI runePathName;
    [SerializeField] private Image runePathImage;
    [SerializeField] private GameObject runePathPanel;
    [SerializeField] private RuneInfoPanel[] runeInfoPanels;
    [SerializeField] private PauseMenuManager pauseMenuManager;
    [SerializeField] private float timeScale = 0.1f;

    [Header("Reroll stuff")]
    public int rerollPrice = 100;
    public TextMeshProUGUI priceText;

    public MMF_Player rerollFeedback, cannotRerollFeedback;

    [Header("Path stuff")]
    public TextMeshProUGUI luckPathtxt;
    public TextMeshProUGUI timePathTxt, focusPathTxt, strengthPathTxt, equilibriumPathTxt;
    public RuneInventory runeInventory;


    [Space(15)]
    public RunData runData;
    public PlayerHolder playerHolder;
    public PointManager pointManager;
    public GlobalStats globalStats;

    [Header("Button stuff")]
    public SelectEventSystemButton selectEventSystemButton;

    private RuneRitualManager runePathManager;
    public event Action OnRunePathPanelIsVisible, OnRunePathPanelIsNotVisible;

    private RuneRitualHolder runeRitualHolder;

    private int rollsCount = 0;

    // void OnEnable()
    // {
    //     playerHolder.GetPlayerInputHandler().OnEscape += HidePanel;
    //     playerHolder.GetPlayerInputHandler().OnInventory += HidePanel;
    // }

    public void ShowInfoOnScreen(RuneRitualHolder runePath, BuyRune buyUpgrade, RuneRitualManager runePathManager)
    {
        runeRitualHolder = runePath;
        this.runePathManager = runePathManager;

        if (playerHolder != null && playerHolder.GetPlayerInputHandler() != null)
        {
            playerHolder.GetPlayerInputHandler().SetCanMove(false);
        }

        runePathName.text = runeRitualHolder.GetName(languageData.currentLanguage);
        runePathImage.sprite = runeRitualHolder.GetIcon();

        List<RuneData> runes = runeRitualHolder.GetRunes();

        runes.MMShuffle();

        for (int i = 0; i < 3; i++)
        {
            runeInfoPanels[i].SetRuneInfo(runes[i]);
            runeInfoPanels[i].GetComponent<ChooseRunePath>().SetBuyUpgrade(buyUpgrade);
        }

        pauseMenuManager.ChangeTimeScale(timeScale);
        runePathPanel.SetActive(true);

        UpdatePrice();

        luckPathtxt.text = runeInventory.GetPathQuantity(PathsOfPowerNames.luck).ToString();
        timePathTxt.text = runeInventory.GetPathQuantity(PathsOfPowerNames.time).ToString();
        focusPathTxt.text = runeInventory.GetPathQuantity(PathsOfPowerNames.focus).ToString();
        strengthPathTxt.text = runeInventory.GetPathQuantity(PathsOfPowerNames.strength).ToString();
        equilibriumPathTxt.text = runeInventory.GetPathQuantity(PathsOfPowerNames.equilibrium).ToString();


        //Set the default selected button.
        selectEventSystemButton.SetButtonToSelect(runeInfoPanels[0].gameObject);

        OnRunePathPanelIsVisible?.Invoke();
    }

    /// <summary>
    /// Called by the button.
    /// </summary>
    public void Reroll()
    {
        int price = UpdatePrice();

        if (pointManager.currentPoints >= price)
        {
            rollsCount++;
            pointManager.RemovePoints(price);
            rerollFeedback.PlayFeedbacks();
            StartCoroutine(Reroll_Routine(price));
        }
        else
        {
            cannotRerollFeedback.PlayFeedbacks();
        }

        UpdatePrice();

    }

    private IEnumerator Reroll_Routine(int price)
    {
        List<RuneData> runes = runeRitualHolder.GetRunes();

        runes.MMShuffle();


        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            runeInfoPanels[i].SetRuneInfo(runes[i]);
        }



    }

    private int UpdatePrice()
    {
        float price = rerollPrice * runData.currentWaveNumber;
        price = (price * rollsCount) / 3; //Double the price every three rerolls.

        price *= globalStats.ForCurrentRun.globalPriceMultiplier;
        // Check affordability and color the price accordingly.
        int priceInt = Mathf.FloorToInt(price);
        if (pointManager.currentPoints >= price)
        {
            priceText.text = $"<color=#00D95F>{priceInt}</color>"; // Green.
        }
        else
        {
            priceText.text = $"<color=#FF0040>{priceInt}</color>"; // Red.
        }
        return priceInt;
    }

    public void HidePanel()
    {
        OnRunePathPanelIsNotVisible?.Invoke();

        runePathManager.UnlockRune();
        pauseMenuManager.ChangeTimeScale(1);
        runePathPanel.SetActive(false);
        runePathManager.enabled = false;

        if (playerHolder != null && playerHolder.GetPlayerInputHandler() != null)
        {
            playerHolder.GetPlayerInputHandler().SetCanMove(true);
        }


        playerHolder.GetPlayerInputHandler().OnEscape -= HidePanel;
        playerHolder.GetPlayerInputHandler().OnInventory -= HidePanel;

    }
}
