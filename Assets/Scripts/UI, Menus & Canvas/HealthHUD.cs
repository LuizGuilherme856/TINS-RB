using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUD : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;
    private Health playerhealth; // Resolve references automatically to avoid missing assignments.
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform firstHeartPos;
    [SerializeField] private Vector3 newHeartPosOffSet;
    [SerializeField] private TextMeshProUGUI moreThanMaxLifeIndicator;

    /// <summary>
    /// Points fitting in the original panel without resizing.
    /// </summary>
    [SerializeField] private int numberOfHpThatFitsTheInitialSize = 8;
    [SerializeField] private int maxHearts = 25; //Maximum number of visible hearts.

    [Header("   PANEL SIZE INCREASER")]
    [SerializeField] private RectTransform panelExtenderTransform;
    [SerializeField] private RectTransform panelEndTransform;
    [SerializeField] private float defaultWidthHpExtender = 30f;
    [SerializeField] private float animationDuration = 1f;

    //Additional hearts available when the health bar expands.
    private int extraNumberOfHearts = 0;

    private int currentHealth;
    private readonly List<GameObject> hearts = new();
    private int maxVisibleHearts_TechBooh;

    private Vector2 panelExtenderInitialSize;
    private Vector3 panelExtenderInitialPos;
    private Vector3 panelEndInitialPos;

    private Vector2 currentPanelExtenderInitialSize;
    private Vector3 currentPanelExtenderInitialPos;
    private Vector3 currentPanelEndInitialPos;

    private bool hasUiBeenUpdated = false;
    private int maxExpanderMultiplier;
    private Coroutine animateUiCoroutine_TechBooh;
    private Vector3 cachedFirstHeartLocalPosition_TechBooh;
    private Vector3 cachedHeartOffsetLocal_TechBooh;

    private void Awake()
    {
        playerhealth = playerHolder.GetPlayerHealth();
        if (playerhealth == null)
        {
            playerhealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        }
    }

    private void Start()
    {
        if (playerhealth == null)
        {
            playerhealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        }

        currentHealth = Mathf.RoundToInt(playerhealth.GetCurrentHealth());

        ImportPanelsSizes();
        maxVisibleHearts_TechBooh = Mathf.Max(0, maxHearts - 1);
        maxExpanderMultiplier = Mathf.Max(0, maxHearts - numberOfHpThatFitsTheInitialSize);

        CreateHearts();
        playerhealth.OnHealthChange += TryToUpdateHealth;
        InvokeRepeating(nameof(TryToUpdateHealth), 1f, 0.5f);
        TryToUpdateHealth();
    }

    private void OnDestroy()
    {
        if (playerhealth != null)
        {
            playerhealth.OnHealthChange -= TryToUpdateHealth;
        }

        CancelInvoke(nameof(TryToUpdateHealth));
    }

    private void ImportPanelsSizes()
    {
        panelExtenderInitialSize = panelExtenderTransform.sizeDelta;
        panelExtenderInitialPos = panelExtenderTransform.localPosition;
        panelEndInitialPos = panelEndTransform.localPosition;

        currentPanelExtenderInitialSize = panelExtenderInitialSize;
        currentPanelExtenderInitialPos = panelExtenderInitialPos;
        currentPanelEndInitialPos = panelEndInitialPos;

        // TechBooh:
        // Keep heart positions and spacing fixed in UI space.
        // Cache the local reference once instead of recalculating during zoom.
        cachedFirstHeartLocalPosition_TechBooh = transform.InverseTransformPoint(firstHeartPos.position);
        cachedHeartOffsetLocal_TechBooh = transform.InverseTransformVector(newHeartPosOffSet);
    }

    private void CreateHearts()
    {
        EnsureVisibleHeartCount_TechBooh(Mathf.Clamp(currentHealth, 0, maxVisibleHearts_TechBooh));

        if (currentHealth > numberOfHpThatFitsTheInitialSize + extraNumberOfHearts)
        {
            int sizeToIncrease = currentHealth - numberOfHpThatFitsTheInitialSize - extraNumberOfHearts;
            if (sizeToIncrease > maxExpanderMultiplier)
            {
                sizeToIncrease = maxExpanderMultiplier;
            }

            IncreasePanelSize(sizeToIncrease);
        }
    }

    private void TryToUpdateHealth()
    {
        try
        {
            UpdateHealth();
        }
        catch (System.Exception e)
        {
            foreach (var heart in hearts)
            {
                if (heart != null)
                {
                    heart.SetActive(false);
                }
            }

            hearts.Clear();
            CreateHearts();
            Debug.LogWarning("Erro na UI de vida: " + e.Message);
        }
    }

    private void UpdateHealth()
    {
        currentHealth = Mathf.RoundToInt(playerhealth.GetCurrentHealth());
        int visibleHearts = Mathf.Clamp(currentHealth, 0, maxVisibleHearts_TechBooh);

        if (currentHealth <= 0)
        {
            foreach (var heart in hearts)
            {
                if (heart != null)
                {
                    heart.SetActive(false);
                }
            }

            moreThanMaxLifeIndicator.text = "";
            return;
        }

        if (currentHealth > numberOfHpThatFitsTheInitialSize + extraNumberOfHearts)
        {
            IncreasePanelSize(currentHealth - numberOfHpThatFitsTheInitialSize - extraNumberOfHearts);
        }

        EnsureVisibleHeartCount_TechBooh(visibleHearts);
        RefreshHeartLayoutAndState_TechBooh(visibleHearts);

        int extraHp = currentHealth - maxVisibleHearts_TechBooh;
        moreThanMaxLifeIndicator.text = extraHp > 0 ? "+ " + extraHp : "";
    }

    /// <summary>
    /// TechBooh:
    /// Keep instantiated heart count aligned with the visible maximum
    /// without relying on activeSelf to count existing hearts.
    /// </summary>
    private void EnsureVisibleHeartCount_TechBooh(int visibleHearts)
    {
        while (hearts.Count < visibleHearts)
        {
            int heartIndex = hearts.Count;
            GameObject newHeart = Instantiate(heartPrefab, transform);
            hearts.Add(newHeart);
            PositionHeart_TechBooh(newHeart.transform, heartIndex);
            newHeart.SetActive(true);
            newHeart.GetComponent<MMF_Player>()?.PlayFeedbacks();
        }
    }

    /// <summary>
    /// TechBooh:
    /// Position every heart relative to firstHeartPos.
    /// This HUD uses the initial marker's world-space position;
    /// preserve that reference so hearts do not move to screen center.
    /// </summary>
    private void RefreshHeartLayoutAndState_TechBooh(int visibleHearts)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            GameObject heart = hearts[i];
            if (heart == null)
            {
                continue;
            }

            PositionHeart_TechBooh(heart.transform, i);

            bool shouldBeVisible = i < visibleHearts;
            if (heart.activeSelf != shouldBeVisible)
            {
                heart.SetActive(shouldBeVisible);
            }
        }
    }

    private void PositionHeart_TechBooh(Transform heartTransform, int index)
    {
        heartTransform.localPosition = cachedFirstHeartLocalPosition_TechBooh + index * cachedHeartOffsetLocal_TechBooh;
    }

    private void IncreasePanelSize(int extraSlots)
    {
        if (extraSlots <= 0 || extraNumberOfHearts >= maxExpanderMultiplier)
        {
            return;
        }

        int sizeToIncrease = extraSlots;

        if (extraSlots + extraNumberOfHearts > maxExpanderMultiplier)
        {
            extraSlots = maxExpanderMultiplier - extraNumberOfHearts;
            sizeToIncrease = extraSlots;
        }

        if (sizeToIncrease == 0 && hasUiBeenUpdated == false)
        {
            sizeToIncrease = currentHealth - numberOfHpThatFitsTheInitialSize;
            if (sizeToIncrease < 0)
            {
                sizeToIncrease = 0;
            }
            else if (sizeToIncrease > maxExpanderMultiplier)
            {
                sizeToIncrease = maxExpanderMultiplier;
            }
        }

        if (sizeToIncrease <= 0)
        {
            return;
        }

        extraNumberOfHearts += extraSlots;
        hasUiBeenUpdated = true;

        if (animateUiCoroutine_TechBooh != null)
        {
            StopCoroutine(animateUiCoroutine_TechBooh);
        }

        animateUiCoroutine_TechBooh = StartCoroutine(AnimateUI(extraNumberOfHearts + 1));
    }

    private IEnumerator AnimateUI(int sizeToIncrease)
    {
        // TechBooh:
        // The frame needs extra width for its closing edge;
        // otherwise, it appears to fit one fewer heart.
        Vector2 targetSize = new(
            panelExtenderInitialSize.x + defaultWidthHpExtender * sizeToIncrease,
            panelExtenderInitialSize.y
        );

        Vector3 targetPos = new(
            panelExtenderInitialPos.x + sizeToIncrease * defaultWidthHpExtender / 2,
            panelExtenderInitialPos.y,
            panelExtenderInitialPos.z
        );

        Vector3 targetPosEnd = new(
            panelEndInitialPos.x + defaultWidthHpExtender * sizeToIncrease,
            panelEndInitialPos.y,
            panelEndInitialPos.z
        );

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / animationDuration);

            panelExtenderTransform.sizeDelta = Vector2.Lerp(currentPanelExtenderInitialSize, targetSize, t);
            panelExtenderTransform.localPosition = Vector3.Lerp(currentPanelExtenderInitialPos, targetPos, t);
            panelEndTransform.localPosition = Vector3.Lerp(currentPanelEndInitialPos, targetPosEnd, t);

            yield return null;
        }

        panelExtenderTransform.sizeDelta = targetSize;
        panelExtenderTransform.localPosition = targetPos;
        panelEndTransform.localPosition = targetPosEnd;

        currentPanelExtenderInitialSize = targetSize;
        currentPanelExtenderInitialPos = targetPos;
        currentPanelEndInitialPos = targetPosEnd;
        animateUiCoroutine_TechBooh = null;
    }
}
