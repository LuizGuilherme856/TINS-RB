using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FloorExplosions;
using static LuciferBoss;

public class LuciStage2Attacks : MonoBehaviour
{

    [SerializeField] protected LuciferData luciData;


    protected EnemyHealth luciHealth; 

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

    //Assigned in the Editor.
    [SerializeField] protected GameObject WindAttackRight;
    [SerializeField] protected GameObject WindAttackLeft;
    [SerializeField] protected GameObject VCaos;
    [SerializeField] protected GameObject VPattern;
    [SerializeField] protected GameObject VSenoid;
    [SerializeField] protected GameObject Spinning;
    [SerializeField] private FloorExplosions randomFloorExplosions;
    [SerializeField] private FloorExplosions followFloorExplosions;
    [SerializeField] private List<LuciClone> luciClones;

    [Header("Clone Stuff")]
    public List<GameObject> fakeClones;
    public float cloneSpawnDuration = 1f;


    //Shoot Laser Related
    private bool isShootingLaser = false;
    public Animator[] eyeShotAnimators;
    public ObjectShooter laserEye;
    protected float laserEyesTimer = 0f;
    private void Awake()
    {

        try
        {
            luciHealth = this.gameObject.GetComponent<EnemyHealth>();
        }
        catch (Exception)
        {
            Debug.LogError("Lucifer não possui um script EnemyHealth");
        }
        //Detach the object so it no longer follows Lucifer.
        Spinning.transform.parent = null;
        randomFloorExplosions.gameObject.transform.parent = null;
        followFloorExplosions.gameObject.transform.parent = null;
    }

    public void Update()
    {
        if (isShootingLaser)
        {
            laserEyesTimer += Time.deltaTime;
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
        //ResetHealingRoutines();
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

    public void VCaos_AnimationEvent() 
    {
        StartCoroutine(VCaos_coroutine());
    }

    public void VPattern_AnimationEvent()
    {
        StartCoroutine(VPattern_coroutine());
    }

    public void VSenoid_AnimationEvent()
    {
        StartCoroutine(VSenoid_coroutine());
    }

    public void SpinningLaser_AnimationEvent()
    {
        Spinning.SetActive(true);
    }

    public void TriggerRandomFloorExplosions_AnimationEvent(floorExplosionMode mode) 
    {
        randomFloorExplosions.TriggerExplosionMode(mode);
    }

    public void TriggerFollowloorExplosions_AnimationEvent(floorExplosionMode mode)
    {
        followFloorExplosions.TriggerExplosionMode(mode);
    }

    public void ActivateClones_AnimationEvent()
    {
        StartCoroutine(ActivateClones_Routine());
    }

    private IEnumerator ActivateClones_Routine()
    {

        foreach (var fakeClone in fakeClones)
        {
            fakeClone.SetActive(true);
            fakeClone.GetComponent<Animator>().SetTrigger("Spawn");
        }


        //Activate stand-in clones for the spawn animation.
        yield return new WaitForSeconds(cloneSpawnDuration);

        //Activate the actual clones after the spawn animation.
        //and deactivate the stand-in clones.

        foreach (var fakeClone in fakeClones) fakeClone.SetActive(false);
        

        if (HasHealthToClone())
        {
            foreach (LuciClone targetClone in luciClones)
            {
                EnemyHealth targetHealth = targetClone.gameObject.GetComponent<EnemyHealth>();
                if (!targetClone.gameObject.activeInHierarchy)
                {
                    targetClone.gameObject.SetActive(true);

                    targetHealth.SetMaxHealth(luciData.cloneInitialHealth);
                    targetHealth.ReplenishHealth();
                    luciHealth.SetDamage(luciData.cloneInitialHealth);
                }
                else
                {
                    //targetHealth.ReplenishHealth(); //Optionally heal an already active object.
                }

            }
        }
        else
        {
            foreach (LuciClone targetClone in luciClones)
            {
                targetClone.gameObject.SetActive(true);
                EnemyHealth targetHealth = targetClone.gameObject.GetComponent<EnemyHealth>();
                targetHealth.SetMaxHealth(1);
                targetHealth.ReplenishHealth();
            }
        }
    }

    public void StartShootingLaser_Animation()
    {

        laserEyesTimer = 0;
        if (!isShootingLaser)
        {
            StartCoroutine(ShootLaser_Routine(0.2f));
        }

    }

    #endregion

    #region CoRoutines
    protected IEnumerator WindAttackLeft_coroutine()
    { 

        WindAttackLeft.SetActive(true);
        yield return new WaitForSeconds(luciData.windDurationS2);
        WindAttackLeft.SetActive(false);

    }

    protected IEnumerator WindAttackRight_coroutine()
    {

        WindAttackRight.SetActive(true);
        yield return new WaitForSeconds(luciData.windDurationS2);
        WindAttackRight.SetActive(false);

    }



    protected IEnumerator HealPillarAttack_coroutine()
    {
        healingCoRoutineActive = true;
        //Healing starts after the delay defined in luciData.
        yield return new WaitForSeconds(luciData.delayBeforeHealingS2);

        while (IsPillarActive())
        {

            ActivatePillarsLineRender();
            if (luciHealth.CanHeal())
            {

                float totalHealing = luciData.pillarHealingPowerS2 * activePillars;
                luciHealth.HealOverTime(luciData.pillarHealingTickS2, luciData.healingFrequencyS2, totalHealing);
                float reApplyHealTime = (totalHealing / luciData.pillarHealingTickS2) * luciData.healingFrequencyS2; //See Miro for more details.

                yield return new WaitForSeconds(reApplyHealTime); // Add a delay here to make movement occur in short hops.

            }
            else
            {
                yield return null;
            }
        }

        healingCoRoutineActive = false;
    }
    protected IEnumerator VCaos_coroutine()
    {

        VCaos.SetActive(true);
        yield return new WaitForSeconds(luciData.VCaosDurationS2);
        VCaos.SetActive(false);

    }

    protected IEnumerator VPattern_coroutine()
    {

        VPattern.SetActive(true);
        yield return new WaitForSeconds(luciData.VPatternDurationS2);
        VPattern.SetActive(false);

    }

    protected IEnumerator VSenoid_coroutine()
    {

        VSenoid.SetActive(true);
        yield return new WaitForSeconds(luciData.VSenoidDurationS2);
        VSenoid.SetActive(false);

    }
    #endregion

    private IEnumerator ShootLaser_Routine(float delayBetweenShots = 0.4f)
    {
        isShootingLaser = true;
        while (isShootingLaser)
        {

            foreach (var anim in eyeShotAnimators) anim.SetTrigger("Shoot");

            yield return new WaitForSeconds(0.1f);

            laserEye.LaunchObject();
            //laserRightEye.LaunchObject();

            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    protected bool HasHealthToClone() 
    {
        if (luciHealth.currentHealth > 2 * luciData.cloneInitialHealth)
        {
            return true;
        }
        else 
        {
            return false;
        }
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

        foreach (LuciClone targetClone in luciClones)
        {
            targetClone.gameObject.SetActive(false);
        }
        WindAttackRight.SetActive(false);

        WindAttackLeft.SetActive(false);

        StopCoroutine(HealPillarAttack_coroutine());

        damageAreaScript.gameObject.SetActive(false);

        Spinning.GetComponent<ManagerSpinningLaser>().DeactivateAndReset();

        isShootingLaser = false;
        laserEyesTimer = 0;

        VCaos.SetActive(false);
        VPattern.SetActive(false);
        VSenoid.SetActive(false);

        this.gameObject.SetActive(false);
    }

}
