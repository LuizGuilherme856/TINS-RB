using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
	public enum AttackType {area, spawner, projectile}

    protected int currentForm = 0;
	[Serializable]public struct BossAttack
	{
		public bool targetPlayer;
		public AttackType attackType;
		public GameObject attackGameObject;
        public string attackName;
    }

    [SerializeField] List<BossAttack> bossAttacks;

    [SerializeField] protected BossNextForm nextFormScript;

    [SerializeField] protected bool usesBossNextForm = true;

    protected virtual void Awake() 
    {
        //Try Catches
        if (usesBossNextForm)
        {
            try
            {
                nextFormScript = GetComponent<BossNextForm>();
            }
            catch (System.Exception)
            {
                Debug.LogError("Gorfo boss não tem script nextForm");
            }
            nextFormScript.OnChangeForm += UpdateCurrentForm;
        }
    }
    protected virtual void StateMachine()
    {
        throw new System.NotImplementedException();
    }

    protected virtual void UpdateCurrentForm() 
    {
        currentForm = nextFormScript.currentForm;
    }


    public void AttackByIndex(int targetIndex) 
	{
		bossAttacks[targetIndex].attackGameObject.SetActive(true);
	}
    public void DeactivateAttackByIndex(int targetIndex)
    {
        bossAttacks[targetIndex].attackGameObject.SetActive(false);
    }

    public void AttackByName(string targetName)
    {
        foreach (BossAttack attack in bossAttacks)
        {
            if (targetName == attack.attackName)
            {
                attack.attackGameObject.SetActive(true);
            }
        }

    }

    public void DeactivateAllAttacks()
    {
        foreach (BossAttack attack in bossAttacks)
        {
            attack.attackGameObject.SetActive(false);
        }
        
    }
    public int getCurrentForm() 
    {
        return currentForm;
    }

    public BossAttack GetAttackByName(string targetName) 
    {
        foreach (BossAttack attack in bossAttacks)
        {
            if (targetName == attack.attackName)
            {
                return attack;
            }
        }
        BossAttack Empty = new BossAttack();
        return Empty;
    }
}
