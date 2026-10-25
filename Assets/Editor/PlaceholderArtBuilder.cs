using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Generates small, original pixel-art placeholder sprites (floor, wall, a generic marker
    /// blob, the player, and a memory-fragment spark) so the prototype is playable and visually
    /// cohesive before final art from the team exists. Markers/player are plain white silhouettes
    /// tinted per-instance via SpriteRenderer.color to keep a single consistent shape language.
    /// </summary>
    public static class PlaceholderArtBuilder
    {
        private const string ArtFolder = "Assets/Art/Placeholders";
        private const int Size = 16;

        public static Sprite FloorTile => GetOrCreate("floor_tile", BuildFloorTile, wrapRepeat: true);
        public static Sprite WallTile => GetOrCreate("wall_tile", BuildWallTile, wrapRepeat: true);
        public static Sprite MarkerBlob => GetOrCreate("marker_blob", BuildMarkerBlob);
        public static Sprite PlayerBlob => GetOrCreate("player_blob", BuildPlayerBlob);
        public static Sprite MemorySpark => GetOrCreate("memory_spark", BuildMemorySpark);

        public static void EnsureAll()
        {
            _ = FloorTile; _ = WallTile; _ = MarkerBlob; _ = PlayerBlob; _ = MemorySpark;
        }

        private static Sprite GetOrCreate(string name, Func<Texture2D> builder, bool wrapRepeat = false)
        {
            string path = $"{ArtFolder}/{name}.png";
            var existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existingSprite != null) return existingSprite;

            Directory.CreateDirectory(ArtFolder);
            var tex = builder();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = Size;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.wrapMode = wrapRepeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Texture2D NewTex()
        {
            var t = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var clear = new Color32[Size * Size];
            t.SetPixels32(clear);
            return t;
        }

        private static Texture2D BuildFloorTile()
        {
            var t = NewTex();
            Color32 baseColor = new Color32(0xE8, 0xD9, 0xB8, 255); // warm tan
            Color32 lineColor = new Color32(0xD7, 0xC4, 0x9A, 255);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    t.SetPixel(x, y, (x == 0 || y == 0) ? lineColor : baseColor);
            t.Apply();
            return t;
        }

        private static Texture2D BuildWallTile()
        {
            var t = NewTex();
            Color32 baseColor = new Color32(0x6B, 0x4F, 0x3A, 255); // muted brown
            Color32 lineColor = new Color32(0x54, 0x3C, 0x2A, 255);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    bool brickLine = y % 8 == 0 || (x + (y / 8 % 2) * 8) % 16 == 0;
                    t.SetPixel(x, y, brickLine ? lineColor : baseColor);
                }
            t.Apply();
            return t;
        }

        private static Texture2D BuildMarkerBlob()
        {
            var t = NewTex();
            Vector2 center = new Vector2(8f, 7f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    if (dist <= 6f) t.SetPixel(x, y, Color.white);
                }
            t.Apply();
            return t;
        }

        private static Texture2D BuildPlayerBlob()
        {
            var t = NewTex();
            Vector2 body = new Vector2(8f, 5.5f);
            Vector2 head = new Vector2(8f, 11.5f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    bool inBody = Vector2.Distance(p, body) <= 5.5f;
                    bool inHead = Vector2.Distance(p, head) <= 3f;
                    if (inBody || inHead) t.SetPixel(x, y, Color.white);
                }
            t.Apply();
            return t;
        }

        private static Texture2D BuildMemorySpark()
        {
            var t = NewTex();
            Vector2 center = new Vector2(8f, 8f);
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    Vector2 d = p - center;
                    // Four-pointed sparkle: small diamond plus thin cross arms.
                    bool diamond = Mathf.Abs(d.x) + Mathf.Abs(d.y) <= 2.5f;
                    bool arms = (Mathf.Abs(d.x) <= 0.8f && Mathf.Abs(d.y) <= 7f) ||
                                (Mathf.Abs(d.y) <= 0.8f && Mathf.Abs(d.x) <= 7f);
                    if (diamond || arms) t.SetPixel(x, y, Color.white);
                }
            t.Apply();
            return t;
        }
    }
}
