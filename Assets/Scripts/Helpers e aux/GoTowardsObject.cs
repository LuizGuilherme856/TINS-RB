using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Move toward the target after entering the trigger.
/// </summary>

public class GoTowardsObject : MonoBehaviour
{
    [SerializeField] private GameObject gameObjectToMove;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float speed = 2f;

    private bool goTowards = false;
    private GameObject target;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag(targetTag))
        {
            target = collision.gameObject;
            goTowards = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            goTowards = false;
        }
    }


    private void FixedUpdate()
    {
        if (goTowards == false) return;

        try
        {
            gameObjectToMove.transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.fixedDeltaTime );
        }
        catch (System.Exception ex)
        {
            Debug.Log("Target nulo! "+ex.Message);
            throw;
        }




    }

}

