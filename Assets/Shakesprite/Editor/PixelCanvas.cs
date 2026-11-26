using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shakesprite
{
    internal sealed class PixelCanvas : IMGUIContainer
    {
        private readonly EditorWindow window;
        private readonly Texture2D checker;
        private int capture;
        private Vector2 pan;
        private Vector2Int? lastPixel;
        private Rect displayRect;

        public PixelDocument Document;
        public PixelFrame Frame;
        public Rect AnimationBounds;
        public Color32 PaintColor;
        public int Zoom;
        public Action StrokeStarted;
        public Action StrokeChanged;
        public Action StrokeEnded;

        public PixelCanvas(EditorWindow window, Texture2D checker)
        {
            this.window = window;
            this.checker = checker;
            name = "pixel-canvas";
            AddToClassList("pixel-canvas");
            onGUIHandler = Draw;
        }

        public void ResetPan()
        {
            pan = Vector2.zero;
            window.Repaint();
        }

        public void Finish()
        {
            if (capture != 0 && GUIUtility.hotControl == capture)
                GUIUtility.hotControl = 0;

            capture = 0;
            lastPixel = null;
            Document?.EndStroke();
        }

        private void Draw()
        {
            if (Document?.Texture == null)
                return;

            var texture = Document.Texture;
            var viewport = new Rect(Vector2.zero, contentRect.size);
            int id = GUIUtility.GetControlID(FocusType.Passive);
            var frame = Frame.Rect;
            float fit = Mathf.Min((viewport.width - 32) / AnimationBounds.width, (viewport.height - 32) / AnimationBounds.height);
            float scale = Zoom == 0 ? Mathf.Clamp(fit < 1 ? fit : Mathf.Floor(fit), 0.01f, 32) : Zoom;
            Vector2 animationSize = AnimationBounds.size * scale;
            Vector2 limit = Vector2.Max(Vector2.zero, (animationSize - viewport.size) * 0.5f + Vector2.one * 16);
            pan.x = Mathf.Clamp(pan.x, -limit.x, limit.x);
            pan.y = Mathf.Clamp(pan.y, -limit.y, limit.y);
            Vector2 origin = (viewport.size - animationSize) * 0.5f + pan;
            origin.x += (-Frame.Pivot.x - AnimationBounds.xMin) * scale;
            origin.y += (AnimationBounds.yMax - frame.height + Frame.Pivot.y) * scale;
            var size = new Vector2(frame.width * scale, frame.height * scale);
            displayRect = new Rect(Mathf.Round(origin.x), Mathf.Round(origin.y), size.x, size.y);

            var input = Event.current;
            if (input.type == EventType.Repaint)
            {
                var border = new Rect(displayRect.x - 1, displayRect.y - 1, size.x + 2, size.y + 2);
                EditorGUI.DrawRect(border, new Color(0.47f, 0.52f, 0.55f));
                var checkerUv = new Rect(0, 0, (float)frame.width / checker.width, (float)frame.height / checker.height);
                GUI.DrawTextureWithTexCoords(displayRect, checker, checkerUv);
                GUI.DrawTextureWithTexCoords(displayRect, texture, new Rect(
                    (float)frame.x / texture.width, (float)frame.y / texture.height,
                    (float)frame.width / texture.width, (float)frame.height / texture.height));
                EditorGUIUtility.AddCursorRect(displayRect, MouseCursor.ArrowPlus);
            }

            switch (input.GetTypeForControl(id))
            {
                case EventType.MouseDown when input.button == 0 && displayRect.Contains(input.mousePosition):
                    StrokeStarted?.Invoke();
                    capture = id;
                    GUIUtility.hotControl = id;
                    Document.BeginStroke(PaintColor);
                    Paint(input.mousePosition);
                    input.Use();
                    break;
                case EventType.MouseDown when input.button == 2 && viewport.Contains(input.mousePosition):
                    capture = id;
                    GUIUtility.hotControl = id;
                    input.Use();
                    break;
                case EventType.MouseDrag when GUIUtility.hotControl == id:
                    if (input.button == 0)
                        Paint(input.mousePosition);
                    if (input.button == 2)
                    {
                        pan += input.delta;
                        window.Repaint();
                    }
                    input.Use();
                    break;
                case EventType.MouseUp when GUIUtility.hotControl == id:
                    Finish();
                    StrokeEnded?.Invoke();
                    input.Use();
                    break;
            }
        }

        private void Paint(Vector2 position)
        {
            if (!displayRect.Contains(position))
            {
                lastPixel = null;
                return;
            }

            var frame = Frame.Rect;
            int x = Mathf.FloorToInt((position.x - displayRect.xMin) / displayRect.width * frame.width);
            int y = Mathf.FloorToInt((position.y - displayRect.yMin) / displayRect.height * frame.height);
            var pixel = new Vector2Int(frame.xMin + Mathf.Min(x, frame.width - 1), frame.yMax - 1 - Mathf.Min(y, frame.height - 1));
            Document.PaintLine(lastPixel ?? pixel, pixel);
            lastPixel = pixel;
            StrokeChanged?.Invoke();
            window.Repaint();
        }
    }
}
