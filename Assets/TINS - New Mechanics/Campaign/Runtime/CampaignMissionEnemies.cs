using System;
using System.Collections.Generic;
using UnityEngine;

namespace TINS.Campaign
{
    [Serializable]
    public sealed class CampaignEnemyEntry
    {
        [Tooltip("Enemy prefab with EnemyHealth and PoolCreator on the root.")]
        public GameObject prefab;
        [Min(0f), Tooltip("Relative chance among the enemies available at the current light radius.")]
        public float weight = 1f;
        [Min(0f), Tooltip("Set to zero to make this enemy available from the start.")]
        public float minimumLightRadius;
    }

    [CreateAssetMenu(fileName = "Mission Enemies", menuName = "TINS/Campaign/Mission Enemies")]
    public sealed class CampaignMissionEnemies : ScriptableObject
    {
        [Header("Initial enemies")]
        [Min(0), Tooltip("Enemies spawned immediately. This is capped by the initial active enemy limit.")]
        public int initialEnemyCount = 3;
        [Min(1)] public int initialActiveEnemyLimit = 7;
        [Min(0.1f), Tooltip("Seconds between spawns at the beginning of the mission.")]
        public float initialSpawnInterval = 1.65f;
        [Min(0.1f), Tooltip("1 keeps the movement speed defined by each enemy.")]
        public float movementSpeedMultiplier = 1.65f;
        [Min(0.01f), Tooltip("1 keeps the base health defined by each enemy at the start of the mission.")]
        public float healthMultiplier = 2.4f;

        [Header("Difficulty growth")]
        [Min(1), Tooltip("Absolute cap on simultaneous enemies. Never lower than the initial limit.")]
        public int maximumActiveEnemies = 15;
        [Min(0f)] public float extraActiveEnemiesPerLightUnit = 1.3f;
        [Min(0f)] public float secondsRemovedPerLightUnit = 0.085f;
        [Min(0.1f)] public float minimumSpawnInterval = 0.85f;
        [Min(0f), Tooltip("Additional fraction of initial health per wave. 0.2 adds 20% each wave.")]
        public float extraHealthPerWave = 0.10f;

        [Header("Reinforcements")]
        [Min(0), Tooltip("After this many successful regular spawns, send a group. Zero disables groups.")]
        public int spawnsBetweenReinforcements;
        [Min(0)] public int extraEnemiesInGroup;
        [Min(0f)] public float minimumReinforcementRadius;

        [Header("Enemy roster")]
        public List<CampaignEnemyEntry> enemies = new List<CampaignEnemyEntry>();

        public int ActiveEnemyLimit(float lightGrowth)
        {
            int initial = Mathf.Max(1, initialActiveEnemyLimit);
            int maximum = Mathf.Max(initial, maximumActiveEnemies);
            return initial + Mathf.FloorToInt(Mathf.Min(maximum - initial,
                Mathf.Max(0f, lightGrowth) * Mathf.Max(0f, extraActiveEnemiesPerLightUnit)));
        }

        public float SpawnInterval(float lightGrowth)
        {
            float initial = Mathf.Max(0.1f, initialSpawnInterval);
            float minimum = Mathf.Clamp(minimumSpawnInterval, 0.1f, initial);
            return Mathf.Max(minimum, initial - Mathf.Max(0f, lightGrowth) * Mathf.Max(0f, secondsRemovedPerLightUnit));
        }
    }
}
