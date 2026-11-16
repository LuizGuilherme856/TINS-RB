using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TINS.Campaign
{
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

        private const int SlotsPerRow = 6;
        private const float RowStep = 44f;
        private const int EmptyFrame = 4;
        private readonly List<UnityEngine.UI.RawImage> slots = new();
        private readonly List<float> shotTimes = new();
        private readonly List<float> slotHeights = new();
        private readonly Rect[] frameUvs = new Rect[5];
        private BeamMeleeUsesManager manager;
        private Vector2 firstRowBarPosition;
        private int visibleSlots;

        private void Awake()
        {
            firstRowBarPosition = reloadBar.anchoredPosition;
            foreach (var slot in chargeSlots) AddSlot(slot);
            for (int i = 0; i < EmptyFrame; i++)
            {
                Sprite sprite = chargeFrames[Mathf.Max(0, i - 1)];
                // The Aseprite frames contain a strip of charges. Use just the relevant four pixels.
                int chargeOffset = i == 1 || i == 2 ? 6 : 12;
                frameUvs[i] = FrameUv(sprite, chargeOffset, 10);
            }
            frameUvs[EmptyFrame] = FrameUv(chargeFrames[3], 0, 6);
        }

        public void Bind(BeamMeleeUsesManager source)
        {
            Unbind();
            manager = source;
            if (manager == null)
            {
                gameObject.SetActive(false);
                return;
            }

            manager.OnUseSpent += AnimateShot;
            manager.OnUsesChanged += RefreshCapacity;
            RefreshCapacity(manager.UsesRemaining, manager.MaxUses);
            Redraw();
        }

        private void OnDestroy() => Unbind();

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
            {
                int index = slots.Count;
                var template = chargeSlots[index % SlotsPerRow];
                var slot = Instantiate(template, template.transform.parent, false);
                slot.name = $"Charge {index + 1:00}";
                slot.rectTransform.sizeDelta = new Vector2(slot.rectTransform.sizeDelta.x, slotHeights[index % SlotsPerRow]);
                slot.rectTransform.anchoredPosition += Vector2.up * (index / SlotsPerRow * RowStep);
                AddSlot(slot);
            }

            if (visibleSlots != maximum)
            {
                visibleSlots = maximum;
                for (int i = 0; i < slots.Count; i++)
                    slots[i].gameObject.SetActive(i < maximum);
                reloadBar.anchoredPosition = firstRowBarPosition + Vector2.up * ((maximum - 1) / SlotsPerRow * RowStep);
            }

            for (int i = 0; i < remaining; i++) shotTimes[i] = float.NegativeInfinity;
        }

        private void AddSlot(UnityEngine.UI.RawImage slot)
        {
            slots.Add(slot);
            slotHeights.Add(slot.rectTransform.sizeDelta.y);
            shotTimes.Add(float.NegativeInfinity);
        }

        private void LateUpdate()
        {
            if (manager != null) Redraw();
        }

        private void Redraw()
        {
            for (int i = 0; i < visibleSlots; i++)
            {
                int frame = EmptyFrame;
                if (i < manager.UsesRemaining)
                    frame = 0;
                else
                {
                    float elapsed = Time.time - shotTimes[i];
                    if (elapsed < shotFrameDuration * 3f)
                        frame = 1 + Mathf.FloorToInt(elapsed / shotFrameDuration);
                }

                var slot = slots[i];
                if (slot.uvRect == frameUvs[frame]) continue;
                slot.texture = chargeFrames[Mathf.Clamp(frame - 1, 0, 3)].texture;
                slot.uvRect = frameUvs[frame];
                Vector2 size = slot.rectTransform.sizeDelta;
                size.y = slotHeights[i] * (frame == EmptyFrame ? 0.6f : 1f);
                slot.rectTransform.sizeDelta = size;
            }

            bool reloading = manager.IsRecharging;
            reloadingLabel.enabled = reloading;
            reloadBar.gameObject.SetActive(reloading);
            if (!reloading) return;
            PlaceMarker(progressMarker, manager.RechargeProgress);
            skillCheckMarker.gameObject.SetActive(manager.IsSkillCheckAvailable);
            if (manager.IsSkillCheckAvailable) PlaceMarker(skillCheckMarker, manager.SkillCheckTarget);
        }

        private void PlaceMarker(RectTransform marker, float progress)
        {
            // The cursor travels downward. Keep it on the same four-unit pixel grid as the sprites.
            float halfHeight = progressMarker.rect.height * 0.5f;
            float travel = Mathf.Max(0f, travelArea.rect.height - 2f * halfHeight);
            Vector2 position = marker.anchoredPosition;
            position.y = -halfHeight - Mathf.Clamp(4f * Mathf.Round(progress * travel / 4f), 0f, travel);
            marker.anchoredPosition = position;
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
