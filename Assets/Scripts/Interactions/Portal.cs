using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static System.TimeZoneInfo;

public class Portal : MonoBehaviour
{
    [SerializeField] private GameObject portalFx;
    [SerializeField] private CircleCollider2D myCircleCollider;
    [SerializeField] private MMF_Player transitionScreenFeedback;
    [SerializeField] private string sceneName;

    [SerializeField] private bool hasZoomTransition = true;
    [SerializeField] private float transitionTime = 1.5f;
    private SceneLoader sceneLoader;
    private CameraZoom cameraZoom;

    [SerializeField] private bool startEnabled = false;

    [SerializeField] private RunData runData;

    private void Awake()
    {
        sceneLoader = FindFirstObjectByType<SceneLoader>();
        cameraZoom = FindFirstObjectByType<CameraZoom>();


        if (startEnabled) return;
        EnablePortal(false);
    }

    public void EnablePortal(bool enable)
    {
        portalFx.SetActive(enable);
        myCircleCollider.enabled = enable;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sceneLoader.LoadSpecificScene(sceneName, transitionTime);
            
            if(hasZoomTransition)
            {
                transitionScreenFeedback.PlayFeedbacks();
                cameraZoom.Zoom(0.5f, 1);

            }

        }
    }
}
