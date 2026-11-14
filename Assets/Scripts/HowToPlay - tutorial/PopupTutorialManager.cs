using System.Text.RegularExpressions;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ForceButtonSelection))]
public class PopupTutorialManager : MonoBehaviour
{



    [Header("References")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Image imageExample;
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject popupGameObj;

    [Header("Feedbacks")]
    public MMF_Player popFeedback;
    public MMF_Player closeFeedback;


    [Header("Other")]
    public LanguageData languageData;
    [SerializeField] private ButtonsData buttonsData;
    public PlayerHolder playerHolder;
    [SerializeField] private PauseMenuManager pauseMenuManager;
    [SerializeField] private EventSystem eventSystem;

    //private
    private PlayerInputHandler playerInputHandler;
    private PopupTutorialData currentTutorialData;


    #region buttons Strings
    /// Replace these placeholders with the appropriate button prompts.
    private const string INTERACTION_BUTTON = "{INTERACTION_BUTTON}";
    private const string DASH_BUTTON = "{DASH_BUTTON}";
    private const string FIRE_BUTTON = "{FIRE_BUTTON}";
    private const string SPECIAL_BUTTON = "{SPECIAL_BUTTON}";
    private const string MELEE_BUTTON = "{MELEE_BUTTON}";
    #endregion

    void Awake()
    {
        playerInputHandler = playerHolder.GetPlayerInputHandler();
        if (playerInputHandler != null)
        {
            playerInputHandler.OnControllerChange += RefreshCurrentPopup_TechBooh;
        }

        popupGameObj.SetActive(false);
    }

    private void OnDestroy()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnControllerChange -= RefreshCurrentPopup_TechBooh;
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.SetPauseInputBlocked(false);
        }
    }

    public void ShowPopup(PopupTutorialData tutorialData)
    {
        currentTutorialData = tutorialData;
        popFeedback.PlayFeedbacks();

        popupGameObj.SetActive(true);
        title.text = tutorialData.GetName(languageData.currentLanguage);
        imageExample.sprite = tutorialData.exampleImage;
        //Update description prompts for the current input device.
        InputPromptDeviceType deviceType = playerInputHandler != null
            ? playerInputHandler.CurrentPromptDevice
            : InputPromptDeviceType.KeyboardMouse;
        string newDescription = ReplaceButtonPlaceholders(tutorialData.GetDescription(languageData.currentLanguage), deviceType);
        description.text = newDescription;

        eventSystem.SetSelectedGameObject(closeButton);

        playerInputHandler.SetCanMove(false);
        playerInputHandler.SetPauseInputBlocked(true);
        pauseMenuManager.ChangeTimeScale(0);

        GetComponent<ForceButtonSelection>().EnableCheck();
    }

    // The close feedback controls visual dismissal; this method immediately restores gameplay input/time.
    // When layering another modal, coordinate ownership so closing this popup cannot dismiss its pause.
    public void ClosePopup()
    {
        currentTutorialData = null;
        closeFeedback.PlayFeedbacks();
        playerInputHandler.SetPauseInputBlocked(false);
        playerInputHandler.SetCanMove(true);
        pauseMenuManager.ChangeTimeScale(1);
        //popupGameObj.SetActive(false);
    }


    /// <summary>
    /// Replace {KEY} placeholders using GetButtonText.
    /// </summary>
    public string ReplaceButtonPlaceholders(string input, bool isUsingController, bool useTMP = true)
    {
        InputPromptDeviceType deviceType = isUsingController
            ? InputPromptDeviceDetector_TechBooh.DetectCurrentGamepad()
            : InputPromptDeviceType.KeyboardMouse;
        return ReplaceButtonPlaceholders(input, deviceType, useTMP);
    }

    public string ReplaceButtonPlaceholders(string input, InputPromptDeviceType deviceType, bool useTMP = true)
    {
        return Regex.Replace(input, @"\{([A-Z_]+)\}", match =>
        {
            string key = match.Value; // Inclui as chaves, ex: {DASH_BUTTON}
            string replacement = GetButtonText(key, deviceType, useTMP);
            return string.IsNullOrEmpty(replacement) ? match.Value : replacement;
        });
    }

    private string GetButtonText(string buttonName, InputPromptDeviceType deviceType, bool useTMP = true)
    {
        switch (buttonName)
        {
            case INTERACTION_BUTTON:
                return buttonsData.GetInteractionButton(deviceType, useTMP);
            case DASH_BUTTON:
                return buttonsData.GetDashButton(deviceType, useTMP);
            case FIRE_BUTTON:
                return buttonsData.GetFireButton(deviceType, useTMP);
            case SPECIAL_BUTTON:
                return buttonsData.GetSpecialButton(deviceType, useTMP);
            case MELEE_BUTTON:
                return buttonsData.GetMeleeButton(deviceType, useTMP);
            default:
                return string.Empty;
        }
    }

    private void RefreshCurrentPopup_TechBooh()
    {
        if (currentTutorialData == null || popupGameObj == null || !popupGameObj.activeInHierarchy)
        {
            return;
        }

        InputPromptDeviceType deviceType = playerInputHandler != null
            ? playerInputHandler.CurrentPromptDevice
            : InputPromptDeviceType.KeyboardMouse;
        description.text = ReplaceButtonPlaceholders(
            currentTutorialData.GetDescription(languageData.currentLanguage),
            deviceType);
    }
}
