using UnityEngine;

[CreateAssetMenu(fileName = "OrbitalBlades_StatsChange", menuName = "Upgrades/StatsChange/OrbitalBladeUpgrade")]
public class OrbitalBlade_StatsChange : StatsChange
{

    [SerializeField] private VariableOperation<float> OrbitalBladeDamageMultiplier;
    [Space(10)][SerializeField] private VariableOperation<int> IncreaseOrbitalBladeDamage;
    [Space(10)][SerializeField] private VariableOperation<float> IncreaseOrbitalSpeed;
    [Space(10)][SerializeField] private VariableOperation<float> OrbitalSpeedMultiplier;
    [Space(10)][SerializeField] private VariableOperation<float> OrbitalBladeInstaKillChance;
    [Space(10)][SerializeField] private VariableOperation<float> OrbitalRadiusMultiplier;
    [Space(10)][SerializeField] private VariableOperation<float> IncreaseOrbitalCorrectionSpeed;
    [Space(10)][SerializeField] private VariableOperation<float> OrbitalCorrectionSpeedMultiplier;


    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeIncreaseOrbitalBladeDamageBy(IncreaseOrbitalBladeDamage.variable, IncreaseOrbitalBladeDamage.operationType);
        globalStats.ChangeOrbitalBladeDamage_Multiplier(OrbitalBladeDamageMultiplier.variable, OrbitalBladeDamageMultiplier.operationType);

        globalStats.ChangeIncreaseOrbitalSpeedBy(IncreaseOrbitalSpeed.variable, IncreaseOrbitalSpeed.operationType);
        globalStats.ChangeOrbitalSpeed_Multiplier(OrbitalSpeedMultiplier.variable, OrbitalSpeedMultiplier.operationType);

        globalStats.ChangeOrbitalBladeInstaKillChance(OrbitalBladeInstaKillChance.variable, OrbitalBladeInstaKillChance.operationType);
        globalStats.ChangeOrbitalRadiusMultiplier(OrbitalRadiusMultiplier.variable, OrbitalRadiusMultiplier.operationType);

        globalStats.ChangeIncreaseOrbitalCorrectionSpeedBy(IncreaseOrbitalCorrectionSpeed.variable, IncreaseOrbitalCorrectionSpeed.operationType);
        globalStats.ChangeOrbitalCorrectionSpeed_Multiplier(OrbitalCorrectionSpeedMultiplier.variable, OrbitalCorrectionSpeedMultiplier.operationType);
    }



}
