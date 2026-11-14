using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Control post-processing from the game settings.
/// </summary>
public class EnableDisablePostProcessing : MonoBehaviour
{
    [SerializeField] private Volume volume;
    private GameDataAndSettings gameData;
    private void Awake()
    {
        gameData = FindFirstObjectByType<GameDataAndSettings>();
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (gameData.postProcessing) volume.enabled = true;
        else volume.enabled = false;
    }
}
