using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Show the tutorial popup when the player enters the area.
/// </summary>
public class PopUpTutorialTrigger : MonoBehaviour
{
    public PopupTutorialManager popupTutorialManager;
    public PopupTutorialData popupTutorialData;
    public bool showOnlyOnce = true;

    private bool alreadyShown = false;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (alreadyShown && showOnlyOnce) return;

        if (collision.CompareTag("Player"))
        {
            popupTutorialManager.ShowPopup(popupTutorialData);
            alreadyShown = true;
        }
    }


}
