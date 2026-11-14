using TMPro;
using UnityEngine;

/// <summary>Displays the player's remaining beam melee uses in the HUD.</summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public sealed class BeamMeleeUsesHUD : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField, Min(0.05f)] private float refreshInterval = 0.15f;

    private BeamMeleeUsesManager observedManager;
    private float nextRefreshTime;

    private void Awake()
    {
        if (counterText == null) counterText = GetComponent<TextMeshProUGUI>();
        if (playerHolder == null) playerHolder = FindFirstObjectByType<PlayerHolder>();

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-36f, -150f);
        rect.sizeDelta = new Vector2(240f, 48f);

        counterText.fontSize = 25f;
        counterText.color = Color.white;
        counterText.alignment = TextAlignmentOptions.MidlineRight;
        counterText.raycastTarget = false;
        counterText.text = "Light 3/3";
    }

    private void OnEnable()
    {
        RefreshBinding();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefreshTime) return;
        nextRefreshTime = Time.unscaledTime + refreshInterval;
        RefreshBinding();
        UpdateCounter();
    }

    private void RefreshBinding()
    {
        if (playerHolder == null) playerHolder = FindFirstObjectByType<PlayerHolder>();
        GameObject player = playerHolder != null ? playerHolder.GetPlayerGameObject() : null;
        BeamMeleeUsesManager manager = player != null ? player.GetComponentInChildren<BeamMeleeUsesManager>(true) : null;
        if (manager == observedManager) return;

        Unbind();
        observedManager = manager;
        if (observedManager != null)
            observedManager.OnUsesChanged += HandleUsesChanged;
        UpdateCounter();
    }

    private void Unbind()
    {
        if (observedManager != null)
            observedManager.OnUsesChanged -= HandleUsesChanged;
        observedManager = null;
    }

    private void HandleUsesChanged(int remaining, int maximum)
    {
        if (counterText != null)
            counterText.text = $"Light {remaining}/{maximum}";
    }

    private void UpdateCounter()
    {
        if (counterText == null) return;
        if (observedManager == null)
        {
            counterText.text = "Light --/--";
            return;
        }

        counterText.text = $"Light {observedManager.UsesRemaining}/{observedManager.MaxUses}";
    }
}
