using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayFeedbackOnTriggerEnter : MonoBehaviour
{
    [SerializeField] private string tagName;
    [SerializeField] private MMF_Player feedbacks;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag(tagName))
        {
            feedbacks.PlayFeedbacks();
        }
    }
}
