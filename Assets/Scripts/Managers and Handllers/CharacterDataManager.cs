// TECHBOOH_YODINHA_CONSOLE_REMOVED_V1: applied to a dedicated CONSOLE project copy.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Store each character's CharacterData and update the selection when characters change.
public class CharacterDataManager : MonoBehaviour
{
    [SerializeField] private CharacterData currentCharacterData;
    [SerializeField] private CharacterData campaignCharacter;
    public List<CharacterData> charactersData;

    private Animator myAnimator;
    private RunData runData;

    public CharacterData GetCurrentCharacterData() { return currentCharacterData; }
    public void SetCharacterData(CharacterData data) => this.currentCharacterData = ResolveConsoleCharacter_TechBooh(data);


    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        runData = GetComponent<RunDataManager>().GetRunData();
        // Initialize in Awake rather than Start so currentCharacterData
        // is ready when other scripts call GetCurrentCharacterData in Start.
        UpdateCharacter();
    }


    private void Start()
    {
        // UpdateCharacter moved to Awake.
    }



    private void UpdateCharacter()
    {
        CharacterData data = ResolveConsoleCharacter_TechBooh(runData.currentCharacter);
        runData.currentCharacter = data; // TechBooh: sanitize legacy selections before applying the Animator.

        // Find CharacterData for the currently selected character.
        //var data = charactersData.Find(cd => cd != null && cd.character == currentChar.character);

        if (data != null)
        {
            myAnimator.runtimeAnimatorController = data.animatorController;
            currentCharacterData = data;
        }
        else
        {
            Debug.LogWarning($"CharacterData não encontrado");
        }
    }
    public void SetCharacter(Character character)
    {
        if (character == Character.yodista) character = Character.azaus; // TechBooh console restriction.

        var data = charactersData.Find(cd => cd != null && cd.character == character);
        runData.currentCharacter = data;
        UpdateCharacter();
    }


    public CharacterData GetPlayerCurrentCharacter()
    {
        if (currentCharacterData == null)
        {
            Debug.LogError("Current Character Data is null. Please set it before accessing.");
            return null;
        }
        return currentCharacterData;
    }


    // TechBooh: a missing/unsupported character falls back to Azaus; never load Yodinha.
    private CharacterData ResolveConsoleCharacter_TechBooh(CharacterData data)
    {
        bool campaignScene = gameObject.scene.name == TINS.Campaign.CampaignTravel.BaseScene ||
            gameObject.scene.name == TINS.Campaign.CampaignTravel.MissionScene;
        if (campaignScene && campaignCharacter != null) return campaignCharacter;
        if (!campaignScene && data != null && data.character != Character.yodista && data.character != Character.lich)
            return data;
        var fallback = charactersData.Find(item => item != null && item.character == Character.azaus);
        if (fallback == null) throw new System.InvalidOperationException("Console character fallback Azaus is missing.");
        return fallback;
    }
}
