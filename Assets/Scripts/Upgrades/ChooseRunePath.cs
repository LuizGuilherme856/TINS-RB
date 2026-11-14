using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Assign the selected rune-path upgrade to the player and station.
/// </summary>
public class ChooseRunePath : MonoBehaviour
{
    private BuyRune buyUpgrade;
    [SerializeField] private ShowRunePathInfoOnUI showRunePathInfoOnUI;
    [SerializeField] private float delayToHidePanel = 0.01f;
    [SerializeField] private float minimumTimeToChoose = 0.1f; // Time.
    private RuneInfoPanel runeInfoPanel;

    private OnButtonSelectFeedbacks onButtonSelectFeedbacks;
    private RectTransform rectTransform;
    private float timeSinceEnabled;

    void Awake()
    {
        onButtonSelectFeedbacks = GetComponent<OnButtonSelectFeedbacks>();
        rectTransform = GetComponent<RectTransform>();
        runeInfoPanel = GetComponent<RuneInfoPanel>();
    }

    private void OnEnable()
    {
        // Reset elapsed time on enable.
        timeSinceEnabled = 0f;
    }

    private void Update()
    {
        // Track time since activation.
        timeSinceEnabled += Time.unscaledDeltaTime;
    }

    /// <summary>
    /// Called by the button.
    /// </summary>
    public void ChooseRunePathMethod()
    {
        if(buyUpgrade.enabled == true && buyUpgrade.isReady)
        {
            Debug.LogWarning("Voc� j�� escolheu um caminho de runa.");
            return;
        }

        // Check whether the minimum duration has elapsed.
        if (timeSinceEnabled < minimumTimeToChoose)
        {
            Debug.LogWarning("Voc� precisa esperar mais tempo antes de escolher o caminho da runa.");
            return;
        }

        buyUpgrade.enabled = true;
        buyUpgrade.isReady = true;
        buyUpgrade.SetRuneData(runeInfoPanel.upgradeData);
        buyUpgrade.Upgrade();
        
        Invoke(nameof(HidePanel), delayToHidePanel);
    }

    private void HidePanel()
    {
        //Reset its size.
        onButtonSelectFeedbacks.ResetEverything();
        rectTransform.localScale = Vector3.one;
        showRunePathInfoOnUI.HidePanel();
    }

    /// <summary>
    /// Use the purchase component to assign the rune offered by the upgrade station.
    /// </summary>
    public void SetBuyUpgrade(BuyRune buyUpgrade)
    {
        this.buyUpgrade = buyUpgrade;
    }
}
