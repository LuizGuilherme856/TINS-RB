using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorExplosions : MonoBehaviour
{

    [SerializeField] private Transform[] explosionsTransforms;
    [SerializeField] private MMF_Player randomPosExplosionFeedback, fixedPosExplosionFeedback;
    [SerializeField] private Transform linearExplosionFirstPos;
    [SerializeField] private FloorExplosionData data;
    [SerializeField] private BossNextForm bossNextForm;

    [SerializeField] private bool autoTriggerExplosion = false; //Must be triggered manually.
    
    private int  numberOfExplosions = 0;
    private int currentForm = 0; // Change the attack type for each boss form.
    private MMF_InstantiateObject randomInstantiate, fixedPosInstantitate;
    private Transform fixedPosTransform;
    private floorExplosionMode explosionMode = floorExplosionMode.none;
    private float explosionRateTimer, timeBetweenExplosionTriggersTimer;
    private Transform playerTransform; 
    

    private void Awake()
    {
        randomInstantiate = randomPosExplosionFeedback.GetFeedbackOfType<MMF_InstantiateObject>();
        //fixedPosInstantitate = fixedPosExplosionFeedback.GetFeedbackOfType<MMF_InstantiateObject>();
        fixedPosTransform = fixedPosExplosionFeedback.transform;
        explosionRateTimer = data.explosionRate[currentForm];
        timeBetweenExplosionTriggersTimer = data.timeBetweenExplosionTriggers[currentForm];
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void OnEnable()
    {
        bossNextForm.OnChangeForm += () => currentForm = bossNextForm.currentForm;


    }


    private void Update()
    {
        if (explosionMode == floorExplosionMode.none)
        {
            CheckAndTriggerExplosion();

            return;
        }

        explosionRateTimer -= Time.deltaTime;

        if(explosionRateTimer <= 0)
        {
            explosionRateTimer = data.explosionRate[currentForm];
            TriggerExplosion();

        }
    }

    private void CheckAndTriggerExplosion()
    {
        if(autoTriggerExplosion == false) return;

        timeBetweenExplosionTriggersTimer -= Time.deltaTime;
        if (timeBetweenExplosionTriggersTimer <= 0)
        {
            var randomNumber = Random.Range(0f, 1f);
            if (randomNumber < 0.33f)
            {
                explosionMode = floorExplosionMode.linearExplosion;
            }
            else if (randomNumber <= 1f)
            {
                explosionMode = floorExplosionMode.sinExplosion;
            }
            else
            {
                explosionMode = floorExplosionMode.randomExplosion;
            }

            timeBetweenExplosionTriggersTimer = data.timeBetweenExplosionTriggers[currentForm];
        }
    }

    private void TriggerExplosion()
    {
        if (numberOfExplosions >= data.maxExplosions[currentForm])
        {
            numberOfExplosions = 0;
            explosionMode = floorExplosionMode.none;
            return;
        }

        switch (explosionMode)
        {
            case floorExplosionMode.linearExplosion:
                 var linearPos = linearExplosionFirstPos.position + data.linearOffSet[currentForm] * numberOfExplosions;
                fixedPosTransform.position = linearPos;
                break;
            case floorExplosionMode.sinExplosion:
                SinPatternExplosion();
                break;
            case floorExplosionMode.randomExplosion:
                TriggerRandomExplosion();
                break;
            case floorExplosionMode.followPlayer:
                fixedPosTransform.position = playerTransform.position;
                break;

        }

        fixedPosExplosionFeedback.PlayFeedbacks();
        numberOfExplosions++;
    }

    private void SinPatternExplosion()
    {
        var pos = linearExplosionFirstPos.position + new Vector3(data.linearOffSet[currentForm].x * numberOfExplosions, Mathf.Sin(data.sinOffset[currentForm] * numberOfExplosions), 0);
        if (pos.x >= data.limitPos.x) //Wrap around after reaching the limit.
        {
            fixedPosTransform.position -= new Vector3(data.linearOffSet[currentForm].x, Mathf.Sin(data.sinOffset[currentForm] * numberOfExplosions), 0);
        }
        else
        {
            fixedPosTransform.position = pos;
        }
    }

    private void TriggerRandomExplosion()
    {
        if (numberOfExplosions >= data.maxExplosions[currentForm])
        {
            numberOfExplosions = 0;
            explosionMode = floorExplosionMode.none;
            return;
        }

        randomPosExplosionFeedback.PlayFeedbacks();
        numberOfExplosions++;

    }

    public void TriggerExplosionMode( floorExplosionMode explosionMode) //Trigger the start immediately.
    {
        this.explosionMode = explosionMode;
    }

    
    public enum floorExplosionMode {none, linearExplosion, sinExplosion, randomExplosion, followPlayer }
}
