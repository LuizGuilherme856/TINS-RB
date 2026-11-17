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
        [Min(1f)] public float missionTargetRadius = 12f;
        [Min(0f)] public float enemySpeedMultiplier = 1f;
        public CampaignMissionEnemies enemySettings;
        public string rewardResourceId = "ether";
        public string rewardResourceName = "Éter";
        [Min(0)] public int rewardPerMission = 1;
        [Range(0f, 1f)] public float mapX = .5f;
        [Range(0f, 1f)] public float mapY = .5f;
    }

    [Serializable]
    public sealed class CampaignSkill
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        [Min(0)] public int soulCost = 10;
        public string requiredResourceId;
        [Min(0)] public int requiredResourceAmount;
        [Min(1)] public int maximumRank = 1;
        public string unlockedWeaponResourcePath;

        public int CostForRank(int currentRank) => Mathf.CeilToInt(Mathf.Max(0, soulCost) * Mathf.Pow(1.75f, Mathf.Max(0, currentRank)));
        public int ResourceCostForRank(int currentRank) => Mathf.CeilToInt(Mathf.Max(0, requiredResourceAmount) * Mathf.Pow(1.4f, Mathf.Max(0, currentRank)));
    }

    [CreateAssetMenu(menuName = "TINS/Campaign/Campaign Catalog")]
    public sealed class CampaignCatalog : ScriptableObject
    {
        public CampaignPlanet[] planets =
        {
            new CampaignPlanet { id="velaria", displayName="Velária", missionsToPurify=5, missionTargetRadius=11f, enemySpeedMultiplier=.70f, rewardResourceId="ether", rewardResourceName="Éter", rewardPerMission=1, mapX=.26f, mapY=.65f },
            new CampaignPlanet { id="nivora", displayName="Nivora", missionsToPurify=5, missionTargetRadius=13f, enemySpeedMultiplier=.72f, rewardResourceId="cristal-frio", rewardResourceName="Cristal Frio", rewardPerMission=2, mapX=.42f, mapY=.30f },
            new CampaignPlanet { id="asterim", displayName="Asterim", missionsToPurify=5, missionTargetRadius=15f, enemySpeedMultiplier=.74f, rewardResourceId="poeira-solar", rewardResourceName="Poeira Solar", rewardPerMission=2, mapX=.61f, mapY=.68f },
            new CampaignPlanet { id="dorsalia", displayName="Dorsália", missionsToPurify=5, missionTargetRadius=17f, enemySpeedMultiplier=.76f, rewardResourceId="fragmento-umbral", rewardResourceName="Fragmento Umbral", rewardPerMission=3, mapX=.72f, mapY=.43f },
            new CampaignPlanet { id="varkora", displayName="Varkora", missionsToPurify=5, missionTargetRadius=19f, enemySpeedMultiplier=.78f, rewardResourceId="nucleo-rubro", rewardResourceName="Núcleo Rubro", rewardPerMission=3, mapX=.83f, mapY=.69f },
            new CampaignPlanet { id="marevia", displayName="Marévia", missionsToPurify=5, missionTargetRadius=21f, enemySpeedMultiplier=.80f, rewardResourceId="sal-estelar", rewardResourceName="Sal Estelar", rewardPerMission=4, mapX=.18f, mapY=.38f }
        };

        public CampaignSkill[] skills =
        {
            new CampaignSkill { id="flash-duration", displayName="Flash Prolongado", description="+5% de duração do Flash e do atordoamento por nível.", soulCost=10, maximumRank=5 },
            new CampaignSkill { id="speed", displayName="Passos Livres", description="+5% de velocidade de movimento por nível.", soulCost=15, maximumRank=3 },
            new CampaignSkill { id="flash-capacity", displayName="Célula de Luz", description="+1 carga máxima do Flash por nível.", soulCost=20, maximumRank=4 },
            new CampaignSkill { id="flash-recharge", displayName="Foco Restaurado", description="−10% no tempo de recarga do Flash por nível.", soulCost=25, requiredResourceId="ether", requiredResourceAmount=1, maximumRank=3 },
            new CampaignSkill { id="solar-vigor", displayName="Vigor Solar", description="+1 de vida inicial e máxima por nível.", soulCost=30, requiredResourceId="poeira-solar", requiredResourceAmount=2, maximumRank=3 },
            new CampaignSkill { id="weapon-laser-pistol", displayName="Pistola Astral", description="Desbloqueia uma nova arma para futuras missões.", soulCost=180, requiredResourceId="ether", requiredResourceAmount=4, maximumRank=1, unlockedWeaponResourcePath="CampaignGuns/laser pistol_GunData" },
            new CampaignSkill { id="weapon-auto-pistol", displayName="Pistola de Rajada", description="Desbloqueia uma arma automática para futuras missões.", soulCost=450, requiredResourceId="cristal-frio", requiredResourceAmount=3, maximumRank=1, unlockedWeaponResourcePath="CampaignGuns/automatic pistol_GunData" }
        };

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
