using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shakesprite
{
    [Serializable]
    internal struct PixelFrame
    {
        public RectInt Rect;
        public string Name;
        public float Seconds;
        public Vector2 Pivot;

        public PixelFrame(RectInt rect, string name, float seconds = 0, Vector2? pivot = null)
        {
            Rect = rect;
            Name = name;
            Seconds = seconds;
            Pivot = pivot ?? new Vector2(rect.width * 0.5f, rect.height * 0.5f);
        }
    }

    [Serializable]
    internal sealed class PixelDocument : IDisposable
    {
        public const int MaxDimension = 4096;
        public const int MaxFrames = 4096;
        private const int HistoryBudget = 1_000_000;

        [SerializeField] private Texture2D texture;
        [SerializeField] private int revision;
        [SerializeField] private int savedRevision;
        [NonSerialized] private int nextRevision;
        [NonSerialized] private List<Stroke> history;
        [NonSerialized] private int historyPosition;
        [NonSerialized] private int historySize;
        [NonSerialized] private Stroke activeStroke;

        public Texture2D Texture => texture;
        public bool IsDirty => revision != savedRevision || activeStroke?.Before.Count > 0;
        public bool CanUndo => historyPosition > 0;
        public bool CanRedo => historyPosition < History.Count;
        private List<Stroke> History => history ??= new List<Stroke>();

        public PixelDocument(Texture2D texture, bool hasEdits = false)
        {
            this.texture = texture;
            revision = hasEdits ? 1 : 0;
        }

        public void BeginStroke(Color32 color)
        {
            EndStroke();
            activeStroke = new Stroke(color, revision);
        }

        public void PaintLine(Vector2Int from, Vector2Int to)
        {
            if (activeStroke == null)
                return;

            var pixels = texture.GetRawTextureData<Color32>();
            int dx = Mathf.Abs(to.x - from.x);
            int dy = -Mathf.Abs(to.y - from.y);
            int sx = from.x < to.x ? 1 : -1;
            int sy = from.y < to.y ? 1 : -1;
            int error = dx + dy;
            bool changed = false;

            while (true)
            {
                int index = from.y * texture.width + from.x;
                Color32 previous = pixels[index];
                if (!previous.Equals(activeStroke.Color))
                {
                    activeStroke.Before.TryAdd(index, previous);
                    pixels[index] = activeStroke.Color;
                    changed = true;
                }

                if (from == to)
                    break;

                int twice = 2 * error;
                if (twice >= dy)
                {
                    error += dy;
                    from.x += sx;
                }
                if (twice <= dx)
                {
                    error += dx;
                    from.y += sy;
                }
            }

            if (changed)
                texture.Apply(false);
        }

        public void EndStroke()
        {
            var stroke = activeStroke;
            activeStroke = null;
            if (stroke == null || stroke.Before.Count == 0)
                return;

            for (int i = History.Count - 1; i >= historyPosition; --i)
            {
                historySize -= History[i].Before.Count;
                History.RemoveAt(i);
            }

            nextRevision = Math.Max(nextRevision, Math.Max(revision, savedRevision)) + 1;
            revision = nextRevision;
            stroke.AfterRevision = revision;
            History.Add(stroke);
            historySize += stroke.Before.Count;
            while (History.Count > 1 && (History.Count > 64 || historySize > HistoryBudget))
            {
                historySize -= History[0].Before.Count;
                History.RemoveAt(0);
            }
            historyPosition = History.Count;
        }

        public void Undo()
        {
            EndStroke();
            if (CanUndo)
                Apply(History[--historyPosition], false);
        }

        public void Redo()
        {
            EndStroke();
            if (CanRedo)
                Apply(History[historyPosition++], true);
        }

        public void MarkSaved()
        {
            EndStroke();
            savedRevision = revision;
        }

        public void Dispose()
        {
            if (texture)
                Object.DestroyImmediate(texture);
        }

        private void Apply(Stroke stroke, bool redo)
        {
            var pixels = texture.GetRawTextureData<Color32>();
            foreach (var pixel in stroke.Before)
                pixels[pixel.Key] = redo ? stroke.Color : pixel.Value;

            revision = redo ? stroke.AfterRevision : stroke.BeforeRevision;
            texture.Apply(false);
        }

        private sealed class Stroke
        {
            public readonly Color32 Color;
            public readonly int BeforeRevision;
            public readonly Dictionary<int, Color32> Before = new();
            public int AfterRevision;

            public Stroke(Color32 color, int revision)
            {
                Color = color;
                BeforeRevision = revision;
            }
        }
    }
}
