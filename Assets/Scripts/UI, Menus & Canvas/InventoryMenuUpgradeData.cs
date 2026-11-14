using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenuUpgradeData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private LanguageData languageData;

    [SerializeField]  private Image inventoryIcon;
    private InventoryMenuItemDetails inventoryItemDetails;
    private Button button;

    //weapon details
    private Sprite upgradeIcon;
    private string upgradeName, upgradeDescription;

    private void Awake()
    {
        inventoryItemDetails = FindFirstObjectByType<InventoryMenuItemDetails>();
        // button.onClick.AddListener(() => { SetDataToItemDetails(); });
    }

    private void Refresh()
    {
        inventoryIcon.sprite = upgradeIcon;
    }
    public void SetDataToItemDetails() //will add the item info to the panel that shows the info. Its called when the button is pressed
    {
        inventoryItemDetails.SetGenericDataDetails(upgradeIcon, upgradeName, upgradeDescription);
    }

    public void SetUpgradeData(RuneData upgradeData, int quantity)
    {
        upgradeIcon = upgradeData.inventoryIcon;
        upgradeName = upgradeData.GetName(languageData.currentLanguage);
        upgradeDescription = upgradeData.GetDescription(languageData.currentLanguage, true);
        quantityText.text = quantity.ToString();
        Refresh();
    }

}
