using UnityEditor;
using UnityEngine;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Imports everything under Assets/Resources/Menu (the menu and ending backdrops) as crisp pixel art: Point filtering,
    /// no compression or mip-maps, full-rect sprites, centre pivot. The anchors JSON is left as a plain TextAsset.
    /// </summary>
    public class MenuTexturePostprocessor : AssetPostprocessor
    {
        private const string Folder = "Assets/Resources/Menu/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder) || !assetPath.EndsWith(".png")) return;
            var importer = (TextureImporter)assetImporter;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 1f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
        }
    }
}
