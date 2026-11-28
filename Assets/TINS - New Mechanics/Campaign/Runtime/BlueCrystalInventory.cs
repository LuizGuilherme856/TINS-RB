using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>Crystals carried by the player until they are brought back to the tower.</summary>
public sealed class BlueCrystalInventory : MonoBehaviour
{
    [SerializeField, Min(0)] private int count;
    [Header("Player crystal orbit")]
    [SerializeField, Min(0.01f)] private float playerOrbitRadius = 1.1f;
    [SerializeField] private float playerOrbitSpeed = 24f;
    [SerializeField, Min(0f)] private float playerOrbitSpeedDifference = 2f;
    [SerializeField] private float playerOrbitPhaseStep = 137.5f;
    [Header("Tower deposit animation")]
    [SerializeField, Min(0.01f)] private float absorptionDuration = 0.32f;

    private readonly List<BlueCrystalPickup> carriedCrystals = new List<BlueCrystalPickup>();
    private bool isDepositing;

    public int Count => count;
    public bool HasCarriedCrystals => carriedCrystals.Count > 0;
    public event Action<int> OnCountChanged;

    public void Add(int amount = 1)
    {
        if (amount <= 0) return;
        count += amount;
        OnCountChanged?.Invoke(count);
    }

    public void Collect(BlueCrystalPickup crystal, int amount)
    {
        if (amount <= 0) return;

        count += amount;
        if (crystal != null && !carriedCrystals.Contains(crystal))
        {
            crystal.PlayCollectSound();
            carriedCrystals.Add(crystal);
            crystal.SetAsCarried(this, carriedCrystals.Count - 1);
        }

        OnCountChanged?.Invoke(count);
    }

    public float PlayerOrbitRadius => playerOrbitRadius;
    public float GetPlayerOrbitSpeed(int index)
    {
        if (index <= 0)
            return playerOrbitSpeed;

        int variationLevel = (index + 1) / 2;
        float direction = index % 2 == 1 ? 1f : -1f;
        return Mathf.Max(1f, playerOrbitSpeed + direction * variationLevel * playerOrbitSpeedDifference);
    }

    public float GetPlayerOrbitPhase(int index) => index * playerOrbitPhaseStep;

    public void BeginTowerOrbit(Vector3 center, float radius, float degreesPerSecond)
    {
        if (isDepositing || carriedCrystals.Count == 0) return;

        float angleStep = 360f / carriedCrystals.Count;
        for (int i = 0; i < carriedCrystals.Count; i++)
        {
            if (carriedCrystals[i] != null)
                carriedCrystals[i].SetOrbit(center, radius, degreesPerSecond, angleStep * i);
        }
    }

    public void StopTowerOrbit(bool playFailureFeedback = true)
    {
        if (isDepositing) return;
        for (int i = 0; i < carriedCrystals.Count; i++)
        {
            if (carriedCrystals[i] != null)
            {
                carriedCrystals[i].SetAsCarried(this, i);
                if (playFailureFeedback)
                    carriedCrystals[i].PlaySkillCheckFailureFeedback();
            }
        }
    }

    public IEnumerator AbsorbAllAt(Vector3 center)
    {
        if (isDepositing || count <= 0)
            yield break;

        isDepositing = true;
        float duration = Mathf.Max(0.01f, absorptionDuration);
        for (int i = 0; i < carriedCrystals.Count; i++)
        {
            if (carriedCrystals[i] != null)
                carriedCrystals[i].BeginAbsorption(center, duration);
        }

        if (carriedCrystals.Count > 0)
            yield return new WaitForSecondsRealtime(duration);

        for (int i = 0; i < carriedCrystals.Count; i++)
        {
            if (carriedCrystals[i] != null)
                Destroy(carriedCrystals[i].gameObject);
        }

        carriedCrystals.Clear();
        count = 0;
        isDepositing = false;
        OnCountChanged?.Invoke(count);
    }

    public bool TryAbsorbOneAt(Vector3 center, float duration, out IEnumerator transfer)
    {
        transfer = null;
        if (isDepositing || count <= 0) return false;
        isDepositing = true;
        transfer = AbsorbOneAt(center, duration);
        return true;
    }

    private IEnumerator AbsorbOneAt(Vector3 center, float duration)
    {
        float travelTime = Mathf.Max(0.01f, duration);
        BlueCrystalPickup crystal = carriedCrystals.Count > 0 ? carriedCrystals[0] : null;
        if (crystal != null)
        {
            crystal.BeginAbsorption(center, travelTime);
            yield return new WaitForSecondsRealtime(travelTime);
            carriedCrystals.RemoveAt(0);
            Destroy(crystal.gameObject);
            for (int i = 0; i < carriedCrystals.Count; i++)
                if (carriedCrystals[i] != null)
                    carriedCrystals[i].SetAsCarried(this, i);
        }

        count = Mathf.Max(0, count - 1);
        isDepositing = false;
        OnCountChanged?.Invoke(count);
    }

    public int TakeAll()
    {
        if (isDepositing) return 0;
        int delivered = count;
        if (delivered <= 0) return 0;

        count = 0;
        for (int i = 0; i < carriedCrystals.Count; i++)
        {
            if (carriedCrystals[i] != null)
                Destroy(carriedCrystals[i].gameObject);
        }
        carriedCrystals.Clear();
        OnCountChanged?.Invoke(count);
        return delivered;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < carriedCrystals.Count; i++)
        {
            if (carriedCrystals[i] != null)
                Destroy(carriedCrystals[i].gameObject);
        }
    }
}
