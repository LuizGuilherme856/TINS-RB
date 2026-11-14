using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "new Damage data", menuName = "Damage/Damage Data")]
public class DamageData : ScriptableObject
{
    [SerializeField] public float damage;
    public DamageType damageType;
    public float criticalChance;
    public float criticalMultiplier;
    [Range(0, 1)] public float statusChance = 1;
    [Range(0, 10)] public float statusDuration = 2;
    public float statusDamagePerSec = 0;

}
