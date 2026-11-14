using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnParticlesOnMoving : MonoBehaviour
{

    [SerializeField] private Rigidbody2D playerRigidbody;
    private ParticleSystem walkParticles;

    private Vector2 lastVelocity;

    private void Awake()
    {
        walkParticles = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if(playerRigidbody.linearVelocity.x > 0 && lastVelocity.x < 0) //Movement changed from backward to forward.
        {
            walkParticles.Play();
        }
        else if(playerRigidbody.linearVelocity.x < 0 && lastVelocity.x > 0)
        {
            walkParticles.Play();
        }
        else if (playerRigidbody.linearVelocity.y > 0 && lastVelocity.y < 0) //Movement changed from backward to forward.
        {
            walkParticles.Play();
        }
        else if (playerRigidbody.linearVelocity.y < 0 && lastVelocity.y > 0)
        {
            walkParticles.Play();
        }


        lastVelocity = playerRigidbody.linearVelocity;
    }
}
