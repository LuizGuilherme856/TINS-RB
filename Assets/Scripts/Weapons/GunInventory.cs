using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;
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
    private CharacterData characterData;
    //private int maxGuns= 0;
    private RunData runData;
    public event Action OnWeaponChange;



    private void Awake()
    {
        characterDataHolder = GetComponent<CharacterDataManager>();
        runData = GetComponent<RunDataManager>().GetRunData();

        

        if(enemyWaveManager == null) enemyWaveManager = AssetFinder.GetEnemyWaveManager();

        if (playerClone == false) characterData = characterDataHolder.GetCurrentCharacterData();

        defaultGun = characterData.defaultGun;

        guns.Clear();
        guns = runData.guns;
        OnWeaponChange?.Invoke();

        if (guns.Count == 0) gunManager.SetEquipedGun(null);
        else gunManager.SetEquipedGun(guns[0]);


    }

    private void OnEnable()
    {
        playerInput.OnMouseScrollDown += PreviousGun;
        playerInput.OnMouseScrollUp += NextGun;
        playerInput.OnKey1 += () => EquipGunByIndex(0);
        playerInput.OnKey2 += () => EquipGunByIndex(1);
        playerInput.OnKey3 += () => EquipGunByIndex(2);
        playerInput.OnKey4 += () => EquipGunByIndex(3);

        enemyWaveManager.OnEnemyDied += TryToRefillAmmo;
    }

    private void OnDisable()
    {
        enemyWaveManager.OnEnemyDied -= TryToRefillAmmo;
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
            guns[0].SetFullAmmo();
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
                    guns[i] = newGun;
                    guns[i].SetFullAmmo();
                    Aux.Print("Arma substituida", showDebug);

                }
            }

        }
        else
        {
            guns.Add(newGun);
            guns[GetGunIndex(newGun)].SetFullAmmo();
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

        startingGun.SetFullAmmo();
        startingGun.SetMaxSpareAmmo();
        EquipGun(startingGun);
    }

    public void RemoveCurrentWeapon()
    {
        Aux.Print("Remove gun", showDebug);
        guns.Remove(GetCurrentEquipedGun());

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
        guns[GetGunIndex(gun)].SetMaxSpareAmmo();
        EquipGun(gun);
    }

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
        if (UnityEngine.Random.Range(0f, 1f) <= globalStats.chanceOfFillingAmmoOnKill)
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
            if (g.currentAmmoOnClip <= 0 && g.currentSpareAmmo <= 0)
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

    private int GetGunIndex(GunData gun)
    {
        int index = 0;

        for (int i = 0; i < guns.Count; i++)
        {
            if (guns[i] == gun)
            {
                index = i;
            }
        }

        return index;
    }

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
        if (defaultGun == null)
        {
            gunManager.PlayerHasNoGun();
            EquipGun(null);
            return;
        }
        guns.Add(defaultGun);
        guns[GetGunIndex(defaultGun)].SetFullAmmo();
        guns[GetGunIndex(defaultGun)].SetMaxSpareAmmo();
        EquipGun(defaultGun); // Equip the default gun so the player cannot retain a weapon they no longer own.
    }

    public void ResetGuns(GunData gunData)
    {
        defaultGun = gunData;
        guns.Clear();
        if (defaultGun == null) return;
        guns.Add(defaultGun);
        guns[GetGunIndex(defaultGun)].SetFullAmmo();
        guns[GetGunIndex(defaultGun)].SetMaxSpareAmmo();
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
        
        var maxGuns = (int)(characterData.numberOfGunsItCanCarry + globalStats.increaseWeaponCapacityBy);// + runeUpgradeManager.GetNumberOfExtraGuns();
        if (!playerClone && TINS.Campaign.CampaignCombatRules.IsCampaignMission)
            maxGuns = Mathf.Max(maxGuns, guns.Count);
        return Mathf.FloorToInt(Aux.MinValueToReturn(maxGuns, 1));
    }

    #endregion

}
