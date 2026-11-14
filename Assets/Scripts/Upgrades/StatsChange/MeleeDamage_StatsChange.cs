using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeDamage_StatsChange", menuName = "Upgrades/StatsChange/MeleeDamage")]
public class MeleeDamage_StatsChange : StatsChange
{
    [Header("Alterações de stats relacionadas ao dano de armas corpo a corpo.\n" +
        "Valores aceitos são entre 0 e 10, ou seja 0% e 1000%.\n" +
        "Valores menores que 1 diminuem o dano/chance da arma.\n" +
        "0 a arma não causa mais dano/chance e \n" +
        "maiores que 1 aumentam o dano/chance da arma")]
    [Space(20)]

    [Tooltip("Aumenta/Diminui o dano das armas corpo a corpo em %")]
    [SerializeField] private VariableOperation<float> MeleeDamageMultiplier;

    [Tooltip("Aumenta/Diminui o dano dos acertos críticos das armas corpo a corpo em %")]
    [SerializeField] private VariableOperation<float> MeleeCriticalDamageMultiplier;

    [Tooltip("Aumenta/Diminui a chance de crítico das armas corpo a corpo em %")]
    [SerializeField] private VariableOperation<float> MeleeCriticalChanceMultiplier;

    [Header("Valores que somam/subtraem com o padrão.")]
    [Space(10)]

    [Tooltip("Aumenta/Diminui o dano. Soma/subtrai de acordo com sinal do valor")]
    [SerializeField] private VariableOperation<int> IncreaseMeleeDamageBy;

    /// <summary>
    /// Apply stat modifications to GlobalStats.
    /// </summary>
    /// <param name="globalStats">ScriptableObject storing shared stat modifiers.</param>
    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        Debug.Log("Executando a mudança de stats do dano corpo a corpo");

        // Additive modifiers.
        globalStats.ChangeIncreaseMeleeDamageBy(IncreaseMeleeDamageBy.variable, IncreaseMeleeDamageBy.operationType);

        // Multiplicative modifiers.
        globalStats.ChangeMeleeDamageMultiplier(MeleeDamageMultiplier.variable, MeleeDamageMultiplier.operationType);
        globalStats.ChangeMeleeCriticalDamageMultiplier(MeleeCriticalDamageMultiplier.variable, MeleeCriticalDamageMultiplier.operationType);
        globalStats.ChangeMeleeCriticalChanceMultiplier(MeleeCriticalChanceMultiplier.variable, MeleeCriticalChanceMultiplier.operationType);
    }

}
