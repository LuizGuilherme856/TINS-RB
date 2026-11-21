using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TINS.Campaign
{
    public sealed class CampaignPlanetMapViewport : MonoBehaviour, IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform chart;
        [SerializeField] private TMP_Text sunCaption;
        [SerializeField, Range(.25f, 1f)] private float minimumZoom = .54f;
        [SerializeField, Min(1f)] private float maximumZoom = 1.65f;

        private CampaignPlanetMarker[] markers;
        private Vector2 pan;
        private float zoom = 1f;
        private int? dragPointer;
        private Vector2 lastDragPoint;

        private RectTransform Viewport => (RectTransform)transform;

        public void ZoomIn() => ChangeZoom(1.2f, Vector2.zero);

        public void ZoomOut() => ChangeZoom(1f / 1.2f, Vector2.zero);

        internal void FitOverview()
        {
            markers ??= chart.GetComponentsInChildren<CampaignPlanetMarker>(true);
            if (markers.Length == 0)
            {
                Focus(null);
                return;
            }

            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(chart, markers[0].transform);
            foreach (var marker in markers)
                bounds.Encapsulate(RectTransformUtility.CalculateRelativeRectTransformBounds(chart, marker.transform));

            Vector2 available = Viewport.rect.size - Vector2.one * 32f;
            zoom = Mathf.Clamp(Mathf.Min(available.x / bounds.size.x, available.y / bounds.size.y), minimumZoom, 1f);
            pan = -(Vector2)bounds.center * zoom;
            ApplyView();
        }

        internal void Focus(RectTransform destination)
        {
            zoom = 1f;
            pan = Vector2.zero;
            if (destination != null)
            {
                Vector2 position = chart.InverseTransformPoint(destination.TransformPoint(destination.rect.center));
                pan = -position;
            }
            ApplyView();
        }

        internal void KeepVisible(RectTransform destination)
        {
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(Viewport, destination);
            Rect safeArea = Viewport.rect;
            safeArea.min += Vector2.one * 16f;
            safeArea.max -= Vector2.one * 16f;
            Vector2 offset = Vector2.Max(Vector2.zero, safeArea.min - (Vector2)bounds.min) +
                             Vector2.Min(Vector2.zero, safeArea.max - (Vector2)bounds.max);
            if (offset.sqrMagnitude <= .01f) return;
            pan = chart.anchoredPosition + offset;
            ApplyView();
        }

        public void OnScroll(PointerEventData data)
        {
            float ticks = Mathf.Clamp(data.scrollDelta.y, -3f, 3f);
            if (ticks == 0f || !TryGetLocalPoint(data.position, data.enterEventCamera, out Vector2 point)) return;
            ChangeZoom(Mathf.Pow(1.2f, ticks), point);
            data.Use();
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (dragPointer.HasValue || data.button == PointerEventData.InputButton.Right ||
                !TryGetLocalPoint(data.pressPosition, data.pressEventCamera, out lastDragPoint)) return;
            dragPointer = data.pointerId;
            pan = chart.anchoredPosition;
        }

        public void OnDrag(PointerEventData data)
        {
            if (dragPointer != data.pointerId || !TryGetLocalPoint(data.position, data.pressEventCamera, out var point)) return;
            pan += point - lastDragPoint;
            lastDragPoint = point;
            ApplyView();
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (dragPointer == data.pointerId) dragPointer = null;
        }

        private void OnDisable() => dragPointer = null;

        private void ChangeZoom(float multiplier, Vector2 pivot)
        {
            float next = Mathf.Clamp(zoom * multiplier, minimumZoom, maximumZoom);
            if (Mathf.Approximately(next, zoom)) return;
            pan = pivot + (chart.anchoredPosition - pivot) * (next / zoom);
            zoom = next;
            ApplyView();
        }

        private bool TryGetLocalPoint(Vector2 screenPoint, Camera eventCamera, out Vector2 local)
            => RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport, screenPoint, eventCamera, out local);

        private void ApplyView()
        {
            Vector2 limit = Vector2.Max(Vector2.zero, (chart.rect.size * zoom - Viewport.rect.size) * .5f);
            pan = Vector2.Max(-limit, Vector2.Min(limit, pan));
            if (!Mathf.Approximately(chart.localScale.x, zoom))
            {
                chart.localScale = new Vector3(zoom, zoom, 1f);
                markers ??= chart.GetComponentsInChildren<CampaignPlanetMarker>(true);
                foreach (var marker in markers) marker.UpdateMapLabels(zoom);
                if (sunCaption != null) sunCaption.enabled = zoom >= .7f;
            }
            chart.anchoredPosition = pan;
        }
    }
}
