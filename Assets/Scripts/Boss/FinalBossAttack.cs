using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Placeholder used to record the trailer.
public class FinalBossAttack : MonoBehaviour
{
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private MMF_Player impactPlayer, prepareFeedback, idleFeedback;
    [SerializeField] private GameObject impactExplosion;

    [SerializeField] private Transform impactPos;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        InvokeRepeating(nameof(Attack),3, attackRate);

    }


    private void Attack()
    {
        animator.SetTrigger("Attack");
    }

    public void ImpactFeedback()
    {
        //controlCameraShake.TriggerCameraShake(shakeAngle, shakeMagnitude);
        var explosion = Instantiate(impactExplosion, impactPos.position, transform.rotation);
        explosion.GetComponentInChildren<Explosion>().TriggerExplosion();

        impactPlayer.PlayFeedbacks();
    }

    public void IdleFeedback()
    {
        idleFeedback.PlayFeedbacks();
    }

    public void PrepareFeedback()
    {
        prepareFeedback.PlayFeedbacks();
    }
}
