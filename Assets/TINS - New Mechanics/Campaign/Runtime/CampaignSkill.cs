using System;
using UnityEngine;

namespace TINS.Campaign
{
    public enum CampaignSkillEffectType
    {
        None,
        FlashDuration,
        FlashArea,
        FlashCapacity,
        FlashRecharge,
        MovementSpeed,
        PlayerHealth,
        WeaponUnlock,
        LastShot,
        ExplosiveReload,
        FlashRepulsion,
        BladeTurret,
        GuardianBlades,
        CrystalMagnet,
        DoubleTap,
        ScatterShot,
        EliteFlashRecovery,
        ExtraDash,
        MonkeyDecoy,
        GunCritical
    }

    [Serializable]
    public sealed class CampaignSkillPrerequisite
    {
        public CampaignSkill skill;
        [Min(1)] public int minimumRank = 1;
    }

    [CreateAssetMenu(menuName = "TINS/Campaign/Skill", fileName = "New Campaign Skill")]
    public sealed class CampaignSkill : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        [Min(1)] public int maximumRank = 1;

        [Header("Progression and cost")]
        [Min(0)] public int soulCost = 10;
        [Min(0)] public int unlockCost;
        [Min(1f)] public float soulCostGrowth = 1.75f;
        [Tooltip("Optional exact soul prices by purchased rank, after any unlock rank. Empty entries use the growth multiplier.")]
        public int[] soulCostsByRank = Array.Empty<int>();
        public bool requiresFlash;
        public string requiredResourceId;
        [Min(0)] public int requiredResourceAmount;
        [Min(1), Tooltip("First purchased level that requires this material.")] public int resourceStartsAtRank = 1;
        [Min(1f)] public float resourceCostGrowth = 1.4f;
        [Tooltip("Optional exact material amounts by paid rank. Empty entries use the growth multiplier.")]
        public int[] resourceCostsByRank = Array.Empty<int>();
        public string unlockedWeaponResourcePath;

        [Header("Effect per level")]
        public CampaignSkillEffectType effectType;
        [Tooltip("Amount applied per level. Edit Flash skill values in the shared FlashData asset.")]
        public float effectPerRank;
        [Tooltip("Optional second value per level. Edit Flash range bonuses in FlashData.")]
        public float secondaryEffectPerRank;
        [Tooltip("Optional prefab deployed by this upgrade during a campaign mission.")]
        public GameObject effectPrefab;
        [Tooltip("Orbital weapon configuration used by Guardian Blades.")]
        public OrbitalUpgrade_ActiveEffect orbitalEffect;
        [Min(1)] public int effectStartsAtRank = 1;

        [Header("Progressive tree")]
        public bool visibleAtStart;
        public bool requireAllRevealPrerequisites = true;
        public bool requireAllPrerequisites = true;
        [Tooltip("Buying one of these skills reveals this node to the player.")]
        public CampaignSkillPrerequisite[] revealPrerequisites = Array.Empty<CampaignSkillPrerequisite>();
        [Tooltip("These skills must reach their minimum ranks before this node can be purchased.")]
        public CampaignSkillPrerequisite[] prerequisites = Array.Empty<CampaignSkillPrerequisite>();
        [Tooltip("Buying any of these skills blocks this specialization until the tree is reset.")]
        public CampaignSkill[] exclusiveWith = Array.Empty<CampaignSkill>();
        public Vector2 editorPosition;

        public Vector2 EffectPerRank
        {
            get
            {
                var original = new Vector2(effectPerRank, secondaryEffectPerRank);
                return FlashData.Shared?.EffectPerRank(effectType, original) ?? original;
            }
        }

        public string Description
        {
            get
            {
                Vector2 bonus = EffectPerRank;
                return effectType switch
                {
                    CampaignSkillEffectType.FlashDuration =>
                        $"Desbloqueia o Flash. Depois, +{bonus.x * 100f:0.#}% de duração e atordoamento por nível.",
                    CampaignSkillEffectType.FlashArea =>
                        $"+{bonus.x:0.#}° de abertura e +{bonus.y * 100f:0.#}% do alcance base por nível.",
                    CampaignSkillEffectType.FlashCapacity =>
                        $"+{bonus.x:0.#} carga(s) do Flash por nível.",
                    CampaignSkillEffectType.FlashRecharge =>
                        $"−{bonus.x * 100f:0.#}% do tempo base de recarga por nível.",
                    CampaignSkillEffectType.EliteFlashRecovery =>
                        $"Derrotar um elite ou chefe devolve {bonus.x:0.#} carga(s) de Flash, até o limite.",
                    CampaignSkillEffectType.BladeTurret =>
                        $"{description} Intervalo de {bonus.x:0.#} s.",
                    _ => description
                };
            }
        }

        public Vector2 EffectAtRank(int rank)
        {
            int activeRanks = Mathf.Max(0, Mathf.Min(rank, maximumRank) - Mathf.Max(1, effectStartsAtRank) + 1);
            return EffectPerRank * activeRanks;
        }

        public int CostForRank(int currentRank)
        {
            if (unlockCost > 0 && currentRank == 0) return unlockCost;
            int upgradeRank = currentRank - (unlockCost > 0 ? 1 : 0);
            upgradeRank = Mathf.Max(0, upgradeRank);
            if (soulCostsByRank != null && upgradeRank < soulCostsByRank.Length)
                return Mathf.Max(0, soulCostsByRank[upgradeRank]);
            return Mathf.CeilToInt(Mathf.Max(0, soulCost) * Mathf.Pow(Mathf.Max(1f, soulCostGrowth), upgradeRank));
        }

        public int ResourceCostForRank(int currentRank)
        {
            int paidMaterialRank = currentRank + 1 - Mathf.Max(1, resourceStartsAtRank);
            if (paidMaterialRank < 0) return 0;
            if (resourceCostsByRank != null && paidMaterialRank < resourceCostsByRank.Length)
                return Mathf.Max(0, resourceCostsByRank[paidMaterialRank]);
            return Mathf.CeilToInt(Mathf.Max(0, requiredResourceAmount) * Mathf.Pow(Mathf.Max(1f, resourceCostGrowth), paidMaterialRank));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            maximumRank = Mathf.Max(1, maximumRank);
            soulCost = Mathf.Max(0, soulCost);
            unlockCost = Mathf.Max(0, unlockCost);
            soulCostGrowth = Mathf.Max(1f, soulCostGrowth);
            resourceCostGrowth = Mathf.Max(1f, resourceCostGrowth);
            resourceStartsAtRank = Mathf.Max(1, resourceStartsAtRank);
            if (string.IsNullOrWhiteSpace(id)) Debug.LogError($"Campaign skill asset '{name}' requires a permanent ID.", this);
        }
#endif
    }
}
