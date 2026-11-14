using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Cinemachine camera zoom helpers.
/// </summary>
public class CameraZoom : MonoBehaviour
{
    [SerializeField] private CinemachineCamera virtualCam;
    [SerializeField] private float _transitionDuration, defaultZoom;
    [SerializeField] private AnimationCurve zoomCurve;


    private Coroutine zoomCoroutine;
    public bool IsZooming_TechBooh { get; private set; }

    private void Start()
    {
        // Preserve the initial OrthographicSize.
        defaultZoom = virtualCam.Lens.OrthographicSize;
    }

    /// <summary>
    /// Zooms the camera to a specified field of view over a duration.
    /// </summary>
    /// <param name="duration">Time to reach the requested FOV.</param>
    /// <param name="fov">Defaults to 10.</param>
    /// <param name="delay">Delay before zooming begins.</param>

    public void Zoom(float duration, float fov, float delay = 0)
    {
        #region values Checks

        if (virtualCam == null)
        {
            Debug.LogError("CinemachineVirtualCamera is not assigned.");
            return;
        }

        if (duration <= 0)
        {
            Debug.LogWarning("Duration must be greater than zero. Using default duration.");
            duration = _transitionDuration;
        }

        if(delay < 0)
        {
            Debug.LogWarning("Delay cannot be negative. Using default delay of 0.");
            delay = 0;
        }

        if (fov <= 0)
        {
            Debug.LogWarning("Field of view must be greater than zero. Using default zoom value.");
            fov = defaultZoom;
        }

        #endregion

        StartCoroutine(Zoom_Routine(duration, fov, delay));
    }

    private IEnumerator Zoom_Routine(float duration, float fov, float delay)
    {
        IsZooming_TechBooh = true;
        yield return new WaitForSeconds(delay);
        StartZoomCoroutine(duration, fov);
    }



    public void ResetZoom(float duration)
    {
        StartZoomCoroutine(duration, defaultZoom);
    }

    private void StartZoomCoroutine(float duration, float targetFov)
    {
        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);

        zoomCoroutine = StartCoroutine(ZoomRoutine(duration, targetFov));
    }

    private IEnumerator ZoomRoutine(float duration, float targetFov)
    {
        float startFov = virtualCam.Lens.OrthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveValue = zoomCurve.Evaluate(t);
            virtualCam.Lens.OrthographicSize = Mathf.LerpUnclamped(startFov, targetFov, curveValue);
            yield return null;
        }

        virtualCam.Lens.OrthographicSize = targetFov;
        IsZooming_TechBooh = false;
        zoomCoroutine = null;
    }
}

