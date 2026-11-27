using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TINS.Campaign
{
    public static class CampaignCombatRules
    {
        public static int RoundsPerShot(GunData gun)
        {
            int extra = Mathf.RoundToInt(CampaignMissionRuntime.Active?.SkillEffect(CampaignSkillEffectType.ScatterShot).y ?? 0f);
            return Mathf.Max(1, gun.GetAmmoConsumedPerShot() + extra);
        }

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
        [SerializeField] private CampaignDeathScreen deathScreenPrefab;
        [SerializeField, Min(0f)] private float resultReturnDelay = 3f;
        [SerializeField, Range(0f, 1f)] private float soulsKeptOnFailure = .7f;
        [SerializeField, Range(0f, 1f)] private float soulDropChance = 1f;
        [SerializeField, Min(1)] private int soulsPerDrop = 1;
        [SerializeField] private CampaignFlashHUD flashHud;
        [SerializeField] private CampaignTowerHealthHUD towerHealthHud;
        [Header("Purification Burst")]
        [SerializeField] private AudioClip purificationBurstSound;
        [SerializeField, Min(0.1f)] private float purificationBurstDuration = 0.9f;
        [SerializeField, Min(0f)] private float corpseThrowDistance = 5f;
        private readonly List<GameObject> corpses = new();
        private readonly Dictionary<CampaignSkillEffectType, Vector2> skillEffects = new();
        private CampaignDefenseUpgrades defenses;
        public float SuperCriticalChance { get; private set; }
        public BlueCrystalInventory CrystalCollector => resolved ? null : soulInventory;
        public Vector2 SkillEffect(CampaignSkillEffectType effect) =>
            skillEffects.TryGetValue(effect, out Vector2 value) ? value : Vector2.zero;
        public void DeployDashDecoy(Vector3 position)
        {
            if (!resolved) defenses?.DeployDecoy(position);
        }
        public void DeployFlashTurret(Vector3 position)
        {
            if (!resolved) defenses?.DeployTurret(position);
        }

        public void OnEliteDefeated()
        {
            if (!resolved)
                playerCharges?.RestoreUses(Mathf.RoundToInt(SkillEffect(CampaignSkillEffectType.EliteFlashRecovery).x));
        }
        private BeamMeleeUsesManager playerCharges;
        private CampaignProfile profile;
        private CampaignPlanet planet;
        private CampaignDarknessBands darkness;
        private CampaignResourceCaches resourceCaches;
        private int initialWave;
        private int soulsCollected;
        private int lastInventoryCount;
        private bool resolved;
        private float? timeScaleBeforeDefeat;
        private MMTimeManager defeatTimeManager;

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

        public void TrackCorpse(GameObject corpse)
        {
            corpses.RemoveAll(existing => existing == null);
            corpses.Add(corpse);
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
            bool profileChanged = profile.EnsurePurchaseLedger(catalog.skills);
            if (profile.lastVisitedPlanetId != planet.id)
            {
                profile.lastVisitedPlanetId = planet.id;
                profileChanged = true;
            }
            if (profileChanged) profile.Save();

            planet.ValidateRadii();
            // Set the win threshold before Radius, whose setter can invoke the win event.
            lightReveal.WinRadius = planet.missionTargetRadius;
            lightReveal.Radius = planet.initialLightRadius;
            darkness = FindFirstObjectByType<CampaignDarknessBands>();
            darkness?.ConfigurePlanet(planet);
            if (darkness != null && planet.resourceCacheCount > 0 && planet.resourceCacheSprite != null &&
                catalog.depletedCacheSprite != null && !string.IsNullOrWhiteSpace(planet.rewardResourceId))
                resourceCaches = new CampaignResourceCaches(transform, lightReveal.transform.position, planet, catalog.depletedCacheSprite);
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
            playerCharges = playerHealth.GetComponent<BeamMeleeUsesManager>();
            ApplyCampaignSkills();
            flashHud?.Bind(profile.HasFlash ? playerCharges : null);
            towerHealthHud?.Bind(tower);
            playerHealth.GetComponent<EnsureStartingPistol>()?.Equip(catalog, profile);
            playerHealth.ResetHealth();
            deposit?.ConfigureGrowth(planet.lightGrowthPerSoul);
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
            {
                Finish(true, "Missão concluída.");
                return;
            }
            if (resourceCaches != null && darkness != null)
                resourceCaches.CollectNearby(playerHealth.transform.position, darkness.BreuRadius);
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
            defenses?.StopDefenses();
            if (soulInventory != null) soulInventory.OnCountChanged -= TrackCollectedSouls;
            if (tower != null && tower.OnTowerDeath != null) tower.OnTowerDeath.RemoveListener(OnDefenseCoreDestroyed);

            int returnedSouls = success ? soulsCollected : Mathf.CeilToInt(soulsCollected * soulsKeptOnFailure);
            if (success) returnedSouls += planet.victorySouls;
            bool firstExpedition = !profile.firstExpeditionSettled;
            if (firstExpedition)
            {
                int unlockCost = catalog.FlashSkill?.CostForRank(0) ?? 0;
                returnedSouls = Mathf.Max(returnedSouls, unlockCost - profile.souls);
                profile.firstExpeditionSettled = true;
            }
            profile.souls = Mathf.Max(0, profile.souls + returnedSouls);
            int returnedMaterials = resourceCaches?.Collected ?? 0;
            profile.AddResource(planet.rewardResourceId, returnedMaterials);
            if (success) profile.CompleteMission(planet);
            profile.Save();

            EnemySpawnManager spawner = FindFirstObjectByType<EnemySpawnManager>();
            spawner?.StopAutomaticSpawning();
            var playerInput = playerHealth.GetComponent<PlayerInputHandler>();
            playerInput?.SetCanMove(false);

            bool flashAvailable = firstExpedition && !profile.HasFlash;
            if (!success && deathScreenPrefab != null)
            {
                playerInput?.SetPauseInputBlocked(true);
                PauseForDefeat();
                Instantiate(deathScreenPrefab).Present(planet.displayName, message, returnedSouls,
                    ResourceRewardText(returnedMaterials), flashAvailable, LeaveDefeatScreen);
                return;
            }

            if (missionLabel != null) missionLabel.text = success ? "MISSÃO CONCLUÍDA" : "MISSÃO ENCERRADA";
            if (objectiveLabel != null)
                objectiveLabel.text = $"{planet.displayName}\n{message}\n{CampaignCatalog.ResourceIcon("almas")}+{returnedSouls} almas" +
                    ResourceRewardText(returnedMaterials + (success ? planet.rewardPerMission : 0), "\n") +
                    (flashAvailable ? "\nFlash de Luz disponível na Base." : "");
            StartCoroutine(ReturnToBase(success, spawner));
        }

        private IEnumerator ReturnToBase(bool success, EnemySpawnManager spawner)
        {
            float startTime = Time.unscaledTime;
            if (success) yield return PlayPurificationBurst(spawner);
            float remaining = resultReturnDelay - (Time.unscaledTime - startTime);
            if (remaining > 0f) yield return new WaitForSecondsRealtime(remaining);
            SceneManager.LoadScene(CampaignTravel.BaseScene);
        }

        private void LeaveDefeatScreen()
        {
            RestoreDefeatTimeScale();
            SceneManager.LoadScene(CampaignTravel.BaseScene);
        }

        private void PauseForDefeat()
        {
            timeScaleBeforeDefeat = Time.timeScale;
            // The feedback manager can keep changing global time after a death hit-stop.
            var manager = FindFirstObjectByType<MMTimeManager>();
            if (manager != null && manager.isActiveAndEnabled)
            {
                defeatTimeManager = manager;
                timeScaleBeforeDefeat = manager.NormalTimeScale;
                manager.enabled = false;
            }
            Time.timeScale = 0f;
        }

        private void RestoreDefeatTimeScale()
        {
            if (!timeScaleBeforeDefeat.HasValue) return;
            Time.timeScale = timeScaleBeforeDefeat.Value;
            timeScaleBeforeDefeat = null;
            if (defeatTimeManager != null)
            {
                defeatTimeManager.ResetTimeScale();
                defeatTimeManager.enabled = true;
                defeatTimeManager = null;
            }
        }

        private IEnumerator PlayPurificationBurst(EnemySpawnManager spawner)
        {
            // The final delivery already paid out and reached the target radius.
            deposit?.StopAllCoroutines();
            Vector2 origin = lightReveal.transform.position;
            float arenaRadius = Mathf.Max(lightReveal.Radius, spawner != null ? spawner.FarthestSpawnDistance(origin) : 0f);
            Camera camera = Camera.main;
            if (camera != null && camera.orthographic)
            {
                float halfDiagonal = camera.orthographicSize * Mathf.Sqrt(1f + camera.aspect * camera.aspect);
                arenaRadius = Mathf.Max(arenaRadius, Vector2.Distance(origin, camera.transform.position) + halfDiagonal);
            }

            foreach (EnemyHealth enemy in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
                if (enemy != null && enemy.isActiveAndEnabled) enemy.DefeatByPurification();

            var throws = new List<(Transform body, Vector3 start, Vector2 direction, float distance, float angle)>();
            foreach (GameObject corpse in corpses)
            {
                if (corpse == null || !corpse.activeInHierarchy) continue;
                Vector2 offset = (Vector2)corpse.transform.position - origin;
                Vector2 direction = offset.sqrMagnitude > 0.001f ? offset.normalized : Vector2.right;
                float distance = corpseThrowDistance * Random.Range(0.85f, 1.3f);
                foreach (Rigidbody2D body in corpse.GetComponentsInChildren<Rigidbody2D>())
                    body.simulated = false;
                arenaRadius = Mathf.Max(arenaRadius, offset.magnitude + distance);
                throws.Add((corpse.transform, corpse.transform.position, direction, distance, Random.Range(-160f, 160f)));
            }

            if (purificationBurstSound != null)
            {
                AudioSource audio = gameObject.AddComponent<AudioSource>();
                audio.PlayOneShot(purificationBurstSound, 0.75f);
            }
            FindFirstObjectByType<ControlCameraShake>()?.TriggerCameraShake(75f, 2.4f);
            float initialRadius = lightReveal.Radius;
            arenaRadius += 2f; // Keep the final shockwave beyond the visible arena.
            float elapsed = 0f;
            while (true)
            {
                float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.1f, purificationBurstDuration));
                float travel = 1f - Mathf.Pow(1f - progress, 2f);
                float radius = Mathf.Lerp(initialRadius, arenaRadius, travel);
                lightReveal.Radius = radius;
                darkness?.SetPurificationWave(radius, 1.35f, 0.9f * (1f - progress));
                foreach (var launched in throws)
                    if (launched.body != null)
                        launched.body.SetPositionAndRotation(launched.start + (Vector3)(launched.direction * launched.distance * travel),
                            Quaternion.Euler(0f, 0f, launched.angle * travel));
                if (progress >= 1f) break;
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
            darkness?.SetPurificationWave(0f, 0f, 0f);
        }

        private void ApplyCampaignSkills()
        {
            if (catalog != null && catalog.skills != null)
            {
                foreach (CampaignSkill skill in catalog.skills)
                {
                    if (skill == null) continue;
                    int rank = profile.Rank(skill.id);
                    Vector2 effect = skill.EffectAtRank(rank);
                    if (rank <= 0 || effect == Vector2.zero) continue;
                    skillEffects[skill.effectType] = SkillEffect(skill.effectType) + effect;
                    if (skill.effectType == CampaignSkillEffectType.GunCritical && rank >= skill.maximumRank)
                        SuperCriticalChance = Mathf.Max(SuperCriticalChance, Mathf.Clamp01(skill.secondaryEffectPerRank));
                    if (skill.effectType == CampaignSkillEffectType.MonkeyDecoy ||
                        skill.effectType == CampaignSkillEffectType.BladeTurret ||
                        skill.effectType == CampaignSkillEffectType.GuardianBlades)
                    {
                        if (defenses == null) defenses = gameObject.AddComponent<CampaignDefenseUpgrades>();
                        defenses.Configure(tower, skill, effect);
                    }
                }
            }

            Vector2 flashAreaEffect = SkillEffect(CampaignSkillEffectType.FlashArea);
            BeamMeleeAttack flash = playerHealth.GetComponent<BeamMeleeAttack>();
            flash?.SetCampaignDurationMultiplier(1f + SkillEffect(CampaignSkillEffectType.FlashDuration).x);
            flash?.SetCampaignArea(flashAreaEffect.x, flashAreaEffect.y);

            if (playerCharges != null)
            {
                playerCharges.enabled = profile.HasFlash;
                if (profile.HasFlash)
                {
                    playerCharges.ConfigureMaximumUses(Mathf.Max(0, Mathf.RoundToInt(SkillEffect(CampaignSkillEffectType.FlashCapacity).x)));
                    float rechargeMultiplier = 1f - Mathf.Clamp(SkillEffect(CampaignSkillEffectType.FlashRecharge).x, 0f, 0.9f);
                    playerCharges.ConfigureRechargeDuration(FlashData.Shared.rechargeDuration * rechargeMultiplier);
                    playerCharges.RefillUses();
                }
            }

            playerHealth.GetComponent<PlayerMovement>()?.SetCampaignSpeedMultiplier(1f + SkillEffect(CampaignSkillEffectType.MovementSpeed).x);
            if (SkillEffect(CampaignSkillEffectType.ExtraDash).x > 0f)
                foreach (MeleeManager action in playerHealth.GetComponentsInChildren<MeleeManager>())
                    action.RefillCampaignDash();

            GlobalStats stats = AssetFinder.GetGlobalStats();
            if (stats != null) stats.AddToPlayerInitialHealth += Mathf.RoundToInt(SkillEffect(CampaignSkillEffectType.PlayerHealth).x);
        }

        private string ResourceRewardText(int amount, string prefix = "") => amount > 0
            ? $"{prefix}{CampaignCatalog.ResourceIcon(planet.rewardResourceId)}+{amount} {planet.rewardResourceName}"
            : string.Empty;

        private void RefreshLabels()
        {
            if (resolved || planet == null) return;
            if (missionLabel != null) missionLabel.text = $"MISSÃO  ·  {planet.displayName}";
            if (objectiveLabel != null)
            {
                float progress = Mathf.Clamp01(lightReveal.Radius / Mathf.Max(1f, planet.missionTargetRadius));
                objectiveLabel.text = $"Defenda o centro\nLUZ {progress:P0}   ·   ALMAS {soulInventory?.Count ?? 0}" +
                    ResourceRewardText(resourceCaches?.Collected ?? 0, "\n");
            }
        }

        private void OnDestroy()
        {
            RestoreDefeatTimeScale();
            if (soulInventory != null) soulInventory.OnCountChanged -= TrackCollectedSouls;
            if (tower != null && tower.OnTowerDeath != null) tower.OnTowerDeath.RemoveListener(OnDefenseCoreDestroyed);
            if (Active == this) Active = null;
            CampaignCombatRules.SetMissionActive(false);
        }
    }
}
