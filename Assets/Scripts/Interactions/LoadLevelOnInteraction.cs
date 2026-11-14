using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevelOnInteraction : InteractWithPlayer
{
    [Header("   LOAD CONFIG   ")]
    [SerializeField] private int levelToLoad;
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private PlayFeedbacks transitionFX;
    [SerializeField] private SceneLoader sceneLoader;

    [Header("   OTHER   ")]
    [SerializeField] private bool resetRunData = false;
    [SerializeField] private HandleStartNewRun handleStartNewRun;


    protected override void StartInteraction()
    {
        base.StartInteraction();
        switch (languageData.currentLanguage)
        {
            case Language.english:
                generalTextBoxUI.SetText(interactionButton + " Change Character ");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText(interactionButton + " 更换角色 ");
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText(interactionButton + " Trocar personagem ");
                break;
            case Language.spanish:
                generalTextBoxUI.SetText(interactionButton + " Cambiar personaje");
                break;
            default:
                break;
        }

    }


    protected override void InteractionAction()
    {
        if (resetRunData) handleStartNewRun.StartNewRun();
        sceneLoader.LoadSpecificScene(levelToLoad, transitionTime);
        transitionFX.PlayAllFeedbacks();
    }
}
