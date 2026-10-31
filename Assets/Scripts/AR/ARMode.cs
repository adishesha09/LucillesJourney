using UnityEngine;

namespace LucillesJourney.AR
{
    /// <summary>
    /// The player's AR choice. Always compiled, so the menu and game code can ask about AR even in a project that has
    /// no AR packages installed (then <see cref="Supported"/> is simply false and nothing AR-related appears).
    /// The AR scripts only compile once the editor tool has installed AR Foundation and defined LJ_AR.
    /// </summary>
    public static class ARMode
    {
        private const string Key = "lj_ar_requested";
        private const string WebcamKey = "lj_webcam_requested";

        /// <summary>True if this build can run AR: AR packages present, and a phone/tablet (or the editor, for simulation).</summary>
        public static bool Supported
        {
            get
            {
#if LJ_AR
                return Application.isMobilePlatform || Application.isEditor;
#else
                return false;
#endif
            }
        }

        /// <summary>Remembered between sessions. Read when a gameplay scene loads; the main menu sets it.</summary>
        public static bool Requested
        {
            get => Supported && PlayerPrefs.GetInt(Key, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(Key, value ? 1 : 0);
                if (value) PlayerPrefs.SetInt(WebcamKey, 0); // one mode at a time
                PlayerPrefs.Save();
            }
        }

        /// <summary>Desktop camera mode is offered on every non-mobile platform, even when no webcam is plugged in right now.</summary>
        public static bool WebcamSupported => !Application.isMobilePlatform;

        /// <summary>True if a webcam is actually connected at the moment.</summary>
        public static bool WebcamAvailable => WebCamTexture.devices.Length > 0;

        public static bool WebcamRequested
        {
            get => WebcamSupported && PlayerPrefs.GetInt(WebcamKey, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(WebcamKey, value ? 1 : 0);
                if (value) PlayerPrefs.SetInt(Key, 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>Set when AR was requested but the device could not start it, so the game fell back to flat mode.</summary>
        public static string FallbackReason { get; internal set; }
    }
}