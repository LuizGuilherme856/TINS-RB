using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Change the followed target; defaults to the player.
//Currently follows the player; intended to support decoys as well.

public class ChangeTarget : MonoBehaviour
{
    private EnemyAI enemyAI;
    private EnemyMovementAnimation enemyMovementAnimation;
    private MoveTowardsPlayer moveTowardsPlayer;
   

    private Transform playerTransform, currentTarget;
    

    public Transform CurrentTarget => enemyAI != null ? enemyAI.GetCurrentTargetTransform() :
        moveTowardsPlayer != null ? moveTowardsPlayer.CurrentTarget : currentTarget;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        enemyMovementAnimation = GetComponent<EnemyMovementAnimation>();
        moveTowardsPlayer = GetComponent<MoveTowardsPlayer>();

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }


    public void SetNewTargetFor(Transform newTarget, float duration)
    {
        SetTargetToAllClasses(newTarget);
        Invoke(nameof(SetPlayerAsTarget), duration);
    }

    public void SetNewTarget(Transform newTarget)
    {
        SetTargetToAllClasses(newTarget);
    }

    public void SetPlayerAsTarget()
    {
        SetTargetToAllClasses(playerTransform);
    }

    private void SetTargetToAllClasses(Transform target)
    {
        currentTarget = target;
        enemyAI?.SetTarget(target);
        enemyMovementAnimation?.SetTarget(target);
        moveTowardsPlayer?.SetTarget(target);
    }

    /*
    public bool IsTargetingPlayer() //Check whether another decoy is already being followed.
    {
        //if(currentTarget== null) return true;

        if (currentTarget.CompareTag("Player")) return true;
        else return false;
    }
    */
    
}
