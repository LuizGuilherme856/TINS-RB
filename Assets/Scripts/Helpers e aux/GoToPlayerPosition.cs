using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToPlayerPosition : MonoBehaviour
{

    [SerializeField] private Transform playerTransform;


    private void FixedUpdate()
    {
        transform.position = playerTransform.position;
    }

}
