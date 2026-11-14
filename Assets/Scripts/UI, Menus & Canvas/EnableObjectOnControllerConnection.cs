using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Despite the legacy class/method names, this reacts to the active prompt device, not physical loss.
// Use actual device/user loss events for a controller-disconnected modal.
public class EnableObjectOnControllerConnection : MonoBehaviour
{
    [Tooltip("Lista de objetos que serão ativados quando o controle conectar.")]
    [SerializeField] private List<GameObject> objectsToEnableOnConnect;

    [Tooltip("Lista de objetos que serão desativados quando o controle conectar.")]
    [SerializeField] private List<GameObject> objectsToDisableOnConnect;

    [Tooltip("Lista de objetos que serão ativados quando o controle desconectar.")]
    [SerializeField] private List<GameObject> objectsToEnableOnDisconnect;

    [Tooltip("Lista de objetos que serão desativados quando o controle desconectar.")]
    [SerializeField] private List<GameObject> objectsToDisableOnDisconnect;

    [SerializeField] private PlayerHolder playerHolder;

    private PlayerInputHandler playerInputHandler;

    private void Awake()
    {
        // Get the PlayerInputHandler reference.
        playerInputHandler = playerHolder.GetPlayerInputHandler();
        if (playerInputHandler == null)
        {
            Debug.LogError("PlayerInputHandler não encontrado");
        }
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            // Subscribe to active input-device changes, not physical connection events.
            playerInputHandler.OnControllerChange += CheckControllerChange;
        }

        if(playerInputHandler.isUsingController)
        {
            // Apply controller-mode objects if controller input is active.
            HandleControllerConnected();
        }
        else
        {
            // Apply keyboard/mouse-mode objects otherwise.
            HandleControllerDisconnected();
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            // Unsubscribe from active input-device changes.
            playerInputHandler.OnControllerChange -= CheckControllerChange;
        }
    }

    private void CheckControllerChange()
    {
        // Check the active input mode; this is not a physical connection test.
        if (playerInputHandler.isUsingController)
        {
            HandleControllerConnected();
        }
        else
        {
            HandleControllerDisconnected();
        }
    }

    /// <summary>
    /// Apply controller-mode visibility.
    /// </summary>
    private void HandleControllerConnected()
    {
        // Activate objects configured for controller mode.
        SetActiveState(objectsToEnableOnConnect, true);
        SetActiveState(objectsToDisableOnConnect, false);
    }

    /// <summary>
    /// Apply non-controller-mode visibility.
    /// </summary>
    private void HandleControllerDisconnected()
    {
        // Activate objects configured for non-controller mode.
        SetActiveState(objectsToEnableOnDisconnect, true);
        SetActiveState(objectsToDisableOnDisconnect, false);
    }

    /// <summary>
    /// Set the active state of every object in the list.
    /// </summary>
    /// <param name="objects">Objects to update.</param>
    /// <param name="state">True activates objects; false deactivates them.</param>
    private void SetActiveState(List<GameObject> objects, bool state)
    {
        foreach (var obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(state);
            }
        }
    }
}
