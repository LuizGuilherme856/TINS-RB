using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 Reset jumping enemies that have separate grounded and airborne objects.
Restore the grounded enemy here.
 */
public class JumpingEnemyManager : MonoBehaviour
{
    [SerializeField] protected GameObject baseEnemy; // Grounded version that walks through the level.
    
    [SerializeField] protected GameObject jumpingEnemy;// Airborne version that follows the jump trajectory.

    [SerializeField] protected bool resetPosInDeath = false; 

    protected void Awake()
    {
        if (!resetPosInDeath)
        {
            baseEnemy.GetComponent<ILander>().OnPrepareToDie += ResetJumpingManeger;
            jumpingEnemy.GetComponent<JumpingEnemy>().OnPrepareToDie += ResetJumpingManeger;
        }
        else 
        {
            baseEnemy.GetComponent<ILander>().OnPrepareToDie += ResetJumpingManegerToASpot;
            jumpingEnemy.GetComponent<JumpingEnemy>().OnPrepareToDie += ResetJumpingManegerToASpot;
        }

        
    }

    protected void OnEnable()
    {
        baseEnemy.SetActive(true);
        jumpingEnemy.SetActive(false);
    }



    protected void ResetJumpingManeger()
    {
        this.gameObject.SetActive(false);
    }
    protected void ResetJumpingManegerToASpot()
    {
        this.gameObject.SetActive(false);
        baseEnemy.transform.position = this.gameObject.transform.position;
        jumpingEnemy.transform.position = this.gameObject.transform.position;
        //TODO: Consider resetting status effects on child objects here.
    }
    
}
