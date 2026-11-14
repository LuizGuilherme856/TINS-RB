using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HandAI;
//[CreateAssetMenu(fileName = "new enemy JumpingHand", menuName = "Hand")]
public class HandData : JumpingEnemyData
{
    [Header("Hand Specific")]
    //Speed Multiplier
    [SerializeField] public float escapeSpeedMultiplier = 1.5f;
    //Timer Related
    //Time spent stationary and screaming in alert.
    [SerializeField] public float timeInAlert = 0.5f;
    [SerializeField] public float timeInRecover= 1f;
    //Delay after spawning during which the enemy cannot change state.
    [SerializeField] public float timeInSpawnLockState = 0.3f;

    //Distance Related
    [SerializeField] public float minDistToJump = 7;
    [SerializeField] public float limitDistToJump = 5;
    
    

    //Tolerance around the maximum jump distance.
    //[SerializeField] public float DistToJumpTolerance = 1;



    //[Header("Hand Jump Specific")]
    //Explosion related
    /*
    [SerializeField] public float damageOnEnemy = 0f;
    [SerializeField] public float impulseStrength = 10f;
    [SerializeField] public float damageOnPlayer = 1f;
    [SerializeField] public float explosionRadius = 9f;
    [SerializeField] public float impulseRadius = 9f;
    */

    public override float GetSpeed(bool secondForm = false)
    {
        Debug.LogError("Esta sendo usado o handData, utilize GetSpeed(handStates currentState, bool secondForm = false) ao inves de somente get speed");
        return 0;
    }

    public float GetSpeed(handStates currentState, bool secondForm = false)
    {
        float newSpeed = base.GetSpeed();

        
        switch (currentState)
        {
            case handStates.seeking:
                //newSpeed = newSpeed + (newSpeed * runData.currentWaveNumber) / 135; //Increase speed as waves progress.
                break;
            case handStates.jumping:
               // newSpeed = newSpeed + (newSpeed * runData.currentWaveNumber) / 135; //Increase speed as waves progress.
                break;
            //While escaping, multiply speed by the factor in HandData.
            case handStates.escaping:
                newSpeed = newSpeed * escapeSpeedMultiplier; //+ (newSpeed * runData.currentWaveNumber) / 135; //Increase speed as waves progress.
                break;
            default:
                newSpeed = 0; // Alert and recovering states have zero speed.
                break;
        }

        return newSpeed;
    }
}
