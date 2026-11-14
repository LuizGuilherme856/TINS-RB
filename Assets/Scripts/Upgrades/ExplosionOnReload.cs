using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger an explosion when the player owns the reload-explosion skill.
/// </summary>
public class ExplosionOnReload : MonoBehaviour
{
    [SerializeField] private GlobalStats globalStats;
    [SerializeField] private GunManager gunManager;
    public ExplosiveData defaultExplosion, eletricExplosion, iceExplosion, fireExplosion, poisonExplosion;
    private Explosion explosion;

    private void Awake()
    {
        explosion = GetComponent<Explosion>();
    }

    private void OnEnable()
    {
        gunManager.OnReloadWithEmptyMagazine += TriggerExplosion;
    }

    private void OnDisable()
    {
        gunManager.OnReloadWithEmptyMagazine -= TriggerExplosion;
    }

    private void TriggerExplosion()
    {
        if(globalStats.explosionOnReload)
        {
            ExplosiveData explosionData = GetExplosionData(globalStats.explosionOnReloadDamageType);
            explosion.SetExplosiveData(explosionData);
            explosion.TriggerExplosion();
        }
    }

    private ExplosiveData GetExplosionData(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Explosive:
                return defaultExplosion;
            case DamageType.Electricity:
                return eletricExplosion;
            case DamageType.Ice:
                return iceExplosion;
            case DamageType.Fire:
                return fireExplosion;
            case DamageType.Poison:
                return poisonExplosion;
            default:
                return defaultExplosion;
        }
    }

}
