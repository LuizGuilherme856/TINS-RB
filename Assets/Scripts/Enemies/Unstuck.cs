//adds a random impulse in case the enemy is stuck is some part of the map
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unstuck : MonoBehaviour
{
    [SerializeField] private float updateRate = 5f; //every 5s or so it'll check if the enemy is stuck
    [SerializeField] private float impulseForce = 5f;
    public bool killToUnstuck = true;
    [SerializeField] private bool debug = false;

    private int timesStuck = 0; //Kill after five failed recoveries; the enemy may be trapped in unreachable geometry.

    private Rigidbody2D rb;
    private EnemyHealth enemyHealth;
    private float timer;
    private StatusEffectOnEnemy statusEffectOnEnemy;

    private Vector3 lastPosition, currentPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        timer = updateRate;

        enemyHealth = GetComponent<EnemyHealth>();
        statusEffectOnEnemy = GetComponent<StatusEffectOnEnemy>();
    }

    private void OnEnable()
    {
        timesStuck = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;   
        if(timer < 0)
        {
            timer = updateRate; //Advance the timer while frozen to avoid repeatedly running this check every frame.
            currentPosition = transform.position;
            Aux.Print("vector magnitude: " + Vector3.Magnitude(currentPosition - lastPosition), debug);

            if(Vector3.Magnitude(currentPosition - lastPosition) < 0.05) //if it didnt move
            {
                if (statusEffectOnEnemy.GetCurrentStatus() == StatusEffect.Frozen || statusEffectOnEnemy.GetCurrentStatus() == StatusEffect.Electrocuted)
                {
                    Aux.Print("ta congelado/stunado. nao ta preso", debug);
                    return; //Frozen, not stuck.
                }

                Vector3 force = new Vector2(Random.Range(-impulseForce, impulseForce), Random.Range(-impulseForce, impulseForce));
                rb.AddForce(force, ForceMode2D.Impulse);

                timesStuck++;
                if(timesStuck >= 3 && killToUnstuck)
                {
                    KillToUnstuck();
                    timesStuck = 0;
                }
            }
            lastPosition = transform.position;
        }
                
    }

    private void KillToUnstuck()
    {
        Debug.Log("inimigo travado por 3 vezes em uma vida. Eliminando para desbugar");
        enemyHealth.InstaKill(true);
    }
}
