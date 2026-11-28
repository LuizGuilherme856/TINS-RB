using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TINS.Campaign
{
    [DefaultExecutionOrder(10000)]
    internal sealed class CampaignMissionVignette : MonoBehaviour
    {
        private const string ResourceFolder = "Mission Travel/";
        private const string LoadingText = "Loading";
        private const string LoadingDots = "...";

        private static CampaignMissionVignette active;
        private Image matte;
        private float resumeTimeScale;
        private ThreadPriority previousLoadingPriority;
        private AsyncOperation sceneLoad;

        // Each visit to the Hub starts these requests before the player picks a destination.
        internal sealed class FlightAssets
        {
            internal readonly ResourceRequest backdrop = Resources.LoadAsync<Texture2D>(ResourceFolder + "espaco");
            internal readonly ResourceRequest flightAtlas = Resources.LoadAsync<Texture2D>(ResourceFolder + "voo-spritesheet");
            internal readonly ResourceRequest planetAtlas = Resources.LoadAsync<Texture2D>(ResourceFolder + "planetas-estaticos-atlas");
            internal readonly ResourceRequest sequenceFile = Resources.LoadAsync<TextAsset>(ResourceFolder + "viagem");
            internal readonly ResourceRequest planetsFile = Resources.LoadAsync<TextAsset>(ResourceFolder + "planetas-estaticos");

            internal bool IsReady => backdrop.isDone && flightAtlas.isDone && planetAtlas.isDone &&
                                     sequenceFile.isDone && planetsFile.isDone;
        }

        internal static bool IsLoading => active != null;

        internal static void LoadMission(FlightAssets assets, string planetId, int purificationStage, TMP_FontAsset font, Action onLoadFailed)
        {
            var transition = CreateOverlay(font);
            transition.resumeTimeScale = Time.timeScale;
            transition.previousLoadingPriority = Application.backgroundLoadingPriority;
            active = transition;
            DontDestroyOnLoad(transition.gameObject);
            Time.timeScale = 0f;
            transition.StartCoroutine(transition.Load(assets, planetId, purificationStage, onLoadFailed));
        }

        private IEnumerator Load(FlightAssets assets, string planetId, int purificationStage, Action onLoadFailed)
        {
            while (!assets.IsReady) yield return null;
            Coroutine flight = StartCoroutine(Play(assets, planetId, purificationStage));
            yield return null;

            Application.backgroundLoadingPriority = ThreadPriority.Low;
            try
            {
                sceneLoad = SceneManager.LoadSceneAsync(CampaignTravel.MissionScene);
                if (sceneLoad != null) sceneLoad.allowSceneActivation = false;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }

            if (sceneLoad == null)
            {
                onLoadFailed();
                Destroy(gameObject);
                yield break;
            }

            yield return flight;
            // Awake/Start and teardown can block the main thread. Keep them behind the final opaque frame.
            Application.backgroundLoadingPriority = previousLoadingPriority;
            sceneLoad.allowSceneActivation = true;
            yield return sceneLoad;
            // Give the destination scene a frame to finish its Start callbacks behind the overlay.
            yield return null;

            if (resumeTimeScale == 0f) resumeTimeScale = 1f;
            Destroy(gameObject);
        }

        private void LateUpdate()
        {
            // A scene's time manager can change the scale during startup.
            if (Time.timeScale != 0f) Time.timeScale = 0f;
        }

        private void OnDestroy()
        {
            if (active != this) return;
            active = null;
            if (sceneLoad != null && !sceneLoad.isDone) sceneLoad.allowSceneActivation = true;
            Application.backgroundLoadingPriority = previousLoadingPriority;
            Time.timeScale = resumeTimeScale;
        }

        private IEnumerator Play(FlightAssets assets, string planetId, int purificationStage)
        {
            var backdrop = assets.backdrop.asset as Texture2D;
            var flightAtlas = assets.flightAtlas.asset as Texture2D;
            var planetAtlas = assets.planetAtlas.asset as Texture2D;
            var sequenceFile = assets.sequenceFile.asset as TextAsset;
            var planetsFile = assets.planetsFile.asset as TextAsset;
            if (backdrop == null || flightAtlas == null || planetAtlas == null || sequenceFile == null || planetsFile == null)
            {
                Debug.LogWarning("Os arquivos da vinheta de viagem estão incompletos.");
                yield break;
            }

            Sequence sequence = JsonUtility.FromJson<Sequence>(sequenceFile.text);
            PlanetAtlas planets = JsonUtility.FromJson<PlanetAtlas>(planetsFile.text);
            string lightingId = LightingFor(purificationStage);
            PlanetCell cell = Array.Find(planets?.entries ?? Array.Empty<PlanetCell>(),
                entry => entry.planet_id == planetId && entry.lighting_id == lightingId);
            FlightFrame[] frames = sequence?.voo.frames;
            if (cell.width <= 0 || cell.height <= 0 || frames == null || frames.Length == 0 ||
                sequence.camera_size_px?.Length != 2 || sequence.planeta.screen_origin_px?.Length != 2 ||
                sequence.planeta.display_size_px?.Length != 2 || sequence.fade.color_rgb?.Length != 3 ||
                sequence.timeline.frame_duration_ms <= 0 || sequence.fade.alpha_by_frame?.Length != frames.Length)
            {
                Debug.LogWarning($"A vinheta de viagem não tem uma sequência válida para {planetId}.");
                yield break;
            }

            Vector2 size = PixelPair(sequence.camera_size_px);
            int[] fadeRgb = sequence.fade.color_rgb;
            var fadeColor = new Color32((byte)fadeRgb[0], (byte)fadeRgb[1], (byte)fadeRgb[2], 255);
            GetComponent<CanvasScaler>().referenceResolution = size;
            matte.color = fadeColor;

            var shot = new GameObject("Travel Frame", typeof(RectTransform), typeof(RectMask2D)).GetComponent<RectTransform>();
            shot.SetParent(transform, false);
            shot.SetSiblingIndex(1);
            shot.anchorMin = shot.anchorMax = shot.pivot = Vector2.one * .5f;
            shot.sizeDelta = size;

            RawImage stars = AddLayer<RawImage>(shot, "Space");
            Stretch(stars.rectTransform);
            stars.texture = backdrop;

            RawImage planet = AddLayer<RawImage>(shot, "Destination Planet");
            planet.texture = planetAtlas;
            planet.uvRect = AtlasRegion(planetAtlas, cell.x, cell.y, cell.width, cell.height);
            Place(planet.rectTransform, PixelPair(sequence.planeta.screen_origin_px), PixelPair(sequence.planeta.display_size_px));

            RawImage flight = AddLayer<RawImage>(shot, "Couracada Flight");
            flight.texture = flightAtlas;
            Image fade = AddLayer<Image>(shot, "Fade");
            Stretch(fade.rectTransform);

            double secondsPerFrame = sequence.timeline.frame_duration_ms / 1000d;
            double duration = frames.Length * secondsPerFrame;
            double elapsed = 0d;
            int displayed = -1;

            while (elapsed < duration)
            {
                int index = Math.Min(frames.Length - 1, (int)(elapsed / secondsPerFrame));
                if (index != displayed) DrawFrame(index);
                yield return null;
                // A long engine frame must not skip the opening fade or any of the ship's poses.
                elapsed += Math.Min(Time.unscaledDeltaTime, secondsPerFrame);
            }

            void DrawFrame(int index)
            {
                FlightFrame frame = frames[index];
                int[] rect = frame.rect_px;
                flight.uvRect = AtlasRegion(flightAtlas, rect[0], rect[1], rect[2], rect[3]);
                Place(flight.rectTransform, PixelPair(frame.screen_origin_px), new Vector2(rect[2], rect[3]));
                fadeColor.a = (byte)sequence.fade.alpha_by_frame[index];
                fade.color = fadeColor;
                displayed = index;
            }
        }

        private static CampaignMissionVignette CreateOverlay(TMP_FontAsset font)
        {
            var overlay = new GameObject("Mission Travel", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(CampaignMissionVignette));
            var transition = overlay.GetComponent<CampaignMissionVignette>();
            var canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            canvas.pixelPerfect = true;
            var scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            Image matte = AddLayer<Image>(overlay.transform, "Matte");
            Stretch(matte.rectTransform);
            matte.color = Color.black;
            transition.matte = matte;
            matte.raycastTarget = true;

            var label = AddLayer<TextMeshProUGUI>(overlay.transform, "Loading Label");
            if (font != null) label.font = font;
            label.text = LoadingText + LoadingDots;
            label.fontSize = 30f;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.MidlineRight;

            RectTransform rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.right;
            rect.anchoredPosition = new Vector2(-32f, 24f);
            rect.sizeDelta = new Vector2(160f, 48f);
            label.StartCoroutine(AnimateLoadingDots(label));
            return transition;
        }

        private static IEnumerator AnimateLoadingDots(TMP_Text label)
        {
            var interval = new WaitForSecondsRealtime(0.3f);
            int dots = 0;
            while (true)
            {
                label.maxVisibleCharacters = LoadingText.Length + dots;
                yield return interval;
                dots = (dots + 1) % (LoadingDots.Length + 1);
            }
        }

        private static string LightingFor(int stage) => stage switch
        {
            <= 0 => "escuro",
            <= 2 => "luz-25",
            3 => "luz-75",
            _ => "iluminado"
        };

        private static T AddLayer<T>(Transform parent, string name) where T : Graphic
        {
            var layer = new GameObject(name, typeof(RectTransform), typeof(T)).GetComponent<T>();
            layer.transform.SetParent(parent, false);
            layer.raycastTarget = false;
            return layer;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rect, Vector2 topLeft, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.up;
            rect.anchoredPosition = new Vector2(topLeft.x, -topLeft.y);
            rect.sizeDelta = size;
        }

        private static Vector2 PixelPair(int[] values) => new(values[0], values[1]);

        private static Rect AtlasRegion(Texture2D atlas, int x, int y, int width, int height) => new(
            (float)x / atlas.width, (float)(atlas.height - y - height) / atlas.height,
            (float)width / atlas.width, (float)height / atlas.height);

        [Serializable] private sealed class Sequence
        {
            public int[] camera_size_px;
            public PlanetPlacement planeta;
            public Flight voo;
            public Timeline timeline;
            public Fade fade;
        }

        [Serializable] private struct PlanetPlacement
        {
            public int[] screen_origin_px;
            public int[] display_size_px;
        }

        [Serializable] private struct Flight
        {
            public FlightFrame[] frames;
        }

        [Serializable] private struct FlightFrame
        {
            public int[] rect_px;
            public int[] screen_origin_px;
        }

        [Serializable] private struct Timeline
        {
            public int frame_duration_ms;
        }

        [Serializable] private struct Fade
        {
            public int[] color_rgb;
            public int[] alpha_by_frame;
        }

        [Serializable] private sealed class PlanetAtlas
        {
            public PlanetCell[] entries;
        }

        [Serializable] private struct PlanetCell
        {
            public string planet_id;
            public string lighting_id;
            public int x;
            public int y;
            public int width;
            public int height;
        }
    }
}
