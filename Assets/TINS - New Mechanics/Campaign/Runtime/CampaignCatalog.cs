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
        [Min(0.01f)] public float lightGrowthPerSoul = 0.5f;
        [Min(0f)] public float enemySpeedMultiplier = 1f;
        public CampaignMissionEnemies enemySettings;
        public string rewardResourceId = "ether";
        public string rewardResourceName = "Éter";
        [Min(0)] public int rewardPerMission = 1;
        [Min(0)] public int victorySouls;
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
        [Min(0)] public int unlockCost;
        public bool requiresFlash;
        public string requiredResourceId;
        [Min(0)] public int requiredResourceAmount;
        [Min(1), Tooltip("First purchased level that requires this material.")] public int resourceStartsAtRank = 1;
        [Min(1)] public int maximumRank = 1;
        public string unlockedWeaponResourcePath;

        public int CostForRank(int currentRank)
        {
            if (unlockCost > 0 && currentRank == 0) return unlockCost;
            int upgradeRank = currentRank - (unlockCost > 0 ? 1 : 0);
            return Mathf.CeilToInt(Mathf.Max(0, soulCost) * Mathf.Pow(1.75f, Mathf.Max(0, upgradeRank)));
        }
        public int ResourceCostForRank(int currentRank)
        {
            int paidMaterialRank = currentRank + 1 - Mathf.Max(1, resourceStartsAtRank);
            return paidMaterialRank < 0 ? 0 :
                Mathf.CeilToInt(Mathf.Max(0, requiredResourceAmount) * Mathf.Pow(1.4f, paidMaterialRank));
        }
    }

    [CreateAssetMenu(menuName = "TINS/Campaign/Campaign Catalog")]
    public sealed class CampaignCatalog : ScriptableObject
    {
        public CampaignPlanet[] planets =
        {
            new CampaignPlanet { id="velaria", displayName="Velária", missionsToPurify=5, missionTargetRadius=11f, lightGrowthPerSoul=0.5f, enemySpeedMultiplier=1.65f, victorySouls=22, rewardResourceId="ether", rewardResourceName="Éter", rewardPerMission=1, mapX=.26f, mapY=.65f },
            new CampaignPlanet { id="nivora", displayName="Nivora", missionsToPurify=5, missionTargetRadius=13f, lightGrowthPerSoul=0.45f, enemySpeedMultiplier=1.68f, victorySouls=46, rewardResourceId="cristal-frio", rewardResourceName="Cristal Frio", rewardPerMission=2, mapX=.42f, mapY=.30f },
            new CampaignPlanet { id="asterim", displayName="Asterim", missionsToPurify=5, missionTargetRadius=15f, lightGrowthPerSoul=0.38f, enemySpeedMultiplier=1.7f, victorySouls=68, rewardResourceId="poeira-solar", rewardResourceName="Poeira Solar", rewardPerMission=2, mapX=.61f, mapY=.68f },
            new CampaignPlanet { id="dorsalia", displayName="Dorsália", missionsToPurify=5, missionTargetRadius=17f, lightGrowthPerSoul=0.34f, enemySpeedMultiplier=1.72f, victorySouls=92, rewardResourceId="fragmento-umbral", rewardResourceName="Fragmento Umbral", rewardPerMission=3, mapX=.72f, mapY=.43f },
            new CampaignPlanet { id="varkora", displayName="Varkora", missionsToPurify=5, missionTargetRadius=19f, lightGrowthPerSoul=0.3f, enemySpeedMultiplier=1.74f, victorySouls=122, rewardResourceId="nucleo-rubro", rewardResourceName="Núcleo Rubro", rewardPerMission=3, mapX=.83f, mapY=.69f },
            new CampaignPlanet { id="marevia", displayName="Marévia", missionsToPurify=5, missionTargetRadius=21f, lightGrowthPerSoul=0.27f, enemySpeedMultiplier=1.76f, victorySouls=156, rewardResourceId="sal-estelar", rewardResourceName="Sal Estelar", rewardPerMission=4, mapX=.18f, mapY=.38f }
        };

        public CampaignSkill[] skills =
        {
            new CampaignSkill { id=CampaignProfile.FlashSkillId, displayName="Flash de Luz", description="Desbloqueia o Flash. Depois, +5% de duração e de atordoamento por nível.", unlockCost=6, soulCost=10, requiredResourceId="sal-estelar", requiredResourceAmount=4, resourceStartsAtRank=7, maximumRank=9 },
            new CampaignSkill { id="speed", displayName="Passos Livres", description="+5% de velocidade de movimento por nível.", soulCost=15, requiredResourceId="fragmento-umbral", requiredResourceAmount=3, resourceStartsAtRank=4, maximumRank=6 },
            new CampaignSkill { id="flash-capacity", requiresFlash=true, displayName="Célula de Luz", description="+1 carga máxima do Flash por nível.", soulCost=20, requiredResourceId="nucleo-rubro", requiredResourceAmount=3, resourceStartsAtRank=5, maximumRank=6 },
            new CampaignSkill { id="flash-recharge", requiresFlash=true, displayName="Foco Restaurado", description="−10% no tempo de recarga do Flash por nível.", soulCost=25, requiredResourceId="ether", requiredResourceAmount=1, maximumRank=3 },
            new CampaignSkill { id="solar-vigor", displayName="Vigor Solar", description="+1 de vida inicial e máxima por nível.", soulCost=30, requiredResourceId="poeira-solar", requiredResourceAmount=2, maximumRank=3 },
            new CampaignSkill { id="weapon-laser-pistol", displayName="Pistola Astral", description="Desbloqueia uma nova arma para futuras missões.", soulCost=180, requiredResourceId="ether", requiredResourceAmount=4, maximumRank=1, unlockedWeaponResourcePath="CampaignGuns/laser pistol_GunData" },
            new CampaignSkill { id="weapon-auto-pistol", displayName="Pistola de Rajada", description="Desbloqueia uma arma automática para futuras missões.", soulCost=450, requiredResourceId="cristal-frio", requiredResourceAmount=3, maximumRank=1, unlockedWeaponResourcePath="CampaignGuns/automatic pistol_GunData" }
        };

        public CampaignSkill FlashSkill => Array.Find(skills, skill => skill != null && skill.id == CampaignProfile.FlashSkillId);

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
