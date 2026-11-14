///Stores explosion parameters.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Explosive data", menuName = "Damage/Explosive")]
public class ExplosiveData : DamageData
{
    public float impulseStrength = 10f, damageOnPlayer =1f;
    
    public StatusEffect statusEffect = StatusEffect.None;
    [field: SerializeField] public float explosionVolume { get; private set; }

}
