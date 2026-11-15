using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusSpecialEffect : MonoBehaviour
{
    [SerializeField] private GameObject fireEffect, bleedingEffect, concussionEffect, explodedEffect, soakedEffect, electrocutedEffect,
        poisonedEffect;

    private ChainReactionStatusEffect chainReactionStatusEffect;

    private float explosionCooldown = 1f;
    private float timerExplosionCooldown;



    private void Awake()
    {
        chainReactionStatusEffect = GetComponent<ChainReactionStatusEffect>();
    }

    private void Update()
    {
        if(timerExplosionCooldown > 0)
        {
            timerExplosionCooldown -= Time.deltaTime;
        }
    }

    private void OnEnable()
    {
        RemoveAllEffects();
    }


    /// <summary>
    /// Remove all currently applied effects.
    /// </summary>
    public void RemoveAllEffects()
    {
        fireEffect?.SetActive(false);
        bleedingEffect?.SetActive(false);
        concussionEffect?.SetActive(false);
        explodedEffect?.SetActive(false);
        soakedEffect?.SetActive(false);
        electrocutedEffect?.SetActive(false);
        poisonedEffect?.SetActive(false);
    }

    /// <summary>
    /// Activate the visual for the specified StatusEffect.
    /// </summary>
    /// <param name="effect">StatusEffect to activate.</param>
    public void ApplyEffect(StatusEffect effect)
    {
        // Disable all effects first.
        RemoveAllEffects();

        // Activate the matching effect.
        switch (effect)
        {
            case StatusEffect.Burning:
                fireEffect?.SetActive(true);
                break;
            case StatusEffect.Bleeding:
                bleedingEffect?.SetActive(true);
                break;
            case StatusEffect.Concussion:
                concussionEffect?.SetActive(true);
                break;
            case StatusEffect.Exploded:
                if(timerExplosionCooldown <= 0)
                {
                    explodedEffect?.SetActive(true);
                    timerExplosionCooldown = explosionCooldown;
                }
                else
                {
                    Debug.Log("Tentou ativar a explosao antes do cooldown");
                }

                break;
            case StatusEffect.Soaked:
                soakedEffect?.SetActive(true);
                break;
            case StatusEffect.Electrocuted:
                electrocutedEffect?.SetActive(true);
                chainReactionStatusEffect.TriggerReaction();
                break;
            case StatusEffect.Punctured:
            case StatusEffect.Frozen:
            case StatusEffect.Poisoned:
                poisonedEffect?.SetActive(true);
                break;
            case StatusEffect.None:
            default:
                // No action for other effects or None.
                break;
        }
    }
}
