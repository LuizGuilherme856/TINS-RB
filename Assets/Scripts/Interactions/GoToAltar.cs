using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToAltar : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody2D rb2d;

    private Transform destination;
    private Animator animator;
    private bool reachedDestination = false;
    private ParticleSystem myparticleSystem;

    private bool wasAbsorbed = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        myparticleSystem = GetComponent<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        if(reachedDestination) return;

        if(Vector2.Distance(transform.position, destination.position) < 0.2f)
        {
            animator.SetTrigger("Absorb");
            reachedDestination = true;
            myparticleSystem.Stop();
            
            if (wasAbsorbed) return; //Prevent repeated absorption.
            
            wasAbsorbed= true;
            destination.parent.gameObject.GetComponent<OrbSpawner>().AddOrb();
            Destroy(gameObject,1f);
        }
        else
        {
            Vector2 direction = (destination.position - rb2d.transform.position).normalized;

            Vector2 pos = new Vector2(transform.position.x, transform.position.y);
            rb2d.MovePosition(pos + speed * Time.fixedDeltaTime * direction);

        }



    }

    public void SetDestination(Transform destination)
    {
        this.destination = destination;
    }

}
