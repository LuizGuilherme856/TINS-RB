using UnityEngine;

/// <summary>Updates sprite sorting based on minPosition's Y coordinate.</summary>
public class SpriteLayerByYValue : MonoBehaviour
{
    [SerializeField] private Transform minPosition;
    [SerializeField] private bool isStatic = true;
    [SerializeField] private float addToLayer = 0f;
    [SerializeField] private float updateRate = 0.2f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isVisible = true;

    private void Awake()
    {
        ResolveReferences();
    }

    private bool ResolveReferences()
    {
        if (minPosition == null) minPosition = transform;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>(true);
        return spriteRenderer != null;
    }

    private void OnEnable()
    {
        if (!ResolveReferences()) return;

        if (isStatic)
            UpdateSortingLayer();
        else
            InvokeRepeating(nameof(UpdateSortingLayer), 0f, Mathf.Max(0.02f, updateRate));
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(UpdateSortingLayer));
    }

    private void UpdateSortingLayer()
    {
        if (!isVisible || !ResolveReferences()) return;
        spriteRenderer.sortingOrder = Mathf.RoundToInt(minPosition.position.y * 35f + addToLayer) * -1;
    }

    private void OnBecameInvisible() => isVisible = false;
    private void OnBecameVisible() => isVisible = true;
}