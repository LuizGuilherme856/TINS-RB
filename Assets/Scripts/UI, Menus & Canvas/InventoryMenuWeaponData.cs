using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Stores the weapon's ScriptableObject reference.

public class InventoryMenuWeaponData : MonoBehaviour
{
    [SerializeField] Image inventoryIcon;
    [SerializeField] private LanguageData languageData;
    private InventoryMenuItemDetails inventoryItemDetails;

    //weapon details
    private Sprite weaponBuyIcon;
    private string weaponName, weaponDescription;
    private GunData gunData;
    private MeleeData meleeData;
    public ItemData itemData;
    //private bool isMeleeWeapon = false;

    private void Awake()
    {
        //inventoryIcon= GetComponent<Image>();
        inventoryItemDetails = FindFirstObjectByType<InventoryMenuItemDetails>();
       // button.onClick.AddListener(() => { SetDataToItemDetails(); });
    }

    private void Refresh()
    {
        inventoryIcon.sprite = weaponBuyIcon;
        //SetDataToItemDetails();
    }
    public void SetDataToItemDetails() //will add the item info to the panel that shows the info. Its called when the button is pressed
    {
        if(meleeData)
        {
            inventoryItemDetails.SetMeleeWeaponDetails(weaponBuyIcon, weaponName, weaponDescription, meleeData);
        }
        else if(gunData)
        {
            inventoryItemDetails.SetWeaponDetails(weaponBuyIcon, weaponName, weaponDescription, gunData);
        }
        else if (itemData)
        {
            inventoryItemDetails.SetItemDetails(weaponBuyIcon, weaponName, weaponDescription);
        }
    }

    public void UpdateItemData(ItemData newItemData)
    {
        if (newItemData == null) return;
        itemData = newItemData;
        weaponBuyIcon = newItemData.sprite;
        weaponName = newItemData.GetName(languageData.currentLanguage);
        weaponDescription = newItemData.GetDescription(languageData.currentLanguage);
        gunData = null;
        meleeData = null;
        Refresh();
    }

    public void UpdateWeaponData(GunData weaponData)
    {
        if (weaponData == null) return;
        gunData = weaponData;
        //meleeData = null;
        weaponBuyIcon = weaponData.buyIcon;
        weaponName = weaponData.GetName(languageData.currentLanguage);
        weaponDescription = weaponData.GetDescription(languageData.currentLanguage);
        //isMeleeWeapon = false;
        Refresh();
    }

    public void UpdateWeaponData(MeleeData weaponData)
    {
        if (weaponData == null) return;
        meleeData = weaponData;
       // gunData = null;
        weaponBuyIcon = weaponData.buyIcon;
        weaponName = weaponData.GetName(languageData.currentLanguage);
        weaponDescription = weaponData.GetDescription(languageData.currentLanguage);
        //isMeleeWeapon = true;
        Refresh();
    }
}
