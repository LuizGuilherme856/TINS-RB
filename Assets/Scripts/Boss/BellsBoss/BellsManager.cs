using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public class BellsManager : MonoBehaviour
{
    public bool testChangeForm,testRing = false;

    public float speedIncreaseMod = 1.1f;

    public event Action OnChangeForm, OnTimeToRing;

    [SerializeField]  protected EnemyHealth targethealBell; 

    [Tooltip("Tempo entre rings em relação a Forma")]
    [SerializeField][Range(0, 50)] public List<float> ringCooldownPerForm; // Assigned in the Editor.
    //TODO: Track how many sibling bells remain alive
    //and strengthen the survivors when another bell dies.
    [SerializeField] public List<GameObject> bells; // Assigned in the Editor.
    [SerializeField] public List<EnemyHealth> bellsHealth; // Assigned in the Editor.
    public int currentBellForm = 0; // Adjusted manually during development for testing.
    [SerializeField] private Transform healBellFirePointTransform;

    //Drop Related
    public GameObject spawnLocation, dropPrefab;

    [Header("Healing Fx stuff")]
    [SerializeField] private GameObject[] healingFxs;
    [SerializeField] private float healingAmount = 25;
    [Tooltip("a cada x de tempo aqui atribuido vai curar a quantidade de vida que esta no healingAmount")]
    [SerializeField] private float healTickFrequency = 0.5f;


    protected void Awake()
    {
        InvokeRepeating(nameof(RingAttack), 10, ringCooldownPerForm[currentBellForm]);
    }

    protected void Start()
    {
        foreach (var healFx in healingFxs)
        {
            healFx.SetActive(false);
        }

        //Resolve Health references here if they should be obtained in Awake.
        /*
        foreach (GameObject bell in bells)
        { 
            bellsHealth.Add(bell.GetComponent<EnemyHealth>());
        }
        */

    }

    protected void FixedUpdate()
    {


        if (testRing)
        {
            testRing = false;
            OnTimeToRing?.Invoke();
        }

        if (testChangeForm)
        {
            testChangeForm = false;
            BellDeath();
        }

        
    }

    /// <summary>
    /// Increase the surviving bells' level when one dies.
    /// </summary>
    public void BellDeath()
    {
        ++currentBellForm;
        UpdateRingAttackCoolDown();
        RemoveBell();

        if (currentBellForm == 3) // Three entries for three bells; update this count if another bell is added.
        {
            EndBossFight();
        }


        OnChangeForm?.Invoke();
    }

    public void EndBossFight()
    {
        Instantiate(dropPrefab, spawnLocation.transform.position, dropPrefab.transform.rotation);

    }

    public float GetSpeedIncrease()
    {
        return speedIncreaseMod;
    }

    protected void RingAttack()
    {
        OnTimeToRing?.Invoke();
    }
    protected void UpdateRingAttackCoolDown()
    {
        CancelInvoke(nameof(RingAttack));
        InvokeRepeating(nameof(RingAttack), 0, ringCooldownPerForm[currentBellForm]);
    }

    protected void RemoveBell() 
    {
        foreach (EnemyHealth eh in bellsHealth)
        {
            if (eh.currentHealth<= 0)
            {
                bellsHealth.Remove(eh);
                return; // Exit the entire loop so only one entry is removed per call.
            }
        }
    }


    public bool CanHeal()
    {
        foreach (EnemyHealth eh in bellsHealth)
        {
            if (eh.currentHealth < eh.maxHealth)
            {
                return true;
            }
        }
        return false;
    }

    public GameObject GetTargetBell() 
    {   

        if (CanHeal())
        {
            float lowestHealth = Mathf.Infinity;
            GameObject targetBell = bellsHealth[0].gameObject;
            foreach (EnemyHealth eh in bellsHealth)
            {
                if (eh.currentHealth < lowestHealth)
                {
                    lowestHealth = eh.currentHealth;
                    targetBell = eh.gameObject;
                    targethealBell = eh;
                }
            }

            return targetBell;
        }
        else 
        {
            //If no bell needs healing, approach a random bell.
            targethealBell = bellsHealth[UnityEngine.Random.Range(0, bellsHealth.Count)];
            return targethealBell.gameObject;


        }
       
    }

    /// <summary>
    /// Add the bells to the healing list when the boss fight starts.
    /// </summary>
    public void StartBossFight()
    {
        foreach (GameObject bell in bells)
        {
            if (bell.activeInHierarchy)
            {
                bellsHealth.Add(bell.GetComponent<EnemyHealth>());
            }
        }
    }

    public void Heal(int amount)
    {
        if (targethealBell.currentHealth < targethealBell.maxHealth)
        {
            //targethealBell.Heal(amount);
            targethealBell.HealOverTime(healingAmount, healTickFrequency,amount);

            MoveHealingFxToTarget();
        }
    }


    public void MoveHealingFxToTarget()
    {
        // Find the first inactive object in healingFxs.
        GameObject healingFx = Array.Find(healingFxs, fx => !fx.activeInHierarchy);

        if (healingFx == null)
        {
            Debug.LogWarning("Nenhum objeto desativado disponível em healingFxs.");
            return;
        }

        // Activate it at the healer bell's position.
        healingFx.SetActive(true);
        healingFx.transform.position = healBellFirePointTransform.position;

        // Start moving smoothly toward targetBell.
        StartCoroutine(MoveHealingFx(healingFx, healBellFirePointTransform.position, targethealBell.transform));
    }

    private IEnumerator MoveHealingFx(GameObject healingFx, Vector3 start, Transform target)
    {
        var animator = healingFx.GetComponent<Animator>();
        animator.SetTrigger("Start");

        float duration = 1.0f; // Movement duration in seconds.
        float loopDuration = duration - 0.06f; // Loop duration.
        float elapsedTime = 0f;

        while (elapsedTime < loopDuration)
        {
            elapsedTime += Time.deltaTime;
            // Interpolate between the starting position and the target.
            healingFx.transform.position = Vector3.Lerp(start, target.position, elapsedTime / duration);

            yield return null;
        }

        //Change the animation and continue moving.
        animator.SetTrigger("End");

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // Interpolate between the starting position and the target.
            healingFx.transform.position = Vector3.Lerp(start, target.position, elapsedTime / duration);
            yield return null;
        }

        // Ensure the object reaches the exact target position.
        healingFx.transform.position = target.position;

        // Deactivate the object.
        healingFx.SetActive(false);
    }
}
