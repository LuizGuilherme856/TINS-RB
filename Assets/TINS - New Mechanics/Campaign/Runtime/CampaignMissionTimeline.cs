using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TINS.Campaign
{
    public sealed class CampaignMissionTimeline : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private Image eliteMarker;
        [SerializeField] private TMP_Text status;

        private static readonly Color32 Pending = new(233, 110, 124, 255);
        private static readonly Color32 Arriving = new(255, 197, 94, 255);
        private static readonly Color32 Passed = new(130, 224, 204, 255);

        private readonly List<Image> markers = new();
        private CampaignMissionRuntime mission;
        private EnemySpawnManager spawner;
        private float startRadius;
        private float goalRadius;
        private string previousFormat;
        private int previousValue = -1;

        public void Bind(CampaignMissionRuntime runtime, CampaignPlanet planet, EnemySpawnManager spawns)
        {
            mission = runtime;
            spawner = spawns;
            startRadius = planet.initialLightRadius;
            goalRadius = planet.missionTargetRadius;

            if (spawner != null)
            {
                for (int i = 0; i < spawner.EliteMilestoneCount; i++)
                {
                    float radius = spawner.EliteMilestoneRadius(i);
                    if (radius >= goalRadius) break;
                    float position = Mathf.InverseLerp(startRadius, goalRadius, radius);
                    Image marker = Instantiate(eliteMarker, eliteMarker.transform.parent, false);
                    marker.name = $"Elite {i + 1}";
                    marker.rectTransform.anchorMin = marker.rectTransform.anchorMax = new Vector2(position, .5f);
                    marker.rectTransform.anchoredPosition = Vector2.zero;
                    marker.gameObject.SetActive(true);
                    markers.Add(marker);
                }
            }
            Draw();
        }

        private void LateUpdate()
        {
            if (mission != null) Draw();
        }

        private void Draw()
        {
            float radius = mission.EnemyPressureRadius;
            float progress = Mathf.InverseLerp(startRadius, goalRadius, radius);
            var rect = fill.rectTransform;
            if (rect.anchorMax.x != progress) rect.anchorMax = new Vector2(progress, 1f);

            float extraction = mission.ExtractionRemaining;
            string format = extraction > 0f ? "EXTRAÇÃO  {0} s"
                : mission.IsProtection ? "PROTEÇÃO  {0} s" : "PURIFICAÇÃO  {0}%";
            int value = extraction > 0f ? Mathf.CeilToInt(extraction)
                : mission.IsProtection ? Mathf.CeilToInt(mission.ProtectionRemaining)
                : Mathf.FloorToInt(progress * 100f);
            if (value != previousValue || format != previousFormat)
            {
                previousFormat = format;
                previousValue = value;
                status.SetText(format, value);
            }

            if (spawner == null) return;
            for (int i = 0; i < markers.Count; i++)
                markers[i].color = i < spawner.SpawnedEliteMilestones ? Passed
                    : radius >= spawner.EliteMilestoneRadius(i) ? Arriving : Pending;
        }
    }
}
