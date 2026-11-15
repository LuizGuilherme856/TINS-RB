using UnityEngine;

namespace TINS.Campaign
{
    /// <summary>Expands a circular stencil over the green map as the planet is purified.</summary>
    public sealed class CampaignPurificationMapMask : MonoBehaviour
    {
        [SerializeField] private RectTransform mapRect;
        [SerializeField] private RectTransform maskRect;
        [SerializeField, Range(0.02f, 0.3f)] private float maximumRadiusFraction = 0.13f;

        public void Configure(RectTransform map, RectTransform circle)
        {
            mapRect = map;
            maskRect = circle;
        }

        public void SetProgress(float normalizedProgress)
        {
            float progress = Mathf.Clamp01(normalizedProgress);
            if (progress <= 0f)
            {
                if (gameObject.activeSelf) gameObject.SetActive(false);
                return;
            }

            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (mapRect == null || maskRect == null) return;

            float radius = Mathf.Min(mapRect.rect.width, mapRect.rect.height) * maximumRadiusFraction * Mathf.Sqrt(progress);
            maskRect.sizeDelta = Vector2.one * (radius * 2f);
        }
    }
}
