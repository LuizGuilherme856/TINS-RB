/// 
using UnityEngine;
using System;
using MoreMountains.Tools;
using TechBooh;

/// <summary>
/// Adjusts damage from active enemy status effects.
/// including frozen, burning, and other statuses.
/// </summary>
public class StatusEffectOnEnemy : MonoBehaviour
{
    private StatusEffect currentStatus = StatusEffect.None;
    [SerializeField] private GameObject hitFeedback;
     private StatusSpecialEffect statusSpecialEffect;
    private EnemyData enemyData;
    [SerializeField] private EnemyHealth enemyHealth;

    private ShaderReplacer shaderReplacer;
    //private HandleAttackOnEnemy handleAttackOnEnemy;
    public event Action OnStatusChange;

    private float timeAlive = 0, effectTimer;

    private void Awake()
    {
        if(enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
        enemyData = enemyHealth.enemyData;
        //handleAttackOnEnemy= GetComponent<HandleAttackOnEnemy>();
        shaderReplacer = Try.GetComponent<ShaderReplacer>(this.gameObject);

        statusSpecialEffect = GetComponentInChildren<StatusSpecialEffect>();

    }

    private void OnEnable()
    {
        currentStatus = StatusEffect.None; //Status-immune enemies previously started frozen, preventing stuck-enemy recovery.
        OnStatusChange?.Invoke();
        timeAlive = 0;
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;

        if(effectTimer > 0)
        {
            effectTimer -= Time.deltaTime; 
        }
        else if (currentStatus != StatusEffect.None)
        {
            ResetStatusEffect();
        }
    }

    public void ApplyFlashConcussion(float duration)
    {
        if (currentStatus != StatusEffect.Concussion)
        {
            ClearStatusEffect();
            currentStatus = StatusEffect.Concussion;
            statusSpecialEffect?.ApplyEffect(StatusEffect.Concussion);
            OnStatusChange?.Invoke();
        }
        effectTimer = Mathf.Max(effectTimer, duration);
    }

    public void SetStatus(StatusEffect status, float effectDuration, float statusDamage)
    {
        if(enemyData == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
            enemyData = enemyHealth.enemyData;
            shaderReplacer = GetComponent<ShaderReplacer>();

        }
        if (enemyHealth != null && enemyHealth.IsCampaignFlashStunned) return;
        // Only the Flash applies concussion through ApplyFlashConcussion.
        if (status == StatusEffect.None || status == StatusEffect.Concussion) return;

        if (enemyData.immuneToStatus) return;
        if (status == currentStatus) return;
        ResetStatusEffect();

        switch (status)
        {
            case StatusEffect.Punctured:
                statusSpecialEffect.ApplyEffect(StatusEffect.Punctured);
                break;
            case StatusEffect.Bleeding:
                statusSpecialEffect.ApplyEffect(StatusEffect.Bleeding);
                enemyHealth.SetContinuousDamage(statusDamage, DamageType.Slash);
                break;
            case StatusEffect.Exploded:
                statusSpecialEffect.ApplyEffect(StatusEffect.Exploded);
                break;
            case StatusEffect.Burning:
                ApplyBurning(statusDamage);
                break;
            case StatusEffect.Frozen:
                ApplyFrozenStatus();
                break;
            case StatusEffect.Poisoned:
                ApplyPoisoned(statusDamage);
                break;
            case StatusEffect.Soaked:
                statusSpecialEffect.ApplyEffect(StatusEffect.Soaked);
                shaderReplacer.SetShaderValue(DamageType.Water);
                break;
            case StatusEffect.Electrocuted:
                ApplyElectrocuted(statusDamage);
                break;
            case StatusEffect.None:
                break;
            default:
                break;
        }

        currentStatus = status;

        OnStatusChange?.Invoke();
        

        //Randomize thaw timing so enemies do not all unfreeze together.
        var durationRandomness = UnityEngine.Random.Range(-effectDuration * 0.1f, effectDuration * 0.1f);
        effectTimer = effectDuration + durationRandomness;

        //Invoke(nameof(ResetStatusEffect), );
  
    }

    private void ApplyElectrocuted(float statusDamage)
    {
        enemyHealth.SetContinuousDamage(statusDamage, DamageType.Electricity);
        shaderReplacer.SetShaderValue(DamageType.Electricity);
        statusSpecialEffect.ApplyEffect(StatusEffect.Electrocuted);
    }

    private void ApplyPoisoned(float statusDamage)
    {
        shaderReplacer.SetShaderValue(DamageType.Poison);
        enemyHealth.SetDamageType(DamageType.Poison);
        //enemyHealth.SetContinuousDamage(statusDamage);
        statusSpecialEffect.ApplyEffect(StatusEffect.Poisoned);
    }

    private void ApplyBurning(float statusDamage)
    {
        statusSpecialEffect.ApplyEffect(StatusEffect.Burning);
        enemyHealth.SetDamageType(DamageType.Fire);
        enemyHealth.SetContinuousDamage(statusDamage, DamageType.Fire);
    }

    private void ApplyFrozenStatus()
    {
        //Instantly kill status-vulnerable enemies only after one second alive, since they may spawn frozen.
        if (enemyData.weakenessToIce && timeAlive > 1f)
        {
            //enemyHealth.InstaKill();
            //currentStatus = StatusEffect.none;
            enemyHealth.SetDamage(5000);
            enemyHealth.SetDamageType(DamageType.Explosive);
            return;
        }


        hitFeedback.SetActive(false);
        shaderReplacer.SetShaderValue(DamageType.Ice);
        enemyHealth.SetDamageType(DamageType.Ice);
    }

    public void ResetStatusEffect()
    {
        if (enemyData != null && enemyData.immuneToStatus && currentStatus != StatusEffect.Concussion) return;
        ClearStatusEffect();
    }

    private void ClearStatusEffect()
    {
        currentStatus = StatusEffect.None;
        statusSpecialEffect?.RemoveAllEffects();
        OnStatusChange?.Invoke();
        shaderReplacer?.SetDefaultShaderValue();
        hitFeedback?.SetActive(true);
        enemyHealth.SetContinuousDamage(0 , DamageType.None);
    }

    public StatusEffect GetCurrentStatus()
    {
        return currentStatus;
    }
}

