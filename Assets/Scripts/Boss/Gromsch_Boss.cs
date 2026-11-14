using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Controls Gromsch's additional attacks, such as the laser.
public class Gromsch_Boss : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private BossNextForm bossNextForm;
    [SerializeField] private MoveTowardsPlayer moveTowardsPlayer;

    [Header("    LASER")]
    [SerializeField] private float minAttackTime;
    [SerializeField] private float maxAttackTime, damageDelay = 3f, attackDuration;
    [SerializeField] private GameObject laserManager;
    [SerializeField] CapsuleCollider2D[] colliders;
    [SerializeField] Animator[] animators;
    [SerializeField] private Transform laserParent;
    [SerializeField] private Vector3 secondAttackLaserSize;
    [SerializeField] private MMF_Player laserStartFeedbacks;

    [SerializeField] private MMF_Player laserAttack_1, laserAttack_2;
    private float laserTimer = 0;
    private MMF_Rotation mmf_rotationLaser_1, mmf_rotationLaser_2;

    private void Awake()
    {
        mmf_rotationLaser_1 = laserAttack_1.GetFeedbackOfType<MMF_Rotation>();
        mmf_rotationLaser_2 = laserAttack_2.GetFeedbackOfType<MMF_Rotation>();

        enemyHealth.OnZeroHealth += () => gameObject.SetActive(false);

        DeactivateLaser();
    }

    private void FixedUpdate()
    {
        if(laserTimer > 0)
        {
            laserTimer -= Time.fixedDeltaTime;
        }
        else
        {
            laserTimer = Random.Range(minAttackTime, maxAttackTime);
            TriggerLaserAttack();
        }
    }

    private void TriggerLaserAttack()
    {
        if (bossNextForm.currentForm == 0) return; //The attack is not ready yet.

        laserStartFeedbacks.PlayFeedbacks();

        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].SetTrigger("start");
        }

        Invoke(nameof(ActivateLaser), damageDelay);
        if(bossNextForm.currentForm == 1)
        {
            Invoke(nameof(LaserAttack_1), damageDelay);
        }
        else
        {
            laserParent.localScale = secondAttackLaserSize;
            Invoke(nameof(LaserAttack_2), damageDelay);
        }


    }

    private void ActivateLaser()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;         
            animators[i].SetTrigger("damage");
        }
    }
    private void DeactivateLaser()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
            animators[i].SetTrigger("off");
        }

        moveTowardsPlayer.SetCanMove(true);
    }


    private void LaserAttack_1()
    {
        mmf_rotationLaser_1.AnimateRotationDuration = attackDuration;
        laserAttack_1.PlayFeedbacks();
        moveTowardsPlayer.SetCanMove(false);
        Invoke(nameof(DeactivateLaser), attackDuration);
    }

    private void LaserAttack_2()
    {
        laserParent.localScale = secondAttackLaserSize;
        mmf_rotationLaser_2.AnimateRotationDuration = attackDuration;
        laserAttack_2.PlayFeedbacks();
        Invoke(nameof(DeactivateLaser), attackDuration);
    }
}
