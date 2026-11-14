using System.Collections;
using System.Collections.Generic;
using TechBooh;
using UnityEngine;

/// <summary>
/// Handle interaction with the station/NPC that opens the skill tree.
/// </summary>
public class ShowSkillTreeInteraction : InteractWithPlayer
{

    public ShowSkillTreeManager showSkillTreeManager;

    protected override void Awake()
    {
        base.Awake();
        if(showSkillTreeManager == null) showSkillTreeManager = Try.FindObjectOfType<ShowSkillTreeManager>();
    }

    protected override void InteractionAction()
    {
        Debug.Log("show skill tree InteractionAction");
        showSkillTreeManager.ShowSkillTree();
    }

    protected override void StartInteraction()
    {
                Debug.Log("show skill tree StartInteraction");
        base.StartInteraction();

        UpdateUiText();
    }

    public void UpdateUiText()
    {
        string statusText = null;

        switch (languageData.currentLanguage)
        {
            case Language.english:
                statusText = interactionButton + " Show Skill Tree";
                break;
            case Language.chinese:
                statusText = interactionButton + " 显示技能树";
                break;
            case Language.portuguese:
                statusText = interactionButton + " Mostrar árvore de habilidades";
                break;
            case Language.spanish:
                statusText = interactionButton + " Mostrar árbol de habilidades";
                break;
            default:
                break;
        }

        generalTextBoxUI.SetText(statusText);
    }
}
