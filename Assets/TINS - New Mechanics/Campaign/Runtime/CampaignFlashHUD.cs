using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TINS.Campaign
{
    [DefaultExecutionOrder(1000)]
    public sealed class CampaignFlashHUD : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.RawImage[] chargeSlots;
        [SerializeField] private Sprite[] chargeFrames;
        [SerializeField, Min(0.01f)] private float shotFrameDuration = 0.1f;
        [SerializeField] private TMP_Text reloadingLabel;
        [SerializeField] private RectTransform reloadBar;
        [SerializeField] private RectTransform travelArea;
        [SerializeField] private RectTransform progressMarker;
        [SerializeField] private RectTransform skillCheckMarker;
        [SerializeField, Min(0f)] private float headGap = 0.12f;

        private const int SlotsPerRow = 8;
        private const float SlotWidth = 8f;
        private const float SlotHeight = 16f;
        private const float SlotStep = 12f;
        private const float RowStep = 20f;
        private const float TrackHeight = 7f;
        private const float ReadyDuration = .85f;
        private const int EmptyFrame = 4;

        private readonly List<UnityEngine.UI.RawImage> slots = new();
        private readonly List<float> shotTimes = new();
        private readonly Rect[] frameUvs = new Rect[5];
        private BeamMeleeUsesManager manager;
        private Camera gameplayCamera;
        private Canvas canvas;
        private CanvasGroup visibility;
        private RectTransform root;
        private RectTransform parent;
        private RectTransform panel;
        private RectTransform reloadTrack;
        private RectTransform chargeRow;
        private SpriteRenderer playerSprite;
        private int visibleSlots;
        private float trackWidth;
        private bool wasRecharging;
        private bool statusVisible;
        private float readyUntil;
        private int displayedTenths;

        private void Awake()
        {
            root = (RectTransform)transform;
            root.pivot = new Vector2(0.5f, 0f);
            parent = (RectTransform)root.parent;
            canvas = GetComponentInParent<Canvas>();
            visibility = gameObject.AddComponent<CanvasGroup>();
            visibility.blocksRaycasts = false;
            visibility.interactable = false;
            visibility.alpha = 0f;
            root.SetAsLastSibling();

            foreach (var slot in chargeSlots) AddSlot(slot);
            chargeRow = (RectTransform)chargeSlots[0].transform.parent;
            for (int i = 0; i < EmptyFrame; i++)
            {
                Sprite sprite = chargeFrames[Mathf.Max(0, i - 1)];
                // The source frames contain a strip. Sample just the relevant charge.
                int chargeOffset = i == 1 || i == 2 ? 6 : 12;
                frameUvs[i] = FrameUv(sprite, chargeOffset, 10);
            }
            frameUvs[EmptyFrame] = FrameUv(chargeFrames[3], 0, 6);

            panel = (RectTransform)transform.Find("Flash Panel");
            var backdrop = panel.GetComponent<UnityEngine.UI.Image>();
            backdrop.sprite = null;
            backdrop.color = new Color32(18, 15, 35, 235);
            panel.Find("Flash Icon").gameObject.SetActive(false);
            var track = reloadBar.GetComponentInChildren<UnityEngine.UI.Image>(true);
            reloadTrack = track.rectTransform;
            track.sprite = null;
            track.color = new Color32(65, 53, 83, 255);
            progressMarker.GetComponent<UnityEngine.UI.Image>().color = new Color32(255, 197, 94, 255);
            skillCheckMarker.GetComponent<UnityEngine.UI.Image>().color = new Color32(116, 236, 212, 210);
            skillCheckMarker.SetAsLastSibling();
            reloadingLabel.fontSize = 13f;
            reloadingLabel.alignment = TextAlignmentOptions.Center;
            reloadingLabel.raycastTarget = false;
            reloadingLabel.enabled = false;
        }

        public void Bind(BeamMeleeUsesManager source)
        {
            Unbind();
            manager = source;
            playerSprite = source != null ? source.GetComponent<SpriteRenderer>() : null;
            gameObject.SetActive(source != null);
            if (manager == null) return;

            wasRecharging = manager.IsRecharging;
            statusVisible = false;
            reloadingLabel.enabled = false;
            readyUntil = 0f;
            displayedTenths = -1;
            manager.OnUseSpent += AnimateShot;
            manager.OnUsesChanged += RefreshCapacity;
            RefreshCapacity(manager.UsesRemaining, manager.MaxUses);
            LayoutPanel(false, false);
            Redraw();
        }

        private void OnDestroy() => Unbind();

        private void OnEnable() => Canvas.willRenderCanvases += UpdatePosition;

        private void OnDisable() => Canvas.willRenderCanvases -= UpdatePosition;

        private void Unbind()
        {
            if (manager == null) return;
            manager.OnUseSpent -= AnimateShot;
            manager.OnUsesChanged -= RefreshCapacity;
        }

        private void AnimateShot(int slotIndex)
        {
            if (slotIndex < shotTimes.Count) shotTimes[slotIndex] = Time.time;
        }

        private void RefreshCapacity(int remaining, int maximum)
        {
            while (slots.Count < maximum)
                AddSlot(Instantiate(chargeSlots[0], chargeSlots[0].transform.parent, false));

            if (visibleSlots != maximum)
            {
                visibleSlots = maximum;
                trackWidth = Mathf.Max(92f, (Mathf.Min(maximum, SlotsPerRow) - 1) * SlotStep + SlotWidth);
                Layout(reloadBar, 3f, trackWidth, TrackHeight);
                Layout(reloadTrack, 0f, trackWidth, TrackHeight);
                Layout(travelArea, 0f, trackWidth, TrackHeight);
                for (int i = 0; i < slots.Count; i++)
                {
                    slots[i].gameObject.SetActive(i < maximum);
                    if (i >= maximum) continue;
                    int row = i / SlotsPerRow;
                    int countInRow = Mathf.Min(SlotsPerRow, maximum - row * SlotsPerRow);
                    Layout(slots[i].rectTransform, row * RowStep, SlotWidth, SlotHeight);
                    slots[i].rectTransform.anchoredPosition += Vector2.right * ((i % SlotsPerRow - (countInRow - 1) * 0.5f) * SlotStep);
                }

                LayoutPanel(manager.IsRecharging, statusVisible);
            }

            for (int i = 0; i < remaining; i++) shotTimes[i] = float.NegativeInfinity;
        }

        private void LayoutPanel(bool reloading, bool showStatus)
        {
            float bottom = reloading ? TrackHeight + 5f : 3f;
            int rows = (visibleSlots + SlotsPerRow - 1) / SlotsPerRow;
            float chargesTop = bottom + (rows - 1) * RowStep + SlotHeight;
            Layout(chargeRow, bottom, trackWidth, 0f);
            Layout(reloadingLabel.rectTransform, chargesTop + 1f, trackWidth + 12f, 16f);
            Layout(panel, 0f, trackWidth + 14f, chargesTop + (showStatus ? 20f : 3f));
            root.sizeDelta = panel.sizeDelta;
        }

        private void AddSlot(UnityEngine.UI.RawImage slot)
        {
            slots.Add(slot);
            slot.raycastTarget = false;
            shotTimes.Add(float.NegativeInfinity);
        }

        private void LateUpdate()
        {
            if (manager == null) return;
            Redraw();
        }

        private void UpdatePosition()
        {
            if (manager == null) return;
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            if (gameplayCamera == null || !manager.isActiveAndEnabled)
            {
                visibility.alpha = 0f;
                return;
            }
            Vector3 head = manager.transform.position;
            head.y = playerSprite != null ? playerSprite.bounds.max.y + headGap : head.y + 1f + headGap;
            // The Game view and pixel-perfect camera can change pixel size before rendering.
            Vector3 viewport = gameplayCamera.WorldToViewportPoint(head);
            if (viewport.z <= 0f || viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f ||
                !TryGetCanvasPosition(head, viewport, out Vector2 local))
            {
                visibility.alpha = 0f;
                return;
            }
            visibility.alpha = 1f;
            float halfWidth = root.rect.width * root.localScale.x * 0.5f;
            float height = root.rect.height * root.localScale.y;
            local.x = Mathf.Clamp(local.x, parent.rect.xMin + halfWidth, parent.rect.xMax - halfWidth);
            local.y = Mathf.Clamp(local.y, parent.rect.yMin, parent.rect.yMax - height);
            root.localPosition = new Vector3(local.x, local.y, root.localPosition.z);
        }

        private bool TryGetCanvasPosition(Vector3 head, Vector3 viewport, out Vector2 local)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,
                    gameplayCamera.WorldToScreenPoint(head), canvas.worldCamera, out local);

            var canvasRect = (RectTransform)canvas.rootCanvas.transform;
            Rect view = gameplayCamera.rect;
            Vector2 normalized = view.position + Vector2.Scale(view.size, viewport);
            Vector2 point = canvasRect.rect.min + Vector2.Scale(canvasRect.rect.size, normalized);
            local = parent.InverseTransformPoint(canvasRect.TransformPoint(point));
            return true;
        }

        private void Redraw()
        {
            int remaining = manager.UsesRemaining;
            for (int i = 0; i < visibleSlots; i++)
            {
                int frame = EmptyFrame;
                if (i < remaining)
                    frame = 0;
                else
                {
                    float elapsed = Time.time - shotTimes[i];
                    if (elapsed < shotFrameDuration * 3f)
                        frame = 1 + Mathf.FloorToInt(elapsed / shotFrameDuration);
                }

                var slot = slots[i];
                slot.color = frame == EmptyFrame ? new Color32(151, 138, 171, 255) : Color.white;
                if (slot.uvRect == frameUvs[frame]) continue;
                slot.texture = chargeFrames[Mathf.Clamp(frame - 1, 0, 3)].texture;
                slot.uvRect = frameUvs[frame];
            }

            bool reloading = manager.IsRecharging;
            if (reloading != wasRecharging)
            {
                wasRecharging = reloading;
                displayedTenths = -1;
                if (!reloading)
                {
                    readyUntil = Time.time + ReadyDuration;
                    reloadingLabel.color = new Color32(153, 245, 228, 255);
                    reloadingLabel.text = "FLASH PRONTO";
                }
            }
            bool showStatus = reloading || Time.time < readyUntil;
            if (statusVisible != showStatus || reloadBar.gameObject.activeSelf != reloading)
            {
                statusVisible = showStatus;
                reloadingLabel.enabled = showStatus;
                reloadBar.gameObject.SetActive(reloading);
                LayoutPanel(reloading, showStatus);
            }

            if (!reloading) return;

            float progress = manager.RechargeProgress;
            int tenths = Mathf.CeilToInt((1f - progress) * manager.RechargeDuration * 10f);
            if (tenths != displayedTenths)
            {
                displayedTenths = tenths;
                reloadingLabel.color = new Color32(255, 219, 147, 255);
                reloadingLabel.SetText("RECARGA {0:1} s", tenths / 10f);
            }

            PlaceBarSegment(progressMarker, 0f, progress, TrackHeight - 2f);
            skillCheckMarker.gameObject.SetActive(manager.IsSkillCheckAvailable);
            if (manager.IsSkillCheckAvailable)
            {
                float halfWidth = manager.SkillCheckWidth * 0.5f;
                PlaceBarSegment(skillCheckMarker, manager.SkillCheckTarget - halfWidth,
                    manager.SkillCheckTarget + halfWidth, TrackHeight);
            }
        }

        private static void Layout(RectTransform rect, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private void PlaceBarSegment(RectTransform rect, float start, float end, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(start * trackWidth, 0f);
            rect.sizeDelta = new Vector2((end - start) * trackWidth, height);
        }

        private static Rect FrameUv(Sprite sprite, int x, int height)
        {
            Rect rect = sprite.rect;
            Texture texture = sprite.texture;
            return new Rect((rect.x + x) / texture.width, rect.y / texture.height,
                4f / texture.width, (float)height / texture.height);
        }
    }
}
