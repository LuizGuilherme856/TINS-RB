using System.Collections.Generic;
using GraveyardShift.Gameplay;
using TMPro;
using UnityEngine;

namespace TINS.Campaign
{
    internal sealed class CampaignResourceCaches
    {
        private const float PixelsPerUnit = 16f;
        private const float ResultDuration = .55f;
        private readonly List<SpriteRenderer> remaining = new();
        private readonly Vector2 center;
        private readonly Sprite depletedSprite;
        private readonly int resourcePerCache;
        private readonly float interactionRadius;
        private readonly float timingDuration;
        private readonly TextMeshPro prompt;
        private readonly SkillCheckBar skillCheck;
        private SpriteRenderer nearbyCache;
        private SpriteRenderer activeCache;
        private float hideResultAt;

        public int Collected { get; private set; }
        public event System.Action CollectedChanged;

        public CampaignResourceCaches(Transform parent, Vector2 center, CampaignPlanet planet, Sprite depletedSprite,
            float interactionRadius, float timingDuration)
        {
            this.center = center;
            this.depletedSprite = depletedSprite;
            resourcePerCache = Mathf.Max(1, planet.resourcePerCache);
            this.interactionRadius = Mathf.Max(.01f, interactionRadius);
            this.timingDuration = Mathf.Max(.1f, timingDuration);

            var group = new GameObject("Resource Caches");
            group.transform.SetParent(parent);
            var promptObject = new GameObject("Cache Interaction Prompt", typeof(RectTransform), typeof(TextMeshPro));
            promptObject.transform.SetParent(group.transform, false);
            prompt = promptObject.GetComponent<TextMeshPro>();
            prompt.text = "[F] ABRIR CACHE";
            prompt.fontSize = 2.3f;
            prompt.alignment = TextAlignmentOptions.Center;
            prompt.color = new Color32(248, 232, 192, 255);
            prompt.rectTransform.sizeDelta = new Vector2(4f, .65f);
            prompt.sortingOrder = 24;
            prompt.gameObject.SetActive(false);

            var timingObject = new GameObject("Cache Timing");
            timingObject.SetActive(false);
            timingObject.transform.SetParent(group.transform, false);
            skillCheck = timingObject.AddComponent<SkillCheckBar>();
            skillCheck.ConfigureManualPrompt("F   ·   ACERTE A MARCA");
            skillCheck.ResultResolved += ResolveTiming;
            timingObject.SetActive(true);
            float startAngle = Random.Range(0f, 2f * Mathf.PI);
            float sector = 2f * Mathf.PI / planet.resourceCacheCount;

            for (int i = 0; i < planet.resourceCacheCount; i++)
            {
                // Each cache lies inside the penumbra at a different stage of light growth.
                float progress = .55f * i / Mathf.Max(1, planet.resourceCacheCount - 1);
                float lightRadius = Mathf.Lerp(planet.initialLightRadius, planet.missionTargetRadius, progress);
                float outerRadius = lightRadius + planet.PenumbraWidthAt(lightRadius);
                float innerRadius = lightRadius;
                if (i > 0 && outerRadius > planet.initialPenumbraRadius + .16f)
                    innerRadius = Mathf.Max(innerRadius, planet.initialPenumbraRadius + .08f);
                float radius = Mathf.Lerp(innerRadius, outerRadius, .5f);
                float angle = startAngle + sector * (i + Random.Range(-.15f, .15f));
                Vector2 position = center + radius * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                var cache = new GameObject($"Cache {i + 1}");
                cache.transform.SetParent(group.transform);
                cache.transform.position = new Vector3(
                    Mathf.Round(position.x * PixelsPerUnit) / PixelsPerUnit,
                    Mathf.Round(position.y * PixelsPerUnit) / PixelsPerUnit,
                    0f);
                var renderer = cache.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 15;
                renderer.sprite = planet.resourceCacheSprite;
                remaining.Add(renderer);
            }
        }

        public void Update(Vector2 playerPosition, float breuRadius)
        {
            nearbyCache = null;
            float reachableRadiusSquared = breuRadius * breuRadius;
            if (activeCache != null)
            {
                if (!InRange(activeCache, playerPosition, reachableRadiusSquared))
                    Cancel();
                else if (skillCheck.CurrentMarkerPosition >= 1f)
                    skillCheck.ResolveSkillCheck();
                return;
            }

            if (hideResultAt > 0f)
            {
                if (Time.unscaledTime < hideResultAt) return;
                skillCheck.HideUntilStarted();
                hideResultAt = 0f;
            }

            float bestDistance = interactionRadius * interactionRadius;
            foreach (SpriteRenderer cache in remaining)
            {
                if (!InRange(cache, playerPosition, reachableRadiusSquared)) continue;
                float distance = ((Vector2)cache.transform.position - playerPosition).sqrMagnitude;
                if (distance > bestDistance) continue;
                bestDistance = distance;
                nearbyCache = cache;
            }
            prompt.gameObject.SetActive(nearbyCache != null);
            if (nearbyCache != null)
                prompt.transform.position = nearbyCache.transform.position + Vector3.up * .8f;
        }

        public void Interact(Vector2 playerPosition, float breuRadius)
        {
            float reachableRadiusSquared = breuRadius * breuRadius;
            if (activeCache != null)
            {
                if (InRange(activeCache, playerPosition, reachableRadiusSquared))
                    skillCheck.ResolveSkillCheck();
                else
                    Cancel();
                return;
            }
            if (nearbyCache == null || !InRange(nearbyCache, playerPosition, reachableRadiusSquared)) return;

            activeCache = nearbyCache;
            nearbyCache = null;
            prompt.gameObject.SetActive(false);
            skillCheck.BeginSkillCheckAt(Random.Range(.35f, .82f), timingDuration);
        }

        public void Cancel()
        {
            activeCache = null;
            nearbyCache = null;
            hideResultAt = 0f;
            prompt.gameObject.SetActive(false);
            skillCheck.HideUntilStarted();
        }

        private bool InRange(SpriteRenderer cache, Vector2 playerPosition, float reachableRadiusSquared)
        {
            Vector2 position = cache.transform.position;
            return (position - center).sqrMagnitude < reachableRadiusSquared &&
                (position - playerPosition).sqrMagnitude <= interactionRadius * interactionRadius;
        }

        private void ResolveTiming(SkillCheckBar.SkillCheckResult result)
        {
            if (result.hitTarget && activeCache != null && remaining.Remove(activeCache))
            {
                Collected += resourcePerCache;
                CollectedChanged?.Invoke();
                activeCache.sprite = depletedSprite;
                TowerAudioFeedbacks.Instance?.PlayCrystalPickup();
            }
            activeCache = null;
            hideResultAt = Time.unscaledTime + ResultDuration;
        }
    }
}
