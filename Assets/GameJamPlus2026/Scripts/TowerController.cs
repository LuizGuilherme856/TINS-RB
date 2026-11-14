using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using MoreMountains.Feedbacks;
using System.Collections;
using TINS.Campaign;

public class TowerController : MonoBehaviour
{
    public TowerSO TowerData;

    public float timeSinceLastHit = 0;

    public FSGameManager gameManager;


    //Variables
    public int maxLife = 10;
    public int currentLife = 3;
    public float immunityAfterDamage = 1.0f;

    


    public UnityEvent OnTowerDeath, OnPlayerInteract;
    private readonly HashSet<Collider2D> playersInRange = new HashSet<Collider2D>();
    private MMF_Player purificationLightDamageFeedback;
    private Light2D purificationLight;
    private Color purificationLightBaseColor;
    private float purificationLightBaseIntensity;
    private Coroutine restoreLightCoroutine;

    [SerializeField, Min(0f)] private float lightFeedbackRecoveryDelay = 0.6f;

    private void Awake()
    {
        Transform[] descendants = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < descendants.Length; i++)
        {
            if (descendants[i].name != "Purification Light 2D") continue;
            purificationLightDamageFeedback = descendants[i].GetComponent<MMF_Player>();
            purificationLight = descendants[i].GetComponent<Light2D>();
            break;
        }

        CachePurificationLightDefaults();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetStartVariables();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastHit += Time.deltaTime;

        playersInRange.RemoveWhere(player => player == null || !player.enabled || !player.gameObject.activeInHierarchy);
        if (!CampaignCombatRules.IsCampaignMission && playersInRange.Count > 0 && gameManager != null && !gameManager.isDoingMinigame && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Tower interaction started");
            OnPlayerInteract?.Invoke();
        }
    }

    protected void GetStartVariables()
    {
        if (TowerData == null) return;
        maxLife = TowerData.maxLife;
        currentLife = TowerData.baseLife;
        immunityAfterDamage = TowerData.immunityAfterDamage;
    }


    protected void OnTriggerEnter2D(Collider2D collision)
    {
        TrackPlayer(collision);

    }


    protected void OnTriggerStay2D(Collider2D collision)
    {
        TrackPlayer(collision);

    }

    /// <summary>Called only by the demon's tower-attack animation event.</summary>
    public bool TryTakeDamageFromEnemy()
    {
        if (currentLife <= 0 || timeSinceLastHit <= immunityAfterDamage) return false;

        timeSinceLastHit = 0f;
        currentLife = Mathf.Max(0, currentLife - 1);
        if (purificationLightDamageFeedback == null)
            CachePurificationLightFeedback();
        if (purificationLightDamageFeedback != null)
            purificationLightDamageFeedback.PlayFeedbacks(transform.position);
        RestorePurificationLightAfterFeedback();

        if (currentLife <= 0) OnTowerDeath?.Invoke();
        return true;
    }

    private void CachePurificationLightFeedback()
    {
        Transform[] descendants = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < descendants.Length; i++)
        {
            if (descendants[i].name != "Purification Light 2D") continue;
            purificationLightDamageFeedback = descendants[i].GetComponent<MMF_Player>();
            purificationLight = descendants[i].GetComponent<Light2D>();
            break;
        }

        CachePurificationLightDefaults();
    }

    private void CachePurificationLightDefaults()
    {
        if (purificationLight == null) return;

        purificationLightBaseColor = purificationLight.color;
        purificationLightBaseIntensity = purificationLight.intensity;
    }

    private void RestorePurificationLightAfterFeedback()
    {
        if (purificationLight == null) return;

        if (restoreLightCoroutine != null)
            StopCoroutine(restoreLightCoroutine);

        restoreLightCoroutine = StartCoroutine(RestorePurificationLight());
    }

    private IEnumerator RestorePurificationLight()
    {
        yield return new WaitForSecondsRealtime(lightFeedbackRecoveryDelay);

        if (purificationLight != null)
        {
            purificationLight.enabled = true;
            purificationLight.color = purificationLightBaseColor;
            purificationLight.intensity = purificationLightBaseIntensity;
        }

        restoreLightCoroutine = null;
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playersInRange.Remove(collision);
    }

    private void TrackPlayer(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playersInRange.Add(collision);
    }
}
