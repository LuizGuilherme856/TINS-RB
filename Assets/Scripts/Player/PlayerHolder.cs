using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Central player reference to avoid repeated scene searches.
//Assign the player in one place.
public class PlayerHolder : MonoBehaviour
{
    [SerializeField] private GameObject playerGameObject;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Health playerHealth;
    [SerializeField] private RunDataManager runDataManager;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private GunInventory gunInventory;
    [SerializeField] private MeleeManager meleeManager;
    [SerializeField] private MeleeManager specialMeleeManager;
    [SerializeField] private RevivePlayer revivePlayer;
    [SerializeField] private PlayerDeath playerDeath;
    [SerializeField] private MeleeManager dashManager;

    [field: SerializeField] public MeleeWeaponInventory meleeWeaponInventory { get; private set; }

    public PlayerData playerData;



    public PlayerDeath GetPlayerDeathScript()
    {
        return playerDeath;
    }

    public RevivePlayer GetRevivePlayer() 
    { 
        return revivePlayer; 
    }
    public GameObject GetPlayerGameObject()
    { 
        return playerGameObject; 
    }

    public Health GetPlayerHealth()
    {
        return playerHealth;
    }

    public RunDataManager GetRunDataManager()
    {
        return runDataManager;
    }

    public PlayerInputHandler GetPlayerInputHandler()
    {
        return playerInputHandler;
    }

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    public GunInventory GetGunInventory()
    {
        return gunInventory;
    }

    public MeleeManager GetSpecialMeleeManager()
    {
        return specialMeleeManager;
    }

    public MeleeManager GetMeleeManager()
    {
        return meleeManager;
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return playerGameObject.GetComponent<SpriteRenderer>();
    }
    public MeleeManager GetDashManager()
    {
        return dashManager;
    }

    public CharacterData GetCharacterData()
    {
        return playerGameObject.GetComponent<CharacterDataManager>().GetCurrentCharacterData();
    }
}
