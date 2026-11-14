using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LuciferStage2Boss;

public class LuciClone : LuciferStage2Boss
{
    [SerializeField] protected LuciferStage2Boss mainLuci; // Assigned in the Editor.
    [SerializeField] protected GameObject Spinning;
    public GameObject fakeClone;

    protected override void Awake()
    {
        try
        {
            animator = GetComponent<Animator>();
        }
        catch (Exception)
        {

            Debug.LogError("Clone Lucifer não possui um animator");
        }
    
        try
        {
            luciAttacksScript = this.gameObject.GetComponent<LuciStage2Attacks>();
        }
        catch (Exception)
        {
            Debug.LogError("Clone Lucifer não possui um script luciAttack");
        }



        try
        {
            luciHealth = this.gameObject.GetComponent<EnemyHealth>();
            luciHealth.OnZeroHealth += CloneDeath;

        }
        catch (Exception)
        {
            Debug.LogError("Clone Lucifer não possui um script EnemyHealth");
        }
    }



    protected void OnEnable()
    {
        //Subscribe to Lucifer's actions when enabled.
        try
        {
            mainLuci.CloneCopyState += OnCloneAttack;

        }
        catch (System.Exception)
        {
            Debug.LogError("Clone Lucifer nao tem referencia a main luci");
        }
    }
    protected override void Start()
    { 
        
    }




    protected  IEnumerator ChangeState()
    {
        switch (mainLuci.GetCurrentState())
        {
            default:
                yield return StartCoroutine(IdleState());
                break;
            case LuciferS2.shoot:
                yield return StartCoroutine(ShootState());
                break;
            case LuciferS2.shootLaser:
                yield return StartCoroutine(ShootLaserState());
                break;
            case LuciferS2.laser:
                yield return StartCoroutine(LaserState());
                break;
        }
    }

    protected void OnCloneAttack()
    {
        StartCoroutine(ChangeState());
    }
    

    public void CloneDeath()
    {
        fakeClone.SetActive(true);
        fakeClone.GetComponent<Animator>().SetTrigger("Death");



        Spinning.GetComponent<ManagerSpinningLaser>().DeactivateAndReset();
        //Unsubscribe when disabled to avoid stale callbacks and unnecessary processing.
        mainLuci.CloneCopyState -= OnCloneAttack;

        this.gameObject.SetActive(false);
    }
    
}
