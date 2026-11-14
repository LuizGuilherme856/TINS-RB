using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Animation-event adapter for playing feedbacks.
public class PlayMMF_PlayerFeedbacks : MonoBehaviour
{

    [SerializeField] private MMF_Player[] feedbacks;


    public void PlayFeedbacks(int index)
    {
        feedbacks[index].PlayFeedbacks();
    }
}
