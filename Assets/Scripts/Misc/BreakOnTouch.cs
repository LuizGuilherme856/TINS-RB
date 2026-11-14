using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(IsVisible))]
public class BreakOnTouch : MonoBehaviour
{
    [SerializeField] private bool changeSpriteLayer = true;
    [SerializeField] private BreakType breakType = BreakType.animation;

    [SerializeField] private BoxCollider2D myCollider;
    [SerializeField] private Animator myAnimator;
    [SerializeField] private MMF_Player breakFeedbacks;
    [SerializeField] private SpriteRenderer mySpriteRenderer;
    [SerializeField] private GameObject particle;
    [SerializeField] LayerMask collisionLayer;

    [SerializeField] private bool playFeedbacksWhileInvisible = false;

    [Header("Power Up drop")]
    [SerializeField] private GameObject[] spawnDrop;
    [SerializeField] private float dropSpawnChance = 0.02f;

    private IsVisible isVisible;

    public Action OnBreak;
    private void Awake()
    {
        if (playFeedbacksWhileInvisible == false)
        {
            isVisible = GetComponent<IsVisible>();
        }

    }

    private void OnEnable()
    {
        //Workaround for vases respawning with an intact appearance.
        //If the collider is already disabled, restore the broken visual state.
        if(myCollider.enabled == false)
        {
            BreakVisualFx();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collisionLayer.value & 1 << collision.gameObject.layer) > 0)

            Break();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collisionLayer.value & 1 << collision.gameObject.layer) > 0)
            Break();
    }

    public void Break()
    {
        if (changeSpriteLayer)
        {
            mySpriteRenderer.sortingLayerName = "Default";
            mySpriteRenderer.sortingOrder = 7;
        }

        //Verify that at least one spawn candidate exists.
        if (spawnDrop.Length > 0)
        {
            float number = UnityEngine.Random.Range(0, 100);

            if (number <= dropSpawnChance)
            {
                Instantiate(spawnDrop[UnityEngine.Random.Range(0, spawnDrop.Length)], transform.position, transform.rotation);
            }
        }


        if (playFeedbacksWhileInvisible == true)
        {
            breakFeedbacks.PlayFeedbacks();
        }
        else
        {
            if (isVisible.GetIsVisible() == true)
            {
                breakFeedbacks.PlayFeedbacks();
            }
        }
        OnBreak?.Invoke();
        BreakVisualFx();

    }

    private void BreakVisualFx()
    {
        switch (breakType)
        {
            case BreakType.animation:
                myCollider.enabled = false;
                myAnimator.SetTrigger("Break");
                break;
            case BreakType.particleFx:
                Instantiate(particle, transform.position, Quaternion.identity);
                Destroy(gameObject);
                //Debug.Log("Destroy breakable obj");
                break;
            default:
                break;
        }
    }

    private enum BreakType { animation, particleFx}
}

