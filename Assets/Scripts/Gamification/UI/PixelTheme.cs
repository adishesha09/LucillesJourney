using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace LucillesJourney.Gamification.UI
{
    /// <summary>
    /// The shared look of every gameplay interface: palette, pixel-art sprites (loaded from
    /// Assets/Resources/JourneyUI) and the two pixel fonts. Screens ask this class for styling
    /// rather than hard-coding colours or fonts.
    /// </summary>
    public static class PixelTheme
    {
        // Palette (warm tones for the interface chrome; chapter art carries the warm/muted story palette).
        public static readonly Color Ink = Hex("1A2224");
        public static readonly Color DeepBrown = Hex("2B2321");
        public static readonly Color Brown = Hex("4A3B32");
        public static readonly Color MidBrown = Hex("6A5444");
        public static readonly Color Tan = Hex("A8895E");
        public static readonly Color Sand = Hex("C8B58A");
        public static readonly Color Cream = Hex("F4EAD2");
        public static readonly Color Parchment = Hex("E8D9B5");
        public static readonly Color Gold = Hex("F2D26B");
        public static readonly Color Orange = Hex("D98F4B");
        public static readonly Color Terracotta = Hex("C05A4A");
        public static readonly Color Green = Hex("6FA86B");
        public static readonly Color Teal = Hex("4B8F8A");
        public static readonly Color Blue = Hex("6B8FA8");
        public static readonly Color Muted = Hex("7A7268");
        public static readonly Color Dim = new Color(0.10f, 0.09f, 0.08f, 0.82f);

        /// <summary>Source pixels are drawn this many times larger than native (a whole number keeps pixels square).</summary>
        public const int PixelScale = 3;

        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        private static readonly HashSet<string> Missing = new HashSet<string>();

        /// <summary>Returns the named sprite from Resources/JourneyUI, or null (with a one-time warning) if absent.</summary>
        public static Sprite Sprite(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (Sprites.TryGetValue(name, out var cached) && cached != null) return cached;

            var sprite = Resources.Load<Sprite>("JourneyUI/" + name);
            if (sprite == null)
            {
                if (Missing.Add(name)) Debug.LogWarning($"PixelTheme: missing sprite 'JourneyUI/{name}'.");
                return null;
            }
            Sprites[name] = sprite;
            return sprite;
        }

        public static TMP_FontAsset HeadingFont => PixelFonts.Heading;
        public static TMP_FontAsset BodyFont => PixelFonts.Body;

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }

    /// <summary>
    /// Creates the two pixel-art TextMesh Pro font assets from the bundled OFL fonts
    /// (Press Start 2P for headings and labels, VT323 for reading text) and gives both the stock
    /// Liberation Sans asset as a fallback for any glyph the pixel fonts lack.
    /// </summary>
    public static class PixelFonts
    {
        private static TMP_FontAsset heading;
        private static TMP_FontAsset body;

        public static TMP_FontAsset Heading => heading != null ? heading : (heading = Create("Fonts/PressStart2P-Regular", 8));
        public static TMP_FontAsset Body => body != null ? body : (body = Create("Fonts/VT323-Regular", 5));

        private static TMP_FontAsset Create(string resourcePath, int padding)
        {
            var font = Resources.Load<Font>(resourcePath);
            if (font == null)
            {
                Debug.LogWarning($"PixelFonts: font '{resourcePath}' not found in Resources; falling back to the default TMP font.");
                return TMP_Settings.defaultFontAsset;
            }

            var asset = TMP_FontAsset.CreateFontAsset(font, 64, padding, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                1024, 1024, AtlasPopulationMode.Dynamic, true);
            if (asset == null)
            {
                Debug.LogWarning($"PixelFonts: could not create a font asset from '{resourcePath}'.");
                return TMP_Settings.defaultFontAsset;
            }

            asset.name = font.name + " SDF (runtime)";
            var fallback = TMP_Settings.defaultFontAsset;
            if (fallback != null && fallback != asset)
            {
                asset.fallbackFontAssetTable = new List<TMP_FontAsset> { fallback };
            }
            return asset;
        }
    }
}
