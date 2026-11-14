using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{

    [SerializeField] private GameObject objToSpawn;
    [SerializeField] private MMF_Player breakFeedbacks;
    [SerializeField] private float myHealth = 50;
    private float defaultHealth;

    private DamageStatus myDamageStatus = DamageStatus.notDamaged;
    private Animator myAnimator;
    private bool broken = false;

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        defaultHealth = myHealth;
    }

    private void OnEnable()
    {
        if(broken) myAnimator.SetTrigger("Break");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (myDamageStatus == DamageStatus.broken) return; //Already broken; nothing to do.

        var collisionObj = collision.gameObject;

        if(collisionObj.CompareTag("Bullet"))
        {
            var bullet = collisionObj.GetComponent<BulletStats>();
            SetDamage(bullet.GetDamage().damage);   
        }
    }

    private void TriggerBreak()
    {
        broken = true;
        myAnimator.SetTrigger("Break");
        Instantiate(objToSpawn, transform.position, Quaternion.identity);
        breakFeedbacks.PlayFeedbacks();
    }

    public void SetDamage(float damage)
    {
        if (myDamageStatus == DamageStatus.broken) return; //Already broken; nothing to do.

        myHealth -= damage;
        if (myHealth <= 0)
        {
            TriggerBreak();
            myDamageStatus = DamageStatus.broken;
        }
        else if(myHealth <= defaultHealth * 0.25f)
        {
            if (myDamageStatus != DamageStatus.heavilyDamaged)
            {
                myDamageStatus = DamageStatus.heavilyDamaged;
                myAnimator.SetTrigger("damage2");  
            }
        }
        else if (myHealth <= defaultHealth * 0.75f)
        {
             if (myDamageStatus != DamageStatus.slightlyDamaged)
            {
                myDamageStatus = DamageStatus.slightlyDamaged;
                myAnimator.SetTrigger("damage1");
            }
        }
    }
}
public enum DamageStatus {notDamaged, slightlyDamaged, heavilyDamaged, broken }