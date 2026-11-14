using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUp : MonoBehaviour
{
    [SerializeField] private float speed = 1f;

    private void FixedUpdate()
    {
        transform.position += Vector3.one * speed * Time.fixedDeltaTime;
    }

}
