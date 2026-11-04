using System;
using UnityEngine;
using UnityEngine.UI;

namespace LucillesJourney.AR
{
    /// <summary>
    /// The laptop/PC webcam as a picture for a RawImage: picks a sensible camera and mode, skips infrared cameras, recovers
    /// from a camera that will not start, and keeps the picture upright, mirrored and cropped to fill. Shared by the
    /// gameplay board and the ending screen.
    /// </summary>
    public sealed class WebcamFeed
    {
        private WebCamDevice[] devices = new WebCamDevice[0];
        private RawImage target;
        private int index, failedStarts;
        private float startedAt;

        public WebCamTexture Texture { get; private set; }

        /// <summary>Selfie-style mirroring. On by default, which suits a laptop webcam.</summary>
        public bool Mirrored { get; set; } = true;

        public bool HasPicture => Texture != null && Texture.isPlaying && Texture.width >= 100; // width stays ~16 until the first frame

        /// <summary>Raised whenever a different camera starts, so anything following the picture can start afresh.</summary>
        public event Action Restarted;

        public void Begin(RawImage image)
        {
            target = image;
            devices = WebCamTexture.devices;
            Start(FirstUsable(), true);
        }

        public void Next() => Start(NextUsable(index), true);

        public void ToggleMirror() => Mirrored = !Mirrored;

        public void Stop()
        {
            if (Texture == null) return;
            if (Texture.isPlaying) Texture.Stop();
            UnityEngine.Object.Destroy(Texture);
            Texture = null;
        }

        /// <summary>Call every frame: skips a camera that never delivers a picture, and fits the picture to the screen.</summary>
        public void Tick(AspectRatioFitter fit)
        {
            if (Texture != null && Texture.width < 100 && devices.Length > 1 && failedStarts < devices.Length - 1 &&
                Time.unscaledTime - startedAt > 3f)
            {
                failedStarts++;
                Debug.LogWarning("Camera: '" + devices[index].name + "' gave no picture - trying the next camera.");
                Start(NextUsable(index), false);
            }

            if (!HasPicture || target == null) return;
            bool flippedY = Texture.videoVerticallyMirrored;
            target.uvRect = new Rect(Mirrored ? 1f : 0f, flippedY ? 1f : 0f, Mirrored ? -1f : 1f, flippedY ? -1f : 1f);
            target.rectTransform.localEulerAngles = new Vector3(0f, 0f, -Texture.videoRotationAngle);
            if (fit != null)
            {
                bool sideways = Texture.videoRotationAngle == 90 || Texture.videoRotationAngle == 270;
                fit.aspectRatio = sideways ? (float)Texture.height / Texture.width : (float)Texture.width / Texture.height;
            }
        }

        // -------------------------------------------------------------------------------------------------------------

        private void Start(int deviceIndex, bool userChoice)
        {
            Stop();
            Restarted?.Invoke();
            if (devices.Length == 0)
            {
                Debug.LogWarning("Camera: no webcam found - using a plain background.");
                if (target != null) target.texture = null;
                return;
            }
            if (userChoice) failedStarts = 0;
            index = ((deviceIndex % devices.Length) + devices.Length) % devices.Length;
            Texture = Create(devices[index]);
            if (target != null) { target.texture = Texture; target.color = Color.white; }
            Texture.Play();
            startedAt = Time.unscaledTime;
        }

        // Asks the camera which modes it really supports and takes the one closest to 1280x720. Requesting a fixed mode makes
        // Windows fail with "Couldn't config the stream!" on cameras that do not offer exactly that.
        private static WebCamTexture Create(WebCamDevice device)
        {
            var modes = device.availableResolutions;
            if (modes != null && modes.Length > 0)
            {
                Resolution best = modes[0];
                int bestDistance = int.MaxValue;
                foreach (var mode in modes)
                {
                    int distance = Mathf.Abs(mode.width - 1280) + Mathf.Abs(mode.height - 720);
                    if (distance < bestDistance) { bestDistance = distance; best = mode; }
                }
                return new WebCamTexture(device.name, best.width, best.height, best.refreshRate > 0 ? best.refreshRate : 30);
            }
            return new WebCamTexture(device.name); // let the system choose
        }

        // Infrared / depth cameras (the face-login sensor on many laptops) do not give a normal picture.
        private static bool LooksLikeInfrared(string name) =>
            name.IndexOf(" IR", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("infrared", StringComparison.OrdinalIgnoreCase) >= 0 ||
            name.IndexOf("depth", StringComparison.OrdinalIgnoreCase) >= 0;

        private int FirstUsable()
        {
            for (int i = 0; i < devices.Length; i++)
                if (!LooksLikeInfrared(devices[i].name)) return i;
            return 0;
        }

        private int NextUsable(int from)
        {
            for (int step = 1; step <= devices.Length; step++)
            {
                int candidate = (from + step) % devices.Length;
                if (!LooksLikeInfrared(devices[candidate].name)) return candidate;
            }
            return (from + 1) % Mathf.Max(1, devices.Length);
        }
    }
}
