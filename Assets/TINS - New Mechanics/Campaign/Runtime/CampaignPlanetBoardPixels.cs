using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TINS.Campaign
{
    internal sealed class CampaignPlanetBoardPixels : IDisposable
    {
        private static readonly Vector2Int ReferenceSize = new(480, 270);

        private readonly RectTransform host;
        private readonly Canvas hostCanvas;
        private readonly Canvas sourceCanvas;
        private readonly Camera sourceCamera;
        private readonly RawImage display;
        private readonly Vector2 designSize;
        private RenderTexture target;

        internal CampaignPlanetBoardPixels(RectTransform host, RectTransform layout)
        {
            this.host = host;
            designSize = layout.rect.size;
            hostCanvas = host.GetComponentInParent<Canvas>().rootCanvas;

            var renderer = new GameObject("Expedition Pixel Camera", typeof(Camera));
            renderer.SetActive(false);
            SceneManager.MoveGameObjectToScene(renderer, host.gameObject.scene);
            renderer.transform.position = new Vector3(0, 0, -10000);
            sourceCamera = renderer.GetComponent<Camera>();
            sourceCamera.clearFlags = CameraClearFlags.SolidColor;
            sourceCamera.backgroundColor = new Color32(12, 16, 27, 255);
            sourceCamera.cullingMask = 1 << layout.gameObject.layer;
            sourceCamera.orthographic = true;
            sourceCamera.nearClipPlane = .1f;
            sourceCamera.farClipPlane = 2f;
            sourceCamera.allowHDR = false;
            sourceCamera.allowMSAA = false;
            sourceCamera.allowDynamicResolution = false;
            sourceCamera.targetDisplay = hostCanvas.targetDisplay;

            var surface = new GameObject("Expedition Pixel Canvas",
                typeof(RectTransform), typeof(Canvas), typeof(CampaignPlanetBoardRaycaster));
            surface.layer = layout.gameObject.layer;
            surface.transform.SetParent(renderer.transform, false);
            sourceCanvas = surface.GetComponent<Canvas>();
            sourceCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            sourceCanvas.worldCamera = sourceCamera;
            sourceCanvas.planeDistance = 1f;
            sourceCanvas.pixelPerfect = true;
            sourceCanvas.targetDisplay = hostCanvas.targetDisplay;
            sourceCanvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;

            var presentation = new GameObject("Expedition Pixel Display",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            presentation.layer = host.gameObject.layer;
            presentation.transform.SetParent(host, false);
            display = presentation.GetComponent<RawImage>();
            display.raycastTarget = false;
            display.rectTransform.anchorMin = display.rectTransform.anchorMax = Vector2.one * .5f;
            surface.GetComponent<CampaignPlanetBoardRaycaster>().Display = display;

            layout.SetParent(surface.transform, false);
            layout.localScale = Vector3.one;
        }

        internal void Show()
        {
            Fit();
            sourceCamera.gameObject.SetActive(true);
        }

        internal void Hide()
        {
            if (sourceCamera != null) sourceCamera.gameObject.SetActive(false);
        }

        internal void Fit()
        {
            float canvasScale = hostCanvas.scaleFactor;
            Vector2 available = host.rect.size * canvasScale;
            if (available.x <= 0f || available.y <= 0f || canvasScale <= 0f) return;

            var resolution = ReferenceSize;
            while (resolution.x > available.x || resolution.y > available.y)
            {
                if (resolution.x % 2 != 0 || resolution.y % 2 != 0) break;
                resolution /= 2;
            }
            if (target == null || target.width != resolution.x || target.height != resolution.y)
                SetResolution(resolution);

            float fit = Mathf.Min(available.x / resolution.x, available.y / resolution.y);
            float scale = fit >= 1f ? Mathf.Floor(fit + .0001f) : fit;
            Vector2 size = (Vector2)resolution * scale;
            var rect = display.rectTransform;
            rect.sizeDelta = size / canvasScale;
            // Floor the bottom-left edge so an odd-width window never puts the whole board between pixels.
            Vector2 margin = (available - size) * .5f;
            rect.anchoredPosition = new Vector2(Mathf.Floor(margin.x) - margin.x,
                Mathf.Floor(margin.y) - margin.y) / canvasScale;
        }

        private void SetResolution(Vector2Int resolution)
        {
            ReleaseTarget();
            target = new RenderTexture(resolution.x, resolution.y, 24, RenderTextureFormat.ARGB32)
            {
                name = "Expedition Pixel Buffer",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                antiAliasing = 1,
                useMipMap = false
            };
            sourceCamera.targetTexture = target;
            sourceCanvas.scaleFactor = resolution.x / designSize.x;
            display.texture = target;
        }

        private void ReleaseTarget()
        {
            if (target == null) return;
            if (sourceCamera != null) sourceCamera.targetTexture = null;
            if (display != null) display.texture = null;
            target.Release();
            UnityEngine.Object.Destroy(target);
            target = null;
        }

        public void Dispose()
        {
            ReleaseTarget();
            if (sourceCamera != null) UnityEngine.Object.Destroy(sourceCamera.gameObject);
            if (display != null) UnityEngine.Object.Destroy(display.gameObject);
        }
    }
}
