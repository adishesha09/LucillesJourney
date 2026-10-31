using System;
using UnityEngine;

namespace LucillesJourney.AR
{
    /// <summary>
    /// Estimates how the camera moved between two webcam frames, using nothing but the video (no sensors, no packages).
    ///
    /// Each frame is shrunk to 160 px wide and turned to grey. A grid of small patches from the previous frame is
    /// searched for in the new frame (block matching, refined to sub-pixel), giving a field of little arrows. A
    /// similarity transform (shift + zoom + twist) is fitted to those arrows, ignoring outliers such as a moving hand.
    ///
    /// This is a 2D approximation of camera motion: panning/tilting shifts the picture, moving closer zooms it. It is not
    /// 3D tracking - no depth, no correct perspective - it drifts slowly, and it gives up (returns false) on blank walls,
    /// darkness or very fast motion.
    /// </summary>
    public sealed class WebcamMotionTracker : IDisposable
    {
        public const int Width = 160;

        private const int Half = 6;                 // patches are 12 x 12
        private const int Radius = 7;               // search +-7 px (about +-4% of the frame per camera frame)
        private const int Cols = 7, Rows = 4;
        private const int Margin = Half + Radius + 1;
        private const float MinTexture = 3f;        // mean deviation a patch needs to be trackable

        private int height;
        private RenderTexture mid, small;
        private Texture2D readback;
        private byte[] prev, cur;
        private bool havePrev, lastFlipX, lastFlipY;

        private readonly float[] px = new float[Cols * Rows], py = new float[Cols * Rows];
        private readonly float[] fx = new float[Cols * Rows], fy = new float[Cols * Rows];
        private readonly float[] residual = new float[Cols * Rows], sorted = new float[Cols * Rows];
        private readonly bool[] keep = new bool[Cols * Rows];
        private readonly int[] sad = new int[(2 * Radius + 1) * (2 * Radius + 1)];

        public void Reset() => havePrev = false;

        /// <summary>
        /// Compares the camera's current frame with the last one. Returns true with a transform
        /// p' = [a -b; b a] p + t (p in pixels of the 160-wide frame, from its centre, y up) describing how the picture moved.
        /// </summary>
        public bool Step(WebCamTexture cam, bool flipX, bool flipY, out float a, out float b, out Vector2 t)
        {
            a = 1f; b = 0f; t = Vector2.zero;
            if (cam == null || !cam.isPlaying || cam.width < 100) return false;

            EnsureBuffers(cam);
            if (flipX != lastFlipX || flipY != lastFlipY) { havePrev = false; lastFlipX = flipX; lastFlipY = flipY; }

            Capture(cam, flipX, flipY);
            bool ok = havePrev && Estimate(out a, out b, out t);

            var swap = prev; prev = cur; cur = swap;
            havePrev = true;
            return ok;
        }

        // -------------------------------------------------------------------------------------------------------------

        private void EnsureBuffers(WebCamTexture cam)
        {
            int h = Mathf.Clamp(Mathf.RoundToInt(Width * (float)cam.height / cam.width), 36, 240);
            if (small != null && h == height) return;

            Dispose();
            height = h;
            mid = new RenderTexture(Width * 2, height * 2, 0, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Bilinear };
            small = new RenderTexture(Width, height, 0, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Bilinear };
            readback = new Texture2D(Width, height, TextureFormat.RGBA32, false);
            prev = new byte[Width * height];
            cur = new byte[Width * height];
            havePrev = false;
        }

        // Two-step shrink (camera -> 320 -> 160) so every output pixel averages its neighbourhood instead of aliasing.
        private void Capture(WebCamTexture cam, bool flipX, bool flipY)
        {
            Graphics.Blit(cam, mid, new Vector2(flipX ? -1f : 1f, flipY ? -1f : 1f), new Vector2(flipX ? 1f : 0f, flipY ? 1f : 0f));
            Graphics.Blit(mid, small);

            var active = RenderTexture.active;
            RenderTexture.active = small;
            readback.ReadPixels(new Rect(0, 0, Width, height), 0, 0, false);
            RenderTexture.active = active;

            var raw = readback.GetRawTextureData<Color32>();
            for (int i = 0; i < cur.Length; i++)
            {
                Color32 c = raw[i];
                cur[i] = (byte)((c.r * 77 + c.g * 150 + c.b * 29) >> 8);
            }
        }

        private bool Estimate(out float a, out float b, out Vector2 t)
        {
            a = 1f; b = 0f; t = Vector2.zero;
            float cx0 = Width * 0.5f, cy0 = height * 0.5f;
            int n = 0;
            for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
            {
                int cx = Margin + c * (Width - 2 * Margin) / (Cols - 1);
                int cy = Margin + r * (height - 2 * Margin) / (Rows - 1);
                if (!Flow(cx, cy, out float dx, out float dy)) continue;
                px[n] = cx - cx0; py[n] = cy - cy0; fx[n] = dx; fy[n] = dy;
                n++;
            }
            return n >= 6 && Fit(n, out a, out b, out t);
        }

        // Where did the patch around (cx, cy) in the previous frame go in the current one?
        private bool Flow(int cx, int cy, out float dx, out float dy)
        {
            dx = dy = 0f;
            int x0 = cx - Half, y0 = cy - Half, size = Half * 2;

            int sum = 0;
            for (int y = 0; y < size; y++)
            {
                int row = (y0 + y) * Width + x0;
                for (int x = 0; x < size; x++) sum += prev[row + x];
            }
            int mean = sum / (size * size), deviation = 0;
            for (int y = 0; y < size; y++)
            {
                int row = (y0 + y) * Width + x0;
                for (int x = 0; x < size; x++) deviation += Mathf.Abs(prev[row + x] - mean);
            }
            if (deviation / (float)(size * size) < MinTexture) return false; // blank wall: nothing to follow

            int span = 2 * Radius + 1, best = int.MaxValue, bestIndex = 0;
            long total = 0;
            for (int oy = -Radius; oy <= Radius; oy++)
            for (int ox = -Radius; ox <= Radius; ox++)
            {
                int s = 0;
                for (int y = 0; y < size; y++)
                {
                    int p = (y0 + y) * Width + x0;
                    int q = (y0 + y + oy) * Width + x0 + ox;
                    for (int x = 0; x < size; x++) s += Mathf.Abs(prev[p + x] - cur[q + x]);
                }
                int index = (oy + Radius) * span + (ox + Radius);
                sad[index] = s;
                total += s;
                if (s < best) { best = s; bestIndex = index; }
            }
            if (best > 0.85f * total / sad.Length) return false; // no clear winner (repeating or smooth texture)

            int bx = bestIndex % span, by = bestIndex / span;
            dx = bx - Radius + SubPixel(bx > 0 && bx < span - 1, sad[bestIndex - 1], best, sad[bestIndex + 1]);
            dy = by - Radius + SubPixel(by > 0 && by < span - 1, sad[bestIndex - span], best, sad[bestIndex + span]);
            return true;
        }

        private static float SubPixel(bool inside, int before, int centre, int after)
        {
            if (!inside) return 0f;
            float denominator = before - 2f * centre + after;
            return denominator > 0f ? Mathf.Clamp(0.5f * (before - after) / denominator, -0.5f, 0.5f) : 0f;
        }

        // Least-squares shift + zoom + twist through the arrows, dropping the ones that disagree (a hand, a pet...).
        private bool Fit(int n, out float a, out float b, out Vector2 t)
        {
            a = 1f; b = 0f; t = Vector2.zero;
            for (int i = 0; i < n; i++) keep[i] = true;

            for (int pass = 0; pass < 2; pass++)
            {
                int m = 0;
                double pmx = 0, pmy = 0, qmx = 0, qmy = 0;
                for (int i = 0; i < n; i++)
                {
                    if (!keep[i]) continue;
                    m++; pmx += px[i]; pmy += py[i]; qmx += px[i] + fx[i]; qmy += py[i] + fy[i];
                }
                if (m < 5) return false;
                pmx /= m; pmy /= m; qmx /= m; qmy /= m;

                double numA = 0, numB = 0, den = 0;
                for (int i = 0; i < n; i++)
                {
                    if (!keep[i]) continue;
                    double ux = px[i] - pmx, uy = py[i] - pmy, vx = px[i] + fx[i] - qmx, vy = py[i] + fy[i] - qmy;
                    numA += ux * vx + uy * vy;
                    numB += ux * vy - uy * vx;
                    den += ux * ux + uy * uy;
                }
                if (den < 1e-3) return false;

                a = (float)(numA / den);
                b = (float)(numB / den);
                t = new Vector2((float)(qmx - (a * pmx - b * pmy)), (float)(qmy - (b * pmx + a * pmy)));
                if (pass == 1) break;

                for (int i = 0; i < n; i++)
                {
                    float qx = a * px[i] - b * py[i] + t.x, qy = b * px[i] + a * py[i] + t.y;
                    residual[i] = Mathf.Sqrt((px[i] + fx[i] - qx) * (px[i] + fx[i] - qx) + (py[i] + fy[i] - qy) * (py[i] + fy[i] - qy));
                    sorted[i] = residual[i];
                }
                Array.Sort(sorted, 0, n);
                float threshold = Mathf.Max(0.6f, 2f * sorted[n / 2]);
                for (int i = 0; i < n; i++) keep[i] = residual[i] <= threshold;
            }

            float scale = Mathf.Sqrt(a * a + b * b);
            return scale > 0.9f && scale < 1.1f; // a jump bigger than 10% in one frame is a glitch, not camera motion
        }

        public void Dispose()
        {
            if (mid != null) { mid.Release(); UnityEngine.Object.Destroy(mid); mid = null; }
            if (small != null) { small.Release(); UnityEngine.Object.Destroy(small); small = null; }
            if (readback != null) { UnityEngine.Object.Destroy(readback); readback = null; }
        }
    }
}
