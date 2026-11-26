using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shakesprite
{
    internal static class PixelAssetIO
    {
        public static (Texture2D Texture, PixelFrame[] Frames, int Selected, string PngGuid, string Hash) Open(Object asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            if (!path.StartsWith("Assets/", StringComparison.Ordinal))
                throw new InvalidOperationException("Choose a pixel art asset from the project's Assets folder.");

            if (asset is AnimationClip clip)
                return OpenClip(clip);

            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".png")
                return OpenPng(path, asset as Sprite, null, null);

            if (extension == ".ase" || extension == ".aseprite")
            {
                var imported = AssetDatabase.LoadAllAssetsAtPath(path);
                var animation = imported.OfType<AnimationClip>().FirstOrDefault(item => SpriteKeys(item).Length > 0);
                if (animation && !(asset is Sprite))
                    return OpenClip(animation);

                var sprites = imported.OfType<Sprite>().ToArray();
                if (sprites.Length == 0)
                    throw new InvalidOperationException("Import the file with Unity's 2D Aseprite Importer or open an exported PNG.");

                return OpenAtlas(sprites, asset as Sprite, null, null);
            }

            throw new InvalidOperationException("Choose a PNG, an imported Aseprite file, or a sprite AnimationClip.");
        }

        public static bool IsSupported(Object asset)
        {
            if (!asset || !AssetDatabase.GetAssetPath(asset).StartsWith("Assets/", StringComparison.Ordinal))
                return false;

            return asset is AnimationClip || Path.GetExtension(AssetDatabase.GetAssetPath(asset)).ToLowerInvariant()
                is ".png" or ".ase" or ".aseprite";
        }

        public static Texture2D DecodePng(byte[] bytes)
        {
            if (bytes.Length < 24 || bytes[0] != 137 || bytes[1] != 80 || bytes[2] != 78 || bytes[3] != 71)
                throw new InvalidDataException("The selected file is not a valid PNG.");

            int width = ReadBigEndianInt(bytes, 16);
            int height = ReadBigEndianInt(bytes, 20);
            CheckSize(width, height);
            var decoded = NewTexture(2, 2);
            try
            {
                if (!ImageConversion.LoadImage(decoded, bytes))
                    throw new InvalidDataException("Unity could not read this PNG.");

                var editable = NewTexture(decoded.width, decoded.height);
                editable.SetPixels32(decoded.GetPixels32());
                editable.Apply(false);
                return editable;
            }
            finally
            {
                Object.DestroyImmediate(decoded);
            }
        }

        public static string SaveOriginal(Texture2D texture, string guid, string expectedHash)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
                throw new IOException("The original PNG is missing. Use Save Copy to keep your edits.");

            string fullPath = FullPath(assetPath);
            if (!File.Exists(fullPath) || Hash(File.ReadAllBytes(fullPath)) != expectedHash)
                throw new IOException("The original PNG changed on disk. Use Save Copy to keep your edits.");

            byte[] bytes = texture.EncodeToPNG();
            WritePng(fullPath, bytes, true);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            return Hash(bytes);
        }

        public static void SaveCopy(Texture2D texture, string assetPath, bool singleImage, float pixelsPerUnit)
        {
            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal)
                || !string.Equals(Path.GetExtension(assetPath), ".png", StringComparison.OrdinalIgnoreCase))
                throw new IOException("Choose a PNG destination in the project's Assets folder.");

            string fullPath = FullPath(assetPath);
            if (!fullPath.StartsWith(Application.dataPath.Replace('/', Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase))
                throw new IOException("Choose a PNG destination in the project's Assets folder.");

            WritePng(fullPath, texture.EncodeToPNG(), false);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);

            if (AssetImporter.GetAtPath(assetPath) is TextureImporter importer)
            {
                importer.textureType = singleImage ? TextureImporterType.Sprite : TextureImporterType.Default;
                importer.spriteImportMode = singleImage ? SpriteImportMode.Single : SpriteImportMode.None;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = Mathf.NextPowerOfTwo(Mathf.Max(32, Mathf.Max(texture.width, texture.height)));
                importer.SaveAndReimport();
            }
        }

        private static (Texture2D, PixelFrame[], int, string, string) OpenClip(AnimationClip clip)
        {
            var keys = SpriteKeys(clip);
            if (keys.Length == 0 || keys.Any(key => !(key.value is Sprite)))
                throw new InvalidOperationException("Choose a clip with one sprite track and a sprite in each keyframe.");
            if (keys.Length > PixelDocument.MaxFrames)
                throw new InvalidOperationException("This clip has more than 4096 sprite keyframes.");

            var sprites = keys.Select(key => (Sprite)key.value).ToArray();
            var durations = new float[keys.Length];
            float tick = 1f / Mathf.Max(1f, clip.frameRate);
            for (int i = 0; i < keys.Length; ++i)
            {
                float nextTime = i + 1 < keys.Length ? keys[i + 1].time : clip.length;
                float time = i == 0 ? 0 : keys[i].time;
                durations[i] = Mathf.Max(tick, nextTime - time);
            }

            string spritePath = AssetDatabase.GetAssetPath(sprites[0]);
            if (Path.GetExtension(spritePath).Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                if (sprites.Any(sprite => AssetDatabase.GetAssetPath(sprite) != spritePath))
                    throw new InvalidOperationException("This clip uses multiple images. Open its sprite sheets individually.");
                return OpenPng(spritePath, null, sprites, durations);
            }

            if (Path.GetExtension(spritePath).ToLowerInvariant() is ".ase" or ".aseprite")
                return OpenAtlas(sprites, null, durations, clip);

            throw new InvalidOperationException("This clip needs sprites from a single PNG or imported Aseprite atlas.");
        }

        private static ObjectReferenceKeyframe[] SpriteKeys(AnimationClip clip)
        {
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip)
                .Where(binding => binding.propertyName == "m_Sprite").ToArray();
            return bindings.Length == 1
                ? AnimationUtility.GetObjectReferenceCurve(clip, bindings[0])
                : Array.Empty<ObjectReferenceKeyframe>();
        }

        private static (Texture2D, PixelFrame[], int, string, string) OpenPng(
            string path, Sprite selected, Sprite[] ordered, float[] durations)
        {
            if (!path.StartsWith("Assets/", StringComparison.Ordinal))
                throw new InvalidOperationException("Open PNGs located in the project's Assets folder.");

            byte[] bytes = File.ReadAllBytes(FullPath(path));
            var texture = DecodePng(bytes);
            try
            {
                var sprites = ordered ?? AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                    .OrderByDescending(sprite => sprite.rect.yMax).ThenBy(sprite => sprite.rect.xMin)
                    .ThenBy(sprite => sprite.name, StringComparer.Ordinal).ToArray();

                PixelFrame[] frames = MakeFrames(texture, sprites, durations, false);
                int index = Mathf.Max(0, Array.IndexOf(sprites, selected));
                return (texture, frames, index, AssetDatabase.AssetPathToGUID(path), Hash(bytes));
            }
            catch
            {
                Object.DestroyImmediate(texture);
                throw;
            }
        }

        private static (Texture2D, PixelFrame[], int, string, string) OpenAtlas(
            Sprite[] sprites, Sprite selected, float[] durations, AnimationClip clip)
        {
            var atlas = sprites[0].texture;
            if (!atlas || sprites.Any(sprite => sprite.texture != atlas || sprite.packed))
                throw new InvalidOperationException("Open an Aseprite file that imports to one unpacked texture.");

            CheckSize(atlas.width, atlas.height);
            if (!clip)
                sprites = sprites.OrderBy(sprite => sprite.name, StringComparer.Ordinal).ToArray();

            var texture = NewTexture(atlas.width, atlas.height);
            try
            {
                if (atlas.isReadable)
                    texture.SetPixels32(atlas.GetPixels32());
                else
                    Readback(atlas, texture);

                texture.Apply(false);
                PixelFrame[] frames = MakeFrames(texture, sprites, durations, true);
                return (texture, frames, Mathf.Max(0, Array.IndexOf(sprites, selected)), null, null);
            }
            catch
            {
                Object.DestroyImmediate(texture);
                throw;
            }
        }

        private static PixelFrame[] MakeFrames(Texture2D texture, Sprite[] sprites, float[] durations, bool atlas)
        {
            if (sprites.Length == 0)
                return new[] { new PixelFrame(new RectInt(0, 0, texture.width, texture.height), "Image") };
            if (sprites.Length > PixelDocument.MaxFrames)
                throw new InvalidOperationException("This image contains more than 4096 frames.");

            var frames = new PixelFrame[sprites.Length];
            for (int i = 0; i < sprites.Length; ++i)
            {
                var sprite = sprites[i];
                Rect rect = atlas ? sprite.textureRect : sprite.rect;
                var bounds = new RectInt(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y),
                    Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height));
                if (bounds.width < 1 || bounds.height < 1 || bounds.xMin < 0 || bounds.yMin < 0
                    || bounds.xMax > texture.width || bounds.yMax > texture.height)
                    throw new InvalidDataException($"Frame '{sprite.name}' falls outside the source image.");

                Vector2 pivot = atlas ? sprite.pivot - sprite.textureRectOffset : sprite.pivot;
                frames[i] = new PixelFrame(bounds, sprite.name, durations == null ? 0 : durations[i], pivot);
            }

            return frames;
        }

        private static void Readback(Texture2D source, Texture2D destination)
        {
            var previous = RenderTexture.active;
            bool previousSrgb = GL.sRGBWrite;
            var temporary = RenderTexture.GetTemporary(source.width, source.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            try
            {
                GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                Graphics.Blit(source, temporary);
                RenderTexture.active = temporary;
                destination.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
            }
            finally
            {
                GL.sRGBWrite = previousSrgb;
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(temporary);
            }
        }

        private static void CheckSize(int width, int height)
        {
            if (width < 1 || height < 1 || width > PixelDocument.MaxDimension || height > PixelDocument.MaxDimension)
                throw new InvalidOperationException("Shakesprite supports images up to 4096 × 4096 pixels.");
        }

        private static int ReadBigEndianInt(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
        }

        private static Texture2D NewTexture(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "Shakesprite working image",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private static string Hash(byte[] bytes)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }

        private static string FullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath));
        }

        private static void WritePng(string path, byte[] bytes, bool replace)
        {
            if (bytes == null || bytes.Length == 0)
                throw new IOException("Unity could not encode the edited image.");

            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllBytes(temporary, bytes);
                if (replace)
                    File.Replace(temporary, path, null);
                else
                    File.Move(temporary, path);
            }
            finally
            {
                if (File.Exists(temporary))
                    File.Delete(temporary);
            }
        }
    }
}
