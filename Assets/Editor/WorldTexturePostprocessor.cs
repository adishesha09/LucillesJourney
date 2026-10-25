using UnityEditor;
using UnityEngine;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Imports everything under Assets/Resources/World as crisp 16 pixels-per-unit pixel art: Point filtering, no
    /// compression or mip-maps. The pivot is the bottom-centre (feet / base of an object) so a prop's layout position
    /// is where it touches the ground and sorting by height works; baked ground layers (name ending "_ground") use
    /// the bottom-left corner so they line up with the map origin.
    /// </summary>
    public class WorldTexturePostprocessor : AssetPostprocessor
    {
        private const string Folder = "Assets/Resources/World/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder) || !assetPath.EndsWith(".png")) return;

            string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            var importer = (TextureImporter)assetImporter;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
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
            settings.spriteAlignment = (int)(name.EndsWith("_ground") ? SpriteAlignment.BottomLeft : SpriteAlignment.BottomCenter);
            importer.SetTextureSettings(settings);
        }
    }
}
