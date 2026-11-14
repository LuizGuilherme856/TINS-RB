using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NecromancerAI;

//[CreateAssetMenu(fileName = "new enemy Shooter", menuName = "NecromancerEnemy")]
public class NecromancerData : EnemyData
{
    [Header("Necromancer Specific")]
    //Speed Multiplier
    [SerializeField] public float escapeSpeedMultiplier = 1.5f;
    //Timer Related
    //Time spent stationary and screaming in alert.
    [SerializeField] public float timeInAlert = 0.5f;
    //Delay after spawning during which the enemy cannot change state.
    [SerializeField] public float timeInSpawnLockState = 0.3f;
    //Cooldown after firing during which aiming and shooting are disabled.
    [SerializeField] public float timeInCooldown = 0.7f;

    //Distance Related
    [SerializeField] public float minDistToAim = 7;
    [SerializeField] public float limitDistToAim = 5;
    [Header("Range of distance of spawn (with interpolation)")]
    [SerializeField][Range(0, 1)] public float minDisOfSpawn = 0.4f;
    [SerializeField][Range(0, 1)] public float limitDisOfSpawn = 0.6f;


    public override float GetSpeed(bool secondForm = false)
    {
        Debug.LogError("Esta sendo usado o NecromancerData, utilize GetSpeed(skeletonStages currentState, bool secondForm = false) ao inves de somente get speed");
        return 0;
    }

    public float GetSpeed(NecromancerStates currentState, bool secondForm = false)
    {
        float newSpeed = base.GetSpeed();
    
        switch (currentState)
        {
            case NecromancerStates.seeking:
                //newSpeed = newSpeed + (newSpeed * runData.currentWaveNumber) / 135; //Increase speed as waves progress.
                break;
            //While escaping, multiply speed by the factor in ShooterData.
            case NecromancerStates.escaping:
                newSpeed = newSpeed * escapeSpeedMultiplier; //+ (newSpeed * runData.currentWaveNumber) / 135; //Increase speed as waves progress.
                break;
            default:
                newSpeed = 0f; // Alert and firing states.
                break;
        }

        return newSpeed;
    }
}
