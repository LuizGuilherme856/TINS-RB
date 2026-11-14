using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Track purchased upgrades and their counts.
public class UpgradesCounter : MonoBehaviour
{
    /*
    private List<UpgradeQuantityClass> upgradesQuantityClass = new List<UpgradeQuantityClass>();

    public void AddUpgrade(UpgradeData upgradeData)
    {
        bool hasThisUpgrade = false;
       
        foreach (var item in upgradesQuantityClass.Where(x => x.upgrade == upgradeData))
        {
            hasThisUpgrade = true;
            item.quantity += 1;
        }
        if(hasThisUpgrade == false)
        {
            //Debug.Log("Create new upgrade on the list");
            UpgradeQuantityClass newUpgradeQuantity = new UpgradeQuantityClass(upgradeData, 1);
            upgradesQuantityClass.Add(newUpgradeQuantity);
        }
        
    }

    public List<UpgradeQuantityClass> GetUpgradesQuantityClasses() { return upgradesQuantityClass; }

    public int GetUpgradeCount( UpgradeData upgradeData) 
    {
        foreach (var item in upgradesQuantityClass)
        {
            if(item.upgrade == upgradeData)
            {
                return item.quantity;
            }
        }
        return 0;
    }
    */

}
/*
public class UpgradeQuantityClass
{

    public UpgradeData upgrade;
    public int quantity;
    public UpgradeQuantityClass(UpgradeData upgradeData, int quantity)
    {
        upgrade= upgradeData;
        this.quantity = quantity;
    }

    public void SetQuantity(int quantity) { this.quantity = quantity; }
}
*/