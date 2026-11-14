//invocation fx is the spawn feedback that appears on the screen
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO: Introduce an explicit disabled/paralyzed status that prevents damage
/// without deactivating the enemy GameObject,
/// since deactivation would interfere with pooled respawning.
/// </summary>

public class HandleInvocationFX : MonoBehaviour
{
    [SerializeField] private Animator invocationAnimator;
    [SerializeField] private float delayToRespawn = 3f;

    private StatusEffectOnEnemy statusEffectOnEnemy;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth enemyHealth;
    private Rigidbody2D rb2d;
    private float defaultMass;

    [SerializeField] protected bool activateFXOnEnable = true;

    private void Awake()
    {
        statusEffectOnEnemy = GetComponent<StatusEffectOnEnemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealth>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (activateFXOnEnable)
        {
            invocationAnimator.SetTrigger("spawnFX");
        }
        statusEffectOnEnemy.SetStatus(StatusEffect.Frozen, delayToRespawn, 0); //Workaround to stop enemy movement.
        spriteRenderer.enabled = false;
        enemyHealth.SetInvulnerableTime(delayToRespawn);
        rb2d.bodyType = RigidbodyType2D.Static;
        Invoke(nameof(EnableSprite), delayToRespawn + 0.1f); //Delay slightly so the enemy does not appear frozen.
        Invoke(nameof(ResetRigidBody), delayToRespawn - 0.1f);
    }

    private void EnableSprite()
    {
        spriteRenderer.enabled = true;
    }

    private void ResetRigidBody()
    {
        rb2d.bodyType = RigidbodyType2D.Dynamic;
    }

    public void SetActivateFXOnEnable(bool targetBool)
    {
        activateFXOnEnable = targetBool;
    }

}
