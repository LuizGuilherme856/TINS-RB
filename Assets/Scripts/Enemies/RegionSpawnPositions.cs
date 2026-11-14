using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a region's area and spawn points.
/// Enemy spawn regions become available when their doors are opened.
/// </summary>
public class RegionSpawnPositions : MonoBehaviour
{
    [SerializeField] public Transform[] spawnPos;
    [SerializeField] public bool isActive = false;
    [SerializeField] private int regionIndex = 1;

   // private PolygonCollider2D regionCollider;
    private SpawnManager spawnManager;
    private bool isPlayerOnRegion = false;
    private Transform playerTransform;
    [SerializeField] private bool showDebug= true;

    private void Awake()
    {
        //regionCollider = GetComponentInChildren<PolygonCollider2D>();
        spawnManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<SpawnManager>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        if(spawnManager == null) spawnManager = FindFirstObjectByType<SpawnManager>();  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            spawnManager.SetCurrentPlayerRegion(regionIndex);
            Aux.Print("Player is now on region: " + regionIndex, showDebug);
            
        }
    }



    public Transform GetRandomSpawnPositionOnRegion(bool canBeNearPlayer)
    {
        var pos = spawnPos[Random.Range(0, spawnPos.Length)];

        if (canBeNearPlayer)
        {
            return pos;
        }
        else
        {
            int iterations = 0;

            while (Vector2.Distance(pos.position, playerTransform.position) < 7 || iterations > 25)
            {
                pos = spawnPos[Random.Range(0, spawnPos.Length)];
                iterations++;
            }

            if (iterations > 25) Debug.LogWarning("fez 25 iterações e nao saiu do loop. Saindo do loop automaticamente...");

            return pos;
        }
    }

    public int GetRegionIndex()
    {
        return regionIndex;
    }

    public bool IsPlayerOnRegion()
    {
        return isPlayerOnRegion;
    }
}

//public enum Regions {initialRegion, secondRegion }
