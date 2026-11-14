using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestCathedral : MonoBehaviour
{
    [SerializeField] MMF_Player moveFeedbacks, disappearFeedbacks;
    [SerializeField] private float delayToDisappear = 3f;
    [SerializeField] GameObject portalFX;

    private Animator myAnimator;
    private bool hasPlayed = false;

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasPlayed) return;

        if(collision.CompareTag("Player"))
        {
            hasPlayed = true;
            moveFeedbacks.PlayFeedbacks();

            Invoke(nameof(Disappear), delayToDisappear);
        }
    }

    private void Disappear()
    {
        myAnimator.SetTrigger("Disappear");
        disappearFeedbacks.PlayFeedbacks();
        Destroy(portalFX, 2f);
        Destroy(gameObject, 3f);
    }

}
