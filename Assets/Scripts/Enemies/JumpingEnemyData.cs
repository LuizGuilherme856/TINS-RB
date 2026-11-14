using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingEnemyData : EnemyData
{
    [Header("Jumping Enemy Specific")]
    //Leap Realated
    [SerializeField] public float maxTimeLeaping = 2;
    //Some enemies, such as the hand, may force this to match another inherited value.
    [Tooltip("Em alguns casos (por exemplo com o inimigo da mão), pode ser forçada a se igualar a outra variavel de uma classe(SO) filho")]
    [SerializeField] public float maxLeapingDistance = 7;
    [SerializeField] public float midPointScaleMul = 1.5f;
    [Header("ObjectOnHead And ScreenEffect Specific")]
    [Tooltip("Caso useHealth o inimigo na Cabeça Do Player morrerá com dano,\n" +
        " caso contrario será usado calculo de tempo")]
    public bool usesHealth = true;
    [Tooltip("Booleano usado para controlar se o inimigo pode ir na cabeça do player ou não")]
    public bool neverGrab = false;
    [Tooltip("caso não use vida, quanto tempo o effeito e o objeto devem ficar ativos na cabeça/tela do player")]
    public float timeInHead = 2f;

}
