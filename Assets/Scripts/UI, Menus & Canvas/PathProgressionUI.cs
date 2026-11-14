using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PathProgressionUI : MonoBehaviour
{
    [SerializeField] private PathOfPower path;
    [SerializeField] private RuneInventory runeInventory;
    [SerializeField] private float fillAmount = 0.01f, fillInterval = 0.01f;
    [SerializeField] private Image progressionBar;

    [Space(10)]
    [SerializeField] private GameObject icon5Runes;
    [SerializeField] private GameObject icon10Runes;
    [Space(10)]
    [SerializeField] private GameObject infoPanel;

    [SerializeField] private TextMeshProUGUI pathNameText;
    [SerializeField] private TextMeshProUGUI pathDescriptionText;
    [SerializeField] private LanguageData languageData;

    private float currentFillAmount;
    private float maxRunes = 10;
    private void OnEnable()
    {
        currentFillAmount = runeInventory.GetPathQuantity(path.path) / maxRunes;
        progressionBar.fillAmount = 0;
        StartCoroutine(FillImage());
        infoPanel.SetActive(false);
    }




    private IEnumerator FillImage()
    {
        while(progressionBar.fillAmount < currentFillAmount)
        {
            progressionBar.fillAmount += fillAmount;
            yield return new WaitForSecondsRealtime(fillInterval);
        }

        //if (icon5Runes.activeSelf == false && progressionBar.fillAmount >= 0.5f)
        //{
        //    icon5Runes.SetActive(true);
        //}


        //if (icon10Runes.activeSelf == false && progressionBar.fillAmount >= 0.95f)
        //{
        //    icon10Runes.SetActive(true);
        //}

    }

    /// <summary>
    /// Called by an event.
    /// </summary>
    public void ShowInfoPanel(bool show)
    {
        infoPanel.SetActive(show);
    }

    public void UpdateInfoText_Button()
    {
        pathNameText.text = path.GetName(languageData.currentLanguage);
        pathDescriptionText.text = path.GetDescription(languageData.currentLanguage);

    }

}
