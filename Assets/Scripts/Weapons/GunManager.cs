using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MoreMountains.Feedbacks;
using Random = UnityEngine.Random;

/// <summary>
/// Controls gun firing and reloading, delegating
/// projectile creation to ShootingHandler.
/// </summary>
// Coordinates trigger state, ammunition, and reload lifecycle; ShootingHandler owns projectile pools.
// A weapon switch must cancel the old reload coroutine before starting work on another weapon.
public class GunManager : MonoBehaviour
{
    #region Properties
    [field: SerializeField] public GunData gunData { get; private set; }
    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform gunTransform;
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private StatsCalculator statsCalculator;

    [SerializeField] private RuneInventory runeUpgradeManager;
    [SerializeField] private MMF_Player noAmmoFeedbacks, changeWeaponFeedbacks, ammoNotConsumedFeedbacks;
    [SerializeField] private bool infiniteAmmo = false,
        
        playerClone = false; //Whether this manager belongs to an ability-created player clone.

    public event Action OnWeaponChange;
   // public event Action OnStopShooting;

    public event Action OnStartReloading, OnCancelReloading, OnFinishReload, OnReloadWithEmptyMagazine;

    private Aim aim;
    private bool infiniteReserveAmmo => !playerClone && TINS.Campaign.CampaignCombatRules.IsCampaignMission;
    private bool isShootButtonHeldDown = false, isReloading = false;
    private float currentFireRateTimer = 0;
    private float currentBurstRate = 0;
    private int burstSize;
    private bool shootBurst = false;
    private HoveringShotManager hoveringShotManager;
    private Coroutine reloadCoroutine_TechBooh;

    //cached components
    private ShootingHandler shootingHandler;
    private RaycastShootingHandler raycastShootingHandler;
    private SpriteRenderer spriteRenderer;
    private HandleGunSFX handleGunSFX;

    
    //[Header("DEBUG")]
    //[SerializeField] private bool showDebug = true;

    #endregion

    private void Awake()
    {
        // The campaign player keeps an inexhaustible reserve, but still empties and reloads each magazine.
        if (infiniteReserveAmmo) infiniteAmmo = false;

        spriteRenderer = GetComponent<SpriteRenderer>();
        shootingHandler = GetComponent<ShootingHandler>();
        handleGunSFX = GetComponent<HandleGunSFX>();
        raycastShootingHandler = GetComponent<RaycastShootingHandler>();
        hoveringShotManager = GetComponentInChildren<HoveringShotManager>();
        aim = GetComponentInParent<Aim>();
    }

    private void OnEnable()
    {
        playerInput.OnShootButtonDown += TriggerShooting;
        playerInput.OnReload += TryManualReload;
        playerInput.OnShootButtonUp += StopShooting;

        
    }

    private void OnDisable()
    {
        playerInput.OnShootButtonDown -= TriggerShooting;
        playerInput.OnReload -= TryManualReload;
        playerInput.OnShootButtonUp -= StopShooting;
    }

    private void Start()
    {
        if (gunData == null)
        {
            Debug.Log("Arma nula no gun manager");
            return;
        }
        Refresh();
        if (infiniteReserveAmmo && gunData.currentSpareAmmo <= 0) gunData.SetMaxSpareAmmo();
        firePoint.localPosition = gunData.firePointOffset;
        gunTransform.localPosition = gunData.posOffset;
    }

     
    private void Update()
    {
        ShootContinuously();
        ShootBurst();
    }

    private void ShootBurst()
    {
        if (gunData == null || (infiniteReserveAmmo && isReloading)) return;
        if (gunData.gunType != GunType.burst) return;

        if (shootBurst && burstSize >1 && currentBurstRate <= 0)
        {
            TryToShoot();
            burstSize--;
            currentBurstRate = gunData.burstRate;

            if (burstSize <= 0) StopShooting();
        }
        currentBurstRate -= Time.deltaTime;
        
    }

    private void TriggerShooting()
    {
        if (gunData == null || (infiniteReserveAmmo && isReloading)) return;

        if(playerClone == false)
            if (playerHealth.GetCurrentHealth() <= 0) return; //The player is dead.

        isShootButtonHeldDown = true; //The fire button is held.

        if (currentFireRateTimer <= 0)
        {
            if (gunData.gunType == GunType.burst)
            {
                shootBurst = true;
                burstSize = gunData.burstSize;
            }

            TryToShoot();
        }

        //Clamp ammunition after reload to prevent exceeding capacity.
        if (gunData.currentAmmoOnClip > gunData.GetMaxAmmoOnClip()) gunData.SetAmmoOnClip( gunData.GetMaxAmmoOnClip());

    }

    private void TryToShoot()
    {
        if (infiniteReserveAmmo && isReloading) return;
        //At least one magazine round is required. If a shot costs more than remains,
        //take the remainder from reserves: a three-round shot with two in the magazine
        //consumes two magazine rounds and one reserve round.
        if (gunData.currentAmmoOnClip >= 1 || infiniteAmmo)
        {
            CancelReload_TechBooh();

            if(gunData.gunType == GunType.hoveringShot)
            {
                hoveringShotManager.StartShooting();
                shootingHandler.InstantiateShot(firePoint.position, playerInput.GetAimAngle());
            }
            else
            {
                shootingHandler.InstantiateShot(firePoint.position, playerInput.GetAimAngle());
            }

            //Consume ammunition only when this weapon manager is not using infinite ammo.
            if (!infiniteAmmo)
            {
                float randomNumber = Random.Range(0.0f, 1.0f);
                if(randomNumber<= statsCalculator.CalculateChanceOfNotConsumingAmmo(gunData.chanceOfNotConsumingAmmo))
                {
                    ammoNotConsumedFeedbacks.PlayFeedbacks();
                }
                else
                {
                    if (infiniteReserveAmmo)
                        gunData.SetAmmoOnClip(Mathf.Max(0, gunData.currentAmmoOnClip - Mathf.Max(1, gunData.GetAmmoConsumedPerShot())));
                    else
                        gunData.DecreaseAmmoOnClip();
                }
            }

            currentFireRateTimer = gunData.fireRate;

            //Reload automatically when the magazine is empty.
            if (!infiniteAmmo && gunData.currentAmmoOnClip < 1 && (gunData.currentSpareAmmo > 0 || infiniteReserveAmmo))
            {
                TriggerReload();
            }

            //Aux.Print(gunData.currentAmmoOnClip.ToString(), showDebug);
        }
        else if (gunData.currentSpareAmmo > 0 || infiniteReserveAmmo)
        {
            TriggerReload();
        }
        else //No ammunition; play the dry-fire sound.
        {
            noAmmoFeedbacks.PlayFeedbacks();
        }
    }

    private void ShootContinuously()
    {
        if (gunData == null) return;

        if (isShootButtonHeldDown && currentFireRateTimer <= 0 && (gunData.gunType == GunType.automatic || gunData.gunType == GunType.hoveringShot))
        {
            TriggerShooting();
            currentFireRateTimer = gunData.fireRate;
        }
        currentFireRateTimer -= Time.deltaTime;
    }

    public void StopShooting()
    {
        if (gunData == null) return;

        if (playerClone == false)
        {
            hoveringShotManager?.StopShooting();
            if(gunData.gunType == GunType.hoveringShot) handleGunSFX.StopContinuousShootingSfx();
            
        }

        isShootButtonHeldDown = false;
        shootBurst = false;
        burstSize = gunData.burstSize;
    }

    private void TryManualReload()
    {
        if (gunData == null) return;

        if (isShootButtonHeldDown || shootBurst)
        {
            return;
        }

        TriggerReload();
    }

    private void TriggerReload()
    {
        if (gunData == null) return;

        if (isReloading == false && gunData.currentAmmoOnClip < gunData.GetMaxAmmoOnClip())
        {
            if(gunData.currentSpareAmmo > 0 || infiniteReserveAmmo)
            {
                // TechBooh:
                // Stop an existing reload coroutine before starting another.
                // Ordering matters: cancellation after marking
                // isReloading = true would invalidate the new reload and prevent completion.
                CancelReload_TechBooh(false);

                OnStartReloading?.Invoke();

                if (gunData.currentAmmoOnClip == 0)
                {
                    OnReloadWithEmptyMagazine?.Invoke();
                }

                isReloading = true;
                if (infiniteReserveAmmo)
                {
                    hoveringShotManager?.StopShooting();
                    reloadCoroutine_TechBooh = StartCoroutine(Reload(
                        statsCalculator.CalculateNewReloadTime(gunData.reloadTime), gunData.GetMaxAmmoOnClip(), 0));
                    return;
                }

                //Fill the magazine from reserves, limited by available ammunition.
                int ammoForTheClip = gunData.currentSpareAmmo >= gunData.GetMaxAmmoOnClip() - gunData.currentAmmoOnClip
                    ? gunData.GetMaxAmmoOnClip() : gunData.currentSpareAmmo;
                int ammoToDecreaseFromSpareAmmo = ammoForTheClip;

                if(gunData.currentAmmoOnClip != 0)
                {
                    ammoToDecreaseFromSpareAmmo -= gunData.currentAmmoOnClip;
                    ammoToDecreaseFromSpareAmmo = Mathf.Abs(ammoToDecreaseFromSpareAmmo);

                    if (gunData.GetMaxAmmoOnClip() - gunData.currentAmmoOnClip > gunData.currentSpareAmmo) ammoToDecreaseFromSpareAmmo = 100000; //Use a large value to force the timer to zero.

                    if(ammoToDecreaseFromSpareAmmo > gunData.currentSpareAmmo) ammoToDecreaseFromSpareAmmo = gunData.currentSpareAmmo;
                }

                if (ammoForTheClip + gunData.currentAmmoOnClip > gunData.GetMaxAmmoOnClip())
                {
                    ammoForTheClip = gunData.GetMaxAmmoOnClip();
                }
                else
                {
                    ammoForTheClip += gunData.currentAmmoOnClip;
                }

                float reloadTime = statsCalculator.CalculateNewReloadTime(gunData.reloadTime);

                hoveringShotManager?.StopShooting();
                reloadCoroutine_TechBooh = StartCoroutine(Reload(reloadTime, ammoForTheClip, ammoToDecreaseFromSpareAmmo));
            }
        }
    }


    private IEnumerator Reload(float timer, int ammoForTheClip, int ammoToDecreaseFromSpareAmmo)
    {
        yield return new WaitForSeconds(timer);
        
        if(isReloading == true) //Cancel reload when changing weapons.
        {
            isReloading = false;
            reloadCoroutine_TechBooh = null;
            //OnCancelReloading?.Invoke();
            gunData.SetAmmoOnClip(ammoForTheClip);
            if (!infiniteReserveAmmo) gunData.DecreaseSpareAmmo(ammoToDecreaseFromSpareAmmo);
            OnFinishReload?.Invoke();

        }
    }

    public void ChangeGun(GunData gunData)
    {
        if (gunData == null) return;
        if (gunData.gunType == GunType.hoveringShot) hoveringShotManager.UpdateHoveringShot(gunData);
        if(this.gunData != null &&  this.gunData.gunType == GunType.hoveringShot)
        {
            hoveringShotManager?.StopShooting();
            handleGunSFX.StopContinuousShootingSfx();
        }

        if (this.gunData != gunData) changeWeaponFeedbacks.PlayFeedbacks(); //Do not replay the sound when switching to the same weapon.
        this.gunData = gunData;
        Refresh();
        CancelReload_TechBooh();
        firePoint.localPosition = gunData.firePointOffset;
         gunTransform.localPosition = gunData.posOffset;


    }

    /// <summary>
    /// TechBooh:
    /// Actually stop the previous reload coroutine.
    /// Previously, only isReloading changed; the old coroutine remained active
    /// and could finish the next reload early using the previous elapsed time.
    /// </summary>
    private void CancelReload_TechBooh(bool triggerEvent = true)
    {
        isReloading = false;

        if (reloadCoroutine_TechBooh != null)
        {
            StopCoroutine(reloadCoroutine_TechBooh);
            reloadCoroutine_TechBooh = null;
        }

        if (triggerEvent)
        {
            OnCancelReloading?.Invoke();
        }
    }

    private void Refresh()
    {
        if (gunData == null) return;
        spriteRenderer.sprite = gunData.sprite;
        shootingHandler.UpdateBulletData(aim, gunData);
        burstSize = gunData.burstSize;
        //raycastShootingHandler.UpdateBulletData(aim, gunData);
        handleGunSFX.SetGunData(gunData);
        OnWeaponChange?.Invoke();
        //maxAmmo = gunData.maxAmmo;
    }

    public bool IsShooting()
    {
        if(isShootButtonHeldDown && gunData.currentAmmoOnClip > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void PlayerHasNoGun()
    {
        SetEquipedGun(null);
        spriteRenderer.sprite = null;
    }

    public void SetEquipedGun( GunData gunData)
    {
        this.gunData = gunData;
        Refresh();
    }

    public GunData GetEquipedGun()
    {
        return gunData;
    }
}
