using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Boss-specific EnemyHealth implementation that constrains health
/// to prevent it from exceeding the configured limit.
/// </summary>
public class BossHealth : EnemyHealth
{
    [Tooltip("Quantidade Minima de vida que ele pode chegar em %")]
    [Range(0,1)] public float minHpPercentage_ = 0.101f;
    public override event Action OnHealthChange, OnZeroHealth, OnTakeDamage;//, OnInstaDieWithNoCorpse;
    private bool reachedMinHealth = false;
    public event Action OnMinHealth;

    public override void ApplyHealthDamage(float amount, bool allowFlashExecution)
    {

        if(currentHealth/maxHealth <= minHpPercentage_ && reachedMinHealth == false) //Do not apply damage below the minimum health threshold.
        {
            OnMinHealth?.Invoke(); //Raise the minimum-health event.
            reachedMinHealth = true;
            //return;
        }

        if (currentHealth <= 0) return; //Do not apply damage to an already dead target.

        if (invulnerableTimer > 0) return; //Ignore damage.

        // Aux.Print("Health damage", showDebug);


        currentHealth -= amount;

        //Some phases must prevent the boss from dying yet.
        if (currentHealth / maxHealth <= minHpPercentage_ && minHpPercentage_ > 0)
        {
            currentHealth = minHpPercentage_ * maxHealth; //Keep health at the minimum threshold.
        }

        if (currentHealth <= 0) //Dead.
        {
            currentHealth = 0;
            Die();
            OnZeroHealth?.Invoke();
        }

        OnHealthChange?.Invoke();
        OnTakeDamage?.Invoke();

    }
}
