using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class FeelTest : MonoBehaviour
{
    public MMFeedbacks feedback;


    private void Start()
    {
        feedback.PlayFeedbacks();
    }
}
