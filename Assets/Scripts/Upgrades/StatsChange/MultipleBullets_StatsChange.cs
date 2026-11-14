using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MultipleBullets_StatsChange", menuName = "Upgrades/StatsChange/MultipleBullets")]
public class MultipleBullets_StatsChange : StatsChange
{
    [SerializeField] private VariableOperation<int> IncreaseScatteredBulletsBy;
    [SerializeField] private VariableOperation<int> IncreaseLinedUpBulletsBy;
    [SerializeField] private VariableOperation<float> chanceOfShootingExtraBullet;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        // Scattered-projectile count modifiers.
        globalStats.ChangeIncreaseScatteredBulletsBy(IncreaseScatteredBulletsBy.variable, IncreaseScatteredBulletsBy.operationType);

        // Aligned-projectile count modifiers.
        globalStats.ChangeIncreaseLinedUpBulletsBy(IncreaseLinedUpBulletsBy.variable, IncreaseLinedUpBulletsBy.operationType);

        // Extra-bullet chance modifiers.
        globalStats.ChangeChanceOfShootingExtraBullets(chanceOfShootingExtraBullet.variable, chanceOfShootingExtraBullet.operationType);
    }
}
