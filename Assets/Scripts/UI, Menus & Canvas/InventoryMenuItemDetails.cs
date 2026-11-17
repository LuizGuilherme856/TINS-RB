using Kamgam.SettingsGenerator;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// Update the side panel with the item's name, icon, and description.
/// </summary>
public class InventoryMenuItemDetails : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameTxt, itemDescriptionTxt;
    [SerializeField] private RuneInventory runeInventory;

    /// <summary>
    /// Objects displaying weapon damage, ammunition, critical chance, and other stats.
    /// </summary>
    [SerializeField] private List<WeaponStatsUI> weaponStatsUI, meleeWeaponStatsUI;

    [SerializeField] private GameObject weaponStatsObj, meleeWeaponStatsObj;

    [SerializeField] private StatsCalculator statsCalculator;
    public LanguageData LanguageData;

    private void OnEnable()
    {
        if(statsCalculator == null)
        {
            Debug.LogWarning($"Nao foi encontrado statsCalculator no objeto {gameObject.name}. Procurando manualmente.");
            statsCalculator = AssetFinder.GetStatsCalculator();
        }

        if (runeInventory == null)
        {
            Debug.LogWarning($"Nao foi encontrado runeUpgradeManager no objeto {gameObject.name}. Procurando manualmente.");
            runeInventory = AssetFinder.GetRuneInventory();
        }
    }

    /// <summary>
    /// Shared setup for rune, gun, and melee icons.
    /// Gun and melee wrappers call this before applying specialized settings.
    /// </summary>
    public void SetGenericDataDetails(Sprite itemIcon, string itemName, string itemDescription)
    {
        this.itemIcon.sprite = itemIcon;
        itemNameTxt.text = itemName;
        itemDescriptionTxt.text = itemDescription;

    }



    public void SetMeleeWeaponDetails(Sprite itemIcon, string itemName, string itemDescription, MeleeData meleeData)
    {
        weaponStatsObj.SetActive(false);
        meleeWeaponStatsObj.SetActive(true);
        SetGenericDataDetails(itemIcon, itemName, itemDescription);

        foreach (WeaponStatsUI statsItem in meleeWeaponStatsUI)
        {
            string infoText = "", infoTextUpgraded = "";
            float defaultValue;
            float value;

            switch (statsItem.MeleeWeaponStats)
            {
                case MeleeWeaponStats.damage:
                    defaultValue = meleeData.damage;
                    value = statsCalculator.CalculateNewMeleeDamage(meleeData.damage, meleeData.damageType);
                    infoText = ValueComparison(defaultValue, value);
                    break;
                case MeleeWeaponStats.criticalChance:

                    defaultValue = meleeData.criticalChance;
                    value = statsCalculator.CalculateNewMeleeCriticalChance(meleeData.criticalChance);
                    infoText = ValueComparison(defaultValue, value, true, 2, " %");

                    break;
                case MeleeWeaponStats.criticalMultiplier:
                    defaultValue = meleeData.criticalMultiplier;
                    value = statsCalculator.CalculateNewMeleeCriticalMultiplier(meleeData.criticalMultiplier);
                    infoText = ValueComparison(defaultValue, value, true, 1, "x");
                    
                    break;
                case MeleeWeaponStats.damageType:
                    string color = GetDamageTypeColor(meleeData);
                    infoText = color + meleeData.GetDamageTypeName(LanguageData.currentLanguage);
                    break;
                case MeleeWeaponStats.attackRate:
                    defaultValue = meleeData.attackRate;
                    value = statsCalculator.CalculateMaxTimeCoolDown(meleeData.attackRate, PlayerAction.melee);
                    infoText = ValueComparison(defaultValue, value);
                    break;
                case MeleeWeaponStats.knockBackForce:
                    defaultValue = meleeData.knockBackForce;
                    infoText = ValueComparison(defaultValue, defaultValue); //The value is unchanged, but its color still needs updating.
                    break;
                case MeleeWeaponStats.throwSpeed:
                    defaultValue = meleeData.flySpeed;
                    infoText = ValueComparison(defaultValue, defaultValue); //The value is unchanged, but its color still needs updating.
                    break;
                case MeleeWeaponStats.playerImpulse:
                    defaultValue = meleeData.playerImpulse;
                    infoText = ValueComparison(defaultValue, defaultValue); //The value is unchanged, but its color still needs updating.
                    break;
                default:
                    break;
            }

            statsItem.SetInfoText(infoText);
            statsItem.SetInfoTextUpgraded(infoTextUpgraded);

        }
    }

    public void SetItemDetails(Sprite itemIcon, string itemName, string itemDescription) //Overload for items other than guns and melee weapons.
    {
        weaponStatsObj.SetActive(false);
        meleeWeaponStatsObj.SetActive(false);
        SetGenericDataDetails(itemIcon, itemName, itemDescription);

    }

    public void SetWeaponDetails(Sprite itemIcon, string itemName, string itemDescription, GunData gunData)
    {
        weaponStatsObj.SetActive(true);
        meleeWeaponStatsObj.SetActive(false);
        SetGenericDataDetails(itemIcon, itemName, itemDescription);

        foreach (WeaponStatsUI statsItem in weaponStatsUI)
        {
            string infoText = "", infoTextUpgraded = "";
            string color;
            float defaultValue;
            float value;
            switch (statsItem.WeaponStats)
            {
                case WeaponStats.damage:

                     defaultValue = gunData.GetDamage(true);
                     value = gunData.GetDamage();

                    infoText = ValueComparison(defaultValue, value);
                    break;
                case WeaponStats.criticalChance:
   
                    defaultValue = gunData.criticalChance;
                    value = statsCalculator.CalculateNewGunCriticalChance(gunData.criticalChance);

                    infoText = ValueComparison(defaultValue, value, true, 2, " %");

                    break;
                case WeaponStats.criticalMultiplier:

                    defaultValue = gunData.criticalMultiplier;
                    value = statsCalculator.CalculateNewGunCriticalMultiplier(gunData.criticalMultiplier);//runeUpgradeManager.CalculateGunCriticalMultiplier(gunData.criticalMultiplier);

                    infoText = ValueComparison(defaultValue, value, true, 1, "x");

                    break;
                case WeaponStats.damageType:
                    color = GetDamageTypeColor(gunData);
                    infoText = color + gunData.GetDamageTypeName(LanguageData.currentLanguage);
                    break;
                case WeaponStats.fireRate:

                    defaultValue = gunData.fireRate;
                    value = gunData.fireRate;

                    infoText = ValueComparison(defaultValue, value);

                    break;
                case WeaponStats.gunType:
                    infoText = gunData.gunType.ToString();
                    break;
                case WeaponStats.reloadTime:

                    defaultValue = gunData.reloadTime;
                    value = statsCalculator.CalculateNewReloadTime(gunData.reloadTime);

                    infoText = ValueComparison(defaultValue, value, false, 2, " s");

                    break;
                case WeaponStats.StatusEffect:
                    color = GetStatusEffectColor(gunData);
                    infoText = color +  gunData.GetStatusEffectName( LanguageData.currentLanguage, Damage2.GetStatusEffect(gunData.damageType));
                    break;
                case WeaponStats.statusChance:
                    color = GetStatusEffectColor(gunData);
                    infoText = color + Aux.FormatNumber(gunData.GetStatusChance(defaultValue: true) * 100f) + "%";
                    DisableIfNoStatusEffect(gunData, statsItem);

                    break;
                case WeaponStats.statusDuration:
                    color = GetStatusEffectColor(gunData);
                    infoText = color + Aux.FormatNumber(gunData.statusDuration) + " s";
                    DisableIfNoStatusEffect(gunData, statsItem);
                    break;
                case WeaponStats.statusDamagePerSec:
                    color = GetStatusEffectColor(gunData);
                    infoText = color + Aux.FormatNumber(gunData.statusDamagePerSec);
                    DisableIfNoStatusEffect(gunData, statsItem);
                    break;
                case WeaponStats.ammo:

                    defaultValue = gunData.GetMaxAmmoOnClip(true);
                    value = gunData.GetMaxAmmoOnClip();

                    infoText = ValueComparison(defaultValue, value);

                    break;
                case WeaponStats.maxSpareAmmo:

                    defaultValue = gunData.GetMaxSpareAmmo(true);
                    value = gunData.GetMaxSpareAmmo();

                    infoText = ValueComparison(defaultValue, value);

                    break;
                case WeaponStats.ammoConsumedPerShot:
                    defaultValue = gunData.GetAmmoConsumedPerShot(true);
                    value = gunData.GetAmmoConsumedPerShot();

                    infoText = ValueComparison(defaultValue, value, false);

                    break;
                case WeaponStats.bulletSpeed:
                    infoText = Aux.FormatNumber(gunData.bulletSpeed);
                    break;
                case WeaponStats.bulletsPerShot:
                    defaultValue = gunData.GetScatteredBulletsPerShot(true);
                    value = gunData.GetScatteredBulletsPerShot();

                    infoText = ValueComparison(defaultValue, value);

                    break;
                case WeaponStats.recoilForce:
                    infoText = Aux.FormatNumber(gunData.recoilForceOnPlayer);
                    break;
                case WeaponStats.imprecision:
                    infoText = gunData.imprecision.ToString();
                    break;
                default:
                    break;
            }

            statsItem.SetInfoText(infoText);
            statsItem.SetInfoTextUpgraded(infoTextUpgraded);
        }


    }

    /// <summary>
    /// Compare original and upgraded values and color improvements or reductions accordingly.
    /// For example, damage 10 to 15 produces "10 > 15" with 15 in green.
    /// </summary>
    /// <param name="decimalPlaces">Number of displayed decimal places.</param>
    /// <param name="extraSymbol">Optional suffix,
    /// for example % for a percentage.</param>
    /// <returns></returns>
    private static string ValueComparison(float defaultValue, float value, bool biggerIsBetter = true, int decimalPlaces = 1, string extraSymbol = "")
    {
        string newValue = "<color=#577277>" + Aux.FormatNumber(defaultValue) + extraSymbol + "</color>";

        if(biggerIsBetter)
        {
            if (defaultValue > value) //The value decreased.
            {
                newValue += " > <color=#b10e25>" + Aux.FormatNumber(value, decimalPlaces) + extraSymbol + "</color>";
            }
            else if (defaultValue < value) //The value increased.
            {
                newValue += " > <color=#14d233>" + Aux.FormatNumber(value, decimalPlaces) + extraSymbol + "</color>";
            }
        }
        else
        {
            if (defaultValue > value) //The value decreased.
            {
                newValue += " > <color=#233>" + Aux.FormatNumber(value, decimalPlaces) + extraSymbol + "</color>";
            }
            else if (defaultValue < value) //The value increased.
            {
                newValue += " > <color=#b10e25>" + Aux.FormatNumber(value, decimalPlaces) + extraSymbol + "</color>";
            }
        }



        return newValue;
    }

    private static string GetDamageTypeColor(GunData gunData)
    {
        switch (gunData.damageType)
        {
            case DamageType.Slash:
                return "<color=#C60015>";
            case DamageType.Impact:
                return "";
            case DamageType.Explosive:
                return "<color=#FFDD00>";
            case DamageType.Fire:
               return "<color=#FF4B00>";
            case DamageType.Ice:
                return "<color=#00E8FF>";
            case DamageType.Poison:
                return "<color=#7AFF00>";
            default:
                return "";
        }
    }

    private static string GetDamageTypeColor(MeleeData meleeData)
    {
        switch (meleeData.damageType)
        {
            case DamageType.Slash:
                return "<color=#C60015>";
            case DamageType.Impact:
                return "";
            case DamageType.Explosive:
                return "<color=#FFDD00>";
            case DamageType.Fire:
                return "<color=#FF4B00>";
            case DamageType.Ice:
                return "<color=#00E8FF>";
            case DamageType.Poison:
                return "<color=#7AFF00>";
            default:
                return "";
        }
    }

    private static string GetStatusEffectColor(GunData gunData)
    {
        string color = string.Empty;

        switch (Damage2.GetStatusEffect( gunData.damageType))
        {
            case StatusEffect.Punctured:
                break;
            case StatusEffect.Bleeding:
                break;
            case StatusEffect.Concussion:
                break;
            case StatusEffect.Exploded:
                break;
            case StatusEffect.Burning:
                color = "<color=#FF4B00>";
                break;
            case StatusEffect.Frozen:
                color = "<color=#00E8FF>";
                break;
            case StatusEffect.Poisoned:
                color = "<color=#7AFF00>";
                break;
            case StatusEffect.Soaked:
                break;
            case StatusEffect.Electrocuted:
                break;
            case StatusEffect.None:
                break;
            default:
                break;
        }



            return color;
    }

    private static void DisableIfNoStatusEffect(GunData gunData, WeaponStatsUI statsItem)
    {
        //Hide status chance when no status effect exists.
        if (Damage2.GetStatusEffect(gunData.damageType) != StatusEffect.None)
        {
            statsItem.gameObject.SetActive(true);
        }
        else
        {
            statsItem.gameObject.SetActive(false);
        }
    }
}
