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

        private CampaignPlanetBoardRaycaster raycaster;
        private Canvas canvas;
        private CampaignPlanetMarker[] markers;
        private Vector2 pan;
        private float zoom = 1f;
        private int? dragPointer;
        private Vector2 lastDragPoint;

        private RectTransform Viewport => (RectTransform)transform;

        public void ZoomIn() => ChangeZoom(1.2f, Vector2.zero);

        public void ZoomOut() => ChangeZoom(1f / 1.2f, Vector2.zero);

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
            if (ticks == 0f || !TryGetLocalPoint(data.position, out Vector2 point)) return;
            ChangeZoom(Mathf.Pow(1.2f, ticks), point);
            data.Use();
        }

        public void OnBeginDrag(PointerEventData data)
        {
            if (dragPointer.HasValue || data.button == PointerEventData.InputButton.Right ||
                !TryGetLocalPoint(data.pressPosition, out lastDragPoint)) return;
            dragPointer = data.pointerId;
            pan = chart.anchoredPosition;
        }

        public void OnDrag(PointerEventData data)
        {
            if (dragPointer != data.pointerId || !TryGetLocalPoint(data.position, out var point)) return;
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

        private bool TryGetLocalPoint(Vector2 screenPoint, out Vector2 local)
        {
            local = default;
            if (raycaster == null) raycaster = GetComponentInParent<CampaignPlanetBoardRaycaster>();
            return raycaster != null && raycaster.TryGetBufferPoint(screenPoint, out var pixel) &&
                   RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport, pixel, raycaster.eventCamera, out local);
        }

        private void ApplyView()
        {
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            Vector2 limit = Vector2.Max(Vector2.zero, (chart.rect.size * zoom - Viewport.rect.size) * .5f);
            pan = Vector2.Max(-limit, Vector2.Min(limit, pan));
            if (!Mathf.Approximately(chart.localScale.x, zoom))
            {
                chart.localScale = new Vector3(zoom, zoom, 1f);
                markers ??= chart.GetComponentsInChildren<CampaignPlanetMarker>(true);
                foreach (var marker in markers) marker.UpdateMapLabels(zoom);
                if (sunCaption != null) sunCaption.enabled = zoom >= .7f;
            }
            // Snap the pan to the off-screen Canvas grid without rounding away small pointer movements.
            chart.anchoredPosition = new Vector2(Mathf.Round(pan.x * scale), Mathf.Round(pan.y * scale)) / scale;
        }
    }
}
