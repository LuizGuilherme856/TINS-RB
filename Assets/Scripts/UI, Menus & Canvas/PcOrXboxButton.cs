using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Activate the controller or keyboard/mouse prompt in the pause menu.
public class PcOrXboxButton : MonoBehaviour
{
    [Serializable]
    private class DevicePromptObject
    {
        public InputPromptDeviceType deviceType;
        public GameObject promptObject;
    }

    [SerializeField] private GameObject xboxButton, pcButton;
    [Tooltip("Opcional. Associe aqui objetos especificos de PlayStation, Switch ou outros controles.")]
    [SerializeField] private List<DevicePromptObject> devicePromptObjects = new();
    
    private PlayerInputHandler playerInput;

    private void Awake()
    {
        playerInput = FindFirstObjectByType<PlayerInputHandler>();
        ChangeButton();
    }

    private void OnEnable()
    {
        if (playerInput == null)
        {
            playerInput = FindFirstObjectByType<PlayerInputHandler>();
        }

        if (playerInput != null)
        {
            playerInput.OnControllerChange -= ChangeButton;
            playerInput.OnControllerChange += ChangeButton;
        }

        ChangeButton();
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.OnControllerChange -= ChangeButton;
        }
    }

    private void ChangeButton()
    {
        if (playerInput == null)
        {
            return;
        }

        GameObject mappedPrompt = FindMappedPrompt_TechBooh(playerInput.CurrentPromptDevice);
        SetMappedPromptsActive_TechBooh(false);

        if (mappedPrompt != null)
        {
            SetActiveIfPresent_TechBooh(xboxButton, false);
            SetActiveIfPresent_TechBooh(pcButton, false);
            mappedPrompt.SetActive(true);
            return;
        }

        // TechBooh: existing prefabs remain compatible without reconfiguration.
        SetActiveIfPresent_TechBooh(xboxButton, playerInput.isUsingController);
        SetActiveIfPresent_TechBooh(pcButton, !playerInput.isUsingController);
    }

    private GameObject FindMappedPrompt_TechBooh(InputPromptDeviceType deviceType)
    {
        GameObject exactMatch = null;
        GameObject genericFallback = null;

        foreach (DevicePromptObject mapping in devicePromptObjects)
        {
            if (mapping == null || mapping.promptObject == null)
            {
                continue;
            }

            if (mapping.deviceType == deviceType)
            {
                exactMatch = mapping.promptObject;
                break;
            }

            if (mapping.deviceType == InputPromptDeviceType.GenericGamepad)
            {
                genericFallback = mapping.promptObject;
            }
        }

        return exactMatch ?? (playerInput.isUsingController ? genericFallback : null);
    }

    private void SetMappedPromptsActive_TechBooh(bool active)
    {
        foreach (DevicePromptObject mapping in devicePromptObjects)
        {
            if (mapping != null && mapping.promptObject != null)
            {
                mapping.promptObject.SetActive(active);
            }
        }
    }

    private static void SetActiveIfPresent_TechBooh(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }
}
