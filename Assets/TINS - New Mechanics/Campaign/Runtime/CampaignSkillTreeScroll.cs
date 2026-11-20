using UnityEngine;
using UnityEngine.EventSystems;

namespace TINS.Campaign
{
    public sealed class CampaignSkillTreeScroll : UnityEngine.UI.ScrollRect
    {
        [SerializeField] private Rect graphBounds = new Rect(-450f, -260f, 900f, 520f);
        [SerializeField, Min(0.1f)] private float minimumZoom = .5f;
        [SerializeField, Min(1f)] private float maximumZoom = 2f;

        public override void OnScroll(PointerEventData eventData)
        {
            if (!IsActive() || eventData.scrollDelta.y == 0f) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, eventData.position,
                eventData.enterEventCamera, out Vector2 position))
                Zoom(Mathf.Pow(1.16f, Mathf.Clamp(eventData.scrollDelta.y, -4f, 4f)), position);
            eventData.Use();
        }

        public void Zoom(float multiplier) => Zoom(multiplier, viewport.rect.center);

        private void Zoom(float multiplier, Vector2 pivot)
        {
            float previous = content.localScale.x;
            float zoom = Mathf.Clamp(previous * multiplier, minimumZoom, maximumZoom);
            if (Mathf.Approximately(zoom, previous)) return;

            StopMovement();
            content.localScale = new Vector3(zoom, zoom, 1f);
            content.anchoredPosition = pivot + (content.anchoredPosition - pivot) * (zoom / previous);
            UpdatePanSpace();
        }

        public void CenterGraph() => CenterGraph(graphBounds);

        public void CenterGraph(Rect visibleBounds)
        {
            Canvas.ForceUpdateCanvases();
            float zoom = Mathf.Clamp(Mathf.Min(1f, viewport.rect.width / visibleBounds.width,
                viewport.rect.height / visibleBounds.height), minimumZoom, maximumZoom);
            content.localScale = new Vector3(zoom, zoom, 1f);
            content.anchoredPosition = -visibleBounds.center * zoom;
            StopMovement();
            UpdatePanSpace();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (viewport != null && content != null) UpdatePanSpace();
        }

        private void UpdatePanSpace()
        {
            // Leave room to drag even when the whole graph fits at the current zoom.
            float zoom = Mathf.Max(.01f, content.localScale.x);
            Vector2 extent = Vector2.Max(-graphBounds.min, graphBounds.max);
            Vector2 size = extent * 2f + viewport.rect.size / zoom;
            if (content.sizeDelta != size) content.sizeDelta = size;
        }
    }
}
