using UnityEngine;
using System;


//[CreateAssetMenu(fileName ="new gun", menuName = "Gun")]
public class GunData : WeaponData
{

    #region gun stats


    // [Header("#---- AMMO ----#")]
    [SerializeField] public int maxAmmoOnClip;
    [SerializeField] public int maxSpareAmmo;
    [SerializeField] public int ammoConsumedPerShot = 1;
    [Range(0, 1)] public float chanceOfNotConsumingAmmo = 0;


   // [Header("#---- BULLET ----#")]
    public GameObject bullet;
    public float bulletSpeed;
    [Tooltip("tempo de vida que a bala tem antes de se destruir")]
    public float bulletLifeTime = 20f; //
    [Tooltip("obj que spawna quando a bala morre. No caso da 12 s� da um fade out nela")]
    public GameObject bulletDeathFX; //
    [Tooltip("obj que spawna quando a bala acerta algo. utilizado em armas que tem ricochete")]
    public GameObject bulletRicochetFx;
    [Tooltip("obj que spawna quando a bala acerta algo e morre.")]
    public GameObject bulletCollisionDeathFX;
    [Tooltip("Balas que saem da arma de forma espalhada. Como um tiro de shotgun")]
    [SerializeField] [Range(1, 50)] [Min(1)] public int scatteredBulletsPerShot = 1;
    [Tooltip("quando uma arma dispara mais de uma bala espalhada, as balas saem com um desvio no angulo")]
    [Range(0, 50)] public float multipleBulletAngleOffset = 2;
    [Tooltip("Balas alinhadas. Saem uma atras da outra")]
    [SerializeField] public int linedUpBulletsPerShot;
    [Tooltip("Aleatoriza a velocidade das balas para nao viajarem todas na mesma velocidade")]
    public float speedRandomness =0 ;   

    //[Space(15)]
    //[Header("#---- RECOIL ----#")]
    public float recoilForceOnPlayer;
    public float recoilAngle = 20f, recoilDuration = 0.1f, imprecision = 0;

    //[Space(15)]
    //[Header("#---- CAMERA ----#")]
    public float camShakeMag;
    public float camShakeDuration;

   // [Space(15)]
   // [Header("#---- SHELLS AND CLIP ----#")]
    public GameObject bulletCapsule;
    public GameObject clip;
    public bool dropCapsuleOnshooting;

    [Tooltip("offset em rela��o ao (0,0) da arma ")]
    public Vector3 capsuleSpawnOffset = new Vector3(0.3f, 0, 0);

    //[Space(15)]
   // [Header("#---- MUZZLE FLASH ----#")]
    public GameObject muzzleFlash;
    public bool hasMuzzleFlash = true;
    public float muzzleFlashSpawnOffset = 1f;

    //[Space(15)]
    //[Header("#---- SOUNDS ----#")]
    public AudioClip shootingSFX;
    [Range(0, 1)] public float shootingVolume = 1f;
    [Tooltip("tipo de som do tiro. Se for discreto da play a cada bala disparada. Continuo da play num som ao segurar o gatilho como na lan�a chamas")]
    public SoundType soundType = SoundType.discrete; //
    [Tooltip("so serve pra armas tipo a flamethrower")]
    public AudioClip startShootingSFX; //
    public AudioClip stopShootingSFX;
    [Range(0, 1)] public float reloadVolume = 1f;
    public AudioClip startReloadingSFX; 
    public AudioClip stopReloadingSFX;

   // [Space(15)]
    //[Header("#---- OTHERS ----#")]
    public float fireRate;
    [Tooltip("Tamanho da rajada")]
    public int burstSize =1;
    [Tooltip("tempo de disparo dentro da rajada. firerate seria o tempo entre rajadas")]
    public float burstRate = 0.1f;
    [Range(0, 10)] public float reloadTime;
    public GunType gunType;
    public string hoveringShotName;
    public Vector3 firePointOffset;
    public Vector3 posOffset;
    public bool isGoldenWeapon = false;
    public GunData goldenVersion;

    public float DPS = -1;
    public float PricePerDPS = -1;
    public Rarity rarity;




    [Range(0, 1)] public float impactFxSpawnProbability = 1;
    [Range(0, 1)] public float deathFxSpawnProbability = 1;

    #endregion

    #region Ammo Related

    public int GetAmmoConsumedPerShot(bool defaultValue = false)
    {
        if(defaultValue) return ammoConsumedPerShot;
        return statsCalculator.CalculateNewAmmoConsumedPerShot(ammoConsumedPerShot, GetMaxSpareAmmo());
    }

    public int GetMaxAmmoOnClip(bool defaultValue = false)
    {
        if (defaultValue) return maxAmmoOnClip;
        return statsCalculator.CalculateNewAmmoOnClip(maxAmmoOnClip);
    }

    public int GetMaxSpareAmmo(bool defaultValue = false)
    {
        if(defaultValue) return maxSpareAmmo;
        return statsCalculator.CalculateNewSpareAmmo(maxSpareAmmo);
    }

    #endregion

    #region multiple Shots

    public float GetEmissiveShotsDelay()
    {
        Debug.Log("Fun��o nao implementada...");
        return 0.1f;// runeUpgradeManager.DelayBetweenEmissiveShots();
    }

    public bool HasEmissiveShot()
    {
        // Debug.Log("Method not implemented");
        return false;// runeUpgradeManager.HasEmissiveShots();
    }

    public int GetScatteredBulletsPerShot(bool defaultValue = false)
    {
        if(defaultValue) return scatteredBulletsPerShot;
        return statsCalculator.CalculateScatteredBulletsAmount(scatteredBulletsPerShot);
    }

    public int GetLinedUpBulletsPerShot()
    {
        return statsCalculator.CalculateLinedUpBulletsAmount(linedUpBulletsPerShot);
    }

    #endregion

    #region Damage & critical

    public float GetCriticalChance(bool defaultValue = false)
    {
        if (defaultValue) return criticalChance;
        return statsCalculator.CalculateNewGunCriticalChance(criticalChance);
    }

    public float GetCriticalMultiplier(bool defaultValue = false)
    {
        if (defaultValue) return criticalMultiplier;
        return statsCalculator.CalculateNewGunCriticalMultiplier(criticalMultiplier);
    }

    public override float GetDamage(bool getDefaultDamage = false)
    {
        if (getDefaultDamage) return damage;
        return statsCalculator.CalculateNewGunDamage(damage, damageType);
    }

    #endregion

    #region Status

    public float GetStatusChance(bool defaultValue = false)
    {
        if (Damage2.GetStatusEffect(damageType) == StatusEffect.None) return 0f;
        if (defaultValue) return statusChance;
        return statsCalculator.CalculateNewStatusChance(statusChance);
    }

    public float GetStatusDamage(bool defaultValue = false)
    {
        if (defaultValue) return statusDamagePerSec;
        return statsCalculator.CalculateNewStatusDamage(statusDamagePerSec);
    }

    public float GetStatusDuration(bool defaultValue = false)
    {
        if (defaultValue) return statusDuration;
        return statsCalculator.CalculateNewStatusDuration(statusDuration);
    }

    #endregion

    #region DPS & Score

    public float CalculateDPS()
    {
        // Critical probability as a fraction; 20% is 0.2.
        float criticalChanceDecimal = GetCriticalChance()/100f;

        // Average projectile damage including critical hits.
        float averageDamagePerBullet = GetDamage() * (1 - criticalChanceDecimal) +
                                       (GetDamage() * GetCriticalMultiplier() * criticalChanceDecimal);

        // Average damage of a complete shot.
        float averageDamagePerShot = averageDamagePerBullet * GetScatteredBulletsPerShot() *(GetLinedUpBulletsPerShot() + 1);

        // Shots per second.
        float timeToEmptyMagazine = (GetMaxAmmoOnClip() * (GetFireRate()) / GetAmmoConsumedPerShot()); // Time required to empty the magazine.
        float totalCycleTime = timeToEmptyMagazine + GetReloadTime(); // Total cycle time: firing plus reloading.

        // Shots per second including reload time.
        float effectiveFireRate = GetMaxAmmoOnClip() / totalCycleTime;

        // Final DPS.
        float dps = averageDamagePerShot * effectiveFireRate;

        return dps;
    }


    // Calculate the status-effect contribution.
    public  float CalculateStatusRelevance()
    {
        // Effect weights.
        float weight = GetStatusWeight(damageType);

        // Total status damage: DPS times duration.
        float totalStatusDamage = (GetStatusDamage() + 1 ) * (GetStatusDuration() * 10);

        // Expected status damage after applying its probability.
        float expectedStatusDamage = totalStatusDamage * GetStatusChance();

        // Status contribution: expected damage times its weight.
        float statusRelevance = expectedStatusDamage * weight;
        if (statusRelevance == 0) statusRelevance = 1;

        return statusRelevance;
    }

    // Return the weight for the status type.
    private  float GetStatusWeight(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Puncture:
                return 0f;
            case DamageType.Water:
                return 1f;
            default:
                return 5f;
        }
    }


    #endregion

    public float GetReloadTime(bool defaultValue = false)
    {
        if (defaultValue) return reloadTime;
        return statsCalculator.CalculateNewReloadTime(reloadTime);
    }

    public float GetFireRate(bool defaultValue = false)
    {
        if (defaultValue) return fireRate;
        return statsCalculator.CalculateNewFireRate(fireRate);
    }

    public override int GetPrice()
    {
        if(globalStats == null) globalStats = AssetFinder.GetGlobalStats();

        return (int)(base.GetPrice() * globalStats.ForCurrentRun.gunPriceMultiplier * globalStats.ForCurrentRun.globalPriceMultiplier);
    }

    [ContextMenu("Buff ammo 30%")]
    public void BuffAmmo()
    {
        maxSpareAmmo = (int)(Math.Floor(maxSpareAmmo * 1.3));
    }

}

public enum GunType { semiAutomatic, automatic, laser, burst, hoveringShot}



public enum SoundType { discrete, continuous }

