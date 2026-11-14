///Implementation based on:
///https://www.youtube.com/watch?v=YOaYQrN1oYQ
///Apply user-selected settings and persist them in PlayerPrefs.
using System.Collections.Generic;
using TechBooh;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// This menu writes game-owned PlayerPrefs. Kamgam also persists a separate settings payload;
// storage/profile migration must cover both rather than only the player.blood progression file.
public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    [SerializeField] private SettingsData settingsData;
    [SerializeField] private GameObject videoTab, gameTab, audioTab, mainScreen, keyBindingsTab;

    [Header("Tab First Buttons")]
    [SerializeField] private GameObject videoTab1stButton;
    [SerializeField] private GameObject gameTab1stButton;
    [SerializeField] private GameObject audioTab1stButton;
    [SerializeField] private GameObject mainScreen1stButton;
    [SerializeField] private GameObject keyBindingsTab1stButton;

    public EventSystem eventSystem;
    //[SerializeField] Dropdown resolutionDropdown;
    //private Resolution[] resolutions;
    private GameDataAndSettings gameData;
    private PlayerInputHandler playerInputHandler_TechBooh;
    private bool backInputReleased_TechBooh = true;


    private void Awake()
    {
        gameData = Try.FindObjectOfType<GameDataAndSettings>();
        playerInputHandler_TechBooh = FindFirstObjectByType<PlayerInputHandler>(FindObjectsInactive.Include);
    }

    void Start()
    {
        if (gameData != null && gameData.isActiveAndEnabled) return;
        var allGameData = Object.FindObjectsByType<GameDataAndSettings>( FindObjectsInactive.Include,FindObjectsSortMode.None); // Include inactive instances.
        foreach (var gd in allGameData)
        {
            if (gd.gameObject.activeInHierarchy)
            {
                gameData = gd;
                break;
            }
        }
        // Fall back to the first instance when no active instance is found.
        if (gameData == null && allGameData.Length > 0)
            gameData = allGameData[0];
    }

    private void Update()
    {
        HandleStandaloneSettingsBackInput_TechBooh();
    }

    private void OnEnable()
    {
        TrySubscribeBackInput_TechBooh();
    }

    private void OnDisable()
    {
        if (playerInputHandler_TechBooh != null)
        {
            playerInputHandler_TechBooh.OnMenuB -= HandleMenuBack_TechBooh;
        }
    }

    private void HandleResolutions()
    {
        Debug.LogError("DESABILITEI ISSO. ");
        //resolutions = Screen.resolutions;

        //resolutionDropdown.ClearOptions();
        //int currentResolutionIndex = 0;

        //List<string> options = new List<string>();
        //for (int i = 0; i < resolutions.Length; i++) //Store values as a string.
        //{
        //    string option = resolutions[i].width + " x " + resolutions[i].height;
        //    options.Add(option);

        //    if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
        //    {
        //        currentResolutionIndex = i;
        //        gameData.resolution = resolutions[i];
        //    }
        //}
        //resolutionDropdown.AddOptions(options);
        //resolutionDropdown.value = currentResolutionIndex;
        //resolutionDropdown.RefreshShownValue();
    }
    public void ShowKeyBindingsTab_Button()
    {
        videoTab.SetActive(false);
        gameTab.SetActive(false);
        audioTab.SetActive(false);
        mainScreen.SetActive(false);
        keyBindingsTab.SetActive(true);


        if (eventSystem && keyBindingsTab1stButton)
            eventSystem.SetSelectedGameObject(keyBindingsTab1stButton);
    }

    public void ShowVideoTab_Button()
    {

        videoTab.SetActive(true);
        gameTab.SetActive(false);
        audioTab.SetActive(false);
        mainScreen.SetActive(false);
        keyBindingsTab.SetActive(false);

        if (eventSystem && videoTab1stButton)
            eventSystem.SetSelectedGameObject(videoTab1stButton);
    }

    public void ShowGameTab_Button()
    {
        videoTab.SetActive(false);
        gameTab.SetActive(true);
        audioTab.SetActive(false);
        mainScreen.SetActive(false);
        keyBindingsTab.SetActive(false);

        if (eventSystem && gameTab1stButton)
            eventSystem.SetSelectedGameObject(gameTab1stButton);
    }

    public void ShowAudioTab_Button()
    {
        videoTab.SetActive(false);
        gameTab.SetActive(false);
        audioTab.SetActive(true);
        mainScreen.SetActive(false);
        keyBindingsTab.SetActive(false);

        if (eventSystem && audioTab1stButton)
            eventSystem.SetSelectedGameObject(audioTab1stButton);
    }

    public void ShowMainScreen_Button()
    {
        videoTab.SetActive(false);
        gameTab.SetActive(false);
        audioTab.SetActive(false);
        mainScreen.SetActive(true);
        keyBindingsTab.SetActive(false);

        if (eventSystem && mainScreen1stButton)
            eventSystem.SetSelectedGameObject(mainScreen1stButton);
    }

    /// <summary>
    /// TechBooh:
    /// The Settings Menu scene lacked consistent controller-back handling
    /// inside submenus. Handle it locally to match the Back button:
    /// return from any submenu to the main
    /// options screen.
    /// </summary>
    private void HandleMenuBack_TechBooh()
    {
        if (IsStandaloneSettingsScene_TechBooh() == false)
        {
            return;
        }

        if (isActiveAndEnabled == false || gameObject.activeInHierarchy == false)
        {
            return;
        }

        if (mainScreen != null && mainScreen.activeInHierarchy)
        {
            return;
        }

        if (IsAnySubMenuOpen_TechBooh() == false)
        {
            return;
        }

        ShowMainScreen_Button();
    }

    private void HandleStandaloneSettingsBackInput_TechBooh()
    {
        if (IsStandaloneSettingsScene_TechBooh() == false)
        {
            return;
        }

        bool backPressed = IsBackPressed_TechBooh();

        if (backPressed == false)
        {
            backInputReleased_TechBooh = true;
            return;
        }

        if (backInputReleased_TechBooh == false)
        {
            return;
        }

        backInputReleased_TechBooh = false;
        HandleMenuBack_TechBooh();
    }

    private void TrySubscribeBackInput_TechBooh()
    {
        if (IsStandaloneSettingsScene_TechBooh() == false)
        {
            return;
        }

        if (playerInputHandler_TechBooh == null)
        {
            playerInputHandler_TechBooh = FindFirstObjectByType<PlayerInputHandler>(FindObjectsInactive.Include);
        }

        if (playerInputHandler_TechBooh == null)
        {
            return;
        }

        playerInputHandler_TechBooh.OnMenuB -= HandleMenuBack_TechBooh;
        playerInputHandler_TechBooh.OnMenuB += HandleMenuBack_TechBooh;
    }

    private bool IsStandaloneSettingsScene_TechBooh()
    {
        return SceneManager.GetActiveScene().name == "Settings Menu";
    }

    private bool IsAnySubMenuOpen_TechBooh()
    {
        return IsTabOpen_TechBooh(videoTab)
            || IsTabOpen_TechBooh(gameTab)
            || IsTabOpen_TechBooh(audioTab)
            || IsTabOpen_TechBooh(keyBindingsTab);
    }

    private bool IsTabOpen_TechBooh(GameObject tab)
    {
        return tab != null && tab.activeInHierarchy;
    }

    private bool IsBackPressed_TechBooh()
    {
        if (Gamepad.current != null)
        {
            return Gamepad.current.buttonEast.isPressed;
        }

        return Keyboard.current != null && Keyboard.current.escapeKey.isPressed;
    }



    #region Setters
    public void SetResolution(int resolutionIndex)
    {
        Debug.LogError("DESABILITEI ISSO. ");
        return;

        //Resolution newResolution = resolutions[resolutionIndex];
        //Screen.SetResolution(newResolution.width, newResolution.height, Screen.fullScreen);
        //PlayerPrefs.SetInt("resolutionWidth", newResolution.width);
        //PlayerPrefs.SetInt("resolutionHeight", newResolution.height);
        //PlayerPrefs.Save();
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        int aux = isFullScreen == true ? 1 : 0;
        PlayerPrefs.SetInt("isFullScreen", aux);
        PlayerPrefs.Save();
    }

    public void SetVSync(bool isVsyncOn)
    {
        if (isVsyncOn)
        {
            QualitySettings.vSyncCount = 1;
            gameData.isVsyncOn = true;
            Debug.Log("ativa a vsync");
            //SetFrameCap(244);
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            gameData.isVsyncOn = false;
            Debug.Log("desativa a vsync");

        }

        int aux = isVsyncOn == true ? 1 : 0;
        PlayerPrefs.SetInt("isVsyncOn", aux);
        PlayerPrefs.Save();
    }



    public void SetBloodParticles(float amount)
    {
        if (gameData == null) gameData = Try.FindObjectOfType<GameDataAndSettings>();


        PlayerPrefs.SetFloat(gameData.BLOOD_PARTICLES_NAME, amount);
        settingsData.SetBloodParticles(amount);
        ApplyGameplaySettings();
    }

    public void SetParticles(float amount)
    {
        if (gameData == null) gameData = Try.FindObjectOfType<GameDataAndSettings>();
        PlayerPrefs.SetFloat(gameData.PARTICLES_NAME, amount);
        settingsData.SetParticles(amount);
        ApplyGameplaySettings();
    }

    public void SetUIVolume(float newVolume)
    {
        if (gameData == null) gameData = Try.FindObjectOfType<GameDataAndSettings>();
        float volume = newVolume;
        if (newVolume < -33) //Set volume to zero.
        {
            volume = -85;
        }

        audioMixer.SetFloat(gameData.UI_VOLUME_NAME, volume);
        PlayerPrefs.SetFloat(gameData.UI_VOLUME_NAME, volume);
        gameData.uiVolume = volume;
    }

    public void SetMusicVolume(float newVolume)
    {
        if (gameData == null) gameData = Try.FindObjectOfType<GameDataAndSettings>();
        float volume = newVolume;
        if (newVolume < -33) //Set volume to zero.
        {
            volume = -85;
        }

        audioMixer.SetFloat(gameData.MUSIC_VOLUME_NAME, volume);
        PlayerPrefs.SetFloat(gameData.MUSIC_VOLUME_NAME, volume);
        gameData.musicVolume = volume;
    }
    public void SetSFXVolume(float newVolume)
    {
        if (gameData == null) gameData = Try.FindObjectOfType<GameDataAndSettings>();
        float volume = newVolume;
        if (newVolume < -33) //Set volume to zero.
        {
            volume = -85;
        }

        audioMixer.SetFloat(gameData.SFX_VOLUME_NAME, volume);
        PlayerPrefs.SetFloat(gameData.SFX_VOLUME_NAME, volume);
        gameData.sfxVolume = volume;
    }

    public void SetPostProcessing(bool postprocessing)
    {
        if (gameData == null) gameData = Try.FindObjectOfType<GameDataAndSettings>();

        gameData.postProcessing = postprocessing;
        int aux = postprocessing ? 1 : 0;
        PlayerPrefs.SetInt("postProcessing", aux);
        ApplyGameplaySettings();
    }



    public void SetCameraShakeIntensity(float shakeIntensity)
    {
        if (gameData == null) gameData = Try.FindObjectOfType<GameDataAndSettings>();
        if (shakeIntensity < 0 || shakeIntensity > 1)
        {
            Debug.LogWarning("Camera shake value should be between [0,1]. You tried to add the value: " + shakeIntensity);
            return;
        } //return

        settingsData.SetCameraShake(shakeIntensity);
        gameData.cameraShakeIntensity = shakeIntensity;
        PlayerPrefs.SetFloat(gameData.CAM_SHAKE_INTENSITY_NAME, shakeIntensity);
        ApplyGameplaySettings();
    }

    public void SetCameraFollowMouseDistance(float intensity)
    {
        if (gameData == null) gameData = Try.FindObjectOfType<GameDataAndSettings>();

        if (intensity < 0 || intensity > 1)
        {
            Debug.LogWarning("Camera follow mouse value should be between [0,1]. You tried to add the value: " + intensity);
            return;
        } //return

        gameData.cameraFollowMouseIntensity = intensity;
        PlayerPrefs.SetFloat(gameData.CAMERA_FOLLOW_MOUSE_NAME, intensity);
        ApplyGameplaySettings();
    }

    public void SetLensDistortionIntensity(float intensity)
    {
        if (gameData == null) gameData = Try.FindObjectOfType<GameDataAndSettings>();
        if (intensity < 0 || intensity > 1)
        {
            Debug.LogWarning("lens intensity value should be between [0,1]. You tried to add the value: " + intensity);
            return;
        }

        settingsData.SetlensDistorion(intensity);
        PlayerPrefs.SetFloat(gameData.LENS_DISTORTION_INTENSITY_NAME, intensity);
        ApplyGameplaySettings();
    }

    #endregion

    private void ApplyGameplaySettings()
    {
        foreach (var postProcessingManager in FindObjectsByType<EnableDisablePostProcessing>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            postProcessingManager.Refresh();
        }

        foreach (var cameraShakeManager in FindObjectsByType<ManageCameraShake>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            cameraShakeManager.UpdateCameraShake();
        }

        foreach (var particleManager in FindObjectsByType<SetMaxParticles>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            particleManager.RefreshMaxParticles();
        }

        foreach (var lensDistortionManager in FindObjectsByType<ManageLensDistortion>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            lensDistortionManager.RefreshIntensity();
        }

        foreach (var chromaticAberrationManager in FindObjectsByType<ManageChromaticAberration>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            chromaticAberrationManager.RefreshIntensity();
        }
    }

    public void SavePlayerPrefs()
    {
        PlayerPrefs.Save();
    }
    
    // Verify that all Inspector-assigned objects are non-null.
private void VerifyInspectorReferences()
{
    bool hasError = false;

    if (videoTab == null)
    {
        Debug.LogError("SettingsMenu: 'videoTab' não foi atribuído no Inspector.");
        hasError = true;
    }
    if (gameTab == null)
    {
        Debug.LogError("SettingsMenu: 'gameTab' não foi atribuído no Inspector.");
        hasError = true;
    }
    if (audioTab == null)
    {
        Debug.LogError("SettingsMenu: 'audioTab' não foi atribuído no Inspector.");
        hasError = true;
    }
    if (mainScreen == null)
    {
        Debug.LogError("SettingsMenu: 'mainScreen' não foi atribuído no Inspector.");
        hasError = true;
    }
    if (keyBindingsTab == null)
    {
        Debug.LogError("SettingsMenu: 'keyBindingsTab' não foi atribuído no Inspector.");
        hasError = true;
    }
    if (videoTab1stButton == null)
    {
        Debug.LogError("SettingsMenu: 'videoTab1stButton' não foi atribuído no Inspector.");
        hasError = true;
    }
    if (gameTab1stButton == null)
    {
        Debug.LogError("SettingsMenu: 'gameTab1stButton' não foi atribuído no Inspector.");
        hasError = true;
    }
    if (audioTab1stButton == null)
    {
        Debug.LogError("SettingsMenu: 'audioTab1stButton' não foi atribuído no Inspector.");
        hasError = true;
    }
    if (mainScreen1stButton == null)
    {
        Debug.LogError("SettingsMenu: 'mainScreen1stButton' não foi atribuído no Inspector.");
        hasError = true;
    }
    if (keyBindingsTab1stButton == null)
    {
        Debug.LogError("SettingsMenu: 'keyBindingsTab1stButton' não foi atribuído no Inspector.");
        hasError = true;
    }

    if (eventSystem == null)
    {
        Debug.LogError("SettingsMenu: 'eventSystem' não foi atribuído no Inspector.");
        hasError = true;
    }

    if (!hasError)
    {
        Debug.Log("SettingsMenu: Todas as referências do Inspector estão corretamente atribuídas.");
    }
}


}
