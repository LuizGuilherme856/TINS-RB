using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BomberAI;
//[CreateAssetMenu(fileName = "new enemy Bomber", menuName = "Bomber")]
public class BomberData : EnemyData
{
    [Header("Bomber Specific")]
    //Timer Related
    //Delay after spawning during which the enemy cannot change state.
    [SerializeField] public float timeInSpawnLockState = 0.3f;
    //Time spent stationary and screaming in alert.
    [SerializeField] public float timeInAlert = 0.5f;
    //Time spent preparing to explode.
    [SerializeField] public float timeWarmUpExplosion = 1;

    //Distance Related
    [SerializeField] public float minDistToAlert = 7;

    [SerializeField] public float minDistToExplode = 2;

    //RunSpeed Related
    [SerializeField] public float runSpeedMult = 1.5f;

    [Header("Bomber Explosion Specific")]
    //Explosion related
    [SerializeField] public float damageOnEnemy = 0f;
    [SerializeField] public float impulseStrength = 10f;
    [SerializeField] public float damageOnPlayer = 1f;
    [SerializeField] public float explosionRadius = 9f;
    [SerializeField] public float impulseRadius = 9f;

    public override float GetSpeed(bool secondForm = false)
    {
        Debug.LogError("Esta sendo usado o BomberData, utilize GetSpeed(bomberStates currentState, bool secondForm = false) ao inves de somente get speed");
        return 0;
    }

    public float GetSpeed(bomberStates currentState, bool secondForm = false)
    {
        float newSpeed = base.GetSpeed();
        
        switch (currentState)
        {
            case bomberStates.seeking:
                //newSpeed = newSpeed + (newSpeed * runData.currentWaveNumber) / 135; //Increase speed as waves progress.
                break;
            case bomberStates.running:
                newSpeed = (newSpeed * runSpeedMult);//+ (newSpeed * runData.currentWaveNumber) / 135;
                break;
            default:
                newSpeed = 0; // Alert and exploding states have zero speed.
                break;
        }

        return newSpeed;

    }
}
