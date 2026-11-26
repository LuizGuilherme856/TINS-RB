using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Shakesprite
{
    public sealed class ShakespriteWindow : EditorWindow
    {
        private const string MenuPath = "Window/2D/Shakesprite";
        private const string DefaultStatus = "Middle-drag to pan.";
        private static readonly string[] LayoutNames = { "Imported frames", "Whole image", "Grid" };
        private static readonly string[] ZoomNames = { "Fit", "1×", "2×", "4×", "8×", "16×", "32×", "64×" };

        [SerializeField] private Object source;
        [SerializeField] private PixelDocument document;
        [SerializeField] private PixelFrame[] importedFrames;
        [SerializeField] private string pngGuid;
        [SerializeField] private string pngHash;
        [SerializeField] private Color color = new(1f, 0.40f, 0.30f, 1f);
        [SerializeField] private int frameIndex;
        [SerializeField] private int zoom;
        [SerializeField] private int layoutMode;
        [SerializeField] private int columns = 1;
        [SerializeField] private int rows = 1;
        [SerializeField] private int fps = 12;
        [SerializeField] private bool erasing;
        [SerializeField] private byte[] reloadPng;

        private PixelFrame[] frames = Array.Empty<PixelFrame>();
        private string status = DefaultStatus;
        private bool error;
        private bool playing;
        private double playStarted;
        private double playOffset;
        private bool fromClip;
        private Texture2D checker;
        private PixelCanvas canvas;
        private ObjectField sourceField;
        private VisualElement loaded;
        private VisualElement empty;
        private VisualElement gridFields;
        private Button pencilButton;
        private Button eraserButton;
        private Button saveButton;
        private Button copyButton;
        private Button undoButton;
        private Button redoButton;
        private Button playButton;
        private Button previousButton;
        private Button nextButton;
        private IntegerField fpsField;
        private Label clipTiming;
        private SliderInt frameSlider;
        private Label frameLabel;
        private Label statusLabel;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            GetWindow<ShakespriteWindow>().Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Shakesprite");
            minSize = new Vector2(410, 370);
            saveChangesMessage = "Save your Shakesprite pixel edits before closing?";
            EditorApplication.update += UpdatePlayback;
            AssemblyReloadEvents.beforeAssemblyReload += PreserveEdits;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdatePlayback;
            AssemblyReloadEvents.beforeAssemblyReload -= PreserveEdits;
        }

        private void OnDestroy()
        {
            document?.Dispose();
            if (checker)
                DestroyImmediate(checker);
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();
            string script = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.GetDirectoryName(script).Replace('\\', '/') + "/Shakesprite.uss");
            if (style)
                root.styleSheets.Add(style);

            root.AddToClassList("shakesprite");
            root.EnableInClassList("shakesprite--dark", EditorGUIUtility.isProSkin);
            RestoreEdits();

            var sourceRow = AddRow(root, "asset-bar");
            sourceField = new ObjectField { name = "source", objectType = typeof(Object), allowSceneObjects = false };
            sourceField.AddToClassList("grow");
            sourceField.tooltip = "Drop a PNG, a Sprite, or a sprite AnimationClip from the Project window.";
            sourceField.SetValueWithoutNotify(source);
            sourceField.RegisterValueChangedCallback(evt =>
            {
                OpenAsset(evt.newValue);
                sourceField.SetValueWithoutNotify(source);
            });
            sourceRow.Add(sourceField);
            AddButton(sourceRow, "Use Selection", "use-selection", () => OpenAsset(Selection.activeObject),
                "Open the selected asset from the Project window.");

            empty = new VisualElement();
            empty.AddToClassList("empty-panel");
            var emptyTitle = new Label("Open a sprite or sprite sheet.");
            emptyTitle.AddToClassList("empty-title");
            empty.Add(emptyTitle);
            var emptyHint = new Label("Drag a PNG into the asset field or select one in the Project window.");
            emptyHint.AddToClassList("empty-hint");
            empty.Add(emptyHint);
            root.Add(empty);

            loaded = new VisualElement();
            loaded.AddToClassList("workspace");
            root.Add(loaded);
            var actions = AddRow(loaded, "edit-bar");
            pencilButton = AddButton(actions, "Pencil", "pencil", () => SetTool(false));
            eraserButton = AddButton(actions, "Eraser", "eraser", () => SetTool(true));
            var ink = new ColorField { name = "ink", value = color, showAlpha = true, hdr = false, tooltip = "Pencil color and opacity." };
            ink.AddToClassList("ink");
            ink.RegisterValueChangedCallback(evt =>
            {
                color = evt.newValue;
                SetTool(false);
            });
            actions.Add(ink);
            actions.Add(new VisualElement { style = { flexGrow = 1 } });
            undoButton = AddButton(actions, "Undo", "undo", Undo);
            redoButton = AddButton(actions, "Redo", "redo", Redo);
            var zoomField = new PopupField<string>(new System.Collections.Generic.List<string>(ZoomNames), zoom);
            zoomField.name = "zoom";
            zoomField.tooltip = "Preview zoom. Middle-drag the canvas to pan.";
            zoomField.AddToClassList("zoom");
            zoomField.RegisterValueChangedCallback(_ =>
            {
                zoom = zoomField.index;
                canvas.Zoom = ZoomFactor;
                canvas.ResetPan();
            });
            actions.Add(zoomField);

            if (!checker)
                checker = MakeChecker();
            canvas = new PixelCanvas(this, checker) { Zoom = ZoomFactor, PaintColor = BrushColor };
            canvas.StrokeStarted = StopPlayback;
            canvas.StrokeChanged = RefreshActions;
            canvas.StrokeEnded = RefreshActions;
            loaded.Add(canvas);

            var timeline = AddRow(loaded, "timeline");
            previousButton = AddButton(timeline, "‹", "previous", () => Step(-1), "Previous frame");
            playButton = AddButton(timeline, "Play", "play", TogglePlayback, "Play the animation in a loop.");
            nextButton = AddButton(timeline, "›", "next", () => Step(1), "Next frame");
            frameSlider = new SliderInt(1, 2) { name = "frame", tooltip = "Scrub through frames." };
            frameSlider.AddToClassList("grow");
            frameSlider.RegisterValueChangedCallback(evt =>
            {
                StopPlayback();
                canvas.Finish();
                ShowFrame(evt.newValue - 1);
                RefreshActions();
            });
            timeline.Add(frameSlider);
            frameLabel = new Label { name = "frame-label" };
            frameLabel.AddToClassList("frame-label");
            timeline.Add(frameLabel);

            var layoutRow = AddRow(loaded, "layout-bar");
            var layout = new PopupField<string>(new System.Collections.Generic.List<string>(LayoutNames), layoutMode);
            layout.name = "layout";
            layout.tooltip = "Choose imported slices, the full image, or an evenly spaced grid.";
            layout.RegisterValueChangedCallback(_ =>
            {
                layoutMode = layout.index;
                UpdateFrames(0);
            });
            layoutRow.Add(layout);
            gridFields = new VisualElement();
            gridFields.AddToClassList("row");
            AddNumber(gridFields, "Columns", "columns", columns, value =>
            {
                columns = Mathf.Clamp(value, 1, Mathf.Min(document.Texture.width, 64));
                UpdateFrames(0);
                return columns;
            });
            AddNumber(gridFields, "Rows", "rows", rows, value =>
            {
                rows = Mathf.Clamp(value, 1, Mathf.Min(document.Texture.height, 64));
                UpdateFrames(0);
                return rows;
            });
            layoutRow.Add(gridFields);
            fpsField = AddNumber(layoutRow, "FPS", "fps", fps, value =>
            {
                StopPlayback();
                fps = Mathf.Clamp(value, 1, 120);
                return fps;
            });

            clipTiming = new Label("Clip timing") { tooltip = "Uses the timing from the selected AnimationClip." };
            clipTiming.AddToClassList("clip-timing");
            layoutRow.Add(clipTiming);

            var footer = AddRow(root, "footer");
            statusLabel = new Label { name = "status" };
            statusLabel.AddToClassList("status");
            footer.Add(statusLabel);
            copyButton = AddButton(footer, "Save Copy…", "save-copy", () => SaveCopy(), "Write a separate PNG under Assets.");
            saveButton = AddButton(footer, "Save PNG", "save", () => SaveEdits(), "Save pixel changes and reimport the asset.");

            UpdateFrames(frameIndex);
        }

        public override void SaveChanges()
        {
            if (SaveEdits())
                base.SaveChanges();
        }

        private int ZoomFactor => zoom == 0 ? 0 : 1 << (zoom - 1);
        private Color32 BrushColor => erasing ? new Color32(0, 0, 0, 0) : (Color32)color;

        private void OpenAsset(Object asset)
        {
            if (asset == source && document?.Texture != null)
                return;

            if (!PixelAssetIO.IsSupported(asset))
            {
                SetStatus("Select a PNG, an imported Aseprite file, or a sprite AnimationClip.", true);
                return;
            }
            if (document?.IsDirty == true)
            {
                int choice = EditorUtility.DisplayDialogComplex("Unsaved pixel edits", "Save the current edits before opening another asset?",
                    "Save", "Cancel", "Discard");
                if (choice == 1 || (choice == 0 && !SaveEdits()))
                    return;
            }

            try
            {
                var opened = PixelAssetIO.Open(asset);
                canvas.Finish();
                document?.Dispose();
                document = new PixelDocument(opened.Texture);
                importedFrames = opened.Frames;
                source = asset;
                pngGuid = opened.PngGuid;
                pngHash = opened.Hash;
                layoutMode = 0;
                columns = rows = 1;
                rootVisualElement.Q<PopupField<string>>("layout").index = 0;
                rootVisualElement.Q<IntegerField>("columns").SetValueWithoutNotify(1);
                rootVisualElement.Q<IntegerField>("rows").SetValueWithoutNotify(1);
                sourceField.SetValueWithoutNotify(asset);
                canvas.ResetPan();
                UpdateFrames(opened.Selected);
                SetStatus(DefaultStatus);
            }
            catch (Exception exception)
            {
                SetStatus(exception.Message, true);
            }
        }

        private void UpdateFrames(int index)
        {
            StopPlayback();
            canvas.Finish();
            canvas.Document = document;

            bool available = document?.Texture != null;
            loaded.style.display = available ? DisplayStyle.Flex : DisplayStyle.None;
            empty.style.display = available ? DisplayStyle.None : DisplayStyle.Flex;
            copyButton.style.display = available && !string.IsNullOrEmpty(pngGuid) ? DisplayStyle.Flex : DisplayStyle.None;
            saveButton.style.display = available ? DisplayStyle.Flex : DisplayStyle.None;
            gridFields.style.display = available && layoutMode == 2 ? DisplayStyle.Flex : DisplayStyle.None;
            if (!available)
            {
                frames = Array.Empty<PixelFrame>();
                RefreshActions();
                return;
            }

            int width = document.Texture.width;
            int height = document.Texture.height;
            fromClip = layoutMode == 0 && importedFrames[0].Seconds > 0;
            switch (layoutMode)
            {
                case 2:
                    columns = Mathf.Clamp(columns, 1, Mathf.Min(width, 64));
                    rows = Mathf.Clamp(rows, 1, Mathf.Min(height, 64));
                    frames = new PixelFrame[columns * rows];
                    for (int row = 0; row < rows; ++row)
                    for (int column = 0; column < columns; ++column)
                    {
                        int x0 = column * width / columns;
                        int x1 = (column + 1) * width / columns;
                        int top = height - row * height / rows;
                        int bottom = height - (row + 1) * height / rows;
                        int i = row * columns + column;
                        frames[i] = new PixelFrame(new RectInt(x0, bottom, x1 - x0, top - bottom), $"Frame {i + 1}");
                    }
                    break;
                case 1:
                    frames = new[] { new PixelFrame(new RectInt(0, 0, width, height), "Image") };
                    break;
                default:
                    frames = importedFrames;
                    break;
            }

            Vector2 lower = new(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 upper = new(float.NegativeInfinity, float.NegativeInfinity);
            foreach (var frame in frames)
            {
                lower = Vector2.Min(lower, -frame.Pivot);
                upper = Vector2.Max(upper, (Vector2)frame.Rect.size - frame.Pivot);
            }
            canvas.AnimationBounds = new Rect(lower, upper - lower);

            bool animated = frames.Length > 1;
            playButton.SetEnabled(animated);
            previousButton.SetEnabled(animated);
            nextButton.SetEnabled(animated);
            frameSlider.SetEnabled(animated);
            frameSlider.highValue = Mathf.Max(2, frames.Length);
            fpsField.style.display = fromClip ? DisplayStyle.None : DisplayStyle.Flex;
            fpsField.SetEnabled(animated);
            fpsField.tooltip = "Frames per second.";
            clipTiming.style.display = fromClip ? DisplayStyle.Flex : DisplayStyle.None;
            ShowFrame(Mathf.Clamp(index, 0, frames.Length - 1));
            RefreshActions();
        }

        private void ShowFrame(int index)
        {
            if (frames.Length == 0)
                return;
            frameIndex = Mathf.Clamp(index, 0, frames.Length - 1);
            var frame = frames[frameIndex];
            canvas.Frame = frame;
            frameSlider.SetValueWithoutNotify(frameIndex + 1);
            frameLabel.text = $"{frameIndex + 1} / {frames.Length}  ·  {frame.Rect.width} × {frame.Rect.height}";
            frameLabel.tooltip = frame.Name;
            Repaint();
        }

        private void Step(int direction)
        {
            StopPlayback();
            ShowFrame((frameIndex + direction + frames.Length) % frames.Length);
        }

        private float Duration(int i) => fromClip ? frames[i].Seconds : 1f / fps;

        private void TogglePlayback()
        {
            if (playing)
            {
                StopPlayback();
                return;
            }
            if (frames.Length < 2)
                return;

            canvas.Finish();
            playOffset = 0;
            for (int i = 0; i < frameIndex; ++i)
                playOffset += Duration(i);
            playStarted = EditorApplication.timeSinceStartup;
            playing = true;
            playButton.text = "Pause";
        }

        private void StopPlayback()
        {
            playing = false;
            if (playButton != null)
                playButton.text = "Play";
        }

        private void UpdatePlayback()
        {
            if (!playing)
                return;
            double length = 0;
            for (int i = 0; i < frames.Length; ++i)
                length += Duration(i);
            double elapsed = (EditorApplication.timeSinceStartup - playStarted + playOffset) % length;
            int index = 0;
            while (index < frames.Length - 1 && elapsed >= Duration(index))
                elapsed -= Duration(index++);
            if (index != frameIndex)
                ShowFrame(index);
        }

        private void SetTool(bool erase)
        {
            erasing = erase;
            canvas.PaintColor = BrushColor;
            RefreshActions();
        }

        private void Undo()
        {
            StopPlayback();
            canvas.Finish();
            document.Undo();
            RefreshActions();
            Repaint();
        }

        private void Redo()
        {
            StopPlayback();
            canvas.Finish();
            document.Redo();
            RefreshActions();
            Repaint();
        }

        private bool SaveEdits()
        {
            if (document?.Texture == null || !document.IsDirty)
                return true;
            canvas?.Finish();
            if (string.IsNullOrEmpty(pngGuid))
                return SaveCopy();

            try
            {
                pngHash = PixelAssetIO.SaveOriginal(document.Texture, pngGuid, pngHash);
                document.MarkSaved();
                SetStatus("Saved " + AssetDatabase.GUIDToAssetPath(pngGuid));
                RefreshActions();
                return true;
            }
            catch (Exception exception)
            {
                SetStatus(exception.Message, true);
                return false;
            }
        }

        private bool SaveCopy()
        {
            if (document?.Texture == null)
                return false;
            canvas?.Finish();

            string sourcePath = AssetDatabase.GetAssetPath(source);
            string folder = Path.GetDirectoryName(sourcePath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
                folder = "Assets";
            string file = string.IsNullOrEmpty(sourcePath) ? "Sprite" : Path.GetFileNameWithoutExtension(sourcePath);
            string path = EditorUtility.SaveFilePanelInProject("Save PNG to Project", file + "-edited", "png",
                "Choose a file in Assets.", folder);
            if (string.IsNullOrEmpty(path))
                return false;

            try
            {
                float ppu = source is Sprite sprite ? sprite.pixelsPerUnit : 100;
                if (AssetImporter.GetAtPath(sourcePath) is TextureImporter importer)
                    ppu = importer.spritePixelsPerUnit;
                bool wholeImage = frames.Length == 1 && frames[0].Rect.width == document.Texture.width
                    && frames[0].Rect.height == document.Texture.height;
                PixelAssetIO.SaveCopy(document.Texture, path, wholeImage, ppu);
                if (string.IsNullOrEmpty(pngGuid))
                    document.MarkSaved();
                SetStatus("Saved " + path);
                RefreshActions();
                return true;
            }
            catch (Exception exception)
            {
                SetStatus(exception.Message, true);
                return false;
            }
        }

        private void RefreshActions()
        {
            hasUnsavedChanges = document?.IsDirty == true;
            pencilButton.EnableInClassList("active-tool", !erasing);
            eraserButton.EnableInClassList("active-tool", erasing);
            saveButton.SetEnabled(hasUnsavedChanges);
            saveButton.text = string.IsNullOrEmpty(pngGuid) ? "Save PNG As…" : "Save PNG";
            undoButton.SetEnabled(document?.CanUndo == true);
            redoButton.SetEnabled(document?.CanRedo == true);
            statusLabel.text = hasUnsavedChanges && status == DefaultStatus ? "Unsaved pixel changes" : status;
            if (!error && !hasUnsavedChanges && document?.Texture == null)
                statusLabel.text = string.Empty;
            statusLabel.EnableInClassList("status-error", error);
        }

        private void SetStatus(string message, bool isError = false)
        {
            status = message;
            error = isError;
            RefreshActions();
        }

        private void PreserveEdits()
        {
            canvas?.Finish();
            if (document?.Texture != null && document.IsDirty)
                reloadPng = document.Texture.EncodeToPNG();
        }

        private void RestoreEdits()
        {
            if (document?.Texture != null)
            {
                reloadPng = null;
                return;
            }
            try
            {
                if (reloadPng is { Length: > 0 })
                {
                    document = new PixelDocument(PixelAssetIO.DecodePng(reloadPng), true);
                    reloadPng = null;
                }
                else if (source)
                {
                    var opened = PixelAssetIO.Open(source);
                    document = new PixelDocument(opened.Texture);
                    importedFrames = opened.Frames;
                    pngGuid = opened.PngGuid;
                    pngHash = opened.Hash;
                }
            }
            catch (Exception exception)
            {
                status = exception.Message;
                error = true;
                document = null;
            }
        }

        private static Texture2D MakeChecker()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.HideAndDontSave
            };
            var dark = new Color32(62, 66, 71, 255);
            var light = new Color32(82, 87, 92, 255);
            texture.SetPixels32(new[] { dark, light, light, dark });
            texture.Apply(false, true);
            return texture;
        }

        private static VisualElement AddRow(VisualElement parent, string rowClass)
        {
            var row = new VisualElement();
            row.AddToClassList("row");
            row.AddToClassList(rowClass);
            parent.Add(row);
            return row;
        }

        private static Button AddButton(VisualElement parent, string text, string name, Action action, string tooltip = "")
        {
            var button = new Button(action) { text = text, name = name, tooltip = tooltip };
            parent.Add(button);
            return button;
        }

        private static IntegerField AddNumber(VisualElement parent, string label, string name, int value, Func<int, int> change)
        {
            var field = new IntegerField(label) { name = name, value = value, isDelayed = true };
            field.AddToClassList("compact-number");
            field.RegisterValueChangedCallback(evt => field.SetValueWithoutNotify(change(evt.newValue)));
            parent.Add(field);
            return field;
        }
    }
}
