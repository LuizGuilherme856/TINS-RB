using UnityEngine;

namespace TINS.Campaign
{
    /// <summary>Reads one saved purification stage from a grid of animation frames.</summary>
    internal sealed class CampaignPlanetRotation
    {
        public const int FrameCount = 240;
        public const float SecondsPerFrame = .1f;
        private const int Columns = 16;
        private const int Rows = FrameCount / Columns;
        private const int StageCount = 5;

        private readonly Texture2D sheet;
        private readonly Sprite[] frames = new Sprite[FrameCount];
        private readonly int side;
        public int Stage { get; }

        public CampaignPlanetRotation(string planetId, int stage)
        {
            Stage = stage;
            sheet = Resources.Load<Texture2D>($"PlanetRotations/{planetId}/{planetId}-{stage}");
            if (sheet != null && sheet.width % Columns == 0 && sheet.height == sheet.width / Columns * Rows)
                side = sheet.width / Columns;
        }

        public static int StageFor(int victories, int goal)
        {
            goal = Mathf.Max(1, goal);
            if (victories <= 0) return 0;
            if (victories >= goal) return StageCount - 1;
            return Mathf.Min(StageCount - 2, Mathf.CeilToInt(victories * (StageCount - 1f) / goal));
        }

        public Sprite Frame(int index)
        {
            if (side == 0 || sheet == null) return null;

            if (frames[index] == null)
            {
                int x = index % Columns * side;
                int y = sheet.height - (index / Columns + 1) * side;
                frames[index] = Sprite.Create(sheet, new Rect(x, y, side, side),
                    new Vector2(.5f, .5f), 16f, 0, SpriteMeshType.FullRect);
            }
            return frames[index];
        }

        public void Release()
        {
            foreach (Sprite sprite in frames)
                if (sprite != null) Object.Destroy(sprite);
        }
    }
}
