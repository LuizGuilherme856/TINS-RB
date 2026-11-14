using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Play HUD point sounds and shake to indicate insufficient points.
/// Component wrapper so this behavior can be found by type.
/// </summary>
public class NotEnoughPointsFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player feedbacks;

    public void TriggerNotEnoughPointsFeedbacks()
    {
        feedbacks.PlayFeedbacks();
    }

}
