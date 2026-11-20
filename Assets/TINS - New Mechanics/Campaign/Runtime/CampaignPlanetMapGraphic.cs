using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TINS.Campaign
{
    internal enum CampaignDestinationState { Locked, Available, Purified }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CampaignPlanetMapGraphic : MaskableGraphic
    {
        [SerializeField] private RectTransform sun;
        [SerializeField] private CampaignPlanetMarker[] markers;
        [SerializeField] private Color blocked = new Color32(130, 139, 170, 255);
        [SerializeField] private Color available = new Color32(242, 192, 120, 255);
        [SerializeField] private Color purified = new Color32(119, 221, 191, 255);
        [SerializeField] private Color orbit = new Color32(55, 73, 105, 125);

        private IReadOnlyList<CampaignPlanetMarker> orderedMarkers;
        private float animationTime;
        private float pixelSize = 2f;

        internal Color Tint(CampaignDestinationState state) => state switch
        {
            CampaignDestinationState.Purified => purified,
            CampaignDestinationState.Available => available,
            _ => blocked
        };

        internal void Bind(IReadOnlyList<CampaignPlanetMarker> campaignOrder)
        {
            orderedMarkers = campaignOrder;
            SetVerticesDirty();
        }

        private void LateUpdate()
        {
            float time = Mathf.Floor(Time.unscaledTime * 24f) / 24f;
            if (time == animationTime) return;
            animationTime = time;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (canvas != null)
            {
                float relativeScale = canvas.transform.InverseTransformVector(rectTransform.TransformVector(Vector3.right)).magnitude;
                pixelSize = 1f / Mathf.Max(.01f, canvas.scaleFactor * relativeScale);
            }
            if (sun != null)
            {
                Vector2 center = CenterOf(sun);
                Ellipse(mesh, center, new Vector2(91, 82), 1, orbit);
                Ellipse(mesh, center, new Vector2(235, 146), 1, Alpha(orbit, .58f), 10f);
                Ellipse(mesh, center, new Vector2(493, 282), 1, Alpha(orbit, .53f), -5f);
            }

            var destinations = orderedMarkers ?? markers;
            if (destinations == null) return;
            bool pointerOnMap = false;
            foreach (var marker in destinations)
                pointerOnMap |= marker != null && marker.IsHovered;

            CampaignPlanetMarker previous = null;
            foreach (var marker in destinations)
            {
                if (marker == null)
                {
                    previous = null;
                    continue;
                }

                if (previous != null) DrawRoute(mesh, previous, marker);
                previous = marker;
            }

            foreach (var marker in destinations)
            {
                if (marker == null) continue;
                DrawMarker(mesh, marker, marker.IsHovered || (!pointerOnMap && marker.IsFocused));
            }
        }

        private void DrawRoute(VertexHelper mesh, CampaignPlanetMarker from, CampaignPlanetMarker to)
        {
            var start = CenterOf(from.PlanetArt.rectTransform);
            var end = CenterOf(to.PlanetArt.rectTransform);
            var direction = (end - start).normalized;
            start += direction * (Radius(from) + 9f);
            end -= direction * (Radius(to) + 9f);
            float length = Vector2.Dot(end - start, direction);
            if (length <= 0) return;

            Color tint = Tint(to.State);
            if (to.State == CampaignDestinationState.Locked)
            {
                for (float distance = 0; distance < length; distance += 9)
                    Line(mesh, start + direction * distance, start + direction * Mathf.Min(distance + 4, length),
                        1.5f, Alpha(tint, to.IsHovered || to.IsFocused ? .85f : .46f));
                return;
            }

            Line(mesh, start, end, 7, Alpha(tint, .12f));
            Line(mesh, start, end, 2, Alpha(tint, .74f));
            if (to.State != CampaignDestinationState.Available) return;

            float travel = Mathf.Repeat(animationTime * .45f, 1);
            Vector2 trail = Vector2.Lerp(start, end, Mathf.Max(0, travel - .15f));
            Vector2 signal = Vector2.Lerp(start, end, travel);
            Line(mesh, trail, signal, 4, tint);
            Quad(mesh, signal, new Vector2(5, 5), Color.white);
        }

        private void DrawMarker(VertexHelper mesh, CampaignPlanetMarker marker, bool highlighted)
        {
            var center = CenterOf(marker.PlanetArt.rectTransform);
            float radius = Radius(marker);
            Color tint = Tint(marker.State);
            bool availableNow = marker.State == CampaignDestinationState.Available;
            float pulse = .6f + .4f * Mathf.Sin(animationTime * 3.5f);

            Ellipse(mesh, center, Vector2.one * (radius + 5), highlighted ? 2 : 1,
                Alpha(tint, highlighted ? 1f : availableNow ? .6f + pulse * .32f : .43f));
            if (highlighted || availableNow)
            {
                float reach = radius + 11;
                Ellipse(mesh, center, Vector2.one * reach, 1,
                    Alpha(tint, highlighted ? .35f : .1f + pulse * .16f));
                Quad(mesh, center + Vector2.up * reach, new Vector2(9, 2), tint);
                Quad(mesh, center + Vector2.left * reach, new Vector2(2, 9), tint);
                Quad(mesh, center + Vector2.right * reach, new Vector2(2, 9), tint);
            }

            Vector2 meter = CenterOf((RectTransform)marker.transform) + Vector2.down * (radius + 26);
            if (marker.Goal <= 0) return;
            int segments = Mathf.Min(marker.Goal, 10);
            int filled = Mathf.FloorToInt((float)marker.Completed / marker.Goal * segments);
            float pitch = Mathf.Min(11f, 66f / segments);
            float left = meter.x - pitch * (segments - 1) * .5f;
            for (int i = 0; i < segments; i++)
            {
                Color pip = i < filled ? tint : Alpha(tint, .23f);
                Quad(mesh, new Vector2(left + pitch * i, meter.y), new Vector2(pitch * .72f, 3), pip);
            }
        }

        private Vector2 CenterOf(RectTransform target)
            => rectTransform.InverseTransformPoint(target.TransformPoint(target.rect.center));

        private float Radius(CampaignPlanetMarker marker)
        {
            var art = marker.PlanetArt.rectTransform;
            Vector2 size = rectTransform.InverseTransformVector(art.parent.TransformVector(art.rect.size));
            return Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.y)) * .5f;
        }

        private static Color Alpha(Color tint, float multiplier)
        {
            tint.a *= multiplier;
            return tint;
        }

        private void Ellipse(VertexHelper mesh, Vector2 center, Vector2 radius, float thickness, Color tint, float tilt = 0)
        {
            const int segments = 64;
            Quaternion rotation = Quaternion.Euler(0, 0, tilt);
            Vector2 last = center + (Vector2)(rotation * new Vector3(radius.x, 0));
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * (2f * Mathf.PI / segments);
                Vector2 next = center + (Vector2)(rotation * new Vector3(radius.x * Mathf.Cos(angle), radius.y * Mathf.Sin(angle)));
                Line(mesh, last, next, thickness, tint);
                last = next;
            }
        }

        private void Quad(VertexHelper mesh, Vector2 center, Vector2 size, Color tint)
        {
            Vector2 horizontal = Vector2.right * (Mathf.Max(pixelSize, size.x) * .5f);
            Vector2 vertical = Vector2.up * (Mathf.Max(pixelSize, size.y) * .5f);
            Strip(mesh, center - horizontal, center + horizontal, vertical, tint);
        }

        private void Line(VertexHelper mesh, Vector2 start, Vector2 end, float width, Color tint)
        {
            Vector2 direction = (end - start).normalized;
            float thickness = Mathf.Max(pixelSize, width);
            Strip(mesh, start, end, new Vector2(-direction.y, direction.x) * (thickness * .5f), tint);
        }

        private static void Strip(VertexHelper mesh, Vector2 start, Vector2 end, Vector2 normal, Color tint)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(start - normal, tint, Vector2.zero);
            mesh.AddVert(start + normal, tint, Vector2.zero);
            mesh.AddVert(end + normal, tint, Vector2.zero);
            mesh.AddVert(end - normal, tint, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
