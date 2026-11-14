using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player_StatsChange", menuName = "Upgrades/StatsChange/PlayerStats")]
public class Player_StatsChange : StatsChange
{
    [Space(10)][SerializeField] private VariableOperation<float> IncreaseChanceOfIgnoreDamageBy;
    [Space(10)][SerializeField] private VariableOperation<float> ChanceOfIgnoreDamage_Multiplier;
    [Space(10)][SerializeField] private VariableOperation<float> IncreaseWeaponCapacityBy;
    [Space(10)][SerializeField] private VariableOperation<float> ChanceOfGain1HpEvery30s;
    [Space(10)][SerializeField] private VariableOperation<float> ShieldDurationOnTakingDamage;

    [Space(10)][SerializeField] private VariableOperation<float> playerWalkSpeedMultiplier;
    [Space(10)][SerializeField] private VariableOperation<int> AddToPlayerInitialHealth;
    //[Space(10)][SerializeField] private VariableOperation<int> IncreasePlayerMaxHealthBy;
    [Space(10)][SerializeField] private VariableOperation<int> healthPointsToHealOnBuyWeapon;

    [Space(10)][SerializeField] private VariableOperation<float> increaseWeaponCapacityBy;


    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeIncreaseChanceOfIgnoreDamageBy(IncreaseChanceOfIgnoreDamageBy.variable, IncreaseChanceOfIgnoreDamageBy.operationType);
        globalStats.ChangeOrbitalBladeDamage_Multiplier(ChanceOfIgnoreDamage_Multiplier.variable, ChanceOfIgnoreDamage_Multiplier.operationType);
        globalStats.ChangeIncreaseWeaponCapacityBy(IncreaseWeaponCapacityBy.variable, IncreaseWeaponCapacityBy.operationType);
        globalStats.ChangeChanceOfGain1HpEvery30s(ChanceOfGain1HpEvery30s.variable, ChanceOfGain1HpEvery30s.operationType);
        globalStats.ChangeShieldDurationOnTakingDamage(ShieldDurationOnTakingDamage.variable, ShieldDurationOnTakingDamage.operationType);


        globalStats.ChangePlayerWalkSpeedMultiplier(playerWalkSpeedMultiplier.variable, playerWalkSpeedMultiplier.operationType);
        globalStats.ChangeAddToPlayerInitialHealth(AddToPlayerInitialHealth.variable, AddToPlayerInitialHealth.operationType);
       // globalStats.ChangeIncreasePlayerMaxHealthBy(IncreasePlayerMaxHealthBy.variable, IncreasePlayerMaxHealthBy.operationType);
        globalStats.ChangeHealthPointsToHealOnBuyWeapon(healthPointsToHealOnBuyWeapon.variable, healthPointsToHealOnBuyWeapon.operationType);
        globalStats.ChangeIncreaseWeaponCapacityBy(increaseWeaponCapacityBy.variable, increaseWeaponCapacityBy.operationType);


    }

}
