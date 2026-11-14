using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Show only characters other than the active player character in the Hub.
//For example, hide Hanba's NPC when playing as Hanba.
public class NPC_SpawnHubArea : MonoBehaviour
{
    [SerializeField] private RunData runData;
    [SerializeField] private NPC_Name[] npcs;

    private void Start()
    {
        foreach (var npc in npcs)
        {
            if(npc.myName == runData.currentCharacter.character)
            {
                Debug.Log("desativa: " + npc.myName);
                npc.gameObject.SetActive(false);
                return;
            }
        }
    }

}
