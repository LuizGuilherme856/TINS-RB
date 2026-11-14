using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HpBarPlayerHead : MonoBehaviour
{
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform firstHeartPos;
    [SerializeField] private Vector3 newHeartPosOffSet;
    [SerializeField] private GameObject plusIndicator;


    [SerializeField] private Health health;
    [SerializeField] private MMF_Player feedback;
    private List<GameObject> hearts = new List<GameObject>();
    [SerializeField] private int maxHearts = 10;

    private void Awake()
    {
        CreateHearts();
        
    }

    private void OnEnable()
    {
        // TechBooh: avoid duplicate subscriptions when this HUD is re-enabled.
        health.OnHealthChange -= UpdateHpIndicator;
        health.OnHealthChange += UpdateHpIndicator;

        transform.parent = null;

        UpdateHpIndicator();
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChange -= UpdateHpIndicator;
        }
    }

    private void CreateHearts()
    {
        plusIndicator.SetActive(false);
        for (int i = 0; i < maxHearts; i++)
        {
            var newHeart = Instantiate(heartPrefab, transform);
            newHeart.transform.localPosition = firstHeartPos.localPosition + newHeartPosOffSet * i;
            newHeart.SetActive(true);
            hearts.Add(newHeart);
        }
    }

    private void UpdateHpIndicator()
    {
        for (int i = 0; i < maxHearts; i++)
        {
            if(i < health.GetCurrentHealth())
            {
                hearts[i].SetActive(true);
            }
            else
            {
                hearts[i].SetActive(false);
            }
        }

        if(health.GetCurrentHealth() > maxHearts)
        {
            plusIndicator.SetActive(true);
        }
        else
        {
            plusIndicator.SetActive(false);
        }

        feedback.PlayFeedbacks();
    }
}
