using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Make the player intangible by switching to a collider that only hits obstacles.
public class SetIntangible : MonoBehaviour
{
    //[SerializeField] private LayerMask intangibleLayerMask;

    [SerializeField] private GameObject playerGameObject;
    [SerializeField] private MMF_Player activateIntangibleFeedback;
    [SerializeField] private MMF_Player deactivateIntangibleFeedback;
    [SerializeField] private MMF_Player timeRunningOutFeedback;
    private Material material;
    private LayerMask defaultLayer;
    private Health health;
    private bool isIntangibleActive;

    private void Awake()
    {
        material = playerGameObject.GetComponent<SpriteRenderer>().material;
        health = playerGameObject.GetComponent<Health>();


        material.SetFloat("_HologramFade", 0);
        playerGameObject.layer = LayerMask.NameToLayer("Player");
        isIntangibleActive = false;
    }
    public void ActivateIntangible(float duration)
    {
        // Cancel earlier scheduled calls when reactivating the ability
        // so the previous expiration cannot end the new activation early.
        CancelInvoke();

        material.SetFloat("_HologramFade", 1);
        playerGameObject.layer = LayerMask.NameToLayer("IntangiblePlayer"); //This layer does not collide with enemies.

        // Previously, Hanba's ability only changed collision layers.
        // Some enemy attacks apply damage directly or through area checks,
        // so also apply the ability duration to the health invulnerability timer.
        if (health != null)
        {
            health.SetInvulnerableTime(duration);
        }

        // The special-attack flow uses this state to prevent spending another charge
        // while the ability is active.
        isIntangibleActive = true;

        activateIntangibleFeedback.PlayFeedbacks();

        Invoke(nameof(TimeIsRunningOut), duration*0.6f);
        Invoke(nameof(DeactivateIntangible), duration);
    }

    public void TimeIsRunningOut()
    {
        timeRunningOutFeedback.PlayFeedbacks();
    }

    private void DeactivateIntangible()
    {
        deactivateIntangibleFeedback.PlayFeedbacks();
        material.SetFloat("_HologramFade", 0);
        isIntangibleActive = false;

        // Restore the normal player layer when the ability ends.
        // Invulnerability expires through the timer started on activation.
        playerGameObject.layer = LayerMask.NameToLayer("Player");
    }

    public bool IsIntangibleActive()
    {
        return isIntangibleActive;
    }
}
