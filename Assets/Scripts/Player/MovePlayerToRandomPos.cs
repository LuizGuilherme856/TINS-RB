using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayerToRandomPos : MoveObjectToRandomPos
{
    public PlayerData playerData;
    public Transform firstRunPosition;

    [Header("Tutorial")]
    public float tutorialDelay = 5f;
    public PopupTutorialManager popupTutorialManager;
    public PopupTutorialData popupTutorialData;

    protected override void MoveObject()
    {
        if (playerData.runsStarted <= 1)
        {
            objToMove.position = firstRunPosition.position;
            StartCoroutine(ShowTutorial());
        }
        else
        {
            objToMove.position = possiblePositions[Random.Range(0, possiblePositions.Length)].position;

        }

    }

    private IEnumerator ShowTutorial()
    {
        yield return new WaitForSeconds(tutorialDelay);
        popupTutorialManager.ShowPopup(popupTutorialData);
    }

}
