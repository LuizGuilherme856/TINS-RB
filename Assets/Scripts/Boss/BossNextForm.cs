using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <description>
/// This scripts triggers the animations "transform" when HP decreases to certain values
/// </description>
public class BossNextForm : MonoBehaviour
{
    private Animator animator;
    private EnemyHealth enemyHealth;
    [SerializeField] private bool hasThirdTransformation = true;
    [SerializeField] [Range(0,1)] private float hpToSecondForm, hpToThirdForm;
    [SerializeField] private MMF_Player secondFormTransformationFeedback, thirdFormTransformationFeedback;

    public event Action OnChangeForm;
    private float timeInForm1 = 0; //Prevents skipping directly from transformation 0 to 2.

    [SerializeField] private float minimalTimeInForm = 5f;
    [field: SerializeField] public int currentForm { get; private set; }

    private void Awake()
    {
        currentForm = 0;
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        enemyHealth.OnHealthChange += CompareHealth;
    }

    private void FixedUpdate()
    {
        if(currentForm == 1) timeInForm1+= Time.fixedDeltaTime;
    }

    private void CompareHealth()
    {
        if(enemyHealth.currentHealth / enemyHealth.maxHealth < hpToSecondForm && currentForm == 0)
        {
            TriggerTransformation(1);
        }
        else if ( enemyHealth.currentHealth / enemyHealth.maxHealth < hpToThirdForm && currentForm == 1 && timeInForm1 > minimalTimeInForm)
        {
            TriggerTransformation(2);
        }
        else if(enemyHealth.currentHealth <= 0)
        {
            animator.SetTrigger("Die");
        }

    }

    private void TriggerTransformation(int form)
    {
        //Debug.Log("Transform. form: "+form);
        currentForm = form;
        OnChangeForm?.Invoke();

        //Gromsch has three forms, but no transformation into the third; this requires special handling.
        if(form == 1 ) animator.SetTrigger("Transform1");
        if (form == 2 && hasThirdTransformation)
            animator.SetTrigger("Transform2");
    }

    public void PlayTransformationFeedback(int nextForm) //used on the animator
    {
        if(nextForm == 1)
        {
            secondFormTransformationFeedback.PlayFeedbacks();
        }
        else
        {
            thirdFormTransformationFeedback.PlayFeedbacks();
        }
    }
}
