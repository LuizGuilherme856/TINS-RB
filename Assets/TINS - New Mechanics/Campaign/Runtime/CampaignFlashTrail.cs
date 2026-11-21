using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TINS.Campaign
{
    /// <summary>Stores when the flash last reached each patch of ground in world space.</summary>
    internal sealed class CampaignFlashTrail : IDisposable
    {
        private const int SamplesPerUnit = 8;
        public static readonly int BoundsId = Shader.PropertyToID("_FlashTrailBounds");
        private static readonly int TextureId = Shader.PropertyToID("_FlashTrail");

        private readonly Texture2D stamps;
        private readonly Vector2 minimum;
        private readonly float worldSize;
        private readonly float lifetime;
        private readonly float cellSize;
        private readonly List<(ShadowCaster2D caster, CircleCollider2D baseCollider)> shadowCasters = new();
        private readonly List<ShadowFootprint> nearbyShadows = new();
        private float lastStamp;
        private bool dirty;

        // Start at 1 so zero can mean that the ground has never been lit.
        private float Clock => 1f + Time.timeSinceLevelLoad / lifetime;

        public CampaignFlashTrail(Vector2 center, float radius, float lifetime)
        {
            this.lifetime = lifetime;
            worldSize = 2f * radius;
            minimum = center - Vector2.one * radius;
            int size = Mathf.Min(1024, Mathf.CeilToInt(worldSize * SamplesPerUnit));
            cellSize = worldSize / size;
            stamps = new Texture2D(size, size, TextureFormat.RFloat, false, true)
            {
                name = "Flash terrain trail",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = stamps.GetRawTextureData<float>();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = 0f;

            // Velária's tree and prop casters are placed on the circular base of the sprite.
            foreach (ShadowCaster2D caster in UnityEngine.Object.FindObjectsByType<ShadowCaster2D>(FindObjectsSortMode.None))
            {
                if (caster.TryGetComponent(out CircleCollider2D baseCollider))
                    shadowCasters.Add((caster, baseCollider));
            }
        }

        public void Stamp(Vector2 origin, Vector2 direction, float radius, float halfAngle)
        {
            nearbyShadows.Clear();
            foreach (var (caster, baseCollider) in shadowCasters)
            {
                if (caster == null || baseCollider == null || !caster.isActiveAndEnabled || !caster.castsShadows)
                    continue;

                Transform footprint = baseCollider.transform;
                Vector2 center = footprint.TransformPoint(baseCollider.offset);
                Vector3 scale = footprint.lossyScale;
                float footprintRadius = baseCollider.radius * 0.96f;
                float maxDistance = radius + footprintRadius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
                if ((center - origin).sqrMagnitude <= maxDistance * maxDistance)
                    nearbyShadows.Add(new ShadowFootprint(footprint, baseCollider.offset, footprintRadius, origin));
            }

            int size = stamps.width;
            Vector2 bottomLeft = (origin - Vector2.one * radius - minimum) / cellSize;
            Vector2 topRight = (origin + Vector2.one * radius - minimum) / cellSize;
            int minX = Mathf.Max(0, Mathf.FloorToInt(bottomLeft.x));
            int minY = Mathf.Max(0, Mathf.FloorToInt(bottomLeft.y));
            int maxX = Mathf.Min(size - 1, Mathf.FloorToInt(topRight.x));
            int maxY = Mathf.Min(size - 1, Mathf.FloorToInt(topRight.y));
            float radiusSquared = radius * radius;
            float minDot = Mathf.Cos(halfAngle * Mathf.Deg2Rad);
            float minDotSquared = minDot * minDot;
            var pixels = stamps.GetRawTextureData<float>();
            lastStamp = Clock;
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 point = minimum + new Vector2(x + 0.5f, y + 0.5f) * cellSize;
                Vector2 offset = point - origin;
                float distanceSquared = offset.sqrMagnitude;
                float forward = Vector2.Dot(offset, direction);
                if (distanceSquared > radiusSquared || forward < 0f || forward * forward < distanceSquared * minDotSquared)
                    continue;

                bool shadowed = false;
                foreach (ShadowFootprint shadow in nearbyShadows)
                {
                    if (!shadow.Blocks(point)) continue;
                    shadowed = true;
                    break;
                }
                if (shadowed) continue;

                pixels[y * size + x] = lastStamp;
                dirty = true;
            }
        }

        public void Apply(MaterialPropertyBlock properties)
        {
            if (dirty)
            {
                stamps.Apply(false);
                dirty = false;
            }

            float now = Clock;
            properties.SetTexture(TextureId, stamps);
            properties.SetVector(BoundsId, new Vector4(minimum.x, minimum.y, 1f / worldSize,
                lastStamp > 0f && now - lastStamp < 1f ? now : 0f));
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(stamps);
        }

        private readonly struct ShadowFootprint
        {
            private readonly Matrix4x4 worldToLocal;
            private readonly Vector2 center;
            private readonly Vector2 localOrigin;
            private readonly float radiusSquared;

            public ShadowFootprint(Transform transform, Vector2 center, float radius, Vector2 origin)
            {
                worldToLocal = transform.worldToLocalMatrix;
                this.center = center;
                localOrigin = (Vector2)worldToLocal.MultiplyPoint3x4(origin) - center;
                radiusSquared = radius * radius;
            }

            public bool Blocks(Vector2 point)
            {
                if (localOrigin.sqrMagnitude <= radiusSquared) return false;

                Vector2 segment = (Vector2)worldToLocal.MultiplyPoint3x4(point) - center - localOrigin;
                float lengthSquared = segment.sqrMagnitude;
                if (lengthSquared <= Mathf.Epsilon) return false;
                float fraction = Mathf.Clamp01(-Vector2.Dot(localOrigin, segment) / lengthSquared);
                Vector2 nearest = localOrigin + segment * fraction;
                return nearest.sqrMagnitude < radiusSquared;
            }
        }
    }
}
