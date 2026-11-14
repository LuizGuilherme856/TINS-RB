using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Select the destination scene when starting gameplay.
/// On the first launch of the day, open the cutscene scene.
/// Otherwise, go directly to loadout.
/// </summary>
public class TriggerStartGame : MonoBehaviour
{
    [SerializeField] private string loadoutSceneName, storyTrailerSceneName;
    [SerializeField] private SceneLoader sceneLoader;
    
    private GameDataAndSettings gameDataAndSettings;

    private void Awake()
    {
        gameDataAndSettings = FindFirstObjectByType<GameDataAndSettings>();
    }


    //Called by the main-menu button.
    public void StartGame()
    {

        if(gameDataAndSettings.hasPlayedStoryIntro)
        {
            sceneLoader.LoadSpecificScene(loadoutSceneName);
        }
        else
        {
            gameDataAndSettings.hasPlayedStoryIntro = true;
            sceneLoader.LoadSpecificScene(storyTrailerSceneName);
        }

    }

}
