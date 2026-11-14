using System;
using UnityEngine;

public class BuyWeapon : InteractWithPlayer
{
    [SerializeField] public GunData gun;
    [SerializeField] public MeleeData meleeWeapon;

    [SerializeField] public WeaponType weaponType;
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] private Material boughtGunIconMaterial;
    public bool canBuy = true;
    [Header("SPECIAL WEAPON SETTINGS")]
    [SerializeField] private int heartsCost = 0;

    private MeleeManager meleeManager;

    public event Action OnBuySpecialWeapon, OnBuyMeleeWeapon, OnBuyGun;

    public GlobalStats globalStats;

    protected override void Awake()
    {
        base.Awake();
        FindMeleeManager();
    }

    /// <summary>
    /// Find the correct instance because the player has two MeleeManagers:
    /// </summary>
    private void FindMeleeManager()
    {
        if (weaponType == WeaponType.melee)
        {
            //one for melee and one for special attacks.
            var meleeManagers = FindObjectsByType<MeleeManager>(FindObjectsSortMode.None);

            switch (weaponType)
            {
                //Inspect each manager's configured action type.

                case WeaponType.melee: //For a melee action,
                    foreach (var melee in meleeManagers)
                    {
                        if (melee.GetPlayerAction() == PlayerAction.melee)  //the RMB manager handles melee rather than special effects.
                        {
                            meleeManager = melee;
                        }
                    }
                    break;
                case WeaponType.special:
                    foreach (var melee in meleeManagers)
                    {
                        if (melee.GetPlayerAction() == PlayerAction.special)
                        {
                            meleeManager = melee;
                        }
                    }
                    break;
                default:
                    break;
            }


        }
    }

    private void Start()
    {
        // if (showWeaponIcon == false) return;

        if (weaponType != WeaponType.gun)
        {
            weaponSpriteRenderer.sprite = meleeWeapon.buyIcon;
        }
        else
        {
            weaponSpriteRenderer.sprite = gun.buyIcon;

        }
    }


    protected override void StartInteraction()
    {
        base.StartInteraction();

        UpdateText();
    }

    private void UpdateText()
    {
        if (weaponType == WeaponType.special)
        {
            SetBuySpecialWeaponText();

        }
        else if (weaponType == WeaponType.melee)
        {
            if (meleeManager.GetMeleeData() == meleeWeapon)
            {
                //TODO: Verify whether this has any effect.
            }
            else
            {
                SetBuyWeaponText(meleeWeapon.GetName(languageData.currentLanguage), meleeWeapon.GetPrice(), true);
            }
        }
        else
        {
            if (gunInventory.HasThisGun(gun))
            {
                SetBuyAmmoText(gun);
            }
            else if (gunInventory.HasThisGun(gun.goldenVersion))
            {
                SetBuyAmmoText(gun.goldenVersion);
            }
            else
            {
                SetBuyWeaponText(gun.GetName(languageData.currentLanguage), gun.GetPrice());
            }

        }
    }

    private void SetBuyAmmoText(GunData gun)
    {
        string gunName = gun.GetName(languageData.currentLanguage);

        string coloredPrice;
        //Show the price in green when affordable.
        if (pointManager.currentPoints >= gun.ammoPrice)
        {
            coloredPrice = UiColors.green_ + gun.ammoPrice + UiColors.endColor_;
        }
        else
        {
            coloredPrice = UiColors.red_ + gun.ammoPrice + UiColors.endColor_;
        }
        // string coloredPrice = red_ + gun.ammoPrice + endColor_;

        switch (languageData.currentLanguage)
        {
            case Language.english:
                generalTextBoxUI.SetText(interactionButton + " Ammo for " + gunName + " (Cost: " + coloredPrice + ")");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText(interactionButton + " 购买  " + gunName + " 的弹药 (消耗: " + coloredPrice + ")");
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText(interactionButton + " Munição para " + gunName + " (Preço: " + coloredPrice + ")");
                break;
            case Language.spanish:
                generalTextBoxUI.SetText(interactionButton + " Munición para " + gunName + " (Precio: " + coloredPrice + ")");
                break;
            default:
                break;
        }
    }

    private void SetBuyWeaponText(string name, int price, bool isMelee = false)
    {
        string replacing = null;
        string gunName = name;

        string coloredGunPrice;
        //Show the price in green when affordable.
        if (pointManager.currentPoints >= price)
        {
            coloredGunPrice = UiColors.green_ + price + UiColors.endColor_;
        }
        else
        {
            coloredGunPrice = UiColors.red_ + price + UiColors.endColor_;
        }

        bool inventoryFullAndNotMelee = gunInventory.IsInventoryFull() && isMelee == false;

        switch (languageData.currentLanguage)
        {
            case Language.english:
                string replacingTxt = "Replacing";
                if (inventoryFullAndNotMelee) replacing = " [" + UiColors.yellow_ + replacingTxt + UiColors.endColor_ + "] \n";
                generalTextBoxUI.SetText(replacing + interactionButton + " Buy " + gunName + " (Cost: " + coloredGunPrice + "</color>)");
                break;
            case Language.chinese:
                replacingTxt = "替换";
                if (inventoryFullAndNotMelee) replacing = " [" + UiColors.yellow_ + replacingTxt + UiColors.endColor_ + "] \n";
                generalTextBoxUI.SetText(replacing + interactionButton + " 购买  " + gunName + " (消耗: " + coloredGunPrice + ")"); ;
                break;
            case Language.portuguese:
                replacingTxt = "Substituindo";
                if (inventoryFullAndNotMelee) replacing = " [" + UiColors.yellow_ + replacingTxt + UiColors.endColor_ + "] \n";
                generalTextBoxUI.SetText(replacing + interactionButton + " Comprar " + gunName + " (Preço: " + coloredGunPrice + ")");
                break;
            case Language.spanish:
                replacingTxt = "Reemplazando";
                if (inventoryFullAndNotMelee) replacing = " [" + UiColors.yellow_ + replacingTxt + UiColors.endColor_ + "] \n";
                generalTextBoxUI.SetText(replacing + interactionButton + " Comprar " + gunName + " (Precio: " + coloredGunPrice + ")");
                break;
            default:
                break;
        }
    }

    private void SetBuySpecialWeaponText() //Purchase by sacrificing one heart.
    {
        string replacing = null;
        //string gunName = gun.GetName(languageData.currentLanguage);

        switch (languageData.currentLanguage)
        {
            case Language.english:
                if (heartsCost == 1) generalTextBoxUI.SetText(replacing + interactionButton + " Sacrifice 1 heart ");
                else generalTextBoxUI.SetText(replacing + interactionButton + " Sacrifice " + heartsCost + " hearts ");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText(replacing + interactionButton + " 键牺牲 " + heartsCost + " 点生命值 ");
                break;
            case Language.portuguese:
                if (heartsCost == 1) generalTextBoxUI.SetText(replacing + interactionButton + " Sacrificar 1 coração");
                else generalTextBoxUI.SetText(replacing + interactionButton + " Sacrificar " + heartsCost + " corações ");

                break;
            case Language.spanish:
                if (heartsCost == 1) generalTextBoxUI.SetText(replacing + interactionButton + " Sacrificar 1 Punto de Salud");
                else generalTextBoxUI.SetText(replacing + interactionButton + " Sacrificar " + heartsCost + " 1 Puntos de Salud ");
                break;
            default:
                break;
        }
    }



    protected override void InteractionAction()
    {
        if (canBuy == false)
        {
            cantBuyFeedback.PlayFeedbacks();
        }
        else if (weaponType == WeaponType.melee)
        {
            TryToBuyMeleeWeapon();
        }
        else if (weaponType == WeaponType.special)
        {
            player.GetComponent<SpecialWeaponInventory>().ChangeSpecialWeapon(meleeWeapon);
            player.GetComponent<HandleAttackOnPlayer>().DamagePlayer(heartsCost);
            buyFeedback.PlayFeedbacks();
            OnBuySpecialWeapon?.Invoke();
            Destroy(gameObject);
        }
        else //gun
        {
            TryToBuyGunOrAmmo();
        }

        UpdateText();

    }

    private void TryToBuyGunOrAmmo()
    {
        if (gunInventory.HasThisGun(gun)) //Purchase ammunition.
        {
            TryToBuyAmmo(gun);

        }
        else if (gunInventory.HasThisGun(gun.goldenVersion)) //Purchase ammunition for the golden variant.
        {
            TryToBuyAmmo(gun.goldenVersion);
        }
        else //Purchase a weapon.
        {
            if (pointManager.currentPoints >= gun.GetPrice())
            {
                weaponSpriteRenderer.material = boughtGunIconMaterial;
                gunInventory.AddGunToInventory(gun);
                pointManager.RemovePoints(gun.GetPrice());
                buyFeedback.PlayFeedbacks();
                HealOnBuy();

                OnBuyGun?.Invoke();
                //gun.ResetGunData();
            }
            else
            {
                notEnoughPointsFeedbacks?.TriggerNotEnoughPointsFeedbacks();
            }

        }
    }

    private void HealOnBuy()
    {
        if (globalStats.healthPointsToHealOnBuyWeapon > 0)
        {
            player.GetComponent<Health>().ReplenishHealth(globalStats.healthPointsToHealOnBuyWeapon);
        }
        else if (globalStats.healthPointsToHealOnBuyWeapon < 0)
        {
            player.GetComponent<Health>().SetDamage(-globalStats.healthPointsToHealOnBuyWeapon);
        }
    }

    private void TryToBuyAmmo(GunData gun)
    {
        if (pointManager.currentPoints >= gun.ammoPrice && gun.currentSpareAmmo < gun.GetMaxSpareAmmo())
        {
            gunInventory.RefillAmmo(gun);
            pointManager.RemovePoints(gun.ammoPrice);
            buyFeedback.PlayFeedbacks();

            //generalTextBoxUI.SetText("Press " + buttons.interaction + " to buy AMMO for " + gun.gunName + " for " + gun.ammoPrice);
        }
        else
        {
            //Insufficient points for purchase.
            if (pointManager.currentPoints < gun.ammoPrice)
            {
                notEnoughPointsFeedbacks?.TriggerNotEnoughPointsFeedbacks();
            }
            else
            {
                cantBuyFeedback.PlayFeedbacks();
            }

        }
    }

    private void TryToBuyMeleeWeapon()
    {

        if (pointManager.currentPoints >= meleeWeapon.GetPrice() && meleeManager.GetMeleeData() != meleeWeapon)
        {
            weaponSpriteRenderer.material = boughtGunIconMaterial;
            player.GetComponent<MeleeWeaponInventory>().ChangeMeleeWeapon(meleeWeapon);
            OnBuyMeleeWeapon?.Invoke();
            pointManager.RemovePoints(meleeWeapon.GetPrice());
            buyFeedback.PlayFeedbacks();
            HealOnBuy();
        }
        else
        {
            //Insufficient points for purchase.
            if (pointManager.currentPoints < meleeWeapon.GetPrice())
            {
                notEnoughPointsFeedbacks?.TriggerNotEnoughPointsFeedbacks();
            }
            else
            {
                cantBuyFeedback.PlayFeedbacks();
            }
        }
    }

    public void SetGunData(GunData gunData)
    {
        gun = gunData;
    }

    public void SetMeleeData(MeleeData meleeData)
    {
        meleeWeapon = meleeData;
    }

    public enum WeaponType { gun, melee, special }

}
