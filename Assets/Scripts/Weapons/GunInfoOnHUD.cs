using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

/// <summary>
/// Display weapon information on the HUD.
/// </summary>
public class GunInfoOnHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoOnclipText, spareAmmoText, gunNameText;
    [SerializeField] private Image gunIcon;
    [SerializeField] private Sprite emptySprite;

    [Header("   JUICE")]
    [SerializeField] private MMF_Player changeWeaponFeedbacks;

    [Header("   OTHER")]
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private LanguageData languageData;



    private GunInventory gunInventory;
    private GunData currentGun; 

    private void Awake()
    {
        gunInventory = playerHolder.GetGunInventory();
        if (gunInventory == null) gunInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<GunInventory>();

    }


    private void OnEnable()
    {
        if(gunInventory == null) gunInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<GunInventory>();

        RefreshCurrentGun();
        RefreshCurrentAmmo();
        RefreshGunIconAndName();

        gunInventory.OnWeaponChange += RefreshCurrentGun;
        gunInventory.OnWeaponChange += RefreshCurrentAmmo;
        gunInventory.OnWeaponChange += RefreshGunIconAndName;
    }
    private void OnDisable()
    {
        if (gunInventory != null)
        {
            gunInventory.OnWeaponChange -= RefreshCurrentGun;
            gunInventory.OnWeaponChange -= RefreshCurrentAmmo;
            gunInventory.OnWeaponChange -= RefreshGunIconAndName;
        }
        if (currentGun != null) currentGun.OnAmmoChange -= RefreshCurrentAmmo;
    }

    private void RefreshGunIconAndName()
    {
        if (currentGun == null)
        {
            gunIcon.sprite = emptySprite;
            gunNameText.text = "";
        }
        else
        {
            gunIcon.sprite = currentGun.buyIcon;
            gunNameText.text = currentGun.GetName(languageData.currentLanguage);
            changeWeaponFeedbacks.PlayFeedbacks();
        }

    }

    private void RefreshCurrentAmmo()
    {
        if (currentGun == null)
        {
            ammoOnclipText.text = "- -";
            spareAmmoText.text = "- -";
        }
        else
        {
            ammoOnclipText.text = currentGun.currentAmmoOnClip.ToString();
            spareAmmoText.text = TINS.Campaign.CampaignCombatRules.IsCampaignMission
                ? "∞"
                : currentGun.currentSpareAmmo.ToString();
        }

    }

    private void RefreshCurrentGun()
    {
        if (currentGun != null) currentGun.OnAmmoChange -= RefreshCurrentAmmo;
        currentGun = gunInventory.GetCurrentEquipedGun();
        if (currentGun == null) return;
        currentGun.OnAmmoChange += RefreshCurrentAmmo;
    }

}
