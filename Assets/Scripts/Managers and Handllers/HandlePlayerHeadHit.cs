using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlePlayerHeadHit : MonoBehaviour
{
    protected HandleScreenEffecs handleScreenEffecs;

    [SerializeField] protected List<HeadObject> headObjects;

    //Assign both melee references in the Editor; these components are shared by melee and special attacks.
    [SerializeField] protected MeleeManager meleeManager; // Editor-assigned melee attack manager.

    [SerializeField] protected MeleeWeaponInventory meleeInventoryManager; //Editor-assigned melee weapon manager.
    //Track objects currently attached to the player's head.
    protected ObjectOnHead currentOnHeadObject;
    protected bool headIsOccupied = false;
    [Serializable]
    public struct HeadObject
    {
        public string name;
        public GameObject headGameObject;
        public ObjectOnHead objectOnHeadScript;
    }

    protected void Awake()
    {
        try
        {
            handleScreenEffecs = FindFirstObjectByType<HandleScreenEffecs>();
        }
        catch (Exception)
        {
            Debug.Log("Não encontrou o handleScreenEffecs nessa scene");
        }

    }



    protected void OnEnable()
    {
        //Add listeners
        meleeInventoryManager.OnChangeMeleeWeapon += UpdateOnHeadObjectHealth;

    }
    protected void OnDisable()
    {
        //Remove Listeners
        meleeInventoryManager.OnChangeMeleeWeapon -= UpdateOnHeadObjectHealth;
    }


    public void ResetAllHeadObjects()
    {
        foreach (HeadObject HeadObjectI in headObjects)
        {
            HeadObjectI.headGameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Set the selected HeadObject flag by name.
    /// Activate or deactivate that HeadObject.
    /// </summary>
    public void SetHeadObjectByName(string name, bool targetBool)
    {
        foreach (HeadObject HeadObjectI in headObjects)
        {
            if (name == HeadObjectI.name)
            {
                HeadObjectI.headGameObject.SetActive(targetBool);
                //Only one head object can be active, so apply the supplied value to targetBool.
                headIsOccupied = targetBool;

            }
        }
    }

    /// <summary>
    /// Read the selected HeadObject flag by name.
    /// Activate or deactivate that HeadObject.
    /// </summary>
    public HeadObject getHeadObjectByName(string name)
    {
        foreach (HeadObject HeadObjectI in headObjects)
        {
            if (name == HeadObjectI.name)
            {
                return HeadObjectI;
            }
        }
        HeadObject error = new HeadObject();
        error.name = "Não foi encontrado este headObject na lista";
        Debug.LogError("Não foi encontrado este headObject na lista");
        return error;
    }


    /// <summary>
    /// Set the selected HeadObject flag by index.
    /// Activate or deactivate that HeadObject.
    /// </summary>
    public void SetHeadObjectByIndex(int index, bool targetBool)
    {
        headObjects[index].headGameObject.SetActive(targetBool);
        headIsOccupied = targetBool;
    }



    
    /// <summary>
    /// Called when a jumping hand collides with the player's head.
    /// Activate the head object and associated effects.
    /// </summary>
    public void HandHeadHit(float targetHealth,JumpingEnemyData jumpingEnemyData)
    {
        //Activate the damageable prefab.
        handleScreenEffecs.SetEffectByName("HandGrabEffect", true);
        SetHeadObjectByName("HandOnHead", true);
        
        ObjectOnHead targetHeadObject = getHeadObjectByName("HandOnHead").objectOnHeadScript;
        //Update the shared value to centralize state in the ScriptableObject.
        targetHeadObject.usesHealth = jumpingEnemyData.usesHealth;
        currentOnHeadObject = targetHeadObject;
        if (targetHeadObject.usesHealth)
        {
            //Pass the attached hand's current health here.
            targetHeadObject.SetHealth(targetHealth, meleeManager.GetMeleeData().GetDamage());
            targetHeadObject.OnTakeDamage += HandHeadDamage;
            targetHeadObject.OnZeroHealth+= HandHeadDeath;

        }
        
    }


    /// <summary>
    /// Remove effects, objects, and listeners when JumpingHandOnHead dies.
    /// </summary>
    protected void HandHeadDamage()
    {
        Animator targetAnimator = handleScreenEffecs.GetEffectByName("HandGrabEffect").GetComponent<Animator>();
        targetAnimator.SetTrigger("Hit");

    }

    /// <summary>
    /// Remove effects, objects, and listeners when JumpingHandOnHead dies.
    /// </summary>
    protected void HandHeadDeath()
    {
        handleScreenEffecs.SetEffectByName("HandGrabEffect", false);
        //Deactivate the prefab attached to the player's head.
        SetHeadObjectByName("HandOnHead", false);
        ObjectOnHead targetHObject = getHeadObjectByName("HandOnHead").objectOnHeadScript;
        targetHObject.OnZeroHealth -= HandHeadDeath; //Remove listener
        
    }

    public void EggHeadHit(float timeInHead, JumpingEnemyData jumpingEnemyData)
    {
        //Activate the damageable prefab.
        handleScreenEffecs.SetEffectByName("EggGrabEffect", true);
        SetHeadObjectByName("EggOnHead", true);

        ObjectOnHead targetHeadObject = getHeadObjectByName("EggOnHead").objectOnHeadScript;
        //Update the shared value to centralize state in the ScriptableObject.
        targetHeadObject.usesHealth = jumpingEnemyData.usesHealth;
        currentOnHeadObject = targetHeadObject;
        if (!targetHeadObject.usesHealth)
        {
            targetHeadObject.SetTime(jumpingEnemyData.timeInHead);
            targetHeadObject.OnEndTime += EggHeadDeath;

        }

    }

    /// <summary>
    /// Remove effects, objects, and listeners when the egg timer expires.
    /// </summary>
    protected void EggHeadDeath()
    {
        handleScreenEffecs.SetEffectByName("EggGrabEffect", false);
        //Deactivate the prefab attached to the player's head.
        SetHeadObjectByName("EggOnHead", false);
        ObjectOnHead targetHObject = getHeadObjectByName("EggOnHead").objectOnHeadScript;
        targetHObject.OnEndTime -= EggHeadDeath; //Remove listener
    }

    public void UpdateOnHeadObjectHealth()
    {
        if (currentOnHeadObject != null)
        {
            currentOnHeadObject.UpdateHealth(meleeManager.GetMeleeData().GetDamage());
        }
    }

    public bool isHeadOccupied() 
    {
        return headIsOccupied;
    }
}
