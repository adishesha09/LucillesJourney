using UnityEditor;
using UnityEngine;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Imports every PNG under Assets/Resources/JourneyUI as a crisp pixel-art sprite: Point filtering,
    /// no compression or mip-maps, and 9-slice borders for the UI frame sprites. UI sprites use 25
    /// pixels-per-unit so that, with the canvas reference of 100 PPU, one source pixel is drawn as 4
    /// canvas units; world sprites (world_*) use 16 PPU to match the scene's 16 px/unit placeholder art.
    /// </summary>
    public class JourneyUITexturePostprocessor : AssetPostprocessor
    {
        private const string Folder = "Assets/Resources/JourneyUI/";
        private const float UiPixelsPerUnit = 25f;
        private const float WorldPixelsPerUnit = 16f;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder)) return;

            string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            var importer = (TextureImporter)assetImporter;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = name.StartsWith("world_") ? WorldPixelsPerUnit : UiPixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048;
            importer.spriteBorder = BorderFor(name);

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
        }

        private static Vector4 BorderFor(string name)
        {
            switch (name)
            {
                case "ui_panel":
                case "ui_panel_parchment":
                case "ui_button":
                case "ui_button_hover":
                case "ui_button_pressed":
                case "ui_button_disabled":
                case "ui_button_selected":
                case "ui_tab":
                case "ui_tab_active":
                case "ui_slot":
                case "ui_slot_open":
                case "ui_toast":
                    return new Vector4(8, 8, 8, 8);
                case "ui_bar_frame":
                case "ui_slider_bg":
                    return new Vector4(4, 4, 4, 4);
                default:
                    return Vector4.zero;
            }
        }
    }
}
