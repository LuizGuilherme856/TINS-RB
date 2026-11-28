using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GunInventory : MonoBehaviour
{
    [SerializeField] private GunManager gunManager;

    [SerializeField] private bool showDebug = false;
    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private bool playerClone = false;
    [SerializeField] private GlobalStats globalStats;

    /// <summary>
    /// Used by skills that refill player ammunition.
    /// </summary>
    [SerializeField] private MMF_Player refillAmmoFeedback;

    [SerializeField] private EnemyWaveManager enemyWaveManager;
    private GunData defaultGun;
    private CharacterDataManager characterDataHolder;
    private List<GunData> guns = new();
    private sealed class RunAmmo
    {
        public GunInventory Owner;
        public readonly Dictionary<GunData, GunAmmo> Weapons = new();
    }

    private static ConditionalWeakTable<RunData, RunAmmo> legacyAmmoByRun = new();
    private RunAmmo legacyAmmo;
    private Dictionary<GunData, GunAmmo> ammoByGun = new();
    internal bool IsCampaignLoadout { get; private set; }
    private CharacterData characterData;
    //private int maxGuns= 0;
    private RunData runData;
    public event Action OnWeaponChange;



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetLegacyAmmo() => legacyAmmoByRun = new();

    public GunAmmo GetAmmo(GunData gun)
    {
        if (gun == null) return null;
        if (!ammoByGun.TryGetValue(gun, out GunAmmo ammo))
        {
            ammo = new GunAmmo();
            ammo.Reset(gun);
            ammoByGun.Add(gun, ammo);
        }
        return ammo;
    }

    internal bool IsStartingGun(GunData gun) => gun != null && gun == defaultGun;

    private void Awake()
    {
        characterDataHolder = GetComponent<CharacterDataManager>();
        runData = GetComponent<RunDataManager>()?.GetRunData();

        

        if(enemyWaveManager == null) enemyWaveManager = AssetFinder.GetEnemyWaveManager();

        characterData = characterDataHolder != null ? characterDataHolder.GetCurrentCharacterData() : runData?.currentCharacter;
        defaultGun = characterData != null ? characterData.defaultGun : null;
        // The original game's player restores ammunition across scene changes.
        if (!playerClone && runData != null)
        {
            RunAmmo session = legacyAmmoByRun.GetOrCreateValue(runData);
            if (session.Owner == null)
            {
                session.Owner = this;
                legacyAmmo = session;
                ammoByGun = session.Weapons;
                guns = runData.guns ??= new List<GunData>();
            }
        }
        if (legacyAmmo == null && runData?.guns != null) guns.AddRange(runData.guns);
        OnWeaponChange?.Invoke();

        if (guns.Count == 0) gunManager.SetEquipedGun(null);
        else gunManager.SetEquipedGun(guns[0]);


    }

    private void OnEnable()
    {
        playerInput.OnMouseScrollDown += PreviousGun;
        playerInput.OnMouseScrollUp += NextGun;
        playerInput.OnKey1 += EquipFirstGun;
        playerInput.OnKey2 += EquipSecondGun;
        playerInput.OnKey3 += EquipThirdGun;
        playerInput.OnKey4 += EquipFourthGun;

        if (enemyWaveManager != null) enemyWaveManager.OnEnemyDied += TryToRefillAmmo;
    }

    private void OnDisable()
    {
        playerInput.OnMouseScrollDown -= PreviousGun;
        playerInput.OnMouseScrollUp -= NextGun;
        playerInput.OnKey1 -= EquipFirstGun;
        playerInput.OnKey2 -= EquipSecondGun;
        playerInput.OnKey3 -= EquipThirdGun;
        playerInput.OnKey4 -= EquipFourthGun;
        if (enemyWaveManager != null) enemyWaveManager.OnEnemyDied -= TryToRefillAmmo;
    }

    private void EquipFirstGun() => EquipGunByIndex(0);
    private void EquipSecondGun() => EquipGunByIndex(1);
    private void EquipThirdGun() => EquipGunByIndex(2);
    private void EquipFourthGun() => EquipGunByIndex(3);

    private void OnDestroy() => ReleaseLegacyAmmo();

    private void ReleaseLegacyAmmo()
    {
        if (legacyAmmo == null) return;
        if (legacyAmmo.Owner == this) legacyAmmo.Owner = null;
        legacyAmmo = null;
    }

    private void Start()
    {
        EquipGun(GetCurrentEquipedGun());//Workaround for an observed bug.
    }

    public void AddGunToInventory(GunData newGun)
    {
        if (GetCurrentEquipedGun() == null) //No weapon is owned.
        {
            guns.Clear();
            guns.Add(newGun);
            GetAmmo(newGun).Reset(newGun);
        }
        else if (guns.Count >= GetMaxGuns())
        {
            Aux.Print("Ja tem o numero maximo de armas equipadas", showDebug);
            Aux.Print("...\n Substituindo arma atual", showDebug);

            GunData currentGun = gunManager.GetEquipedGun();
            for (int i = 0; i < guns.Count; i++)
            {
                if (guns[i] == currentGun)
                {
                    ammoByGun.Remove(currentGun);
                    guns[i] = newGun;
                    GetAmmo(newGun).Reset(newGun);
                    Aux.Print("Arma substituida", showDebug);

                }
            }

        }
        else
        {
            guns.Add(newGun);
            GetAmmo(newGun).Reset(newGun);
        }

        EquipGun(newGun);
    }

    /// <summary>Guarantees that a configured starting weapon is owned and equipped on player spawn.</summary>
    public void EnsureStartingGunEquipped(GunData startingGun)
    {
        if (startingGun == null) return;
        if (guns == null) guns = new List<GunData>();

        if (!guns.Contains(startingGun))
            guns.Insert(0, startingGun);
        else
        {
            guns.Remove(startingGun);
            guns.Insert(0, startingGun);
        }

        GetAmmo(startingGun).Reset(startingGun);
        EquipGun(startingGun);
    }

    public void RemoveCurrentWeapon()
    {
        Aux.Print("Remove gun", showDebug);
        GunData equipped = GetCurrentEquipedGun();
        if (equipped != null) ammoByGun.Remove(equipped);
        guns.Remove(equipped);

        if (guns.Count == 0) gunManager.PlayerHasNoGun();
        else EquipGunByIndex(0);
    }

    /// <summary>
    /// Return true if the weapon is already in the inventory.
    /// </summary>
    public bool HasThisGun(GunData gun)
    {
        if (guns.Contains(gun)) return true;
        else return false;
    }
    public bool IsInventoryFull()
    {
        return guns.Count == GetMaxGuns();
    }

    #region Ammo

    /// <summary>
    /// Refill the specified weapon's ammunition.
    /// </summary>
    public void RefillAmmo(GunData gun)
    {
        if (gun == null || !HasThisGun(gun)) return;
        RefillReserve(gun);
        EquipGun(gun);
    }

    public void RefillReserve(GunData gun) => GetAmmo(gun)?.RefillReserve(gun);

    public void RefillAmmoCurrentGun(bool playFeedback = false)
    {
        if (playFeedback) refillAmmoFeedback.PlayFeedbacks();
        RefillAmmo(GetCurrentEquipedGun());
    }

    public void RefillAmmoAllGuns()
    {
        foreach (var gunItem in guns)
        {
            RefillAmmo(gunItem);
        }
    }

    /// <summary>
    /// Attempt an ammunition refill using the skill's probability.
    /// </summary>
    private void TryToRefillAmmo()
    {
        if (UnityEngine.Random.Range(0f, 1f) <= globalStats.ForCurrentRun.chanceOfFillingAmmoOnKill)
        {
            RefillAmmoCurrentGun(true);
        }

    }

    // Return true if any owned weapon has an empty magazine and no reserve ammunition.
    public bool AnyGunWithoutAmmo()
    {
        if (guns == null || guns.Count == 0) return false;

        foreach (var g in guns)
        {
            if (g == null) continue;
            GunAmmo ammo = GetAmmo(g);
            if (ammo.Rounds <= 0 && ammo.Reserve <= 0)
                return true;
        }

        return false;
    }

    #endregion

    #region Change gun
    private void NextGun()
    {
        Aux.Print("Next gun", showDebug);
        if (guns.Count > 1)
        {
            int currentGunIndex = GetCurrentGunIndex();

            if (currentGunIndex < GetMaxGuns() - 1 && guns.Count >= currentGunIndex + 2)
            {
                EquipGun(guns[currentGunIndex + 1]);
            }
            else
            {
                EquipGun(guns[0]);
            }
        }
    }

    private void PreviousGun()
    {
        Aux.Print("Previous Gun", showDebug);
        if (guns.Count > 1)
        {
            int currentGunIndex = GetCurrentGunIndex();

            if (currentGunIndex > 0)
            {
                EquipGun(guns[currentGunIndex - 1]);
            }
            else
            {
                EquipGun(guns[guns.Count - 1]);
            }
        }

    }


    private void EquipGunByIndex(int index)
    {
        if (index + 1 <= guns.Count)
        {
            EquipGun(guns[index]);
        }
    }

    private void EquipGun(GunData gun)
    {
        gunManager.ChangeGun(gun);
        OnWeaponChange?.Invoke();
    }

    #endregion

    #region Getters and Setters

    private int GetCurrentGunIndex()
    {
        int currentGunIndex = 0;
        if (guns.Count > 1)
        {
            for (int i = 0; i < guns.Count; i++)
            {
                if (guns[i] == gunManager.GetEquipedGun())
                {
                    currentGunIndex = i;
                }
            }
        }

        return currentGunIndex;
    }

    public GunData GetCurrentEquipedGun()
    {
        return gunManager.GetEquipedGun();
    }

    public void ResetGuns()
    {
        guns.Clear();
        ammoByGun.Clear();
        if (defaultGun == null)
        {
            gunManager.PlayerHasNoGun();
            EquipGun(null);
            return;
        }
        guns.Add(defaultGun);
        GetAmmo(defaultGun).Reset(defaultGun);
        EquipGun(defaultGun); // Equip the default gun so the player cannot retain a weapon they no longer own.
    }

    public void ResetGuns(GunData gunData)
    {
        defaultGun = gunData;
        guns.Clear();
        ammoByGun.Clear();
        if (defaultGun == null) return;
        guns.Add(defaultGun);
        GetAmmo(defaultGun).Reset(defaultGun);
    }

    internal void BeginCampaignLoadout(GunData gunData)
    {
        IsCampaignLoadout = true;
        ReleaseLegacyAmmo();
        guns = new List<GunData>();
        ammoByGun = new Dictionary<GunData, GunAmmo>();
        ResetGuns(gunData);
    }

    public void SetGuns(List<GunData> guns)
    {
        this.guns.Clear();
        this.guns = guns;
    }

    public List<GunData> GetGuns()
    {
        return guns;
    }


    public int GetMaxGuns()
    {
        
        var maxGuns = (int)(characterData.numberOfGunsItCanCarry + globalStats.ForCurrentRun.increaseWeaponCapacityBy);// + runeUpgradeManager.GetNumberOfExtraGuns();
        if (!playerClone && TINS.Campaign.CampaignCombatRules.IsCampaignMission)
            maxGuns = Mathf.Max(maxGuns, guns.Count);
        return Mathf.FloorToInt(Aux.MinValueToReturn(maxGuns, 1));
    }

    #endregion

}
