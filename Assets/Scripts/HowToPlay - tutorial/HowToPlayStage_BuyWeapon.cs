using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlayStage_BuyWeapon : HowToPlayStage
{
    [SerializeField] private BuyWeapon buyWeaponStation;

    private void Start()
    {
        buyWeaponStation.OnBuyGun += SetCompleted;
        buyWeaponStation.OnBuyMeleeWeapon += SetCompleted;
    }

}
