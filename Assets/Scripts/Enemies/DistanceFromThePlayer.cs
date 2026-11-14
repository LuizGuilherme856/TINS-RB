using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceFromThePlayer : MonoBehaviour
{
    Transform playerTransform;

    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform == null )
        {
            Debug.Log("nao achou o player pela tag. Tentando de novo");
            playerTransform = FindFirstObjectByType<PlayerDeath>().transform;
        }
    }

    public float GetDistance()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        return Mathf.Abs(distance);
    }
}
