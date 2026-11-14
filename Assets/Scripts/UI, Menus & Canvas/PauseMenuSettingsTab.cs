using UnityEngine;

/// <summary>
/// Coordinates pause-menu settings and confirmations.
/// </summary>
public class PauseMenuSettingsTab : MonoBehaviour
{
    [SerializeField] SelectEventSystemButton selectEventSystemButton;

    [SerializeField] GameObject optionsMenu;
    [Tooltip("O botao da primera tela do Options")]
    [SerializeField] GameObject optionsButton;
    [Tooltip("O botao da primera tela do settings")]
    [SerializeField] GameObject rootButton;

    [Header("Restart Confirmation Screen")]
    [SerializeField] GameObject restartConfirmationScreen;
    [SerializeField] GameObject restartConfirmationButton;

    [Header("Main Menu Confirmation Screen")]
    [SerializeField] GameObject mainMenuConfirmationScreen;
    [SerializeField] GameObject mainMenuConfirmationButton;

    [Header("Quit Confirmation Screen")]
    [SerializeField] GameObject quitConfirmationScreen;
    [SerializeField] GameObject quitConfirmationButton;

    /// <summary>
    /// Show or hide restart confirmation and select the appropriate button.
    /// </summary>
    /// <param name="show">True shows confirmation; false hides it and selects the options button.</param>
    public void ShowRestartConfirmationScreen_Button(bool show)
    {
        if (show)
        {
            restartConfirmationScreen.SetActive(true);
            selectEventSystemButton.SetButtonToSelect(restartConfirmationButton);
        }
        else
        {
            restartConfirmationScreen.SetActive(false);
            selectEventSystemButton.SetButtonToSelect(rootButton);
        }
    }

    /// <summary>
    /// Show or hide main-menu confirmation and select the appropriate button.
    /// </summary>
    /// <param name="show">True shows confirmation; false hides it and selects the options button.</param>
    public void ShowMainMenuConfirmationScreen_Button(bool show)
    {
        if (show)
        {
            mainMenuConfirmationScreen.SetActive(true);
            selectEventSystemButton.SetButtonToSelect(mainMenuConfirmationButton);
        }
        else
        {
            mainMenuConfirmationScreen.SetActive(false);
            selectEventSystemButton.SetButtonToSelect(rootButton);
        }
    }

    /// <summary>
    /// Show or hide quit confirmation and select the appropriate button.
    /// </summary>
    /// <param name="show">True shows confirmation; false hides it and selects the options button.</param>
    public void ShowQuitConfirmationScreen_Button(bool show)
    {
        if (show)
        {
            quitConfirmationScreen.SetActive(true);
            selectEventSystemButton.SetButtonToSelect(quitConfirmationButton);
        }
        else
        {
            quitConfirmationScreen.SetActive(false);
            selectEventSystemButton.SetButtonToSelect(rootButton);
        }
    }

    /// <summary>
    /// Show or hide the options menu.
    /// </summary>
    /// <param name="show">Whether to show the options menu.</param>
    public void ShowOptionsMenu_Button(bool show)
    {
        if(show)
        {
            optionsMenu.SetActive(true);
            selectEventSystemButton.SetButtonToSelect(optionsButton);

        }
        else
        {
            optionsMenu.SetActive(false);
            selectEventSystemButton.SetButtonToSelect(rootButton);

        }
    }
}
