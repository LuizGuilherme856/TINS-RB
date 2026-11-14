using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handle gamepad cursor navigation in the skill tree.
/// Skill purchases do not use EventSystem.
/// </summary>
public class GamePadCursor : MonoBehaviour
{
    [Tooltip("Referencia ao botao sobre o qual o cursor esta atualmente.")]
    [SerializeField] private Button currentButton;
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private SelectEventSystemButton selectEventSystemButton;

    private PlayerInputHandler playerInputHandler;
    private Button currentBuyButton;
    private readonly List<Button> overlappedButtons = new();

    private void Awake()
    {
        playerInputHandler = playerHolder.GetPlayerInputHandler();
    }

    private void OnEnable()
    {
        if (selectEventSystemButton != null)
        {
            selectEventSystemButton.SetButtonToSelect(null);
        }

        if (playerInputHandler != null)
        {
            playerInputHandler.OnInteract += TryToClickBuyButton;
            playerInputHandler.OnReload += TryToClickBuyButton;
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnInteract -= TryToClickBuyButton;
            playerInputHandler.OnReload -= TryToClickBuyButton;
        }

        ClearCurrentSelection();
        overlappedButtons.Clear();
    }

    private void FixedUpdate()
    {
        if (playerInputHandler == null)
        {
            Debug.LogWarning("PlayerInputHandler nao esta configurado.");
            return;
        }

        if (selectEventSystemButton != null)
        {
            // Prevent analog input from selecting another button.
            selectEventSystemButton.SetButtonToSelect(null);
        }

        RefreshCurrentSelection();
    }

    private void TryToClickBuyButton()
    {
        if (currentBuyButton != null)
        {
            currentBuyButton.onClick.Invoke();
            currentBuyButton = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Button button = collision.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        if (!overlappedButtons.Contains(button))
        {
            overlappedButtons.Add(button);
        }

        RefreshCurrentSelection();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Button button = collision.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        overlappedButtons.Remove(button);
        RefreshCurrentSelection();
    }

    private void RefreshCurrentSelection()
    {
        CleanupInvalidOverlaps();

        Button bestButton = GetClosestOverlappedButton();
        if (bestButton == currentButton)
        {
            if (currentButton != null)
            {
                UpdateCurrentBuyButton(currentButton);
            }

            return;
        }

        if (currentButton != null)
        {
            HandleGamePadCursor previousCursorHandler = currentButton.GetComponentInParent<HandleGamePadCursor>();
            if (previousCursorHandler != null)
            {
                previousCursorHandler.OnGamePadCursorExit();
            }
        }

        currentButton = bestButton;

        if (currentButton == null)
        {
            currentBuyButton = null;
            return;
        }

        HandleGamePadCursor currentCursorHandler = currentButton.GetComponentInParent<HandleGamePadCursor>();
        if (currentCursorHandler != null)
        {
            currentCursorHandler.OnGamePadCursorHover();
            currentBuyButton = currentCursorHandler.GetRelevantButton();
        }
        else
        {
            currentBuyButton = null;
        }
    }

    private void CleanupInvalidOverlaps()
    {
        for (int i = overlappedButtons.Count - 1; i >= 0; i--)
        {
            Button button = overlappedButtons[i];
            if (button == null || !button.gameObject.activeInHierarchy)
            {
                overlappedButtons.RemoveAt(i);
            }
        }
    }

    private Button GetClosestOverlappedButton()
    {
        Button bestButton = null;
        float bestDistance = float.MaxValue;
        Vector3 cursorPosition = transform.position;

        foreach (Button button in overlappedButtons)
        {
            if (button == null)
            {
                continue;
            }

            float currentDistance = Vector2.SqrMagnitude(button.transform.position - cursorPosition);
            if (currentDistance < bestDistance)
            {
                bestDistance = currentDistance;
                bestButton = button;
            }
        }

        return bestButton;
    }

    private void UpdateCurrentBuyButton(Button button)
    {
        HandleGamePadCursor cursorHandler = button.GetComponentInParent<HandleGamePadCursor>();
        currentBuyButton = cursorHandler != null ? cursorHandler.GetRelevantButton() : null;
    }

    private void ClearCurrentSelection()
    {
        if (currentButton == null)
        {
            currentBuyButton = null;
            return;
        }

        HandleGamePadCursor cursorHandler = currentButton.GetComponentInParent<HandleGamePadCursor>();
        if (cursorHandler != null)
        {
            cursorHandler.OnGamePadCursorExit();
        }

        currentButton = null;
        currentBuyButton = null;
    }

    /// <summary>
    /// Return the current button reference.
    /// </summary>
    public Button GetCurrentButton()
    {
        return currentButton;
    }
}
