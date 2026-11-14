using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Push nearby enemies away.
/// </summary>
public class ForceField : MonoBehaviour
{
    public float pushForce = 10f;
    public Transform centerTransform;

    void OnTriggerEnter2D(Collider2D collision)
    {
        var obj = collision.gameObject;

        if (obj.CompareTag("Enemy"))
        {

            Vector3 direction = obj.transform.position - centerTransform.position;
            obj.GetComponent<Rigidbody2D>().AddForce(direction * pushForce, ForceMode2D.Impulse);

        }
    }
}
