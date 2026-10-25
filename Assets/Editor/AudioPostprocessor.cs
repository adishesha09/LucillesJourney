using UnityEditor;
using UnityEngine;

namespace LucillesJourney.EditorTools
{
    /// <summary>
    /// Imports everything under Assets/Resources/Audio with sensible game settings: music and ambience are
    /// streamed Vorbis (small build, low memory, loops cleanly); short effects are decoded on load so they
    /// play with no latency.
    /// </summary>
    public class AudioPostprocessor : AssetPostprocessor
    {
        private const string Folder = "Assets/Resources/Audio/";

        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(Folder)) return;

            string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            bool longForm = name.StartsWith("music_") || name.StartsWith("amb_");
            var importer = (AudioImporter)assetImporter;

            var settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = longForm ? 0.55f : 0.7f;
            settings.loadType = longForm ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings = settings;
            importer.forceToMono = true;
            importer.loadInBackground = longForm;
            importer.ambisonic = false;
        }
    }
}
