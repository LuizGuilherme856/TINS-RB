using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// Track Survival-mode XP and levels,
/// calculate the XP requirement, and control
/// spawn rate for each level.
/// </summary>
public class XpManager_SurvivalMode : MonoBehaviour
{
    public event Action OnLevelUp, OnPickUpPoint;

    private readonly int DEFAULT_XP_VALUE = 10;
    public  int currentLevel { get; private set; } = 0;
    public int CurrentXpPoints { get; private set; } = 0;


    /// <summary>
    /// Use polynomial regression to approximate Vampire Survivors-style level progression.
    /// Formula: 0.2813*x^3 - 1.5593*x^2 + 35.3768*x - 17.1.
    /// Multiply by ten because each blood drop awards ten XP.
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public int CalculateNecessaryXpForLevel(int level)
    {
        if (level == 0) return 0;
        else if (level == 1) return 5 * DEFAULT_XP_VALUE;
        else if(level == 2) return 15* DEFAULT_XP_VALUE;
        else if (level == 3) return 30* DEFAULT_XP_VALUE;
        else if (level == 4) return 65 * DEFAULT_XP_VALUE;
        else if (level == 5) return 120* DEFAULT_XP_VALUE;

        float xp = DEFAULT_XP_VALUE * (0.2812f * Mathf.Pow(level, 3) - 1.5593f * level * level + 35.3768f * level - 17.1f);

        return Mathf.FloorToInt(Mathf.Abs(xp));
    }

    /// <summary>
    /// Return the current level's minimum XP threshold; for example,
    /// if level 2 requires 100 XP, its minimum is 100.
    /// </summary>
    /// <returns></returns>
    public int CurrentLevelMinXp()
    {
        return CalculateNecessaryXpForLevel((int)currentLevel);
    }

    public int NextLevelMinXp()
    {
        return CalculateNecessaryXpForLevel(((int)currentLevel) + 1);
    }
    public int GetCurrentLevel()
    {
        return currentLevel;

    }

    public void CheckLevelUp()
    {
        if (CurrentXpPoints >= CalculateNecessaryXpForLevel(currentLevel + 1))
        {
            currentLevel++;
            OnLevelUp?.Invoke();
        }
    }

    public void AddXpPoint()
   {
        CurrentXpPoints += DEFAULT_XP_VALUE;
        OnPickUpPoint.Invoke();

        CheckLevelUp();
        Debug.Log("Points: " + CurrentXpPoints);
    }

    
}