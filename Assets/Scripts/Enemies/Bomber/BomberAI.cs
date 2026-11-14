using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BomberAI : EnemyAI
{
    protected bomberStates state = bomberStates.seeking;
    protected BomberData bomberData;
    [SerializeField] protected GameObject bomberWithExplosion;

    [SerializeField] protected EnemyHealth bomberHealth;
    //Timer Related
    [SerializeField] private float currentAlertTimer = 0;

    [SerializeField] private float currentExplosionTimer = 0;

    [SerializeField] private float currentSpawnLockStateTimer = 0;

    [SerializeField] private MMF_Player alertFeedback, screamingFeedback;

    public event Action OnChangeState;


    protected override void Awake()
    {
        base.Awake();
        try
        {
           bomberData = (BomberData)enemyData;
        }
        catch (Exception)
        {
            Debug.LogError("enemyData não é uma instancia de BomberData! Coloque um BomberData na referencia 'Enemy Data'");
        }
    }

    protected override void OnEnable()
    {
        state = bomberStates.seeking;
        currentSpawnLockStateTimer = 0;
        currentAlertTimer = 0;
        currentExplosionTimer = 0;
        OnChangeState?.Invoke();
        base.OnEnable();

        OnChangeState += StateChangeFeedback;
    }

    protected override void Update()
    {
        base.Update();
         currentExplosionTimer += Time.deltaTime;
        if (state == bomberStates.seeking)
        {
            currentSpawnLockStateTimer += Time.deltaTime;
        }
        else if(state == bomberStates.alert)
        {
            currentAlertTimer += Time.deltaTime;
        }
        else if (state == bomberStates.exploding)
        {
            currentExplosionTimer += Time.deltaTime;
        }

    }
    protected override void FixedUpdate()
    {
        if (currentStatusEffect == StatusEffect.Frozen) return;

        switch (state)
        {
            //Seek while outside distToAlert; enter alert once close enough.
            case bomberStates.seeking:
                //Debug.LogWarning("Seeking!");
                //bomberBeat.FeedbacksList[0].SetFeedbackDuration(0.25f);
                //If
                if (currentSpawnLockStateTimer >= bomberData.timeInSpawnLockState)
                {
                    //Enter alert when distance is at most distToAlert.
                    if (Vector2.Distance(transform.position, target.position) < bomberData.minDistToAlert)
                    {
                        //Initialize the alert timer.
                        currentAlertTimer = 0;
                        state = bomberStates.alert;
                        OnChangeState?.Invoke();
                    }
                }
                break;
            //Remain still until timeInAlert elapses, then switch speed and enter running.
            case bomberStates.alert:
                //Debug.LogWarning("Alert!");
               
                
                if (currentAlertTimer >= bomberData.timeInAlert)
                {
                   state = bomberStates.running;
                   OnChangeState?.Invoke();
                }
                break;
            //Run until distance is at most disToExplode.
            case bomberStates.running:
                //Debug.LogWarning("Running!");
                if (Vector2.Distance(transform.position, target.position) < bomberData.minDistToExplode)
                {
                    //Initialize the alert timer.
                    currentExplosionTimer = 0;
                    state = bomberStates.exploding;
                    OnChangeState?.Invoke();
                }
                break;
            //Return the bomber to the pool, then enable its explosion sprite and prefab.
            case bomberStates.exploding:
                //Debug.LogWarning("Explodindo!");

                if (currentExplosionTimer >= bomberData.timeWarmUpExplosion)
                {
                    GameObject targetCorpse =  Instantiate(bomberWithExplosion, this.transform.position, this.transform.rotation);
                    
                    try
                    {
                        //Configure explosion values from BomberData.
                        //Values stored on the explosion.
                        Explosion explosionScript = targetCorpse.GetComponent<Explosion>();
                        explosionScript.SetExplosionRadius(bomberData.explosionRadius);
                        explosionScript.SetImpulseRadius(bomberData.impulseRadius);
                        //Values stored in explosiveData.
                        ExplosiveData targetExplosiveData =  explosionScript.GetExplosionData();
                        targetExplosiveData.damage = bomberData.damageOnEnemy;
                        targetExplosiveData.impulseStrength = bomberData.impulseStrength;
                        targetExplosiveData.damageOnPlayer = bomberData.damageOnPlayer;
                        //Trigger the explosion.
                        explosionScript.TriggerExplosion();

                    }
                    catch (Exception)
                    {
                        Debug.LogError("BomberCorpse não tem uma explosão (script Explosion), verifique o prefab BomberCorpse'");
                    }

                    
                    bomberHealth.InstaDieWithNoCorpse();
                    this.gameObject.SetActive(false);
                }
                break;
            default:
                break;
        }


        //MoveAndTarget();
        base.FixedUpdate();
 
    }

    private void OnDisable()
    {
        OnChangeState -= StateChangeFeedback;
    }

    /// <summary>
    /// Override because the base FixedUpdate used the wrong GetSpeed implementation for state-based movement.
    /// </summary>
    protected void MoveAndTarget()
    {
        if (path == null) return;

        if (currentStatusEffect == StatusEffect.Frozen) return;

        if (isNPC && Vector3.Distance(target.position, transform.position) < 1) return; //Stop when the target is reached.

        if (currentWayPoint >= path.vectorPath.Count) //The end of the path was reached.
        {
            //reachedEndOfPath = true;
            //return;
        }
        else
        {
            desiredDirection = ((Vector2)path.vectorPath[currentWayPoint] - myRigidbody.position).normalized;
            //Debug.Log(desiredDirection);


            walkSpeed =  GetWalkSpeed();

            Vector2 force = desiredDirection * walkSpeed * Time.fixedDeltaTime;

            myRigidbody.AddForce(force, ForceMode2D.Force);


            float distance = Vector2.Distance(myRigidbody.position, path.vectorPath[currentWayPoint]);

            if (distance < nextWaypointDistance)
            {
                currentWayPoint++;
            }
        }

    }

    protected override float GetWalkSpeed()
    {
        if (Vector2.Distance(transform.position, target.position) < enemyData.minDistanceToIncreaseSpeed)
        {
            return bomberData.GetSpeed(state);

        }
        else
        {
            return bomberData.GetSpeed(state) * enemyData.speedMultiplierWhenNotVisible;
        }
    }

    public bomberStates GetCurrentState()
    {
        return state;
    }

    private void StateChangeFeedback()
    {
        switch (state)
        {
            case bomberStates.seeking:
                break;
            case bomberStates.alert:
                alertFeedback.PlayFeedbacks();
                break;
            case bomberStates.running:
                break;
            case bomberStates.exploding:
                screamingFeedback.PlayFeedbacks();
                break;
            default:
                break;
        }
    }
    public enum bomberStates { seeking, alert, running, exploding}
}
