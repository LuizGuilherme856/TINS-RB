using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activates a list of GameObjects at a configured MonoBehaviour lifecycle stage.
/// Activation can occur in Awake, Start, or OnEnable.
/// </summary>
public class EnableGameObject : MonoBehaviour
{


    [Header("Configurações")]
    /// <summary>
    /// Lifecycle stage used for activation.
    /// </summary>
    public ActivationTime activationTime;

    /// <summary>
    /// GameObjects activated at the configured stage.
    /// </summary>
    public List<GameObject> gameObjectsToEnable;

    /// <summary>
    /// Called when the script initializes.
    /// Activate the objects if Awake is selected.
    /// </summary>
    private void Awake()
    {
        if (activationTime == ActivationTime.Awake)
        {
            EnableGameObjects();
        }
    }

    /// <summary>
    /// Called before the first frame update while active.
    /// Activate the objects if Start is selected.
    /// </summary>
    private void Start()
    {
        if (activationTime == ActivationTime.Start)
        {
            EnableGameObjects();
        }
    }

    /// <summary>
    /// Called whenever the associated GameObject is enabled.
    /// Activate the objects if OnEnable is selected.
    /// </summary>
    private void OnEnable()
    {
        if (activationTime == ActivationTime.OnEnable)
        {
            EnableGameObjects();
        }
    }

    /// <summary>
    /// Activate all objects in <see cref="gameObjectsToEnable"/>.
    /// Skip null objects.
    /// </summary>
    private void EnableGameObjects()
    {
        foreach (var obj in gameObjectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
}

/// <summary>
/// Supported activation lifecycle stages.
/// </summary>
public enum ActivationTime
{
    /// <summary>Activate GameObjects in Awake.</summary>
    Awake,
    /// <summary>Activate GameObjects in Start.</summary>
    Start,
    /// <summary>Activate GameObjects in OnEnable.</summary>
    OnEnable
}