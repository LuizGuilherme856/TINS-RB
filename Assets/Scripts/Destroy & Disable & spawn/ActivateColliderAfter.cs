using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Delay projectile collision so newly spawned projectiles do not immediately explode against walls.

public class ActivateColliderAfter : MonoBehaviour
{
    [SerializeField] private CapsuleCollider2D capsuleCollider;
    [SerializeField] private float timeToActivate = 1f;

    private void OnEnable()
    {
        capsuleCollider.enabled = false;
        Invoke(nameof(ActivateCollider), timeToActivate);
    }

    private void ActivateCollider()
    {
        capsuleCollider.enabled = true;
    }

}
