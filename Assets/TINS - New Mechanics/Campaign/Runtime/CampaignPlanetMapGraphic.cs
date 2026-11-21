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
        private float screenPixel = 1f;

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
                Canvas root = canvas.rootCanvas;
                float relativeScale = root.transform.InverseTransformVector(rectTransform.TransformVector(Vector3.right)).magnitude;
                screenPixel = 1f / Mathf.Max(.01f, root.scaleFactor * relativeScale);
            }
            if (sun != null)
            {
                Vector2 center = CenterOf(sun);
                Ellipse(mesh, center, sun.rect.size * .5f + Vector2.one * 5f, 1, orbit);
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
            Vector2 size = rectTransform.InverseTransformVector(art.TransformVector(art.rect.size));
            return Mathf.Min(Mathf.Abs(size.x), Mathf.Abs(size.y)) * .5f;
        }

        private static Color Alpha(Color tint, float multiplier)
        {
            tint.a *= multiplier;
            return tint;
        }

        private void Ellipse(VertexHelper mesh, Vector2 center, Vector2 radius, float thickness, Color tint, float tilt = 0)
        {
            // Increase curve resolution with the on-screen size. Shared cross sections keep the ring closed.
            float screenRadius = Mathf.Max(radius.x, radius.y) / screenPixel;
            int segments = Mathf.Clamp(Mathf.CeilToInt(Mathf.PI * Mathf.Sqrt(screenRadius / .4f)), 40, 256);
            float tiltRadians = tilt * Mathf.Deg2Rad;
            float cosTilt = Mathf.Cos(tiltRadians);
            float sinTilt = Mathf.Sin(tiltRadians);
            int first = mesh.currentVertCount;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * (2f * Mathf.PI / segments);
                float x = Mathf.Cos(angle);
                float y = Mathf.Sin(angle);
                Vector2 point = Rotate(new Vector2(radius.x * x, radius.y * y), cosTilt, sinTilt);
                Vector2 normal = Rotate(new Vector2(radius.y * x, radius.x * y).normalized, cosTilt, sinTilt);
                int section = AddStrokeSection(mesh, center + point, normal, thickness, tint);
                if (i > 0) JoinStrokeSections(mesh, section - 4, section);
            }
            JoinStrokeSections(mesh, mesh.currentVertCount - 4, first);
        }

        private static Vector2 Rotate(Vector2 vector, float cos, float sin)
            => new(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);

        private static void Quad(VertexHelper mesh, Vector2 center, Vector2 size, Color tint)
        {
            Vector2 half = size * .5f;
            int index = mesh.currentVertCount;
            mesh.AddVert(center - half, tint, Vector2.zero);
            mesh.AddVert(center + new Vector2(-half.x, half.y), tint, Vector2.zero);
            mesh.AddVert(center + half, tint, Vector2.zero);
            mesh.AddVert(center + new Vector2(half.x, -half.y), tint, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2);
            mesh.AddTriangle(index, index + 2, index + 3);
        }

        private void Line(VertexHelper mesh, Vector2 start, Vector2 end, float width, Color tint)
        {
            Vector2 delta = end - start;
            float length = delta.magnitude;
            if (length <= Mathf.Epsilon) return;

            Vector2 direction = delta / length;
            Vector2 normal = new(-direction.y, direction.x);
            Vector2 endFeather = direction * Mathf.Min(screenPixel * .5f, length * .5f);
            Color clear = Alpha(tint, 0f);
            int first = AddStrokeSection(mesh, start - endFeather, normal, width, clear);
            int bodyStart = AddStrokeSection(mesh, start + endFeather, normal, width, tint);
            int bodyEnd = AddStrokeSection(mesh, end - endFeather, normal, width, tint);
            int last = AddStrokeSection(mesh, end + endFeather, normal, width, clear);
            JoinStrokeSections(mesh, first, bodyStart);
            JoinStrokeSections(mesh, bodyStart, bodyEnd);
            JoinStrokeSections(mesh, bodyEnd, last);
        }

        private int AddStrokeSection(VertexHelper mesh, Vector2 point, Vector2 normal, float width, Color tint)
        {
            // Fade across one screen pixel while leaving the stroke width in map units.
            float outer = (width + screenPixel) * .5f;
            float inner = Mathf.Max(0f, (width - screenPixel) * .5f);
            Color clear = Alpha(tint, 0f);
            Color center = Alpha(tint, Mathf.Min(1f, width / screenPixel));
            int start = mesh.currentVertCount;
            mesh.AddVert(point - normal * outer, clear, Vector2.zero);
            mesh.AddVert(point - normal * inner, center, Vector2.zero);
            mesh.AddVert(point + normal * inner, center, Vector2.zero);
            mesh.AddVert(point + normal * outer, clear, Vector2.zero);
            return start;
        }

        private static void JoinStrokeSections(VertexHelper mesh, int from, int to)
        {
            for (int i = 0; i < 3; i++)
            {
                mesh.AddTriangle(from + i, from + i + 1, to + i + 1);
                mesh.AddTriangle(from + i, to + i + 1, to + i);
            }
        }
    }
}
