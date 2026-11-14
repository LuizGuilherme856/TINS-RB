using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechBooh;
using static LuciferBoss;

public class LuciAttacks : MonoBehaviour
{

    [SerializeField] protected LuciferData luciData;


    protected EnemyHealth luciHealth; //Assigned in the Editor.

    #region attackRelated
    [SerializeField] protected MinionSpawner minionSpawner; // Assigned in the Editor.

    [SerializeField] protected TwoStepDamageArea damageAreaScript; //Assigned in the Editor.

    [System.Serializable]
    public class LuciBossAttack
    {
        [SerializeField] private GameObject attackPrefab;
        [SerializeField] private Transform attackSpawnPoint;
        [SerializeField] private int maxAttacks = 3;
        [HideInInspector] public int currentAttacks = 0;

        public GameObject AttackPrefab => attackPrefab;
        public Transform AttackSpawnPoint => attackSpawnPoint;
        public int MaxAttacks => maxAttacks;
    }

    [SerializeField] public LuciBossAttack followBulletAttack; // Assigned in the Editor.

    [SerializeField] protected List<GameObject> healPillars;

    [SerializeField] protected int activePillars = 0;
    #endregion

    [SerializeField] protected bool healingCoRoutineActive = false;

    [SerializeField] protected GameObject WindAttackRight;
    [SerializeField] protected GameObject WindAttackLeft;
    public GameObject windWarningLeft, windWarningRight; //Assigned in the Editor.



    //Shoot Laser Related
    private bool isShootingLaser = false;
    public Animator[] eyeShotAnimators;
    public ObjectShooter laserLeftEye, laserRightEye;
    protected float laserEyesTimer = 0f;

    [SerializeField] protected GameObject luci2Stage; //Assigned in the Editor.

    private void Awake()
    {

        luciHealth = Try.GetComponent<EnemyHealth>(gameObject);

        luci2Stage.gameObject.SetActive(false);
    }

    public void Update()
    {
        if (isShootingLaser)
        { 
            laserEyesTimer  += Time.deltaTime;
            if (luciData.laserEyesDuration <= laserEyesTimer)
            {
                laserEyesTimer = 0;
                isShootingLaser = false;
            }
        }
    }

    #region TriggerByAnimation
    public void SpawnMinions_Animation()
    {
        StartCoroutine(minionSpawner.SpawnEnemies());
    }


    public void SpawnFollowBullet_AnimationEvent()
    {
        var projectileBall = Instantiate(followBulletAttack.AttackPrefab, followBulletAttack.AttackSpawnPoint.position, Quaternion.identity);
        projectileBall.SetActive(true);
        followBulletAttack.currentAttacks++;
    }

    public void ActivateHealPillars_AnimationEvent()
    {

        foreach (GameObject pillar in healPillars)
        {
            pillar.SetActive(true);
            HealPillar target = pillar.GetComponent<HealPillar>();
            target.ResetPillar();
            target.OnDestroyPillar += PillarDestroyed;
        }
        activePillars = healPillars.Count;
        if (!healingCoRoutineActive)
        {
            StartCoroutine(HealPillarAttack_coroutine());
        }
    }

    public void ActivateGas_AnimationEvent()
    {
        damageAreaScript.gameObject.SetActive(true);
        damageAreaScript.ActivateArea();
    }

    public void ExplodeGas_AnimationEvent()
    {
        damageAreaScript.TriggerDamage_AnimationEvent();
    }

    public void WindAttackLeft_AnimationEvent()
    {
        StartCoroutine(WindAttackLeft_coroutine());
    }

    public void WindAttackRight_AnimationEvent()
    {
        StartCoroutine(WindAttackRight_coroutine());
    }

    public void StartShootingLaser_Animation()
    {
        
        laserEyesTimer = 0;
        if (!isShootingLaser)
        {
            StartCoroutine(ShootLaser_Routine());
        }

    }




    #endregion
    protected IEnumerator ShootLaser_Routine(float delayBetweenShots = 0.4f)
    {
        isShootingLaser = true;
        while (isShootingLaser)
        {

            foreach (var anim in eyeShotAnimators) anim.SetTrigger("Shoot");

            yield return new WaitForSeconds(0.1f);

            laserLeftEye.LaunchObject();
            laserRightEye.LaunchObject();

            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    protected IEnumerator WindAttackLeft_coroutine()
    { 

        windWarningLeft.SetActive(true); // Enable the left wind warning.
        yield return new WaitForSeconds(luciData.windDelay); // Wait for the configured delay before attacking.
        windWarningLeft.SetActive(false); // Disable the left wind warning.

        WindAttackLeft.SetActive(true);
        yield return new WaitForSeconds(luciData.windDuration);
        WindAttackLeft.SetActive(false);

    }

    protected IEnumerator WindAttackRight_coroutine()
    {
        windWarningRight.SetActive(true); // Enable the right wind warning.
        yield return new WaitForSeconds(luciData.windDelay); // Wait for the configured delay before attacking.
        windWarningRight.SetActive(false); // Disable the right wind warning.

        WindAttackRight.SetActive(true);
        yield return new WaitForSeconds(luciData.windDuration);
        WindAttackRight.SetActive(false);

    }



    protected IEnumerator HealPillarAttack_coroutine()
    {
        healingCoRoutineActive = true;
        //Healing starts after the delay defined in luciData.
        yield return new WaitForSeconds(luciData.delayBeforeHealing);

        while (IsPillarActive())
        {

            ActivatePillarsLineRender();
            if (luciHealth.CanHeal())
            {

                float totalHealing = luciData.pillarHealingPower * activePillars;
                luciHealth.HealOverTime(luciData.pillarHealingTick, luciData.healingFrequency, totalHealing);
                float reApplyHealTime = (totalHealing / luciData.pillarHealingTick) * luciData.healingFrequency; //See Miro for more details.

                yield return new WaitForSeconds(reApplyHealTime); // Add a delay here to make movement occur in short hops.

            }
            else
            {
                yield return null;
            }
        }

        healingCoRoutineActive = false;
    }

    protected void ActivatePillarsLineRender()
    {
        foreach (GameObject pillar in healPillars)
        {

            LineRenderer lineRenderObj = pillar.GetComponent<LineRenderer>();
            lineRenderObj.enabled = true;
            // Configure LineRenderer.
            lineRenderObj.positionCount = 2;
            lineRenderObj.SetPosition(1, this.gameObject.transform.position);
        }
    }

    /// <summary>
    /// Unsubscribe the observer to prevent stale callbacks and wasted processing.
    /// </summary>
    public void PillarDestroyed()
    {
        activePillars -= 1;
        foreach (GameObject pillar in healPillars)
        {
            if (!pillar.activeInHierarchy)
            {
                pillar.GetComponent<HealPillar>().OnDestroyPillar -= PillarDestroyed;
            }

        }
    }



    protected void ResetHealingRoutines()
    {

        StopCoroutine(HealPillarAttack_coroutine());
        luciHealth.StopAllCoroutines();
    }

    protected bool IsPillarActive()
    {
        foreach (GameObject pillar in healPillars)
        {
            if (pillar.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }



    /// <summary>
    /// Deactivate all attack objects on this GameObject.
    /// </summary>
    public void EndBossFight()
    {
        

        foreach (GameObject pillar in healPillars)
        {
            pillar.SetActive(false);
        }
        WindAttackRight.SetActive(false);

        WindAttackLeft.SetActive(false);

        StopCoroutine(HealPillarAttack_coroutine());

        damageAreaScript.gameObject.SetActive(false);

        isShootingLaser = false;
        laserEyesTimer = 0;

        
    }



}
