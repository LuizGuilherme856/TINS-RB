using TechBooh;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Ensure menus always have an EventSystem selection.
/// </summary>
public class SelectEventSystemButton : MonoBehaviour
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private Button pauseMenuButton, inventoryMenuButton, settingsMenuButton;
    [SerializeField] private GameObject pauseMenu, inventory, inventoryMenuContent;

    [Header("----- main Menu ----")]
    [SerializeField] private bool isMainMenu = false;
    [SerializeField] private Button mainMenuButton;

    [Header("Settings Menu")]
    [SerializeField] private Button videoTab1stButton;
    [SerializeField] private GameObject gameTab1stButton;
    [SerializeField] private GameObject audioTab1stButton;
    [SerializeField] private GameObject settingsMenuTab1stButton;

    public PlayerHolder playerHolder;
    private PlayerInputHandler playerInput;
    private GameObject lastSelectedGameObject;

    private void Awake()
    {
        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
        }

        if (playerHolder)
        {
            playerInput = playerHolder.GetPlayerInputHandler();
        }
        else
        {
            playerInput = Try.GetComponent<PlayerInputHandler>(gameObject);
        }

        if (playerInput == null)
        {
            Debug.LogError("Nao tem playerInput ou playerHolder no: " + gameObject.name);
        }
    }

    private void OnEnable()
    {
        if (playerInput == null) return;

        if (isMainMenu)
        {
           // playerInput.OnControllerChange += SelectMainMenuButton;
            SelectMainMenuButton();
            return;
        }

        playerInput.OnInventory += SelectInventoryButton;
        playerInput.OnEscape += OBSOLETO_SelectPauseMenuButton;
    }

    private void OnDisable()
    {
        if (playerInput == null) return;

        if (isMainMenu)
        {
            //playerInput.OnControllerChange -= SelectMainMenuButton;
            return;
        }

        playerInput.OnInventory -= SelectInventoryButton;
        playerInput.OnEscape -= OBSOLETO_SelectPauseMenuButton;
    }

    private void Update()
    {
        if (isMainMenu) return;
        if (eventSystem == null) return;
        if (!ShouldKeepSelectionActive()) return;
        if (!IsUsingControllerNavigation()) return;

        GameObject selected = eventSystem.currentSelectedGameObject;
        if (selected != null && selected.activeInHierarchy)
        {
            return;
        }

        TryRestoreSelection();
    }

    private void SelectMainMenuButton()
    {
        if (mainMenuButton == null) return;
        SetButtonToSelect(mainMenuButton.gameObject);
    }

    private void SelectInventoryButton()
    {
        if (pauseMenu != null && pauseMenu.activeSelf) return;
        if (inventoryMenuButton == null) return;

        SetButtonToSelect(inventoryMenuButton.gameObject);
    }

    public void OBSOLETO_SelectPauseMenuButton()
    {
        if (inventory != null && inventory.activeSelf) return;
        if (pauseMenuButton == null) return;

        SetButtonToSelect(pauseMenuButton.gameObject);
    }

    public void OBSOLETO_SelectSettingsMenuButton()
    {
        if (inventory != null && inventory.activeSelf) return;
        if (settingsMenuButton == null) return;

        SetButtonToSelect(settingsMenuButton.gameObject);
    }

    public void SetButtonToSelect(GameObject buttonGameObject)
    {
        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
            if (eventSystem == null) return;
        }

        GameObject target = ResolveSelectableTarget(buttonGameObject);
        lastSelectedGameObject = target;

        eventSystem.SetSelectedGameObject(null);

        if (target != null && IsUsingControllerNavigation())
        {
            eventSystem.SetSelectedGameObject(target);
        }
    }

    public void SelectVideoTabButton_Button() => SetButtonToSelect(videoTab1stButton != null ? videoTab1stButton.gameObject : null);
    public void SelectGameTabButton_Button() => SetButtonToSelect(gameTab1stButton);
    public void SelectAudioTabButton_Button() => SetButtonToSelect(audioTab1stButton);
    public void SelectSettingsMenuTabButton_Button() => SetButtonToSelect(settingsMenuTab1stButton);

    private void TryRestoreSelection()
    {
        GameObject target = ResolveSelectableTarget(lastSelectedGameObject);

        if (target == null)
        {
            target = GetDefaultSelection();
        }

        if (target == null) return;

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(target);
        lastSelectedGameObject = target;
    }

    private GameObject GetDefaultSelection()
    {
        if (isMainMenu)
        {
            return ResolveSelectableTarget(mainMenuButton != null ? mainMenuButton.gameObject : null);
        }

        if (pauseMenu != null && pauseMenu.activeInHierarchy)
        {
            GameObject settingsTarget = ResolveSelectableTarget(settingsMenuButton != null && settingsMenuButton.gameObject.activeInHierarchy ? settingsMenuButton.gameObject : null);
            if (settingsTarget != null) return settingsTarget;

            GameObject pauseTarget = ResolveSelectableTarget(pauseMenuButton != null ? pauseMenuButton.gameObject : null);
            if (pauseTarget != null) return pauseTarget;
        }

        if (inventory != null && inventory.activeInHierarchy)
        {
            GameObject inventoryTarget = ResolveSelectableTarget(inventoryMenuButton != null ? inventoryMenuButton.gameObject : null);
            if (inventoryTarget != null) return inventoryTarget;

            return ResolveSelectableTarget(inventoryMenuContent);
        }

        return null;
    }

    private bool ShouldKeepSelectionActive()
    {
        if (isMainMenu)
        {
            return gameObject.activeInHierarchy;
        }

        bool pauseMenuVisible = pauseMenu != null && pauseMenu.activeInHierarchy;
        bool inventoryVisible = inventory != null && inventory.activeInHierarchy;
        return pauseMenuVisible || inventoryVisible;
    }

    private bool IsUsingControllerNavigation()
    {
        return playerInput != null && playerInput.isUsingController;
    }

    private GameObject ResolveSelectableTarget(GameObject target)
    {
        if (target == null) return null;
        if (!target.activeInHierarchy) return null;

        Selectable selectable = target.GetComponent<Selectable>();
        if (IsSelectableValid(selectable))
        {
            return selectable.gameObject;
        }

        Selectable[] selectables = target.GetComponentsInChildren<Selectable>(true);
        foreach (Selectable childSelectable in selectables)
        {
            if (IsSelectableValid(childSelectable))
            {
                return childSelectable.gameObject;
            }
        }

        return null;
    }

    private bool IsSelectableValid(Selectable selectable)
    {
        return selectable != null && selectable.IsInteractable() && selectable.gameObject.activeInHierarchy;
    }
}
