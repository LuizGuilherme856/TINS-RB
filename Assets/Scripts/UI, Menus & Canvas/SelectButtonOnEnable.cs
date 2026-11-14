using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectButtonOnEnable : MonoBehaviour
{

    [SerializeField] private SelectEventSystemButton selectEventSystemButton;
    [SerializeField] private GameObject buttonGameObj;

    private void OnEnable()
    {
        
        selectEventSystemButton.SetButtonToSelect(buttonGameObj);
    }

    /// <summary>
    /// Calling selection directly during OnEnable previously activated the button;
    /// a short delay was introduced to avoid that unintended click.
    /// </summary>
    private void SetButtonMethod()
    {
        
    }

}
