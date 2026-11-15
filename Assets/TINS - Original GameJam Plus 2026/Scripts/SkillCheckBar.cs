using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace GraveyardShift.Gameplay
{
    /// <summary>
    /// A self-contained timing skill check. Add this component to a scene object;
    /// its HUD preview is generated in Edit Mode; timing and input are active only in Play Mode.
    /// </summary>
    [ExecuteAlways]
    public sealed class SkillCheckBar : MonoBehaviour
    {
        [Serializable]
        public struct SkillCheckResult
        {
            [Range(0f, 1f)] public float accuracy;
            [Range(0f, 1f)] public float markerPosition;
            [Range(0f, 1f)] public float targetPosition;
            [Range(0f, 1f)] public float timingQuality;
            public bool hitTarget;
            public string rating;
        }

        [Serializable] public sealed class AccuracyEvent : UnityEvent<float> { }
        [Serializable] public sealed class SkillCheckEvent : UnityEvent { }

        [Header("Skill check")]
        [SerializeField, Min(0.1f)] private float travelDuration = 2.5f;
        [SerializeField, Range(0.02f, 0.5f)] private float targetWidth = 0.13f;
        [SerializeField, Range(0f, 1f)] private float perfectAccuracy = 0.995f;
        [SerializeField] private bool startOnAwake = true;
        [SerializeField] private bool restartWithSpaceAfterResult = true;
        [SerializeField] private bool useUnscaledTime = true;

        [Header("Optional UI references (auto-created when empty)")]
        [SerializeField] private RectTransform barTrack;
        [SerializeField] private RectTransform targetZone;
        [SerializeField] private RectTransform marker;
        [SerializeField] private Text resultLabel;
        [SerializeField] private Text promptLabel;

        [Header("Visual style")]
        [SerializeField] private Color panelColor = new Color(0.025f, 0.008f, 0.035f, 0.97f);
        [SerializeField] private Color frameColor = new Color(0.86f, 0.12f, 1f, 1f);
        [SerializeField] private Color frameGlowColor = new Color(0.85f, 0.08f, 1f, 0.22f);
        [SerializeField] private Color trackColor = new Color(0.10f, 0.018f, 0.13f, 1f);
        [SerializeField] private Color targetColor = new Color(0.92f, 0.18f, 1f, 0.72f);
        [SerializeField] private Color markerColor = new Color(1f, 0.48f, 1f, 1f);
        [SerializeField] private Color perfectColor = new Color(1f, 0.60f, 1f, 1f);
        [SerializeField] private Color greatColor = new Color(0.82f, 0.35f, 1f, 1f);
        [SerializeField] private Color missColor = new Color(1f, 0.22f, 0.48f, 1f);

        [Header("Events")]
        [SerializeField] private AccuracyEvent onAccuracyResolved = new AccuracyEvent();
        [SerializeField] private SkillCheckEvent onSuccess = new SkillCheckEvent();
        [SerializeField] private SkillCheckEvent onFailure = new SkillCheckEvent();

        public event Action<SkillCheckResult> ResultResolved;
        public event Action Started;
        public event Action Cancelled;
        public AccuracyEvent OnAccuracyResolved => onAccuracyResolved;
        public SkillCheckEvent OnSuccess => onSuccess;
        public SkillCheckEvent OnFailure => onFailure;

        private Canvas generatedCanvas;
        [SerializeField] private RectTransform layoutRoot;
        private Image trackImage;
        private Image targetImage;
        private Image markerImage;
        [SerializeField, HideInInspector] private float targetPosition = 0.5f;
        [SerializeField, HideInInspector] private bool hasPreviewTarget;
        private float elapsed;
        private float currentMarkerPosition;
        private int startedFrame = -1;
        private bool isRunning;
        private bool hasResolved;
        private bool waitForSpaceRelease;
        private bool useFixedTargetOnNextStart;
        private bool advanceOnce;
        private float requestedTargetPosition = 0.8f;
        private float requestedTravelDuration = 2.5f;
        private PlayerInputHandler lockedPlayerInput;
        private bool movementLockApplied;
        private bool IsExternallyActivated => GetComponent<LightRadiusSkillCheckActivator>() != null;

        public float CurrentTargetPosition => targetPosition;
        public bool IsRunning => isRunning;

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                if (!HasCompleteUI())
                    BuildRuntimeUI(false);

                ApplyVisualStyle();

                if (IsExternallyActivated)
                    HideUntilStarted();
                else if (startOnAwake)
                    BeginSkillCheck();
                return;
            }

            if (!HasCompleteUI())
                BuildRuntimeUI(true);

            ApplyVisualStyle();
        }
#else
        private void OnEnable()
        {
            if (!HasCompleteUI())
                BuildRuntimeUI(false);

            ApplyVisualStyle();

            if (IsExternallyActivated)
                HideUntilStarted();
            else if (startOnAwake)
                BeginSkillCheck();
        }
#endif

        private void Awake()
        {
            if (!Application.isPlaying)
                return;

            if (!HasCompleteUI())
                BuildRuntimeUI(false);

            ApplyVisualStyle();

        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
                return;

            HideUntilStarted();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (onAccuracyResolved == null)
                onAccuracyResolved = new AccuracyEvent();

            ApplyVisualStyle(false);
            PlaceTargetZone();
        }

        private void UpdateMarker(float normalizedPosition)
        {
            if (marker == null || barTrack == null)
                return;

            float halfWidth = barTrack.rect.width * 0.5f;
            Vector2 position = marker.anchoredPosition;
            position.x = Mathf.Lerp(-halfWidth, halfWidth, Mathf.Clamp01(normalizedPosition));
            marker.anchoredPosition = position;
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;

            if (!advanceOnce && isRunning && !movementLockApplied)
                SetPlayerMovementLock(true);

            // Do not let the Space press that activated the tower also resolve the new check.
            Keyboard keyboard = Keyboard.current;
            if (waitForSpaceRelease && (keyboard == null || !keyboard.spaceKey.isPressed))
                waitForSpaceRelease = false;

            // The tower activator owns both starting and submitting its checks.
            // Reading Space here too allowed one press to resolve and restart.
            if (!advanceOnce && !IsExternallyActivated && Time.frameCount > startedFrame && !waitForSpaceRelease &&
                keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            {
                if (isRunning)
                    ResolveSkillCheck();
                else if (hasResolved && restartWithSpaceAfterResult && !IsExternallyActivated)
                    BeginSkillCheck();
            }

            if (!isRunning)
                return;

            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float rawProgress = elapsed / Mathf.Max(0.1f, travelDuration);
            float progress = advanceOnce ? Mathf.Clamp01(rawProgress) : Mathf.PingPong(rawProgress, 1f);
            currentMarkerPosition = progress;
            UpdateMarker(currentMarkerPosition);
        }

        public void BeginSkillCheck()
        {
            if (isRunning)
                return;

            if (!HasCompleteUI())
                BuildRuntimeUI(false);

            if (layoutRoot != null)
                layoutRoot.gameObject.SetActive(true);
            if (barTrack != null)
                barTrack.gameObject.SetActive(true);
            if (resultLabel != null)
                resultLabel.gameObject.SetActive(true);
            if (promptLabel != null)
                promptLabel.gameObject.SetActive(true);

            elapsed = 0f;
            currentMarkerPosition = 0f;
            startedFrame = Time.frameCount;
            isRunning = true;
            hasResolved = false;
            waitForSpaceRelease = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            if (!advanceOnce) SetPlayerMovementLock(true);
            targetPosition = useFixedTargetOnNextStart
                ? Mathf.Clamp(requestedTargetPosition, targetWidth * 0.5f, 1f - targetWidth * 0.5f)
                : UnityEngine.Random.Range(targetWidth * 0.5f, 1f - targetWidth * 0.5f);
            if (useFixedTargetOnNextStart)
                travelDuration = Mathf.Max(0.1f, requestedTravelDuration);
            useFixedTargetOnNextStart = false;
            PlaceTargetZone();
            UpdateMarker(0f);

            if (resultLabel != null)
            {
                resultLabel.text = string.Empty;
                resultLabel.color = greatColor;
            }

            if (promptLabel != null)
                promptLabel.text = IsExternallyActivated ? "USE O FLASH   ·   ACERTE A MARCA" : "SPACE   HIT TARGET ZONE";

            Started?.Invoke();
        }

        public void ResolveSkillCheck()
        {
            if (!isRunning)
                return;

            float markerPosition = currentMarkerPosition;
            float distance = Mathf.Abs(markerPosition - targetPosition);
            float accuracy = Mathf.Clamp01(1f - distance);
            bool hitTarget = distance <= targetWidth * 0.5f;
            float timingQuality = hitTarget
                ? Mathf.Clamp01(1f - distance / Mathf.Max(0.0001f, targetWidth * 0.5f))
                : 0f;
            string rating = hitTarget
                ? (accuracy >= perfectAccuracy ? "PERFECT!" : "GREAT!")
                : "MISS!";

            isRunning = false;
            hasResolved = true;
            SetPlayerMovementLock(false);
            UpdateMarker(markerPosition);

            Color feedbackColor = rating == "PERFECT!" ? perfectColor : rating == "GREAT!" ? greatColor : missColor;
            if (resultLabel != null)
            {
                resultLabel.text = rating;
                resultLabel.color = feedbackColor;
            }

            if (promptLabel != null)
                promptLabel.text = IsExternallyActivated
                    ? string.Empty
                    : restartWithSpaceAfterResult ? "SPACE   TRY AGAIN" : "SKILL CHECK COMPLETE";

            SkillCheckResult result = new SkillCheckResult
            {
                accuracy = accuracy,
                markerPosition = markerPosition,
                targetPosition = targetPosition,
                timingQuality = timingQuality,
                hitTarget = hitTarget,
                rating = rating
            };

            if (hitTarget)
                onSuccess?.Invoke();
            else
                onFailure?.Invoke();

            onAccuracyResolved?.Invoke(accuracy);
            ResultResolved?.Invoke(result);
        }

        public void BeginSkillCheckAt(float normalizedTargetPosition, float duration, float initialProgress = 0f)
        {
            requestedTargetPosition = Mathf.Clamp01(normalizedTargetPosition);
            requestedTravelDuration = Mathf.Max(0.1f, duration);
            useFixedTargetOnNextStart = true;
            advanceOnce = true;
            BeginSkillCheck();
            elapsed = Mathf.Clamp01(initialProgress) * travelDuration;
            currentMarkerPosition = Mathf.Clamp01(initialProgress);
            UpdateMarker(currentMarkerPosition);
        }

        public void CancelSkillCheck() => HideUntilStarted();

        public void HideUntilStarted()
        {
            // The tower interaction is only allowed to run while the player is
            // inside its light. Leaving/re-entering must never reveal an old result.
            bool wasRunning = isRunning;
            isRunning = false;
            SetPlayerMovementLock(false);

            elapsed = 0f;
            currentMarkerPosition = 0f;
            hasResolved = false;
            advanceOnce = false;
            Keyboard keyboard = Keyboard.current;
            waitForSpaceRelease = keyboard != null && keyboard.spaceKey.isPressed;

            if (resultLabel != null)
                resultLabel.text = string.Empty;
            if (promptLabel != null)
                promptLabel.text = string.Empty;

            if (layoutRoot != null)
                layoutRoot.gameObject.SetActive(false);

            if (wasRunning)
                Cancelled?.Invoke();
        }

        private void SetPlayerMovementLock(bool locked)
        {
            if (!locked)
            {
                if (lockedPlayerInput != null)
                    lockedPlayerInput.SetSkillCheckMovementLocked(false);

                lockedPlayerInput = null;
                movementLockApplied = false;
                return;
            }

            if (movementLockApplied)
                return;

            PlayerInputHandler[] players = FindObjectsByType<PlayerInputHandler>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null || players[i].GetIsClone())
                    continue;

                lockedPlayerInput = players[i];
                lockedPlayerInput.SetSkillCheckMovementLocked(true);
                movementLockApplied = true;
                return;
            }
        }

        private bool HasCompleteUI()
        {
            return barTrack != null && targetZone != null && marker != null;
        }

        private void PlaceTargetZone()
        {
            if (barTrack == null || targetZone == null)
                return;

            float width = barTrack.rect.width * targetWidth;
            float centerX = Mathf.Lerp(-barTrack.rect.width * 0.5f, barTrack.rect.width * 0.5f, targetPosition);
            targetZone.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            targetZone.anchoredPosition = new Vector2(centerX, targetZone.anchoredPosition.y);
        }

        private void BuildRuntimeUI(bool preview = false)
        {
            if (HasCompleteUI()) return;
#if UNITY_EDITOR
            if (preview && layoutRoot != null)
            {
                Undo.DestroyObjectImmediate(layoutRoot.gameObject);
                layoutRoot = null;
                barTrack = null;
                targetZone = null;
                marker = null;
                resultLabel = null;
                promptLabel = null;
            }
#endif
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            Transform uiParent = parentCanvas != null ? parentCanvas.transform : null;

            if (uiParent == null)
            {
                if (preview)
                    return;

                GameObject canvasObject = new GameObject("Skill Check HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                generatedCanvas = canvasObject.GetComponent<Canvas>();
                generatedCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
                uiParent = canvasObject.transform;
            }

            layoutRoot = CreateRect("Skill Check Layout", uiParent);
            RectTransform root = layoutRoot;
            SetAnchors(root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            root.sizeDelta = new Vector2(900f, 230f);

            resultLabel = CreateText("Result", root, 38, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetAnchors(resultLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            resultLabel.rectTransform.anchoredPosition = new Vector2(0f, -5f);
            resultLabel.rectTransform.sizeDelta = new Vector2(500f, 58f);
            resultLabel.color = perfectColor;
            resultLabel.text = "";

            barTrack = CreateRect("Timing Bar", root);
            SetAnchors(barTrack, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            barTrack.anchoredPosition = new Vector2(0f, -5f);
            barTrack.sizeDelta = new Vector2(880f, 48f);
            trackImage = barTrack.gameObject.AddComponent<Image>();
            trackImage.color = trackColor;
            trackImage.raycastTarget = false;

            AddGridLines(barTrack, 24);

            targetZone = CreateRect("Random Target Zone", barTrack);
            SetAnchors(targetZone, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            targetZone.sizeDelta = new Vector2(100f, 42f);
            targetImage = targetZone.gameObject.AddComponent<Image>();
            targetImage.color = targetColor;
            targetImage.raycastTarget = false;

            RectTransform glow = CreateRect("Target Glow", targetZone);
            SetAnchors(glow, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            glow.offsetMin = new Vector2(-12f, -5f);
            glow.offsetMax = new Vector2(12f, 5f);
            Image glowImage = glow.gameObject.AddComponent<Image>();
            glowImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0.15f);
            glowImage.raycastTarget = false;
            glow.SetAsFirstSibling();

            marker = CreateRect("Moving Marker", barTrack);
            SetAnchors(marker, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            marker.sizeDelta = new Vector2(5f, 76f);
            markerImage = marker.gameObject.AddComponent<Image>();
            markerImage.color = markerColor;
            markerImage.raycastTarget = false;

            RectTransform markerGlow = CreateRect("Marker Glow", marker);
            SetAnchors(markerGlow, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            markerGlow.offsetMin = new Vector2(-7f, -5f);
            markerGlow.offsetMax = new Vector2(7f, 5f);
            Image markerGlowImage = markerGlow.gameObject.AddComponent<Image>();
            markerGlowImage.color = new Color(markerColor.r, markerColor.g, markerColor.b, 0.18f);
            markerGlowImage.raycastTarget = false;
            markerGlow.SetAsFirstSibling();

            promptLabel = CreateText("Instruction", root, 27, FontStyle.Normal, TextAnchor.MiddleCenter);
            SetAnchors(promptLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            promptLabel.rectTransform.anchoredPosition = new Vector2(0f, 11f);
            promptLabel.rectTransform.sizeDelta = new Vector2(520f, 58f);
            promptLabel.color = new Color(0.96f, 0.82f, 1f, 1f);
            promptLabel.text = "SPACE   HIT TARGET ZONE";

            targetPosition = UnityEngine.Random.Range(targetWidth * 0.5f, 1f - targetWidth * 0.5f);
            hasPreviewTarget = true;
            PlaceTargetZone();
            ApplyVisualStyle();
#if UNITY_EDITOR
            if (preview)
            {
                Undo.RegisterCreatedObjectUndo(layoutRoot.gameObject, "Generate Skill Check HUD");
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
#endif

            // Keep result and prompt readable over busy backgrounds without introducing image assets.
            SetShadow(resultLabel, new Color(perfectColor.r, perfectColor.g, perfectColor.b, 0.65f));
            SetShadow(promptLabel, new Color(0f, 0f, 0f, 0.9f));
        }

        private static void AddGridLines(RectTransform parent, int count)
        {
            for (int i = 1; i < count; i++)
            {
                RectTransform line = CreateRect("Grid Line", parent);
                float x = (float)i / count;
                SetAnchors(line, new Vector2(x, 0f), new Vector2(x, 1f), new Vector2(0.5f, 0.5f));
                line.sizeDelta = new Vector2(1f, 0f);
                Image image = line.gameObject.AddComponent<Image>();
                image.color = new Color(0.82f, 0.2f, 0.94f, 0.16f);
                image.raycastTarget = false;
            }
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject child = new GameObject(objectName, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }

        private static Text CreateText(string objectName, Transform parent, int fontSize, FontStyle style, TextAnchor alignment)
        {
            RectTransform rect = CreateRect(objectName, parent);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = pivot;
        }

        private static void SetShadow(Graphic graphic, Color color)
        {
            UnityEngine.UI.Shadow shadow = graphic.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = new Vector2(0f, -2f);
        }

        private void ApplyVisualStyle(bool createFrameIfMissing = true)
        {
            if (layoutRoot != null)
            {
                if (createFrameIfMissing)
                {
                    EnsurePixelGothicFrame();
                }
                else
                {
                    UnityEngine.UI.Image panel = layoutRoot.GetComponent<UnityEngine.UI.Image>();
                    if (panel != null)
                    {
                        panel.color = panelColor;
                        panel.raycastTarget = false;
                    }

                    Transform frame = layoutRoot.Find("Pixel Gothic Frame");
                    if (frame != null)
                        RefreshFrameColors(frame);
                }
            }

            Font pixelFont = Resources.Load<Font>("SkillCheckPixelFont");

            if (resultLabel != null)
            {
                if (pixelFont != null) resultLabel.font = pixelFont;
                resultLabel.fontSize = 34;
                resultLabel.fontStyle = FontStyle.Bold;
                resultLabel.color = resultLabel.text == "MISS!"
                    ? missColor
                    : resultLabel.text == "GREAT!" ? greatColor : perfectColor;
            }

            if (promptLabel != null)
            {
                if (pixelFont != null) promptLabel.font = pixelFont;
                promptLabel.fontSize = 23;
                promptLabel.color = new Color(0.96f, 0.82f, 1f, 1f);
            }

            if (barTrack != null)
            {
                trackImage = barTrack.GetComponent<Image>();
                if (trackImage != null) trackImage.color = trackColor;
            }

            if (targetZone != null)
            {
                targetImage = targetZone.GetComponent<Image>();
                if (targetImage != null) targetImage.color = targetColor;

                Transform glow = targetZone.Find("Target Glow");
                Image glowImage = glow != null ? glow.GetComponent<Image>() : null;
                if (glowImage != null)
                    glowImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0.2f);
            }

            if (marker != null)
            {
                markerImage = marker.GetComponent<Image>();
                if (markerImage != null) markerImage.color = markerColor;

                Transform glow = marker.Find("Marker Glow");
                Image glowImage = glow != null ? glow.GetComponent<Image>() : null;
                if (glowImage != null)
                    glowImage.color = new Color(markerColor.r, markerColor.g, markerColor.b, 0.24f);
            }
        }

        private void EnsurePixelGothicFrame()
        {
            UnityEngine.UI.Image panel = layoutRoot.GetComponent<UnityEngine.UI.Image>();
            if (panel == null)
                panel = layoutRoot.gameObject.AddComponent<UnityEngine.UI.Image>();
            panel.color = panelColor;
            panel.raycastTarget = false;

            Transform existingFrame = layoutRoot.Find("Pixel Gothic Frame");
            if (existingFrame != null)
            {
                existingFrame.SetAsFirstSibling();
                RefreshFrameColors(existingFrame);
                return;
            }

            RectTransform frame = CreateRect("Pixel Gothic Frame", layoutRoot);
            SetAnchors(frame, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            frame.offsetMin = Vector2.zero;
            frame.offsetMax = Vector2.zero;

            // Soft underlay plus crisp pixel edges gives the reference's neon frame
            // without requiring a sliced image or changing the existing HUD assets.
            AddFrameEdge(frame, "Glow Top", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(884f, 10f), frameGlowColor);
            AddFrameEdge(frame, "Glow Bottom", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(884f, 10f), frameGlowColor);
            AddFrameEdge(frame, "Glow Left", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(10f, 204f), frameGlowColor);
            AddFrameEdge(frame, "Glow Right", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-6f, 0f), new Vector2(10f, 204f), frameGlowColor);

            AddFrameEdge(frame, "Top Edge", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(876f, 3f), frameColor);
            AddFrameEdge(frame, "Bottom Edge", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(876f, 3f), frameColor);
            AddFrameEdge(frame, "Left Edge", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(3f, 204f), frameColor);
            AddFrameEdge(frame, "Right Edge", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-8f, 0f), new Vector2(3f, 204f), frameColor);

            AddPixelCorner(frame, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -18f), false, false);
            AddPixelCorner(frame, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -18f), true, false);
            AddPixelCorner(frame, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(18f, 18f), false, true);
            AddPixelCorner(frame, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 18f), true, true);

            RectTransform crest = CreateRect("Center Crest", frame);
            SetAnchors(crest, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f));
            crest.anchoredPosition = new Vector2(0f, -8f);
            crest.sizeDelta = new Vector2(10f, 10f);
            crest.localRotation = Quaternion.Euler(0f, 0f, 45f);
            UnityEngine.UI.Image crestImage = crest.gameObject.AddComponent<UnityEngine.UI.Image>();
            crestImage.color = perfectColor;
            crestImage.raycastTarget = false;

            frame.SetAsFirstSibling();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RegisterCreatedObjectUndo(frame.gameObject, "Create Skill Check Pixel Frame");
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
#endif
        }

        private void RefreshFrameColors(Transform frame)
        {
            UnityEngine.UI.Image[] images = frame.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i].name.StartsWith("Glow", StringComparison.Ordinal))
                    images[i].color = frameGlowColor;
                else if (images[i].name == "Center Crest")
                    images[i].color = perfectColor;
                else if (images[i].name == "Corner Pixel")
                    images[i].color = new Color(0.7f, 0.16f, 0.96f, 1f);
                else
                    images[i].color = frameColor;
            }
        }

        private static void AddPixelCorner(RectTransform parent, Vector2 anchor, Vector2 pivot, Vector2 position, bool right, bool bottom)
        {
            Vector2 horizontalPosition = position + new Vector2(right ? -2f : 2f, bottom ? -2f : 2f);
            Vector2 verticalPosition = horizontalPosition;
            AddFrameEdge(parent, "Corner Horizontal", anchor, pivot, horizontalPosition, new Vector2(30f, 3f), new Color(1f, 0.38f, 1f, 1f));
            AddFrameEdge(parent, "Corner Vertical", anchor, pivot, verticalPosition, new Vector2(3f, 30f), new Color(1f, 0.38f, 1f, 1f));

            Vector2 stepPosition = position + new Vector2(right ? -36f : 36f, bottom ? -36f : 36f);
            AddFrameEdge(parent, "Corner Pixel", anchor, pivot, stepPosition, new Vector2(5f, 5f), new Color(0.7f, 0.16f, 0.96f, 1f));
        }

        private static void AddFrameEdge(RectTransform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Color color)
        {
            RectTransform edge = CreateRect(name, parent);
            SetAnchors(edge, anchor, anchor, pivot);
            edge.anchoredPosition = position;
            edge.sizeDelta = size;
            UnityEngine.UI.Image image = edge.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = false;
        }
        private void OnRectTransformDimensionsChange()
        {
            if (barTrack == null)
                return;

            PlaceTargetZone();
            UpdateMarker(isRunning ? currentMarkerPosition : 0f);
        }

        private void OnDestroy()
        {
            if (generatedCanvas != null)
                Destroy(generatedCanvas.gameObject);
        }
    }
}
