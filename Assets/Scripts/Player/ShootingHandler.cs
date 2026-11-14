using UnityEngine;
using System;
using MoreMountains.Feedbacks;
using Object = UnityEngine.Object;

/// <summary>
///Spawns bullets through object pools.
///Keep weapon-dependent references refreshed here
///to avoid adding more responsibilities to GunManager.
/// </summary>
public class ShootingHandler : MonoBehaviour
{
    [SerializeField] private ControlCameraShake controlCameraShake;
    [SerializeField] private GunRecoil gunRecoil;

    [SerializeField] private CapsuleSpawner capsuleSpawner; //TODO: Simplify this implementation.
    [SerializeField] private MuzzleFlashSpawner muzzleFlashSpawner;
    [SerializeField] private MMMiniObjectPooler miniObjectPooler;
    [SerializeField] private MMMiniObjectPooler emissiveShotMiniObjectPooler;

    [SerializeField] private CheckIfEnemiesAreTooClose checkIfEnemiesAreTooClose;

    [SerializeField] private PlayerData playerData;

    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool playerClone = false;
    [SerializeField] private GlobalStats globalStats;

    public event Action OnShoot;

    private Aim aim;
    //private RaycastShootingHandler raycastShootingHandler;
    private GunData gunData;
    private Vector3 firePoint;
    private float aimAngle, multipleBulletsAngleOffset =0;
    private float damageMultiplier = 1;
    private float gunImprecision, speedRandomness;
    private BulletDeathFxPooler bulletDeathFxPooler;
    private BulletRicochetEffectPooler ricochetEffectPooler;

    //[Header("DEBUG")]
    //[SerializeField] private bool showDebug = true;

    private int numberOfInstaKillsActive = 0; //Current number of instant-kill effects.

    private void Awake()
    {
        if(controlCameraShake == null)
        {
            Debug.LogWarning("ShootingHandler: ControlCameraShake não estava setado, buscando na cena...");
            controlCameraShake = Object.FindFirstObjectByType<ControlCameraShake>();
        }

        if (globalStats == null) globalStats = AssetFinder.GetGlobalStats();
        bulletDeathFxPooler = GetComponent<BulletDeathFxPooler>();
        ricochetEffectPooler = GetComponent<BulletRicochetEffectPooler>();
        // raycastShootingHandler = GetComponent<RaycastShootingHandler>();

    }

    //Refresh after switching weapons.
    public void UpdateBulletData(Aim aim, GunData gunData)
    {
        this.aim = aim;
        this.gunData = gunData;
        muzzleFlashSpawner.UpdateBulletData(gunData);

        if(playerClone == false)
        {
            capsuleSpawner.UpdateBulletData(gunData);

        }
    }


    public void InstantiateShot(Vector3 firePoint, float aimAngle)
    {
        //When an enemy is very close, spawn the bullet at the player so it does not
        //appear behind the enemy; otherwise, use the weapon muzzle.
        bool isEnemyTooClose = checkIfEnemiesAreTooClose.IsEnemyTooCloseCircle();
        if (isEnemyTooClose) this.firePoint = playerTransform.position;
        else this.firePoint = firePoint;


        this.aimAngle = aimAngle;
        multipleBulletsAngleOffset = 0;

        bool useRotationFeedback = true;

        if(gunData.gunType == GunType.laser)
        {
            //raycastShootingHandler.ShootLaser();
        }
        else if(gunData.gunType == GunType.hoveringShot)
        {
            useRotationFeedback = false;
        }
        else
        {
            int extraShotBlessing = 0;
            if (UnityEngine.Random.Range(0f, 1f) < globalStats.chanceOfShootingExtraBullet) extraShotBlessing = 1;

            //Regular discrete projectiles.
            int numberOfBulets =  gunData.GetScatteredBulletsPerShot() + extraShotBlessing;
            Projectile[] projectiles = new Projectile[numberOfBulets]; //Return an array because some weapons fire multiple bullets per shot.
            Projectile[] linedUpProjectiles = new Projectile[numberOfBulets * gunData.GetLinedUpBulletsPerShot()];

            for (int i = 0; i < numberOfBulets; i++)
            {
                UpdateProjectileProperties(projectiles, i);
                SetBulletSpeed(projectiles[i], i, 1);

                if(i == 0) //Only the first bullet adds trailing aligned bullets.
                {
                    for (int j = 0; j < gunData.GetLinedUpBulletsPerShot(); j++)
                    {
                        UpdateProjectileProperties(linedUpProjectiles, j);
                        //Copy shot data from the preceding bullet.
                        //Each trailing bullet retains 90% of its predecessor's value.
                        float speedMultiplier = 90 - j * 3;
                        speedMultiplier /= 100; //Convert to a percentage.
                        SetBulletSpeed(linedUpProjectiles[j], i, speedMultiplier, false, false);
                    }

                }
                //Refresh bullets after assigning their data; otherwise, the new values take effect only on the next shot.
                //Aux.Print("ShootingHandler: " + gunData.GetGunName(Language.english).ToString() + " - "+gunData.bullet.name.ToString(), showDebug);
            }
        }
        
        OnShoot?.Invoke();

        gunRecoil.TriggerRecoil(aim.GetIsAimingRight(), aimAngle, gunData.recoilForceOnPlayer, useRotationFeedback);
        if (playerClone) return;
        controlCameraShake.TriggerCameraShake(aimAngle, gunData.camShakeMag);

    }

    private void UpdateProjectileProperties(Projectile[] projectiles, int i)
    {
        projectiles[i] = miniObjectPooler.GetPooledGameObject().GetComponent<Projectile>();
        
        projectiles[i].gameObject.SetActive(true);
        projectiles[i].transform.localScale = transform.localScale * gunData.projectileSize;
       
        //Run this after activating the GameObject.
        projectiles[i].gameObject.GetComponent<UpdateBulletGunData>().SetGunData(gunData, bulletDeathFxPooler, ricochetEffectPooler); //Refresh the GunData that fired this bullet.

        var bulletStats = projectiles[i].gameObject.GetComponent<BulletStats>();
        bulletStats.SetDamageMultiplier_PowerUp(damageMultiplier);
        
        //Double weapon damage for the last bullet.
        if (gunData.currentAmmoOnClip <= gunData.GetAmmoConsumedPerShot() && globalStats.doubleDamageOnLastBullet)
        {
            bulletStats.SetDamageMultiplier_FirstBullet(2);
        }
        else
        {
            bulletStats.SetDamageMultiplier_FirstBullet(1);
        }

        //Change color for upgraded weapons.
        if (gunData.isGoldenWeapon) projectiles[i].GetComponent<ChangeBulletShader>()?.ChangeBulletColorToBlue();
        else projectiles[i].GetComponent<ChangeBulletShader>()?.ChangeColorToDefault();
        
        EmissiveShot(projectiles, i);
    }

    private void EmissiveShot(Projectile[] projectiles, int i)
    {
        var emissiveShot = projectiles[i].GetComponent<EmissiveShot>();

        if(emissiveShot == null)
        {
            Debug.Log($"O projetil {projectiles[0].name} n�o contem referencia � classe EmissiveShot. Certifique-se de que " +
                $"a classe esteja devidamente colocada no objeto no inspector");
            return;
        }

        if (gunData.HasEmissiveShot())
        {
            emissiveShot.spawnBullets = true;
            emissiveShot.SetObjectPooler(emissiveShotMiniObjectPooler);
            emissiveShot.SetDelay(gunData.GetEmissiveShotsDelay());
            emissiveShot.damageMultiplier = damageMultiplier;
            emissiveShot.gunData = gunData;
        }
        else
        {
            emissiveShot.spawnBullets = false;
        }
    }

    //Switch bullet pools when the equipped weapon changes.
    public void SetObjectPooler(MMMiniObjectPooler miniObjectPooler)
    {
        this.miniObjectPooler = miniObjectPooler;
    }

    private void SetBulletSpeed(Projectile newProjectile, int currentIndex, float speedMultiplier, 
        bool recalculateAngleOffset = true, bool recalculateRandomness = true) //Speed, inaccuracy, and angular spread for multiple bullets.
    {
        var originalLocalScale = newProjectile.transform.localScale;
        Vector3 newRotation;
        //gunImprecision = gunData.imprecision;

        if(recalculateRandomness) //Do not recalculate for aligned bullets.
        {
            gunImprecision =  UnityEngine.Random.Range(-gunImprecision, gunImprecision);
            speedRandomness =  UnityEngine.Random.Range(-gunData.speedRandomness, gunData.speedRandomness);
        }

        if (aim.GetIsAimingRight())
        {
            newProjectile.SetDirection(1);

            newRotation = aim.GetAimTransform().rotation.eulerAngles;
            newRotation = new Vector3(newRotation.x, newRotation.y, newRotation.z + gunImprecision); //Apply inaccuracy.
        }
        else
        {
            newProjectile.SetDirection(-1);
            newRotation = new Vector3(0, 0, -(aimAngle + 180));
            newRotation = new Vector3(newRotation.x, newRotation.y, newRotation.z + gunImprecision); //Apply inaccuracy.
            newProjectile.gameObject.GetComponent<Transform>().eulerAngles = -newRotation;
            newProjectile.gameObject.GetComponent<Transform>().localScale = new Vector3(-originalLocalScale.x, originalLocalScale.y, 1);
        }


        if(gunData.GetScatteredBulletsPerShot() > 1)
        {
            //Preserve the original angle for aligned bullets so their formation remains intact.
            if(recalculateAngleOffset)
            {
                if (currentIndex == 0)
                {
                     //Do not offset the first bullet's angle.
                }
                else if(currentIndex%2 !=0) //For odd indices, apply a positive angle offset.
                {
                    multipleBulletsAngleOffset = Mathf.Abs(multipleBulletsAngleOffset);
                    multipleBulletsAngleOffset += gunData.multipleBulletAngleOffset;
                }
                else //For even indices, apply a negative angle offset.
                {
                    multipleBulletsAngleOffset *= -1;
                }
            }

            newRotation = new Vector3(newRotation.x, newRotation.y, newRotation.z + multipleBulletsAngleOffset);

        }
        newProjectile.SetRotation(newRotation);

        newProjectile.transform.position = firePoint;
        newProjectile.SetprojectileSpeed(gunData.bulletSpeed*speedMultiplier + speedRandomness);
        newProjectile.RefreshVelocity();
    }

    public void SetDamageMultiplierDuration(float damageMultiplier, float duration)
    {
        numberOfInstaKillsActive++;
        this.damageMultiplier = damageMultiplier * numberOfInstaKillsActive;
        Invoke(nameof(UpdateDamageMultiplier), duration);
    }

    private void UpdateDamageMultiplier()
    {
        numberOfInstaKillsActive--;
        if (numberOfInstaKillsActive <= 0)
        {
            numberOfInstaKillsActive = 0;//Defensive check.
            damageMultiplier = 1;
        }
        else
        {
            float damageMultiplier_gambiarra = 2; //TODO: Obtain this value from SetDamageMultiplierDuration instead of hard-coding 5.
            damageMultiplier = damageMultiplier_gambiarra * numberOfInstaKillsActive;
        }
    }
}
