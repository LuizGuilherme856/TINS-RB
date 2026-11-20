using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private CampaignPlanetMapGraphic routeChart;
        [SerializeField] private CampaignPlanetMapViewport mapViewport;
        [SerializeField] private TMP_Text campaignProgress;
        [SerializeField] private TMP_Text mapHint;
        [SerializeField] private TMP_Text detailStatus;
        [SerializeField] private UnityEngine.UI.Image detailMeter;
        [SerializeField] private CanvasGroup detailCard;
        [SerializeField] private AudioSource feedbackAudio;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip lockedSound;
        [SerializeField] private AudioClip backSound;
        [SerializeField, Min(0)] private float zoomDuration = .32f;

        private CampaignCatalog catalog;
        private CampaignProfile profile;
        private Action<int> launch;
        private CampaignPlanetMarker selected;
        private int selectedIndex = -1;
        private Vector2 focusPosition;
        private Vector2 focusSize;
        private readonly Dictionary<CampaignPlanetMarker, CampaignPlanetRotation> rotations = new();
        private readonly Dictionary<CampaignPlanetMarker, Sprite> placeholderSprites = new();
        private CampaignPlanetMarker[] campaignOrder;
        private float rotationTime;
        private int displayedFrame;
        private CampaignPlanetBoardPixels pixelBoard;

        private void OnEnable()
        {
            if (layout == null) return;
            pixelBoard ??= new CampaignPlanetBoardPixels((RectTransform)transform, layout);
            pixelBoard.Show();
        }

        private void OnDisable() => pixelBoard?.Hide();

        private void OnRectTransformDimensionsChange() => pixelBoard?.Fit();

        public void Configure(CampaignCatalog campaignCatalog, CampaignProfile campaignProfile,
            Action<int> launchMission, Action close)
        {
            catalog = campaignCatalog;
            profile = campaignProfile;
            launch = launchMission;
            focusPosition = focusedPlanet.rectTransform.anchoredPosition;
            focusSize = focusedPlanet.rectTransform.sizeDelta;
            campaignOrder = new CampaignPlanetMarker[catalog.planets.Length];

            foreach (CampaignPlanetMarker marker in markers)
            {
                placeholderSprites.TryAdd(marker, marker.PlanetArt.sprite);
                int index = catalog.IndexOf(marker.PlanetId);
                marker.Button.interactable = index >= 0;
                if (index >= 0) campaignOrder[index] = marker;
                marker.Button.onClick.AddListener(() => ShowPlanet(marker));
                marker.DirectionalFocus += KeepDestinationVisible;
            }
            routeChart?.Bind(campaignOrder);
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
            LoadPlanetRotations();
            RefreshMap();
            ShowOverview(true);
        }

        public bool TryGoBack()
        {
            if (selectedIndex < 0) return false;
            PlaySound(backSound);
            ShowOverview();
            return true;
        }

        private void ShowOverview(bool resetView = false)
        {
            StopAllCoroutines();
            selectedIndex = -1;
            details.SetActive(false);
            overview.SetActive(true);
            ApplyPlanetFrame();

            CampaignPlanetMarker destination = selected != null && selected.Button.interactable
                ? selected : DefaultDestination();
            if (resetView) mapViewport?.Focus(destination?.PlanetArt.rectTransform);
            Select(destination != null ? destination.Button : closeButton);
        }

        private CampaignPlanetMarker DefaultDestination()
        {
            CampaignPlanetMarker first = null;
            foreach (CampaignPlanetMarker marker in campaignOrder)
            {
                if (marker == null || !marker.Button.interactable) continue;
                if (marker.State == CampaignDestinationState.Available) return marker;
                if (first == null) first = marker;
            }
            return first;
        }

        public void FocusDestination() => mapViewport?.Focus(DefaultDestination()?.PlanetArt.rectTransform);

        private void KeepDestinationVisible(CampaignPlanetMarker marker)
            => mapViewport?.KeepVisible((RectTransform)marker.transform);

        private void RefreshMap()
        {
            int purifiedCount = 0;
            int firstAvailable = -1;
            for (int i = 0; i < campaignOrder.Length; i++)
            {
                CampaignPlanet planet = catalog.planets[i];
                if (planet == null) continue;
                bool isPurified = profile.IsPurified(planet);
                if (isPurified) purifiedCount++;
                var state = !profile.IsUnlocked(catalog, i) ? CampaignDestinationState.Locked
                    : isPurified ? CampaignDestinationState.Purified : CampaignDestinationState.Available;
                if (state == CampaignDestinationState.Available && firstAvailable < 0) firstAvailable = i;

                var marker = campaignOrder[i];
                if (marker != null)
                    marker.ShowProgress(state, DestinationColor(state), profile.MissionsOn(planet.id), planet.missionsToPurify);
            }

            if (campaignProgress != null)
                campaignProgress.text = $"{purifiedCount}/{campaignOrder.Length} planetas purificados";
            if (mapHint != null)
                mapHint.text = firstAvailable < 0 ? "Purificação concluída"
                    : firstAvailable == campaignOrder.Length - 1 ? $"Último destino: {catalog.planets[firstAvailable].displayName}"
                    : $"Próxima rota: purifique {catalog.planets[firstAvailable].displayName}.";
            routeChart?.SetVerticesDirty();
        }

        private Color DestinationColor(CampaignDestinationState state)
            => routeChart != null ? routeChart.Tint(state) : Color.white;

        private void ShowPlanet(CampaignPlanetMarker marker)
        {
            int index = catalog.IndexOf(marker.PlanetId);
            if (index < 0 || selectedIndex >= 0) return;

            selected = marker;
            selectedIndex = index;
            CampaignPlanet planet = catalog.planets[index];
            CampaignPlanet prerequisite = profile.FirstUnpurifiedPrerequisite(catalog, index);
            PlaySound(prerequisite == null ? selectSound : lockedSound);
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
            if (detailMeter != null)
            {
                CampaignPlanet target = prerequisite ?? planet;
                float fraction = Mathf.Clamp01((float)profile.MissionsOn(target.id) / Mathf.Max(1, target.missionsToPurify));
                var fill = detailMeter.rectTransform;
                fill.anchorMax = new Vector2(fraction, fill.anchorMax.y);
                detailMeter.color = DestinationColor(marker.State);
            }
            if (detailStatus != null)
            {
                detailStatus.color = DestinationColor(marker.State);
                detailStatus.text = marker.State switch
                {
                    CampaignDestinationState.Locked => "ROTA BLOQUEADA",
                    CampaignDestinationState.Purified => "PLANETA PURIFICADO",
                    _ => "EXPEDIÇÃO DISPONÍVEL"
                };
            }
            if (launchButton.GetComponentInChildren<TMP_Text>() is TMP_Text launchLabel)
                launchLabel.text = prerequisite == null ? "Iniciar missão" : "Missão bloqueada";

            RectTransform art = marker.PlanetArt.rectTransform;
            RectTransform focus = focusedPlanet.rectTransform;
            Vector2 start = focus.parent.InverseTransformPoint(art.TransformPoint(art.rect.center));
            Vector2 startSize = focus.parent.InverseTransformVector(art.TransformVector(art.rect.size));

            overview.SetActive(false);
            focus.anchoredPosition = start;
            focus.sizeDelta = startSize;
            if (detailCard != null) detailCard.alpha = 0f;
            details.SetActive(true);
            ApplyPlanetFrame();
            StartCoroutine(Zoom(focus, start, startSize));
            Select(launchButton.interactable ? launchButton : backButton);
        }

        private void LoadPlanetRotations()
        {
            foreach (CampaignPlanetMarker marker in markers)
            {
                int index = catalog.IndexOf(marker.PlanetId);
                if (index < 0) continue;
                CampaignPlanet planet = catalog.planets[index];
                int stage = CampaignPlanetRotation.StageFor(profile.MissionsOn(planet.id), planet.missionsToPurify);
                if (rotations.TryGetValue(marker, out CampaignPlanetRotation current))
                {
                    if (current.Stage == stage) continue;
                    current.Release();
                    marker.PlanetArt.sprite = placeholderSprites[marker];
                }
                rotations[marker] = new CampaignPlanetRotation(planet.id, stage);
            }
        }

        private void Update()
        {
            float period = CampaignPlanetRotation.FrameCount * CampaignPlanetRotation.SecondsPerFrame;
            rotationTime = Mathf.Repeat(rotationTime + Time.unscaledDeltaTime, period);
            int frame = Mathf.Min(CampaignPlanetRotation.FrameCount - 1,
                Mathf.FloorToInt(rotationTime / CampaignPlanetRotation.SecondsPerFrame));
            if (frame == displayedFrame) return;
            displayedFrame = frame;
            ApplyPlanetFrame();
        }

        private void ApplyPlanetFrame()
        {
            if (selectedIndex >= 0)
            {
                if (rotations.TryGetValue(selected, out CampaignPlanetRotation rotation) &&
                    rotation.Frame(displayedFrame) is Sprite frame)
                {
                    focusedPlanet.sprite = frame;
                    if (selected.UsesPlanetArtInDetails) detailImage.sprite = frame;
                }
                return;
            }

            foreach (var pair in rotations)
                if (pair.Value.Frame(displayedFrame) is Sprite frame)
                    pair.Key.PlanetArt.sprite = frame;
        }

        private void OnDestroy()
        {
            pixelBoard?.Dispose();
            foreach (CampaignPlanetRotation rotation in rotations.Values) rotation.Release();
        }

        private IEnumerator Zoom(RectTransform target, Vector2 start, Vector2 startSize)
        {
            for (float elapsed = 0; elapsed < zoomDuration; elapsed += Time.unscaledDeltaTime)
            {
                float blend = Mathf.SmoothStep(0, 1, elapsed / zoomDuration);
                target.anchoredPosition = Vector2.LerpUnclamped(start, focusPosition, blend);
                target.sizeDelta = Vector2.LerpUnclamped(startSize, focusSize, blend);
                if (detailCard != null) detailCard.alpha = blend;
                yield return null;
            }
            target.anchoredPosition = focusPosition;
            target.sizeDelta = focusSize;
            if (detailCard != null) detailCard.alpha = 1f;
        }

        private void PlaySound(AudioClip clip)
        {
            if (feedbackAudio != null && clip != null)
                feedbackAudio.PlayOneShot(clip);
        }

        private static void Select(UnityEngine.UI.Button button)
            => EventSystem.current?.SetSelectedGameObject(button.gameObject);
    }
}
