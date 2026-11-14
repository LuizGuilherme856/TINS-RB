using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaelTransformationManager : MonoBehaviour
{
    public EnemyHealth enemyHealth;
    public float hpThreshold = 0.1f;
    //public GameObject bossObject;
    public GameObject giantBoss;
    public SpriteRenderer gaelSmallSprites;
    public SpriteRenderer giantHead;

    public PolygonCollider2D[] giantColliders;

    private PriestGaelBoss priestGaelBoss;

    public EnemyHealth giantHealth;
    public BossFightUiInfo bossFightUiInfo;
    public GameObject gaelShield;
    public AudioClip music2ndPart;
    public float musicVolume = 0.4f;

    [Header("Giant Boss Movement Settings")]
    public Transform targetPosition;
    public float initialSpeed = 5f;
    public float minSpeed = 1f;
    public Animator transformationExplosionAnimator;
    public float delayToExplosion = 2f;
    public float delayToSpawn = 3f;

    private BossFightArena bossFightArena;
    private MusicManager musicManager;


    public float timeToStartSecondPart = 60f; // Delay before starting the second fight phase.
    private float timeSinceBossfightStart = 0f;
    private bool bossFightStarted = false;
    private bool transformationRequested = false;
    private bool giantBossSpawnStarted = false;

    private void Awake()
    {
        priestGaelBoss = enemyHealth.gameObject.GetComponent<PriestGaelBoss>();
        bossFightArena = GetComponent<BossFightArena>();
        musicManager = FindFirstObjectByType<MusicManager>();
    }

    private void OnEnable()
    {
        enemyHealth.OnHealthChange += TriggerTransformation;
        priestGaelBoss.OnReadyToTransform += MoveGiantBoss;
        bossFightArena.OnStartBossFight += HandleBossFightStarted;
    }

    private void OnDisable()
    {
        enemyHealth.OnHealthChange -= TriggerTransformation;
        priestGaelBoss.OnReadyToTransform -= MoveGiantBoss;
        bossFightArena.OnStartBossFight -= HandleBossFightStarted;
    }

    private void HandleBossFightStarted()
    {
        bossFightStarted = true;
        timeSinceBossfightStart = 0f;
    }

    private void TriggerTransformation()
    {
        if (transformationRequested)
        {
            return;
        }

        //The transformation health threshold was reached.
        if (enemyHealth.currentHealth / enemyHealth.maxHealth <= hpThreshold  && priestGaelBoss.transformationMode == false)
        {
            gaelShield.SetActive(false);
            transformationRequested = true;
            priestGaelBoss.transformationMode = true;
        }
    }

    private void Update()
    {
        if(bossFightStarted)
        {
            timeSinceBossfightStart += Time.deltaTime;
        }


        //Force transformation after the time limit if the health condition has not triggered it.
        if (transformationRequested == false && timeSinceBossfightStart > timeToStartSecondPart)
        {
            transformationRequested = true;
            StartCoroutine(priestGaelBoss.ForceTransform());
        }

    }

    public void MoveGiantBoss()
    {
        if (giantBossSpawnStarted)
        {
            return;
        }

        giantBossSpawnStarted = true;
        StartCoroutine(MoveGiantBossCoroutine());
    }

    private IEnumerator MoveGiantBossCoroutine()
    {
        musicManager.FadeOutBossMusicVolume(1f);
        yield return new WaitForSeconds(delayToExplosion);
        transformationExplosionAnimator.SetTrigger("Transform");
        yield return new WaitForSeconds(delayToSpawn);
        giantBoss.SetActive(true);

        float currentSpeed = initialSpeed;
        while (Vector3.Distance(giantBoss.transform.position, targetPosition.position) > 0.1f)
        {
            float distance = Vector3.Distance(giantBoss.transform.position, targetPosition.position);
            currentSpeed = Mathf.Max(minSpeed, initialSpeed * (distance / Vector3.Distance(giantBoss.transform.position, targetPosition.position)));
            giantBoss.transform.position = Vector3.MoveTowards(giantBoss.transform.position, targetPosition.position, currentSpeed * Time.deltaTime);
            yield return null;
        }

        musicManager.PlayBossMusic(music2ndPart);
        //Set volume after playback starts because starting playback restores the default volume.
        musicManager.SetBossMusicVolume(musicVolume, true);

        //Refresh boss health after switching boss objects.
        bossFightUiInfo.SetBossHealth(giantHealth);
        bossFightArena.ChangeBossHealth(giantHealth);

        giantHead.sortingLayerName = "AbovePlayer";
        giantHead.gameObject.GetComponent<PolygonCollider2D>().enabled = true;

        foreach (var collider in giantColliders)
        {
            collider.enabled = true;
        }
    }
}

