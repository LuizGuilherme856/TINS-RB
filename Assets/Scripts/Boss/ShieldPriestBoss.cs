using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPriestBoss : MonoBehaviour
{
    public MMF_Player spawnFeedback;
    private Animator animator;
    private EnemyHealth health;
    private PolygonCollider2D shieldCollider;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        shieldCollider = GetComponent<PolygonCollider2D>();
    }

    private void OnEnable()
    {
        health.OnZeroHealth += SetDead;
        SetDead();
    }

    public void SetAlive()
    {
        health.ReplenishHealth();
        spawnFeedback.PlayFeedbacks();
        shieldCollider.enabled = true;
        animator.Play("shield part idle");
    }

    public void SetDead()
    {
        shieldCollider.enabled = false;
        animator.Play("shield destroy");
    }
}
