using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TINS.Campaign
{
    [CreateAssetMenu(menuName = "TINS/Campaign/Flash Data", fileName = "FlashData")]
    public sealed class FlashData : ScriptableObject
    {
        [Header("Disparo")]
        [Min(0.05f), Tooltip("Tempo total do feixe, em segundos.")] public float duration = 0.5f;
        [Min(0.1f), Tooltip("Tempo de atordoamento ao atingir um inimigo, em segundos.")] public float stunDuration = 1.5f;
        [Range(5f, 180f), Tooltip("Abertura total do cone em graus.")] public float coneAngle = 36f;
        [Min(0.1f), Tooltip("Alcance máximo do cone em unidades de mundo.")] public float coneRange = 7f;
        [Min(0.02f), Tooltip("Tempo para o feixe alcançar a distância máxima.")] public float waveTravelTime = 0.14f;
        [Min(0f)] public float cameraShakeMagnitude = 1.05f;
        [Min(0f)] public float pushSpeed = 2.5f;

        [Header("Cargas e recarga")]
        [Min(1)] public int maxUses = 3;
        [Min(0.1f), Tooltip("Tempo para recuperar todas as cargas após gastar a última.")] public float rechargeDuration = 5f;
        [Range(0f, 1f), Tooltip("Posição da janela na barra de recarga. 0,8 corresponde a 80%.")] public float skillCheckTarget = 0.8f;
        [Range(0.02f, 0.5f), Tooltip("Fração da barra que conta como acerto.")] public float skillCheckWidth = 0.296f;

        [Header("Luz 2D")]
        public bool enableConeLight = true;
        [ColorUsage(false, true)] public Color lightColor = new Color(0.45f, 0.9f, 1f);
        [Min(0f)] public float lightIntensity = 0.68f;
        [Range(0f, 1f)] public float lightFalloff = 0.808f;
        [Range(0f, 1f), Tooltip("Fração do alcance útil antes da borda suave.")] public float innerRadiusFraction = 8.1f / 11.334042f;
        [Range(0f, 1f), Tooltip("Fração da abertura com intensidade máxima.")] public float innerAngleFraction = 47f / 80.45f;
        public bool volumetricLight = true;
        [Min(0f)] public float volumeIntensity = 0.035f;
        public bool castShadows = true;
        [Range(0f, 1f)] public float shadowIntensity = 0.75f;
        [Range(0f, 1f)] public float shadowSoftness = 0.3f;
        [Range(0f, 1f)] public float shadowSoftnessFalloff = 0.5f;
        public bool castVolumetricShadows = true;
        [Range(0f, 1f)] public float volumetricShadowIntensity = 0.9f;

        [Header("Purificação temporária do chão")]
        [Min(0.05f), Tooltip("Tempo até a cor original voltar, contado após a luz sair do trecho.")] public float trailLifetime = 1.1f;

        [Header("Bônus das habilidades")]
        [Min(0f), Tooltip("Flash de Luz. Digite 5 para ganhar 5% por nível com bônus.")] public float durationPercentPerRank = 5f;
        [Min(0f), Tooltip("Área do Flash. Graus de abertura acrescentados por nível.")] public float anglePerRank = 8f;
        [Min(0f), Tooltip("Área do Flash. Digite 12 para ganhar 12% de alcance por nível.")] public float rangePercentPerRank = 12f;
        [Min(0), Tooltip("Célula de Luz. Cargas adicionais por nível.")] public int usesPerRank = 1;
        [Range(0f, 90f), Tooltip("Foco Restaurado. Digite 10 para reduzir a recarga base em 10% por nível.")] public float rechargeReductionPercentPerRank = 10f;
        [Min(0f), Tooltip("Onda de Repulsão. Velocidade de empurrão para longe da torre, quando maior que o valor base.")] public float repulsionPushSpeed = 13f;
        [Min(0), Tooltip("Recuperação por Abate. Cargas devolvidas por elite ou chefe derrotado.")] public int eliteRecoveryUses = 1;
        [Min(0f), Tooltip("Torreta de Lâminas. Intervalo entre ativações, em segundos.")] public float turretCooldown = 22f;

        [Header("Prefabs do efeito")]
        public GameObject beamPrefab;
        public GameObject coneLightPrefab;
        [Range(3, 64)] public int hitboxArcSegments = 24;

        private static FlashData shared;
        public static FlashData Shared => shared != null ? shared : shared = Resources.Load<FlashData>(nameof(FlashData));

        internal Vector2 EffectPerRank(CampaignSkillEffectType type, Vector2 original)
        {
            return type switch
            {
                CampaignSkillEffectType.FlashDuration => new Vector2(durationPercentPerRank / 100f, 0f),
                CampaignSkillEffectType.FlashArea => new Vector2(anglePerRank, rangePercentPerRank / 100f),
                CampaignSkillEffectType.FlashCapacity => new Vector2(usesPerRank, 0f),
                CampaignSkillEffectType.FlashRecharge => new Vector2(rechargeReductionPercentPerRank / 100f, 0f),
                CampaignSkillEffectType.FlashRepulsion => new Vector2(repulsionPushSpeed, 0f),
                CampaignSkillEffectType.EliteFlashRecovery => new Vector2(eliteRecoveryUses, 0f),
                CampaignSkillEffectType.BladeTurret => new Vector2(turretCooldown, 0f),
                _ => original
            };
        }

        internal void ConfigureLight(Light2D light, float range, float angle)
        {
            light.color = lightColor;
            light.intensity = lightIntensity;
            light.falloffIntensity = lightFalloff;
            light.pointLightOuterRadius = range;
            light.pointLightInnerRadius = range * innerRadiusFraction;
            light.pointLightOuterAngle = angle;
            light.pointLightInnerAngle = angle * innerAngleFraction;
            light.volumetricEnabled = volumetricLight;
            light.volumeIntensity = volumeIntensity;
            light.shadowsEnabled = castShadows;
            light.shadowIntensity = shadowIntensity;
            light.shadowSoftness = shadowSoftness;
            light.shadowSoftnessFalloffIntensity = shadowSoftnessFalloff;
            light.volumetricShadowsEnabled = castVolumetricShadows;
            light.shadowVolumeIntensity = volumetricShadowIntensity;
        }
    }
}
