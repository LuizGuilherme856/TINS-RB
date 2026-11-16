using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TINS.Campaign
{
    public static class CampaignCombatRules
    {
        public static bool IsCampaignMission { get; private set; }
        public static float EnemySpeedMultiplier { get; private set; } = 1f;
        public static float EnemyHealthMultiplier { get; private set; } = 1.5f;
        public static void SetMissionActive(bool active, float enemySpeedMultiplier = 1f, float enemyHealthMultiplier = 1.5f)
        {
            IsCampaignMission = active;
            EnemySpeedMultiplier = active ? Mathf.Max(0.1f, enemySpeedMultiplier) : 1f;
            EnemyHealthMultiplier = active ? Mathf.Max(0.01f, enemyHealthMultiplier) : 1f;
        }
    }

    [DefaultExecutionOrder(900)]
    public sealed class CampaignMissionRuntime : MonoBehaviour
    {
        [SerializeField] private CampaignCatalog catalog;
        [SerializeField] private TowerController tower;
        [SerializeField] private TowerCrystalDeposit deposit;
        [SerializeField] private PurificationReveal lightReveal;
        [SerializeField] private Health playerHealth;
        [SerializeField] private BlueCrystalInventory soulInventory;
        [SerializeField] private TMP_Text missionLabel;
        [SerializeField] private TMP_Text objectiveLabel;
        [SerializeField, Min(0f)] private float resultReturnDelay = 3f;
        [SerializeField, Range(0f, 1f)] private float soulsKeptOnFailure = .7f;
        [SerializeField, Range(0f, 1f)] private float soulDropChance = .65f;
        [SerializeField, Min(1)] private int soulsPerDrop = 1;
        [SerializeField] private CampaignFlashHUD flashHud;
        [SerializeField] private CampaignTowerHealthHUD towerHealthHud;
        private BeamMeleeUsesManager playerCharges;
        private CampaignProfile profile;
        private CampaignPlanet planet;
        private int initialWave;
        private int soulsCollected;
        private int lastInventoryCount;
        private bool resolved;

        public static CampaignMissionRuntime Active { get; private set; }
        public Transform DefenseCore => tower != null ? tower.transform : null;
        public int Wave => Mathf.Max(1, 1 + Mathf.FloorToInt((lightReveal != null ? lightReveal.Radius : 0f) / 3f));
        public CampaignPlanet Planet => planet;
        public int SoulsCollected => soulsCollected;
        public bool IsResolved => resolved;

        public static void TryDropSoul(Vector3 position)
        {
            CampaignMissionRuntime active = Active;
            if (active == null || active.resolved || Random.value > active.soulDropChance) return;
            BlueCrystalPickup.Spawn(position, active.soulsPerDrop);
        }

        private void Awake()
        {
            Active = this;
            CampaignCombatRules.SetMissionActive(true);
        }

        private void Start()
        {
            if (catalog == null) catalog = Resources.Load<CampaignCatalog>("CampaignCatalog");
            profile = CampaignProfile.Load();
            string planetId = CampaignTravel.ConsumeMission();
            if (string.IsNullOrWhiteSpace(planetId)) planetId = catalog != null && catalog.planets != null && catalog.planets.Length > 0 ? catalog.planets[0].id : "velaria";
            planet = catalog != null ? catalog.GetPlanet(planetId) : null;
            if (planet == null || tower == null || lightReveal == null || playerHealth == null)
            {
                Debug.LogError("Campaign mission references or selected planet are missing.", this);
                CampaignCombatRules.SetMissionActive(false);
                enabled = false;
                return;
            }
            CampaignMissionEnemies settings = planet.enemySettings;
            initialWave = Wave;
            CampaignCombatRules.SetMissionActive(true,
                settings != null ? settings.movementSpeedMultiplier : planet.enemySpeedMultiplier,
                settings != null ? settings.healthMultiplier : 1.5f);

            if (soulInventory != null)
            {
                lastInventoryCount = soulInventory.Count;
                soulInventory.OnCountChanged += TrackCollectedSouls;
            }
            AssetFinder.GetGlobalStats()?.ResetValues();
            AssetFinder.GetRuneInventory()?.ResetUpgrades();
            FindFirstObjectByType<SkillManager>(FindObjectsInactive.Include)?.ExecuteSkillUpgrades();
            playerCharges = playerHealth.GetComponent<BeamMeleeUsesManager>();
            ApplyCampaignSkills();
            flashHud?.Bind(playerCharges);
            towerHealthHud?.Bind(tower);
            playerHealth.GetComponent<EnsureStartingPistol>()?.Equip(catalog, profile);
            playerHealth.ResetHealth();
            lightReveal.WinRadius = planet.missionTargetRadius;
            lightReveal.enemySlowDownPercentage = Mathf.Clamp(lightReveal.enemySlowDownPercentage, 0f, 100f);
            if (tower.OnTowerDeath != null) tower.OnTowerDeath.AddListener(OnDefenseCoreDestroyed);
            EnemyHealth[] existingEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            foreach (EnemyHealth enemy in existingEnemies)
                if (enemy != null)
                {
                    enemy.BeginCampaignMissionState();
                    enemy.InitializeCampaignHealth();
                }
            EnemyAI[] enemyAgents = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            foreach (EnemyAI enemy in enemyAgents)
                if (enemy != null) enemy.SetTarget(tower.transform);
            MoveTowardsPlayer[] simpleChasers = FindObjectsByType<MoveTowardsPlayer>(FindObjectsSortMode.None);
            foreach (MoveTowardsPlayer enemy in simpleChasers)
                if (enemy != null) enemy.SetTarget(tower.transform);

            FindFirstObjectByType<EnemySpawnManager>()?.StartCampaignSpawning(settings);
            RefreshLabels();
        }

        public float GetEnemyHealthWaveMultiplier()
        {
            CampaignMissionEnemies settings = planet?.enemySettings;
            if (settings == null) return 1f + 0.2f * (Wave - 1);
            return 1f + Mathf.Max(0f, settings.extraHealthPerWave) * Mathf.Max(0, Wave - initialWave);
        }

        private void Update()
        {
            if (resolved) return;
            if (playerHealth != null && playerHealth.GetCurrentHealth() <= 0f)
            {
                Finish(false, "O jogador foi derrotado.");
                return;
            }
            if (tower != null && tower.currentLife <= 0)
            {
                Finish(false, "O centro de luz foi destruído.");
                return;
            }
            if (lightReveal != null && planet != null && lightReveal.Radius >= planet.missionTargetRadius)
                Finish(true, "Missão concluída.");
            RefreshLabels();
        }

        private void TrackCollectedSouls(int currentCount)
        {
            if (currentCount > lastInventoryCount) soulsCollected += currentCount - lastInventoryCount;
            lastInventoryCount = currentCount;
        }

        private void OnDefenseCoreDestroyed() => Finish(false, "O centro de luz foi destruído.");

        private void Finish(bool success, string message)
        {
            if (resolved) return;
            resolved = true;
            if (soulInventory != null) soulInventory.OnCountChanged -= TrackCollectedSouls;
            if (tower != null && tower.OnTowerDeath != null) tower.OnTowerDeath.RemoveListener(OnDefenseCoreDestroyed);

            int returnedSouls = success ? soulsCollected : Mathf.FloorToInt(soulsCollected * soulsKeptOnFailure);
            profile.souls = Mathf.Max(0, profile.souls + returnedSouls);
            if (success) profile.CompleteMission(planet);
            profile.Save();

            FindFirstObjectByType<EnemySpawnManager>()?.StopAutomaticSpawning();
            if (success)
            {
                foreach (EnemyHealth enemy in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
                    if (enemy != null && enemy.currentHealth > 0f) enemy.InstaDieWithNoCorpse();
            }
            playerHealth.GetComponent<PlayerInputHandler>()?.SetCanMove(false);

            if (missionLabel != null) missionLabel.text = success ? "MISSÃO CONCLUÍDA" : "MISSÃO ENCERRADA";
            if (objectiveLabel != null)
                objectiveLabel.text = $"{planet.displayName}\n{message}\n+{returnedSouls} almas" +
                    (success ? $"\n+{planet.rewardPerMission} {planet.rewardResourceName}" : "");
            StartCoroutine(ReturnToBase());
        }

        private IEnumerator ReturnToBase()
        {
            yield return new WaitForSecondsRealtime(resultReturnDelay);
            SceneManager.LoadScene(CampaignTravel.BaseScene);
        }

        private void ApplyCampaignSkills()
        {
            float flashDuration = 1f + 0.05f * profile.Rank("flash-duration");
            playerHealth.GetComponent<BeamMeleeAttack>()?.SetCampaignDurationMultiplier(flashDuration);

            if (playerCharges != null)
            {
                playerCharges.ConfigureMaximumUses(profile.Rank("flash-capacity"));
                float rechargeMultiplier = 1f - 0.1f * profile.Rank("flash-recharge");
                playerCharges.ConfigureRechargeDuration(playerCharges.RechargeDurationPerCharge * rechargeMultiplier);
                playerCharges.RefillUses();
            }

            playerHealth.GetComponent<PlayerMovement>()?.SetCampaignSpeedMultiplier(1f + 0.05f * profile.Rank("speed"));
            int bonusHealth = profile.Rank("solar-vigor");
            GlobalStats stats = AssetFinder.GetGlobalStats();
            if (stats != null) stats.AddToPlayerInitialHealth += bonusHealth;
        }

        private void FixedUpdate()
        {
            if (resolved || tower == null || tower.currentLife <= 0) return;
            if (tower.timeSinceLastHit <= tower.immunityAfterDamage) return;
            foreach (Collider2D hit in Physics2D.OverlapCircleAll(tower.transform.position, 1.1f))
            {
                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
                if (enemy != null && enemy.currentHealth > 0 && !enemy.IsCampaignFlashStunned)
                {
                    tower.TryTakeDamageFromEnemy();
                    break;
                }
            }
        }

        private void RefreshLabels()
        {
            if (resolved || planet == null) return;
            if (missionLabel != null) missionLabel.text = $"MISSÃO  ·  {planet.displayName}";
            if (objectiveLabel != null)
            {
                float progress = Mathf.Clamp01(lightReveal.Radius / Mathf.Max(1f, planet.missionTargetRadius));
                objectiveLabel.text = $"Defenda o centro\nLUZ {progress:P0}   ·   ALMAS {soulInventory?.Count ?? 0}";
            }
        }

        private void OnDestroy()
        {
            if (soulInventory != null) soulInventory.OnCountChanged -= TrackCollectedSouls;
            if (tower != null && tower.OnTowerDeath != null) tower.OnTowerDeath.RemoveListener(OnDefenseCoreDestroyed);
            if (Active == this) Active = null;
            CampaignCombatRules.SetMissionActive(false);
        }
    }
}
