using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class ShowPauseMenuTabs : MonoBehaviour
{
    [SerializeField] private SelectEventSystemButton selectEventSystemButton;
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private int firstTabToShow = 0; // Initially displayed tab index.
    [Tooltip("S�o tabs que, quando estao abertas, n�o permite que o haja a troca de tabs no L1-R1")]
    [SerializeField] private List<GameObject> blockerTabs;

    [SerializeField] private MMF_Player changeTabFeedback; // Tab-switch feedback.
    private PlayerInputHandler playerInputHandler;

    [SerializeField] private List<Tab> tabs; // Tab configurations stored as structures.
    [SerializeField] private List<GameObject> settingsSubTabs; // Add this entry to the tab list.
    public GameObject mainSettingsScreen; //This tab must be enabled.

    private int currentTabIndex = 0; // Active tab index.

    private void Awake()
    {
        playerInputHandler = playerHolder.GetPlayerInputHandler();
    }

    private void OnEnable()
    {
        // Subscribe to PlayerInputHandler shoulder-button events.
        playerInputHandler.OnLeftShoulderDown += PreviousTab;
        playerInputHandler.OnRightShoulderDown += NextTab;

        ShowTab(firstTabToShow);
        currentTabIndex = firstTabToShow; // Initialize the index from the starting tab.
    }

    private void OnDisable()
    {
        // Unsubscribe from PlayerInputHandler shoulder-button events.
        playerInputHandler.OnLeftShoulderDown -= PreviousTab;
        playerInputHandler.OnRightShoulderDown -= NextTab;
    }

    private void NextTab()
    {
        // Check whether a blocking tab is active.
        if (blockerTabs.Any(tab => tab.activeInHierarchy)) return;

        // Advance to the next tab.
        currentTabIndex = (currentTabIndex + 1) % tabs.Count;
        ShowTab(currentTabIndex);
    }

    private void PreviousTab()
    {
        // Check whether a blocking tab is active.
        if (blockerTabs.Any(tab => tab.activeInHierarchy)) return;

        // Return to the previous tab.
        currentTabIndex = (currentTabIndex - 1 + tabs.Count) % tabs.Count;


        ShowTab(currentTabIndex);
    }

    private void ShowTab(int tabIndex)
    {
        // Disable all tabs.
        for (int i = 0; i < tabs.Count; i++)
        {
            tabs[i].tabObject.SetActive(i == tabIndex); // Activate only the selected tab.
            tabs[i].tabIcon.SetDefaultColor(); // Reset all icon colors.
        }
        DeactivateAllSubTabs();

        // Update the active tab's icon color.
        tabs[tabIndex].tabIcon.SetNewColor();

        // Play tab-switch feedback.
        changeTabFeedback?.PlayFeedbacks();

        // Select the corresponding button.
        GameObject buttonToSelect = GetTabButton(tabs[tabIndex]);

        if (buttonToSelect != null)
        {
            selectEventSystemButton.SetButtonToSelect(buttonToSelect);

            // Invoke the button automatically if required.
            if (tabs[tabIndex].pressButton)
            {
                Button buttonComponent = buttonToSelect.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    buttonComponent.onClick.Invoke();
                }
            }
        }
    }

/// <summary>
/// Subtabs are panels inside the Options Canvas.
/// </summary>
    private void DeactivateAllSubTabs()
    {
        mainSettingsScreen.SetActive(true);
        foreach (var tab in settingsSubTabs)
        {
            if (tab != null)
                tab.SetActive(false);
        }
    }

    /// <summary>
    /// Find the tab button in the configured location.
    /// </summary>
    /// <param name="tab">Tab whose button is required.</param>
    /// <returns>The button GameObject.</returns>
    private GameObject GetTabButton(Tab tab)
    {
        if (tab.buttonLocation == ButtonLocation.Self)
        {
            return tab.tabFirstButton; // Return the assigned button directly.
        }
        else if (tab.buttonLocation == ButtonLocation.Child)
        {
            // Find buttons in the tab's children.
            Button[] childButtons = tab.tabFirstButton.GetComponentsInChildren<Button>();

            if (childButtons.Length > 0)
            {
                // Choose the first button active in the hierarchy.
                foreach (var btn in childButtons)
                {
                    if (btn.gameObject.activeInHierarchy)
                    {
                        return btn.gameObject;
                    }
                }

                // Fall back to the first button if none are active.
                Debug.LogWarning("Nenhum botão ativo encontrado em " + tab.tabObject.name + ". Usando o primeiro botão da lista.");
                return childButtons[0].gameObject;
            }
        }

        Debug.LogWarning("Botao nao encontrado para a Tab: " + tab.tabObject.name);
        return null;
    }

    /// <summary>
    /// Activate a tab by index.
    /// </summary>
    /// <param name="tabIndex">Index of the tab to activate.</param>
    public void ActivateTabByIndex(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabs.Count)
        {
            Debug.LogWarning("�ndice de Tab inv�lido: " + tabIndex);
            return;
        }

        currentTabIndex = tabIndex; // Update the current index.
        ShowTab(tabIndex); // Display the selected tab.
    }
}

/// <summary>
/// Whether the EventSystem button is this GameObject or a child.
/// </summary>
public enum ButtonLocation
{
    Self,   // The GameObject itself is the button.
    Child   // The button is a child of the GameObject.
}

/// <summary>
/// Tab configuration structure.
/// </summary>
[System.Serializable]
public struct Tab
{
    [Tooltip("Objeto da Tab")]
    public GameObject tabObject;

    [Tooltip("�cone da Tab")]
    public ChangeImageColor tabIcon;

    [Tooltip("Localiza��o do bot�o (se � o pr�prio GameObject ou um dos filhos)")]
    public ButtonLocation buttonLocation;

    [Tooltip("primeiro botao da Tab ou o parent caso v� procurar nos filhos")]
    public GameObject tabFirstButton;

    [Tooltip("Indica se o bot�o deve ser pressionado automaticamente")]
    public bool pressButton;
}


