using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using System;

public class HandleAttackOnPlayer : MonoBehaviour
{
    private Rigidbody2D myRigidbody;
    private Health health;

    [SerializeField] private MMF_Player damageFeedback;
    [SerializeField] private float damageCooldown = 0.3f;
    [SerializeField] private LanguageData languageData;
    [SerializeField] private MMF_Player damageIgnoreFeedback;
    [SerializeField] private Shield_PowerUp shield_PowerUp;
    [SerializeField] private StatsCalculator statsCalculator;

    [SerializeField] private bool showDebug = false;

    private MMF_FloatingText ignoreDamageText;
    private CharacterDataManager characterDataManager;

    private float knockBackTime = 0.2f, timer=0;
    private float damageCooldownTimer = 0;
    private Vector2 knockBackDirection;

    //HeadHitRelated
    private HandlePlayerHeadHit headHitHandler;
    //ScreenEffecs
    private HandleScreenEffecs handleScreenEffecs;

    private void Awake()
    {
        characterDataManager = GetComponent<CharacterDataManager>();
        health = GetComponent<Health>();
        
        myRigidbody = GetComponent<Rigidbody2D>();
        ignoreDamageText = damageIgnoreFeedback.GetFeedbackOfType<MMF_FloatingText>();
        try
        {
            headHitHandler = GetComponent<HandlePlayerHeadHit>();
        }
        catch (Exception)
        {
            Debug.LogError("Player não possui um script HandlePlayerHeadHit");
        }
        try
        {
            handleScreenEffecs = FindFirstObjectByType<HandleScreenEffecs>();
        }
        catch (Exception)
        {
            Debug.Log("Não encontrou o handleScreenEffecs nessa scene");
        }
        health.OnZeroHealth += ResetHeadAndScreenEffecs;

    }


    private void FixedUpdate()
    {
        if(timer > 0)
        {
            //Restore the body type because death makes it static.
            if(myRigidbody.bodyType == RigidbodyType2D.Dynamic) myRigidbody.linearVelocity = knockBackDirection;
            timer -= Time.deltaTime;
        }

        if(damageCooldownTimer > 0)
        {
            damageCooldownTimer -= Time.deltaTime;
        }
    }
    public void DamagePlayer(float damage, bool forceDamage = false)
    {
        if (damage == 0) return; //  Do not play feedbacks for zero damage.

        if (forceDamage == false)
        {
            if (CanPlayerBeDamaged() == false)
            { 
                   // Debug.Log("Player cannot be damaged at the moment");
                return;
            }



            if (TryToIgnoreDamage())
            {
                IgnoreDamageFeedbacks();
                return;
            }
            
        }     


        if (health.GetCurrentHealth() >0)
        {
            health.SetDamage(damage, forceDamage);
            if (health.GetCurrentHealth() > 0) damageFeedback.PlayFeedbacks();

            damageCooldownTimer = damageCooldown;

        }

    }
    public void DamagePlayer(float damage, Vector2 forceDirection, bool continuous = false)
    {
        if (damage == 0) return; //  Do not play feedbacks for zero damage.
        if (CanPlayerBeDamaged() == false) return;
        if(TryToIgnoreDamage())
        {
            IgnoreDamageFeedbacks();
            return;
        }

        if (health.GetCurrentHealth() > 0)
        {

            health.SetDamage(damage);
            damageFeedback.PlayFeedbacks();
            if (continuous == false)
            {
                knockBackDirection = forceDirection;
                timer = knockBackTime;
            }
            else
            {
                myRigidbody.AddForce(forceDirection, ForceMode2D.Impulse);
            }
            Aux.Print("atacou player com direção: " + forceDirection, showDebug);

            damageCooldownTimer = damageCooldown;
        }
        
    }
    /// <summary>
    /// Controls head effects triggered by attacks or enemies.
    /// Return true if the player's head is available for attachment.
    /// </summary>
    /// <param name="enemyType">Enemy type.</param>
    /// <param name="targetHealth">Health of the enemy attaching to the player, if applicable.</param>
    public bool OnHeadEffect(EnemyType enemyType,float targetHealth, JumpingEnemyData jumpingEnemyData)
    {
        if (CanPlayerBeDamaged() == false) return false;
        //Always damage the player when attachment succeeds.
        DamagePlayer(1.0f);
        //Return false if the head is occupied.
        if (headHitHandler.isHeadOccupied())
        {
            return false;
        }
        switch (enemyType)
        {
            // case EnemyType.brainSpider: // Test interactions with the other head-attaching enemy once implemented.
            //     return true;
            case EnemyType.jumpingHandManager:
                headHitHandler.HandHeadHit(targetHealth, jumpingEnemyData);
                return true;
            case EnemyType.egg:
                headHitHandler.EggHeadHit(targetHealth, jumpingEnemyData);
                return true;
            default:
                Debug.LogError("Interação com a cabeça do player não implementada");
                return false;
        }

    }



    private bool CanPlayerBeDamaged()
    {
        if (damageCooldownTimer > 0) return false;

        if (health.invulnerable || health.invulnerableRune || health.invulnerable_dash || health.invulnerableShield) return false;
        if (health.invulnerableTimer > 0) return false;

        return true;
    }

    private bool TryToIgnoreDamage()
    {
        float defaultIgnoreDamageChance = characterDataManager.GetCurrentCharacterData().chanceToIgnoreDamage;
        float newChance = statsCalculator.CalculateIgnoreDamageChance(defaultIgnoreDamageChance);
        return UnityEngine.Random.Range(0f, 1f) <= newChance; // TODO: Confirm this behavior with Luiz.

    }
    private void IgnoreDamageFeedbacks()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                ignoreDamageText.Value = "Damage Ignored";
                break;
            case Language.chinese:
                ignoreDamageText.Value = "被忽略的伤害";
                break;
            case Language.portuguese:
                ignoreDamageText.Value = "Dano ignorado";
                break;
            case Language.spanish:
                ignoreDamageText.Value = "Daño ignorado";
                break;
            default:
                ignoreDamageText.Value = "Damage Ignored";
                break;
        }

        shield_PowerUp.ActivateShield(2f);
        damageCooldownTimer = 2.1f;
        damageIgnoreFeedback.PlayFeedbacks();
    }
    private void ResetHeadAndScreenEffecs()
    {
        handleScreenEffecs.ResetAllEffecs();
        headHitHandler.ResetAllHeadObjects();
    }

}
