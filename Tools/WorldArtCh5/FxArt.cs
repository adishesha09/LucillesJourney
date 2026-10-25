namespace WorldArtCh5;

/// <summary>Animated effect frames: flames, smoke, water glints, washing lines, reeds.</summary>
public static class FxArt
{
    static readonly Rgba F0 = Rgba.Hex("C9452B"), F1 = Rgba.Hex("E2712F"), F2 = Rgba.Hex("F2A844"), F3 = Rgba.Hex("F8D56E"), F4 = Rgba.Hex("FCF0B8");

    public static void Build()
    {
        S.Cur = "fx";
        for (int f = 0; f < 3; f++)
        {
            S.Add($"fx_c5_flame_s_{f}", Flame(9, 12, f, 1));
            S.Add($"fx_c5_flame_l_{f}", Flame(15, 19, f, 2));
            S.Add($"fx_c5_glint_a_{f}", Glint(22, 8, f, 1));
            S.Add($"fx_c5_glint_b_{f}", Glint(14, 6, f, 2));
        }
        for (int f = 0; f < 4; f++) { S.Add($"fx_c5_smoke_{f}", Smoke(18, 30, f, 1)); S.Add($"fx_c5_smoke_l_{f}", Smoke(24, 38, f, 2)); }
        for (int f = 0; f < 2; f++)
        {
            S.Add($"fx_c5_line_a_{f}", Line(f, 0));
            S.Add($"fx_c5_line_b_{f}", Line(f, 1));
            S.Add($"fx_c5_reeds_a_{f}", Reeds(14, 24, f, 1));
            S.Add($"fx_c5_reeds_b_{f}", Reeds(10, 18, f, 2));
        }
    }

    static Canvas Flame(int w, int h, int frame, int seed)
    {
        var c = new Canvas(w, h);
        double cx = (w - 1) / 2.0;
        for (int row = 0; row < h; row++)
        {
            double t = (h - 1 - row) / (double)(h - 1);                  // 0 bottom .. 1 top
            double wob = Math.Sin(frame * 2.1 + t * 4.0 + seed) * 1.2 * t;
            double hw = (w / 2.0) * (1 - Math.Pow(t, 1.5)) * (0.92 + 0.16 * Math.Sin(frame * 1.7 + t * 6 + seed * 2));
            if (t > 0.88) hw *= 0.5;
            for (int x = 0; x < w; x++)
            {
                double dx = Math.Abs(x + 0.5 - cx - wob);
                if (dx > hw) continue;
                double n = dx / Math.Max(0.5, hw);
                Rgba col;
                if (t < 0.45 && n < 0.35) col = F4;
                else if (n < 0.45 && t < 0.7) col = F3;
                else if (n < 0.75) col = F2;
                else col = t > 0.5 ? F0 : F1;
                if (n > 0.92) col = F0;
                c.Set(x, row, col);
            }
        }
        // bottom row must exist
        for (int x = 1; x < w - 1; x++) if (!c.Opaque(x, h - 1)) c.Set(x, h - 1, F1);
        // loose flicker tip
        c.Set((int)(cx + (frame - 1)), 0, F2);
        return c;
    }

    static Canvas Smoke(int w, int h, int frame, int seed)
    {
        var c = new Canvas(w, h);
        var tones = new[] { Rgba.Hex("B3A6A4"), Rgba.Hex("CCC1BA"), Rgba.Hex("E0D8CC"), Rgba.Hex("F0EADF") };
        const int n = 6;
        for (int k = 0; k < n; k++)
        {
            double phase = (k / (double)n + frame / (4.0 * n)) % 1.0;       // 0 bottom .. 1 top
            double y = h - 3 - phase * (h - 6);
            double r = 1.5 + phase * (w / 3.3);
            double x = w / 2.0 + Math.Sin(phase * 4.2 + seed) * (0.8 + phase * 3.4);
            for (int py = (int)(y - r - 1); py <= y + r + 1; py++)
                for (int px = (int)(x - r - 1); px <= x + r + 1; px++)
                {
                    double d = Math.Sqrt((px + .5 - x) * (px + .5 - x) + (py + .5 - y) * (py + .5 - y));
                    if (d > r) continue;
                    // dissolve towards the top with irregular specks rather than a regular grid
                    if (phase > 0.45 && Fx.R(px, py, 77) > 1.5 - phase * 1.2) continue;
                    double light = -(((px + .5 - x) * 0.6 + (py + .5 - y) * 0.8) / r);
                    int idx = Math.Clamp((int)((0.5 + 0.4 * light + 0.15 * phase) * 4), 0, 3);
                    c.Set(px, py, tones[idx]);
                }
        }
        return c;
    }

    static Canvas Glint(int w, int h, int frame, int seed)
    {
        var c = new Canvas(w, h);
        var hi = Rgba.Hex("E8F6EC"); var mid = Rgba.Hex("BFE3DB"); var lo = Rgba.Hex("8CC3C2");
        int n = 3;
        for (int i = 0; i < n; i++)
        {
            double ph = (frame + i) % 3;
            int len = 3 + (int)(Fx.R(i, seed, 61) * (w / 4.0));
            int x = 1 + (int)(Fx.R(i, 1 + seed, 62) * (w - len - 3)), y = 1 + (i * (h - 2) / n) + (int)(Fx.R(i, 3, 63) * 1.5);
            // glints creep and brighten/fade over the 3 frames
            x = Math.Min(w - len - 1, x + (int)ph);
            if (ph == 0) { c.HL(x, y, len, mid); }
            else if (ph == 1) { c.HL(x, y, len, hi); c.HL(x + 1, y + 1, Math.Max(1, len - 2), lo); }
            else { c.HL(x + 1, y, Math.Max(1, len - 2), mid); c.Set(x + len / 2, y - 1, hi); }
        }
        return c;
    }

    static Canvas Reeds(int w, int h, int frame, int seed)
    {
        var c = new Canvas(w, h);
        int n = 4 + w / 4;
        for (int i = 0; i < n; i++)
        {
            int x0 = 1 + (int)(i * (w - 3) / (double)n) + (int)(Fx.R(i, 1, seed) * 2);
            int len = h - 2 - (int)(Fx.R(i, 2, seed) * (h * 0.45));
            int lean = (int)Math.Round((Fx.R(i, 3, seed) - 0.5) * 6) + (frame == 1 ? (i % 2 == 0 ? 1 : 2) : 0);
            var col = C.Fresh[1 + (i % 4)];
            for (int j = 0; j <= len; j++)
            {
                double t = j / (double)len;
                int x = x0 + (int)Math.Round(lean * t * t);
                c.Set(x, h - 1 - j, col);
                if (j > len * 0.6) c.Set(x + 1, h - 1 - j, C.Fresh[0]);
            }
            int tx = x0 + lean;
            if (i % 3 == 0) { c.Rect(tx, h - 1 - len - 2, 2, 3, Rgba.Hex("6A4A36")); c.Set(tx, h - 1 - len - 2, Rgba.Hex("8B6850")); }
        }
        for (int x = 0; x < w; x++) if ((x + seed) % 2 == 0) c.Set(x, h - 1, Rgba.Hex("4F6B3A"));
        return c;
    }

    // ---- washing line ---------------------------------------------------------------------------------------------------
    static Canvas Line(int frame, int variant)
    {
        int w = 88, h = 42;
        var c = new Canvas(w, h);
        int yb = h - 1;
        // ropes first
        int ry = 6;
        for (int x = 4; x < w - 4; x++) { int y = ry + (int)Math.Round(Math.Sin((x - 4) / (double)(w - 8) * Math.PI) * 2); c.Set(x, y, C.Rope); }
        void Cloth(int x, int w2, int h2, Rgba[] pal, int kind, int seed)
        {
            int yTop = ry + 2 + (int)Math.Round(Math.Sin((x - 4) / (double)(w - 8) * Math.PI) * 2);
            for (int y = 0; y < h2; y++)
            {
                int sh = frame == 1 ? (int)Math.Round(Math.Sin(y * 0.35 + seed) * 1.0 + y / 9.0) : (int)Math.Round(-Math.Sin(y * 0.35 + seed) * 0.5);
                int ww = w2;
                if (kind == 1) ww = y < h2 / 2 ? w2 : w2 - 2;          // shirt body
                int xo = x + sh;
                for (int i = 0; i < ww; i++)
                {
                    int idx = i < 2 ? 3 : i >= ww - 2 ? 1 : 2;
                    if (y == h2 - 1) idx = 0;
                    if (y == 0) idx = 3;
                    if ((i + y + seed) % 11 == 0 && y > 1) idx = Math.Max(0, idx - 1);
                    c.Set(xo + i, yTop + y, pal[idx]);
                }
                if (kind == 1 && y < 4) { c.Set(xo - 2, yTop + y, pal[1]); c.Set(xo - 1, yTop + y, pal[2]); c.Set(xo + ww, yTop + y, pal[1]); c.Set(xo + ww + 1, yTop + y, pal[0]); }   // sleeves
                if (kind == 2 && y > 8) { c.Set(xo + ww / 2, yTop + y, C.Out); }                                                                               // trouser gap
            }
            c.Set(x + 1, yTop - 1, C.Out); c.Set(x + w2 - 2, yTop - 1, C.Out);                                                                                 // pegs
        }
        if (variant == 0)
        {
            Cloth(10, 20, 22, C.Cream, 0, 1);
            Cloth(34, 12, 14, C.Blue, 1, 2);
            Cloth(50, 7, 16, C.Clay, 2, 3);
            Cloth(60, 7, 16, C.Rust, 2, 4);
            Cloth(72, 12, 11, C.Mustard, 0, 5);
        }
        else
        {
            Cloth(8, 14, 12, C.Teal, 1, 6);
            Cloth(26, 22, 26, C.Ochre, 0, 7);
            Cloth(54, 10, 18, C.Plum, 1, 8);
            Cloth(68, 14, 12, C.Cream, 0, 9);
        }
        c.SelOutline(0.4);
        // posts
        foreach (var px in new[] { 2, w - 5 })
            for (int y = 3; y <= yb; y++) { c.Set(px, y, K.Wood[3]); c.Set(px + 1, y, K.Wood[1]); }
        c.Rect(1, 3, 4, 1, K.Wood[4]); c.Rect(w - 6, 3, 4, 1, K.Wood[4]);
        c.Set(3, ry, C.RopeD); c.Set(w - 4, ry, C.RopeD);
        return c;
    }
}
