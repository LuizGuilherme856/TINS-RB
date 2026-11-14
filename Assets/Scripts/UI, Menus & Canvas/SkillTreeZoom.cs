using System.Collections.Generic;
using UnityEngine;

public class SkillTreeZoom : MonoBehaviour
{
    [Header("Zoom Config")]
    [SerializeField] private float zoomSpeed = 1f;
    //[SerializeField] private float zoomStepIncrease = 0.1f;
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 10f;

    [Header("Referências")]
    [Tooltip("Lista de RectTransforms que sofrerão zoom")]
    [SerializeField] private List<RectTransform> targetRectTransforms = new List<RectTransform>();

    [SerializeField] private float currentZoom = 0.25f;

    private void Start()
    {
        // If the list is empty, use this GameObject's RectTransform.
        if (targetRectTransforms.Count == 0)
        {
            RectTransform ownTransform = GetComponent<RectTransform>();
            if (ownTransform != null)
            {
                targetRectTransforms.Add(ownTransform);
            }
        }

        // Initialize zoom from the first RectTransform.
        if (targetRectTransforms.Count > 0 && targetRectTransforms[0] != null)
        {
            currentZoom = targetRectTransforms[0].localScale.x;
        }
    }

    private void Update()
    {
        // Read mouse scrolling.
        float scrollInput = Input.mouseScrollDelta.y;

        if (scrollInput != 0)
        {
            // Negative scrolling zooms out; positive scrolling zooms in.
            currentZoom += scrollInput * zoomSpeed ;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            ApplyZoom();
        }




    }

    ///// <summary>
    ///// Increase zoom by zoomSpeed.
    ///// </summary>
    //public void ZoomIn()
    //{
    //    currentZoom += zoomStepIncrease;
    //    currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    //    ApplyZoom();
    //}

    ///// <summary>
    ///// Decrease zoom by zoomSpeed.
    ///// </summary>
    //public void ZoomOut()
    //{
    //    currentZoom -= zoomStepIncrease;
    //    currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    //    ApplyZoom();
    //}

    public void AddToZoom(float increment)
    {
        currentZoom += increment;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        ApplyZoom();

    }

    private void ApplyZoom()
    {
        // Apply scale to every listed RectTransform.
        foreach (var rectTransform in targetRectTransforms)
        {
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one * currentZoom;
            }
        }
    }

    /// <summary>
    /// Set a specific zoom value.
    /// </summary>
    public void SetZoom(float newZoom)
    {
        currentZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
        ApplyZoom();
    }

    /// <summary>
    /// Reset zoom to 1.
    /// </summary>
    public void ResetZoom()
    {
        SetZoom(1f);
    }

    /// <summary>
    /// Add a RectTransform to the zoom targets.
    /// </summary>
    public void AddZoomTarget(RectTransform rectTransform)
    {
        if (rectTransform != null && !targetRectTransforms.Contains(rectTransform))
        {
            targetRectTransforms.Add(rectTransform);
        }
    }

    /// <summary>
    /// Remove a RectTransform from the zoom targets.
    /// </summary>
    public void RemoveZoomTarget(RectTransform rectTransform)
    {
        if (rectTransform != null)
        {
            targetRectTransforms.Remove(rectTransform);
        }
    }

    public float GetCurrentZoom() => currentZoom;
}
