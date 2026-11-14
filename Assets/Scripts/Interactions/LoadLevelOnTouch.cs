using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevelOnTouch : MonoBehaviour
{
    [SerializeField] private string levelNameToLoad;
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private bool resetRunData = false;
    [SerializeField] private HandleStartNewRun handleStartNewRun;
    [SerializeField] private PlayFeedbacks transitionFX;

    [SerializeField] private SceneLoader sceneLoader;
    private bool loading = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (loading) return;
        if (collision.CompareTag("Player"))
        {
            if (resetRunData) handleStartNewRun.StartNewRun();
            sceneLoader.LoadSpecificScene(levelNameToLoad, transitionTime);
            transitionFX.PlayAllFeedbacks();
            loading = true;
        }
    }
}
