// TECHBOOH_YODINHA_CONSOLE_REMOVED_V1: applied to a dedicated CONSOLE project copy.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechBooh;

/// <summary>
/// Handle Hub arrival dialogue, character unlocks,
/// and camera focus on newly unlocked characters.
/// </summary>
public class HubManager : MonoBehaviour
{
    [Header("NPCs")]
    public LuciferNpc luciferNpc; // Reference to the Lucifer NPC script
    public GameObject skillTreeNpc;
    public GameObject healerNpc;
    public GameObject shopNpc;
    public GameObject tigerNpc, endlessModeStation;
    public GameObject nina, hanba, azaus, deNiro, death, dog, yodinha; // Characters that can be unlocked
    [Header("path Blockers")]
    public GameObject ninaBlocker; // Blocker for Nina NPC, used to prevent interaction before unlocking
    public GameObject hanbaBlocker; // Blocker for Hanba NPC, used to prevent interaction before unlocking
    public GameObject deNiroBlocker;
    public GameObject skillTreeBlocker;
    public GameObject wellBlocker; //Block access to the Endless mode well.

    [Space(10)]
    [Header("Unlock conditions")]
    public int deathsToUnlockNina = 5; // Number of deaths required to unlock Nina
    public int deathToUnlockSkillTree= 1; // Number of deaths required to unlock the Skill Tree NPC 
    public int deathsToUnlockDeNiro = 20; // Number of deaths required to unlock De Niro NPC
    public int deathsToUnlockHanba = 15; // Number of deaths required to unlock Hanba
    public int killsToUnlockNina = 100; // Number of kills required to unlock Nina
    public int killsToUnlockMiki = 2000;

    [Space(10)]
    [Header("Camera movement parameters")]
    public AnimationCurve cameraMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float cameraMoveDuration = 1f;
    public float delayToStartDialogue = 2f; // Delay before starting the dialogue after entering the hub
    public float delayToFocusOnObjects = 1f; // Delay before focusing on each object in the hub


    [Space(15)]
    public PlayerHolder playerHolder; // Reference to the PlayerHolder script to access player data and input handler
    public DlcManager dlcManager;

    [Space(15)]
    [Header("Debug")]
    public GameObject teleportObj;

    //private stuff
    /// <summary>
    /// List of objects to focus the camera on when they are unlocked or activated.
    /// </summary>
    private List<Transform> objToFocus = new();
    private MoveObjectToPos moveObjectToPos; // Reference to the MoveObjectToPos script for moving objects
    private PlayerData playerData; // Reference to the PlayerData scriptable object
    private PlayerInputHandler playerInputHandler; // Reference to the PlayerInputHandler script for player input
    private GunInventory gunInventory; // Reference to the GunInventory script for managing guns
    private void Awake()
    {
        playerData = playerHolder.playerData; // Get the PlayerData from PlayerHolder
        playerInputHandler = playerHolder.GetPlayerInputHandler(); // Get the PlayerInputHandler from PlayerHolder
        moveObjectToPos = Try.GetComponent<MoveObjectToPos>(gameObject); // Get the MoveObjectToPos component attached to this GameObject
        gunInventory = playerHolder.GetGunInventory(); // Get the GunInventory from PlayerHolder

        playerInputHandler.OnCheat += DebugModeOn; // Subscribe to the cheat event to enable debug mode
        playerHolder.GetSpecialMeleeManager()?.SetAttacksEnabled(true);
    }

    // Start is called before the first frame update
    void Start()
    {

        gunInventory.ResetGuns(); // Reset the gun inventory when entering the hub

        //Reaching the Hub means the tutorial is complete.
        playerData.isTutorialFinished = true; // Set the tutorial as finished when entering the hub


        // Calculate progression first, then use the persistent unlock flags as the
        // single source of truth for both characters and their Hub paths.
        CheckUnlocks();
        ApplyUnlockState_TechBooh();

        DisablePlayerCharacterNpc();
        
        SaveSystem.SaveData(playerData); // Save the updated player data

        if (playerData.numberOfTimesKilledLucifer > 0)
        {
            luciferNpc.gameObject.SetActive(false); // If the player has killed Lucifer before, disable the Lucifer NPC}
        }

        StartCoroutine(StartDialogue_Routine());
        
    }


/// <summary>
/// Show or hide character NPCs according to the currently selected character.
/// May be called externally, such as by ChangeCharacter, to resynchronize after switching.
/// </summary>
    public void RefreshCharacterNpcs()  => DisablePlayerCharacterNpc();
    private void DisablePlayerCharacterNpc()
    {
        // Array order matches the Character enum.
        GameObject[] npcs = { azaus, nina, hanba, deNiro, death, dog, yodinha };
        Character current = playerHolder.GetCharacterData().character;
        bool hasDlc = dlcManager != null && dlcManager.CheckDlc();

        for (int i = 0; i < npcs.Length; i++)
        {
            if (npcs[i] != null)
            {
                bool unlocked = true;
                // Check whether the NPC is unlocked.
                switch (i)
                {
                    case 0: unlocked = true; break; // Azaus is always unlocked.
                    case 1: unlocked = playerData.isNinaUnlocked; break;
                    case 2: unlocked = playerData.isHanbaUnlocked; break;
                    case 3: unlocked = playerData.isDeNiroUnlocked; break;
                    case 4: unlocked = true; break; // Death currently has no unlock condition.
                    case 5: unlocked = playerData.isMikiUnlocked; break;
                    case 6: unlocked = false; break; // TechBooh: Yodinha is unavailable on consoles.
                }
                npcs[i].SetActive(unlocked && (int)current != i);
            }
        }
    }

    private void CheckUnlocks()
    {
        if (!playerData.isNinaUnlocked && playerData.deaths >= deathsToUnlockNina && playerData.totalKillsSoFar >= killsToUnlockNina)
        {
            playerData.UnlockNina(); // Unlock Nina if conditions are met
            AddFocusTarget_TechBooh(nina); // Add Nina's position to the focus list
        }

        if (!playerData.isHanbaUnlocked && playerData.deaths >= deathsToUnlockHanba)
        {
            playerData.UnlockHanba(); // Unlock Hanba if conditions are met
            AddFocusTarget_TechBooh(hanba); // Add Hanba's position to the focus list
        }

        // De Niro's persistent Hub unlock depends only on deaths.
        // The presentation/purchase requiring 10,000 kills is handled elsewhere.
        if (!playerData.isDeNiroUnlocked && playerData.deaths >= deathsToUnlockDeNiro)
        {
            playerData.UnlockDeNiro();
            AddFocusTarget_TechBooh(deNiro);
        }

        if (playerData.deaths >= deathToUnlockSkillTree && playerData.isSkillTreeUnlocked == false)
        {
            playerData.isSkillTreeUnlocked = true; // Unlock the skill tree if the NPC is safe
            AddFocusTarget_TechBooh(skillTreeNpc); // Add Skill Tree NPC's position to the focus list
        }
        ///dog miki
        if (playerData.totalKillsSoFar >= killsToUnlockMiki && playerData.isMikiUnlocked == false)
        {
            playerData.UnlockMiki();
            AddFocusTarget_TechBooh(dog);
        }

        if(playerData.numberOfTimesKilledLucifer > 0 && playerData.showedEndlessModeStation == false)
        {
            AddFocusTarget_TechBooh(endlessModeStation);
            playerData.showedEndlessModeStation = true;
        }
    }

    /// <summary>
    /// TechBooh: Synchronize every persistent Hub unlock from the saved flags.
    /// Previously, characters used unlock flags while paths recalculated death counters,
    /// allowing the character and its region to disagree after a save migration or load.
    /// </summary>
    private void ApplyUnlockState_TechBooh()
    {
        SetActiveSafely_TechBooh(skillTreeNpc, playerData.isSkillTreeUnlocked);
        SetActiveSafely_TechBooh(healerNpc, playerData.isHealerNpcUnlocked);
        SetActiveSafely_TechBooh(shopNpc, playerData.isShopNpcUnlocked);
        SetActiveSafely_TechBooh(tigerNpc, playerData.isTigerNpcUnlocked);

        SetActiveSafely_TechBooh(nina, playerData.isNinaUnlocked);
        SetActiveSafely_TechBooh(hanba, playerData.isHanbaUnlocked);
        SetActiveSafely_TechBooh(deNiro, playerData.isDeNiroUnlocked);
        SetActiveSafely_TechBooh(dog, playerData.isMikiUnlocked);

        SetActiveSafely_TechBooh(skillTreeBlocker, !playerData.isSkillTreeUnlocked);
        SetActiveSafely_TechBooh(ninaBlocker, !playerData.isNinaUnlocked);
        SetActiveSafely_TechBooh(hanbaBlocker, !playerData.isHanbaUnlocked);
        SetActiveSafely_TechBooh(deNiroBlocker, !playerData.isDeNiroUnlocked);
        SetActiveSafely_TechBooh(wellBlocker, playerData.numberOfTimesKilledLucifer <= 0);

        if(playerData.numberOfTimesKilledLucifer > 0)
        {
            // Defeating Lucifer also opens these paths independently of character progression.
            SetActiveSafely_TechBooh(deNiroBlocker, false);
            SetActiveSafely_TechBooh(skillTreeBlocker, false);
        }
    }

    private void AddFocusTarget_TechBooh(GameObject target)
    {
        if (target != null)
        {
            objToFocus.Add(target.transform);
        }
        else
        {
            Debug.LogError("HubManager could not focus an unlocked object because its reference is missing.", this);
        }
    }

    private void SetActiveSafely_TechBooh(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
        else
        {
            Debug.LogError("HubManager could not apply an unlock state because a GameObject reference is missing.", this);
        }
    }

    private IEnumerator StartDialogue_Routine()
    {
        playerInputHandler.SetCanMove(false);

        var objMoveData = new ObjMoveData(luciferNpc.transform, cameraMoveCurve, cameraMoveDuration); // Create move data for each object
        
        if (luciferNpc.HasDialogue() && playerData.numberOfTimesKilledLucifer == 0)
        {
            yield return StartCoroutine(moveObjectToPos.MoveObject(objMoveData));

            yield return new WaitForSeconds(delayToStartDialogue); // Wait for a second before starting the dialogue
            luciferNpc.StartDialogue(); // Start the dialogue with Lucifer NPC

            yield return new WaitUntil(() => luciferNpc.dialogueEnded); // Wait until the dialogue ends
        }
        else
        {
            yield return new WaitForSeconds(delayToStartDialogue/2);
        }

            StartCoroutine(FocusCamera(objMoveData));
    }

    private IEnumerator FocusCamera(ObjMoveData objMoveData)
    {
        playerInputHandler.SetCanMove(false);
        if (objToFocus.Count > 0)
        {
            yield return StartCoroutine(FocusCamera_Routine()); //Wait until all focus targets have been shown.
        }

        //Focus on the player last.
        objMoveData.targetPosition = playerInputHandler.transform; // Set the target position to the player
        yield return StartCoroutine(moveObjectToPos.MoveObject(objMoveData)); //Wait for player focus.

        //The player can move freely.
        playerInputHandler.SetCanMove(true); // Allow player movement after focusing the camera
    }

    private IEnumerator FocusCamera_Routine()
    {
        foreach (var obj in objToFocus)
        {
            var objMoveData = new ObjMoveData(obj, cameraMoveCurve, cameraMoveDuration); // Create move data for each object
            yield return StartCoroutine( moveObjectToPos.MoveObject(objMoveData));
            yield return new WaitForSeconds(delayToFocusOnObjects); // Wait for a second before moving to the next object
        }


    }

    private void DebugModeOn()
    {
        teleportObj.SetActive(true); // Activate the teleport object for debugging purposes
    }

}
