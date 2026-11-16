using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreTabHandler : MonoBehaviour
{
    [Header("Referências das abas")]
    [SerializeField] private GameObject globalTab;
    [SerializeField] private GameObject localTab;
    [SerializeField] private GameObject friendsTab;

    // Optionally enforce the initial state.
    private void Start()
    {
        // For example, open the local tab by default.
         ShowLocalTab();
    }

    // Public helpers to close individual tabs.
    public void CloseGlobalTab()
    {
        if (globalTab != null) globalTab.SetActive(false);
    }

    public void CloseLocalTab()
    {
        if (localTab != null) localTab.SetActive(false);
    }

    public void CloseFriendsTab()
    {
        if (friendsTab != null) friendsTab.SetActive(false);
    }

    // Enable one tab and disable all others.
    private void ShowTab(GameObject tabToShow)
    {
        if (globalTab != null) globalTab.SetActive(globalTab == tabToShow);
        if (localTab != null) localTab.SetActive(localTab == tabToShow);
        if (friendsTab != null) friendsTab.SetActive(friendsTab == tabToShow);
    }

    // Public wrappers for Button.onClick assignment.
    public void ShowGlobalTab()
    {
        ShowLocalTab();
    }

    public void ShowLocalTab()
    {
        ShowTab(localTab);
    }

    public void ShowFriendsTab()
    {
        ShowLocalTab();
    }
}
