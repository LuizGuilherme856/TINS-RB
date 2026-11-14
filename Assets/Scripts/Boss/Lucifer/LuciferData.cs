using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[CreateAssetMenu(fileName = "LuciData", menuName = "LuciferBoss")]
public class LuciferData : EnemyData
{

    [SerializeField] public float idleWaitTime = 3f;
    [SerializeField] public float miniIdleWaitTime = 1.5f;
    //[SerializeField] public float bigIdleWaitTime = 6f;

    [Header("-----------Attack related--------")]
    [SerializeField] public int pillarMeleesToDie= 1;
    [SerializeField] public float pillarHealingPower = 200f;
    [SerializeField] public float pillarHealingTick = 200f;
    [SerializeField] public float healingFrequency = 0.05f;
    [SerializeField] public float delayBeforeHealing = 5f;
    [SerializeField] public float windDuration = 3f;
    /// <summary>
    /// Warning duration before the wind attack begins.
    /// </summary>
    public float windDelay = 3f;
    [SerializeField] public float laserEyesDuration = 3f;

    [SerializeField] public float goTo2StageDelay = 1f;
    [Header("-----------Stage 2 related--------")]
    [SerializeField] public float idleWaitTimeS2 = 3f;
    [SerializeField] public float miniIdleWaitTimeS2 = 1.5f;
   // [SerializeField] public float bigIdleWaitTimeS2 = 6f;
    [Header("-----------Attack related--------")]
    //[SerializeField] public int pillarMeleesToDieS2 = 1;
    [SerializeField] public float pillarHealingPowerS2 = 200f;
    [SerializeField] public float pillarHealingTickS2 = 200f;
    [SerializeField] public float healingFrequencyS2 = 0.05f;
    [SerializeField] public float delayBeforeHealingS2 = 5f;
    [SerializeField] public float windDurationS2 = 3f;
    [SerializeField] public float VCaosDurationS2 = 3f;
    [SerializeField] public float VPatternDurationS2 = 3f;
    [SerializeField] public float VSenoidDurationS2 = 3f;
    [SerializeField] public float delayBeforeLaserS2 = 2f;
    [SerializeField] public float cloneInitialHealth = 200f;
    [SerializeField] public int maxCloneQuantity = 2;
    [SerializeField] public float laserEyesDurationS2 = 3f;

}
