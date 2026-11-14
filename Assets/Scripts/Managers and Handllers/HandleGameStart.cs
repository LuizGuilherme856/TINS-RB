using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Choose the tutorial or the regular game entry point.
/// </summary>
public class HandleGameStart : MonoBehaviour
{
    public PlayerData playerData; //Player progression ScriptableObject reference.
    public SceneLoader sceneLoader; //Scene-loading component reference.

    public void StartGame()
    {
        try
        {
            if (playerData.isTutorialFinished)
            {
                sceneLoader.LoadSpecificScene("The Void - HUB");
            }
            else
            {
                sceneLoader.LoadSpecificScene("Intro - Hell 2.0");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Erro ao carregar a cena: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
