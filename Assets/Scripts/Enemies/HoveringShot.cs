using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Start, maintain, and end hovering-shot animations as the player fires or stops.
/// </summary>
public class HoveringShot : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CapsuleCollider2D capsuleCollider;

    private bool isShooting = false;

    public void TriggerStart()
    {
        if (isShooting) return;
        capsuleCollider.enabled = true;
        animator.SetTrigger("Start");
        isShooting = true;
    }

    public void TriggerFinish()
    {
        if (isShooting == false) return;
        isShooting = false;
        //Deactivated through the Animator.
        //capsuleCollider.enabled = false;
        animator.SetTrigger("Finish");
    }

}
