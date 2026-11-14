using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(NPC_Dialogue))]

public class NPC_Behaviour : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI text;
    [SerializeField] private LanguageData languageData;

    [SerializeField] protected Animator animator;

    [SerializeField] protected float timeToShowMessage=1;

    [SerializeField] protected MMF_Player showMessageFeedback, removeMessageFeedback, playerIsCloseFeedback, playerIsNotCloseAnymoreFeedback;
    [SerializeField] protected bool useAnimator = true;
    protected bool playerIsClose = false, messageIsVisible;
    protected float timer;
    protected NPC_Dialogue npc_dialogue;

    protected string previousLine;

    private void Awake()
    {
        npc_dialogue= GetComponent<NPC_Dialogue>();

        if (languageData == null)
        {
            languageData = AssetFinder.GetLanguageData();
        }
    }

    protected virtual void OnEnable()
    {
        if (languageData != null)
        {
            languageData.OnLanguageChange += HandleLanguageChange;
        }
    }

    protected virtual void OnDisable()
    {
        if (languageData != null)
        {
            languageData.OnLanguageChange -= HandleLanguageChange;
        }
    }

    private void Update()
    {
        if (playerIsClose)
        {
            timer += Time.deltaTime; 
            if(timer > timeToShowMessage) 
            {
                timer= 0;
                ShowMessage();
            }
        } 
    }

    public virtual void ShowMessage()
    {
        if (messageIsVisible) return;
        showMessageFeedback.PlayFeedbacks();
        messageIsVisible = true;

        string newLine = npc_dialogue.GetRandomDialogueLine();
        if (npc_dialogue.GetNumberOfLines() > 1)
        {
            int iterations = 0; //Bound retries to avoid an infinite loop.
            while (previousLine == newLine && iterations < 25)
            {
                iterations++;
                newLine = npc_dialogue.GetRandomDialogueLine();
            }
        }
        text.text = newLine;
        previousLine = text.text;
    }


    public virtual void RemoveMessage(float time = 0)
    {

        messageIsVisible = false;
        removeMessageFeedback.PlayFeedbacks();
        timer = time;
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(useAnimator) animator.SetTrigger("playerIsClose");
            playerIsCloseFeedback.PlayFeedbacks();
            playerIsClose = true;
        }
    }

    public virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsClose = false;
            if (useAnimator) animator.SetTrigger("playerIsNotCloseAnymore");
            playerIsNotCloseAnymoreFeedback.PlayFeedbacks();
            RemoveMessage();
        }
    }

    private void HandleLanguageChange()
    {
        if (!playerIsClose)
        {
            return;
        }

        RemoveMessage();
    }


}
