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
        [SerializeField] private Image regularMarker;
        [SerializeField] private RectTransform cursor;
        [SerializeField] private TMP_Text status;
        [SerializeField, Min(0.01f)] private float fillTweenDuration = 0.36f;

        private static readonly Color32 PreviewColor = new(0x30, 0x1E, 0x46, 0xFF);
        private readonly List<Image> markers = new();
        private Image preview;
        private CampaignMissionRuntime mission;
        private EnemySpawnManager spawner;
        private float startRadius;
        private float goalRadius;
        private float displayedProgress;
        private float fillTweenStart;
        private float fillTweenElapsed;
        private string previousFormat;
        private int previousValue = -1;

        public void Bind(CampaignMissionRuntime runtime, CampaignPlanet planet, EnemySpawnManager spawns)
        {
            mission = runtime;
            spawner = spawns;
            startRadius = planet.initialLightRadius;
            goalRadius = planet.missionTargetRadius;
            displayedProgress = ProgressAt(mission.EnemyPressureRadius);
            preview = CreateSpan("Carried Souls Preview", PreviewColor);
            preview.rectTransform.SetSiblingIndex(fill.rectTransform.GetSiblingIndex() + 1);

            if (spawner != null)
            {
                for (int i = 0; i < spawner.WaveMilestoneCount; i++)
                {
                    float position = ProgressAt(spawner.WaveMilestoneRadius(i));
                    Image template = spawner.IsEliteWave(i) ? eliteMarker : regularMarker;
                    Image marker = Instantiate(template, template.transform.parent, false);
                    marker.name = $"Onda {i + 1}";
                    marker.rectTransform.anchorMin = marker.rectTransform.anchorMax = new Vector2(position, .5f);
                    marker.rectTransform.anchoredPosition = Vector2.zero;
                    marker.gameObject.SetActive(true);
                    markers.Add(marker);
                }
            }
            cursor.SetAsLastSibling();
            Draw();
        }

        private void LateUpdate()
        {
            if (mission != null) Draw();
        }

        private void Draw()
        {
            displayedProgress = TweenProgress(ProgressAt(mission.EnemyPressureRadius));
            float progress = displayedProgress;
            float projected = ProgressAt(mission.ProjectedPurificationRadius);
            preview.enabled = projected > progress;
            if (preview.enabled)
                SetSpan(preview.rectTransform, progress, projected);
            var rect = fill.rectTransform;
            if (rect.anchorMax.x != progress)
                rect.anchorMax = new Vector2(progress, rect.anchorMax.y);
            if (cursor.anchorMin.x != progress)
            {
                cursor.anchorMin = new Vector2(progress, cursor.anchorMin.y);
                cursor.anchorMax = new Vector2(progress, cursor.anchorMax.y);
            }

            float extraction = mission.ExtractionRemaining;
            string format = "PURIFICAÇÃO  {0}%";
            int value = Mathf.FloorToInt(progress * 100f);
            if (extraction > 0f)
            {
                format = "EXTRAÇÃO  {0} s";
                value = Mathf.CeilToInt(extraction);
            }
            else if (spawner != null && !string.IsNullOrEmpty(spawner.ActiveBossName))
            {
                format = $"CHEFE  ·  {spawner.ActiveBossName.ToUpperInvariant()}";
            }
            else if (spawner != null && spawner.WaveDirection != Vector2Int.zero)
            {
                Vector2Int direction = spawner.WaveDirection;
                string side = direction.y > 0 ? "NORTE" : direction.y < 0 ? "SUL" : direction.x > 0 ? "LESTE" : "OESTE";
                value = Mathf.CeilToInt(spawner.WaveWarningRemaining - 0.001f);
                string wave = spawner.IsBossWaveAnnounced ? "ONDA FINAL" : "ONDA";
                format = value > 0 ? $"{wave}  ·  {side}  {{0}} s" : $"{wave}  ·  {side}";
            }
            if (value != previousValue || format != previousFormat)
            {
                previousFormat = format;
                previousValue = value;
                status.SetText(format, value);
            }

            if (spawner == null) return;
            for (int i = 0; i < markers.Count; i++)
            {
                bool pending = spawner.IsWaveMilestonePending(i) && !mission.IsResolved;
                GameObject marker = markers[i].gameObject;
                if (marker.activeSelf != pending)
                    marker.SetActive(pending);
            }
        }

        private float TweenProgress(float target)
        {
            float duration = Mathf.Max(0.01f, fillTweenDuration);
            if (target <= displayedProgress)
            {
                fillTweenElapsed = duration;
                return target;
            }

            if (fillTweenElapsed >= duration)
            {
                fillTweenStart = displayedProgress;
                fillTweenElapsed = 0f;
            }

            // Keep the tween moving as successive souls update the target.
            fillTweenElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(fillTweenElapsed / duration);
            return Mathf.Lerp(fillTweenStart, target, t * t * t);
        }

        private float ProgressAt(float radius) => Mathf.InverseLerp(startRadius, goalRadius, radius);

        private Image CreateSpan(string objectName, Color color)
        {
            var span = new GameObject(objectName, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            RectTransform rect = span.rectTransform;
            RectTransform track = fill.rectTransform;
            rect.SetParent(track.parent, false);
            rect.anchorMin = track.anchorMin;
            rect.anchorMax = track.anchorMax;
            rect.anchoredPosition = track.anchoredPosition;
            rect.sizeDelta = track.sizeDelta;
            span.color = color;
            span.raycastTarget = false;
            return span;
        }

        private static void SetSpan(RectTransform rect, float start, float end)
        {
            rect.anchorMin = new Vector2(start, rect.anchorMin.y);
            rect.anchorMax = new Vector2(end, rect.anchorMax.y);
        }
    }
}
