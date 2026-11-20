using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TINS.Campaign
{
    public sealed class CampaignPlanetBoardRaycaster : GraphicRaycaster
    {
        internal RawImage Display { private get; set; }

        public override int sortOrderPriority => Display.canvas.rootCanvas.sortingOrder + 1;
        public override int renderOrderPriority => Display.canvas.rootCanvas.renderOrder;

        public override void Raycast(PointerEventData data, List<RaycastResult> results)
        {
            if (Display == null || data.displayIndex != Display.canvas.rootCanvas.targetDisplay ||
                !TryGetBufferPoint(data.position, out var bufferPoint, true)) return;

            Vector2 screenPosition = data.position;
            int first = results.Count;
            try
            {
                data.position = bufferPoint;
                base.Raycast(data, results);
            }
            finally
            {
                data.position = screenPosition;
            }

            for (int i = first; i < results.Count; i++)
            {
                RaycastResult result = results[i];
                result.screenPosition = screenPosition;
                results[i] = result;
            }
        }

        internal bool TryGetBufferPoint(Vector2 screenPoint, out Vector2 bufferPoint, bool requireInside = false)
        {
            bufferPoint = default;
            if (Display == null || Display.texture == null) return false;
            var host = Display.canvas.rootCanvas;
            Camera camera = host.renderMode == RenderMode.ScreenSpaceOverlay ? null : host.worldCamera;
            var rect = Display.GetPixelAdjustedRect();
            if (rect.width <= 0f || rect.height <= 0f ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(Display.rectTransform, screenPoint, camera, out var local) ||
                (requireInside && !rect.Contains(local))) return false;

            // Drag positions outside the display must stay proportional to pointer movement.
            Vector2 uv = (local - rect.min) / rect.size;
            bufferPoint = uv * new Vector2(Display.texture.width, Display.texture.height);
            return true;
        }
    }
}
