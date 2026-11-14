using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Rune", menuName = "Upgrades/Rune")]
public class RuneData : UpgradeData
{
    [Space(30)]
    [Header("       #--- Rune Info ---#")]
    public Rarity rarity = Rarity.Common; //Rune rarity affects drop probability.
    public int price = 100;
    public int maxPrice = 1000;
    public float priceMultiplier = 1f; //every time you buy the upgrade it gets more expensive
    public Sprite smallerIcon;
    public Sprite inventoryIcon;
    public PathsOfPowerNames path;
    public int numberOfTimesCanBeBought = 4;


}

[Serializable]
public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
