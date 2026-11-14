using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Spawn the duck with the dialogue appropriate to
//whether the player completed the tutorial.

public class DuckDialogueHub : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] GameObject preTutorialDuck, postTutorialDuck;

    private void Awake()
    {
        if(playerData.isTutorialFinished)
        {
            Destroy(preTutorialDuck);
        }
        else
        {
            Destroy(postTutorialDuck);
        }
    }
}
