using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls Gael retreating to the upper part of the arena during the first fight phase.
/// </summary>
public class GaelEscapeManager : MonoBehaviour
{
    /// <summary>
    /// Gael retreats at this health percentage.
    /// </summary>
    [SerializeField] float hpThreshold = 0.5f;
    [SerializeField] Transform gaelBossTranform;
    [SerializeField] GameObject fenceToBreak;
    [SerializeField] BossFightArena bossFightArena;


    [SerializeField] GameObject key;
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] GameObject shieldGameObject;

    private PriestGaelBoss gaelBoss;
    private EnemyHealth gaelHealth;
    private StopWaveManager stopWaveManager;
    private ScanPathFinder scanPathFinder;
    bool gaelEscaped = false;

    private void Awake()
    {
        gaelHealth = gaelBossTranform.gameObject.GetComponent<EnemyHealth>();
        gaelBoss = gaelBossTranform.gameObject.GetComponent<PriestGaelBoss>();
        stopWaveManager = FindFirstObjectByType<StopWaveManager>();
        scanPathFinder = GetComponent<ScanPathFinder>();
    }

    private void OnEnable()
    {
        StartCoroutine(DelayedCheckEscape());
    }

    private void Update()
    {

        if (gaelEscaped == false)
        {
            CheckGaelPosition();
        }
    }

    /// <summary>
    /// Delay slightly so this script reads health after the other components.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedCheckEscape()
    {
        yield return new WaitForSeconds(0.5f);
        gaelHealth.OnTakeDamage += CheckEscape;
    }

    private void CheckEscape()
    {
        if (gaelHealth.currentHealth / gaelHealth.maxHealth <= hpThreshold )
        {
            shieldGameObject.SetActive(false);
            gaelBoss.escapeMode = true;
            trailRenderer.enabled = true;
        }
    }

    /// <summary>
    /// The retreat reached the fence; break it.
    /// </summary>
    private void CheckGaelPosition()
    {
        //The boss is below the fence; crossing above it indicates an impact.
        if (gaelBossTranform.position.y > fenceToBreak.transform.position.y && gaelBoss.readyToEscape)
       {
            gaelEscaped = true;
            fenceToBreak.SetActive(false);
            key.SetActive(true);
            StartCoroutine(bossFightArena.EndBossFightAfterDelay());
            StartCoroutine(stopWaveManager.ResumeWave_Routine(1f));
            scanPathFinder.AsyncScan();
        }
    }
}
