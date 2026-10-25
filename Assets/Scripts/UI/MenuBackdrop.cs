using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using LucillesJourney.Core;

namespace LucillesJourney.UI
{
    /// <summary>Marks a CanvasScaler that must keep its fixed reference resolution when the text-size setting zooms the interface.</summary>
    public class NoInterfaceScale : MonoBehaviour { }

    public enum BackdropKind { Menu, Ending }

    [Serializable] public class AnchorPoint { public int x, y; }
    [Serializable] public class AnchorRect { public int x, y, w, h; }
    [Serializable] public class BirdPath { public int x0, y0, x1, y1; }

    [Serializable]
    public class MenuAnchorData
    {
        public int title_x, title_y;
        public AnchorPoint[] windowGlow = new AnchorPoint[0];
        public AnchorPoint chimney = new AnchorPoint { x = 330, y = 110 };
        public AnchorRect fireflyRect = new AnchorRect { x = 20, y = 130, w = 400, h = 70 };
    }

    [Serializable]
    public class EndingAnchorData
    {
        public int figuresX = 224, figuresY = 200, sunStartX = 192, sunStartY = 130, sunEndY = 90;
        public AnchorRect dustRect = new AnchorRect { x = 0, y = 60, w = 384, h = 150 };
        public BirdPath birdPath = new BirdPath { x0 = -10, y0 = 70, x1 = 394, y1 = 40 };
    }

    [Serializable]
    public class MenuAnchorFile
    {
        public MenuAnchorData menu = new MenuAnchorData();
        public EndingAnchorData ending = new EndingAnchorData();
    }

    /// <summary>
    /// The animated pixel-art backdrop behind the main menu (a dusk garden at Lucille's home) and the ending (a quiet
    /// dawn road). It builds its own full-screen canvas behind the scene's interface from layered sprites in
    /// Resources/Menu: a gentle mouse parallax, twinkling stars, drifting clouds, pulsing window glow, fireflies and
    /// chimney smoke on the menu; a slowly rising sun, birds, drifting dust and the walking family on the ending.
    /// Every effect stands still when Reduced Motion is on. Missing layers are skipped, so it never breaks a scene.
    /// </summary>
    public class MenuBackdrop : MonoBehaviour
    {
        public const int S = 5; // screen pixels per art pixel at the 1920x1080 reference
        private const float StageW = 1920f, StageH = 1080f;

        public static MenuBackdrop Create(BackdropKind kind)
        {
            var go = new GameObject("Backdrop_" + kind);
            var backdrop = go.AddComponent<MenuBackdrop>();
            backdrop.Build(kind);
            return backdrop;
        }

        private sealed class ParallaxLayer { public RectTransform Rect; public float Strength; }

        private BackdropKind kind;
        private Canvas canvas;
        private RectTransform stage;
        private MenuAnchorFile anchors = new MenuAnchorFile();
        private readonly List<ParallaxLayer> parallax = new List<ParallaxLayer>();
        private readonly HashSet<string> warned = new HashSet<string>();
        private Vector2 look;

        // Menu
        private Image starsTop;
        private float nextTwinkle;
        private Image sunGlow, windowGlow;
        private RectTransform clouds;
        private float cloudWidth;
        private RectTransform grass;
        private Image[] fireflyDots, fireflyHalos;
        private float[] fireflySeed;
        private AnchorRect fireflyArea;
        private RectTransform groundLayer;
        private RectTransform smokeRoot;
        private readonly List<(Image img, float born)> smoke = new List<(Image, float)>();
        private float nextSmoke;

        // Ending
        private RectTransform sunRect;
        private Vector2 sunStart, sunEnd;
        private Image figures;
        private Sprite[] figureFrames;
        private Image bird;
        private Sprite[] birdFrames;
        private BirdPath birdPath;
        private float birdStart = 4f;
        private Image[] dust;
        private float[] dustSeed;
        private AnchorRect dustArea;
        private RectTransform dustRoot;
        private float born;

        private bool Reduced => SettingsManager.ReducedMotion;

        // --- Build ----------------------------------------------------------------------------------------

        private void Build(BackdropKind backdropKind)
        {
            kind = backdropKind;
            born = Time.unscaledTime;
            var anchorAsset = Resources.Load<TextAsset>("Menu/menu_anchors");
            if (anchorAsset != null)
            {
                try { anchors = JsonUtility.FromJson<MenuAnchorFile>(anchorAsset.text) ?? anchors; }
                catch (Exception e) { Debug.LogWarning("MenuBackdrop: could not read menu_anchors.json. " + e.Message); }
            }

            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -100; // behind every scene-built canvas
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(StageW, StageH);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<NoInterfaceScale>();

            var stageGo = new GameObject("Stage", typeof(RectTransform));
            stageGo.transform.SetParent(transform, false);
            stage = (RectTransform)stageGo.transform;
            stage.anchorMin = stage.anchorMax = new Vector2(0.5f, 0.5f);
            stage.sizeDelta = new Vector2(StageW, StageH);

            if (kind == BackdropKind.Menu) BuildMenu(); else BuildEnding();
            Update();
        }

        private Sprite Load(string name)
        {
            var sprite = Resources.Load<Sprite>("Menu/" + name);
            if (sprite == null && warned.Add(name)) Debug.LogWarning($"MenuBackdrop: missing sprite 'Resources/Menu/{name}'.");
            return sprite;
        }

        private Image AddImage(Transform parent, string name, Sprite sprite, Vector2 pivot)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = pivot;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            if (sprite != null) rt.sizeDelta = sprite.rect.size * S;
            return img;
        }

        private RectTransform AddLayer(string spriteName, float parallaxStrength)
        {
            var sprite = Load(spriteName);
            if (sprite == null) return null;
            var img = AddImage(stage, spriteName, sprite, new Vector2(0.5f, 0.5f));
            var rt = img.rectTransform;
            if (parallaxStrength > 0f) parallax.Add(new ParallaxLayer { Rect = rt, Strength = parallaxStrength });
            return rt;
        }

        /// <summary>Position inside a layer from art pixels (origin top-left of that layer's image).</summary>
        private static Vector2 InLayer(RectTransform layer, float px, float py)
        {
            var size = layer.sizeDelta / S;
            return new Vector2((px - size.x * 0.5f) * S, (size.y * 0.5f - py) * S);
        }

        private void BuildMenu()
        {
            AddLayer("menu_sky", 0f);
            var s0 = AddLayer("menu_stars_0", 0f);
            var s1 = AddLayer("menu_stars_1", 0f);
            starsTop = s1 != null ? s1.GetComponent<Image>() : null;
            var glow = AddLayer("menu_sun_glow", 1.5f);
            sunGlow = glow != null ? glow.GetComponent<Image>() : null;

            BuildClouds("menu_clouds", 2.2f, 11f);

            AddLayer("menu_hills_far", 3f);
            AddLayer("menu_hills_mid", 7f);
            groundLayer = AddLayer("menu_ground", 12f);

            if (groundLayer != null)
            {
                var wg = Load("menu_window_glow");
                if (wg != null)
                {
                    windowGlow = AddImage(groundLayer, "WindowGlow", wg, new Vector2(0.5f, 0.5f));
                    windowGlow.rectTransform.anchoredPosition = Vector2.zero;
                }
                BuildFireflies();
                var smokeGo = new GameObject("Smoke", typeof(RectTransform));
                smokeGo.transform.SetParent(groundLayer, false);
                smokeRoot = (RectTransform)smokeGo.transform;
                smokeRoot.anchorMin = smokeRoot.anchorMax = new Vector2(0.5f, 0.5f);
            }

            var g = Load("menu_foreground_grass");
            if (g != null)
            {
                var img = AddImage(stage, "ForegroundGrass", g, new Vector2(0.5f, 0f));
                grass = img.rectTransform;
                grass.anchoredPosition = new Vector2(0f, -StageH * 0.5f);
                parallax.Add(new ParallaxLayer { Rect = grass, Strength = 16f });
            }

            AddVignette();
        }

        private void BuildEnding()
        {
            var sky = AddLayer("ending_sky", 0f);

            var sunSprite = Load("ending_sun");
            if (sunSprite != null && sky != null)
            {
                var sun = AddImage(sky, "Sun", sunSprite, new Vector2(0.5f, 0.5f));
                sunRect = sun.rectTransform;
                var e = anchors.ending;
                sunStart = InLayer(sky, e.sunStartX, e.sunStartY);
                sunEnd = InLayer(sky, e.sunStartX, e.sunEndY);
                sunRect.anchoredPosition = sunStart;
            }

            BuildClouds("ending_clouds", 1.5f, 8f);

            birdFrames = new[] { Load("ending_bird_0"), Load("ending_bird_1") };
            if (birdFrames[0] != null && sky != null)
            {
                bird = AddImage(sky, "Bird", birdFrames[0], new Vector2(0.5f, 0.5f));
                bird.enabled = false;
                birdPath = anchors.ending.birdPath;
                birdFrames = new[] { birdFrames[0], birdFrames[1] ?? birdFrames[0] };
            }

            var glow = AddLayer("ending_glow", 1.5f);
            sunGlow = glow != null ? glow.GetComponent<Image>() : null;

            AddLayer("ending_hills_far", 3f);
            AddLayer("ending_hills_mid", 7f);
            var road = AddLayer("ending_road", 12f);

            figureFrames = new[] { Load("ending_figures_0"), Load("ending_figures_1") };
            if (road != null && figureFrames[0] != null)
            {
                figures = AddImage(road, "Family", figureFrames[0], new Vector2(0.5f, 0f));
                figures.rectTransform.anchoredPosition = InLayer(road, anchors.ending.figuresX, anchors.ending.figuresY);
                figureFrames[1] = figureFrames[1] ?? figureFrames[0];
            }

            BuildDust(road);
            AddVignette();
        }

        private void BuildClouds(string spriteName, float speedUnused, float yArtPx)
        {
            var sprite = Load(spriteName);
            if (sprite == null) return;
            var holder = new GameObject("Clouds", typeof(RectTransform));
            holder.transform.SetParent(stage, false);
            clouds = (RectTransform)holder.transform;
            clouds.anchorMin = clouds.anchorMax = new Vector2(0.5f, 0.5f);
            clouds.anchoredPosition = new Vector2(0f, (StageH * 0.5f) - (yArtPx + sprite.rect.height * 0.5f) * S);
            cloudWidth = sprite.rect.width * S;
            for (int i = 0; i < 2; i++)
            {
                var img = AddImage(clouds, "Cloud" + i, sprite, new Vector2(0.5f, 0.5f));
                img.rectTransform.anchoredPosition = new Vector2(i * cloudWidth, 0f);
            }
        }

        private void BuildFireflies()
        {
            var dot = Load("menu_firefly");
            var halo = Load("menu_firefly_glow");
            if (dot == null || groundLayer == null) return;

            const int count = 16;
            fireflyDots = new Image[count];
            fireflyHalos = new Image[count];
            fireflySeed = new float[count];
            fireflyArea = anchors.menu.fireflyRect;
            var rng = new System.Random(7);
            for (int i = 0; i < count; i++)
            {
                fireflySeed[i] = (float)rng.NextDouble() * 100f;
                if (halo != null) fireflyHalos[i] = AddImage(groundLayer, "FireflyGlow" + i, halo, new Vector2(0.5f, 0.5f));
                fireflyDots[i] = AddImage(groundLayer, "Firefly" + i, dot, new Vector2(0.5f, 0.5f));
            }
        }

        private void BuildDust(RectTransform road)
        {
            var small = Load("ending_dust");
            var big = Load("ending_dust_big");
            if (small == null || road == null) return;

            var holder = new GameObject("Dust", typeof(RectTransform));
            holder.transform.SetParent(road, false);
            dustRoot = (RectTransform)holder.transform;
            dustRoot.anchorMin = dustRoot.anchorMax = new Vector2(0.5f, 0.5f);

            const int count = 28;
            dust = new Image[count];
            dustSeed = new float[count];
            dustArea = anchors.ending.dustRect;
            var rng = new System.Random(11);
            for (int i = 0; i < count; i++)
            {
                dustSeed[i] = (float)rng.NextDouble() * 100f;
                dust[i] = AddImage(dustRoot, "Dust" + i, (i % 4 == 0 && big != null) ? big : small, new Vector2(0.5f, 0.5f));
            }
        }

        private void AddVignette()
        {
            var v = Load("menu_vignette");
            if (v == null) return;
            var img = AddImage(stage, "Vignette", v, new Vector2(0.5f, 0.5f));
            // Slightly oversized so it still covers the edges after the stage is scaled to "cover" the screen.
            img.rectTransform.sizeDelta = new Vector2(StageW, StageH) * 1.02f;
        }

        // --- Update -----------------------------------------------------------------------------------------------

        private void Update()
        {
            FitStage();
            float t = Time.unscaledTime;
            UpdateParallax(t);
            if (kind == BackdropKind.Menu) UpdateMenu(t); else UpdateEnding(t);
            UpdateClouds(t);
        }

        // The artwork is 16:9; scale it to cover the screen at any aspect ratio.
        private void FitStage()
        {
            var rect = ((RectTransform)transform).rect;
            if (rect.width <= 0f || rect.height <= 0f) return;
            float scale = Mathf.Max(rect.width / StageW, rect.height / StageH);
            stage.localScale = new Vector3(scale, scale, 1f);
        }

        private void UpdateParallax(float t)
        {
            Vector2 target = Vector2.zero;
            if (!Reduced)
            {
                Vector2 mouse = Vector2.zero;
                if (Pointer.current != null && Screen.width > 0 && Screen.height > 0)
                {
                    var p = Pointer.current.position.ReadValue();
                    mouse = new Vector2(Mathf.Clamp(p.x / Screen.width * 2f - 1f, -1f, 1f), Mathf.Clamp(p.y / Screen.height * 2f - 1f, -1f, 1f));
                }
                target = new Vector2(mouse.x * 0.7f + Mathf.Sin(t * 0.13f) * 0.3f, mouse.y * 0.5f);
            }
            look = Vector2.Lerp(look, target, 1f - Mathf.Exp(-3f * Time.unscaledDeltaTime));

            foreach (var layer in parallax)
            {
                if (layer.Rect == null) continue;
                // Whole art pixels give the stepped, retro feel and keep the pixels crisp.
                float x = Mathf.Round(-look.x * layer.Strength) * S;
                float y = Mathf.Round(-look.y * layer.Strength * 0.25f) * S;
                var basePos = layer.Rect == grass ? new Vector2(0f, -StageH * 0.5f) : Vector2.zero;
                layer.Rect.anchoredPosition = basePos + new Vector2(x, y);
            }
        }

        private void UpdateClouds(float t)
        {
            if (clouds == null || cloudWidth <= 0f) return;
            float offset = Reduced ? 0f : (t * 3f * S) % cloudWidth; // 3 art px per second
            clouds.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(-offset, 0f);
            clouds.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(cloudWidth - offset, 0f);
        }

        private void UpdateMenu(float t)
        {
            if (starsTop != null)
            {
                if (Reduced) starsTop.enabled = true;
                else if (t >= nextTwinkle)
                {
                    starsTop.enabled = !starsTop.enabled;
                    nextTwinkle = t + UnityEngine.Random.Range(0.6f, 1.6f);
                }
            }

            if (sunGlow != null) SetAlpha(sunGlow, Reduced ? 1f : 0.82f + 0.18f * Mathf.Sin(t * 0.5f));
            if (windowGlow != null)
            {
                // A slow warm flicker, in a few steps like a candle.
                float flicker = Reduced ? 1f : 0.78f + 0.22f * Mathf.Round((Mathf.Sin(t * 1.7f) * 0.5f + Mathf.Sin(t * 3.1f + 1f) * 0.5f + 1f) * 1.5f) / 3f;
                SetAlpha(windowGlow, flicker);
            }

            if (fireflyDots != null)
            {
                for (int i = 0; i < fireflyDots.Length; i++)
                {
                    float s = fireflySeed[i];
                    float x = fireflyArea.x + Mathf.Repeat(s * 13.7f, fireflyArea.w) + (Reduced ? 0f : Mathf.Sin(t * 0.31f + s) * 14f);
                    float y = fireflyArea.y + Mathf.Repeat(s * 7.3f, fireflyArea.h) + (Reduced ? 0f : Mathf.Cos(t * 0.27f + s * 1.3f) * 7f);
                    var pos = InLayer(groundLayer, Mathf.Round(x), Mathf.Round(y));
                    fireflyDots[i].rectTransform.anchoredPosition = pos;
                    bool lit = Reduced || Mathf.Sin(t * 0.9f + s * 2f) > -0.2f;
                    fireflyDots[i].enabled = lit;
                    if (fireflyHalos[i] != null)
                    {
                        fireflyHalos[i].rectTransform.anchoredPosition = pos;
                        fireflyHalos[i].enabled = lit && Mathf.Sin(t * 0.9f + s * 2f) > 0.5f;
                    }
                }
            }

            UpdateSmoke(t);
        }

        private void UpdateSmoke(float t)
        {
            if (smokeRoot == null || groundLayer == null) return;
            if (Reduced)
            {
                foreach (var puff in smoke) if (puff.img != null) Destroy(puff.img.gameObject);
                smoke.Clear();
                return;
            }

            if (t >= nextSmoke)
            {
                nextSmoke = t + 1.1f;
                var go = new GameObject("Puff", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(smokeRoot, false);
                var img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.color = new Color(0.78f, 0.72f, 0.78f, 0.5f);
                ((RectTransform)go.transform).sizeDelta = new Vector2(3, 3) * S;
                smoke.Add((img, t));
            }

            for (int i = smoke.Count - 1; i >= 0; i--)
            {
                var (img, bornAt) = smoke[i];
                float age = t - bornAt;
                if (age > 7f || img == null)
                {
                    if (img != null) Destroy(img.gameObject);
                    smoke.RemoveAt(i);
                    continue;
                }
                var c = anchors.menu.chimney;
                float px = c.x + age * 2.2f + Mathf.Sin(age * 0.9f) * 2f;
                float py = c.y - age * 5.5f;
                int size = age < 2f ? 3 : (age < 4f ? 4 : 5);
                var rt = (RectTransform)img.transform;
                rt.sizeDelta = new Vector2(size, size) * S;
                rt.anchoredPosition = InLayer(groundLayer, Mathf.Round(px), Mathf.Round(py));
                var col = img.color;
                col.a = 0.5f * (1f - age / 7f);
                img.color = col;
            }
        }

        private void UpdateEnding(float t)
        {
            float age = t - born;

            if (sunRect != null)
            {
                float k = Reduced ? 1f : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / 40f));
                sunRect.anchoredPosition = Vector2.Lerp(sunStart, sunEnd, k);
            }
            if (sunGlow != null) SetAlpha(sunGlow, Reduced ? 1f : 0.8f + 0.2f * Mathf.Sin(t * 0.45f));

            if (figures != null)
            {
                int frame = Reduced ? 0 : Mathf.FloorToInt(t / 0.32f) % 2;
                figures.sprite = figureFrames[frame];
            }

            if (bird != null)
            {
                float cycle = 16f;
                float local = (age - birdStart) % cycle;
                float duration = 11f;
                bool flying = !Reduced && age >= birdStart && local >= 0f && local <= duration;
                bird.enabled = flying;
                if (flying)
                {
                    float k = local / duration;
                    float x = Mathf.Lerp(birdPath.x0, birdPath.x1, k);
                    float y = Mathf.Lerp(birdPath.y0, birdPath.y1, k) + Mathf.Sin(k * 14f) * 2f;
                    var sky = bird.transform.parent as RectTransform;
                    bird.rectTransform.anchoredPosition = InLayer(sky, Mathf.Round(x), Mathf.Round(y));
                    bird.sprite = birdFrames[Mathf.FloorToInt(t * 6f) % 2];
                }
            }

            if (dust != null)
            {
                var road = dustRoot.parent as RectTransform;
                for (int i = 0; i < dust.Length; i++)
                {
                    float s = dustSeed[i];
                    float rise = Reduced ? 0f : (t * (1.2f + (i % 3) * 0.5f) + s * 5f) % dustArea.h;
                    float x = dustArea.x + Mathf.Repeat(s * 17.3f, dustArea.w) + (Reduced ? 0f : Mathf.Sin(t * 0.4f + s) * 6f);
                    float y = dustArea.y + dustArea.h - rise;
                    dust[i].rectTransform.anchoredPosition = InLayer(road, Mathf.Round(x), Mathf.Round(y));
                    float edge = Mathf.Min(rise, dustArea.h - rise) / 12f;
                    SetAlpha(dust[i], Mathf.Clamp01(edge) * 0.8f);
                }
            }
        }

        private static void SetAlpha(Graphic g, float a)
        {
            var c = g.color;
            c.a = Mathf.Clamp01(a);
            g.color = c;
        }

        public int LayerCount => stage != null ? stage.childCount : 0;
    }
}
