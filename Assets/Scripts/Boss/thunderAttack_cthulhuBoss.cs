using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FloorExplosions;

public class thunderAttack_cthulhuBoss : MonoBehaviour
{
    [SerializeField] private MMF_Player randomPosExplosionFeedback;
    [SerializeField] private int maxThunders = 20;
    [SerializeField] private float thunderRate = 1f; //Interval between lightning strikes.
    [SerializeField] private Transform playerTransform;
    public event Action OnStopAttacking;
    //[SerializeField] Vector2 minPos, maxPos;

    private MMF_InstantiateObject instantiateObject_mmf;
    private int numberOfExplosions =0;
    private float thunderTimer = 0;
    private bool callThunder = false;

    private void Awake()
    {
        instantiateObject_mmf = randomPosExplosionFeedback.GetFeedbackOfType<MMF_InstantiateObject>();
        instantiateObject_mmf.ParentTransform = GameObject.FindGameObjectWithTag("Manager").transform; //Assign a parent here to avoid hierarchy-related issues.
        instantiateObject_mmf.TargetTransform = GameObject.FindGameObjectWithTag("Manager").transform;

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void OnEnable()
    {
        thunderTimer = thunderRate;
    }

    void FixedUpdate()
    {
        if (callThunder == false) return;

        thunderTimer -= Time.fixedDeltaTime;
        if(thunderTimer <= 0)
        {
            TriggerRandomExplosion();
            thunderTimer = thunderRate;
        }
    }


    private void TriggerRandomExplosion()
    {
        if (numberOfExplosions >= maxThunders)
        {
            OnStopAttacking?.Invoke();
            numberOfExplosions = 0;
            return;
        }

        if(playerTransform)
        {
            //Update playback position to the boss's position.
            instantiateObject_mmf.TargetPosition = playerTransform.position;
        }
        else
        {
            Debug.Log("     player transform nao encontrado... colocando no transform do boss");
            instantiateObject_mmf.TargetPosition = playerTransform.position;
        }



        randomPosExplosionFeedback.PlayFeedbacks();
        numberOfExplosions++;

    }

    public void SetCallThunder(bool callThunder)
    {
        this.callThunder = callThunder;
    }
}
