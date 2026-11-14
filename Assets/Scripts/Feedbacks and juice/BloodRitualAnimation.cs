using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodRitualAnimation : MonoBehaviour
{
    [SerializeField] private ManageBloodRitual manageBloodRitual;
    [SerializeField] private float leakDelay = 2f;
    [SerializeField] private GameObject objToSpawn;
    [Space(20)]
    [SerializeField] private MMF_Player blobBurstFeedback;
    [SerializeField] private MMF_Player leakingFeedback, instantiateFeedback;
    [SerializeField] private MMF_Player heartBeatA_Feedback, heartbeatB_Feedback;
   
    /// <summary>
    /// Blood-fill percentage thresholds.
    /// </summary>

    [SerializeField][Range(0, 1)] 
    private float smallLimit, mediumLimit, largeLimit, hugeLimit;
    private float previousCurrentOrbs =0;
    private int maxOrbs;
    private bool completed = false;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        maxOrbs = manageBloodRitual.maxOrbs;
        manageBloodRitual.OnAbsorbOrb += UpdateAnimationState;
        manageBloodRitual.OnCompleted += TriggerLeakAnimation;
    }

    private void UpdateAnimationState()
    {
        if (completed) return;

        float currentOrbPercentage = (float)manageBloodRitual.currentOrbs / (float)maxOrbs;

        if (currentOrbPercentage >= hugeLimit)
        {
            //Completely filled.
            PlayClip("blood full loop");
            completed = true;

        }
        else if (currentOrbPercentage >= largeLimit)
        {
            //Switch to huge.
            PlayClip("Huge blood loop");
        }
        else if (currentOrbPercentage >= mediumLimit)
        {
            //Switch to large.
            PlayClip("large blood loop");
        }
        else if (currentOrbPercentage >= smallLimit)
        {
            //Switch to medium.
            PlayClip("medium blood loop");
        }
        else if (currentOrbPercentage > 0)
        {
            PlayClip("small blood loop");
        }

        previousCurrentOrbs = manageBloodRitual.currentOrbs;

    }

    private void TriggerLeakAnimation()
    {
        completed = true;
        PlayClip("blood full loop");
        Invoke(nameof(PlayLeakAnimation), leakDelay);
    }

    private void PlayLeakAnimation()
    {
        PlayClip("blood leak");
    }

    //Called by the Animator.
    public void PlayHeartBeatFeedback(int index = 0)
    {
        if(index == 0)
        {
            heartBeatA_Feedback.PlayFeedbacks();
        }
        else
        {
            heartbeatB_Feedback.PlayFeedbacks();
        }
    }

    /// <summary>
    /// Called by the Animator.
    /// </summary>
    public void InstantiateTombStone()
    {
        objToSpawn.SetActive(true);
        instantiateFeedback.PlayFeedbacks();
    }

    public void PlayLeakingFeedback()
    {
        leakingFeedback.PlayFeedbacks();
    }

    public void PlayBlobBurstFeedback()
    {
        blobBurstFeedback.PlayFeedbacks();
    }


    private void PlayClip(string clipName)
    {
        if(clipName.Equals(GetCurrentAnimationClipName()) == false)
        {
            animator.Play(clipName);
        }
    }

    private string GetCurrentAnimationClipName()
    {
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            return clipInfo[0].clip.name;
        }
        return "No Animation Playing";
    }
}
