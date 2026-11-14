using System;
using System.Collections;
using TechBooh;
using UnityEngine;

public class HealPillar : MonoBehaviour
{
    [SerializeField] protected LuciferData luciferData; //Assigned in the Editor.
    protected int maxMeleeHits = 2;
    protected int currentHits = 0;
    public virtual event Action OnDestroyPillar;
    protected LineRenderer lr;
    [SerializeField]  protected GameObject laserStartPoint;// Assigned in the Editor.

    private Animator animator;
    protected void Awake()
    {
        lr = GetComponent<LineRenderer>();
        animator = Try.GetComponent<Animator>(gameObject);

    }

    protected void OnEnable()
    {
        lr.enabled = false;
        lr.SetPosition(0, laserStartPoint.transform.position);
        maxMeleeHits = luciferData.pillarMeleesToDie;
    }
    public void ResetPillar() 
    {
        this.gameObject.SetActive(true);
        currentHits = 0;

        //Remove OnDestroyPillar listeners to avoid duplicate subscriptions.
        OnDestroyPillar = null;
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Melee"))
        {
            //Orbital weapons do not count.
            if (collision.gameObject.GetComponent<OrbitalUpgradeHolder>() != null) return;

            currentHits++;
            if (currentHits >= maxMeleeHits)
            {
                StartCoroutine(PillarDeath_Routine());
            }
        }
    }

    private IEnumerator PillarDeath_Routine()
    {
        animator.SetTrigger("Death");
        lr.enabled = false;
        
        yield return new WaitForSeconds(1.7f); // Wait for the death animation to finish.

        this.gameObject.SetActive(false);
        OnDestroyPillar?.Invoke();

    }

}
