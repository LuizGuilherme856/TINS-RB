using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectOnHead : MonoBehaviour
{
    [SerializeField] protected float health, timeActive;
    [SerializeField] protected float timer = 0f;
    [SerializeField] MMF_Player hitfeedbacks;

    [Tooltip("É sobre escrito pela variavel que esta em no data do inimigo, para deixar tudo centralizado")]
    public bool usesHealth = true;

    public event Action OnZeroHealth,OnTakeDamage,OnEndTime;
    // Start is called before the first frame update
    protected void OnEnable()
    {
        
    }

    protected void FixedUpdate()
    {
        if (!usesHealth)
        {
            timer += Time.deltaTime;
            if (timer >= timeActive)
            {
                timer = 0;
                OnEndTime?.Invoke();
            }
        }
    }
    /// <summary>
    /// <para>
    /// Update the attached object's maximum health from player melee damage, allowing at most five melee hits.
    /// </para>
    /// Used when the player switches melee weapons with an object attached to the head.
    /// </summary>
    public void UpdateHealth(float newPlayerMeleeDamage)
    {
        if (health > 5 * newPlayerMeleeDamage)
        {
            this.health = 5 * newPlayerMeleeDamage;
        }
    }

    /// <summary>
    /// Initialize health immediately after attaching to the player's head.
    /// </summary>
    public void SetHealth(float targetHealth, float playerMeleeDamage) 
    {
        if (targetHealth < 5 * playerMeleeDamage)
        {
            this.health = targetHealth;
        }
        else 
        {
            this.health = 5 * playerMeleeDamage;
        }
    
    }

    /// <summary>
    /// Initialize the lifetime immediately after attaching to the player's head.
    /// </summary>
    public void SetTime(float targetTime)
    {

        this.timeActive = targetTime;

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Objects without health must not die from damage.
        if (usesHealth)
        {
            if (collision.gameObject.CompareTag("Melee"))
            {

                var _meleeObject = collision.gameObject;
                var _meleeStats = collision.gameObject.GetComponent<MeleeStats>();
                var _damage = _meleeStats.GetDamage();

                health -= _damage.Item1;
                hitfeedbacks.PlayFeedbacks();
                OnTakeDamage?.Invoke();
                if (health <= 0) //Dead.
                {
                    health = 0;
                    OnZeroHealth?.Invoke();
                    this.gameObject.SetActive(false);
                }
            }

        }

    }

}
