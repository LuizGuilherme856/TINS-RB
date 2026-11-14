using System.Collections.Generic;
using TechBooh;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ForceButtonSelection : MonoBehaviour
{
    [SerializeField] private List<Button> buttons = new List<Button>();
    [SerializeField] private bool isEnabled = true;

    public PlayerHolder playerHolder;
    private PlayerInputHandler playerInput;

    private void Start()
    {
        if (playerHolder)
        {
            playerInput = playerHolder.GetPlayerInputHandler();
        }
        else
        {
            playerInput = Try.GetComponent<PlayerInputHandler>(gameObject);
        }
    }

    private void Update()
    {
        if (!isEnabled) return;
        if (playerInput == null || !playerInput.isUsingController) return;

        CheckAndForceSelection();
    }

    private void CheckAndForceSelection()
    {
        if (!AreAllButtonsDeselected()) return;

        Button fallbackButton = GetFirstValidButton();
        if (fallbackButton == null || EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        fallbackButton.Select();
    }

    private bool AreAllButtonsDeselected()
    {
        if (EventSystem.current == null) return true;

        GameObject selectedGameObject = EventSystem.current.currentSelectedGameObject;
        if (selectedGameObject == null) return true;
        if (!selectedGameObject.activeInHierarchy) return true;

        foreach (Button button in buttons)
        {
            if (button != null && button.IsInteractable() && selectedGameObject == button.gameObject)
            {
                return false;
            }
        }

        return true;
    }

    private Button GetFirstValidButton()
    {
        foreach (Button button in buttons)
        {
            if (button != null && button.IsActive() && button.IsInteractable())
            {
                return button;
            }
        }

        return null;
    }

    public void SetButtons(List<Button> newButtons)
    {
        buttons = newButtons;
    }

    public void EnableCheck()
    {
        isEnabled = true;
    }

    public void DisableCheck()
    {
        isEnabled = false;
    }

    public void Toggle()
    {
        isEnabled = !isEnabled;
    }

    public bool IsEnabled => isEnabled;
}
