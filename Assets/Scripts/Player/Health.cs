using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    private PlayerData playerData;
    private CharacterDataManager characters;
    private float currentHealth;
    private float campaignMaxHealthBonus;

    [SerializeField] private bool isTutorial = false;

    public bool showDebug, invulnerable = false, invulnerable_dash = false, invulnerableRune = false, invulnerableShield;//Separate invulnerability sources prevent interference with Nezha's protection.

    public float invulnerableTimer { get; protected set; }

    private float damagePerSecond = 0f, damagePerSecTimer = 1f;

    public event Action OnHealthChange, OnZeroHealth, OnTakeDamage, OnKnockedDown;

    protected virtual void Awake()
    {
        playerData = GetComponent<PlayerDataHolder>().GetPlayerData();
        characters = GetComponent<CharacterDataManager>();
        invulnerableTimer = 0;
    }

    private void Start()
    {
        OnHealthChange += CheckIfIsDead;
    }

    private void Update()
    {

        if (showDebug) Debug.Log("Invulnerable Timer: " + invulnerableTimer);
        if (invulnerableTimer > 0) invulnerableTimer -= Time.deltaTime;
        CheckContinuousDamage();
    }

    protected void CheckContinuousDamage()
    {
        if (invulnerable || invulnerableRune || invulnerable_dash) return;

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

    //Separate virtual hook so enemy implementations can play damage feedbacks.
    protected virtual void TriggerContinuousDamage()
    {
        SetDamage(damagePerSecond);
    }


    public void ReplenishHealth(float amount)
    {
        currentHealth += amount;
        OnHealthChange?.Invoke();
    }

    //Do not call this directly to damage the player.
    //Use HandleAttackOnPlayer.DamagePlayer() so hit feedbacks are also triggered.
    // forceDamage bypasses the normal protection checks. Use it only for explicit scripted damage;
    // ordinary player hits should enter through HandleAttackOnPlayer for consistent feedbacks.
    public virtual void SetDamage(float amount, bool forceDamage = false)
    {
        if (forceDamage == false)
        {
            if (invulnerable || invulnerable_dash || invulnerableRune || invulnerableShield) return;

            if (GetCurrentHealth() <= 0) return; //Do not apply damage to an already dead target.

            if (invulnerableTimer > 0) return; //Ignore damage.

            if (isTutorial)
            {
                if (GetCurrentHealth() <= 1)
                {
                    ReplenishHealth(5);

                }
            }
    
        }

        Aux.Print("dano na vida", showDebug);

        SetCurrentHealth(GetCurrentHealth() - amount);
        if (GetCurrentHealth() <= 0) //Dead.
        {
            SetCurrentHealth(0);

            //Die();
            OnKnockedDown?.Invoke();
        }

        Aux.Print("current hp: " + GetCurrentHealth(), showDebug);
        OnHealthChange?.Invoke();
        OnTakeDamage?.Invoke();

    }

    public void Kill()
    {
        OnZeroHealth?.Invoke();
    }

    public void InstaKill()
    {
        SetCurrentHealth(0);
        OnHealthChange?.Invoke();
        OnZeroHealth?.Invoke();
    }

    public void SetInvulnerableTime(float time)
    {
        invulnerableTimer = time;
    }

    public void SetContinuousDamage(float damagePerSecond)
    {
        this.damagePerSecond = damagePerSecond;
    }

    public bool IsAlive()
    {
        return GetCurrentHealth() > 0;
    }

    public void ResetHealth() //Call only when starting a run.
    {
        currentHealth = playerData.GetInitialHealth(characters) + campaignMaxHealthBonus;
        OnHealthChange?.Invoke();

    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetCurrentHealth(float health)
    {
        currentHealth = health;
    }

    public void SetCampaignMaxHealthBonus(float bonus)
    {
        campaignMaxHealthBonus = Mathf.Max(0f, bonus);
    }

    public void UpdateHealth()
    {

        OnHealthChange?.Invoke();
    }

    private void CheckIfIsDead() //Keep the player invulnerable during restart to avoid immediate damage.
    {
        if (IsAlive()) return;
        invulnerable = true;
    }

    public void SetIsTutorial(bool isTutorial)
    {
        this.isTutorial = isTutorial;
    }

    public bool IsTutorial()
    {
        return isTutorial;
    }

    

    protected virtual void Die()
    {

    }

    public virtual void SetDamageType(DamageType damageType)
    {

    }
}
