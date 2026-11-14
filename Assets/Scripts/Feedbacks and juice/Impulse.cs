//Apply force in the requested direction.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impulse : MonoBehaviour
{
    private Rigidbody2D rb2d;
    [SerializeField] private bool showDebug = true;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();

    }

    public void TriggerImpulse(float angle, float force, bool oppositeDirection = true,bool ignoreMass = false)
    {
        Aux.Print("angulo graus: " + angle, showDebug);
        angle *= Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Aux.Print("direção: " + direction, showDebug);

        if (ignoreMass)
        {
            force *= rb2d.mass;
        }

        direction *= force;
        Aux.Print("direção força: " + direction, showDebug);
        if (oppositeDirection) direction *= -1;
        
        rb2d.AddForce(direction, ForceMode2D.Impulse);
    }

    public void TriggerForce(float angle, float force, bool oppositeDirection = true, bool ignoreMass = false)
    {
        Aux.Print("angulo graus: " + angle, showDebug);
        angle *= Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Aux.Print("direção: " + direction, showDebug);

        if (ignoreMass)
        {
            force *= rb2d.mass;
        }

        direction *= force;
        Aux.Print("direção força: " + direction, showDebug);
        if (oppositeDirection) direction *= -1;

        rb2d.AddForce(direction, ForceMode2D.Force);
    }
}
