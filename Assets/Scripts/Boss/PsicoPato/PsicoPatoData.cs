using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PsicoPatoBoss;

//[CreateAssetMenu(fileName = "new enemy Psicopato", menuName = "PsicopatoEnemy")]
public class PsicoPatoData : EnemyData
{
    [Header(" PsicoPatoEspecific ")]
    // [Tooltip("Move faster toward the firing position")]
    [SerializeField] public float minimumTimeOnState = 0.1f;
    [SerializeField] public float dashSpeedMultiplier = 0.0f;
    [SerializeField][Range(0, 10000)] public List<float> dashImpulseStrength;
    [Tooltip("X= Tempo maximo de dash" +
        "\r\n  Momento do impulse= x - dashImpulseTiming")]
    [SerializeField][Range(0, 50)] public List<float> dashImpulseTiming;

    [Tooltip("Tempo entre spawn de ovos no momento do egg Attack")]
    [SerializeField] public float eggSpawnRate = 1f;

    [Tooltip("Para correr mais rapido pro local de atirar")]
    [SerializeField] public float shootSpeedMultiplier = 1.5f;

    [Serializable]
    public struct Range
    {
        [SerializeField][Range(0, 50)] public List<float> Ranges;
    }
    [Serializable]
    public struct StatesAndWeight
    {
        //State.
        public PsicoPatoStates state;

        //Attack probability weight for this boss form.
        [Tooltip("Peso da probabilidade desse estado em relação a forma do boss")]
        [SerializeField][Range(0, 50)] public List<int> WeightsPerForm;

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
        //When enabled, limits count iterations; otherwise, they measure time.
        // When enabled, call AddIterationToAttack at the end of the animation.
        [SerializeField] public bool usesIterationCount;
        [SerializeField] public bool copyFirstRangeToAll;
        public void CopyFirstRangeToAll()
        {
            if (copyFirstRangeToAll)
            {
                for (int i = 1; i < RangesPerForm.Count; i++)
                {
                    RangesPerForm[i] = RangesPerForm[0];
                }
            }
        }
    }

    

    [Header("Attack Structs")]
    //StatesAndWeights structures for each state.
    [SerializeField] public StatesAndWeight idle = new StatesAndWeight();
    [SerializeField] public StatesAndWeight laser = new StatesAndWeight();
    [SerializeField] public StatesAndWeight dash = new StatesAndWeight();
    [SerializeField] public StatesAndWeight egg = new StatesAndWeight();
    [SerializeField] public StatesAndWeight shoot = new StatesAndWeight();
    [SerializeField] public StatesAndWeight scream = new StatesAndWeight();

    public override float GetSpeed(bool secondForm = false)
    {
        Debug.LogError("Esta sendo usado o PsicoPatoData, utilize GetSpeed(PsicoPatoBoss currentState, bool secondForm = false) ao inves de somente get speed");
        return 0;
    }

    public float GetSpeed(PsicoPatoStates currentState, PsicoPatoStates nextAttackState, bool secondForm = false)
    {
        
        float newSpeed= base.GetSpeed();
        

        switch (currentState)
        {
            case PsicoPatoStates.seeking:
                if (nextAttackState == PsicoPatoStates.shoot)
                {
                    newSpeed = newSpeed*shootSpeedMultiplier; //Increase speed if the next attack is shoot.
                }

                break;
            //While escaping, multiply speed by the factor in ShooterData.
            case PsicoPatoStates.dash:
                newSpeed = newSpeed * dashSpeedMultiplier; //Increase speed as waves progress.
                break;
            default:
                newSpeed = 0f; // Stationary attack states.
                break;
        }

        return newSpeed;
    }
}
