//Flip the sprite according to X velocity.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipSpriteVelocity : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 originalScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        if(rb.linearVelocity.x >= 0)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        }
    }
}

