using JetBrains.Annotations;
using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Select the bullet pool and assign it to ShootingHandler when weapons change.
/// </summary>
public class SelectWhichPoolToUse : MonoBehaviour
{
    [SerializeField] private GameObject bulletPoolObject;

    [SerializeField]  private List<MMMiniObjectPooler> poolers;

    [SerializeField] private PoolType poolType = PoolType.gunBullet;
    private ShootingHandler shootingHandler;
    private GunManager gunManager;

    private MeleeManager meleeManager;
    private MeleeAttackHandler meleeAttackHandler;
    private BulletRicochetEffectPooler bulletRicochetSpawner;
    private BulletDeathFxPooler bulletDeathFxSpawner;


    private void Awake()
    {
        switch (poolType)
        {
            case PoolType.gunBullet:
                gunManager = GetComponent<GunManager>();
                shootingHandler = GetComponent<ShootingHandler>();
                break;
            case PoolType.meleeProjectile:
                meleeManager= GetComponent<MeleeManager>();
                meleeAttackHandler = GetComponent<MeleeAttackHandler>();
                break;
            case PoolType.bulletRicochet:
                gunManager = GetComponent<GunManager>();
                bulletRicochetSpawner = GetComponent<BulletRicochetEffectPooler>();
                break;
            default:
                break;
        }

        FillPoolList();
    }

    private void Start()
    {
        switch (poolType)
        {
            case PoolType.gunBullet:
                gunManager.OnWeaponChange += UpdatePool;
                break;
            case PoolType.meleeProjectile:
                meleeManager.OnChangeMeleeWeapon += UpdatePool;
                break;
            case PoolType.bulletRicochet:
                gunManager.OnWeaponChange += UpdatePool;
                break;
            default:
                break;
        }

        UpdatePool();
    }

    private void FillPoolList()
    {
        var poolers = bulletPoolObject.GetComponents<MMMiniObjectPooler>();
        this.poolers.AddRange(poolers);

        
    }
    private void UpdatePool()
    {
        string pooledObjectName = GetPooledObjectName();
        if (string.IsNullOrEmpty(pooledObjectName))
        {
            return;
        }

        var pooler = GetPooler(pooledObjectName);
        if (pooler == null)
        {
            Debug.LogWarning($"Nao encontrou pool para '{pooledObjectName}' em {gameObject.name}.");
            return;
        }

        switch (poolType)
        {
            case PoolType.gunBullet:
                shootingHandler.SetObjectPooler(pooler);
                break;
            case PoolType.meleeProjectile:
                meleeAttackHandler.SetObjectPooler(pooler);
                break;
        }

    }

    private MMMiniObjectPooler GetPooler(string pooledGameObjName)
    {

        for (int i = 0; i < poolers.Count; i++)
        {
            if (poolers[i].GameObjectToPool.name == pooledGameObjName)
            {
                return poolers[i];
            }
        }

        return null;
    }

    private string GetPooledObjectName()
    {
        string pooledGameObjName;
        switch (poolType)
        {
            case PoolType.gunBullet:
                pooledGameObjName = gunManager.gunData != null && gunManager.gunData.bullet != null
                    ? gunManager.gunData.bullet.name
                    : string.Empty;
                break;
            case PoolType.meleeProjectile:
                MeleeData meleeData = meleeManager.GetMeleeData();
                pooledGameObjName = meleeData != null && meleeData.meleeGameObject != null
                    ? meleeData.meleeGameObject.name
                    : string.Empty;
                break;
            case PoolType.bulletRicochet:
                pooledGameObjName = gunManager.gunData != null && gunManager.gunData.bulletRicochetFx != null
                    ? gunManager.gunData.bulletRicochetFx.name
                    : string.Empty;
                break;
            default:
                pooledGameObjName = "";
                break;
        }

        return pooledGameObjName;
    }


    /// <summary>
    /// Return the pool for the supplied GunData, independent of the equipped weapon.
    /// </summary>
    public MMMiniObjectPooler GetDeathEffectGameObject(GunData gunData)
    {
        return GetPooler(gunData.bulletDeathFX.name);

    }
    /// <summary>
    /// Separate pools exist for gun projectiles and melee weapons.
    /// </summary>
    private enum PoolType { gunBullet, meleeProjectile, bulletRicochet}
}
