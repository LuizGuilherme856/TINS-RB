using System;
using UnityEngine;

namespace TINS.Campaign
{
    [Serializable]
    public sealed class CampaignPlanet
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        [Min(1)] public int missionsToPurify = 5;
        [Tooltip("IDs of the planets that must be purified to unlock this route.")]
        public string[] prerequisitePlanetIds = Array.Empty<string>();
        [Header("Light and Penumbra (world units from the tower)")]
        [Min(0.01f), Tooltip("Illuminated radius at the start of this planet's missions.")]
        public float initialLightRadius = 3.5f;
        [Min(1f), InspectorName("Final Light Radius"), Tooltip("Illuminated radius required to win the mission.")]
        public float missionTargetRadius = 12f;
        [Min(0.1f), Tooltip("Outer penumbra radius at mission start. The Breu and movement boundary begin here.")]
        public float initialPenumbraRadius = 6f;
        [Min(0.1f), Tooltip("Outer penumbra radius when the light reaches Final Light Radius. Interpolates during purification.")]
        public float finalPenumbraRadius = 14.5f;
        [Header("Mission terrain")]
        [Tooltip("Optional 16×16 floor atlas. Rows from top to bottom: purified, lit, penumbra, and Breu.")]
        public Texture2D floorTiles;
        [Range(0f, 1f), Tooltip("Fraction of the illuminated radius that displays purified grass.")]
        public float purifiedGrassRadiusRatio = .6f;
        [Min(0.01f), Tooltip("World units of light radius gained for each soul delivered.")]
        public float lightGrowthPerSoul = .15f;
        [Range(0f, 1f), Tooltip("Chance for a defeated enemy to drop a soul before skill bonuses.")]
        public float soulDropChance = .8f;
        [Min(0f)] public float enemySpeedMultiplier = 1f;
        public CampaignMissionEnemies enemySettings;
        public string rewardResourceId = "ether";
        public string rewardResourceName = "Éter";
        [Min(0)] public int rewardPerMission = 1;
        [Min(0)] public int victorySouls;
        [Range(0f, 1f)] public float mapX = .5f;
        [Range(0f, 1f)] public float mapY = .5f;
        [Header("Resource caches")]
        [Tooltip("Sprite for a cache containing this planet's reward material.")]
        public Sprite resourceCacheSprite;
        [Min(0), Tooltip("Number of caches scattered per mission. Set to zero to disable.")]
        public int resourceCacheCount = 3;
        [Min(1), Tooltip("Amount of this planet's material collected per cache.")]
        public int resourcePerCache = 1;

        public void ValidateRadii()
        {
            initialLightRadius = Mathf.Max(0.01f, initialLightRadius);
            missionTargetRadius = Mathf.Max(1f, initialLightRadius + 0.1f, missionTargetRadius);
            initialPenumbraRadius = Mathf.Max(initialLightRadius + 0.1f, initialPenumbraRadius);
            finalPenumbraRadius = Mathf.Max(missionTargetRadius + 0.1f, initialPenumbraRadius, finalPenumbraRadius);
        }

        public float PenumbraWidthAt(float lightRadius)
        {
            float progress = Mathf.InverseLerp(initialLightRadius, missionTargetRadius, lightRadius);
            return Mathf.Lerp(Mathf.Max(0.1f, initialPenumbraRadius - initialLightRadius),
                Mathf.Max(0.1f, finalPenumbraRadius - missionTargetRadius), progress);
        }
    }

    [CreateAssetMenu(menuName = "TINS/Campaign/Campaign Catalog")]
    public sealed class CampaignCatalog : ScriptableObject
    {
        public CampaignPlanet[] planets =
        {
            new CampaignPlanet { id="velaria", displayName="Velária", initialLightRadius=4f, initialPenumbraRadius=12.17f, missionTargetRadius=15.4f, finalPenumbraRadius=20.4f, enemySpeedMultiplier=1.65f, victorySouls=22, rewardResourceId="ether", rewardResourceName="Éter", rewardPerMission=5, mapX=.26f, mapY=.65f },
            new CampaignPlanet { id="nivora", displayName="Nivora", prerequisitePlanetIds=new[] { "velaria" }, missionTargetRadius=20f, finalPenumbraRadius=23.5f, lightGrowthPerSoul=.17f, soulDropChance=.82f, enemySpeedMultiplier=1.68f, victorySouls=55, rewardResourceId="cristal-frio", rewardResourceName="Cristal Frio", rewardPerMission=4, mapX=.42f, mapY=.30f },
            new CampaignPlanet { id="asterim", displayName="Asterim", prerequisitePlanetIds=new[] { "nivora" }, missionTargetRadius=22f, finalPenumbraRadius=26f, lightGrowthPerSoul=.18f, soulDropChance=.85f, enemySpeedMultiplier=1.7f, victorySouls=76, rewardResourceId="poeira-solar", rewardResourceName="Poeira Estelar", rewardPerMission=2, mapX=.61f, mapY=.68f },
            new CampaignPlanet { id="dorsalia", displayName="Dorsália", prerequisitePlanetIds=new[] { "marevia" }, missionTargetRadius=24f, finalPenumbraRadius=28.5f, lightGrowthPerSoul=.19f, soulDropChance=.88f, enemySpeedMultiplier=1.72f, victorySouls=98, rewardResourceId="fragmento-umbral", rewardResourceName="Fragmento Umbral", rewardPerMission=3, mapX=.72f, mapY=.43f },
            new CampaignPlanet { id="varkora", displayName="Varkora", prerequisitePlanetIds=new[] { "asterim", "dorsalia" }, missionTargetRadius=26f, finalPenumbraRadius=31f, lightGrowthPerSoul=.2f, soulDropChance=.9f, enemySpeedMultiplier=1.74f, victorySouls=126, rewardResourceId="nucleo-rubro", rewardResourceName="Núcleo Rubro", rewardPerMission=1, mapX=.83f, mapY=.69f },
            new CampaignPlanet { id="marevia", displayName="Marévia", prerequisitePlanetIds=new[] { "velaria" }, missionTargetRadius=21f, finalPenumbraRadius=25f, lightGrowthPerSoul=.17f, soulDropChance=.83f, enemySpeedMultiplier=1.68f, victorySouls=60, rewardResourceId="sal-estelar", rewardResourceName="Granalita", rewardPerMission=6, mapX=.18f, mapY=.38f }
        };

        public const string FlashAreaSkillId = "flash-area";

        internal static string ResourceIcon(string id)
        {
            string name = id?.ToLowerInvariant();
            return name switch
            {
                "almas" or "ether" or "cristal-frio" or "poeira-solar" or "fragmento-umbral" or "nucleo-rubro" or "sal-estelar"
                    => $"<sprite=\"Campaign Resource Icons\" name=\"{name}\"> ",
                _ => string.Empty,
            };
        }

        [Tooltip("Sprite shared by all caches after they are collected.")]
        public Sprite depletedCacheSprite;
        public CampaignSkill[] skills = System.Array.Empty<CampaignSkill>();

        public CampaignSkill FlashSkill => Array.Find(skills ?? System.Array.Empty<CampaignSkill>(),
            skill => skill != null && skill.id == CampaignProfile.FlashSkillId);

        private void OnValidate()
        {
            if (planets != null)
                foreach (CampaignPlanet planet in planets) planet?.ValidateRadii();
            ValidateSkillDefinitions();
        }

        [ContextMenu("Validate Campaign Skill Tree")]
        private void ValidateSkillDefinitions()
        {
            if (skills == null) return;
            System.Collections.Generic.Dictionary<string, CampaignSkill> byId = new System.Collections.Generic.Dictionary<string, CampaignSkill>(StringComparer.OrdinalIgnoreCase);
            foreach (CampaignSkill skill in skills)
            {
                if (skill == null) continue;
                if (string.IsNullOrWhiteSpace(skill.id))
                {
                    Debug.LogError("A campaign skill has no permanent ID.", this);
                    continue;
                }
                if (byId.ContainsKey(skill.id)) Debug.LogError("Duplicate campaign skill ID: " + skill.id, skill);
                else byId.Add(skill.id, skill);
            }

            foreach (CampaignSkill skill in skills)
            {
                if (skill == null) continue;
                ValidateRequirements(skill, skill.prerequisites, byId, "purchase");
                ValidateRequirements(skill, skill.revealPrerequisites, byId, "reveal");
            }
            var visiting = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var visited = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (CampaignSkill skill in skills)
                if (skill != null && HasPrerequisiteCycle(skill, byId, visiting, visited))
                {
                    Debug.LogError("Campaign skill prerequisites contain a cycle near: " + skill.id, skill);
                    break;
                }
        }

        private void ValidateRequirements(CampaignSkill owner, CampaignSkillPrerequisite[] requirements,
            System.Collections.Generic.Dictionary<string, CampaignSkill> byId, string kind)
        {
            if (requirements == null) return;
            foreach (CampaignSkillPrerequisite requirement in requirements)
            {
                if (requirement == null || requirement.skill == null)
                {
                    Debug.LogError($"Campaign skill '{owner.id}' has a missing {kind} prerequisite.", owner);
                    continue;
                }
                if (requirement.minimumRank < 1)
                    Debug.LogError($"Campaign skill '{owner.id}' has a {kind} prerequisite rank below 1.", owner);
                if (requirement.minimumRank > requirement.skill.maximumRank)
                    Debug.LogError($"Campaign skill '{owner.id}' requires rank {requirement.minimumRank} from '{requirement.skill.id}', whose maximum is {requirement.skill.maximumRank}.", owner);
                if (!byId.ContainsKey(requirement.skill.id))
                    Debug.LogError($"Campaign skill '{owner.id}' references '{requirement.skill.id}', which is not in this catalog.", owner);
                if (requirement.skill == owner)
                    Debug.LogError($"Campaign skill '{owner.id}' cannot depend on itself.", owner);
            }
        }

        private static bool HasPrerequisiteCycle(CampaignSkill skill,
            System.Collections.Generic.Dictionary<string, CampaignSkill> byId,
            System.Collections.Generic.HashSet<string> visiting,
            System.Collections.Generic.HashSet<string> visited)
        {
            if (visiting.Contains(skill.id)) return true;
            if (!visited.Add(skill.id)) return false;
            visiting.Add(skill.id);
            if (HasCycleIn(skill.prerequisites, byId, visiting, visited) ||
                HasCycleIn(skill.revealPrerequisites, byId, visiting, visited)) return true;
            visiting.Remove(skill.id);
            return false;
        }

        private static bool HasCycleIn(CampaignSkillPrerequisite[] requirements,
            System.Collections.Generic.Dictionary<string, CampaignSkill> byId,
            System.Collections.Generic.HashSet<string> visiting,
            System.Collections.Generic.HashSet<string> visited)
        {
            if (requirements == null) return false;
            foreach (CampaignSkillPrerequisite requirement in requirements)
                if (requirement != null && requirement.skill != null && byId.TryGetValue(requirement.skill.id, out CampaignSkill dependency) &&
                    HasPrerequisiteCycle(dependency, byId, visiting, visited)) return true;
            return false;
        }

        public CampaignPlanet GetPlanet(string id)
        {
            if (planets == null) return null;
            foreach (CampaignPlanet planet in planets)
                if (planet != null && string.Equals(planet.id, id, StringComparison.OrdinalIgnoreCase)) return planet;
            return null;
        }

        public int IndexOf(string id)
        {
            if (planets == null) return -1;
            for (int i = 0; i < planets.Length; i++)
                if (planets[i] != null && string.Equals(planets[i].id, id, StringComparison.OrdinalIgnoreCase)) return i;
            return -1;
        }
    }
}
