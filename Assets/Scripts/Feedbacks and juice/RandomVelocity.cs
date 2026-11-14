using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomVelocity : MonoBehaviour
{
    [SerializeField] private Vector2 minVelo, maxVelo;
    [SerializeField] bool maxControll = false;
    [SerializeField] private Vector2 veloRange;
    private Rigidbody2D myRigidbody2D;
    void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();

        ///When maxControl is enabled, per-axis values are used and may include zero.

        if(maxControll)
        {
            myRigidbody2D.linearVelocity = new Vector2(Random.Range(minVelo.x, maxVelo.x), Random.Range(minVelo.y, maxVelo.y)) ;
        }
        else
        {
            float sign = Random.Range(-1, 1);
            myRigidbody2D.linearVelocity = new Vector2(Random.Range(1, veloRange.x) * Mathf.Sign(sign), Random.Range(1, veloRange.y) * Mathf.Sign(sign));
        }
    }



    }
