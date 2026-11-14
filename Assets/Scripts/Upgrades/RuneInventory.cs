using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

//[CreateAssetMenu(fileName = "rune inventory", menuName = "Rune inventory")]
/// <summary>
/// Stores the player's current runes.
/// </summary>
public class RuneInventory : ScriptableObject
{
    [SerializeField] public GlobalStats globalStats;

    /// <summary>
    /// Most recently purchased rune.
    /// </summary>
    public RuneData lastRuneDataObtained { get; private set; }

    public (PathsOfPowerNames path, int quantity)[] pathsQuantity =
        {(PathsOfPowerNames.strength, 0), (PathsOfPowerNames.equilibrium, 0), (PathsOfPowerNames.focus, 0), (PathsOfPowerNames.time, 0), (PathsOfPowerNames.luck, 0)};

    #region CAMPOS

    [Header("   Basic")]
    //[SerializeField] private int numOfTimesCanBuyTheSameRune = 5;
     
    [SerializeField] private PlayerData playerData;
   
    private List<RuneQuantityClass> runeQuantityClass = new();

    public event Action OnBuyRune;

    public List<RuneQuantityClass> GetUpgradesQuantityClasses() { return runeQuantityClass; }

    #endregion

    public void AddUpgrade(RuneData runeData)
    {
        bool hasThisUpgrade = false;

        foreach (var item in runeQuantityClass.Where(x => x.rune == runeData))
        {
            hasThisUpgrade = true;
            item.quantity += 1;
        }
        if (hasThisUpgrade == false)
        {
            //Debug.Log("Create new upgrade on the list");
            RuneQuantityClass newUpgradeQuantity = new RuneQuantityClass(runeData, 1);
            runeQuantityClass.Add(newUpgradeQuantity);
        }
        lastRuneDataObtained = runeData;

        UpdatePathQuantity(runeData);

        OnBuyRune?.Invoke();

    }

    /// <summary>
    /// Identify the purchased rune's path and increment its count.
    /// </summary>
    private void UpdatePathQuantity(RuneData runeData)
    {
        for (int i = 0; i < pathsQuantity.Length; i++)
        {
            if (pathsQuantity[i].path == runeData.path)
            {
                pathsQuantity[i].quantity++;
                return;
            }
        }
    }

    public void ResetUpgrades()
    {
        runeQuantityClass.Clear();

        for (int i = 0; i < pathsQuantity.Length; i++)
        {
            pathsQuantity[i] = (pathsQuantity[i].path, 0); // Reset the count to zero.
        }
    }


    ///// <summary>
    ///// Break the station after this purchase count.
    ///// </summary>
    //public int GetNumberOfTimesCanUseTheSameStation()
    //{
    //    return numOfTimesCanBuyTheSameRune;
    //}

    #region METODOS GET NUMBER (UPGRADES)

    public int GetUpgradeCount(RuneData upgradeData)
    {
        foreach (var item in runeQuantityClass)
        {
            if (item.rune == upgradeData)
            {
                return item.quantity;
            }
        }
        return 0;
    }

    public int GetPathQuantity(PathsOfPowerNames path)
    {
        return pathsQuantity.Where(x => x.path == path).FirstOrDefault().quantity;
    }



    #endregion
}


/// <summary>
/// Stores each upgrade and its owned quantity
/// in a dedicated serializable type.
/// </summary>
public class RuneQuantityClass
{

    public RuneData rune;
    public int quantity;
    public RuneQuantityClass(RuneData runeData, int quantity)
    {
        rune = runeData;
        this.quantity = quantity;
    }

    public void SetQuantity(int quantity) { this.quantity = quantity; }

}
