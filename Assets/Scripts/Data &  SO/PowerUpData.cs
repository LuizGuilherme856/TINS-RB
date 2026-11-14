using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "PowerUpData", menuName = "PowerUpData")]
public class PowerUpData : ScriptableObject
{

    [Header("    -------POWER-UP STATS-----    ")]
    [Space(8)]

    public int hpHealedByHearts = 1;
    public int pointPerDrop = 250;
    public float intaKillMeleeDuration = 25f;
    public float intaKillMeleeDamageMultiplier = 5f;
    public float intaKillDuration = 25f;
    public float intaKillDamageMultiplier = 5f;
    public float doublePointsDuration = 25f;
    public float shieldDuration = 25f;
}
