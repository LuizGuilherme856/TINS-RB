using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UnityEvent/Inspector entry point for saving. Search scene and prefab callbacks as well as C# callers
// before renaming SaveGameToDisk or changing when progress is committed.
public class SaveGame : MonoBehaviour
{

    [SerializeField] private PlayerData playerData;

    public void SaveGameToDisk()
    {
        Debug.Log("Saving progress...");
        SaveSystem.SaveData(playerData);
        Debug.Log("Progress saved");
    }
}
