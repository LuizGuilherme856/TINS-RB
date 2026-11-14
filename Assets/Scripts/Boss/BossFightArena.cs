using MoreMountains.Feedbacks;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TechBooh;
using UnityEngine;


[RequireComponent(typeof(SimpleAudioManager))]

[RequireComponent(typeof(SimpleAudioManager))]
/// <summary>
/// Starts the boss fight and confines the player to the arena.
/// </summary>
public class BossFightArena : MonoBehaviour
{
    [SerializeField] private List<EnemyHealth> bossesHealth; // Boss list.
    [SerializeField] private List<GameObject> fakeBossObjects; // Objects used as stand-in bosses.
    [SerializeField] private List<Animator> fakeBossAnimators; // Animators for the stand-in bosses.
    [SerializeField] private List<GameObject> bossesObjects; // Boss objects.
    [Space(10)]

    [SerializeField] private ClosedDoor bossDoor;
    [SerializeField] private bool fadeOutDoor = false;
    [SerializeField] private MoveObjectToPos moveCameraToPos, moveCameraToPlayer;
    [Space(10)]
    [SerializeField] private List<GameObject> arenaLimits;
    [SerializeField] private bool disableArenaLimitOnBossFightEnd = true;

    [Header("Show off settings")]
    [SerializeField] private float showOffZoomTime = 2f;
    [SerializeField] private float showOffZoom = 3;
    [SerializeField] private float timeToTriggerShowOffAnimation = 3;
    [SerializeField] private float delayToStartFight = 3;
    [SerializeField] private float delayToShowFrame = 3;
    [SerializeField] private float frameDuration = 3;
    [SerializeField] private List<MMF_Player> showOffFeedbacks;
    [SerializeField] private PlayerAnimationName playerAnimationToPlay = PlayerAnimationName.PlayerUpMovement;

    [Header("Boss fight settings")]
    [SerializeField] private float bossFightZoom = 8;
    [SerializeField] private float bossFightZoomTime = 1;
    [SerializeField] private bool fixedCamera = false;
    //[SerializeField] private AudioClip bossMusic;
    [SerializeField] private float bossMusicVolume = 0.6f;

    [Header("End Boss fight settings")]
    [SerializeField] private float delayToEndFight = 5;

    [Header("Move Player")]
    [SerializeField] private bool movePlayerToPos = true;
    [SerializeField] private MoveObjectToPos movePlayerToPosScript;
    public PlayerInputHandler playerInputHandler;
    private Health playerHealth;

    [Header("A start pathfinding")]
    [SerializeField] private bool movePathFinding = false;
    [SerializeField] Transform astarPathTransform;
    [SerializeField] Transform astartDestinationTranform;
    [SerializeField] private bool changeCollisionDiameter = false;
    [SerializeField] private float collisionDiameter = 2f;

    [Space(20)]
    [SerializeField] private BossFightUiInfo bossFightUiInfo;

    private bool fightStarted = false;
    
    
    private CameraController cameraController;
    private CameraZoom cameraZoom;
    private StopWaveManager stopWaveManager;
    private MusicManager musicManager;
    private SpriteRenderer doorSprite;
    private SimpleAudioManager audioManager;
    private PlayerAnimation playerAnimation;

    public event Action OnStartBossFight, OnEndBossFight;
    //Achivment related
    public event Action OnAchievmentCheck;

    private void Awake()
    {
        cameraController = Try.FindObjectOfType<CameraController>();
        cameraZoom = Try.FindObjectOfType<CameraZoom>();
        if(bossFightUiInfo==null) bossFightUiInfo = Try.FindObjectOfType<BossFightUiInfo>();
        stopWaveManager = Try.FindObjectOfType<StopWaveManager>();
        musicManager = Try.FindObjectOfType<MusicManager>();
        audioManager = Try.GetComponent<SimpleAudioManager>(gameObject);

        if (playerInputHandler == null)
        {
            playerInputHandler = Try.FindObjectOfType<PlayerInputHandler>();
        }

        if (playerInputHandler != null)
        {
            playerAnimation = Try.GetComponent<PlayerAnimation>(playerInputHandler.gameObject);
            playerHealth = playerInputHandler.GetComponent<Health>();
        }
        else
        {
            Debug.LogError("BossFightArena nao encontrou PlayerInputHandler em " + gameObject.name, this);
        }

        if (fadeOutDoor)
        {
            doorSprite = bossDoor.gameObject.GetComponent<SpriteRenderer>();
        }

        // Populate the Animator list from the stand-in boss objects.
        fakeBossAnimators = fakeBossObjects
            .Select(fakeBoss => fakeBoss.GetComponent<Animator>())
            .ToList();
    }

    private void OnEnable()
    {
        BindRuntimeEvents_TechBooh();
    }

    private void OnDisable()
    {
        UnbindRuntimeEvents_TechBooh();
    }

    private void BindRuntimeEvents_TechBooh()
    {
        UnbindRuntimeEvents_TechBooh();

        if (bossDoor != null)
        {
            bossDoor.OnOpenDoor += HandleBossDoorOpened_TechBooh;
            bossDoor.OnOpenDoor += StopWave;
            bossDoor.OnBuyDoor += HandleBossDoorBought_TechBooh;
        }

        if (bossesHealth == null) return;
        foreach (EnemyHealth boss in bossesHealth)
        {
            if (boss != null) boss.OnZeroHealth += OnBossDefeated;
        }
    }

    private void UnbindRuntimeEvents_TechBooh()
    {
        if (bossDoor != null)
        {
            bossDoor.OnOpenDoor -= HandleBossDoorOpened_TechBooh;
            bossDoor.OnOpenDoor -= StopWave;
            bossDoor.OnBuyDoor -= HandleBossDoorBought_TechBooh;
        }

        if (bossesHealth == null) return;
        foreach (EnemyHealth boss in bossesHealth)
        {
            if (boss != null) boss.OnZeroHealth -= OnBossDefeated;
        }
    }

    private void HandleBossDoorOpened_TechBooh()
    {
        if (!fightStarted)
        {
            StartCoroutine(StartShowOff());
        }
    }

    private void HandleBossDoorBought_TechBooh()
    {
        if (playerInputHandler != null) playerInputHandler.SetCanMove(false);
        if (playerHealth != null) playerHealth.invulnerable = true;
    }

    public IEnumerator StartShowOff()
    {
        Debug.Log("ComeÃ§ou a boss fight  ");

        fightStarted = true;
        playerInputHandler.SetPauseInputBlocked(true);

        UpdateAstarPathfinding();

        musicManager.FadeOutMusicVolume(1f);

        foreach (var item in showOffFeedbacks)
        {
            item.PlayFeedbacks();
        }

        if (movePlayerToPos)
        {
            playerAnimation.UpdateAnimation = false; // Disable player animations.
            playerAnimation.PlayAnimation(playerAnimationToPlay);
            movePlayerToPosScript.StartMove(0);
        }

        cameraController.autoUpdatePos = false;
        moveCameraToPos.StartMove(0);
        cameraZoom.Zoom(showOffZoomTime, showOffZoom);
        Invoke(nameof(StartFight), delayToStartFight);

        yield return new WaitForSeconds(timeToTriggerShowOffAnimation);

        StartCoroutine(bossFightUiInfo.ShowBossFrame(delayToShowFrame, frameDuration));

        // Activate animations on all stand-in bosses.
        foreach (var animator in fakeBossAnimators)
        {
            animator.SetTrigger("Start");
        }

    }

    private void UpdateAstarPathfinding()
    {
        if (changeCollisionDiameter)
        {
            var astar = AstarPath.active;
            if (astar == null)
            {
                Debug.LogWarning("AstarPath nï¿½o encontrado na cena.");
            }
            var grid = astar.data.gridGraph;
            if (grid == null)
            {
                Debug.LogWarning("GridGraph nï¿½o encontrado no AstarPath.");
            }
            //grid.center = astarPathTransform.position;
            grid.collision.diameter = collisionDiameter;
            bossDoor.GetComponent<ScanPathFinder>().AsyncScan();
        }

        if (movePathFinding)
        {
            MovePathFinding();
        }
    }

    private void MovePathFinding()
    {
        var astar = AstarPath.active;
        if (astar == null)
        {
            Debug.LogWarning("AstarPath nï¿½o encontrado na cena.");
        }

        var grid = astar.data.gridGraph;
        if (grid == null)
        {
            Debug.LogWarning("GridGraph nï¿½o encontrado no AstarPath.");
        }

        grid.center = astartDestinationTranform.position;


        bossDoor.GetComponent<ScanPathFinder>().AsyncScan();
    }

    private void StartFight()
    {
        playerInputHandler.SetPauseInputBlocked(false);
        playerAnimation.UpdateAnimation = true;
        playerInputHandler.SetCanMove(true);
        playerHealth.invulnerable = false;

        if (fadeOutDoor) StartCoroutine(FadeDoor_Routine(Fade.fadeOut));
        //musicManager.PlayBossMusic(bossMusic);
        musicManager.SetBossMusicVolume(bossMusicVolume, true);

        cameraController.autoUpdatePos = !fixedCamera;

        foreach (var bossObject in bossesObjects)
        {
            bossObject.SetActive(true);
        }

        foreach (var fakeBossObject in fakeBossObjects)
        {
            fakeBossObject.SetActive(false);
        }

        moveCameraToPos.StartMove(1);
        cameraZoom.Zoom(bossFightZoomTime, bossFightZoom);

        foreach (var item in arenaLimits)
        {
            item.SetActive(true);
        }

        OnStartBossFight?.Invoke();
        StartCoroutine(bossFightUiInfo.ShowBossFightUI(1));

        audioManager.Play();
        StartCoroutine(DoubleCheckMusicVolume_Routine());
    }



    private void OnBossDefeated()
    {
        bossesHealth = bossesHealth.Where(boss => boss.currentHealth > 0).ToList();

        if (bossesHealth.Count == 0)
        {
            StartCoroutine(EndBossFightAfterDelay());
        }

        var enemies = FindObjectsByType<CountEnemies_Helper>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.gameObject.SetActive(false);
        }
    }

    public IEnumerator EndBossFightAfterDelay()
    {
        OnAchievmentCheck?.Invoke();
        fightStarted = false;
        StopMusic();
        yield return new WaitForSeconds(delayToEndFight);
        EndBossFight();
    }

    public void EndBossFight()
    {
        StartCoroutine(bossFightUiInfo.ResetUI(1));
        if (fadeOutDoor) StartCoroutine(FadeDoor_Routine(Fade.fadeIn));
       
        if (disableArenaLimitOnBossFightEnd)
        {
            foreach (var item in arenaLimits)
            {
                item.SetActive(false);
            }
        }

        cameraZoom.Zoom(1f, 10f, 1);
        cameraController.SetAutoUpdatePos(true);
        moveCameraToPlayer.StartMove(0);
        OnEndBossFight?.Invoke();
    }

    private void StopWave()
    {
        StartCoroutine(stopWaveManager.StopWave(3));
    }

    private void StopMusic()
    {
        audioManager.PlayEnd();
    }

    /// <summary>
    /// Refresh BossHealth when the boss object changes, such as during Gael's transformation.
    /// This method supports only a single boss.
    /// </summary>
    public void ChangeBossHealth(EnemyHealth enemyHealth, int index = 0)
    {
        if (index < bossesHealth.Count && bossesHealth[index] != null)
        {
            bossesHealth[index].OnZeroHealth -= OnBossDefeated;
        }

        if(bossesHealth.Count > 1)
        {
            Debug.LogWarning("ChangeBossHealth sï¿½ deve ser chamado quando tem apenas um boss");
            return;
        }
        else if(bossesHealth.Count == 0)
        {
            bossesHealth.Add(enemyHealth);
        }
        this.bossesHealth[index] = enemyHealth;
        bossesHealth[index].OnZeroHealth += OnBossDefeated;
    }

    private IEnumerator FadeDoor_Routine(Fade fade)
    {
        if (doorSprite == null)
        {
            yield break;
        }

        float duration = 1f;
        float startAlpha = doorSprite.color.a;
        float endAlpha = (fade == Fade.fadeOut) ? 0.2f : 1f;
        float elapsedTime = 0f;

        Color spriteColor = doorSprite.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            doorSprite.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, newAlpha);
            yield return null;
        }

        doorSprite.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, endAlpha);
    }

    /// <summary>
    /// Workaround to suppress this music during boss fights.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DoubleCheckMusicVolume_Routine()
    {
        while (fightStarted)
        {
            if (musicManager.GetMusicVolume() > 0.1f)
            {
                musicManager.FadeOutMusicVolume(1f);
            }

            if (musicManager.GetBossMusicVolume() < bossMusicVolume - 0.1f)
            {
                musicManager.FadeInBossMusicVolume(1f);
            }

            yield return new WaitForSeconds(1f);
        }
    }
    public void ResetAchievmentCheck()
    {
        OnAchievmentCheck = null;
    }

    private enum Fade { fadeIn, fadeOut }
}
