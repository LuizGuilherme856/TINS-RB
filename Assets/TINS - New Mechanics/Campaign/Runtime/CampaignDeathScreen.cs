using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TINS.Campaign
{
    public sealed class CampaignDeathScreen : MonoBehaviour
    {
        private const int Columns = 5;
        private const int Rows = 3;
        private const int FrameCount = 13;

        [SerializeField] private UnityEngine.UI.RawImage artwork;
        [SerializeField] private CanvasGroup summary;
        [SerializeField] private TMP_Text reason;
        [SerializeField] private TMP_Text reward;
        [SerializeField] private TMP_Text unlockHint;
        [SerializeField] private UnityEngine.UI.Button returnButton;
        [SerializeField, Min(.01f)] private float secondsPerFrame = .115f;

        private Canvas screenCanvas;
        private Action returnToBase;

        private void Awake()
        {
            screenCanvas = GetComponent<Canvas>();
            returnButton.onClick.AddListener(Continue);
        }

        internal void Present(string destination, string failureReason, int souls, string materials, bool flashAvailable, Action onReturn)
        {
            returnToBase = onReturn;
            reason.text = $"{destination}  ·  {failureReason}";
            reward.text = souls switch
            {
                0 => "0 almas",
                1 => "+1 alma",
                _ => $"+{souls} almas"
            };
            if (!string.IsNullOrEmpty(materials)) reward.text += $"  ·  {materials}";
            unlockHint.text = flashAvailable ? "Flash de Luz disponível na Base." : string.Empty;
            summary.alpha = 0f;
            summary.interactable = false;
            summary.blocksRaycasts = false;
            EventSystem.current?.SetSelectedGameObject(null);
            SetFrame(0);
            FitArtwork();
            StartCoroutine(Reveal());
        }

        private void OnRectTransformDimensionsChange()
        {
            if (screenCanvas != null) FitArtwork();
        }

        private void FitArtwork()
        {
            var image = artwork.rectTransform;
            Vector2 area = ((RectTransform)image.parent).rect.size * screenCanvas.scaleFactor;
            var frame = new Vector2(artwork.texture.width / Columns, artwork.texture.height / Rows);
            float fit = Mathf.Min(area.x / frame.x, area.y / frame.y);
            float scale = fit >= 1f ? Mathf.Floor(fit) : fit;
            Vector2 size = frame * scale;
            image.sizeDelta = size / screenCanvas.scaleFactor;
            Vector2 margins = (area - size) * .5f;
            image.anchoredPosition = new Vector2(Mathf.Floor(margins.x) - margins.x, Mathf.Floor(margins.y) - margins.y)
                / screenCanvas.scaleFactor;
        }

        private void SetFrame(int index)
        {
            int column = index % Columns;
            int row = index / Columns;
            artwork.uvRect = new Rect((float)column / Columns, (float)(Rows - 1 - row) / Rows,
                1f / Columns, 1f / Rows);
        }

        private IEnumerator Reveal()
        {
            float elapsed = 0f;
            int last = 0;
            while (last < FrameCount - 1)
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;
                int frame = Mathf.Min(FrameCount - 1, Mathf.FloorToInt(elapsed / secondsPerFrame));
                if (frame == last) continue;
                SetFrame(frame);
                last = frame;
            }

            const float revealDuration = .22f;
            for (elapsed = 0f; elapsed < revealDuration; elapsed += Time.unscaledDeltaTime)
            {
                summary.alpha = Mathf.SmoothStep(0f, 1f, elapsed / revealDuration);
                yield return null;
            }

            summary.alpha = 1f;
            summary.interactable = true;
            summary.blocksRaycasts = true;
            EventSystem.current?.SetSelectedGameObject(returnButton.gameObject);
        }

        private void Continue()
        {
            if (!summary.interactable) return;
            var action = returnToBase;
            returnToBase = null;
            summary.interactable = false;
            action?.Invoke();
        }
    }
}
