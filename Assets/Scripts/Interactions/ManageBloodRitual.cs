using MoreMountains.Feedbacks;
using System;
using UnityEngine;

public class ManageBloodRitual : OrbSpawner
{
    [SerializeField] private bool startsCompleted;

    [SerializeField] private MMF_Player orbAbsorbedFeedback, sacrificeCompleteFeedback;

    public event Action OnCompleted, OnAbsorbOrb;

    /// <summary>
    /// Use a separate transform because the sprite center may differ from the ritual center.
    /// </summary>
    [field: SerializeField] public Transform ritualCenter { get; private set; }

    private void Awake()
    {
        range = GetComponent<CircleCollider2D>().radius;


        if (startsCompleted) AutoCompleteRitual();
    }

    public void AutoCompleteRitual()
    {
        currentOrbs = maxOrbs;
        OnCompleted?.Invoke();

    }

    public override void AddOrb()
    {

        if (currentOrbs >= maxOrbs) return; //Defensive redundancy.

        OnAbsorbOrb?.Invoke();
        currentOrbs++;

        orbAbsorbedFeedback.PlayFeedbacks();

        if (currentOrbs >= maxOrbs)
        {
            RitualComplete();
        }

    }

    public bool IsRitualComplete()
    {
        if (currentOrbs >= maxOrbs)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void RitualComplete()
    {

        //Instantiate(runeTombstone, runeSpawnPosition.position, Quaternion.identity);

        OnCompleted?.Invoke();
        sacrificeCompleteFeedback.PlayFeedbacks();
    }


}
