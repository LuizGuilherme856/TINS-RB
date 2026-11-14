using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Prices_StatsChange", menuName = "Upgrades/StatsChange/Prices")]
public class Prices_StatsChange : StatsChange
{
    [Header("Alterações de stats relacionadas aos preços.")]
    [Space(30)]

    [SerializeField] private VariableOperation<float> doorPriceMultiplier;
    [SerializeField] private VariableOperation<float> meleeWeaponPriceMultiplier;
    [SerializeField] private VariableOperation<float> gunUpgradePriceMultiplier;
    [SerializeField] private VariableOperation<float> runePriceMultiplier;
    [SerializeField] private VariableOperation<float> gunPriceMultiplier;
    [SerializeField] private VariableOperation<float> globalPriceMultiplier;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeDoorPriceMultiplier(doorPriceMultiplier.variable, doorPriceMultiplier.operationType);
        globalStats.ChangeMeleeWeaponPriceMultiplier(meleeWeaponPriceMultiplier.variable, meleeWeaponPriceMultiplier.operationType);
        globalStats.ChangeGunUpgradePriceMultiplier(gunUpgradePriceMultiplier.variable, gunUpgradePriceMultiplier.operationType);
        globalStats.ChangeRunePriceMultiplier(runePriceMultiplier.variable, runePriceMultiplier.operationType);
        globalStats.ChangeGunPriceMultiplier(gunPriceMultiplier.variable, gunPriceMultiplier.operationType);
        globalStats.ChangeGlobalPriceMultiplier(globalPriceMultiplier.variable, globalPriceMultiplier.operationType);
    }
}
