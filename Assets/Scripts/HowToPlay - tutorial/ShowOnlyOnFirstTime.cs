using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Deactivate the GameObject if the game has been opened before.

public class ShowOnlyOnFirstTime : MonoBehaviour
{
    private readonly string TIMES_GAME_WAS_OPENED = "numberOfTimesThatTheGameWasOpened";

    private void Awake()
    {
        if (PlayerPrefs.HasKey(TIMES_GAME_WAS_OPENED))
        {
            //This counter is not reliable if the same script is used elsewhere in the game.
            /*
            var numberOfTimes = PlayerPrefs.GetInt(TIMES_GAME_WAS_OPENED);
            numberOfTimes++;
            PlayerPrefs.SetInt(TIMES_GAME_WAS_OPENED, numberOfTimes);
            */
            gameObject.SetActive(false);

        }
        else
        {
            PlayerPrefs.SetInt(TIMES_GAME_WAS_OPENED, 1);
        }
    }
 

}
