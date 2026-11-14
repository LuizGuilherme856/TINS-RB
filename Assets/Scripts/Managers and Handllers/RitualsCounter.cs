using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
/// <summary>
/// Count completed rituals on the map.
/// Older code calls these rituals sacrifices.
/// </summary>
public class RitualsCounter : MonoBehaviour
{
    //[SerializeField] int sacrificesNeeded = 2;
    //Originally, rune purchases required a minimum number of completed rituals.
    [field: SerializeField] private int numberOfSacrificesNeeded_forRunes { get; set; }
    public LockedDoor lockedDoor;
    public int sacrificesCompleted { get; private set; }

    private void Awake()
    {
        Debug.Log("game obj:" +gameObject.name);

        var sacrifices = FindObjectsByType<ManageBloodRitual>(FindObjectsSortMode.None);

        foreach (var sacrifice in sacrifices)
        {
            sacrifice.OnCompleted += SacrificeCompleted;
        }  
    }

    public int GetSacrificesNeeded_forRunes()
    {
        if (numberOfSacrificesNeeded_forRunes > 0) return numberOfSacrificesNeeded_forRunes;
        else return 0;
    }

    private void SacrificeCompleted()
    {
        numberOfSacrificesNeeded_forRunes--;
        sacrificesCompleted++;

        if(numberOfSacrificesNeeded_forRunes == 0)
        {
            lockedDoor.canOpen = true;
        }
    }

    /*
    public bool AllSacrificesCompleted()
    {
        if(numberOfSacrificesCompleted>= sacrificesNeeded) return true;
        else return false;
    }
    */

}
