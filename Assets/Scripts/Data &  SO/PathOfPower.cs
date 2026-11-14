using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the effects provided by this path.
/// For the luck path, owning the required number of luck runes applies an additional rune effect.
/// </summary>

 //[CreateAssetMenu(fileName = "path of power", menuName = "Upgrades/Path of power")]
public class PathOfPower : GenericData
{
    [Space(25)]
    [SerializeField] public PathsOfPowerNames path;
    [Tooltip("Caminhos irmãos se conectam e dão bônus único")]
    [SerializeField] public PathsOfPowerNames leftBrother, rightBrother;
    //Apply effects as if a rune had been purchased.
    [SerializeField] protected UpgradeData upgrade, upgrade5th;


    //Maximum number of bonus applications.
    //[SerializeField] private int maxBonuses = 10;

    /// <summary>
    /// Return the upgrade corresponding to the number of runes owned for this path.
    /// </summary>
    public virtual UpgradeData GetUpgradeData(int quantity)
    {
        //TODO: Revisit this simplified implementation.
        return upgrade;

        //if (quantity % 5 == 0)
        //{
        //    return upgrade5th;
        //}
        //else
        //{
        //    return upgrade;
        //}
    }

}
