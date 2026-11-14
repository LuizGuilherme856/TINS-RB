using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [SerializeField] private Vector2 mousePosMultiplier;
    [SerializeField] private Vector2 playerMaxDistance;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private Camera myCamera;
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        myCamera = GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        // transform.position = Vector3.Lerp(transform.position, playerTransform.position, moveSpeed);

        Vector3 mousePosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
        //mousePosition *= mousePosMultiplier;
        var newPos = new Vector3(playerTransform.position.x+mousePosition.x, playerTransform.position.y+mousePosition.y, transform.position.z);

        ///Known limitation: works near the origin but becomes incorrect farther from (0,0).
        ///TODO: Revise camera-to-world position conversion.

        rb.MovePosition(newPos);
    }




    /*
     *         //Vector3 mousePosition = Input.mousePosition;
        Vector2 distanceToPlayer = transform.position - playerTransform.position;
        Vector3 mousePosition = myCamera.ScreenToWorldPoint(Input.mousePosition);

        if(Mathf.Abs( distanceToPlayer.x) < playerMaxDistance.x)
        {
            var xPosition = Mathf.Lerp(transform.position.x, mousePosition.x, moveSpeed);
            //var position = Vector2.Lerp(transform.position, mousePosition, moveSpeed);

            rb.MovePosition(new Vector2(xPosition, transform.position.y));
        }
        else
        {
            rb.MovePosition(new Vector2(playerTransform.position.x + playerMaxDistance.x, transform.position.y));
        }

        if ( Mathf.Abs( distanceToPlayer.y )< playerMaxDistance.y)
        {
            var yPosition = Mathf.Lerp(transform.position.y, mousePosition.y, moveSpeed);
            //var position = Vector2.Lerp(transform.position, mousePosition, moveSpeed);

            rb.MovePosition(new  Vector2(transform.position.x, yPosition));
        }
        else
        {
            rb.MovePosition(new Vector2(transform.position.x, playerTransform.position.y + playerMaxDistance.y));
        }
    */
}
