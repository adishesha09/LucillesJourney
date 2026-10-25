namespace WorldArtCh4;

/// <summary>Chapter 4 sprites: graded re-use of Chapter 1 art plus the handful of new props the leaving day needs.</summary>
public static class Art
{
    static Rgba H(string h) => Rgba.Hex(h);

    // cool grey dust-sheet ramp (graded again with everything else)
    static readonly Rgba[] Dust = { H("6A6470"), H("9A949C"), H("C4BFC2"), H("E1DDD9"), H("F7F3EC") };
    static readonly Rgba[] Canvas1 = { H("7A5E3C"), H("A5804F"), H("C49C64"), H("DDB97F"), H("EFD7A6") };

    // ------------------------------------------------------------------------------------------------------
    public static string Use(string src, Func<Canvas, Canvas>? tweak = null, string? name = null, bool grade = true)
    {
        string n = name ?? (src.StartsWith("prop_") ? "prop_c4_" + src[5..] : throw new Exception("name required for " + src));
        if (S.All.ContainsKey(n)) return n;
        var c = Program.Disk(src);
        if (tweak != null) c = tweak(c);
        S.Add(n, grade ? Look.Grade(c) : c);
        return n;
    }

    public static string Reg(string n, Canvas c, bool grade = true)
    {
        S.Add(n, grade ? Look.Grade(c) : c);
        return n;
    }

    static Canvas Lift(Canvas c, int pad)
    {
        var o = new Canvas(c.W, c.H + pad);
        o.Blit(c, 0, 0);
        return o;
    }

    static Canvas Map(Canvas c, Func<Rgba, Rgba> f)
    {
        var o = new Canvas(c.W, c.H);
        for (int y = 0; y < c.H; y++) for (int x = 0; x < c.W; x++) o.Set(x, y, f(c.Get(x, y)));
        return o;
    }

    public static Canvas Wilted(Canvas c) => Map(c, Look.Wilt);

    // ================================================ derived from Chapter 1 art ============================
    static Canvas DeskBare(Canvas s)
    {
        var o = s.Clone();
        for (int y = 1; y <= 14; y++)
            for (int x = 1; x < 41; x++)
            {
                if (y == 1) { o.Set(x, y, Rgba.Clear); continue; }
                var c = s.Get(1, y);
                if (y > 3 && Fx.R(x, y, 5) > 0.94) c = s.Get(1, 8);
                o.Set(x, y, c);
            }
        return o;
    }

    /// <summary>Region of the sideboard where the framed photograph stands (x 10..29, y 0..16).</summary>
    static Canvas Photo(Canvas s, bool keep)
    {
        var o = new Canvas(s.W, s.H);
        for (int y = 0; y < s.H; y++)
            for (int x = 0; x < s.W; x++)
            {
                bool inFrame = x >= 10 && x <= 29 && y <= 16;
                if (inFrame == keep) o.Set(x, y, s.Get(x, y));
            }
        return o;
    }

    static Canvas StoveCold(Canvas s)
    {
        var o = s.Clone();
        for (int y = 23; y <= 27; y++)
            for (int x = 8; x <= 15; x++)
            {
                bool rim = y == 23 || y == 27 || x == 8 || x == 15;
                o.Set(x, y, rim ? H("3A3036") : (Fx.R(x, y, 3) > 0.8 ? H("5E5660") : H("2E2A2E")));
            }
        return o;
    }

    /// <summary>The kitchen dresser after the family took what mattered: top shelf gone, the other shelves bare but for a stray cup and a cloth.</summary>
    static Canvas ShelfBare(Canvas s)
    {
        var o = new Canvas(s.W, s.H - 12);
        for (int y = 12; y < s.H; y++)
            for (int x = 0; x < s.W; x++) o.Set(x, y - 12, s.Get(x, y));
        Rgba Pat(int x) => (x == 2 || x == 7 || x == 12 || x == 22 || x == 27 || x == 32 || x == 37) ? s.Get(x, 4) : s.Get(3, 4);
        foreach (var (a, b) in new[] { (15, 20), (24, 29) })
            for (int y = a; y <= b; y++)
                for (int x = 3; x <= 41; x++) o.Set(x, y - 12, y == b ? H("2B2321") : Pat(x));
        // a stray cup, a folded cloth, a dusty jar lid
        o.Rect(31, 5, 4, 4, H("B8D2DC")); o.Rect(31, 5, 4, 1, H("F4EDDA")); o.Set(35, 6, H("8FB0C2")); o.Set(35, 7, H("8FB0C2"));
        o.Rect(9, 14, 9, 3, H("E8DEC4")); o.Rect(9, 14, 9, 1, H("FFFAEC")); o.Rect(9, 16, 9, 1, H("B8A88A"));
        o.Rect(24, 15, 4, 2, H("8E6A44"));
        return o;
    }

    public static Canvas TableBare(Canvas s)
    {
        var o = s.Clone();
        for (int y = 12; y <= 32; y++)
            for (int x = 1; x <= 46; x++)
            {
                var c = H("D09A5C");
                int seam = (y - 12) % 7;
                if (seam == 6) c = H("A0693C");
                else if (seam == 0) c = H("DDA865");
                else if (Fx.R(x / 6, y, 21) > 0.9 && Fx.R(x, y, 22) > 0.4) c = H("B87F47");
                int joint = (x + (y / 7) * 17) % 40;
                if (joint == 0 && seam != 6) c = H("A0693C");
                o.Set(x, y, c);
            }
        return o;
    }

    static Canvas Breathe(Canvas s)
    {
        var o = s.Clone();
        int cut = s.H - 7;
        for (int y = 1; y < cut; y++) for (int x = 0; x < s.W; x++) o.Set(x, y, s.Get(x, y - 1));
        for (int x = 0; x < s.W; x++) o.Set(x, 0, Rgba.Clear);
        return o;
    }

    /// <summary>Furniture under a dust sheet: the form of the piece stays readable (shading is kept) but every colour becomes grey cloth with folds and a hem.</summary>
    public static Canvas Sheeted(Canvas s, int yStart, int legRows)
    {
        var o = s.Clone();
        int yEnd = s.H - legRows;
        bool In(int x, int y) => y >= yStart && y < yEnd && s.Opaque(x, y);
        double Luma(int x, int y) { var q = s.Get(x, y); return 0.30 * q.R + 0.59 * q.G + 0.11 * q.B; }
        double Blur(int x, int y)
        {
            double sum = 0; int n = 0;
            for (int j = -2; j <= 2; j++) for (int i = -2; i <= 2; i++) if (In(x + i, y + j)) { sum += Luma(x + i, y + j); n++; }
            return n == 0 ? 128 : sum / n;
        }
        for (int y = yStart; y < yEnd; y++)
            for (int x = 0; x < s.W; x++)
            {
                if (!s.Opaque(x, y)) continue;
                double v = 1.5 + Math.Clamp((Blur(x, y) - 80) / 110.0, 0, 1) * 1.2
                           + 0.55 * (1 - (double)x / s.W) + 0.45 * (1 - (double)(y - yStart) / Math.Max(1, yEnd - yStart));
                bool rim = !In(x + 1, y) || !In(x, y + 1) || y == yEnd - 1;
                bool lit = !In(x - 1, y) || !In(x, y - 1);
                int fx = (x + (int)(Fx.R(x / 7, 0, 7) * 4)) % 7;
                if (fx == 0 && Fx.R(x, y / 4, 9) > 0.15) v -= 0.9;          // crease
                else if (fx == 1 && Fx.R(x, y / 4, 9) > 0.45) v += 0.6;     // lit side of the crease
                if (y >= yEnd - 3) v -= ((x + y) & 1) == 0 ? 0.9 : 0.4;       // hem shadow
                int idx = Math.Clamp((int)Math.Floor(v), 1, 4);
                if (rim) idx = 0;
                else if (lit) idx = 4;
                o.Set(x, y, Dust[idx]);
            }
        return o;
    }

    static Canvas LanternLit(int frame)
    {
        var src = Program.Disk("prop_lantern");
        var g = Look.Grade(src);
        var hot = frame == 0 ? new[] { H("FFE7A0"), H("FFF8DA"), H("F2C45E") } : new[] { H("F4D788"), H("FFEFC0"), H("E5B04C") };
        var from = new[] { H("FDEFB4"), H("FFF8D6"), H("F2D26B") };
        for (int y = 0; y < src.H; y++)
            for (int x = 0; x < src.W; x++)
            {
                var p = src.Get(x, y);
                for (int i = 0; i < 3; i++) if (p == from[i]) g.Set(x, y, hot[i]);
            }
        return g;
    }

    // ================================================ new drawings ==========================================
    static Canvas PapersBundle()
    {
        var c = new Canvas(20, 10);
        c.Rect(1, 2, 7, 2, Rgba.Hex("E8DEC4"));                                  // loose sheet peeking out
        c.Rect(2, 3, 16, 6, Rgba.Hex("F4EDDA"));                                  // top of the stack
        c.Rect(2, 7, 16, 2, Rgba.Hex("D4C7A8"));
        c.HL(2, 8, 16, Rgba.Hex("B8A88A"));
        c.HL(2, 7, 16, Rgba.Hex("E8DEC4"));
        for (int x = 3; x < 8; x += 2) c.Set(x, 4, Rgba.Hex("B8A88A"));          // lines of handwriting
        for (int x = 12; x < 17; x += 2) c.Set(x, 4, Rgba.Hex("B8A88A"));
        for (int x = 3; x < 8; x += 2) c.Set(x, 6, Rgba.Hex("B8A88A"));
        for (int x = 12; x < 16; x += 2) c.Set(x, 6, Rgba.Hex("B8A88A"));
        c.VL(9, 3, 6, K.Tan); c.VL(10, 3, 6, K.Brown);                            // twine
        c.Set(8, 2, K.Tan); c.Set(11, 2, K.Tan); c.Set(9, 2, K.Brown); c.Set(10, 2, K.Brown);
        c.Rect(14, 5, 2, 2, K.Terra);                                             // wax seal
        c.SelOutline(0.45);
        return c;
    }

    static Canvas FoodOnTable()
    {
        var c = new Canvas(24, 12);
        // cloth napkin
        c.Rect(1, 7, 22, 4, Rgba.Hex("F4EDDA"));
        c.HL(1, 8, 22, Rgba.Hex("B8D2DC")); c.HL(1, 7, 22, Rgba.Hex("FFFAEC")); c.HL(1, 10, 22, Rgba.Hex("D4C7A8"));
        // loaf
        c.Ellipse(8.5, 5.2, 7, 3.6, K.Wood[0]);
        c.Ellipse(8.5, 5.0, 6, 3.0, K.Wood[2]);
        c.Ellipse(8, 4.2, 4.6, 1.8, K.Wood[4]);
        for (int i = 0; i < 3; i++) { c.Set(5 + i * 3, 3, K.Wood[5]); c.Set(6 + i * 3, 4, K.Wood[5]); c.Set(6 + i * 3, 2, K.Wood[3]); }
        c.Rect(5, 8, 7, 1, K.Wood[1]);
        // tin
        c.Rect(15, 3, 7, 7, Rgba.Hex("8D8F99"));
        c.VL(15, 3, 7, Rgba.Hex("C9CCD6")); c.VL(21, 3, 7, Rgba.Hex("5E6070"));
        c.Rect(15, 3, 7, 2, Rgba.Hex("B0B3BF")); c.HL(15, 3, 7, Rgba.Hex("E6E8EF"));
        c.Rect(16, 6, 5, 2, K.Terra); c.Set(18, 6, K.Gold); c.Set(19, 7, K.Cream);
        c.HL(15, 9, 7, Rgba.Hex("4A4C5A"));
        c.SelOutline(0.40);
        return c;
    }

    static Canvas Bag(bool open)
    {
        var c = new Canvas(26, 26);
        int cx = 13, top = open ? 9 : 10, bot = 24;
        for (int y = top; y <= bot; y++)
        {
            double t = (double)(y - top) / (bot - top);
            double hw = open ? 6.2 + 5.8 * (1 - (1 - t) * (1 - t)) : 4.6 + 7.4 * Math.Sin(Math.Min(1.0, t * 1.2) * Math.PI / 2);
            if (t > 0.86) hw -= (t - 0.86) * 20;
            for (int x = (int)Math.Round(cx - hw); x < (int)Math.Round(cx + hw); x++)
            {
                double u = (x + 0.5 - cx) / hw;
                double v = 2.65 - 1.55 * u + Fx.Bay(x, y) * 0.55;
                int idx = (int)Math.Floor(v);
                if ((x == cx - 3 || x == cx + 4) && y % 4 != 0 && y > top + 2) idx--;
                if (y >= bot - 1) idx--;
                c.Set(x, y, Canvas1[Math.Clamp(idx, 0, 4)]);
            }
        }
        if (open)
        {
            // the mouth: dark inside with a folded blanket showing, rolled lip in front
            c.Ellipse(cx, top, 8.5, 3.6, Canvas1[3]);
            c.Ellipse(cx, top, 7.2, 2.7, K.Brown2);
            c.Rect(cx - 5, top - 1, 6, 2, Rgba.Hex("BFD3D2")); c.HL(cx - 5, top - 1, 6, Rgba.Hex("E6EFE6"));
            c.Rect(cx + 1, top - 2, 4, 2, K.Terra); c.HL(cx + 1, top - 2, 4, K.RedR[3]);
            c.Set(cx + 2, top + 1, Rgba.Hex("E8D9B5")); c.Set(cx - 3, top + 1, K.Tan);
            c.HL(cx - 8, top + 3, 3, Canvas1[3]); c.HL(cx + 5, top + 3, 3, Canvas1[1]);
            c.HL(cx - 6, top + 4, 12, Canvas1[2]);
        }
        else
        {
            // gathered neck, carry loop, buckled strap
            for (int y = top - 5; y < top; y++)
            {
                int hw = 1 + (y - (top - 5)) / 2 + 1;
                for (int x = cx - hw; x < cx + hw; x++) c.Set(x, y, Canvas1[x < cx ? 3 : 2]);
            }
            c.HL(cx - 3, top - 1, 6, K.Brown);
            c.Line(cx - 4, top - 1, cx - 3, top - 6, K.Brown); c.Line(cx + 4, top - 1, cx + 3, top - 6, K.Brown); c.HL(cx - 3, top - 7, 7, K.Brown2);
            for (int x = cx - 12; x < cx + 12; x++)
                for (int y = top + 5; y <= top + 6; y++)
                    if (c.Opaque(x, y)) c.Set(x, y, y == top + 5 ? K.Wood[3] : K.Wood[1]);
            c.Rect(cx + 1, top + 4, 4, 4, K.Gold); c.Rect(cx + 2, top + 5, 2, 2, K.Wood[0]);
            for (int y = top + 1; y < bot - 3; y++)
                if (c.Opaque(cx - 4, y)) { c.Set(cx - 4, y, K.Wood[3]); c.Set(cx - 3, y, K.Wood[1]); }
        }
        c.SelOutline(0.42);
        return c;
    }

    static Canvas Crate()
    {
        var c = new Canvas(30, 24);
        int w = 30;
        // lid (top face) and front planks
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < w; x++)
            {
                int idx = y == 0 ? 4 : (y == 3 || y == 6) ? 1 : 3;
                if (x == 0) idx = 4; if (x == w - 1) idx = 1;
                if (y > 0 && Fx.R(x / 5, y / 3, 41) > 0.8) idx--;
                c.Set(x, y, K.Pine[Math.Clamp(idx, 0, 4)]);
            }
        Draw.Planks(c, 0, 8, w, 16, K.Wood, 43, 5);
        c.VL(0, 8, 16, K.Wood[1]); c.VL(w - 1, 8, 16, K.Wood[0]);
        c.HL(0, 8, w, K.Wood[0]);
        // diagonal brace
        c.Line(3, 21, 12, 10, K.Wood[1]); c.Line(4, 21, 13, 10, K.Wood[4]);
        // rope ties over lid and face
        foreach (int rx in new[] { 8, 21 })
        {
            c.VL(rx, 0, 24, K.Tan); c.VL(rx + 1, 0, 24, K.Brown);
            c.Set(rx - 1, 0, K.Tan); c.Set(rx + 2, 1, K.Brown);
        }
        // scribbled marking on the side
        foreach (var (sx, sy) in new[] { (13, 14), (14, 13), (15, 14), (16, 13), (17, 14), (15, 16), (16, 17), (17, 16) }) c.Set(sx, sy, K.Brown3);
        c.SelOutline(0.40);
        return c;
    }

    static Canvas Parcel()
    {
        var c = new Canvas(20, 16);
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 20; x++)
            {
                int idx = y < 5 ? 3 : 2;
                if (y == 0 || x == 0) idx = 4; if (x == 19 || y == 15) idx = 1;
                c.Set(x, y, K.Plaster[Math.Clamp(idx, 0, 4)].Shade(0.15));
            }
        c.HL(0, 5, 20, K.Plaster[1]);
        c.VL(9, 0, 16, K.Tan); c.VL(10, 0, 16, K.Brown);
        c.HL(0, 9, 20, K.Tan);
        c.Set(8, 0, K.Tan); c.Set(11, 0, K.Brown); c.Set(8, 1, K.Brown);
        c.SelOutline(0.42);
        return c;
    }

    static Canvas Bundle()
    {
        var c = new Canvas(18, 16);
        Draw.Blob(c, 9, 9, 7.5, 6, Canvas1, 77, 0.0);
        c.HL(2, 8, 14, K.Brown); c.HL(2, 9, 14, K.Tan);
        c.VL(8, 4, 10, K.Brown); c.VL(9, 4, 10, K.Tan);
        c.Rect(6, 1, 6, 3, Canvas1[3]); c.HL(6, 1, 6, Canvas1[4]); c.Set(8, 3, K.Brown); c.Set(9, 3, K.Tan);
        c.Rect(12, 11, 3, 2, K.Terra);                                            // a corner of red blanket
        c.SelOutline(0.42);
        return c;
    }

    static Canvas RugRoll()
    {
        var c = new Canvas(12, 30);
        Draw.Cyl(c, 1, 3, 10, 27, K.TealR, 71, false);
        c.Ellipse(6, 3.5, 5, 2.6, K.TealR[1]);
        c.Ellipse(6, 3.5, 3.6, 1.8, K.RedR[2]);
        c.Ellipse(6, 3.5, 2, 1, K.Mustard[3]);
        c.Set(6, 3, K.RedR[0]);
        foreach (int ty in new[] { 10, 24 }) { c.HL(1, ty, 10, K.Tan); c.HL(1, ty + 1, 10, K.Brown); }
        for (int y = 5; y < 29; y += 3) c.Set(2, y, K.TealR[4]);
        c.SelOutline(0.40);
        return c;
    }

    static Canvas ClothBag()
    {
        var c = new Canvas(14, 14);
        Draw.Blob(c, 7, 9, 5.5, 4.5, K.RedR, 79, 0.0);
        c.Rect(5, 2, 4, 3, K.RedR[3]); c.HL(4, 4, 6, K.Tan); c.HL(4, 5, 6, K.Brown);
        c.Set(4, 6, K.Tan); c.Set(4, 7, K.Brown); c.Set(10, 6, K.Tan);
        c.Set(8, 1, K.Terra); c.Set(7, 1, K.RedR[1]);
        c.Set(6, 9, K.Mustard[3]); c.Set(7, 10, K.Mustard[3]); c.Set(8, 9, K.Mustard[3]);   // a small stitched flower
        c.SelOutline(0.42);
        return c;
    }

    static Canvas DoorLeaf()
    {
        var c = new Canvas(10, 34);
        for (int y = 3; y < 34; y++)
            for (int x = 0; x < 10; x++)
            {
                int idx = x < 2 ? 4 : x < 8 ? (x % 3 == 1 ? 2 : 3) : 1;
                if (x == 7) idx = 0;
                if (y == 3) idx = 5;
                if (y == 33) idx = 0;
                if (y % 11 == 6 && x < 8) idx = 1;
                c.Set(x, y, K.Wood[Math.Clamp(idx, 0, 5)]);
            }
        for (int y = 0; y < 3; y++) c.Rect(1, y, 8, 1, K.Wood[y == 0 ? 5 : 4]);
        c.Rect(2, 17, 2, 3, K.Gold); c.Set(2, 17, K.Mustard[4]);              // handle
        c.Rect(8, 7, 2, 2, K.Iron[2]); c.Rect(8, 27, 2, 2, K.Iron[2]);          // hinges
        c.SelOutline(0.40);
        return c;
    }

    static Canvas SignPost()
    {
        var c = new Canvas(18, 40);
        Draw.Cyl(c, 7, 6, 4, 34, K.Wood, 83, true);
        // two boards pointing opposite ways
        void Board(int x, int y, int w, bool right)
        {
            c.Rect(x, y, w, 7, K.Pine[2]); c.HL(x, y, w, K.Pine[4]); c.HL(x, y + 6, w, K.Pine[0]);
            int tx = right ? x + w : x - 3;
            for (int i = 0; i < 3; i++) c.Rect(right ? tx + i : tx + 2 - i, y + 2 - Math.Min(2, i), 1, 3 + 2 * Math.Min(2, i) - 0, K.Pine[2]);
            for (int i = 0; i < w - 4; i += 2) c.Set(x + 2 + i, y + 3, K.Brown2);
            c.Set(x + 1, y + 1, K.Iron[2]); c.Set(x + w - 2, y + 1, K.Iron[2]);
        }
        Board(4, 4, 11, true);
        Board(2, 14, 11, false);
        c.SelOutline(0.40);
        return c;
    }

    static Canvas Scraps()
    {
        var c = new Canvas(14, 8);
        c.Rect(1, 2, 6, 4, Rgba.Hex("F4EDDA")); c.Set(2, 3, Rgba.Hex("B8A88A")); c.Set(4, 3, Rgba.Hex("B8A88A")); c.Set(3, 5, Rgba.Hex("B8A88A"));
        c.Rect(8, 3, 5, 3, Rgba.Hex("E8DEC4")); c.HL(8, 3, 5, Rgba.Hex("FFFAEC"));
        c.Line(5, 6, 12, 7, K.Tan);
        c.SelOutline(0.5);
        return c;
    }

    /// <summary>Wisp of ground mist, drawn as a sparse ordered dither (no alpha) so it stays pixel art and never hides the ground.</summary>
    static Canvas Mist(int w, int h, int frame, int seed)
    {
        var c = new Canvas(w, h);
        var a = H("C9D5E2"); var b = H("B1C0D2");
        double rx = w / 2.0 - 1, ry = h / 2.0 - 0.5;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                double ox = frame * 1.5;
                double dx = (x + .5 - w / 2.0) / rx, dy = (y + .5 - h / 2.0) / ry;
                double n = Fx.Fbm((x - ox) / 7.0, y / 3.5, seed) - 0.5;
                double d = dx * dx + dy * dy + n * 0.6;
                if (d >= 1.0) continue;
                double density = (1 - d) * 0.9;
                double thr = Fx.Bay(x + frame, y) * 0.5 + 0.5;
                if (density * 0.62 <= thr) continue;
                c.Set(x, y, ((x + y * 3) & 7) == 0 ? b : a);
            }
        return c;
    }

    // ================================================ registration ==========================================
    public static void Build()
    {
        S.Cur = "reused";
        // --- characters
        Use("child_a_stand_down", null, "npc_c4_child_a_wait_0");
        Use("child_a_stand_down", Breathe, "npc_c4_child_a_wait_1");
        Use("child_b_stand_down", null, "npc_c4_child_b_wait_0");
        Use("child_b_stand_down", Breathe, "npc_c4_child_b_wait_1");
        Use("cat_sleep_0", null, "npc_c4_cat_wait_0");
        Use("cat_sleep_1", null, "npc_c4_cat_wait_1");

        S.Cur = "house";
        Use("prop_desk", DeskBare, "prop_c4_desk_bare");
        Use("prop_sideboard_photo", s => Photo(s, false), "prop_c4_sideboard_bare");
        Use("prop_sideboard_photo", s => Photo(s, true), "prop_c4_photo_frame");
        Use("prop_stove", StoveCold, "prop_c4_stove_cold");
        Use("prop_kitchen_shelf", ShelfBare, "prop_c4_shelf_bare");
        Use("prop_table_empty", TableBare, "prop_c4_table_bare");
        Use("prop_sofa", s => Sheeted(s, 0, 4), "prop_c4_sofa_sheeted");
        Use("prop_armchair", s => Sheeted(s, 0, 4), "prop_c4_armchair_sheeted");
        Use("prop_bookcase", s => Sheeted(s, 0, 3), "prop_c4_bookcase_sheeted");
        Use("prop_bed_double", s => Sheeted(s, 11, 10), "prop_c4_bed_sheeted");
        Use("prop_coffee_table", s => Sheeted(s, 0, 5), "prop_c4_coffee_table_sheeted");

        S.Cur = "garden";
        for (int i = 1; i <= 4; i++) Use($"prop_bed_{i}_dry", Wilted, $"prop_c4_bed_{i}_wilted");
        foreach (var f in new[] { "flowers_red", "flowers_yellow", "flowers_pink", "flowers_lavender", "flowers_white", "flowers_sway_0", "flowers_sway_1", "sunflowers", "pot_red", "pot_blue", "windowbox_a", "windowbox_b", "plant_pot", "plant_small", "trellis" })
            Use("prop_" + f, Wilted);

        S.Cur = "newprops";
        Reg("prop_c4_papers", Lift(PapersBundle(), 25));
        Reg("prop_c4_food", Lift(FoodOnTable(), 18));
        Reg("prop_c4_bag_open", Bag(true));
        Reg("prop_c4_bag_closed", Bag(false));
        Reg("prop_c4_crate", Crate());
        Reg("prop_c4_parcel", Parcel());
        Reg("prop_c4_bundle", Bundle());
        Reg("prop_c4_rug_roll", RugRoll());
        Reg("prop_c4_clothbag", ClothBag());
        Reg("prop_c4_door_ajar", DoorLeaf());
        Reg("prop_c4_signpost", SignPost());
        Reg("prop_c4_scraps", Scraps());
        Reg("prop_c4_lantern_0", LanternLit(0), false);
        Reg("prop_c4_lantern_1", LanternLit(1), false);

        S.Cur = "fx";
        for (int f = 0; f < 2; f++)
        {
            Reg($"fx_c4_mist_a_{f}", Mist(64, 14, f, 501), false);
            Reg($"fx_c4_mist_b_{f}", Mist(44, 10, f, 507), false);
            Reg($"fx_c4_mist_c_{f}", Mist(88, 18, f, 513), false);
        }
    }
}
