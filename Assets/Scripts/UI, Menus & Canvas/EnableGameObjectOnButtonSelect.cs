using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Activate a GameObject when this object is selected.
/// Used for UI selection effects.
/// </summary>
public class EnableGameObjectOnButtonSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private GameObject objToEnable;

    private void OnEnable()
    {
        objToEnable.SetActive(false);
    }

    private void OnDisable()
    {
        objToEnable.SetActive(false);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        objToEnable.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        objToEnable.SetActive(true);
    }
}
