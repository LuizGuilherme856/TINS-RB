using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Helper methods used by boss animations.
/// </summary>
public class GiantBossAnimationHelper : MonoBehaviour
{

    public SpriteRenderer[] rightHandAndArm; // References to the boss's right-hand objects.
    public GameObject righWarning; // Reference to the right-hand attack warning.
    public GameObject leftWarning;
    public SpriteRenderer[] leftHandAndArm; // References to the boss's left-hand objects.
    public SpriteRenderer[] handsAndArmsSprites; // References to the boss's hand objects.

    private Animator animator; // Animator reference.

    private readonly string DEFAULT_LAYER = "Default", PLAYER_LAYER = "Player";

    private void Awake()
    {
        animator = GetComponent<Animator>(); // Get the Animator component.
    }

    public void HideRightHand() => ShowOrHideObjects(rightHandAndArm, false);
    public void ShowRightHand() => ShowOrHideObjects(rightHandAndArm, true);
    public void ShowLeftHand() => ShowOrHideObjects(leftHandAndArm, true);
    public void HideLeftHand() => ShowOrHideObjects(leftHandAndArm, false);
    public void HideBothHands() => ShowOrHideObjects(handsAndArmsSprites, false);
    public void ShowBothHands() => ShowOrHideObjects(handsAndArmsSprites, true);

    /// <summary>
    /// Change the layer and disable the collider when the boss's hands move
    /// above or below the arena.
    /// </summary>
    private void ShowOrHideObjects(SpriteRenderer[] objs, bool show)
    {
        for (int i = 0; i < objs.Length; i++)
        {
            if (show)
            {
                objs[i].sortingLayerName = PLAYER_LAYER;
            }
            else
            {
                objs[i].sortingLayerName = DEFAULT_LAYER;
            }
            objs[i].GetComponent<PolygonCollider2D>().enabled = show;
        }
    }


    public void ShowRightExplosionWarning_AnimationEvent()
    {
        StartCoroutine(HandleWarning(righWarning));
    }

    public void ShowLeftExplosionWarning_AnimationEvent()
    {
        StartCoroutine(HandleWarning(leftWarning));
    }

    /// <summary>
    /// Controls attack-warning flashing.
    /// </summary>
    private IEnumerator HandleWarning(GameObject warningObject)
    {
        int count = 0;

        while (count < 3)
        {
            warningObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);

            warningObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            count++;
        }

        count = 0;
        while (count < 5)
        {
            warningObject.SetActive(true);
            yield return new WaitForSeconds(0.05f);

            warningObject.SetActive(false);
            yield return new WaitForSeconds(0.05f);
            count++;
        }
        warningObject.SetActive(false);
    }
}
