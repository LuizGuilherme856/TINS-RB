using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateVolumeOnPlayerDistance : MonoBehaviour
{

    [SerializeField] float updateRate = 0.2f;

    [SerializeField] float minDistance = 3f, maxDistance = 20f;

    [SerializeField] private MMF_Player[] feedbackPlayer;
    private List<MMF_Sound> soundMMFList = new List<MMF_Sound>();

    private Transform playerTransform;
    [SerializeField] [Range(0,1)] private float defaultVolume = 0.8f;


    private void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        foreach (var feedback in feedbackPlayer)
        {
            soundMMFList.Add(feedback.GetFeedbackOfType<MMF_Sound>());
        }

        


    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(UpdateVolume), 0, updateRate);
    }



    private void UpdateVolume()
    {
        float playerDistance = Mathf.Abs( Vector2.Distance(transform.position, playerTransform.position));

        //Do nothing if the player is already far away and volume is zero.
        if (playerDistance > maxDistance && soundMMFList[0].MinVolume <= 0)
        {
            return;
        }

        if (playerDistance < minDistance)
        {
            SetVolume(defaultVolume);
        }
        else if(playerDistance > maxDistance)
        {
            SetVolume(0);
        }
        else
        {
            float newVolume = defaultVolume * (1 - ( playerDistance - minDistance) / maxDistance);

            if (newVolume < 0) newVolume = 0;
            else if (newVolume > 1) newVolume = 1;

            SetVolume(newVolume);
        }  

    }

    private void SetVolume(float volume)
    {
        foreach (var soundMMF in soundMMFList)
        {
            soundMMF.MinVolume = volume;
            soundMMF.MaxVolume = volume;
        }

    }
}
