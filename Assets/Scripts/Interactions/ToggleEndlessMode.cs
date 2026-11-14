using MoreMountains.Feedbacks;
using UnityEngine;

public class ToggleEndlessMode : InteractWithPlayer
{
    public PlayerData playerData;
    public MMF_Player activateEndlessModeFeedback;
    public MMF_Player deactivateEndlessModeFeedback;

    protected override void Awake()
    {
        base.Awake();
        if (playerData.isEndlessModeOn)
        {
            GetComponent<Animator>().SetTrigger("Activate");
        }
    }

    protected override void StartInteraction()
    {
        base.StartInteraction();

        var lang = languageData != null ? languageData.currentLanguage : Language.english;
        string text;

        if (playerData != null && playerData.isEndlessModeOn)
        {
            // Deactivation message.
            switch (lang)
            {
                case Language.chinese:
                    text = "按 " + interactionButton + " 以关闭无尽模式";
                    break;
                case Language.portuguese:
                    text = "Pressione " + interactionButton + " para desativar o Modo Endless";
                    break;
                case Language.spanish:
                    text = "Presiona " + interactionButton + " para desactivar el Modo Endless";
                    break;
                case Language.english:
                default:
                    text = "Press " + interactionButton + " to deactivate Endless Mode";
                    break;
            }
        }
        else
        {
            // Activation message.
            switch (lang)
            {
                case Language.chinese:
                    text = "按 " + interactionButton + " 以开启无尽模式";
                    break;
                case Language.portuguese:
                    text = "Pressione " + interactionButton + " para ativar o Modo Endless";
                    break;
                case Language.spanish:
                    text = "Presiona " + interactionButton + " para activar el Modo Endless";
                    break;
                case Language.english:
                default:
                    text = "Press " + interactionButton + " to activate Endless Mode";
                    break;
            }
        }

        if (generalTextBoxUI != null)
            generalTextBoxUI.SetText(text);
        else
            Debug.LogWarning("GeneralTextBoxUI não encontrado ao tentar mostrar texto de interação.");
    }

    protected override void InteractionAction()
    {
        base.InteractionAction();
        if (playerData.isEndlessModeOn)
        {
            playerData.isEndlessModeOn = false;
            deactivateEndlessModeFeedback.PlayFeedbacks();
        }
        else
        {
            playerData.isEndlessModeOn = true;
            activateEndlessModeFeedback.PlayFeedbacks();
        }

        generalTextBoxUI.SetText("");
    }
}
