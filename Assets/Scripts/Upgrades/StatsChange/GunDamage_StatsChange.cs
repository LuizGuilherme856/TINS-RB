using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GunDamage_StatsChange", menuName = "Upgrades/StatsChange/GunDamage")]
public class GunDamage_StatsChange : StatsChange
{
    [Header("Altera��es de stats relacionadas ao dano de armas de fogo.\n" +
        "Valores aceitos s�o entre 0 e 10, ou seja 0% e 1000%.\n" +
        "Valores menores que 1 diminuem o dano/chance da arma.\n" +
        "0 a arma n�o causa mais dano/chance e \n" +
        "maiores que 1 aumentam o dano/chance da arma")]
    [Space(20)]

    [Tooltip("Aumenta/Diminui o dano das armas de fogo")]
    [SerializeField] private VariableOperation<float> IncreaseGunDamageBy;
    [SerializeField] private VariableOperation<float> gunDamageMultiplier;

    [Tooltip("Aumenta/Diminui o dano dos acertos cr�ticos das armas de fogo")]
    [SerializeField] private VariableOperation<float> GunCriticalDamageMultiplier;

    [Tooltip("Aumenta/Diminui a chance de cr�tico das armas de fogo")]
    [SerializeField] private VariableOperation<float> GunCriticalChanceMultiplier;

    [Tooltip("Dano dobrado na última bala")]
    [SerializeField] private VariableOperation<bool> doubleDamageOnLastBullet;



    /// <summary>
    /// Apply stat modifications to GlobalStats.
    /// </summary>
    /// <param name="globalStats">ScriptableObject storing shared stat modifiers.</param>
    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        // Debug.Log("Applying weapon damage stat changes");

        globalStats.ChangeIncreaseGunDamageBy(IncreaseGunDamageBy.variable, IncreaseGunDamageBy.operationType);
        globalStats.ChangeGunDamageMultiplier(gunDamageMultiplier.variable, gunDamageMultiplier.operationType);
        globalStats.ChangeGunCriticalDamageMultiplier(GunCriticalDamageMultiplier.variable, GunCriticalDamageMultiplier.operationType);
        globalStats.ChangeGunCriticalChanceMultiplier(GunCriticalChanceMultiplier.variable, GunCriticalChanceMultiplier.operationType);
        globalStats.ChangeDoubleDamageOnLastBullet(doubleDamageOnLastBullet.variable, doubleDamageOnLastBullet.operationType);

    }

}