using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechBooh;
using MoreMountains.Feedbacks;

public class PlayAnimation : MonoBehaviour
{
    [SerializeField] private string triggerName = "Start";
    [SerializeField] private float delay = 1.0f;
    [SerializeField] private MMF_Player animationPlayFeedback;

    private Animator animator;

    void Awake()
    {
        animator = Try.GetComponent<Animator>(gameObject);
    }

    void OnEnable()
    {
        StartCoroutine(ActivateTriggerAfterDelay_Routine());
    }

    private IEnumerator ActivateTriggerAfterDelay_Routine()
    {
        yield return new WaitForSeconds(delay);

        if (animationPlayFeedback != null)
        {
            animationPlayFeedback.PlayFeedbacks();
        }
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }
}
