///Randomize casing animations when they hit the ground.
///Consider a shader implementation if further optimization is needed.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ImpactOnTheFloor : MonoBehaviour
{
    [SerializeField] private Rigidbody2D myRigidbody2D;
    [SerializeField] float torque = 5f, timeUntillStop = 0.5f, timeDecreaser = 0.3f, impulseDecreaser = 0.6f;
    [SerializeField] private Vector2 impulse = new Vector2(1, 5);
    [SerializeField] private bool becomeStatic = true, impulseOnEnable = false;

    public event Action OnStatic;
    
    private Vector2 currentimpulse;
    //private float currentTorque;
    private void Awake()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        ResetValues();

    }


    private void OnEnable()
    {
        if(myRigidbody2D == null) myRigidbody2D = GetComponent<Rigidbody2D>();

        ResetValues();
        if (impulseOnEnable)
        {
            AddRandomImpulseWithDecreaser(1);
            StartCoroutine(StopMovement(true, timeUntillStop));

        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Reduce speed after a collision.
        if(collision.gameObject.layer == LayerMask.NameToLayer("Obstacle")
            || collision.gameObject.layer == LayerMask.NameToLayer("Table"))
        {
            myRigidbody2D.linearVelocity = myRigidbody2D.linearVelocity / 4;
            myRigidbody2D.rotation = myRigidbody2D.rotation / 4;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        currentimpulse = myRigidbody2D.linearVelocity;
    }

    private void AddRandomImpulseWithDecreaser(float impulseMultiplier)
    {
        currentimpulse *= impulseMultiplier;
        //currentTorque *= torqueMultiplier;
        if(UnityEngine.Random.Range(0,2) == 0) //Equal probability of moving upward or downward.
        {
            myRigidbody2D.linearVelocity = new Vector2(UnityEngine.Random.Range(-currentimpulse.x, currentimpulse.x), UnityEngine.Random.Range(currentimpulse.y - currentimpulse.y / 4, currentimpulse.y + currentimpulse.y / 4));
        }
        else
        {
            myRigidbody2D.linearVelocity = new Vector2(UnityEngine.Random.Range(-currentimpulse.x, currentimpulse.x), UnityEngine.Random.Range(-currentimpulse.y - currentimpulse.y *0.25f, -currentimpulse.y + currentimpulse.y *0.25f));
        }
        //myRigidbody2D.AddTorque(-currentTorque);
    }

    private void AddImpulseWithDecreaser(float multiplier)
    {
        currentimpulse *= multiplier;
        //currentTorque *= torqueMultiplier;
        myRigidbody2D.linearVelocity = currentimpulse;
        //myRigidbody2D.AddTorque(-currentTorque);
    }

    public void SetImpulse(Vector2 velocity, float timeUntillStop, bool considerMass = false, bool addRandomness = false, float randomFactor =0.5f)
    {
        
        ResetValues();
        if (considerMass)
        {
            currentimpulse = velocity/myRigidbody2D.mass;
        }
        else
        {
            currentimpulse = velocity;
        }

        if (addRandomness)
        {
            var randomImpulse = currentimpulse * randomFactor;
            currentimpulse += new Vector2(currentimpulse.x + UnityEngine.Random.Range(-randomImpulse.x, randomImpulse.x), currentimpulse.y + UnityEngine.Random.Range(-randomImpulse.y, randomImpulse.y));
        }

        myRigidbody2D.AddTorque(UnityEngine.Random.Range(-torque, torque));
        myRigidbody2D.linearVelocity = currentimpulse;
        StartCoroutine(StopMovement(false, timeUntillStop));
        
    }


    /// <summary>
    /// Repeat with progressively smaller impulses
    /// and shorter durations until movement stops.
    /// </summary>
    private IEnumerator StopMovement(bool random, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentimpulse.magnitude < 0.01f)
        {
            if (becomeStatic)
            {
                myRigidbody2D.bodyType = RigidbodyType2D.Static;
                OnStatic?.Invoke();
            }
            yield break ; //Stop the loop.
        }

        myRigidbody2D.linearVelocity = Vector2.zero;
        //myRigidbody2D.rotation = 0;

        if (random == true) AddRandomImpulseWithDecreaser(impulseDecreaser);
        else AddImpulseWithDecreaser(impulseDecreaser);

        StartCoroutine(StopMovement(false, timeUntillStop*timeDecreaser));
    }
    private void ResetValues()
    {
 
        myRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        currentimpulse = impulse;
        //currentTorque = torque;
    }

    public void SetKinematic()
    {
        //myRigidbody2D.mass = 10000;
        currentimpulse = Vector2.zero;
        myRigidbody2D.linearVelocity = Vector2.zero;
        myRigidbody2D.bodyType = RigidbodyType2D.Kinematic;
    }

    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ResetValues();
        AddImpulse(0.5f);
        Invoke(nameof(StopMovement), timeUntillStop*0.5f);
    }
    */
}
