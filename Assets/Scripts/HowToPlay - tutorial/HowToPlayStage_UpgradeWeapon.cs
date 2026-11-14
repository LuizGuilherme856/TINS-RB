using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlayStage_UpgradeWeapon : HowToPlayStage
{
    [SerializeField] private UpgradeWeapon upgradeWeapon;

    private void Start()
    {
        upgradeWeapon.OnUpgrade += SetCompleted;
    }

}
