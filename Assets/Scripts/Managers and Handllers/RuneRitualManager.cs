using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coordinate the player's rune-path selection.
/// </summary>
public class RuneRitualManager : InteractWithPlayer
{
    [Space(10)]
    [Header("   Blood Settings")]
    [SerializeField] private RuneRitualHolder runeRitual;
    [SerializeField] private Animator bloodStationNotActivateAnimator;
    [SerializeField] private MMF_Player interactionFeedbacks, unlockFeedbacks, buyFeedbacks;


    [SerializeField]private ShowRunePathInfoOnUI showRunePathInfoOnUI;
    private BuyRune buyRune;

    private Health playerHealth;

    public event Action OnBuyRune;

    private bool unlocked = false;

    protected override void Awake()
    {
        base.Awake();
        ResolveRunePathUi_TechBooh();
        buyRune = GetComponent<BuyRune>();

        if (runeRitual == null)
        {
            Debug.LogError($"A station de ritual {gameObject.name} não possui um RuneRitualHolder.");
        }
    }

    private void Start()
    {
        ResolveRunePathUi_TechBooh();
    }
    protected override void InteractionAction() //When the player presses interact inside the trigger.
    {
        base.InteractionAction();

        if (isActiveAndEnabled == false || interactionsActionsCount > 1)
        {
            return;
        }
        interactionFeedbacks.PlayFeedbacks();
        
        if (showRunePathInfoOnUI == null)
        {
            ResolveRunePathUi_TechBooh();

            if (showRunePathInfoOnUI == null)
            {
                Debug.LogError("ShowRunePathInfoOnUI não encontrado.");
                return;
            }
        }

        unlocked = true;
        showRunePathInfoOnUI.ShowInfoOnScreen(runeRitual, buyRune, this);

        //Protect the player from damage during rune selection.
        playerHealth.invulnerable = true;
        showRunePathInfoOnUI.OnRunePathPanelIsNotVisible += RemovePlayerInvulnerability;
        generalTextBoxUI.RemoveText();


    }

    private void RemovePlayerInvulnerability()
    {

        playerHealth.invulnerable = false;
        
    }

    //this methods are overrided in the classes 
    protected override void StartInteraction() //When the player enters the trigger.
    {

        if (unlocked) return;
        base.StartInteraction();
        generalTextBoxUI.SetText(GetUnlockRuneText());

        playerHealth = player.GetComponent<Health>();

        if (playerHealth == null)
        {
            Debug.LogError($"nao conseguiu pegar a Health do player.");
        }
    }

    private string GetUnlockRuneText()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                return interactionButton + " Unlock Rune";
            case Language.chinese:
                return interactionButton + " 解锁符文";
            case Language.spanish:
                return interactionButton + " Desbloquear Runa";
            case Language.portuguese:
            default:
                return interactionButton + " Desbloquear Runa";
        }
    }

    /// <summary>
    /// Play the animation that removes the locked station.
    /// </summary>
    public void UnlockRune()
    {
        bloodStationNotActivateAnimator.SetTrigger("unlock");
        buyFeedbacks.PlayFeedbacks();
        unlockFeedbacks.PlayFeedbacks();
        OnBuyRune?.Invoke();
    }

    public RuneRitualHolder GetRuneRitualHolder()
    {
        return runeRitual;
    }

    /// <summary>
    /// TechBooh:
    /// The ritual-rune UI may not share the object tagged Canvas.
    /// First search active and inactive scene instances, then
    /// search children of the Canvas-tagged object.
    /// </summary>
    private void ResolveRunePathUi_TechBooh()
    {
        if (showRunePathInfoOnUI != null) return;

        showRunePathInfoOnUI = FindFirstObjectByType<ShowRunePathInfoOnUI>(FindObjectsInactive.Include);
        if (showRunePathInfoOnUI != null) return;

        var canvasObject = GameObject.FindGameObjectWithTag("Canvas");
        if (canvasObject == null) return;

        showRunePathInfoOnUI = canvasObject.GetComponentInChildren<ShowRunePathInfoOnUI>(true);
    }
}
