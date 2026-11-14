using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Manage blood points earned from kills and spent on purchases.
/// Also handle XP used for skill-tree progression.
/// </summary>
public class PointManager : MonoBehaviour
{
    [SerializeField] private PointData pointData;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private bool showDebug = true;
    [SerializeField] private GlobalStats globalStats;
    [SerializeField] private RunData runData;
    public int currentPoints { get; private set; }
    public int lastPointsAdded { get; private set; }
    public event Action OnPointChange;

    private void Awake()
    {
        if (globalStats == null)
        {
            globalStats = AssetFinder.GetGlobalStats();
            Debug.LogWarning($"O obj {gameObject.name} nao tem global stats. Procurando manualmente. Por favor" +
                $"coloque pelo inspector pra poupar performance");
        }
    }

    private int pointMultiplier= 1;
    private int numberOfDoublePointsActive = 0;
    public int GetInitialPoints(int initialPoints = 0)
    {
        return initialPoints + globalStats.extraInitialPoints;
    }
    public void AddXp(int xp)
    {
        playerData.xp += xp;
        runData.AddXp(xp);

    }


    public void AddKillPoint(DamageType damageType, bool melee)
    {
        int newPoints = 0;

        if (melee)
        {
            newPoints += pointData.GetKillPoints(KillType.melee) * pointMultiplier;
        }
        else
        {
            if (damageType == DamageType.Explosive)
            {
                newPoints += pointData.GetKillPoints(KillType.explosion) * pointMultiplier;
            }
            else
            {
                newPoints += pointData.GetKillPoints(KillType.shot) * pointMultiplier;
            }
        }

        newPoints += globalStats.extraPointsPerKill; //A blessing.

        currentPoints += newPoints;
        lastPointsAdded = newPoints;

        runData.AddPointsInThisRun(newPoints);

        OnPointChange?.Invoke();

        if (showDebug) Aux.Print("pontos: " + currentPoints, showDebug);
    }

    public void AddHitPoint(bool melee)
    {
        int newPoints = 0;

        if (melee)
        {
            newPoints += pointData.GetHitPoints(HitType.melee)*pointMultiplier;
        }
        else
        {
            newPoints += pointData.GetHitPoints(HitType.shot)*pointMultiplier;
        }

        currentPoints += newPoints;
        lastPointsAdded = newPoints;
        runData.AddPointsInThisRun(newPoints);


        OnPointChange?.Invoke();

        if (showDebug) Aux.Print("pontos: " + currentPoints, showDebug);
    }

    public void RemovePoints(int amount)
    {
        if(amount <= currentPoints)
        {
            currentPoints -= amount;
            lastPointsAdded = -amount;
            OnPointChange?.Invoke();
        }
    }

    public void SetPoints(int amount)
    {
        currentPoints = amount;
        lastPointsAdded = amount;
        OnPointChange?.Invoke();
    }

    public void AddPoint(int amount)
    {
        if (amount > 0)
        {
            runData.AddPointsInThisRun(amount);
        }
       
       
        currentPoints += amount;
        lastPointsAdded = amount;
        OnPointChange?.Invoke();
    }

    /// <summary>
    /// The point-multiplier power-up stacks additively
    /// when multiple copies are active.
    /// </summary>

    public void SetPointMultiplier(int pointMultiplier, float duration)
    {
        numberOfDoublePointsActive++;
        this.pointMultiplier = pointMultiplier * numberOfDoublePointsActive;
        Invoke(nameof(UpdatePointMultiplier), duration);
    }

    private void UpdatePointMultiplier()
    {
        numberOfDoublePointsActive--;
        if (numberOfDoublePointsActive <= 0)
        {
            numberOfDoublePointsActive = 0;//Defensive check.
            pointMultiplier = 1;
        }
        else
        {
            int damageMultiplier_gambiarra = 5; //TODO: Obtain this value from SetDamageMultiplierDuration instead of hard-coding 5.
            pointMultiplier = damageMultiplier_gambiarra * numberOfDoublePointsActive;
        }
    }

    public void CHEAT_AddPoint(int amount)
    {
        currentPoints += amount;
        OnPointChange?.Invoke();
    }

    public void CHEAT_ResetPoints()
    {
        currentPoints = 0;
        OnPointChange?.Invoke();
    }
}

