// TECHBOOH_YODINHA_CONSOLE_REMOVED_V1: applied to a dedicated CONSOLE project copy.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class ChangeCharacter : InteractWithPlayer
{
    [SerializeField] private CharacterData characterData;
    [SerializeField] private MMF_Player changeCharacterFeedback;
    public Phrase changeCharacterPhrase;
    public GameObject azaus, nina, hanba, deNiro, death, dog, yodinha;

    /// <summary>
    /// Map each character to its GameObject.
    /// </summary>
    private Dictionary<Character, GameObject> characterNpcMap;

    /// <summary>
    /// HubManager reference for refreshing NPCs after a character switch.
    /// </summary>
    private HubManager hubManager;

    /// <summary>
    /// Cache this station's original Hub position in Awake and restore it after switching
    /// so repeated interactions do not gradually move the NPC.
    /// </summary>
    private Vector3 originalPosition;

    protected override void Awake()
    {
        base.Awake();
        if (characterData == null)
        {
            Debug.LogError("Character Data is not assigned in " + gameObject.name);
        }

        InitializeCharDictionary();
        hubManager = Object.FindFirstObjectByType<HubManager>();
        originalPosition = transform.position;
    }

    private void InitializeCharDictionary()
    {
        characterNpcMap = new Dictionary<Character, GameObject>
        {
            { Character.azaus, azaus },
            { Character.nina, nina },
            { Character.hanba, hanba },
            { Character.deNiro, deNiro },
            { Character.death, death },
            { Character.dog, dog },
            { Character.yodista, yodinha }
        };
    }

    protected override void StartInteraction()
    {
        if (gameObject.scene.name == TINS.Campaign.CampaignTravel.BaseScene) return;
        base.StartInteraction();
        generalTextBoxUI.SetText(interactionButton +
            " " + changeCharacterPhrase.GetTextByLanguage(languageData.currentLanguage)
            + " " + characterData.GetName(languageData.currentLanguage));
    }

    protected override void InteractionAction()
    {
        if (gameObject.scene.name == TINS.Campaign.CampaignTravel.BaseScene) return;
        base.InteractionAction();
        TriggerChangeCharacter();
    }

    private void TriggerChangeCharacter()
    {
        // TechBooh: reject before feedbacks, relocation or equipment changes.
        if (characterData == null || characterData.character == Character.yodista) return;

        changeCharacterFeedback.PlayFeedbacks();

        var charManager = player.GetComponent<CharacterDataManager>();
        Character previousCharacter = charManager.GetCurrentCharacterData().character;

        Vector3 playerPosition = player.transform.position;
        player.transform.position = transform.position;
        transform.position = playerPosition;

        charManager.SetCharacter(characterData.character);
        SyncCharacterLoadoutInHub(charManager.GetCurrentCharacterData());
        UpdateCharacterNpcVisibility(previousCharacter, characterData.character);

        if (hubManager != null)
        {
            hubManager.RefreshCharacterNpcs();
        }
        else
        {
            Debug.LogWarning("HubManager nao encontrado. NPCs nao foram atualizados apos troca de personagem.");
        }

        transform.position = originalPosition;
        gameObject.SetActive(false);
    }

    private void UpdateCharacterNpcVisibility(Character previousCharacter, Character newCharacter)
    {
        if (characterNpcMap == null)
        {
            return;
        }

        if (characterNpcMap.TryGetValue(previousCharacter, out GameObject previousNpc) && previousNpc != null && previousCharacter != Character.yodista)
        {
            previousNpc.SetActive(true);
        }

        if (characterNpcMap.TryGetValue(newCharacter, out GameObject newNpc) && newNpc != null)
        {
            newNpc.SetActive(false);
        }
    }

    private void SyncCharacterLoadoutInHub(CharacterData currentCharacterData)
    {
        if (currentCharacterData == null)
        {
            Debug.LogWarning("Nao foi possivel sincronizar o loadout do personagem no Hub porque o CharacterData atual e nulo.");
            return;
        }

        var runDataManager = player.GetComponent<RunDataManager>();
        if (runDataManager != null)
        {
            RunData runData = runDataManager.GetRunData();
            runData.currentCharacter = currentCharacterData;
            runData.dashData = currentCharacterData.defaultDash;
        }

        var currentGunInventory = player.GetComponent<GunInventory>();
        if (currentGunInventory != null)
        {
            currentGunInventory.ResetGuns(currentCharacterData.defaultGun);
            currentGunInventory.ResetGuns();
        }

        var currentMeleeInventory = player.GetComponent<MeleeWeaponInventory>();
        if (currentMeleeInventory != null)
        {
            currentMeleeInventory.ChangeMeleeWeapon(currentCharacterData.defaultMelee);
        }

        var currentSpecialInventory = player.GetComponent<SpecialWeaponInventory>();
        if (currentSpecialInventory != null)
        {
            currentSpecialInventory.ChangeSpecialWeapon(currentCharacterData.defaultSpecialAbility);
        }

        foreach (var meleeManager in player.GetComponents<MeleeManager>())
        {
            if (meleeManager == null)
            {
                continue;
            }

            if (meleeManager.GetPlayerAction() == PlayerAction.special)
            {
                meleeManager.RefillActions();
                continue;
            }

            if (meleeManager.GetPlayerAction() == PlayerAction.dash)
            {
                meleeManager.SetMeleeWeapon(currentCharacterData.defaultDash);
            }
        }
    }
}
