
using TechBooh;
using UnityEngine;
using UnityEngine.Audio;
/// TODO: Simplify this implementation.

/// <summary>
/// Stores settings and refreshes their UI representations, such as sliders.
/// </summary>
public class GameDataAndSettings : MonoBehaviour
{
    private static bool hasLoadedPlayerSave = false;
    public AudioMixer audioMixer;
    [SerializeField] private float defaultVolume = -20f;
    [SerializeField] private LanguageData languageData;
    [SerializeField] private SettingsData settingsData;
    public bool postProcessing = true;
    public bool fullScreen { set; get; }
    public bool isVsyncOn { set; get; }
    public bool v_sync { get; set; }
    public float numberOfParticles { set; get; }

    public float sfxVolume, musicVolume, uiVolume, shotsVolume, cameraShakeIntensity =1f, 
        cameraFollowMouseIntensity =1, frameCap =244; //Cached values simplify access to the settings.

    public float resWidth, resheight;
    public Resolution resolution { set; get; }

    public readonly string 
        SFX_VOLUME_NAME = "SfxVolume", MUSIC_VOLUME_NAME = "MusicVolume", UI_VOLUME_NAME = "UiVolume",
        CAM_SHAKE_INTENSITY_NAME = "CameraShakeIntensity", LENS_DISTORTION_INTENSITY_NAME = "LensDistortionIntensity", 
        LANGUAGE_NAME = "language", CAMERA_FOLLOW_MOUSE_NAME = "CameraFollowsMouse", BLOOD_PARTICLES_NAME = "bloodParticles",
        PARTICLES_NAME = "particles", FRAME_CAP_NAME = "frameCap", V_SYNC_NAME = "vSync";


    [HideInInspector] public bool hasPlayedStoryIntro = false; //Used by TriggerStartGame.

    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private PlayerData playerData;
    private PlayerInputHandler playerInputHandler;

    private void Awake()
    {
        if (playerHolder == null) playerHolder = Try.FindObjectOfType<PlayerHolder>();

        if (playerData == null && playerHolder != null)
            playerData = playerHolder.playerData;

        if (playerHolder)
            playerInputHandler = playerHolder.GetPlayerInputHandler();

        LoadPlayerProgressOnce();
    }

    private void OnEnable()
    {
        if(playerInputHandler)
            playerInputHandler.OnEscape += RefreshSettings;
    }

    private void OnDisable()
    {
        if (playerInputHandler)
            playerInputHandler.OnEscape -= RefreshSettings;
    }

    private void Start()
    {
        
        //PlayerPrefs.DeleteAll();

        SingletonPattern();

        RefreshLanguage();

        RefreshSettings();
    }

    // Load once for this application session, before Start-based menu/gameplay consumers read progression.
    // The guard is not keyed by platform user; a future user-switch flow must explicitly reload the correct profile.
    private void LoadPlayerProgressOnce()
    {
        if (hasLoadedPlayerSave) return;
        if (playerData == null) return;

        playerData.LoadPlayerData();
        hasLoadedPlayerSave = true;
    }

    public void RefreshSettings()
    {
        RefreshPostProcessing();

        RefreshAudioSettings();

        RefreshNumberOfParticles();

        RefreshCameraShakeIntensity();

        RefreshLensDistortionIntensity();

        RefreshCameraFollowMouseIntensity();

        RefreshFullScreen();

        RefreshResolutions();

        RefreshBloodParticles();

        RefreshParticles();

        //RefreshFrameCap();

        RefreshVsync();
    }

    private void SingletonPattern()
    {
        int numberGameSessions = FindObjectsByType<GameDataAndSettings>(FindObjectsSortMode.None).Length; // Count all session instances; each scene may otherwise create a duplicate and reset the score.

        if (numberGameSessions > 1) //Keep only one instance using the singleton pattern.
        {
            Debug.LogError("Mais de um game session: " + numberGameSessions);

            gameObject.SetActive(false); //Deactivate duplicates before destroying them to avoid callbacks.
            Destroy(gameObject);//Destroy this duplicate session instance.
        }
        else
        {
            if (transform.parent != null) transform.SetParent(null, true);
            DontDestroyOnLoad(gameObject); //Preserve the object across scene loads.
            //Reuse the same GAMESESSION object in every scene.
        }
    }


    private void RefreshLanguage()
    {
        if (PlayerPrefs.HasKey(LANGUAGE_NAME))
            languageData.SetLanguage(PlayerPrefs.GetInt(LANGUAGE_NAME));
        else
        {
            languageData.SetDefaultLanguage();
            // Debug.Log("No saved particle count");
        }
    }
    private void RefreshNumberOfParticles()
    {
        if (PlayerPrefs.HasKey("numberOfParticles"))
            numberOfParticles = PlayerPrefs.GetFloat("numberOfParticles");
        else
        {
            numberOfParticles = 1f;
            // Debug.Log("No saved particle count");
        }
    }

    private void RefreshBloodParticles()
    {
        if (PlayerPrefs.HasKey(BLOOD_PARTICLES_NAME))
            settingsData.SetBloodParticles(PlayerPrefs.GetFloat(BLOOD_PARTICLES_NAME));
        else
        {
            settingsData.SetBloodParticles(1);
            // Debug.Log("No saved particle count");
        }
    }

    private void RefreshParticles()
    {
        if (PlayerPrefs.HasKey(PARTICLES_NAME))
            settingsData.SetParticles(PlayerPrefs.GetFloat(PARTICLES_NAME));
        else
        {
            settingsData.SetParticles(1);
            // Debug.Log("No saved particle count");
        }
    }


    private void RefreshCameraShakeIntensity()
    {
        float newCameraShake = 1;
        if (PlayerPrefs.HasKey(CAM_SHAKE_INTENSITY_NAME))
        {
            newCameraShake = PlayerPrefs.GetFloat(CAM_SHAKE_INTENSITY_NAME);
        }
        else
        {
            newCameraShake = 1f; //Use the default value when no saved value exists.
        }

        cameraShakeIntensity = newCameraShake;
        settingsData.SetCameraShake(newCameraShake);
    }

    private void RefreshLensDistortionIntensity()
    {
        float newDistortion =1;
        if (PlayerPrefs.HasKey(LENS_DISTORTION_INTENSITY_NAME))
        {
            newDistortion = PlayerPrefs.GetFloat(LENS_DISTORTION_INTENSITY_NAME);

        }
        else
        {
            newDistortion = 1f;
            // Debug.Log("No saved particle count");
        }

        //lensDistortionIntensity = newDistortion;
        settingsData.SetlensDistorion(newDistortion);
    }

    private void RefreshCameraFollowMouseIntensity()
    {
        if (PlayerPrefs.HasKey(CAMERA_FOLLOW_MOUSE_NAME))
            cameraFollowMouseIntensity = PlayerPrefs.GetFloat(CAMERA_FOLLOW_MOUSE_NAME);
        else
        {
            cameraFollowMouseIntensity = 0.5f;
            // Debug.Log("No saved particle count");
        }
    }

    private void RefreshAudioSettings()
    {
        if (PlayerPrefs.HasKey(UI_VOLUME_NAME))
        {
            uiVolume = PlayerPrefs.GetFloat(UI_VOLUME_NAME);
            audioMixer.SetFloat(UI_VOLUME_NAME, uiVolume);
        }
        else
        {
            audioMixer.SetFloat(UI_VOLUME_NAME, defaultVolume);
            uiVolume = defaultVolume;
            // Debug.Log("No saved UI volume");
        }

        if (PlayerPrefs.HasKey(SFX_VOLUME_NAME))
        {
            sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_NAME);
            audioMixer.SetFloat(SFX_VOLUME_NAME, sfxVolume);
        }
        else
        {
            audioMixer.SetFloat(SFX_VOLUME_NAME, defaultVolume);
            sfxVolume = defaultVolume;
            // Debug.Log("No saved SFX volume");
        }


        if (PlayerPrefs.HasKey(MUSIC_VOLUME_NAME))
        {
            musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_NAME);
            audioMixer.SetFloat(MUSIC_VOLUME_NAME, musicVolume);
        }
        else
        {
            audioMixer.SetFloat(MUSIC_VOLUME_NAME, defaultVolume);
            musicVolume = defaultVolume;
            // Debug.Log("No saved music volume");
        }
    }

    
    private void RefreshPostProcessing()
    {
        if (PlayerPrefs.HasKey("postProcessing"))
            postProcessing = (PlayerPrefs.GetInt("postProcessing") == 1) ? true : false;
        else
        {
            postProcessing = true; //Default to true on first access.
            // Debug.Log("No saved post-processing setting");
        }
    }

    private void RefreshFullScreen()
    {
        //if (PlayerPrefs.HasKey("isFullScreen"))
        //    fullScreen = (PlayerPrefs.GetInt("isFullScreen") == 1) ? true : false;
        //else
        //{
        //    fullScreen = true; //Default to true on first access.
        // Debug.Log("No saved fullscreen setting");
        //}
    }

    private void RefreshVsync()
    {
        //if (PlayerPrefs.HasKey("isVsyncOn"))
        //{

        //    QualitySettings.vSyncCount = PlayerPrefs.GetInt("isVsyncOn");
        //    Debug.Log("lendo... ativa a vsync. valor: "+ PlayerPrefs.GetInt("isVsyncOn"));
        //}
        //else
        //{
        //    QualitySettings.vSyncCount = 0;
        //    Debug.Log("lendo... desativa a vsync. valor: " + QualitySettings.vSyncCount);
        //}

        //isVsyncOn = (QualitySettings.vSyncCount == 1) ? true : false;

    }

    /*
    private void RefreshFrameCap()
    {
        if (PlayerPrefs.HasKey(FRAME_CAP_NAME))
        {
            frameCap = PlayerPrefs.GetFloat(FRAME_CAP_NAME) ;
            
            Debug.Log("reading frame cap value: " + frameCap);
        }
        else
        {
            Debug.Log("No saved frame cap value found");

           frameCap = 244;
        }

        Application.targetFrameRate = (int)frameCap;
    }
    */

    private void RefreshResolutions()
    {
        //if (PlayerPrefs.HasKey("resolutionWidth") && PlayerPrefs.HasKey("resolutionHeight"))
        //{
        //    int resWidth = PlayerPrefs.GetInt("resolutionWidth");
        //    int resHeight = PlayerPrefs.GetInt("resolutionHeight");
        //    Screen.SetResolution(resWidth, resHeight, fullScreen);
        //}
        //else
        //{
        // Debug.Log("No saved resolution found");
        //}

    }

}
