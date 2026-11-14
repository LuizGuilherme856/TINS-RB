using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableGameObject : MonoBehaviour
{
    [Tooltip("Lista de objetos que serão desativados.")]
    [SerializeField] private GameObject[] gameObjectsToDisable;

    [Tooltip("Define o momento em que os objetos devem ser desativados.")]
    [SerializeField] private ActivationTime activationTime;

    private void Awake()
    {
        if (activationTime == ActivationTime.Awake)
        {
            DisableObjects();
        }
    }

    private void Start()
    {
        if (activationTime == ActivationTime.Start)
        {
            DisableObjects();
        }
    }

    private void OnEnable()
    {
        if (activationTime == ActivationTime.OnEnable)
        {
            DisableObjects();
        }
    }

    /// <summary>
    /// Deactivate the listed objects.
    /// </summary>
    private void DisableObjects()
    {
        foreach (var obj in gameObjectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}
