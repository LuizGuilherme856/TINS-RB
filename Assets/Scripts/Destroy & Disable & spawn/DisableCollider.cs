using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableCollider : MonoBehaviour
{
    [SerializeField] CapsuleCollider2D capsuleCollider;

    //Called by the Animator.
    public void DisableColliderNow()
    {
        capsuleCollider.enabled = false;
    }

}
