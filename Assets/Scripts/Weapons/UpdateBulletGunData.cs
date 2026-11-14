using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Update data for the weapon that fired the bullet.
/// </summary>
public class UpdateBulletGunData : MonoBehaviour
{
    private BulletStats bulletStats;
    private HandleBulletImpact bulletImpact;
    private BulletDeathHandler bulletDeathHandler;
    private GunData gunData;

    private void Awake()
    {
        bulletImpact = GetComponent<HandleBulletImpact>();
        bulletStats = GetComponent<BulletStats>();
        bulletDeathHandler = GetComponent<BulletDeathHandler>();
    }


    /// <summary>
    /// Refresh the firing weapon's data here.
    /// </summary>
    public void SetGunData(GunData gunData, BulletDeathFxPooler bulletDeathFxPooler, BulletRicochetEffectPooler bulletRicochetEffectPooler)
    {
        if(bulletImpact == null) GetComponent<HandleBulletImpact>();
        if (bulletStats == null) GetComponent<BulletStats>();
        if (bulletDeathHandler == null) GetComponent<BulletDeathHandler>();


        if (gunData == null)
        {
            Debug.LogError("Esta tentando colocar uma gunData null no " + gameObject.name);
            return;
        }
        this.gunData = gunData;
        bulletStats.SetGunData(gunData);
        bulletImpact.SetGunData(gunData, bulletRicochetEffectPooler);
        bulletDeathHandler.SetGunData(gunData, bulletDeathFxPooler);
    }

}
