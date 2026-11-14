using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles upgrades that do not fit another specialized component.
/// </summary>
public class GenericUpgradeHandler : MonoBehaviour
{
    [SerializeField] private RuneInventory runeUpgradeManager;
    [SerializeField] private MMF_Player extraHealthFeedbacks;
    [SerializeField] private LanguageData languageData;
    [SerializeField] private Shield_PowerUp shield_PowerUp;

    /// <summary>
    /// Only the GameObject name is needed here.
    /// </summary>
    [SerializeField] private GameObject InfiniteBladeGameObject;

    public bool canAutoHeal = true; //Whether automatic healing is allowed.

    private Health health;
    private MMF_FloatingText ignoreDamageText;
    private OrbitalUpgradeManager orbitalUpgradeManager;

    private int triesToGetAnExtraHealth = 0;
    private int maxTriesToGetAnExtraHealth = 20; //Maximum failed healing attempts before a heal is guaranteed.

    [SerializeField] private GlobalStats globalStats;
    private void Awake()
    {
        health = GetComponent<Health>();

        orbitalUpgradeManager = FindFirstObjectByType<OrbitalUpgradeManager>();

        if(globalStats == null)
        {
            Debug.LogWarning("global stats estava nulo: " + name);
            globalStats = AssetFinder.GetGlobalStats();
        }
    }

    void Start()
    {
        InvokeRepeating(nameof(TryToGetAnExtraHealth), 30, 30);
        ignoreDamageText = extraHealthFeedbacks.GetFeedbackOfType<MMF_FloatingText>();
    }

    private void OnEnable()
    {
        health.OnTakeDamage += TryToActivateShield;
        health.OnTakeDamage += TryToDestroyInfiniteBlades;
    }

    /// <summary>
    /// Infinite Blades loses one blade per hit, down to a minimum of one.
    /// </summary>
    private void TryToDestroyInfiniteBlades()
    {
        if(orbitalUpgradeManager.CountOrbitObjects(InfiniteBladeGameObject) > 1)
        {
            //Remove an Infinite Blade when taking damage.
            orbitalUpgradeManager.RemoveSpecificObjectFromOrbit(InfiniteBladeGameObject, 1);

            //Also remove it from the inventory used during scene changes
            //so a lost blade does not respawn in the next scene.
            foreach (var orbitalUpgradesItem in globalStats.orbitalUpgrades)
            {
                if(orbitalUpgradesItem.meleeDataToOrbit.meleeGameObject.Equals(InfiniteBladeGameObject))
                {
                    globalStats.orbitalUpgrades.Remove(orbitalUpgradesItem);
                    break;
                }
            }
        }

    }

    private void TryToGetAnExtraHealth()
    {
        //Prevent idle healing in safe zones.
        if (!canAutoHeal || globalStats.ChanceOfGain1HpEvery30s <= 0) return;

         triesToGetAnExtraHealth++;

        if ( Random.Range(0f, 1f) > globalStats.ChanceOfGain1HpEvery30s && triesToGetAnExtraHealth < maxTriesToGetAnExtraHealth)
        {
            return;
        }

        switch (languageData.currentLanguage)
        {
            case Language.english:
                ignoreDamageText.Value = "+1 HP";
                break;
            case Language.chinese:
                ignoreDamageText.Value = "+1 血量";
                break;
            case Language.portuguese:
                ignoreDamageText.Value = "+1 HP";
                break;
            case Language.spanish:
                ignoreDamageText.Value = "+1 HP";
                break;
            default:
                ignoreDamageText.Value = "+1 HP";
                break;
        }

        health.ReplenishHealth(1);
        extraHealthFeedbacks.PlayFeedbacks();

        triesToGetAnExtraHealth = 0; //Reset the attempt counter.
    }

    private void TryToActivateShield()
    {
        float duration = globalStats.ShieldDurationOnTakingDamage;

        if (duration <= 0) return;
        shield_PowerUp.ActivateShield(duration);

    }
}
