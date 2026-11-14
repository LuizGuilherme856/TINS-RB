using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainReactionStatusEffect : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] private float radius;
    [SerializeField] private DamageData damageData;
    [SerializeField] private float cooldown = 5f;

    [SerializeField] private GameObject electricArc;
    [SerializeField] float speed = 0.5f;

    [SerializeField] private GlobalStats globalStats;

    private float timer;
    private int numberOfReactions = 0;

    // Track position and elapsed time.
    private Vector3 lastElectricArcPosition;
    private float timeInSamePosition;

    private void OnEnable()
    {
        timer = 0;
        numberOfReactions = 0;
        lastElectricArcPosition = electricArc.transform.position;
        timeInSamePosition = 0f;
    }

    private void FixedUpdate()
    {
        if (timer > 0) timer -= Time.deltaTime;

        // Check whether electricArc stopped moving after its target died.
        if (Vector3.Distance(electricArc.transform.position, lastElectricArcPosition) < 0.01f)
        {
            timeInSamePosition += Time.fixedDeltaTime;

            // Deactivate after remaining stationary for more than one second.
            if (timeInSamePosition > 0.5f)
            {
                // Debug.Log("Electric Arc has remained stationary for over 0.5 seconds; deactivating.");
                Deactivate();
            }
        }
        else
        {
            // Reset the timer when electricArc moves.
            timeInSamePosition = 0f;
            lastElectricArcPosition = electricArc.transform.position;
        }

        // Move toward the destination.
        float distance = Vector2.Distance(electricArc.transform.position, transform.position);
        if (distance > 0.1f)
        {
            var nextPos = Vector2.MoveTowards(electricArc.transform.position, transform.position, Time.fixedDeltaTime * speed);
            electricArc.transform.position = nextPos;
        }
        else if (electricArc.activeSelf) // Deactivate on arrival if still active.
        {
            GetComponentInParent<StatusEffectOnEnemy>().SetStatus(StatusEffect.Electrocuted, damageData.statusDuration, damageData.statusDamagePerSec);
            Deactivate();
        }
    }

    private void Deactivate()
    {
        electricArc.transform.parent = transform;
        electricArc.SetActive(false);
    }

    /// <summary>
    /// Electrocution activates this chain-reaction trigger.
    /// </summary>
    public void TriggerReaction()
    {
        numberOfReactions = 0;
        Vector3 epicenter = transform.position;
        Collider2D[] targetsHit = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);
        foreach (Collider2D target in targetsHit)
        {
            if (numberOfReactions >= globalStats.maxChainReactions) return;

            if (target.CompareTag("Enemy"))
            {
                numberOfReactions++;

                try
                {
                    // Find the other enemy's ChainReaction component and designate it as the next link.
                    var chainReaction = target.GetComponentInChildren<ChainReactionStatusEffect>();
                    chainReaction.ContinueChainReaction(transform.position);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Erro ao aplicar chain reaction no inimigo.\nException: " + e);
                }
            }
        }
    }

    public void ContinueChainReaction(Vector3 pos)
    {
        float randomValue = Random.Range(0f, 1f);
        if (randomValue > damageData.statusChance) return;

        if (timer > 0) return;
        electricArc.SetActive(true);
        electricArc.transform.parent = null;
        electricArc.transform.position = pos;

        timer = cooldown;
    }
}
