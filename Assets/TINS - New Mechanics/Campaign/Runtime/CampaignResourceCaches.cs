using System.Collections.Generic;
using UnityEngine;

namespace TINS.Campaign
{
    internal sealed class CampaignResourceCaches
    {
        private const float PickupRadius = .9f;
        private const float PixelsPerUnit = 16f;
        private readonly List<SpriteRenderer> remaining = new();
        private readonly Vector2 center;
        private readonly Sprite depletedSprite;
        private readonly int resourcePerCache;

        public int Collected { get; private set; }

        public CampaignResourceCaches(Transform parent, Vector2 center, CampaignPlanet planet, Sprite depletedSprite)
        {
            this.center = center;
            this.depletedSprite = depletedSprite;
            resourcePerCache = Mathf.Max(1, planet.resourcePerCache);

            var group = new GameObject("Resource Caches");
            group.transform.SetParent(parent);
            float startAngle = Random.Range(0f, 2f * Mathf.PI);
            float sector = 2f * Mathf.PI / planet.resourceCacheCount;

            for (int i = 0; i < planet.resourceCacheCount; i++)
            {
                // Each cache lies inside the penumbra at a different stage of light growth.
                float progress = .55f * i / Mathf.Max(1, planet.resourceCacheCount - 1);
                float lightRadius = Mathf.Lerp(planet.initialLightRadius, planet.missionTargetRadius, progress);
                float radius = lightRadius + .5f * planet.PenumbraWidthAt(lightRadius);
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

        public void CollectNearby(Vector2 playerPosition, float breuRadius)
        {
            float reachableRadiusSquared = breuRadius * breuRadius;
            for (int i = remaining.Count - 1; i >= 0; i--)
            {
                SpriteRenderer cache = remaining[i];
                Vector2 position = cache.transform.position;
                if ((position - center).sqrMagnitude >= reachableRadiusSquared ||
                    (position - playerPosition).sqrMagnitude > PickupRadius * PickupRadius)
                    continue;

                remaining.RemoveAt(i);
                Collected += resourcePerCache;
                cache.sprite = depletedSprite;
                TowerAudioFeedbacks.Instance?.PlayCrystalPickup();
            }
        }
    }
}
