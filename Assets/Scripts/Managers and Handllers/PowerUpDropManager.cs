using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Drop power-ups in the level.
/// </summary>

public class PowerUpDropManager : MonoBehaviour
{
    [SerializeField] private GameObject[] powerUps, bossPowerUp;
    [SerializeField] private int powerUpsAvailableToSpawn = 3, maxPowerUpsAvailable = 5;
    [SerializeField] private float powerUpRechargeSeconds = 60f;

    [Header("Drop Config")]
    public int heartIndex = 6;


     [Header("Misc")]
    [SerializeField] private RunData runData;
    [SerializeField] private MMMiniObjectPooler miniObjectPooler;
    private  List<int> nonHeartIndexes = new ();

    void Awake()
    {
        //Populate the list of non-heart power-ups.
        for (int i = 0; i < powerUps.Length; i++)
        {
            if (i != heartIndex) nonHeartIndexes.Add(i);
        }
    }
    private void Start()
    {
        InvokeRepeating(nameof(IncreasePowerUpsAvailable), 60, powerUpRechargeSeconds);
    }

public void SpawnPowerUp(Vector3 pos)
{
    if (powerUpsAvailableToSpawn <= 0) return;

    // Check whether a heart spawn should be attempted.
    if (heartIndex >= 0 && heartIndex < powerUps.Length)
    {
        float currentHp = runData.currentHp;

        if (currentHp > 10)
        {
            int randomIndex = nonHeartIndexes[Random.Range(0, nonHeartIndexes.Count)];
            Instantiate(powerUps[randomIndex], pos, transform.rotation);
        }
        else
        {
            // Increase heart spawn probability as health decreases.
            // Example: HP 10 gives 10%; HP 1 gives 90%.
            float chance = Mathf.Clamp01((10f - currentHp) / 10f); // 0 to 0.9.
            float roll = Random.value; // 0 to 1.

            if (roll < chance)
            {
                Instantiate(powerUps[heartIndex], pos, transform.rotation);
            }
            else
            {
                int randomIndex = nonHeartIndexes[Random.Range(0, nonHeartIndexes.Count)];
                Instantiate(powerUps[randomIndex], pos, transform.rotation);
            }
        }
    }
    else
    {
        // Fall back to any power-up if heartIndex is invalid.
        var drop = powerUps[Random.Range(0, powerUps.Length)];
        Instantiate(drop, pos, transform.rotation);
    }

    powerUpsAvailableToSpawn--;
}

    public void SpawnBossPowerUp(Vector3 pos, int index =0)
    {
        Instantiate(bossPowerUp[index], pos, transform.rotation);
        //Debug.Log("spawn boss power up");
    }

    public void SpawnSurvivalModeDrop(Vector3 pos)
    {
        var obj =  miniObjectPooler.GetPooledGameObject();

        obj.SetActive(true); //Activate before changing position; this ordering is required.
        obj.transform.localPosition = pos;

    }

    private void IncreasePowerUpsAvailable()
    {
        if(powerUpsAvailableToSpawn >= maxPowerUpsAvailable) return;
        
        powerUpsAvailableToSpawn++;

    }
}
