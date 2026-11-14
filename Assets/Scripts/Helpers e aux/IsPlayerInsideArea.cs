using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsPlayerInsideArea : MonoBehaviour
{

    [SerializeField] private BoxCollider2D area;
    public event Action OnPlayerIsInsideArea, OnPlayerLeftArea;
    public float TimeInsideArea { get; private set; }
    public float TimeOutsideArea { get; private set; }
    public bool IsInside { get; private set; }


    private void Update()
    {
        if (IsInside)
        {
            TimeInsideArea += Time.deltaTime;
        }
        else
        {
            TimeOutsideArea += Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnPlayerIsInsideArea?.Invoke();
            IsInside = true;
            TimeOutsideArea = 0;
            TimeInsideArea = 0;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnPlayerLeftArea?.Invoke();
            IsInside= false;
            TimeOutsideArea = 0;
            TimeInsideArea = 0;
        }
    }

}
