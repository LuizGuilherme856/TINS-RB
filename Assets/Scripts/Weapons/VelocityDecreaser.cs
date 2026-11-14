using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityDecreaser : MonoBehaviour
{

    private Rigidbody2D rb;
    [SerializeField] [Range(0,1)] private float velocityDecreaser = 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); 
    }

    private void FixedUpdate()
    {
        if(rb.linearVelocity.magnitude > Mathf.Epsilon)
        {
            rb.linearVelocity = rb.linearVelocity*velocityDecreaser;
        }
    }


    public void SetVelocity(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

}
