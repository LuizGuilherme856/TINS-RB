using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "path of power combination", menuName = "Upgrades/Path Combination")]
public class PathOfPowerCombination : ScriptableObject
{
    [SerializeField] private PathsOfPowerNames path1, path2;
    [SerializeField] public UpgradeData combinationUpgrade;

    public bool CheckCombination(PathsOfPowerNames pathToCompare1 , PathsOfPowerNames pathToCompare2)
    {
        if(path1 == pathToCompare1 && path2 == pathToCompare2 ||
           path2 == pathToCompare1 && path1 == pathToCompare2)
        {
            return true;
        }

        return false;
    }



}
