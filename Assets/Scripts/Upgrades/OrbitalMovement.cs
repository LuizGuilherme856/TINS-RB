using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalMovement : MonoBehaviour
{
    public Transform target, axis, player;
    public float speed = 10;
    public Vector3 direction = Vector3.up;

    

    void FixedUpdate()
    {
        axis.localScale = player.localScale; //just to flip when the characters flip
        target.RotateAround(axis.position, direction, speed * Time.fixedDeltaTime);

       // if (player.localScale.x < 0)
    }
}
