using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// After Gael dies, Lucifer appears to direct the player toward Heaven.
/// </summary>
public class LuciferOnHellManager : MonoBehaviour
{
    public float delayToStartLuciferDialogue = 3f; // Delay before starting Lucifer's dialogue after Gael's death
    [Space(10)]
    public ObjMoveData objMoveData; // Data for moving the camera to the Lucifer NPC position
    public GameObject portal;
    public LuciferNpc luciferNpc;
    public EnemyHealth gaelHealth; // Gael's Health reference.
    public MoveObjectToPos moveObjectToPos; // Camera movement controller reference.

    private void OnEnable()
    {
        // Subscribe to Gael's death event.
        gaelHealth.OnZeroHealth += TriggerLuciferDialogue;
    }

    private void Start()
    {
        // Keep LuciferNpc inactive initially.
        luciferNpc.gameObject.SetActive(false);
    }

    private void TriggerLuciferDialogue()
    {
        StartCoroutine(LuciferDialogue_Routine());

    }


    private IEnumerator LuciferDialogue_Routine()
    {
        yield return new WaitForSeconds(delayToStartLuciferDialogue); // Wait for the configured delay before starting dialogue.

        // Activate LuciferNpc when Gael dies.
        luciferNpc.gameObject.SetActive(true);
        // Wait until the camera reaches Lucifer before starting dialogue.
        
        //TODO: Investigate the unresolved error in this sequence.
        //moveObjectToPos.StartMove(0);
        yield return new WaitForSeconds(2);
        // Start Lucifer's dialogue.
        luciferNpc.StartDialogue();

        // Wait for dialogue to finish.
        yield return new WaitUntil(() => luciferNpc.dialogueEnded);

        portal.SetActive(true); // Activate the portal after Lucifer's dialogue.

        yield return new WaitForSeconds(1f); // Wait one second before restoring player movement.

    }

}

