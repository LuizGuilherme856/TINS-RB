using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayerOnTouch : MonoBehaviour
{
    [SerializeField] private CollisionType collisionType;

    [SerializeField] protected bool doDamageOnStay = false;

    [SerializeField] protected float damageInterval = 1f;

    [SerializeField] private float enableDamageDelay = 0f; // Delay after enable before damage is possible

    protected float lastTickOfDamage = Mathf.Infinity;
    private float enableTime = -Mathf.Infinity;

    private void OnEnable()
    {
        enableTime = Time.time;
    }

    private bool CanDealDamage()
    {
        return Time.time >= enableTime + enableDamageDelay;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collisionType == CollisionType.collider) return;
        if (!CanDealDamage()) return;

        if (collision.gameObject.CompareTag("Player") == false) return;

        var player = collision.gameObject.GetComponent<HandleAttackOnPlayer>();

        if (player != null)
        {
            player.DamagePlayer(1);
            lastTickOfDamage = Time.time;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!doDamageOnStay) return;
        if (collisionType == CollisionType.collider) return;
        if (!CanDealDamage()) return;

        if (collision.gameObject.CompareTag("Player") == false) return;

        if (damageInterval <= Time.time - lastTickOfDamage)
        {
            var player = collision.gameObject.GetComponent<HandleAttackOnPlayer>();

            if (player != null)
            {
                player.DamagePlayer(1);
                lastTickOfDamage = Time.time;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collisionType == CollisionType.trigger) return;
        if (!CanDealDamage()) return;

        if (collision.gameObject.CompareTag("Player") == false) return;

        var player = collision.gameObject.GetComponent<HandleAttackOnPlayer>();

        if (player)
        {
            player.DamagePlayer(1);
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!doDamageOnStay) return;
        if (collisionType == CollisionType.trigger) return;
        if (!CanDealDamage()) return;

        if (collision.gameObject.CompareTag("Player") == false) return;

        if (damageInterval <= Time.time - lastTickOfDamage)
        {
            var player = collision.gameObject.GetComponent<HandleAttackOnPlayer>();

            if (player != null)
            {
                player.DamagePlayer(1);
                lastTickOfDamage = Time.time;
            }
        }
    }

    private enum CollisionType { trigger, collider, both }
}
