using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The stork sells weapons.
/// </summary>
public class StorkMovement_NPC : MonoBehaviour
{
    [SerializeField] private float speed =2, timeBetweenTravels = 1f;
    [SerializeField] private Transform[] initialPos, finalPos;
    [SerializeField] private Transform dragonTransform;
    private Vector3 currentDestination;
    private float timer;

    private void Start()
    {
        SetRandomDestination();
    }

    private void SetRandomDestination()
    {
        timer = timeBetweenTravels;
        int index = 0;

        float RandomDirection = Random.Range(0f, 1f);
        if(RandomDirection > 0.5) //Move from left to right.
        {
            index = Random.Range(0, initialPos.Length);
            dragonTransform.position = initialPos[index].position;
            
            index = Random.Range(0, finalPos.Length);
            currentDestination = finalPos[index].position;
            dragonTransform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            index = Random.Range(0, finalPos.Length);
            dragonTransform.position = finalPos[index].position;
            index = Random.Range(0, initialPos.Length);
            currentDestination = initialPos[index].position;
            dragonTransform.localScale = new Vector3(-1,1,1);
        }

    }


    private void FixedUpdate()
    {
        if(Vector3.Magnitude(dragonTransform.position - currentDestination) <= 0.01f) //The target has been reached.
        {
            timer -= Time.fixedDeltaTime;
            if(timer <=0) SetRandomDestination();
        }

        dragonTransform.position = Vector2.MoveTowards(dragonTransform.position, currentDestination, speed * Time.fixedDeltaTime);
    }
}
