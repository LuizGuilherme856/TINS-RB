using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputPromptDeviceType
{
    KeyboardMouse,
    Xbox,
    PlayStation,
    NintendoSwitch,
    GenericGamepad
}

public enum InputPromptAction
{
    Interaction,
    Dash,
    Fire,
    Special,
    Melee
}

[Serializable]
public class InputPromptProfile
{
    [Tooltip("Familia de dispositivo que usa este conjunto de botoes.")]
    public InputPromptDeviceType deviceType;

    [Tooltip("Quando ativo, os valores abaixo sao nomes de sprites do TMP. Quando inativo, sao exibidos como texto.")]
    public bool useTmpSprites = true;

    public string interactionButton;
    public string dashButton;
    public string fireButton;
    public string specialButton;
    public string meleeButton;

    public string GetButton(InputPromptAction action, bool allowTmpSprite)
    {
        string button = action switch
        {
            InputPromptAction.Interaction => interactionButton,
            InputPromptAction.Dash => dashButton,
            InputPromptAction.Fire => fireButton,
            InputPromptAction.Special => specialButton,
            InputPromptAction.Melee => meleeButton,
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(button))
        {
            return string.Empty;
        }

        return allowTmpSprite && useTmpSprites
            ? "<sprite name=" + button + ">"
            : button;
    }
}

/// <summary>
/// Centralizes text and glyphs for each controller family.
/// Add another profile to the ButtonsData asset list to support it.
/// </summary>
public class ButtonsData : ScriptableObject
{
    [SerializeField] private List<InputPromptProfile> promptProfiles = new();

    public string GetInteractionButton(InputPromptDeviceType deviceType, bool useTMP = true)
    {
        return GetButton(InputPromptAction.Interaction, deviceType, useTMP);
    }

    public string GetDashButton(InputPromptDeviceType deviceType, bool useTMP = true)
    {
        return GetButton(InputPromptAction.Dash, deviceType, useTMP);
    }

    public string GetFireButton(InputPromptDeviceType deviceType, bool useTMP = true)
    {
        return GetButton(InputPromptAction.Fire, deviceType, useTMP);
    }

    public string GetSpecialButton(InputPromptDeviceType deviceType, bool useTMP = true)
    {
        return GetButton(InputPromptAction.Special, deviceType, useTMP);
    }

    public string GetMeleeButton(InputPromptDeviceType deviceType, bool useTMP = true)
    {
        return GetButton(InputPromptAction.Melee, deviceType, useTMP);
    }

    // TechBooh: keep compatibility with older callers while systems
    // migrate from the PC/Xbox boolean to the actual device family.
    public string GetInteractionButton(bool isUsingController, bool useTMP = true)
    {
        return GetInteractionButton(GetLegacyDeviceType(isUsingController), useTMP);
    }

    public string GetDashButton(bool isUsingController, bool useTMP = true)
    {
        return GetDashButton(GetLegacyDeviceType(isUsingController), useTMP);
    }

    public string GetFireButton(bool isUsingController, bool useTMP = true)
    {
        return GetFireButton(GetLegacyDeviceType(isUsingController), useTMP);
    }

    public string GetSpecialButton(bool isUsingController, bool useTMP = true)
    {
        return GetSpecialButton(GetLegacyDeviceType(isUsingController), useTMP);
    }

    public string GetMeleeButton(bool isUsingController, bool useTMP = true)
    {
        return GetMeleeButton(GetLegacyDeviceType(isUsingController), useTMP);
    }

    private string GetButton(InputPromptAction action, InputPromptDeviceType deviceType, bool useTMP)
    {
        InputPromptProfile profile = FindProfile(deviceType);

        // Unknown gamepads use Xbox prompts as the most common fallback
        // common on PC. Keyboard input never uses this gamepad fallback.
        if (profile == null && deviceType != InputPromptDeviceType.KeyboardMouse)
        {
            profile = FindProfile(InputPromptDeviceType.GenericGamepad)
                ?? FindProfile(InputPromptDeviceType.Xbox);
        }

        if (profile == null)
        {
            Debug.LogWarning($"ButtonsData nao possui um perfil para {deviceType}.", this);
            return string.Empty;
        }

        return profile.GetButton(action, useTMP);
    }

    private InputPromptProfile FindProfile(InputPromptDeviceType deviceType)
    {
        if (promptProfiles == null)
        {
            return null;
        }

        foreach (InputPromptProfile profile in promptProfiles)
        {
            if (profile != null && profile.deviceType == deviceType)
            {
                return profile;
            }
        }

        return null;
    }

    private static InputPromptDeviceType GetLegacyDeviceType(bool isUsingController)
    {
        return isUsingController
            ? InputPromptDeviceDetector_TechBooh.DetectCurrentGamepad()
            : InputPromptDeviceType.KeyboardMouse;
    }
}

/// <summary>
/// Detect controller family independently of individual binding names.
/// Supports direct HID identification and platform fallbacks on consoles.
/// </summary>
public static class InputPromptDeviceDetector_TechBooh
{
    public static InputPromptDeviceType Detect(InputDevice device)
    {
        if (device is Keyboard || device is Mouse)
        {
            return InputPromptDeviceType.KeyboardMouse;
        }

        if (device is not Gamepad gamepad)
        {
            return GetPlatformGamepadType();
        }

        string description = string.Join(" ",
            gamepad.layout,
            gamepad.name,
            gamepad.displayName,
            gamepad.description.interfaceName,
            gamepad.description.manufacturer,
            gamepad.description.product).ToLowerInvariant();

        if (ContainsAny(description, "dualshock", "dualsense", "playstation", "sony"))
        {
            return InputPromptDeviceType.PlayStation;
        }

        if (ContainsAny(description, "nintendo", "switch", "joy-con", "joycon"))
        {
            return InputPromptDeviceType.NintendoSwitch;
        }

        if (ContainsAny(description, "xinput", "xbox"))
        {
            return InputPromptDeviceType.Xbox;
        }

        InputPromptDeviceType platformType = GetPlatformGamepadType();
        return platformType == InputPromptDeviceType.GenericGamepad
            ? InputPromptDeviceType.GenericGamepad
            : platformType;
    }

    public static InputPromptDeviceType DetectCurrentGamepad()
    {
        return Gamepad.current != null
            ? Detect(Gamepad.current)
            : GetPlatformGamepadType();
    }

    private static bool ContainsAny(string value, params string[] terms)
    {
        foreach (string term in terms)
        {
            if (value.Contains(term))
            {
                return true;
            }
        }

        return false;
    }

    private static InputPromptDeviceType GetPlatformGamepadType()
    {
#if UNITY_PS4 || UNITY_PS5
        return InputPromptDeviceType.PlayStation;
#elif UNITY_SWITCH
        return InputPromptDeviceType.NintendoSwitch;
#elif UNITY_GAMECORE || UNITY_XBOXONE
        return InputPromptDeviceType.Xbox;
#else
        return InputPromptDeviceType.GenericGamepad;
#endif
    }
}
