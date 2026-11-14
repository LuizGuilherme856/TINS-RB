using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Populate weapon and rune icons in the inventory.
/// </summary>
public class InventoryMenu : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;

    [Header("*--- WEAPONS ---*")] 
    [SerializeField] private GameObject weaponModel; //receives a generic version of the weapon
    [SerializeField] private Transform weaponContentTransform;
    
    [Header("*--- Runes (upgrades) ---*")]
    [SerializeField] private GameObject upgradeModel; //receives a generic version of the upgrade button
    [SerializeField] private Transform upgradeContentTransform;
    [SerializeField] private ScrollRect upgradeScrollRect;

    public ItemData chaliceItem;

    private GunInventory gunInventory;
    private MeleeWeaponInventory meleeWeaponInventory;

    [SerializeField] private RuneInventory runeInventory;

    private void Awake()
    {
        gunInventory = playerHolder?.GetGunInventory(); 

        meleeWeaponInventory = playerHolder?.meleeWeaponInventory;

        if (upgradeScrollRect == null && upgradeContentTransform != null)
        {
            upgradeScrollRect = upgradeContentTransform.GetComponentInParent<ScrollRect>(true);
        }
    }

    private void OnEnable()
    {
        UpdateWeaponData();
        UpdateUpgradeData();


        if (chaliceItem == null || weaponModel == null || weaponContentTransform == null) return;

        //Currently specific to the chalice; generalize if other inventory items are added.
        GameObject newWeapon = weaponModel;
        newWeapon = Instantiate(newWeapon, weaponContentTransform);
        newWeapon.GetComponent<InventoryMenuWeaponData>().UpdateItemData(chaliceItem);
    }

    private void UpdateWeaponData()
    {
        if (weaponModel == null || weaponContentTransform == null) return;
        foreach (Transform child in weaponContentTransform) //removes all the guns
        {
            Destroy(child.gameObject);

        }
        var gunsInInventory = gunInventory?.GetGuns();
        if (gunsInInventory != null)
        {
            foreach (var gun in gunsInInventory)
                AddNewWeaponToInventory(gun);
        }
        AddNewWeaponToInventory(meleeWeaponInventory?.GetMeleeWeapon());
    }

    private void UpdateUpgradeData()
    {
        if (upgradeModel == null || upgradeContentTransform == null || runeInventory == null) return;
        foreach (Transform child in upgradeContentTransform) //removes all the upgrades
        {
            Destroy(child.gameObject);
        }

        var upgrades = runeInventory.GetUpgradesQuantityClasses();

        foreach (var upgradeItem in upgrades)
        {
            AddNewUpgradeToInventory(upgradeItem.rune, upgradeItem.quantity);
        }
    }

    private void AddNewWeaponToInventory(GunData gunData)
    {
        if (gunData == null) return;

        GameObject newWeapon = weaponModel;
        newWeapon = Instantiate(newWeapon, weaponContentTransform);
        newWeapon.GetComponent<InventoryMenuWeaponData>().UpdateWeaponData(gunData);
    }
    private void AddNewWeaponToInventory(MeleeData meleeData)
    {
        if (meleeData == null) return;
        GameObject newWeapon = weaponModel;
        newWeapon = Instantiate(newWeapon, weaponContentTransform);
        newWeapon.GetComponent<InventoryMenuWeaponData>().UpdateWeaponData(meleeData);
    }

    private void AddNewUpgradeToInventory(RuneData upgradeData, int quantity)
    {
        GameObject newUpgrade = upgradeModel;
        newUpgrade = Instantiate(newUpgrade, upgradeContentTransform);
        newUpgrade.GetComponent<InventoryMenuUpgradeData>().SetUpgradeData(upgradeData, quantity);
        AttachScrollFollowOnUpgrade_TechBooh(newUpgrade);
    }

    /// <summary>
    /// TechBooh:
    /// Navigation selects rune items rather than the scrollbar.
    /// Each selected item must ask ScrollRect to follow it.
    /// </summary>
    private void AttachScrollFollowOnUpgrade_TechBooh(GameObject upgradeObject)
    {
        if (upgradeScrollRect == null || upgradeObject == null)
        {
            return;
        }

        RectTransform itemRect = upgradeObject.GetComponent<RectTransform>();
        if (itemRect == null)
        {
            return;
        }

        ScrollSelectedIntoView_TechBooh scrollFollow = upgradeObject.GetComponent<ScrollSelectedIntoView_TechBooh>();
        if (scrollFollow == null)
        {
            scrollFollow = upgradeObject.AddComponent<ScrollSelectedIntoView_TechBooh>();
        }

        scrollFollow.Setup(upgradeScrollRect, itemRect);
    }

}

/// <summary>
/// TechBooh:
/// Keep the selected item visible during controller or keyboard navigation.
/// Prevent focus from moving to an offscreen rune without scrolling.
/// </summary>
public class ScrollSelectedIntoView_TechBooh : MonoBehaviour, ISelectHandler
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private float topPadding = 16f;
    [SerializeField] private float bottomPadding = 16f;

    public void Setup(ScrollRect targetScrollRect, RectTransform targetItemRect)
    {
        scrollRect = targetScrollRect;
        targetRect = targetItemRect;
    }

    public void OnSelect(BaseEventData eventData)
    {
        ScrollIntoView_TechBooh();
    }

    private void ScrollIntoView_TechBooh()
    {
        if (scrollRect == null || targetRect == null || scrollRect.content == null || scrollRect.viewport == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        RectTransform contentRect = scrollRect.content;
        RectTransform viewportRect = scrollRect.viewport;

        Bounds targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewportRect, targetRect);
        float viewportTop = viewportRect.rect.yMax - topPadding;
        float viewportBottom = viewportRect.rect.yMin + bottomPadding;

        Vector2 contentPosition = contentRect.anchoredPosition;

        // TechBooh:
        // If selection moves below the visible viewport,
        // increase anchoredPosition.y to scroll down.
        if (targetBounds.min.y < viewportBottom)
        {
            float delta = viewportBottom - targetBounds.min.y;
            contentPosition.y += delta;
        }
        // If selection moves above the visible viewport,
        // decrease anchoredPosition.y to scroll up.
        else if (targetBounds.max.y > viewportTop)
        {
            float delta = targetBounds.max.y - viewportTop;
            contentPosition.y -= delta;
        }

        float maxY = Mathf.Max(0f, contentRect.rect.height - viewportRect.rect.height);
        contentPosition.y = Mathf.Clamp(contentPosition.y, 0f, maxY);
        contentRect.anchoredPosition = contentPosition;
    }
}
