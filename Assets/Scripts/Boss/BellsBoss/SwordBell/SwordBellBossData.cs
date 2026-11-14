using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SwordBellBossAI;
//[CreateAssetMenu(fileName = "new enemy bell", menuName = "SwordBellBoss")]
public class SwordBellBossData : EnemyData
{
    [Header(" SpearBellEspecific ")]
    // [Tooltip("Move faster toward the firing position")]
    [SerializeField] public float minimumTimeOnState = 0.1f;

    //[Tooltip("Tempo entre spawn de ovos no momento do egg Attack")]
    //[SerializeField] public float eggSpawnRate = 1f;

    [Tooltip("Para correr mais rapido pro local de atirar")]
    [SerializeField] public float ringSpeedMultiplier = 1.5f;

    //Spin Related
    [SerializeField] public float pathUpdateInSpin = 1f;
    [SerializeField] public float spinSpeedMultiplier = 1.5f;

    [Tooltip("Maximo de ataques seguidos em relação a Forma")]
    [SerializeField][Range(0, 50)] public List<int> maxAttackWithoutIdle;

    [Tooltip("Tempo minimo entre stuns seguidos em relação a Forma")]
    [SerializeField][Range(0, 50)] public List<float> stunCoolDown;

    [Serializable]
    public struct Range
    {
        [SerializeField][Range(0, 50)] public List<float> Ranges;
    }
    [Serializable]
    public struct StatesAndWeight
    {
        //State.
        public SwordBellStates state;

        //Attack probability weight for this boss form.
        [Tooltip("Peso da probabilidade desse estado em relação a forma do boss")]
        [SerializeField][Range(0, 50)] public List<int> WeightsPerForm;
        [Tooltip("Tempo de antecipação em relação a forma do boss")]
        [SerializeField][Range(0, 50)] public List<float> AntecipationTimePerForm;
        //The following matrix defines ranges for each form, for example:
        /*     Start     End
         form1 [  0    ,   5 ]
         form2 [  2    ,   7 ]
         form3 [  4    ,   9 ]
         */

        [Tooltip("Lista abaixo diz os ranges de acordo com a forma exemplo:\r\n                Inicio    Fim \r\n         form1 [  0    ,   5 ]\r\n         form2 [  2    ,   7 ]\r\n         form3 [  4    ,   9 ]")]
        [SerializeField] public List<Range> RangesPerForm;
        [Tooltip("Distancia para executar o ataque desse estado em relação a forma do boss")] // Some attacks do not require a distance calculation.
        [SerializeField][Range(0, 50)] public List<int> DistancePerForm;

        [Tooltip("Se sim pode ser stunnado durante o ataque quando o inimigo estiver em alert")]
        [SerializeField] public bool canBeStunned;
        //When enabled, limits count iterations; otherwise, they measure time.
        // When enabled, call AddIterationToAttack at the end of the animation.
        [SerializeField] public bool usesIterationCount;
    }

    [Serializable]
    public struct BulletStatsPerForm
    {
        [Tooltip("Status Da Bala em relação a forma do boss")]
        [SerializeField][Range(0, 50)] public List<int> bulletsPerShot, projectileSpeed, multipleBulletAngleOffset;

    }

    [Header("Bullets Related")]
    [SerializeField] public BulletStatsPerForm bulletStatsPerForm = new BulletStatsPerForm();

    [Header("Attack Structs")]
    //StatesAndWeights structures for each state.
    [SerializeField] public StatesAndWeight idle = new StatesAndWeight();
    [SerializeField] public StatesAndWeight spin = new StatesAndWeight();
    [SerializeField] public StatesAndWeight shoot = new StatesAndWeight();
    [SerializeField] public StatesAndWeight ring = new StatesAndWeight();
    [SerializeField] public StatesAndWeight stun = new StatesAndWeight();

    public override float GetSpeed(bool secondForm = false)
    {
        Debug.LogError("Esta sendo usado o SpearBellBossData, utilize GetSpeed(PsicoPatoBoss currentState, bool secondForm = false) ao inves de somente get speed");
        return 0;
    }

    public float GetSpeed(SwordBellStates currentState, SwordBellStates nextAttackState, bool secondForm = false)
    {

        float newSpeed = base.GetSpeed();
        switch (currentState)
        {
            case SwordBellStates.seeking:
                //The only state with nonzero movement speed.
                break;
            case SwordBellStates.spin:
                newSpeed *= spinSpeedMultiplier;
                break;
            default:
                newSpeed = 0f; // Stationary attack states.
                break;
        }

        return newSpeed;
    }
}
