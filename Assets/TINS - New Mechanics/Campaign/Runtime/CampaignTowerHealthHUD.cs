using System.Collections.Generic;
using UnityEngine;

namespace TINS.Campaign
{
    public sealed class CampaignTowerHealthHUD : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image[] healthSlots;
        [SerializeField] private Sprite[] damageFrames;
        [SerializeField, Min(0.01f)] private float frameDuration = 0.1f;
        [SerializeField, Min(1f)] private float extraSlotSpacing = 28f;

        private readonly List<UnityEngine.UI.Image> slots = new();
        private readonly List<float> lostAt = new();
        private readonly List<float> fullSlotHeights = new();
        private TowerController tower;
        private int visibleSlots;
        private int previousLife;

        private void Awake()
        {
            foreach (UnityEngine.UI.Image slot in healthSlots) AddSlot(slot);
        }

        public void Bind(TowerController source)
        {
            tower = source;
            if (tower == null)
            {
                gameObject.SetActive(false);
                return;
            }
            Resize(tower.maxLife);
            previousLife = Mathf.Clamp(tower.currentLife, 0, visibleSlots);
            Draw(previousLife, Time.time);
        }

        private void LateUpdate()
        {
            if (tower == null) return;

            int capacity = Mathf.Max(0, tower.maxLife);
            bool capacityChanged = visibleSlots != capacity;
            if (capacityChanged) Resize(capacity);
            int life = Mathf.Clamp(tower.currentLife, 0, visibleSlots);
            float now = Time.time;
            if (!capacityChanged)
            {
                for (int i = life; i < previousLife; i++) lostAt[i] = now;
                for (int i = previousLife; i < life; i++) lostAt[i] = float.NegativeInfinity;
            }

            previousLife = life;
            Draw(life, now);
        }

        private void AddSlot(UnityEngine.UI.Image slot)
        {
            slots.Add(slot);
            lostAt.Add(float.NegativeInfinity);
            fullSlotHeights.Add(slot.rectTransform.sizeDelta.y);
        }

        private void Resize(int maximum)
        {
            while (slots.Count < maximum)
            {
                int index = slots.Count;
                UnityEngine.UI.Image template = healthSlots[healthSlots.Length - 1];
                UnityEngine.UI.Image slot = Instantiate(template, template.transform.parent, false);
                slot.name = $"HP {index + 1:00}";
                slot.rectTransform.anchoredPosition += Vector2.right * ((index - healthSlots.Length + 1) * extraSlotSpacing);
                slot.rectTransform.sizeDelta = new Vector2(slot.rectTransform.sizeDelta.x, fullSlotHeights[healthSlots.Length - 1]);
                AddSlot(slot);
            }

            visibleSlots = Mathf.Max(0, maximum);
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].gameObject.SetActive(i < visibleSlots);
                lostAt[i] = float.NegativeInfinity;
            }
        }

        private void Draw(int life, float now)
        {
            int emptyFrame = damageFrames.Length - 1;
            float duration = Mathf.Max(0.01f, frameDuration);
            for (int i = 0; i < visibleSlots; i++)
            {
                int frame = 0;
                if (i >= life)
                {
                    float elapsed = now - lostAt[i];
                    frame = elapsed < (emptyFrame - 1) * duration
                        ? 1 + Mathf.FloorToInt(elapsed / duration)
                        : emptyFrame;
                }

                UnityEngine.UI.Image slot = slots[i];
                Sprite sprite = damageFrames[frame];
                if (slot.sprite == sprite) continue;

                slot.sprite = sprite;
                Vector2 size = slot.rectTransform.sizeDelta;
                size.y = fullSlotHeights[i] * (sprite.rect.height / damageFrames[0].rect.height);
                slot.rectTransform.sizeDelta = size;
            }
        }
    }
}
