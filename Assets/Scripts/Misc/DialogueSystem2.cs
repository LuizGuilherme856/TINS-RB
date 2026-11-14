using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem2 : MonoBehaviour
{

    [HideInInspector] public string playerName; // Name of the player
    [HideInInspector] public Sprite playerPortrait;

    [Space(10)]
    [HideInInspector] public string npcName; // Name of the NPC
    [HideInInspector] public Sprite npcPortrait; // Portrait of the NPC

    [Space(10)]
    public Dialogue dialogue; // List of dialogues for the NPC
    public PlayerData playerData; // Reference to the PlayerData scriptable object

    [Space(10)]
    public TextMeshProUGUI dialogueText; // Text component to display the dialogue
    public GameObject playerPanel, npcPanel; // Panel showing the current speaker's name and portrait, whether player or NPC.
    public TextMeshProUGUI playerNameText, npcNameText;
    public Image playerPortraitImage, npcPortraitImage;

    [Space(10)]
    public LanguageData languageData;

    public float wordDelay = 0.08f; // Delay between words.

    public PlayerHolder playerHolder;
    public CharacterPortraitHUD characterPortraitHUD; // CharacterPortraitHUD reference for updating the selected character portrait.

    public MMF_Player talkFeedback, skipLineFeedback;
    private PlayerInputHandler playerInputHandler;

    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;
    private RunData runData;
    private bool isTyping = false;

    public event Action OnDialogueEnd; // Event raised when dialogue ends.
    public event Action OnLineEnd;

    private void Awake()
    {
        // Get PlayerInputHandler through PlayerHolder.
        playerInputHandler = playerHolder.GetPlayerInputHandler();
        if (playerInputHandler == null)
        {
            Debug.LogError("PlayerInputHandler not found on PlayerHolder.");
        }

        runData = playerHolder.GetRunDataManager().GetRunData();


    }


    public void StartDialogue(Dialogue newDialogue, string npcName, Sprite npcPortrait)
    {
        try
        {
            // Disable player movement.
            try { playerInputHandler.SetCanMove(false); }
            catch (Exception e) { Debug.LogError($"[DialogueSystem2] Erro ao desativar movimento do jogador: {e.Message}"); }

            // Read the current character's name from RunData.
            try { playerName = runData.currentCharacter.shortName; }
            catch (Exception e) { Debug.LogError($"[DialogueSystem2] Erro ao obter nome do personagem: {e.Message}"); }

            // Set the current dialogue.
            dialogue = newDialogue;

            // Set the NPC name and portrait.
            this.npcName = npcName;
            this.npcPortrait = npcPortrait;

            // Get the current character portrait.
            try { playerPortrait = runData.currentCharacter.dialoguePortrait; }
            catch (Exception e) { Debug.LogError($"[DialogueSystem2] Erro ao obter retrato do personagem: {e.Message}"); }

            // Subscribe to interaction input for advancing dialogue.
            try { playerInputHandler.OnInteract += SkipDialogue; }
            catch (Exception e) { Debug.LogError($"[DialogueSystem2] Erro ao adicionar evento de interação: {e.Message}"); }

            // Start at the first dialogue line.
            currentLineIndex = 0;
            ShowCurrentLine();
        }
        catch (Exception e)
        {
            Debug.LogError($"[DialogueSystem2] Erro inesperado ao iniciar diálogo: {e.Message}\nStackTrace: {e.StackTrace}");
            EndDialogue();
        }
    }

    private void SkipDialogue()
    {
        skipLineFeedback.PlayFeedbacks();
        if (isTyping)
        {
            // Show the complete line immediately.
            CompleteCurrentLine();

        }
        else
        {
            NextLine();
        }
        OnLineEnd?.Invoke();
    }

    void ShowCurrentLine()
    {
        if (dialogue == null || currentLineIndex >= dialogue.LineCount)
        {
            EndDialogue();
            return;
        }

        var line = dialogue.GetLineAt(currentLineIndex);
        if (line == null)
        {
            EndDialogue();
            return;
        }

        // Set name and portrait.
        if (line.participant == DialogueParticipant.Player)
        {
            npcPanel.SetActive(false); // Hide the NPC panel.
            playerPanel.SetActive(true);
            playerNameText.text = playerName;
            playerPortraitImage.sprite = playerPortrait;
        }
        else
        {
            playerPanel.SetActive(false); // Hide the player panel.
            npcPanel.SetActive(true);
            npcNameText.text = npcName;
            npcPortraitImage.sprite = npcPortrait;
        }

        // Start the typewriter effect.
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(line.GetLine(languageData.currentLanguage)));

    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        string[] words = text.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            talkFeedback.PlayFeedbacks();
            dialogueText.text += words[i];
            if (i < words.Length - 1)
                dialogueText.text += " ";
            yield return new WaitForSeconds(wordDelay);
        }
        isTyping = false;
        OnLineEnd?.Invoke();
    }

    private void CompleteCurrentLine()
    {
        if (dialogue == null || currentLineIndex >= dialogue.LineCount)
            return;

        var line = dialogue.GetLineAt(currentLineIndex);
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        dialogueText.text = line.GetLine(languageData.currentLanguage);
        isTyping = false;
    }

    public void NextLine()
    {
        currentLineIndex++;
        ShowCurrentLine();
    }

    public int GetCurrentLineIndex()
    {
        return currentLineIndex;
    }

    private void EndDialogue()
    {
        playerInputHandler.OnInteract -= SkipDialogue;
        OnDialogueEnd?.Invoke();
        playerInputHandler.SetCanMove(true); // Restore player movement.
        // Hide the dialogue panel or perform additional cleanup here.
        dialogueText.text = "";
        playerNameText.text = "";
        playerPortraitImage.sprite = null;
        gameObject.SetActive(false); // Deactivate the dialogue system.
    }

}
