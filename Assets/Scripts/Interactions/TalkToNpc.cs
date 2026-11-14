using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Advance to a different NPC line on each player interaction.
/// </summary>
public class TalkToNpc : InteractWithPlayer
{
    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private MMF_Player textRevealFeedback;
    [SerializeField] private Voice_NPC voice_npc;

    public event Action OnFinishDialogue, OnChangeLine;

    private string[] lines;
    private int lineIndex =0;


    protected override void StartInteraction()
    {
        base.StartInteraction();

        switch (languageData.currentLanguage)
        {
            case Language.english:
                generalTextBoxUI.SetText(interactionButton + " Talk ");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText(interactionButton + " 交谈 ");
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText(interactionButton + " Falar ");
                break;
            case Language.spanish:
                generalTextBoxUI.SetText(interactionButton + " Hablar ");
                break;
            default:
                break;
        }
    }

    protected override void InteractionAction()
    {
        base.InteractionAction();
        SetTextToCanvas();
    }

    private void SetTextToCanvas()
    {
        string line = GetLine();
        textBox.text = line;
        voice_npc.PlayVoice(line);
        textRevealFeedback.StopFeedbacks();
        textRevealFeedback.PlayFeedbacks();
    }

    private string GetLine()
    {
        OnChangeLine?.Invoke();
        string line;
        if (lineIndex >= lines.Length)
        {
            lineIndex = 0;
            OnFinishDialogue?.Invoke();
        }
        line = lines[lineIndex];
        lineIndex++;
        return line;
    }

    public void SetLines(string[] lines)
    {
        this.lines = lines;
        lineIndex = 0; 
    }

    public void NextLine()
    {
        SetTextToCanvas();
    }
}
