using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calculate level-up XP requirements and award a skill point on level-up.
/// </summary>

[Obsolete]
public class LevelUpManager : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;

    [SerializeField] private List<int> xpToLevel;

    // Start is called before the first frame update
    void Start()
    {
        //CheckLevel();
    }


    private void CheckLevel()
    {
        for (int i = 0; i < xpToLevel.Count; i++)
        {
            if (playerData.xp >= xpToLevel[i] && playerData.level < i + 1)
            {
                playerData.AddSkillPoints(1);
                playerData.level = i + 1;
                Debug.Log($"Level up! New level: {playerData.level}, Skill points: {playerData.GetSkillPoints()}");
            }
        }
    }
}
