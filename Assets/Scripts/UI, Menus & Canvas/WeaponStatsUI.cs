using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Display weapon stats in the inventory.
/// </summary>
public class WeaponStatsUI : MonoBehaviour
{
    [SerializeField] private bool isMelee = false;
    [field: SerializeField] public WeaponStats WeaponStats {  get; private set; }
    [field: SerializeField] public MeleeWeaponStats MeleeWeaponStats { get; private set; }


    [SerializeField] TextMeshProUGUI nameText, infoText, infoTextUpgraded;
    [SerializeField] LanguageData languageData;


    private void Start()
    {
        if(isMelee == false)
        {
            UpdateGunName();
        }
        else
        {
            UpdateMeleeWeaponName();
        }
    }

    private void UpdateMeleeWeaponName()
    {
        switch (MeleeWeaponStats)
        {
            case MeleeWeaponStats.damage:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Damage";
                        break;
                    case Language.chinese:
                        nameText.text = "伤害";
                        break;
                    case Language.portuguese:
                        nameText.text = "Dano";
                        break;
                    case Language.spanish:
                        nameText.text = "Daño";
                        break;
                    default:
                        break;
                }
                break;
            case MeleeWeaponStats.criticalChance:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Crit. Chance";
                        break;
                    case Language.chinese:
                        nameText.text = "暴击几率";
                        break;
                    case Language.portuguese:
                        nameText.text = "Chance Crítica";
                        break;
                    case Language.spanish:
                        nameText.text = "Prob. de Crítico";
                        break;
                    default:
                        break;
                }
                break;
            case MeleeWeaponStats.criticalMultiplier:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Crit. Multiplier";
                        break;
                    case Language.chinese:
                        nameText.text = "暴击倍数";
                        break;
                    case Language.portuguese:
                        nameText.text = "Multip. Crítico";
                        break;
                    case Language.spanish:
                        nameText.text = "Multip. Crítico";
                        break;
                    default:
                        break;
                }
                break;
            case MeleeWeaponStats.damageType:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Damage Type";
                        break;
                    case Language.chinese:
                        nameText.text = "伤害类型";
                        break;
                    case Language.portuguese:
                        nameText.text = "Tipo de Dano";
                        break;
                    case Language.spanish:
                        nameText.text = "Tipo de Daño";
                        break;
                    default:
                        break;
                }
                break;
            case MeleeWeaponStats.attackRate:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Attack Rate";
                        break;
                    case Language.chinese:
                        nameText.text = "攻击速度";
                        break;
                    case Language.portuguese:
                        nameText.text = "Taxa de Ataque";
                        break;
                    case Language.spanish:
                        nameText.text = "Velocidad de Ataque";
                        break;
                    default:
                        break;
                }
                break;
            case MeleeWeaponStats.knockBackForce:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Knockback Force";
                        break;
                    case Language.chinese:
                        nameText.text = "击退力";
                        break;
                    case Language.portuguese:
                        nameText.text = "Força de Repulsão";
                        break;
                    case Language.spanish:
                        nameText.text = "Fuerza de Retroceso";
                        break;
                    default:
                        break;
                }
                break;
            case MeleeWeaponStats.throwSpeed:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Throw Speed";
                        break;
                    case Language.chinese:
                        nameText.text = "投掷速度";
                        break;
                    case Language.portuguese:
                        nameText.text = "Veloc. de Arremesso";
                        break;
                    case Language.spanish:
                        nameText.text = "Veloc. de Lanzamiento";
                        break;
                    default:
                        break;
                }
                break;
            case MeleeWeaponStats.playerImpulse:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Player Impulse";
                        break;
                    case Language.chinese:
                        nameText.text = "玩家冲击力";
                        break;
                    case Language.portuguese:
                        nameText.text = "Impulso Jogador";
                        break;
                    case Language.spanish:
                        nameText.text = "Impulso Jugador";
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

    }

    public void SetInfoText(string text)
    {
        infoText.text = text; 
    }

    public void SetInfoTextUpgraded(string text)
    {
        infoTextUpgraded.text = text;   
    }

    private void UpdateGunName()
    {
        switch (WeaponStats)
        {
            case WeaponStats.damage:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Damage";
                        break;
                    case Language.chinese:
                        nameText.text = "伤害";
                        break;
                    case Language.portuguese:
                        nameText.text = "Dano";
                        break;
                    case Language.spanish:
                        nameText.text = "Daño";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.criticalChance:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Critical chance";
                        break;
                    case Language.chinese:
                        nameText.text = "暴击几率";
                        break;
                    case Language.portuguese:
                        nameText.text = "Chance de Crítico";
                        break;
                    case Language.spanish:
                        nameText.text = "Probabilidad de Crítico";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.criticalMultiplier:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Critical Multiplier";
                        break;
                    case Language.chinese:
                        nameText.text = "暴击倍数";
                        break;
                    case Language.portuguese:
                        nameText.text = "Multiplicador Crítico";
                        break;
                    case Language.spanish:
                        nameText.text = "Multiplicador Crítico";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.damageType:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Damage Type";
                        break;
                    case Language.chinese:
                        nameText.text = "伤害类型";
                        break;
                    case Language.portuguese:
                        nameText.text = "Tipo de Dano";
                        break;
                    case Language.spanish:
                        nameText.text = "Tipo de Daño";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.fireRate:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Fire Rate";
                        break;
                    case Language.chinese:
                        nameText.text = "射速";
                        break;
                    case Language.portuguese:
                        nameText.text = "Taxa de Fogo";
                        break;
                    case Language.spanish:
                        nameText.text = "Cadencia de Tiro";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.gunType:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Gun Type";
                        break;
                    case Language.chinese:
                        nameText.text = "枪支类型";
                        break;
                    case Language.portuguese:
                        nameText.text = "Tipo de Arma";
                        break;
                    case Language.spanish:
                        nameText.text = "Tipo de Arma";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.reloadTime:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Reload Time";
                        break;
                    case Language.chinese:
                        nameText.text = "换弹时间";
                        break;
                    case Language.portuguese:
                        nameText.text = "Tempo de Recarga";
                        break;
                    case Language.spanish:
                        nameText.text = "Tiempo de Recarga";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.StatusEffect:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Status Effect";
                        break;
                    case Language.chinese:
                        nameText.text = "状态效果";
                        break;
                    case Language.portuguese:
                        nameText.text = "Efeito de Status";
                        break;
                    case Language.spanish:
                        nameText.text = "Efecto de Estado";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.statusChance:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Status Chance";
                        break;
                    case Language.chinese:
                        nameText.text = "状态几率";
                        break;
                    case Language.portuguese:
                        nameText.text = "Chance de Status";
                        break;
                    case Language.spanish:
                        nameText.text = "Probabilidad de Estado";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.statusDuration:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Status Duration";
                        break;
                    case Language.chinese:
                        nameText.text = "状态持续时间";
                        break;
                    case Language.portuguese:
                        nameText.text = "Duração do Status";
                        break;
                    case Language.spanish:
                        nameText.text = "Duración del Estado";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.statusDamagePerSec:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Status Damage Per Second";
                        break;
                    case Language.chinese:
                        nameText.text = "状态每秒伤害";
                        break;
                    case Language.portuguese:
                        nameText.text = "Dano por Segundo do Status";
                        break;
                    case Language.spanish:
                        nameText.text = "Daño por Segundo del Estado";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.ammo:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Ammo";
                        break;
                    case Language.chinese:
                        nameText.text = "弹药";
                        break;
                    case Language.portuguese:
                        nameText.text = "Munição";
                        break;
                    case Language.spanish:
                        nameText.text = "Munición";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.maxSpareAmmo:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Max Spare Ammo";
                        break;
                    case Language.chinese:
                        nameText.text = "最大备用弹药";
                        break;
                    case Language.portuguese:
                        nameText.text = "Munição Máxima de Reserva";
                        break;
                    case Language.spanish:
                        nameText.text = "Munición Máxima de Reserva";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.ammoConsumedPerShot:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Ammo Consumed Per Shot";
                        break;
                    case Language.chinese:
                        nameText.text = "每发消耗弹药";
                        break;
                    case Language.portuguese:
                        nameText.text = "Munição Consumida por Disparo";
                        break;
                    case Language.spanish:
                        nameText.text = "Munición Consumida por Disparo";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.bulletSpeed:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Bullet Speed";
                        break;
                    case Language.chinese:
                        nameText.text = "子弹速度";
                        break;
                    case Language.portuguese:
                        nameText.text = "Velocidade da Bala";
                        break;
                    case Language.spanish:
                        nameText.text = "Velocidad de la Bala";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.bulletsPerShot:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Bullets Per Shot";
                        break;
                    case Language.chinese:
                        nameText.text = "每发子弹数";
                        break;
                    case Language.portuguese:
                        nameText.text = "Balas por Tiro";
                        break;
                    case Language.spanish:
                        nameText.text = "Balas por Disparo";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.recoilForce:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Recoil Force";
                        break;
                    case Language.chinese:
                        nameText.text = "后坐力";
                        break;
                    case Language.portuguese:
                        nameText.text = "Força do Recuo";
                        break;
                    case Language.spanish:
                        nameText.text = "Fuerza de Retroceso";
                        break;
                    default:
                        break;
                }
                break;
            case WeaponStats.imprecision:
                switch (languageData.currentLanguage)
                {
                    case Language.english:
                        nameText.text = "Imprecision";
                        break;
                    case Language.chinese:
                        nameText.text = "不精确";
                        break;
                    case Language.portuguese:
                        nameText.text = "Imprecisão";
                        break;
                    case Language.spanish:
                        nameText.text = "Imprecisión";
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }



}

public enum MeleeWeaponStats {
    damage, criticalChance, criticalMultiplier,
    damageType, attackRate, knockBackForce, throwSpeed,
    playerImpulse
}
public enum WeaponStats 
    { damage, criticalChance, criticalMultiplier, 
    damageType, fireRate, gunType, reloadTime, StatusEffect, statusChance, statusDuration,
    statusDamagePerSec, ammo, maxSpareAmmo, ammoConsumedPerShot, bulletSpeed, bulletsPerShot,
    recoilForce, imprecision}
