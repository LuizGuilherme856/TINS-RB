using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameIntroManager : MonoBehaviour
{

    public ClosedDoor spawnDoor, bossDoor;
    public MinionSpawner minionSpawner;
    public Health playerHealth;
    public ManageBloodRitual manageBloodRitual;
    public RuneRitualManager runeRitualManager;
    public GameObject ritualText, openDoorText, buyRuneText;
    public CinematicTextSequence cinematicTextSequence;


    [Space(15)]
    [Header("Intro Gear")]
    public List<GunData> introGuns;
    public int introPoints = 1000;
    public int introHp = 10;
    public List<RuneData> introUpgrades;
    public GunInventory gunInventory;

    [Header("Fade Out")]
    public Image fadeImage; // Assign the fade image here.
    public float fadeDuration = 1.5f; // Fade duration in seconds.
    public float sceneLoadDelay = 2f; // Delay before loading the scene.


    private PlayerInputHandler playerInputHandler => playerHealth.gameObject.GetComponent<PlayerInputHandler>();
    private RuneInventory runeInventory => AssetFinder.GetRuneInventory();
    private RunDataManager runDataManager;
    private RunData runData;
    private GlobalStats globalStats => AssetFinder.GetGlobalStats();

    private void Start()
    {
        playerInputHandler.SetCanMove(false); // Disable player movement during the introduction.

        globalStats.ForCurrentRun.ResetValues();

        runDataManager = FindAnyObjectByType<RunDataManager>();
        runDataManager.ResetRunData();
        runData = runDataManager.GetRunData();
        
        playerHealth.SetCurrentHealth(introHp);
        // TechBooh: SetCurrentHealth updates data without raising the UI event.
        // Refresh explicitly so the player's heart display appears in the tutorial.
        playerHealth.UpdateHealth();
        runData.currentHp = introHp;
        runData.currentpoints = introPoints;

        gunInventory.ResetGuns();
        gunInventory.RemoveCurrentWeapon();

        foreach (var gun in introGuns)
        {
            gunInventory.AddGunToInventory(gun);
        }

        runeInventory.ResetUpgrades();
        foreach (var upgrade in introUpgrades)
        {
            runeInventory.AddUpgrade(upgrade);
        }

    }

    private void OnEnable()
    {
        spawnDoor.OnOpenDoor += SpawnEnemies;
        bossDoor.OnOpenDoor += RemovePlayerInvulnerability;
        manageBloodRitual.OnCompleted += RitualCompleted;
        runeRitualManager.OnBuyRune += RuneBought;
        playerHealth.OnZeroHealth += GameOver;
        cinematicTextSequence.OnSequenceEnd += () => playerInputHandler.SetCanMove(true);
    }

    private void RitualCompleted()
    {
        minionSpawner.StopSpawn();
        //bossDoor.canOpen = true;
        ritualText.SetActive(false);
        buyRuneText.SetActive(true);
        //openDoorText.SetActive(true);
    }

    private void RuneBought()
    {
        bossDoor.canOpen = true;
        
        buyRuneText.SetActive(false);
        openDoorText.SetActive(true);

    }

    private void SpawnEnemies()
    {
        StartCoroutine(minionSpawner.SpawnEnemies());
    }

    private void RemovePlayerInvulnerability()
    {
        playerHealth.SetIsTutorial(false);
    }

    private void GameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.gameObject.SetActive(true);

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = color;
                yield return null;
            }
            color.a = 1f;
            fadeImage.color = color;
        }

        yield return new WaitForSeconds(sceneLoadDelay);

        SceneManager.LoadScene("The Void - HUB");
    }

}
