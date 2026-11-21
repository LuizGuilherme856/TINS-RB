using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MoreMountains.Tools;
using TINS.Campaign;

public class EnemyHealth : MonoBehaviour
{

    [field: SerializeField] public EnemyData enemyData { get; private set; }
    
    public bool showDebug;

    [field: SerializeField] public float maxHealth { get; protected set; } //must be float because the bullet damage is float
    [field: SerializeField] public float currentHealth { get; protected set; }
    public float invulnerableTimer { get; protected set; }
    [Header("Melee protection")]
    [Tooltip("Starts each spawn protected. The configured number of tower-light exposures removes this protection.")]
    [SerializeField] private bool startsInvulnerableUntilPlayerMelee;
    [Header("Purification exposure")]
    [Tooltip("Number of times the enemy must enter the tower's light before becoming vulnerable.")]
    [Min(1)] public int illuminationsRequiredToBecomeVulnerable = 1;
    public int IlluminationCount { get; private set; }
    public bool IsProtectedUntilPlayerMelee { get;  set; }
    public event Action OnMeleeProtectionBroken;

    public void SetIlluminationsRequiredToBecomeVulnerable(int count)
    {
        illuminationsRequiredToBecomeVulnerable = Mathf.Max(1, count);
    }
    private PlayerMeleeAttackSource protectionBreakingAttack;
    private uint protectionBreakingAttackId;
    private float damagePerSecond = 0f, damagePerSecTimer = 1f;

    private DamageType damageTypePerSecond;


    [Header(" other")]
    [SerializeField] private bool forceDoubleCheckWave = false; //Workaround for Alkarion incorrectly reporting a starting wave of 41.
    [SerializeField] private RunData runData;
    [SerializeField] private GlobalStats globalStats;
    /// <summary>
    /// Some objects use enemy health without EnemyDeath or a spawned corpse.
    /// </summary>
    [SerializeField] private bool useEnemyDeath = true;

    private EnemyDeath enemyDeath;
    private Vector3 lastHitMomemtum; //Momentum of the last projectile that hit the enemy: velocity times mass.
    private DamageType lastDamageType;
    private bool lastDamageWasMelee = false;
    private PointManager pointManager;
    private HandleAttackOnEnemy handleAttackOnEnemy;
    private HordeManagerCommunicator hordeManagerCommunicator;
    /// <summary>
    /// Prevent awarding death points twice.
    /// </summary>
    private bool hasGivenDeathPoints = false; 
    private bool campaignFlashStunned;
    private float campaignFlashStunTimer;
    private float campaignRepulsionUntil;
    public bool IsCampaignFlashRepelled => Time.time < campaignRepulsionUntil;
    private bool campaignHealthScaleApplied;
    private bool campaignElite;
    public void SetCampaignElite(bool value) => campaignElite = value;
    public bool IsCampaignFlashStunned => campaignFlashStunned && campaignFlashStunTimer > 0f;

    public virtual event Action OnHealthChange, OnZeroHealth, OnTakeDamage,OnInstaDieWithNoCorpse;
    //OnInstaDieWithNoCorpse kills without spawning a corpse, used by bombers and jumping enemies.

    private void Update()
    {
        if (invulnerableTimer > 0) invulnerableTimer -= Time.deltaTime;
        if (campaignFlashStunTimer > 0f)
        {
            campaignFlashStunTimer -= Time.deltaTime;
            if (campaignFlashStunTimer <= 0f) campaignFlashStunned = false;
        }

        CheckContinuousDamage();

    }
    protected  void Awake()
    {

        
        //base.Awake();
        invulnerableTimer = 0;
        enemyDeath = GetComponent<EnemyDeath>();
        pointManager = GameObject.FindGameObjectWithTag("PointManager")?.GetComponent<PointManager>();
        handleAttackOnEnemy = GetComponent<HandleAttackOnEnemy>();

        hordeManagerCommunicator = GetComponent<HordeManagerCommunicator>();

        if (globalStats == null) globalStats = AssetFinder.GetGlobalStats();

        if(runData == null)
        {
            Debug.Log($"RunData ta nulo em {gameObject.name}. Procurando pelo AssetFinder");
            runData = AssetFinder.GetRunData();
        }
    }

    protected void OnEnable()
    {
        campaignFlashStunned = false;
        campaignFlashStunTimer = 0f;
        campaignRepulsionUntil = 0f;
        campaignHealthScaleApplied = false;
        IsProtectedUntilPlayerMelee = startsInvulnerableUntilPlayerMelee;
        IlluminationCount = 0;
        protectionBreakingAttack = null;
        protectionBreakingAttackId = 0;
        damagePerSecond = 0f;
        damagePerSecTimer = 1f;
        transform.rotation = Quaternion.identity; //Reset rotation so pooled enemies do not respawn tilted.

        hasGivenDeathPoints = false;

        maxHealth = enemyData.health;
        if (CampaignCombatRules.IsCampaignMission) BeginCampaignMissionState();

        if (CampaignCombatRules.IsCampaignMission)
        {
            InitializeCampaignHealth();
            return;
        }

        if (runData.survivalMode)
        {
            UpdateHealth_SurvivalMode();
        }
        else
        {
            UpdateHealth();
        }
    }

    /// <summary>
    /// Scale enemy health with elapsed time.
    /// </summary>
    public void UpdateHealth_SurvivalMode()
    {
        //0.2491*X + 1.139
        float t = runData.timeElapsed;

        if (enemyData.healthIncreasesEveryWave)
        {
            currentHealth = maxHealth * 0.2491f * t + 1.139f;
        }
        else
        {
            currentHealth = maxHealth;
        }

        if (enemyData.increaseHealthOnRainWaves) SetNewHealthForRainWaves(Mathf.FloorToInt(t/60));
        ApplyCampaignHealthScale();
    }

    /// <summary>
    /// Scale enemy health with the current wave.
    /// </summary>
    public void UpdateHealth()
    {
        float healthIncreaseCoefficient = 0.2f;
        try { healthIncreaseCoefficient = hordeManagerCommunicator.GetHealthIncreaseCoefficient(); }
        catch { Debug.LogError("Erro ao tentar aumentar a vida. Nao achou o hordeManagerComunicator"); }

        int currentWave = 2;
        try { currentWave = hordeManagerCommunicator.GetCurrentWave() - 1; }//Subtract one for zero-based indexing.
        catch { Debug.LogError("Erro ao tentar pegar a onda atual. Nao achou o hordeManagerComunicator"); }


        if (healthIncreaseCoefficient > 1)
        {
            Debug.Log("hp multiplier >1! setando pra 0.2f");
            healthIncreaseCoefficient = 0.2f;
        }

       

        if (currentWave < 0) currentWave = 0; //Handle Alkarion already being present before the first wave.

        if (forceDoubleCheckWave) //Guard against an incorrect wave value of 41.
        {
            if (currentWave + 1 != runData.currentWaveNumber)
            {
                Debug.Log("inconsistencia nos valores das ondas. Corrigindo...");
                Debug.Log("current wave = " + currentWave);
                Debug.Log("run data current wave = " + runData.currentWaveNumber);
                currentWave = 1;
            }
        }

        if (currentHealth < 1) currentHealth = 1;

        if (enemyData.healthIncreasesEveryWave && currentWave > enemyData.updateHpOnlyAfterWave)
        {
            //Avoid a sudden health spike when scaling begins by treating
            //the first scaling wave as effective wave zero.
            float effectiveWave = currentWave - enemyData.updateHpOnlyAfterWave;
            effectiveWave = Aux.ReturnZeroIfNegative(effectiveWave);

            switch (enemyData.healthUpdateMode)
            {
                case EnemyHealthUpdateMode.doNotIncreaseHealth:

                    break;
                case EnemyHealthUpdateMode.increaseEveryWaveWithCurve:
                    float curveMultiplier = enemyData.healthIncreaseCurve.Evaluate(currentWave);
                    currentHealth = maxHealth * curveMultiplier * globalStats.enemyHealthMultiplier;
                    break;
                case EnemyHealthUpdateMode.increaseEveryWaveWithHordeManager:
                    healthIncreaseCoefficient = UpdateHealthIncreaseCoefficient(healthIncreaseCoefficient, currentWave);
                    currentHealth = maxHealth + maxHealth * healthIncreaseCoefficient * effectiveWave * globalStats.enemyHealthMultiplier;
                    break;
                case EnemyHealthUpdateMode.increaseEveryWaveWithCustomCoefficient:
                    currentHealth = maxHealth + maxHealth * enemyData.healthIncreaseCustomCoefficient * effectiveWave * globalStats.enemyHealthMultiplier;
                    break;
                case EnemyHealthUpdateMode.setHpToWaveValue:
                    currentHealth = enemyData.healthIncreaseCurve.Evaluate(currentWave) * globalStats.enemyHealthMultiplier;
                    break;
                default:
                    break;
            }
        }
        else
        {
            currentHealth = maxHealth;
        }


        if (enemyData.increaseHealthOnRainWaves) SetNewHealthForRainWaves(currentWave);

        //Reduce health in early waves.
        if (currentWave <= 5) currentHealth *= enemyData.hpUntillwave5Multiplier;
        else if (currentWave <= 10) currentHealth *= enemyData.hpUntillwave10Multiplier;
        else if (currentWave <= 15) currentHealth *= enemyData.hpUntillwave15Multiplier;

        if (currentHealth < 1)
        {
            Debug.LogError("A vida do inimigo nao deveria ser menor que 1 !!!. Corrigindo...");
            currentHealth = 1;
        }

        maxHealth = currentHealth; //Refresh maximum health because it scales with waves.
        ApplyCampaignHealthScale();


        Aux.Print(gameObject.name + " wave: " + currentWave, showDebug);
        Aux.Print(gameObject.name + " HP multiplier: " + healthIncreaseCoefficient, showDebug);
        Aux.Print(gameObject.name + " HP: " + currentHealth, showDebug);
    }

    private static float UpdateHealthIncreaseCoefficient(float healthIncreaseCoefficient, int currentWave)
    {
        //Ramp the multiplier in early waves instead of applying its full value immediately.
        if (currentWave < 6)
        {
            healthIncreaseCoefficient = healthIncreaseCoefficient * 0.6f;
        }
        else if (currentWave < 12)
        {
            healthIncreaseCoefficient = healthIncreaseCoefficient * 0.8f;
        }
        else if (currentWave < 16)
        {
            healthIncreaseCoefficient = healthIncreaseCoefficient * 0.9f;
        }

        return healthIncreaseCoefficient;
    }

    private void SetNewHealthForRainWaves(int currentWave)
    {
        if(enemyData.addWaveNumberToHealth && currentWave > 12)
        {
            currentHealth += currentWave;
        }

        if (currentWave < 20) return;

        float multiplier = 1;

        if (currentWave <= 25) //20
        {
            multiplier = 1.1f;
        }
        else if (currentWave <= 30) //25
        {
            multiplier = 1.3f;
        }
        else if (currentWave <= 49) //49
        {
            multiplier = 2f;
        }
        else //>=50
        {
            multiplier = currentWave - 46;// Subtract 46 so the multiplier is 4 at wave 50.
        }


        currentHealth *= multiplier;
    }

    protected void CheckContinuousDamage()
    {
        if (IsProtectedUntilPlayerMelee) return;
        if (invulnerableTimer > 0 && !IsCampaignFlashStunned) return;

        if (damagePerSecond <= 0) return; //No continuous damage is active.
        else
        {
            if (damagePerSecTimer > 0) damagePerSecTimer -= Time.deltaTime;
            else
            {
                damagePerSecTimer = 1f;
                TriggerContinuousDamage();
                SetDamageType(DamageType.Impact);
            }

        }
    }

    public  void SetDamageType(DamageType damageType)
    {
        if (IsProtectedUntilPlayerMelee) return;
        if (currentHealth <= 0) return; //Do not apply damage to an already dead target.
        if (invulnerableTimer > 0 && !IsCampaignFlashStunned) return;
        lastDamageType = damageType;
    }

    public virtual void SetDamage(float amount) => ApplyHealthDamage(amount, true);

    public virtual void ApplyHealthDamage(float amount, bool allowFlashExecution)
    {
        if (IsProtectedUntilPlayerMelee) return;

        if (currentHealth <= 0) return; //Do not apply damage to an already dead target.

        if (invulnerableTimer > 0 && !IsCampaignFlashStunned) return; //Ignore damage.

        // Aux.Print("Health damage", showDebug);

        currentHealth -= allowFlashExecution && CampaignCombatRules.IsCampaignMission && campaignFlashStunned ? Mathf.Max(amount, currentHealth) : amount;
        if (currentHealth <= 0) //Dead.
        {
            currentHealth = 0;
            Die();
            OnZeroHealth?.Invoke();
        }

        //Aux.Print("current hp: " + currentHealth, showDebug);
        OnHealthChange?.Invoke();
        OnTakeDamage?.Invoke();

    }

    public void DefeatByPurification()
    {
        if (currentHealth <= 0f) return;
        currentHealth = 0f;
        lastDamageType = DamageType.Explosive;
        Die();
        OnZeroHealth?.Invoke();
        OnHealthChange?.Invoke();
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    #region Replenish Health related

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        OnHealthChange?.Invoke();
    }

    public void AddToMaxHealth(float health)
    {
        maxHealth += health;
        ReplenishHealth(health);
        OnHealthChange?.Invoke();

    }

    public void ReplenishHealth()
    {
        currentHealth = maxHealth;
        OnHealthChange?.Invoke();
    }

    public void ReplenishHealth(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        OnHealthChange?.Invoke();
    }

    public bool CanHeal()
    {
        if (maxHealth <= currentHealth)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Clamp current health to maximum health when healing would exceed it.
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(float amount)
    {

        if (currentHealth + amount > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth += amount;
        }

        OnHealthChange?.Invoke();
    }

    /// <summary>
    /// Heal over time up to maximum health.
    /// </summary>
    /// <param name="amount"></param>
    public void HealOverTime(float amountPerTick,float frequency,float LifeLimit)
    {
        StartCoroutine(HealOverTime_Routine(amountPerTick, frequency, LifeLimit));
    }


    protected IEnumerator HealOverTime_Routine(float amount, float frequency, float totalHealth)
    {
        float healed = 0f;

        while (healed < totalHealth)
        {
            if (currentHealth <= 0) //Dead.
            {
                yield break;
            }

            float remaining = totalHealth - healed;
            float healThisTick = Mathf.Min(amount, remaining);

            Heal(amount);

            if (currentHealth + amount >= maxHealth)
            {
                yield break;
            }

                healed += healThisTick;

            yield return new WaitForSeconds(frequency);
        }
    }
    #endregion

    public void InstaKill( bool forceKill = false)
    {
        if (enemyData.bossOrElite && forceKill == false) return;

        SetDamage(9999999);
        SetDamageType(DamageType.Impact);
        //OnHealthChange?.Invoke();
        //OnZeroHealth?.Invoke();
    }

    /// <summary>
    /// Kill the enemy without spawning a corpse,
    /// </summary>
    public void InstaDieWithNoCorpse()
    {
        hordeManagerCommunicator?.NotifyDeath();
        OnInstaDieWithNoCorpse?.Invoke();
        gameObject.SetActive(false);
    }

    public void SetInvulnerableTime(float time)
    {
        invulnerableTimer = time;
    }

    public void BeginCampaignMissionState()
    {
        if (CompareTag("Enemy") && !TryGetComponent<EnemyTowerAttack>(out _))
            gameObject.AddComponent<EnemyTowerAttack>();
        IsProtectedUntilPlayerMelee = false;
        invulnerableTimer = 0f;
        campaignFlashStunned = false;
        campaignFlashStunTimer = 0f;
        campaignRepulsionUntil = 0f;
    }

    public void InitializeCampaignHealth()
    {
        float waveMultiplier = CampaignMissionRuntime.Active?.GetEnemyHealthWaveMultiplier() ?? 1f;
        currentHealth = maxHealth = enemyData.health * waveMultiplier;
        campaignHealthScaleApplied = false;
        ApplyCampaignHealthScale();
    }

    public void ApplyCampaignHealthScale()
    {
        if (!CampaignCombatRules.IsCampaignMission || campaignHealthScaleApplied) return;
        float multiplier = CampaignCombatRules.EnemyHealthMultiplier;
        currentHealth = Mathf.Max(1f, currentHealth * multiplier);
        maxHealth = Mathf.Max(currentHealth, maxHealth * multiplier);
        campaignHealthScaleApplied = true;
        OnHealthChange?.Invoke();
    }

    public void ApplyCampaignFlashStun(float duration)
    {
        if (!CampaignCombatRules.IsCampaignMission || currentHealth <= 0f) return;
        StatusEffectOnEnemy status = GetComponent<StatusEffectOnEnemy>();
        if (status == null) status = GetComponentInChildren<StatusEffectOnEnemy>();
        status?.ApplyFlashConcussion(duration);
        if (!campaignFlashStunned)
        {
            campaignFlashStunned = true;
            if (TryGetComponent(out Rigidbody2D body))
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
        }
        campaignFlashStunTimer = Mathf.Max(campaignFlashStunTimer, duration);
    }

    public void ApplyCampaignFlashRepulsion(Vector2 direction, float speed)
    {
        if (!CampaignCombatRules.IsCampaignMission || speed <= 0f || currentHealth <= 0f ||
            !TryGetComponent(out Rigidbody2D body) || body.bodyType != RigidbodyType2D.Dynamic) return;
        campaignRepulsionUntil = Time.time + .22f;
        Vector2 impulse = direction.normalized * speed * body.mass;
        if (TryGetComponent(out EnemyAI ai)) ai.ApplyExternalKnockback(impulse, .22f);
        else body.AddForce(impulse, ForceMode2D.Impulse);
    }

    public void SetContinuousDamage(float damagePerSecond, DamageType damageType)
    {
        if (IsProtectedUntilPlayerMelee) return;
        damageTypePerSecond = damageType;
        this.damagePerSecond = damagePerSecond;
    }

    protected void Die()
    {
        if(hasGivenDeathPoints == false)
        {
            if(enemyData.giveXp) pointManager?.AddXp(enemyData.xp);
            if(enemyData.giveBloodPoints) pointManager?.AddKillPoint(lastDamageType, lastDamageWasMelee);
            hasGivenDeathPoints = true;
            if (campaignElite || enemyData.bossOrElite)
                CampaignMissionRuntime.Active?.OnEliteDefeated();
        }
        // Create any world drop before EnemyDeath returns this object to its pool.
        if (CampaignCombatRules.IsCampaignMission)
            CampaignMissionRuntime.TryDropSoul(transform.position);
        else
            GetComponent<CrystalDropOnEnemyDeath>()?.DropIfOutsideLight();
        if(useEnemyDeath) enemyDeath.HandleDeath(lastHitMomemtum, lastDamageType);
    }

    public void SetLastHitInfo(Vector3 lastHitMomentum, DamageType damageType, bool lastDamageWasMelee)
    {
        if (IsProtectedUntilPlayerMelee) return;
        this.lastHitMomemtum = lastHitMomentum;
        lastDamageType = damageType;
        this.lastDamageWasMelee = lastDamageWasMelee;
        if (enemyData.giveBloodPoints && currentHealth > 0) pointManager?.AddHitPoint(lastDamageWasMelee);
    }

    public Vector3 GetLastHitVelocity()
    {
        return lastHitMomemtum;
    }

    /// <summary>
    /// Consumes every hit while protected. Protection is removed only after the required
    /// number of entries into the tower's purification light or player beam.
    /// </summary>
    public bool TryBlockProtectedAttack(GameObject attackObject)
    {
        if (IsProtectedUntilPlayerMelee)
        {
            // Protected attacks are consumed. The tower light, not the hit itself,
            // determines when this enemy becomes vulnerable.
            return true;
        }

        var melee = attackObject != null ? attackObject.GetComponentInParent<PlayerMeleeAttackSource>() : null;
        return melee != null && melee == protectionBreakingAttack && melee.AttackId == protectionBreakingAttackId;
    }

    public void RegisterPurificationIllumination()
    {
        if (!IsProtectedUntilPlayerMelee || currentHealth <= 0)
            return;

        IlluminationCount++;
        if (IlluminationCount < Mathf.Max(1, illuminationsRequiredToBecomeVulnerable))
            return;

        IsProtectedUntilPlayerMelee = false;
        OnMeleeProtectionBroken?.Invoke();
    }

    public bool RegisterBeamIllumination(PlayerMeleeAttackSource attackSource)
    {
        if (!IsProtectedUntilPlayerMelee || currentHealth <= 0 || invulnerableTimer > 0f ||
            attackSource == null || !attackSource.CanBreakEnemyProtection)
            return false;

        // A beam can overlap several enemy colliders, but counts as one illumination
        // per enemy and per activation.
        if (protectionBreakingAttack == attackSource && protectionBreakingAttackId == attackSource.AttackId)
            return true;

        protectionBreakingAttack = attackSource;
        protectionBreakingAttackId = attackSource.AttackId;
        RegisterPurificationIllumination();
        return true;
    }

    public void FallingDeath() //for example when falling off an edge.
    {
        SetDamageType(DamageType.Impact);
        SetDamage(1000000);

    }

    public void SetCurrentHealth(float targetHealth) //for example when falling off an edge.
    {
        currentHealth = targetHealth;

    }


    protected void TriggerContinuousDamage()
    {
        SetDamage(damagePerSecond);
        handleAttackOnEnemy.TriggerAttackEnemyFeedback(damagePerSecond, damageTypePerSecond, CriticalDamage.no, DamageOnStatus.none);
    }


    /// <summary>
    /// Expose RunData here to avoid an additional lookup.
    /// across multiple components.
    /// </summary>
    /// <returns></returns>
    public RunData GetRunData()
    {
        return runData;
    }



}

