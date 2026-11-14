using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveHornsOnFirstTime : MonoBehaviour
{
    private readonly string TIMES_GAME_WAS_OPENED = "firstTimeHorns";
    [SerializeField] PlayerData playerData;

    private void Awake()
    {
        if (PlayerPrefs.HasKey(TIMES_GAME_WAS_OPENED))
        {
            if(playerData.horns < 10)
            {
                playerData.AddHorns( 10);
            }
        }
        else
        {
            PlayerPrefs.SetInt(TIMES_GAME_WAS_OPENED, 1);
        }
    }
}
