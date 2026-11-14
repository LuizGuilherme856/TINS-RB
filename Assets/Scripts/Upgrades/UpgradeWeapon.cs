using System;
using UnityEngine;

public class UpgradeWeapon : InteractWithPlayer
{
    [Space(15)]
    [Header("     WEAPON UPGRADE SETTINGS")]

    [SerializeField] private int price = 4000;

    [SerializeField] private GunData[] defaultGuns;//, goldenGuns; //Both arrays must have equal lengths.

    public bool isWeaponReady { get; set; }

    private UpgradeWeaponState currentState = UpgradeWeaponState.noWeapon;
    private GunData currentGunData, goldenGunData;
    private UpgradeWeaponFeedbacks upgradeWeaponFeedbacks;
    private UpgradeWeaponAnimationFX upgradeWeaponAnimationFX;

    public GlobalStats globalStats;

    public bool canBuy = true;
    public event Action OnUpgrade;
    protected override void Awake()
    {
        base.Awake();
        upgradeWeaponFeedbacks = GetComponent<UpgradeWeaponFeedbacks>();
        upgradeWeaponAnimationFX = GetComponentInChildren<UpgradeWeaponAnimationFX>();
    }

    private void Start()
    {
        upgradeWeaponAnimationFX.OnWeaponIsReady += WeaponIsReady;
    }



    protected override void StartInteraction()
    {
        base.StartInteraction();
        switch (currentState)
        {
            case UpgradeWeaponState.noWeapon:
                SetBuyUpgradeText();
                break;
            case UpgradeWeaponState.upgrading:
                SetUpgradingText();
                break;
            case UpgradeWeaponState.weaponReady:
                SetTakeUpgradedWeaponText();
                break;
            default:
                break;
        }


    }


    protected override void InteractionAction()
    {
        if (canBuy == false)
        {
            upgradeWeaponFeedbacks.PlayCantBuyFeedback();
            return;
        }

        switch (currentState)
        {
            case UpgradeWeaponState.noWeapon:
                CheckIfPlayerCanBuyUpgrade();
                break;
            case UpgradeWeaponState.upgrading:
                upgradeWeaponFeedbacks.PlayCantBuyFeedback();
                break;
            case UpgradeWeaponState.weaponReady:
                TakeUpgradedWeapon();
                break;
            default:
                break;
        }

        /*Remove the player's equipped weapon with GunInventory.RemoveCurrentWeapon.
         * Display its sprite in front of the statue and play effects.
         * Find its defaultGuns index and use that index to select the matching golden weapon.
         * After the effects, display the upgraded weapon and let the player collect it. */

    }

    private void CheckIfPlayerCanBuyUpgrade()
    {
        currentGunData = gunInventory.GetCurrentEquipedGun();

        if (currentGunData == null)//Nina has no gun.
        {
            upgradeWeaponFeedbacks.PlayCantBuyFeedback();
            return;
        }

        if (currentGunData.isGoldenWeapon)
        {
            upgradeWeaponFeedbacks.PlayCantBuyFeedback();
            return;
        }

        if (pointManager.currentPoints >= GetPrice())
        {
            isWeaponReady = false;

            GetGoldenVersion(currentGunData);

            if (goldenGunData == null) //Not found.
            {
                upgradeWeaponFeedbacks.PlayCantBuyFeedback();
                return;
            }


            gunInventory.RemoveCurrentWeapon();
            pointManager.RemovePoints(GetPrice());
            upgradeWeaponFeedbacks.PlayBuyFeedback();


            upgradeWeaponFeedbacks.SetWeaponSprite(currentGunData.buyIcon, goldenGunData.buyIcon);
            upgradeWeaponAnimationFX.PlayUpgradeAnimation();


            currentState = UpgradeWeaponState.upgrading;

            if (isTouchingPlayer) SetUpgradingText(); //Update text after the player deposits the weapon.
        }
        else
        {
            upgradeWeaponFeedbacks.PlayCantBuyFeedback();
        }

    }

    private void TakeUpgradedWeapon()
    {
        upgradeWeaponFeedbacks.PlayTakeWeaponFeedback();
        upgradeWeaponFeedbacks.SetWeaponSprite(null, null);

        gunInventory.AddGunToInventory(goldenGunData);

        currentState = UpgradeWeaponState.noWeapon;
        goldenGunData = null;

        OnUpgrade?.Invoke();

        if (isTouchingPlayer) SetBuyUpgradeText();
    }

    private void WeaponIsReady()
    {
        currentState = UpgradeWeaponState.weaponReady;
        if (isTouchingPlayer) SetTakeUpgradedWeaponText();
    }

    private void SetBuyUpgradeText()
    {
        var language = languageData.currentLanguage;


        string priceColored;
        //Show the price in green when affordable.
        if (pointManager.currentPoints >= GetPrice())
        {
            priceColored = UiColors.green_ + GetPrice() + UiColors.endColor_;
        }
        else
        {
            priceColored = UiColors.red_ + GetPrice() + UiColors.endColor_;
        }

        // string priceColored = red_ + price + endColor_;
        switch (language)
        {
            case Language.english:
                generalTextBoxUI.SetText(interactionButton + " Upgrade Weapon" + " (Cost: " + priceColored + ")");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText(interactionButton + " 升级武器" + " (消耗: " + priceColored + ")");
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText(interactionButton + " Melhorar arma" + " (Preço: " + priceColored + ")");
                break;
            case Language.spanish:
                generalTextBoxUI.SetText(interactionButton + " Mejorar arma" + " (Precio: " + priceColored + ")");
                break;
            default:
                break;
        }
    }

    private void SetUpgradingText()
    {
        var language = languageData.currentLanguage;

        switch (language)
        {
            case Language.english:
                generalTextBoxUI.SetText("Upgrading...");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText("升级中...");
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText("Dando upgrade...");
                break;
            case Language.spanish:
                generalTextBoxUI.SetText("Mejorando...");
                break;
            default:
                break;
        }
    }

    private void SetTakeUpgradedWeaponText()
    {
        var language = languageData.currentLanguage;

        switch (language)
        {
            case Language.english:
                generalTextBoxUI.SetText(interactionButton + " Take Upgraded Weapon");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText(interactionButton + " 获取升级后的武器");
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText(interactionButton + " Pegar arma melhorada");
                break;
            case Language.spanish:
                generalTextBoxUI.SetText(interactionButton + " Tomar arma mejorada");
                break;
            default:
                break;
        }

    }


    private void GetGoldenVersion(GunData gunData)
    {
        goldenGunData = gunData.goldenVersion;
    }

    public int GetPrice()
    {
        return (int)(price * globalStats.gunUpgradePriceMultiplier * globalStats.globalPriceMultiplier);
    }

    private enum UpgradeWeaponState { noWeapon, upgrading, weaponReady }

}
