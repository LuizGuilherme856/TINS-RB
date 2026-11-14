using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechBooh;
using System.Linq;

public class LuciferTransformationManager : MonoBehaviour
{
    public GameObject transformationScreenFx;
    public float screenFxDuration = 1f;
    public GameObject fakeLucifer;
    public GameObject lucifer1stForm, lucifer2ndForm;


    [Header("Arena Stuff")]
    public SpriteRenderer arenaSpriteRenderer;
    public Sprite arenaDestroyed;
    public BossFightArena bossFightArena;
    public BossFightUiInfo bossFightUiInfo;

    [Header("Tranformation stuff")]
    public float transformationDuration = 5f; // Transformation duration.
    public BossHealth luciferHealth;
    public EnemyHealth lucifer2ndFormHealth;
    public float bossInvulnerableTime = 15f; // Boss invulnerability duration after transformation.
    public float eyeZoomFov = 0.1f; // Zoom FOV for Lucifer's eye during transformation.
    public AudioClip song2ndForm;
    public List<GameObject> objectsToHideOnTransformation;

    [Header("Camera stuff")]
    [SerializeField] private MoveObjectToPos moveCamera;
    public CameraZoom cameraZoom;
    public float bossFightFov = 15; // Boss-fight FOV.


    [Space(10)]
    [Header("Death stuff")]
    public float deathAnimationDuration = 2f; // Delay before starting the death sequence
    public GameObject fadeToblack; // GameObject to fade to black after the death animation
    public SceneLoader sceneLoader; // SceneLoader reference for loading the next scene.
    public string nextSceneName = "NextScene"; // Name of the next scene.
    public string endlessModeSceneName = string.Empty;//Endless mode may load a different scene.
    public float delayToLoadScene = 5f; // Delay before loading the next scene.
    public GameObject luciferDeathObj;

    public PlayerData playerData;

    [Header("Other stuff")]
    public PlayerInputHandler playerInputHandler;

    private SimpleAudioManager audioManager;
    private ChangeGlobalLight changeGlobalLight;

    public bool startedTransformation;
    private void Awake()
    {
        try
        {
            audioManager = Try.GetComponent<SimpleAudioManager>(gameObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LuciferTransformation] Erro ao inicializar audioManager: {e.Message}\nStackTrace: {e.StackTrace}");
        }

        try
        {
            changeGlobalLight = Try.GetComponent<ChangeGlobalLight>(gameObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LuciferTransformation] Erro ao inicializar changeGlobalLight: {e.Message}\nStackTrace: {e.StackTrace}");
        }
    }

    private void OnEnable()
    {
        Debug.Log("LuciferTransformationManager OnEnable chamado");

        if (luciferHealth == null)
        {
            Debug.Log("Nao achou a vida do lucifer. Procurando...");
            luciferHealth = GameObject.FindGameObjectsWithTag("Enemy").
                Where(x => x.name == "Lucifer").FirstOrDefault().GetComponent<BossHealth>();

            //If the reference is still missing, skip directly to transformation to avoid trapping the player.
            if (luciferHealth == null)
            {
                TriggerTransformation();
                return;
            }
        }

        luciferHealth.OnMinHealth += TriggerTransformation;
        StartCoroutine(DoubleCheckTransformation());
    }

    private void TriggerTransformation()
    {
        Debug.Log("LuciferTransformationManager TriggerTransformation chamado");
        StartCoroutine(StartTransformation());
    }

    /// <summary>
    /// The normal transformation trigger has been unreliable.
    /// This fallback forces the transition
    /// to prevent the encounter from stalling.
    /// </summary>
    private IEnumerator DoubleCheckTransformation()
    {
        Debug.Log("LuciferTransformationManager DoubleCheckTransformation chamado");
        while (true)
        {
            Debug.Log("Verificando vida do Lucifer: " + luciferHealth.currentHealth);
            float hpPercentage = luciferHealth.currentHealth / luciferHealth.maxHealth;
            if (lucifer1stForm.activeInHierarchy && hpPercentage <= 0.15f)
            {
                StartCoroutine(StartTransformation());
            }
            //Run the check every configured interval.
            yield return new WaitForSeconds(5);
        }

    }

    /// <summary>
    /// Start Lucifer's transformation.
    /// Each critical step has its own try/catch so one failure does not abort the cutscene.
    /// Log failures in the important steps.
    /// </summary>
    public IEnumerator StartTransformation()
    {
        Debug.Log("Começou a transformação do tio Luci");
        if (startedTransformation)
        {
            Debug.Log("Ja esta se transformando. nao precisa chamar de novo");
            yield return null;
        }

        startedTransformation = true;


        // Each step has a separate try/catch as a recovery attempt; the transformation issue was not fully resolved.

        try
        {
            if (audioManager == null)
            {
                audioManager = Try.GetComponent<SimpleAudioManager>(gameObject);
            }
            if (changeGlobalLight == null)
            {
                changeGlobalLight = Try.GetComponent<ChangeGlobalLight>(gameObject);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LuciferTransformation] Erro ao inicializar audioManager ou changeGlobalLight: {e.Message}\nStackTrace: {e.StackTrace}");
        }


        // Play the level-completion sound.
        try { audioManager.PlayEnd(); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao tocar PlayEnd: {e.Message}"); }

        // Disable player movement.
        try { playerInputHandler.SetCanMove(false); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao desativar movimento do jogador: {e.Message}"); }

        // Flash the global light.
        try { changeGlobalLight.BlinkGlobalLightOnce(0.4f, 0.1f); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao piscar luz global: {e.Message}"); }

        // Kill all enemies except Lucifer.
        try { KillEnemies(); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao matar inimigos: {e.Message}"); }

        // Move the camera to Lucifer.
        try { moveCamera.StartMove(0); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao mover câmera para Lucifer: {e.Message}"); }

        // Activate stand-in Lucifer and play the transformation animation.
        try
        {
            lucifer1stForm.SetActive(false);
            fakeLucifer.SetActive(true);
            var animator = fakeLucifer.GetComponent<Animator>();
            if (animator != null)
                animator.SetTrigger("Transform");
            else
                Debug.LogError("[LuciferTransformation] Animator não encontrado no fakeLucifer.");
        }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao ativar fakeLucifer: {e.Message}"); }

        // Wait for the transformation duration.
        yield return new WaitForSeconds(transformationDuration);

        // Move the camera to Lucifer for the zoom.
        try { moveCamera.StartMove(2); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao mover câmera para zoom: {e.Message}"); }

        // Zoom tightly onto Lucifer.
        try { cameraZoom.Zoom(0.8f, eyeZoomFov); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao dar zoom na câmera: {e.Message}"); }

        yield return new WaitForSeconds(1);

        // Flash the global light again.
        try { changeGlobalLight.BlinkGlobalLight(); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao piscar luz global (2): {e.Message}"); }

        // Activate the transformation screen effect.
        try { transformationScreenFx.SetActive(true); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao ativar efeito de tela: {e.Message}"); }

                // Switch and play the boss-fight start sound.
        try {
            audioManager.SetStartAndLoopClip(song2ndForm);
            audioManager.Play(); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao tocar áudio da boss fight: {e.Message}"); }

        // Hide objects during transformation.
        try
        {
            foreach (var obj in objectsToHideOnTransformation)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao esconder objetos: {e.Message}"); }

        // Activate Lucifer's second form and configure health, invulnerability, and UI.
        try
        {
            lucifer2ndForm.SetActive(true);
            lucifer2ndFormHealth.SetInvulnerableTime(bossInvulnerableTime);
            lucifer2ndFormHealth.ReplenishHealth();
            bossFightArena.ChangeBossHealth(lucifer2ndFormHealth, 0);
            bossFightUiInfo.SetBossHealth(lucifer2ndFormHealth);
        }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao ativar Lucifer 2nd Form: {e.Message}"); }

        yield return new WaitForSeconds(screenFxDuration);

        // Deactivate stand-in Lucifer.
        try { fakeLucifer.SetActive(false); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao desativar fakeLucifer: {e.Message}"); }

        // Switch to the destroyed-arena sprite.
        try { arenaSpriteRenderer.sprite = arenaDestroyed; }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao trocar sprite da arena: {e.Message}"); }

        // Zoom the camera out.
        try { cameraZoom.Zoom(1f, 5); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao dar zoom out na câmera: {e.Message}"); }

        yield return new WaitForSeconds(2f);

        // Move the camera to the midpoint.
        try { moveCamera.StartMove(1); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao mover câmera para midpoint: {e.Message}"); }

        // Restore player movement.
        try { playerInputHandler.SetCanMove(true); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao reativar movimento do jogador: {e.Message}"); }

        // Apply the final boss-fight zoom.
        try { cameraZoom.Zoom(1f, bossFightFov); }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao dar zoom final na câmera: {e.Message}"); }

        // Subscribe to second-form Lucifer's death event.
        try { lucifer2ndFormHealth.OnZeroHealth += HandleLuciferDeath; }
        catch (System.Exception e) { Debug.LogError($"[LuciferTransformation] Erro ao adicionar evento de morte do Lucifer 2nd Form: {e.Message}"); }


    }


    private IEnumerator HandleLuciferDeath_Routine()
    {
        FinalizeRunScoreOnLuciferDeath_TechBooh();

        playerData.numberOfTimesKilledLucifer++;
        audioManager.PlayEnd(); // Play the fight-completion sound.

        KillEnemies();
        playerInputHandler.gameObject.GetComponent<Health>().invulnerable = true; // Make the player invulnerable during the death animation.
        playerInputHandler.SetCanMove(false); // Disable player movement.

        moveCamera.StartMove(0); // Move the camera to Lucifer.
        luciferDeathObj.SetActive(true); // Activate Lucifer's death object.

        //Wait for the death animation to finish.
        yield return new WaitForSeconds(deathAnimationDuration);

        // Activate the fade-to-black object; activation starts the effect.
        fadeToblack.SetActive(true);

        yield return new WaitForSeconds(delayToLoadScene);

        if(playerData.isEndlessModeOn && !string.IsNullOrEmpty(endlessModeSceneName))
        {
            sceneLoader.LoadSpecificScene(endlessModeSceneName); // Load the Endless mode scene.
        }
        else
        {
            sceneLoader.LoadSpecificScene(nextSceneName); // Load the next scene.
        }
    }

    private void HandleLuciferDeath()
    {
        StartCoroutine(HandleLuciferDeath_Routine());
    }

    /// <summary>
    /// TechBooh:
    /// In the regular campaign, defeating Lucifer ends the attempt outside the usual
    /// death flow, so finalize the score at this point.
    /// In Endless mode, boss death is an internal transition within
    /// the same attempt. Finalizing here would create a partial score and prevent
    /// later time, waves, and kills from being recorded when the run ends.
    /// </summary>
    private void FinalizeRunScoreOnLuciferDeath_TechBooh()
    {
        if (playerData != null && playerData.isEndlessModeOn)
        {
            return;
        }

        var scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager == null) return;

        scoreManager.FinalizeCurrentRunScore();
    }




    private void KillEnemies()
    {
        var enemiesAlive = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (var enemy in enemiesAlive)
        {
            if (enemy != luciferHealth) // Exclude Lucifer.
            {
                enemy.gameObject.SetActive(false); // Deactivate other enemies.
            }
        }
    }
    



}
