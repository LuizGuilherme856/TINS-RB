using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class SkipVideoManager : MonoBehaviour
{

    [SerializeField] private VideoClip videoClip;

    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private MMF_Player playFadeOutFeedbacks;
    
    [SerializeField] private string sceneName;

    private float videoDuration;

    private bool skipHasBeenTriggered = false; //Activate only once.


    private void Awake()
    {
        playerInputHandler.OnEscape += SkipVideo;
        playerInputHandler.OnInteract += SkipVideo;
        playerInputHandler.OnReload += SkipVideo;


        videoDuration = (float)videoClip.length;

        Invoke(nameof(SkipVideo), videoDuration - 0.6f);
    }

    private void SkipVideo()
    {
        if(skipHasBeenTriggered) return;

        Debug.Log("Try to load scene: " + sceneName);
        playFadeOutFeedbacks.PlayFeedbacks();
        sceneLoader.LoadSpecificScene(sceneName);

        skipHasBeenTriggered = true;
    }
}
