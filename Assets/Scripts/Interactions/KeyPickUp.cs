using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickUp : MonoBehaviour
{
    [SerializeField] private MMF_Player feedbacks;
    [SerializeField] private CircleCollider2D circleCollider2D;
    [SerializeField] private LockedDoor lockedDoor;

    private bool hasBeenCollected = false;

    public event Action OnPowerUpPickUp;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasBeenCollected) return;

        if (collision.CompareTag("Player"))
        {
            OnPowerUpPickUp?.Invoke();

            hasBeenCollected = true;
            UnlockDoor();
            feedbacks.PlayFeedbacks();
            
            gameObject.SetActive(false);
            

        }
    }

    private void UnlockDoor()
    {
        lockedDoor.canOpen = true;
    }
}
