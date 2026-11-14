using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathOfPowerManager : MonoBehaviour
{
    [SerializeField] private PathOfPower equilibriumPath, strengthPath, luckPath, timePath, focusPath;
    [SerializeField] private RuneInventory runeInventory;
    [SerializeField] private PathOfPowerCombination[] combinations;
    private UpgradeManager upgradeManager;

    private void Awake()
    {
        upgradeManager = GetComponent<UpgradeManager>();    
    }

    #region On Enable/Disable
    void OnEnable()
    {
        runeInventory.OnBuyRune += ApplyPath; 
    }

    private void OnDisable()
    {
        runeInventory.OnBuyRune -= ApplyPath;
    }
    #endregion

    private void ApplyPath()
    {
        PathsOfPowerNames lastPathName = runeInventory.lastRuneDataObtained.path;
        PathOfPower lastPath = null;
        int pathQuantity = runeInventory.GetPathQuantity(lastPathName);

        switch (lastPathName)
        {
            case PathsOfPowerNames.equilibrium:
                lastPath = equilibriumPath;
                upgradeManager.ExecuteUpgrade(equilibriumPath.GetUpgradeData(pathQuantity));
                break;
            case PathsOfPowerNames.strength:
                lastPath = strengthPath;
                upgradeManager.ExecuteUpgrade(strengthPath.GetUpgradeData(pathQuantity));
                break;
            case PathsOfPowerNames.luck:
                lastPath = luckPath;
                upgradeManager.ExecuteUpgrade(luckPath.GetUpgradeData(pathQuantity));
                break;
            case PathsOfPowerNames.time:
                lastPath = timePath;
                upgradeManager.ExecuteUpgrade(timePath.GetUpgradeData(pathQuantity));
                break;
            case PathsOfPowerNames.focus:
                lastPath = focusPath;
                upgradeManager.ExecuteUpgrade(focusPath.GetUpgradeData(pathQuantity));
                break;
            default:
                Debug.LogError("Ocorreu um erro ao executar os upgrades do path of power");
                break;
        }



        int leftBrotherQuantity = runeInventory.GetPathQuantity(lastPath.leftBrother);
        int rightBrotherQuantity = runeInventory.GetPathQuantity(lastPath.rightBrother);
        //A new combination requires the current path count to be
        //a multiple of five and a sibling path count greater than five.
        if ((pathQuantity % 5 == 0) && (leftBrotherQuantity >= 5 || rightBrotherQuantity >= 5 ))
        {
            //ApplyPathCombination(lastPath, leftBrotherQuantity, rightBrotherQuantity);
        }


    }

    private void ApplyPathCombination(PathOfPower lastPath, int leftBrotherQuantity, int rightBrotherQuantity)
    {

        //Find the matching PathCombinations.
        PathOfPowerCombination leftCombination = null, rightCombination = null;

        foreach (PathOfPowerCombination combinationItem in combinations)
        {
            if(leftBrotherQuantity >= 5 &&  combinationItem.CheckCombination(lastPath.path, lastPath.leftBrother))
            {
                leftCombination = combinationItem;
            }
            else if(rightBrotherQuantity >= 5 && combinationItem.CheckCombination(lastPath.path, lastPath.rightBrother))
            {
                rightCombination = combinationItem;
            }
        }

        if(leftCombination != null)
        {
            upgradeManager.ExecuteUpgrade(leftCombination.combinationUpgrade);
        }

        if(rightCombination != null)
        {
            upgradeManager.ExecuteUpgrade(rightCombination.combinationUpgrade);
        }

    }
}
