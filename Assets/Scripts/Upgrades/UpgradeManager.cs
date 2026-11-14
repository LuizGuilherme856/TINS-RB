using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TechBooh;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// High-level coordinator for upgrades and abilities.
/// Apply path, rune, skill, item, and character modifiers to GlobalStats.
/// Pass GlobalStats to RuneData when applying rune-specific changes.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [field: SerializeField] public PlayerHolder playerHolder;
    [SerializeField] private GlobalStats globalStats;
    [SerializeField] private RuneInventory runeInventory;

    public OrbitalUpgradeManager orbitUpgrade;

    //Action Related
    //Raised when runes, inventory, blessings, or other sources update stats.
    public event Action OnChangeStats;
    public GunInventory gunInventory;

    private void Awake()
    {
        if (orbitUpgrade == null)
        {
            orbitUpgrade = FindFirstObjectByType<OrbitalUpgradeManager>();
        }
        gunInventory = playerHolder.GetGunInventory();
    }
    void Start()
    {
        runeInventory.OnBuyRune += HandleOnBuyRune;
    }

    private void OnDisable()
    {
        runeInventory.OnBuyRune -= HandleOnBuyRune;
    }

    /// <summary>
    /// When a rune is purchased, start evaluating its requirements
    /// and apply its effect once they are satisfied.
    /// </summary>
    private void HandleOnBuyRune()
    {
        // TechBooh:
        // RuneInventory.AddUpgrade raises OnBuyRune after adding the rune.
        // The count is therefore already 1 on the first purchase. Comparing it to zero
        // incorrectly classified every purchase as subsequent and broke cumulative effects.
        // or different behavior on first and subsequent purchases.
        bool isFirstOfItsType = runeInventory.GetUpgradeCount(runeInventory.lastRuneDataObtained) == 1;

        runeInventory.lastRuneDataObtained.ExecuteUpgradeData(this, isFirstOfItsType);

        OnChangeStats?.Invoke();
    }

    /// <summary>
    /// Apply a non-rune upgrade through the rune-effect mechanism.
    /// Used by Paths of Power.
    /// </summary>
    public void ExecuteUpgrade(UpgradeData upgradeData)
    {
        try
        {
            upgradeData.ExecuteUpgradeData(this, true);
        }
        catch
        {
            Debug.LogError("Erro ao executar o upgradeData");
        }

    }

    public OrbitalUpgradeManager GetOrbitalUpgradeManager()
    {
        if (orbitUpgrade == null)
        {
            orbitUpgrade = FindFirstObjectByType<OrbitalUpgradeManager>();
        }

        return orbitUpgrade;
    }


}
