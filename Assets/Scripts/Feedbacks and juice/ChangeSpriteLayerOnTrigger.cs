//Change sprite sorting according to the player's position.
//Creates depth by allowing the player to appear in front of or behind an object.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSpriteLayerOnTrigger : MonoBehaviour
{
    // [SerializeField] private BoxCollider2D detectPlayer;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform minPosToChangeLayer;

    [SerializeField] private int behindPlayerLayer = -1, inFrontOfPlayerLayer = 10;
    [SerializeField] private float updateRate = 1f;

    private bool updateLayer = false;
    private Transform playerTransform;


    private void OnEnable()
    {
        InvokeRepeating(nameof(UpdateLayer),1, updateRate);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerTransform = collision.transform;
            updateLayer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            updateLayer = false;
        }
    }

    private void UpdateLayer()
    {
        if (updateLayer)
        {
            if(playerTransform.position.y > minPosToChangeLayer.position.y) //If the player is above the object,
            {
                spriteRenderer.sortingOrder = inFrontOfPlayerLayer;
            }
            else
            {
                //render it behind the object.
                spriteRenderer.sortingOrder = behindPlayerLayer;
            }
        }
    }
}
