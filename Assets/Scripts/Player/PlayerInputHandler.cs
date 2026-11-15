using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;


public class PlayerInputHandler : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private Health health;
    private Camera mainCamera;
    private PlayerInput playerInput;
    private Vector2 oldJoystickAim, newJoystickAim, movementInput;

    [SerializeField] private bool isOnMenu = false, //Menus skip gameplay-component lookups.
         showDebug = true, playerClone = false; //The clone is summoned by the rabbit character's ability.

    [SerializeField] bool disableMouseAndKeyboard = false;


    [SerializeField] private bool forceReadKey = false; //Bypass normal input restrictions for explicit scripted flows.
    //Used to skip the introduction video.

    public event Action OnShootButtonDown, OnShootButtonUp, OnMeleeButtonDown, OnMeleeButtonUp,
          OnReload, OnSpecialAttackButtonDown, OnMiddleMouseButtonUp, OnDash;
    public event Action OnMouseScrollUp, OnMouseScrollDown;
    public event Action OnKey1, OnKey2, OnKey3, OnKey4;
    public event Action OnInteract;
    public bool IsInteractHeld { get; private set; }
    private bool skillCheckMovementLocked;

    public event Action OnEscape, OnInventory, OnCheat;
    public event Action OnLeftShoulderDown, OnRightShoulderDown;
    public event Action OnLeftShoulderUp, OnRightShoulderUp;
    public event Action OnMenuB, OnMenuY;


    public event Action OnControllerChange;

    private float changeGunDelay = 0.5f;//Throttle weapon switching.
    private float changeGunDelayTimer;
    private IDisposable anyButtonPressListener_TechBooh;

    // TechBooh: serialize this state so recompiling in Play Mode
    // does not temporarily change A prompts back to E.
    [field: SerializeField] public bool isUsingController { get; private set; }

    // TechBooh: retain the controller family so the UI
    // can automatically choose Xbox, PlayStation, or Nintendo Switch glyphs.
    [field: SerializeField] public InputPromptDeviceType CurrentPromptDevice { get; private set; }

    private bool canMove = true;
    private bool allowPauseInputWhenCantMove = false;
    private bool pauseInputBlocked = false;
    [SerializeField] private float pauseDelayAfterSceneStart_TechBooh = 2f;
    private float sceneStartRealtime_TechBooh;

    public GunManager gunManager;


    private void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        playerInput = GetComponent<PlayerInput>();
        if (isOnMenu == false)
            health = GetComponent<Health>();

        // TechBooh: this input handler may survive scene changes.
        // Restart the pause delay on every scene load,
        // not only in Start.
        sceneStartRealtime_TechBooh = Time.unscaledTime;
        SceneManager.sceneLoaded -= HandleSceneLoaded_TechBooh;
        SceneManager.sceneLoaded += HandleSceneLoaded_TechBooh;

    }

    private void OnEnable()
    {
        ApplyConsoleOnlyInputMode();
        // TechBooh: menus use InputSystemUIInputModule and may bypass
        // gameplay callbacks; this listener updates prompts for UI input too.
        anyButtonPressListener_TechBooh?.Dispose();
        anyButtonPressListener_TechBooh = InputSystem.onAnyButtonPress.Call(HandleAnyButtonPress_TechBooh);
    }

    private void OnDisable()
    {
        anyButtonPressListener_TechBooh?.Dispose();
        anyButtonPressListener_TechBooh = null;
    }
    void Start()
    {
        changeGunDelayTimer = changeGunDelay;
        if(isOnMenu == false)
            playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnDestroy()
    {
        anyButtonPressListener_TechBooh?.Dispose();
        SceneManager.sceneLoaded -= HandleSceneLoaded_TechBooh;
    }

    private void Update()
    {
        if(changeGunDelayTimer >0)
        {
            changeGunDelayTimer -= Time.deltaTime;
        }

        CheckIfIsUsingMouseOrController();
        DetectPointerAndAnalogInput_TechBooh();
        if (isOnMenu) return;
        if (canMove == false) return;
        
        if(playerClone == false)
        {
            if (health.GetCurrentHealth() <= 0) return;
        }

        ScrollMouse();
        ReadNumKeys();
    }

    void FixedUpdate()
    {
        if (isOnMenu || skillCheckMovementLocked || playerMovement == null) return;
        if(playerClone == false) //Apply only to the real player, not the clone.
        {
            if (canMove == false || health.GetCurrentHealth() <= 0 ) return;
        }

        playerMovement.Move(new Vector2(movementInput.x, movementInput.y));
    }

    public void ReadMoveInput(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);
        movementInput = context.ReadValue<Vector2>();
    }

    public Vector2 GetLeftAnalogInput()
    {
        return skillCheckMovementLocked ? Vector2.zero : movementInput;
    }

    public void ReadAimInput(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);
        newJoystickAim = context.ReadValue<Vector2>();
    }

    private void UpdateInputDeviceFromContext_TechBooh(InputAction.CallbackContext context)
    {
        // Canceled input often means a button on the previous device was released;
        // it must not replace prompts for the player's latest actual input.
        if (context.phase == InputActionPhase.Canceled)
        {
            return;
        }

        InputDevice inputDevice = context.control?.device;
        if (inputDevice is Gamepad || inputDevice is Keyboard || inputDevice is Mouse)
        {
            SetInputPromptDevice_TechBooh(InputPromptDeviceDetector_TechBooh.Detect(inputDevice));
        }
    }

    private void HandleAnyButtonPress_TechBooh(InputControl control)
    {
        InputDevice inputDevice = control?.device;
        if (inputDevice is Gamepad || inputDevice is Keyboard || inputDevice is Mouse)
        {
            SetInputPromptDevice_TechBooh(InputPromptDeviceDetector_TechBooh.Detect(inputDevice));
        }
    }

    private void DetectPointerAndAnalogInput_TechBooh()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && gamepad.wasUpdatedThisFrame)
        {
            Vector2 leftStick = gamepad.leftStick.ReadValue();
            Vector2 rightStick = gamepad.rightStick.ReadValue();
            if (leftStick.sqrMagnitude > 0.09f || rightStick.sqrMagnitude > 0.09f)
            {
                SetInputPromptDevice_TechBooh(InputPromptDeviceDetector_TechBooh.Detect(gamepad));
                return;
            }
        }

        if (IsConsoleOnlyInputBuild())
        {
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.wasUpdatedThisFrame && mouse.delta.ReadValue().sqrMagnitude > 1f)
        {
            SetInputPromptDevice_TechBooh(InputPromptDeviceType.KeyboardMouse);
        }
    }

    public Vector2 GetRightAnalogInput()
    {
        return newJoystickAim;
    }

    private void ReadNumKeys()
    {
        if (playerClone) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnKey1?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnKey2?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnKey3?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            OnKey4?.Invoke();
        }
    }

    private void ScrollMouse()
    {
        if (playerClone) return;
        if (canMove == false || health.GetCurrentHealth() <= 0) return;
        float mouseScroll = Input.mouseScrollDelta.y;
        if(Mathf.Abs(mouseScroll) > Mathf.Epsilon) //If nonzero,
        {
            if(mouseScroll > 0)
            {
                OnMouseScrollUp?.Invoke();
            }
            else
            {
                OnMouseScrollDown?.Invoke();
            }
        }
    }

    public float GetAimAngle()
    {
        if(playerClone == false)
        {
            if (canMove == false) return 0;
            if (health.GetCurrentHealth() <= 0) return 0;
        }

        if(isUsingController)
       {
            float angle;
            Vector2 joystickAim = new(newJoystickAim.x, newJoystickAim.y);

            //Debug.Log("joystick aim magnitude: " + joystickAim.magnitude);

            if(joystickAim.magnitude > 0.2f) //is Aiming with the analog stick
            {
                angle = Mathf.Atan2(joystickAim.y, joystickAim.x) * Mathf.Rad2Deg;
                oldJoystickAim = joystickAim;
                return angle;

            }
            else //is not aiming. Analog stick is on rest postion
            {
                //This is necessary because the player was always aiming at 0 degres when not aiming with the analog. It shhould be aiming in the last direction it was aiming
                angle = Mathf.Atan2(oldJoystickAim.y, oldJoystickAim.x) * Mathf.Rad2Deg;
                //oldJoystickAim = joystickAim;
                return angle;
            }
            
       }
        else //is using a mouse
        {
            Vector2 mouse = Input.mousePosition;
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(transform.position);

            //Find the vector from screen center to the mouse and calculate its angle.
            return Mathf.Atan2(mouse.y - screenPoint.y, mouse.x - screenPoint.x) * Mathf.Rad2Deg;

        }
       


    }

    /// <summary>
    /// Get movement angle for the dash impulse.
    /// </summary>
    /// <returns></returns>
    public float GetMovementAngle()
    {
        float angle;
        angle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;
        return angle;
    }
    public Vector2 GetMovementInput()
    {
        return skillCheckMovementLocked ? Vector2.zero : movementInput;
    }

    private void CheckIfIsUsingMouseOrController()
    {
        if (IsConsoleOnlyInputBuild())
        {
            SetIfItsUsingController(true);
            return;
        }

        // Vector2 joystickAim = new Vector2(Input.GetAxisRaw("JoystickAimHorizontal"), Input.GetAxisRaw("JoystickAimVertical"));

        // Debug.LogWarning("Analog-input detection needs improvement because controller layouts may differ.");

        //This uses Unity's legacy input API.
        if (Input.GetButtonDown("KeyboardCheck") || Input.GetButtonDown("MouseCheck"))
        {
            SetIfItsUsingController(false);
        }
        else if (Input.GetButtonDown("ControllerCheck") /*|| joystickAim.magnitude > 0.5f */)
        {
            SetIfItsUsingController(true);
        }
       
    }

    public void SetIfItsUsingController(bool isUsingController)
    {
        if (IsConsoleOnlyInputBuild() && isUsingController == false)
        {
            return;
        }

        InputPromptDeviceType deviceType = isUsingController
            ? InputPromptDeviceDetector_TechBooh.DetectCurrentGamepad()
            : InputPromptDeviceType.KeyboardMouse;

        SetInputPromptDevice_TechBooh(deviceType);
    }

    private void SetInputPromptDevice_TechBooh(InputPromptDeviceType deviceType)
    {
        bool usingController = deviceType != InputPromptDeviceType.KeyboardMouse;

        if (IsConsoleOnlyInputBuild() && usingController == false)
        {
            return;
        }

        // Raise the event when switching between controller families too,
        // even when the controller-used boolean remains unchanged.
        if (CurrentPromptDevice == deviceType && isUsingController == usingController)
        {
            return;
        }

        CurrentPromptDevice = deviceType;
        isUsingController = usingController;
        Cursor.visible = !usingController;
        OnControllerChange?.Invoke();
        Debug.Log($"Input prompts alterados para: {deviceType}");
    }

    // Blocking movement also stops firing and clears cached movement/aim input. Scene/menu instances
    // calling this must provide the required GunManager reference.
    public void SetSkillCheckMovementLocked(bool locked)
    {
        skillCheckMovementLocked = locked;
        if (locked) GetComponent<PlayerMovement>()?.StopImmediately();
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
        if (canMove == false)
        {
            gunManager?.StopShooting(); //Stop shooting while movement is blocked.
            movementInput = Vector2.zero; //Clear movement input.
            newJoystickAim = Vector2.zero; //Clear aim input.
        }
    }

    public void SetAllowPauseInputWhenCantMove(bool allow)
    {
        allowPauseInputWhenCantMove = allow;
    }

    public void SetPauseInputBlocked(bool blocked)
    {
        pauseInputBlocked = blocked;
    }

    private void ApplyConsoleOnlyInputMode()
    {
        if (playerClone) return;

        if (!IsConsoleOnlyInputBuild())
        {
            foreach (InputDevice device in InputSystem.devices)
            {
                if ((device is Keyboard || device is Mouse) && !device.enabled)
                    InputSystem.EnableDevice(device);
            }

            if (playerInput != null) playerInput.neverAutoSwitchControlSchemes = false;
            return;
        }

        DisableDeviceIfPresent(Keyboard.current);
        DisableDeviceIfPresent(Mouse.current);

        if (playerInput != null)
        {
            playerInput.neverAutoSwitchControlSchemes = true;
        }

        SetIfItsUsingController(true);
        Cursor.visible = false;
    }

    private void DisableDeviceIfPresent(InputDevice device)
    {
        if (device == null) return;
        if (!device.enabled) return;

        InputSystem.DisableDevice(device);
    }

    // This selects input behavior only. It does not implement platform user assignment,
    // controller-loss handling, or console services; those require separate integration.
    private bool IsConsoleOnlyInputBuild()
    {

        if (disableMouseAndKeyboard) return true;
#if UNITY_PS4 || UNITY_PS5 || UNITY_SWITCH || UNITY_GAMECORE || UNITY_XBOXONE
        return true;
#else
        return false;
#endif
    }

    #region readInput

    public void ReadReloadInput(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

        if(playerClone == false)
        {
            if (canMove == false || health.GetCurrentHealth() <= 0) return;

        }

        if (context.action.triggered)
        {
            Aux.Print("Reloading...", showDebug);
            OnReload?.Invoke();
        }
    }

    public void ReadSpecialAttackInput(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

        if (playerClone) return;

        if (canMove == false || health.GetCurrentHealth() <= 0) return;
        if (context.phase == InputActionPhase.Started)
        {
            Aux.Print("shoot special", showDebug);
            OnSpecialAttackButtonDown?.Invoke();
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            Aux.Print("released MMB", showDebug);
            OnMiddleMouseButtonUp?.Invoke();
        }

    }

    // public void ReadMapInput(InputAction.CallbackContext context)
    // {
    //     if (playerClone) return;
    //     if (context.phase == InputActionPhase.Started)
    //     {
    //        // OnMap?.Invoke();
    //     }

    // }
    public void Shoot(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

        if(playerClone == false)
        {
            if(health.GetCurrentHealth() <= 0) //makes it stop shooting when dead
            {
                OnShootButtonUp?.Invoke();
            }

        }
        if (canMove == false)
        {
            Debug.Log("Tentou usar o botao de Shoot quando o player nao podia se mover");
            return;
        }

        if (context.phase == InputActionPhase.Started)
        {
            Aux.Print("Atira", showDebug);
            OnShootButtonDown?.Invoke();
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            Aux.Print("soltou o bota de atirar", showDebug);
            OnShootButtonUp?.Invoke();
        }
    }

    public void ReadMeleeInput(InputAction.CallbackContext context)
    {
        if (canMove && !playerClone && TINS.Campaign.CampaignCombatRules.IsCampaignMission && TryGetComponent(out BeamMeleeAttack flash))
        {
            UpdateInputDeviceFromContext_TechBooh(context);
            if (context.performed && health.GetCurrentHealth() > 0) flash.FireBeam();
            return;
        }
        UpdateInputDeviceFromContext_TechBooh(context);

        if(playerClone == false) //Clones bypass this check.
        {
            if (canMove == false || health.GetCurrentHealth() <= 0)
            {
                Debug.Log("Tentou usar o botao de melee quando o player estava morto ou nao podia se mover");
                return;
            }
        }
        if (context.action.triggered)
        {
            OnMeleeButtonDown?.Invoke();
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            OnMeleeButtonUp?.Invoke();
        }
    }

    public void ReadInteractInput(InputAction.CallbackContext context)
    {
        IsInteractHeld = !context.canceled;
        UpdateInputDeviceFromContext_TechBooh(context);

        if (forceReadKey && context.action.triggered) OnInteract?.Invoke();

        if (playerClone) return;
        if (context.action.triggered)
        {
            OnInteract?.Invoke();
            Aux.Print("Interact", showDebug);
        }
    }


    public void ReadPauseInput(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

        if(forceReadKey && context.action.triggered) OnEscape?.Invoke(); //Explicitly bypass the normal restrictions.

        if (playerClone) return;

        //When movement is blocked and gameplay is not paused, block pause input too.
        //Prevent Escape from opening pause during dialogue.
        if (pauseInputBlocked) return;
        if (PlayFeedbacks.IsPauseBlockedByFeedback_TechBooh) return;
        if (CanReadPauseAfterSceneStart_TechBooh() == false) return;
        if(canMove == false && allowPauseInputWhenCantMove == false && Time.deltaTime > Mathf.Epsilon) return;
        if (health.IsAlive() == false) return;

        if (context.action.triggered)
        {
            OnEscape?.Invoke();
        }
    }

    public void ReadMenuB(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

        if (forceReadKey && context.action.triggered) OnMenuB?.Invoke();

        if (playerClone) return;
        if (context.action.triggered)
        {
            OnMenuB?.Invoke();
            Aux.Print("OnMenuB", showDebug);
        }
    }

    public void ReadMenuY(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

        if (forceReadKey && context.action.triggered) OnMenuY?.Invoke();

        if (playerClone) return;
        if (context.action.triggered)
        {
            OnMenuY?.Invoke();
            Aux.Print("OnMenuY", showDebug);
        }
    }

    public void ReadInventoryInput(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

        if (forceReadKey && context.action.triggered) OnInventory?.Invoke();

        if (playerClone) return;
        if (pauseInputBlocked) return;
        if (canMove == false && allowPauseInputWhenCantMove == false && Time.deltaTime > Mathf.Epsilon) return;
        if (health.IsAlive() == false) return;

        if (context.action.triggered)
        {
            OnInventory?.Invoke();
        }
    }

    public void ReadChangeGun(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

        if (playerClone) return;
        if (canMove == false || health.GetCurrentHealth() <= 0 || changeGunDelayTimer > 0) return;
        var value = context.ReadValue<float>();

        if (value > 0)
        {
            OnMouseScrollUp?.Invoke();
        }
        else
        {
            OnMouseScrollDown?.Invoke();
        }

        changeGunDelayTimer = changeGunDelay;
    }


    public void ReadDash(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

        if (playerClone == false) //Clones bypass this check.
        {
            if (canMove == false || health.GetCurrentHealth() <= 0) return;

        }
        if (context.action.triggered)
        {
            OnDash?.Invoke();
        }
    }

    /// <summary>
    /// Left shoulder corresponds to PlayStation L1.
    /// </summary>

    public void ReadLeftShoulderInput(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

        if (playerClone) return; // Ignore player clones.
        //if (canMove == false || health.GetCurrentHealth() <= 0) return; // Check whether the player can move and is alive.

        if (context.action.triggered)
        {
            OnLeftShoulderDown?.Invoke(); // Raise the left-shoulder event.
        }
        if(context.phase == InputActionPhase.Canceled)
        {
            OnLeftShoulderUp?.Invoke();
        }
    }

    /// <summary>
    /// Right shoulder corresponds to PlayStation R1.
    /// </summary>
    public void ReadRightShoulderInput(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

        if (playerClone) return; // Ignore player clones.
       // if (canMove == false || health.GetCurrentHealth() <= 0) return; // Check whether the player can move and is alive.

        if (context.action.triggered)
        {
            OnRightShoulderDown?.Invoke(); // Raise the right-shoulder event.
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            OnRightShoulderUp?.Invoke();
        }
    }

    public bool GetIsClone()
    {
        return playerClone;
    }

    // Only the F9/action entry point is Editor-gated below. Other debug components and public cheat
    // methods are not automatically excluded from release builds by this guard.
    public void ReadCheatInput(InputAction.CallbackContext context)
    {
        UpdateInputDeviceFromContext_TechBooh(context);

#if UNITY_EDITOR

        if (forceReadKey && context.action.triggered) OnCheat?.Invoke();
        if (playerClone) return;
        if (pauseInputBlocked) return;
        if (context.action.triggered)
        {
            OnCheat?.Invoke();
        }
#endif
    }

    private bool CanReadPauseAfterSceneStart_TechBooh()
    {
        if (isOnMenu)
        {
            return true;
        }

        return Time.unscaledTime - sceneStartRealtime_TechBooh >= pauseDelayAfterSceneStart_TechBooh;
    }

    private void HandleSceneLoaded_TechBooh(Scene scene, LoadSceneMode loadSceneMode)
    {
        // TechBooh: restart the pause-blocking window after a scene load
        // so the menu cannot appear over the entry transition.
        sceneStartRealtime_TechBooh = Time.unscaledTime;
    }

    #endregion

}
