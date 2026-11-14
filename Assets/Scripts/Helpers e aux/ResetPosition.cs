using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    [SerializeField] private whichPos posType;
    [SerializeField] private method when;
    [SerializeField] private float waitSeconds = 0f; // Delay before resetting.

    private enum method { awake, onEnable, start };
    private enum whichPos { localPosition, position, rectTransformLocalPosition, rectTransformAnchoredPosition };

    private void Awake()
    {
        if (when == method.awake)
        {
            TryResetWithDelay();
        }
    }

    private void OnEnable()
    {
        if (when == method.onEnable)
        {
            TryResetWithDelay();
        }
    }

    void Start()
    {
        if (when == method.start)
        {
            TryResetWithDelay();
        }
    }

    private void TryResetWithDelay()
    {
        if (waitSeconds > 0f)
        {
            StartCoroutine(ResetPosAfterDelay(waitSeconds));
        }
        else
        {
            ResetPos();
        }
    }

    private IEnumerator ResetPosAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ResetPos();
    }

    public void ResetPos()
    {
        switch (posType)
        {
            case whichPos.position:
                transform.position = Vector3.zero;
                break;
            case whichPos.localPosition:
                transform.localPosition = Vector3.zero;
                break;
            case whichPos.rectTransformLocalPosition:
                {
                    RectTransform rect = GetComponent<RectTransform>();
                    if (rect != null)
                        rect.localPosition = Vector3.zero;
                }
                break;
            case whichPos.rectTransformAnchoredPosition:
                {
                    RectTransform rect = GetComponent<RectTransform>();
                    if (rect != null)
                        rect.anchoredPosition = Vector2.zero;
                }
                break;
        }
    }
}
