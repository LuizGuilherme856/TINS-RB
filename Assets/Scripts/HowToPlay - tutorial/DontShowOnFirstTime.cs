using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Deactivate the GameObject on the first game launch.
/// </summary>
public class DontShowOnFirstTime : MonoBehaviour
{
    private readonly string TIMES_GAME_WAS_OPENED = "numberOfTimesThatTheGameWasOpened";

    private void Awake()
    {
        if (PlayerPrefs.HasKey(TIMES_GAME_WAS_OPENED) == false)
        {
            //This counter is not reliable if the same script is used elsewhere in the game.
            /*
            var numberOfTimes = PlayerPrefs.GetInt(TIMES_GAME_WAS_OPENED);
            numberOfTimes++;
            PlayerPrefs.SetInt(TIMES_GAME_WAS_OPENED, numberOfTimes);
            */
            gameObject.SetActive(false);

        }

    }
}
