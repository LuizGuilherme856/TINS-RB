using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ElementalDamage_StatsChange", menuName = "Upgrades/StatsChange/ElementalDamage")]
public class ElementalDamage_StatsChange : StatsChange
{
    [Header("Alterações de stats relacionadas ao dano elemental.")]
    [Space(30)]

    [SerializeField] private VariableOperation<float> fireDamageMultiplier;
    [SerializeField] private VariableOperation<float> iceDamageMultiplier;
    [SerializeField] private VariableOperation<float> poisonDamageMultiplier;
    [SerializeField] private VariableOperation<float> waterDamageMultiplier;
    [SerializeField] private VariableOperation<float> electricityDamageMultiplier;
    [SerializeField] private VariableOperation<float> etherDamageMultiplier;
    [SerializeField] private VariableOperation<float> impactDamageMultiplier;
    [SerializeField] private VariableOperation<float> punctureDamageMultiplier;
    [SerializeField] private VariableOperation<float> slashDamageMultiplier;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeFireDamageMultiplier(fireDamageMultiplier.variable, fireDamageMultiplier.operationType);
        globalStats.ChangeIceDamageMultiplier(iceDamageMultiplier.variable, iceDamageMultiplier.operationType);
        globalStats.ChangePoisonDamageMultiplier(poisonDamageMultiplier.variable, poisonDamageMultiplier.operationType);
        globalStats.ChangeWaterDamageMultiplier(waterDamageMultiplier.variable, waterDamageMultiplier.operationType);
        globalStats.ChangeElectricityDamageMultiplier(electricityDamageMultiplier.variable, electricityDamageMultiplier.operationType);
        globalStats.ChangeEtherDamageMultiplier(etherDamageMultiplier.variable, etherDamageMultiplier.operationType);
        globalStats.ChangeImpactDamageMultiplier(impactDamageMultiplier.variable, impactDamageMultiplier.operationType);
        globalStats.ChangePunctureDamageMultiplier(punctureDamageMultiplier.variable, punctureDamageMultiplier.operationType);
        globalStats.ChangeSlashDamageMultiplier(slashDamageMultiplier.variable, slashDamageMultiplier.operationType);
    }
}
