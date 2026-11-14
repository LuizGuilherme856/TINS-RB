///Triggers boss animations for hits, attacks, death, and related actions.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MoreMountains.Feedbacks;

public class EnemyAnimations : MonoBehaviour
{
    private AttackPlayer attackPlayer;
    private EnemyHealth enemyHealth;
    private Animator animator;

    [SerializeField] private MMMiniObjectPooler miniObjectPooler;
    [SerializeField] bool hasAttackFeedBack = false;
    [SerializeField] private MMF_Player attackFeedback;
    [SerializeField] private float cooldownToPlayHitAnimation = 5f;
    [SerializeField] private bool hasHitAnimation = true, hasAttackAnimation = true;
    [SerializeField] BossNextForm bossNextForm;

    float cooldownTimer = 0;

    private void Awake()
    {
        attackPlayer = GetComponent<AttackPlayer>();    
        enemyHealth = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        attackPlayer.OnAttackPlayer += AttackAnimation;
        enemyHealth.OnTakeDamage += TakeHitAnimation;
    }

    private void Update()
    {
        if(cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void TakeHitAnimation()
    {
        if (hasHitAnimation && cooldownTimer <= 0)
        {
            animator.SetTrigger("Hit");

            cooldownTimer = cooldownToPlayHitAnimation;
            
            if(bossNextForm)
            {
                //Double the value from form 2 onward when a next-form component exists.
                //Gromsch's hurt animation shares attack timing; prevent firing attacks during the hurt animation.
                if(bossNextForm.currentForm > 1)
                {
                    cooldownTimer *= 2;
                }
            }
        }
    }

    private void AttackAnimation()
    {
        if (hasAttackAnimation)
        {
            animator.SetTrigger("Attack");
            if(hasAttackFeedBack)
            {
                attackFeedback.PlayFeedbacks();
            }
        }
    }
}
