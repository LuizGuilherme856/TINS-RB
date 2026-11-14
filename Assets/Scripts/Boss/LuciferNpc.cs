using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class LuciferNpc : MonoBehaviour
{

    public List<Dialogue> dialogueLines;
    public Dialogue facedGaelDialogue;
    public Dialogue reachedHeavenDialogue;
    [Space(15)]
    [Tooltip("Default line to show when no specific dialogue is available")]
    public PlayerData playerData; // Reference to the PlayerData scriptable object
    public DialogueSystem2 dialogueSystem; // Reference to the DialogueSystem2 script
    public Sprite luciferPortrait;

    [Tooltip("Index to always force a specific dialogue line, -1 means no forced dialogue")]
    public int forceDialogIndex = -1;

    [Header("Chalice stuff")]
    public int chaliceLineIndex;
    public GameObject chalice;
    

    public LanguageData languageData;
    public DialogueLine strangerName;

    public bool dialogueEnded { get; private set; } = false; // Flag to check if the dialogue has ended

    private void OnEnable()
    {
        dialogueSystem.OnDialogueEnd += () => dialogueEnded = true; // Subscribe to the dialogue end event

        if (chalice)
        {
            if (playerData.luciferLinesSpoken == 0) dialogueSystem.OnLineEnd += CheckSpawnChalice;
            else dialogueSystem.OnLineEnd += DoubleCheckChaliceAchievement;
        }
    }

    private void CheckSpawnChalice()
    {
        if (dialogueSystem.GetCurrentLineIndex() == chaliceLineIndex)
        {
            chalice.SetActive(true);
            playerData.TriggerCheckFirstLine(); //Achivement Related
        }

    }

    /// <summary>
    /// Retry granting the chalice if the first attempt failed.
    /// </summary>
    private void DoubleCheckChaliceAchievement()
    {
        playerData.TriggerCheckFirstLine();
    }

    public void StartDialogue()
    {
        Dialogue dialogue;
        if (forceDialogIndex >= 0 && forceDialogIndex < dialogueLines.Count)
        {
            // If a specific dialogue line is forced, use it
            dialogue = dialogueLines[forceDialogIndex];
        }
        else
        {
            // Otherwise, use the next available dialogue line
            dialogue = GetDialogue();
        }

        if (dialogue != null)
        {
            dialogueSystem.gameObject.SetActive(true); // Ensure the dialogue system is active
            dialogueSystem.StartDialogue(dialogue, strangerName.GetLine(languageData.currentLanguage), luciferPortrait); // Start the dialogue with Lucifer
        }
    }

    public Dialogue GetDialogue()
    {
        int index = playerData.luciferLinesSpoken;
        if (index < 0 || index >= dialogueLines.Count) return null;

        //Special dialogue cases.
        if (playerData.playerFacedGael == 1)
        {
            playerData.playerFacedGael = 2;
            return facedGaelDialogue;
        }
        else if(playerData.playerReachedHeaven == 1)
        {
            playerData.playerReachedHeaven = 2;
            return reachedHeavenDialogue;
        }

        playerData.luciferLinesSpoken++; // Increment the line spoken count
        return dialogueLines[index];
    }

    public bool HasDialogue()
    {
        int index = playerData.luciferLinesSpoken;
        if (index < 0 || index >= dialogueLines.Count)
        {
            return false;
        }
        else 
        {
            return true;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Melee") || collision.gameObject.CompareTag("Bullet"))
        {
            playerData.TriggerLuciHit();
        }
    }
}
