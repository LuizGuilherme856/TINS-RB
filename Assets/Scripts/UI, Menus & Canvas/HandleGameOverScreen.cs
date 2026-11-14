using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleGameOverScreen : MonoBehaviour
{

    [SerializeField] private MMF_Player MMF_Player;

    [SerializeField] private bool isStartScreen = false;

    private void OnEnable()
    {
        if(isStartScreen)
            MMF_Player.PlayFeedbacks();
    }





}
