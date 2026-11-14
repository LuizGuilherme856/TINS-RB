using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns orbs for both the legacy sacrifice system and blood rituals.
/// </summary>
public class OrbSpawner : MonoBehaviour
{
    [SerializeField] private Transform orbDestination;
    [field: SerializeField] public int maxOrbs { get; protected set; } = 10;
    [SerializeField] protected GameObject orb;
    public int currentOrbs { get; protected set; }
    protected float range = 6f;

    private void Awake()
    {
        range = GetComponent<CircleCollider2D>().radius;
    }


    public void SpawnOrb(Vector3 spawnPos)
    {
        if (currentOrbs >= maxOrbs) return;

        var newOrb = Instantiate(orb, spawnPos, transform.rotation);
        newOrb.GetComponent<GoToAltar>().SetDestination(orbDestination);
    }

    public float GetRange()
    {
        return range;
    }

    public bool IsCompleted()
    {
        if (currentOrbs >= maxOrbs)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public virtual void AddOrb()
    {

    }
}
