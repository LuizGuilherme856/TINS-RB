using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GunAmmo_StatsChange", menuName = "Upgrades/StatsChange/GunAmmo")]
public class GunAmmo_StatsChange : StatsChange
{
    [Header("Alterações de stats relacionadas a munição de armas de fogo.")]
    [Space(30)]


    [Header("Valores que somam/subtraem com o padrão da arma.")]
    [Space(10)]

    [Tooltip("Aumenta/Diminui a munição máxima sobressalente das armas de fogo. Soma com o valor padrão da arma. " +
        "Valores negativos diminuem a munição e positivos aumentam")]
    public int increaseMaxSpareAmmoBy = 0;

    [Tooltip("Aumenta/Diminui a munição máxima do pente das armas de fogo. Soma com o valor padrão da arma. " +
    "Valores negativos diminuem a munição e positivos aumentam")]
    public int increaseMaxAmmoOnClipBy = 0;

    [Tooltip("Aumenta/Diminui o consumo de munição. Soma com o valor padrão da arma. " +
    "Valores negativos diminuem a munição consumida e positivos aumentam")]
    public int increaseAmmoConsumedPerShot = 0;

    [Space(20)]
    [Header("Valores que aumentam/diminuem em %\n" +
        "Valores aceitos são entre 0 e 10, ou seja 0% e 1000%.\n" +
        "Valores menores que 1 diminuem o dano/chance da arma.\n" +
        "0 a arma não causa mais dano/chance e \n" +
        "maiores que 1 aumentam o dano/chance da arma.")]
    [Space(10)]

    [Tooltip("Aumenta/Diminui em % a munição sobressalente máxima.")]
    [SerializeField] private VariableOperation<float> GunSpareAmmoMultiplier;

    [Tooltip("Aumenta/Diminui em % a munição do pente máxima.")]
    [SerializeField] private VariableOperation<float> GunMaxAmmoOnClipMultiplier;

    [Tooltip("Aumenta/Diminui em % a munição consumida por disparo.")]
    [SerializeField] private VariableOperation<float> GunAmmoConsumedPerShotMultiplier;

    [SerializeField] private VariableOperation<float> ChanceOfNotConsumingAmmoMultiplier;
    [SerializeField] private VariableOperation<float> IncreaseChanceOfNotConsumingAmmoBy;


    [SerializeField] private VariableOperation<float> IncreaseReloadTimeBy;

    [SerializeField] private VariableOperation<float> ReloadTime_Multiplier;
    [SerializeField] private VariableOperation<float> chanceOfFillingAmmoOnKill;

    [SerializeField] private VariableOperation<bool> explosionOnReload;
    [SerializeField] private VariableOperation<DamageType> explosionOnReloadDamageType;



    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        // Apply multiplicative ammunition modifiers.
        globalStats.ChangeGunSpareAmmoMultiplier(GunSpareAmmoMultiplier.variable, GunSpareAmmoMultiplier.operationType);
        globalStats.ChangeGunMaxAmmoOnClipMultiplier(GunMaxAmmoOnClipMultiplier.variable, GunMaxAmmoOnClipMultiplier.operationType);
        globalStats.ChangeGunAmmoConsumedPerShotMultiplier(GunAmmoConsumedPerShotMultiplier.variable, GunAmmoConsumedPerShotMultiplier.operationType);
        
        globalStats.ChangeChanceOfNotConsumingAmmoMultiplier(ChanceOfNotConsumingAmmoMultiplier.variable, ChanceOfNotConsumingAmmoMultiplier.operationType);
        globalStats.ChangeIncreaseChanceOfNotConsumingAmmoBy(IncreaseChanceOfNotConsumingAmmoBy.variable, IncreaseChanceOfNotConsumingAmmoBy.operationType);

        globalStats.ChangeReloadTime_Multiplier(ReloadTime_Multiplier.variable, ReloadTime_Multiplier.operationType);
        globalStats.ChangeIncreaseReloadTimeBy(IncreaseReloadTimeBy.variable, ReloadTime_Multiplier.operationType);
        globalStats.ChangeChanceOfFillingAmmoOnKill(chanceOfFillingAmmoOnKill.variable, chanceOfFillingAmmoOnKill.operationType);

        globalStats.ChangeExplosionOnReload(explosionOnReload.variable, explosionOnReload.operationType);
        globalStats.ChangeExplosionOnReloadDamageType(explosionOnReloadDamageType.variable, explosionOnReloadDamageType.operationType);


        // Apply additive changes to the default value.
        globalStats.increaseMaxSpareAmmoBy += increaseMaxSpareAmmoBy;
        globalStats.increaseMaxAmmoOnClipBy += increaseMaxAmmoOnClipBy;
        globalStats.increaseAmmoConsumedPerShot += increaseAmmoConsumedPerShot;
    }

}