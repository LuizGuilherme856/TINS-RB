using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TINS.Campaign
{
    public sealed class CampaignPlanetNavigation : MonoBehaviour
    {
        [SerializeField] private RectTransform layout;
        [SerializeField] private GameObject overview;
        [SerializeField] private GameObject details;
        [SerializeField] private CampaignPlanetMarker[] markers;
        [SerializeField] private UnityEngine.UI.Image focusedPlanet;
        [SerializeField] private UnityEngine.UI.Image detailImage;
        [SerializeField] private TMP_Text planetName;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_Text rewards;
        [SerializeField] private TMP_Text progress;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private UnityEngine.UI.Button backButton;
        [SerializeField] private UnityEngine.UI.Button launchButton;
        [SerializeField, Min(0)] private float zoomDuration = .32f;

        private CampaignCatalog catalog;
        private CampaignProfile profile;
        private Action<int> launch;
        private CampaignPlanetMarker selected;
        private int selectedIndex = -1;
        private Vector2 focusPosition;
        private Vector2 focusSize;

        private void OnEnable() => FitLayout();

        private void OnRectTransformDimensionsChange() => FitLayout();

        private void FitLayout()
        {
            if (layout == null) return;
            Vector2 available = ((RectTransform)transform).rect.size;
            Vector2 design = layout.rect.size;
            if (available.x <= 0 || available.y <= 0 || design.x <= 0 || design.y <= 0) return;
            float scale = Mathf.Min(available.x / design.x, available.y / design.y);
            layout.localScale = new Vector3(scale, scale, 1);
        }

        public void Configure(CampaignCatalog campaignCatalog, CampaignProfile campaignProfile,
            Action<int> launchMission, Action close)
        {
            catalog = campaignCatalog;
            profile = campaignProfile;
            launch = launchMission;
            focusPosition = focusedPlanet.rectTransform.anchoredPosition;
            focusSize = focusedPlanet.rectTransform.sizeDelta;

            foreach (CampaignPlanetMarker marker in markers)
            {
                marker.Button.interactable = catalog.IndexOf(marker.PlanetId) >= 0;
                marker.Button.onClick.AddListener(() => ShowPlanet(marker));
            }
            closeButton.onClick.AddListener(() => close());
            backButton.onClick.AddListener(() => TryGoBack());
            launchButton.onClick.AddListener(() =>
            {
                if (selectedIndex >= 0 && profile.IsUnlocked(catalog, selectedIndex))
                    launch(selectedIndex);
            });
        }

        public void Open()
        {
            selected = null;
            gameObject.SetActive(true);
            ShowOverview();
        }

        public bool TryGoBack()
        {
            if (selectedIndex < 0) return false;
            ShowOverview();
            return true;
        }

        private void ShowOverview()
        {
            StopAllCoroutines();
            selectedIndex = -1;
            details.SetActive(false);
            overview.SetActive(true);

            if (selected != null && selected.Button.interactable)
            {
                Select(selected.Button);
                return;
            }
            foreach (CampaignPlanetMarker marker in markers)
            {
                if (!marker.Button.interactable) continue;
                Select(marker.Button);
                return;
            }
            Select(closeButton);
        }

        private void ShowPlanet(CampaignPlanetMarker marker)
        {
            int index = catalog.IndexOf(marker.PlanetId);
            if (index < 0 || selectedIndex >= 0) return;

            selected = marker;
            selectedIndex = index;
            CampaignPlanet planet = catalog.planets[index];
            CampaignPlanet prerequisite = profile.FirstUnpurifiedPrerequisite(catalog, index);
            focusedPlanet.sprite = marker.PlanetArt.sprite;
            focusedPlanet.color = marker.PlanetArt.color;
            detailImage.sprite = marker.DetailImage;
            planetName.text = planet.displayName;
            description.text = planet.description;
            rewards.text = $"{planet.victorySouls} almas + {planet.rewardPerMission} {planet.rewardResourceName}";
            progress.text = prerequisite != null
                ? $"Conclua {prerequisite.missionsToPurify} missões em {prerequisite.displayName} ({profile.MissionsOn(prerequisite.id)}/{prerequisite.missionsToPurify})."
                : profile.IsPurified(planet)
                    ? "Purificado"
                    : $"Purificação  {profile.MissionsOn(planet.id)}/{planet.missionsToPurify}";
            launchButton.interactable = prerequisite == null;

            RectTransform art = marker.PlanetArt.rectTransform;
            RectTransform focus = focusedPlanet.rectTransform;
            Vector2 start = focus.parent.InverseTransformPoint(art.TransformPoint(art.rect.center));
            Vector2 startSize = Vector2.Scale(art.rect.size, art.localScale);

            overview.SetActive(false);
            focus.anchoredPosition = start;
            focus.sizeDelta = startSize;
            details.SetActive(true);
            StartCoroutine(Zoom(focus, start, startSize));
            Select(launchButton.interactable ? launchButton : backButton);
        }

        private IEnumerator Zoom(RectTransform target, Vector2 start, Vector2 startSize)
        {
            for (float elapsed = 0; elapsed < zoomDuration; elapsed += Time.unscaledDeltaTime)
            {
                float blend = Mathf.SmoothStep(0, 1, elapsed / zoomDuration);
                target.anchoredPosition = Vector2.LerpUnclamped(start, focusPosition, blend);
                target.sizeDelta = Vector2.LerpUnclamped(startSize, focusSize, blend);
                yield return null;
            }
            target.anchoredPosition = focusPosition;
            target.sizeDelta = focusSize;
        }

        private static void Select(UnityEngine.UI.Button button)
            => EventSystem.current?.SetSelectedGameObject(button.gameObject);
    }
}
