using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map_DoorIcon : MonoBehaviour
{
    [SerializeField] ClosedDoor doorToObserve;

    private void Start()
    {
        doorToObserve.OnOpenDoor += () => gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        if (doorToObserve.GetIsOpen())
        {
            gameObject.SetActive(false);
        }
    }
}
