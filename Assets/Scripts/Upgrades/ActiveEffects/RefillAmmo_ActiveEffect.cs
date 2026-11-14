using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Refill Ammo_ActiveEffect", menuName = "Upgrades/ActiveEffects/Refill Ammo")]
public class RefillAmmo_ActiveEffect : ActiveEffect
{
    [SerializeField] private guns gunsToRefillAmmo;
    private enum guns { currentGun, allGuns}

    public override void TriggerActiveEffect(UpgradeManager upgradeManager, GlobalStats globalStats)
    {
        switch (gunsToRefillAmmo)
        {
            case guns.currentGun:
                upgradeManager.gunInventory.RefillAmmoCurrentGun();
                break;
            case guns.allGuns:
                upgradeManager.gunInventory.RefillAmmoAllGuns();
                break;
            default:
                break;
        }
    }


}
