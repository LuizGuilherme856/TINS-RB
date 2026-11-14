using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlayStage_BuyUpgrade : HowToPlayStage
{
    [SerializeField] private BuyRune upgradeStationToObserve;

    private void Start()
    {
        upgradeStationToObserve.OnBuyUpgrade += SetCompleted;
    }
}
