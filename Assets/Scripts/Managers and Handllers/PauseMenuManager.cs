using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private ShowSkillTreeManager showSkillTreeManager;
    private PlayerInputHandler playerInput;
    [SerializeField] private GameObject pauseMenu, inventoryMenu, statisticsScreen, settingsMenu, pauseMenuItems;

    [SerializeField] private MMF_Player mmf_player, showPauseMenuFeedback, showInventoryMenuFeedback;

    [Space(20)]
    [Header("   Confirmation Screens")]
    [SerializeField] private GameObject restartConfScreen;
    [SerializeField] private GameObject restartConfButton, mainMenuConfScreen, mainMenuConfButton;

    public GameObject pathsGameObj;
    private MMF_TimescaleModifier MMF_TimescaleModifier;
    private SelectEventSystemButton selectEventButton;

    public event Action OnPauseMenuIsVisible, OnInventoryIsVisible;
    public event Action OnPauseMenuIsNotVisible, OnInventoryIsNotVisible;

    private void Awake()
    {
        selectEventButton = Object.FindFirstObjectByType<SelectEventSystemButton>();
        MMF_TimescaleModifier = mmf_player.GetFeedbackOfType<MMF_TimescaleModifier>();
        if (showSkillTreeManager == null) showSkillTreeManager = Object.FindFirstObjectByType<ShowSkillTreeManager>();

        playerInput = playerHolder.GetPlayerInputHandler();
        
    }

    private void OnEnable()
    {
        playerInput.OnEscape += ShowinventoryMenu;
        playerInput.OnCheat += ToggleCheatMenu;
        playerInput.OnCheat += TryHideInventoryMenu;
        playerInput.OnInventory += ShowinventoryMenu;
    }

    /// <summary>
    /// Toggle the cheat menu, formerly the pause menu.
    /// </summary>
    private void ToggleCheatMenu()
    {
        if (inventoryMenu.activeSelf) return;

        if (pauseMenu.activeSelf)
        {
            ClosePauseMenu();
        }
        else
        {
            OpenPauseMenu();
        }

    }

    private void OpenPauseMenu()
    {
        OnPauseMenuIsVisible?.Invoke();

        playerInput.SetCanMove(false);
        ResumePauseGame(false);
        pauseMenu.SetActive(true);
        pauseMenuItems.SetActive(true);
        showPauseMenuFeedback.PlayFeedbacks();

        HideScreens();
    }

    private void HideScreens()
    {
        settingsMenu.SetActive(false);
        restartConfScreen.SetActive(false);
        mainMenuConfScreen.SetActive(false);
    }

    public void ClosePauseMenu()
    {
        if (pauseMenu.activeSelf)
        {
            OnPauseMenuIsNotVisible?.Invoke();

            playerInput.SetCanMove(true);
            ResumePauseGame(true);
            pauseMenu.SetActive(false);
        }

        HideScreens();
    }

    // This is a local menu pause toggle, not a stack of pause owners. Resuming sets timeScale to 1;
    // a disconnect overlay or another modal must preserve its own blocking state instead of blindly resuming.
    private void ResumePauseGame(bool resume)
    {
        if (resume)
        {
            ChangeTimeScale(1);
            statisticsScreen.SetActive(true);
        }
        else
        {
            ChangeTimeScale(0);
            statisticsScreen.SetActive(false);
        }
    }

    private void ShowinventoryMenu()
    {
        if (showSkillTreeManager != null && showSkillTreeManager.IsSkillTreeOpen()) return;
        if (pauseMenu.activeSelf || pathsGameObj.activeSelf) return;

        if (inventoryMenu.activeSelf)
        {
            HideInventoryMenu();
        }
        else
        {
            OnInventoryIsVisible?.Invoke();
            playerInput.SetCanMove(false);
            ResumePauseGame(false);
            inventoryMenu.SetActive(true);
            showInventoryMenuFeedback.PlayFeedbacks();
        }
    }

    private void HideInventoryMenu()
    { 
        OnInventoryIsNotVisible?.Invoke();
        playerInput.SetCanMove(true);
        ResumePauseGame(true);
        inventoryMenu.SetActive(false);
    }

    private void TryHideInventoryMenu()
    {
        //if (pauseMenu.activeSelf == false) return; //Escape closed the pause menu rather than the inventory.
        if (inventoryMenu.activeSelf == false) return;

        OnInventoryIsNotVisible?.Invoke();
        playerInput.SetCanMove(true);
        ResumePauseGame(true);
        inventoryMenu.SetActive(false);
    }

    public void ChangeTimeScale(float timeScale)
    {
        MMF_TimescaleModifier.TimeScale = timeScale;
        mmf_player.PlayFeedbacks();
    }

    //handle menu buttons

    public void ResumeGame()
    {
        ToggleCheatMenu();
    }

    public void ShowSettingsMenu()
    {
        if (settingsMenu.activeSelf) //Toggle the active state.
        {
            settingsMenu.SetActive(false);
            pauseMenuItems.SetActive(true);
            selectEventButton.OBSOLETO_SelectPauseMenuButton();
        }
        else
        {
            settingsMenu.SetActive(true);
            pauseMenuItems.SetActive(false);
            selectEventButton.OBSOLETO_SelectSettingsMenuButton();
        }
    }

 
    public void ShowRestartConfirmationScreen()
    {
        if (restartConfScreen.activeSelf) //Toggle the active state.
        {
            pauseMenuItems.SetActive(true);
            restartConfScreen.SetActive(false);
            selectEventButton.OBSOLETO_SelectPauseMenuButton();
        }
        else
        {
            pauseMenuItems.SetActive(false);
            restartConfScreen.SetActive(true);
            selectEventButton.SetButtonToSelect(restartConfButton);
        }
    }

    public void ShowMainMenuConfirmationScreen()
    {
        if (mainMenuConfScreen.activeSelf) //Toggle the active state.
        {
            pauseMenuItems.SetActive(true);
            mainMenuConfScreen.SetActive(false);
            selectEventButton.OBSOLETO_SelectPauseMenuButton();
        }
        else
        {
            pauseMenuItems.SetActive(false);
            mainMenuConfScreen.SetActive(true);
            selectEventButton.SetButtonToSelect(mainMenuConfButton);
        }
    }

    public bool IsPauseMenuActive()
    {
        return pauseMenu.activeSelf;
    }

    public bool IsInventoryActive()
    {
        return inventoryMenu.activeSelf;
    }

}
