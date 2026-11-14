using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Spawns sacrifices.
public class ManageSacrifice : OrbSpawner
{

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fillSpeed = 2f;
    
    [SerializeField] private bool startsCompleted;

    [SerializeField] private MMF_Player orbAbsorbedFeedback, sacrificeCompleteFeedback;

    private Material material;
    private float currentFillAmount =0;


    public event Action OnSacrificeComplete; 

    private void Awake()
    {
        range = GetComponent<CircleCollider2D>().radius;
        material = spriteRenderer.material;
        material.SetFloat("_FullGlowDissolveFade", currentFillAmount);


        if (startsCompleted) AutoCompleteSacrifice();
    }

    private void FixedUpdate()
    {
        if(currentFillAmount < (float)currentOrbs / (float)maxOrbs)
        {
            currentFillAmount += Time.fixedDeltaTime * fillSpeed;
            material.SetFloat("_FullGlowDissolveFade", currentFillAmount);

        }

        if((float)currentOrbs / (float)maxOrbs < 0.21 && currentOrbs > 0)
        {
            material.SetFloat("_FullGlowDissolveFade", 0.21f);
        }


    }


    public void AutoCompleteSacrifice()
    {
        fillSpeed = 10000; //Fill very quickly.
        currentOrbs = maxOrbs;
        OnSacrificeComplete?.Invoke();

    }

    public override void AddOrb()
    {

        if (currentOrbs >= maxOrbs) return; //Defensive redundancy.

        currentOrbs++;
    
        orbAbsorbedFeedback.PlayFeedbacks();

        if(currentOrbs >= maxOrbs)
        {
            SacrificeComplete();
        }

    }



    private void SacrificeComplete()
    {
        OnSacrificeComplete?.Invoke();
        sacrificeCompleteFeedback.PlayFeedbacks();
    }

}
