namespace WorldArtCh2;

/// <summary>Market props: barrels, sacks, baskets, carts, notice board, sign post, planters, bunting, steam, speech bubble.</summary>
public static class Props
{
    static Rgba H(string h) => Rgba.Hex(h);
    static readonly Rgba[] Sack = { H("9C8458"), H("B89E72"), H("D0B88C"), H("E2D0A8"), H("F0E2C0") };
    static readonly Rgba[] Grain = { H("B8923F"), H("D2AE55"), H("E6C86E"), H("F2DC8C"), H("FAEBB0") };
    static readonly Rgba Rope = H("8A6A44"), RopeHi = H("B8946A");

    // ---- barrels ------------------------------------------------------------------------------------------------------
    static Canvas Barrel(bool open, int seed)
    {
        var c = new Canvas(22, 26);
        // body: bulging staves, cylinder shading
        for (int y = 5; y <= 23; y++)
        {
            double t = (y - 5) / 18.0;
            double hw = 8.6 + 1.5 * Math.Sin(t * Math.PI);
            for (int x = 0; x < 22; x++)
            {
                double dx = x + .5 - 11; if (Math.Abs(dx) > hw) continue;
                double u = (dx + hw) / (2 * hw);
                int tone = u < 0.2 ? 4 : u < 0.5 ? 3 : u < 0.75 ? 2 : 1;
                if (((int)(u * 7) & 1) == 1 && tone > 1 && y % 5 != 0) tone--;
                if (Fx.R(x / 2, y / 6, seed) > 0.9 && tone > 1) tone--;
                c.Set(x, y, K.Wood[tone]);
            }
        }
        // iron hoops
        foreach (int hy in new[] { 8, 20 })
            for (int x = 0; x < 22; x++)
            {
                double t = (hy - 5) / 18.0; double hw = 8.6 + 1.5 * Math.Sin(t * Math.PI);
                double dx = x + .5 - 11; if (Math.Abs(dx) > hw) continue;
                c.Set(x, hy, K.Iron[dx < -2 ? 3 : dx > 3 ? 1 : 2]); c.Set(x, hy + 1, K.Iron[dx > 3 ? 0 : 1]);
            }
        // lid ellipse
        c.Ellipse(11, 5.5, 9.5, 4.3, K.Wood[4]);
        c.Ellipse(11, 5.5, 8.2, 3.3, open ? H("3E4A3A") : K.Wood[3]);
        if (open)
        {
            for (int i = 0; i < 9; i++) { Stalls.Disc(c, 6.5 + (i % 5) * 2.3, 4.2 + (i / 5) * 1.8, 1.3, new[] { H("4F6B2E"), H("6B8E3A"), H("8DB052"), H("A9C860") }, seed + i); }
            c.Paint((x, y) => { double dx = (x + .5 - 11) / 9.3, dy = (y + .5 - 5.5) / 4.1; double d = dx * dx + dy * dy; return d <= 1 && d > 0.76; }, K.Wood[4]);
        }
        else
        {
            for (int x = 5; x < 17; x++) { c.Set(x, 4, K.Wood[(x & 1) == 0 ? 3 : 2]); c.Set(x, 6, K.Wood[2]); }
            c.HL(6, 5, 10, K.Wood[1]);
            c.Set(11, 4, K.Iron[2]);
        }
        c.HL(7, 24, 8, K.Wood[0]); c.HL(7, 25, 8, K.Wood[0]);
        c.SelOutline(0.40, false);
        return c;
    }

    // ---- sacks --------------------------------------------------------------------------------------------------------
    static Canvas SackClosed(int seed)
    {
        var c = new Canvas(16, 18);
        c.Ellipse(8, 11.5, 6.4, 5.6, Sack[2]);
        c.Rect(5, 3, 6, 5, Sack[2]);
        c.Set(5, 3, Rgba.Clear); c.Set(10, 3, Rgba.Clear);
        // neck tie
        c.HL(5, 7, 6, Rope); c.Set(6, 7, RopeHi); c.Set(7, 8, Rope);
        // flop
        c.Set(4, 2, Sack[3]); c.Set(5, 2, Sack[3]); c.Set(11, 2, Sack[1]); c.Set(10, 2, Sack[2]);
        // shading
        c.Paint((x, y) => x <= 5 && c.Get(x, y) == Sack[2] && y > 7, Sack[3]);
        c.Paint((x, y) => x >= 11 && c.Get(x, y) == Sack[2] && y > 7, Sack[1]);
        c.HL(4, 16, 8, Sack[0]);
        c.Set(9, 11, Sack[1]); c.Set(10, 12, Sack[1]); c.Set(6, 12, Sack[3]);
        c.SelOutline(0.40, false);
        return c;
    }

    static Canvas SackOpen(int seed)
    {
        var c = new Canvas(20, 18);
        c.Ellipse(10, 11.5, 7.6, 6.0, Sack[2]);
        c.Rect(4, 7, 12, 8, Sack[2]);
        c.Ellipse(10, 7, 7.2, 3.1, Sack[4]);          // rolled lip
        c.Ellipse(10, 6.6, 5.6, 2.2, Grain[2]);
        Stalls.Disc(c, 10, 6, 3.4, Grain, seed);
        c.Paint((x, y) => x <= 5 && c.Get(x, y) == Sack[2] && y > 8, Sack[3]);
        c.Paint((x, y) => x >= 14 && c.Get(x, y) == Sack[2] && y > 8, Sack[1]);
        c.HL(4, 16, 12, Sack[0]);
        // spilled grain
        for (int i = 0; i < 7; i++) c.Set(2 + (int)(Fx.R(i, 1, seed) * 16), 16 + (int)(Fx.R(i, 2, seed) * 1.9), Grain[1 + i % 3]);
        c.SelOutline(0.40, false);
        return c;
    }

    // ---- basket (keepsake) ----------------------------------------------------------------------------------------------
    static Canvas MarketBasket()
    {
        var c = new Canvas(24, 20);
        var w1 = H("E2BC7E"); var w2 = H("C4965A"); var w3 = H("9A6D3C"); var w4 = H("6E4A2A");
        // body (tapered)
        for (int y = 9; y <= 18; y++)
        {
            int hw = 8 - (y - 9) / 4;
            for (int x = 12 - hw; x < 12 + hw; x++)
            {
                bool a = ((x + (y / 2) * 2) / 2 + y) % 2 == 0;
                int tone = a ? 1 : 2;
                if (x < 12 - hw + 2) tone = a ? 0 : 1;
                if (x >= 12 + hw - 2) tone = a ? 3 : 2;
                c.Set(x, y, new[] { w1, w2, w3, w4 }[Math.Clamp(tone, 0, 3)]);
            }
        }
        // rim
        for (int x = 3; x < 21; x++) { c.Set(x, 8, x < 6 ? w1 : w2); c.Set(x, 9, w3); }
        c.HL(4, 19, 16, w4);
        // folded cloth with check, loaf and greens peeking out
        for (int x = 5; x < 19; x++) for (int y = 5; y <= 8; y++) { bool ck = ((x / 2) + (y / 2)) % 2 == 0; c.Set(x, y, ck ? H("F4EAD2") : H("D9786A")); }
        c.Set(5, 5, Rgba.Clear); c.Set(18, 5, Rgba.Clear);
        for (int x = 5; x < 19; x++) c.Set(x, 8, ((x / 2) & 1) == 0 ? H("C9B894") : H("B5604F"));
        Stalls.Disc(c, 9.5, 4.2, 3.6, new[] { H("7E4A24"), H("A0642F"), H("C0823F"), H("D9A056"), H("EBC17A") }, 3);
        c.Set(8, 2, H("EBC17A")); c.Set(10, 3, H("7E4A24")); c.Set(9, 4, H("A0642F"));
        for (int i = 0; i < 4; i++) { c.Set(15 + i, 3 - (i % 2), H("6FA86B")); c.Set(15 + i, 4 - (i % 2), H("447A4B")); }
        Stalls.Disc(c, 17.5, 3.2, 1.9, new[] { H("8A2F2F"), H("B03E3A"), H("D2554A"), H("E5705C"), H("F2927C") }, 4);
        // handle (arched, drawn behind the cloth items at the sides)
        for (int a = 190; a <= 350; a += 6)
        {
            double r = Math.Sqrt(a); _ = r;
            int hx = (int)Math.Round(12 + 8.5 * Math.Cos(a * Math.PI / 180)), hy = (int)Math.Round(9 + 8 * Math.Sin(a * Math.PI / 180));
            if (hy < 1) continue;
            if (hy < 8 && Math.Abs(hx - 12) < 6) continue;
            c.Set(hx, hy, w3); c.Set(hx + (hx < 12 ? 1 : -1), hy, w2);
        }
        c.SelOutline(0.40, true);
        return c;
    }

    // ---- carts ------------------------------------------------------------------------------------------------------------
    static void Wheel(Canvas c, double cx, double cy, double r)
    {
        c.Disc(cx, cy, r, K.Wood[0]);
        c.Disc(cx, cy, r - 1.2, K.Wood[3]);
        c.Disc(cx, cy, r - 2.8, K.Wood[2]);
        // spokes
        for (int a = 0; a < 180; a += 30)
        {
            double rad = a * Math.PI / 180;
            c.Line((int)(cx - Math.Cos(rad) * (r - 1.5)), (int)(cy - Math.Sin(rad) * (r - 1.5)), (int)(cx + Math.Cos(rad) * (r - 1.5)), (int)(cy + Math.Sin(rad) * (r - 1.5)), K.Wood[1]);
        }
        c.Disc(cx, cy, 1.6, K.Iron[2]);
        c.Set((int)cx - 1, (int)cy - 1, K.Iron[4]);
    }

    /// <summary>Parked handcart seen from the side (handles to the left) with crates of produce.</summary>
    static Canvas Handcart()
    {
        var c = new Canvas(52, 34);
        // handles
        c.Line(0, 17, 13, 24, K.Wood[3]); c.Line(0, 18, 13, 25, K.Wood[1]);
        c.Line(2, 28, 12, 24, K.Wood[2]);   // prop leg
        c.VL(2, 24, 5, K.Wood[2]); c.VL(3, 25, 5, K.Wood[1]);
        // cart bed
        Stalls.CrateFront(c, 10, 17, 36, 11);
        c.HL(10, 17, 36, K.Wood[5]); c.HL(10, 28, 36, K.Wood[0]);
        c.Rect(11, 12, 34, 5, K.Wood[1]); c.HL(11, 12, 34, K.Wood[3]);
        // load: cabbages and a sack
        for (int i = 0; i < 5; i++) Stalls.Disc(c, 16 + i * 5.6, 12.5 - (i & 1) * 1.2, 3.4, K.LeafWarm, 20 + i);
        c.Rect(36, 7, 8, 7, Sack[2]); c.HL(36, 7, 8, Sack[4]); c.VL(43, 8, 6, Sack[1]); c.Set(36, 7, Rgba.Clear); c.Set(43, 7, Rgba.Clear);
        // wheel
        Wheel(c, 32, 26, 7.2);
        c.SelOutline(0.40, false);
        return c;
    }

    static Canvas LoadedCart()
    {
        var body = LoadedCartBody();
        const int ext = 18;
        var c = new Canvas(48, body.H + ext);
        var pl = K.Wood;
        // long shafts: the carter holds the far ends, so he walks well ahead of the load
        for (int i = 0; i < 2; i++)
        {
            int x0 = i == 0 ? 13 : 33, x1 = i == 0 ? 17 : 29;
            c.Line(x0 + 1, 22 + ext, x1 + 1, 2, pl[1]); c.Line(x0, 22 + ext, x1, 2, pl[3]);
            c.Line(x0 + 2, 22 + ext, x1 + 2, 2, pl[0]);
        }
        c.HL(15, 2, 17, pl[2]);
        c.Blit(body, 0, ext);
        c.SelOutline(0.40, false);
        return c;
    }

    /// <summary>Cart loaded with bundles, seen from behind, heading north.</summary>
    static Canvas LoadedCartBody()
    {
        var c = new Canvas(48, 52);
        var pl = K.Wood;
        c.Rect(7, 22, 34, 8, pl[2]);
        for (int x = 7; x < 41; x += 5) c.VL(x, 22, 8, pl[1]);
        c.HL(7, 22, 34, pl[4]);
        // sides (left / right panels rising)
        c.Rect(6, 17, 3, 20, pl[3]); c.Rect(39, 17, 3, 20, pl[2]); c.VL(6, 17, 20, pl[4]); c.VL(41, 17, 20, pl[1]);
        // load: big sacks and bundles
        c.Ellipse(15, 26, 8, 9, Sack[2]); c.Ellipse(14, 24.5, 6, 6, Sack[3]);
        c.HL(9, 31, 12, Sack[1]);
        c.Ellipse(33, 25, 8, 9, H("7C6AA3")); c.Ellipse(32, 23.5, 6, 6, H("9F8CC4"));
        c.Ellipse(24, 17, 9, 8, H("C05A4A")); c.Ellipse(23, 15.5, 7, 6, H("D9786A"));
        c.Ellipse(24, 8.5, 6, 4.5, H("DCC794")); c.Ellipse(23, 7.5, 4.5, 3, H("F4EAD2"));
        // ropes
        c.Line(8, 20, 22, 32, Rope); c.Line(40, 18, 26, 32, Rope); c.Line(24, 9, 24, 33, Rope);
        c.Line(9, 20, 23, 32, RopeHi); 
        // a cooking pot and rolled mat tied on the side
        c.Rect(36, 10, 7, 6, K.Iron[2]); c.HL(36, 10, 7, K.Iron[4]); c.VL(42, 11, 5, K.Iron[1]); c.HL(37, 9, 5, K.Iron[3]);
        c.Rect(3, 13, 4, 18, H("B39A6C")); c.VL(3, 13, 18, H("DECDA0")); c.VL(6, 14, 17, H("8F7552")); for (int y = 15; y < 30; y += 4) c.HL(3, y, 4, Rope);
        // rear panel
        Stalls.CrateFront(c, 6, 33, 36, 11);
        c.HL(6, 33, 36, pl[5]); c.HL(6, 43, 36, pl[0]);
        // wheels (thin ellipses seen edge-on)
        foreach (int wx in new[] { 2, 45 })
        {
            c.Ellipse(wx, 40, 2.6, 8.6, K.Wood[0]); c.Ellipse(wx - 0.3, 40, 1.4, 7.2, K.Wood[wx < 24 ? 3 : 2]);
            c.Ellipse(wx, 40, 0.7, 1.3, K.Iron[2]);
        }
        for (int x = 8; x < 40; x++) c.Set(x, 45, K.Wood[0]);
        return c;
    }

    static Canvas Bundles()
    {
        var c = new Canvas(28, 22);
        c.Ellipse(9, 14, 8, 6.5, H("C05A4A")); c.Ellipse(8, 12.8, 6, 4.6, H("D9786A"));
        c.Ellipse(20, 15, 7, 6, H("6B8FA8")); c.Ellipse(19, 13.5, 5, 4.2, H("8FB0C2"));
        c.Ellipse(14, 8, 6.5, 5.5, Sack[2]); c.Ellipse(13, 7, 5, 4, Sack[3]);
        c.Line(3, 12, 14, 17, Rope); c.Line(16, 8, 26, 17, Rope); c.VL(14, 4, 8, Rope);
        c.HL(4, 19, 20, K.Dark[3]);
        c.SelOutline(0.40, false);
        return c;
    }

    // ---- notice board, signpost, planter ---------------------------------------------------------------------------------
    static Canvas NoticeBoard()
    {
        var c = new Canvas(30, 40);
        Draw.Cyl(c, 3, 18, 3, 22, K.Wood, 2); Draw.Cyl(c, 24, 18, 3, 22, K.Wood, 3);
        c.Rect(1, 4, 28, 20, K.Wood[2]); c.Frame(1, 4, 28, 20, K.Wood[4]); c.HL(1, 23, 28, K.Wood[0]);
        c.Rect(3, 6, 24, 16, H("9A7650"));
        for (int x = 3; x < 27; x += 4) c.VL(x, 6, 16, H("8A6844"));
        // roof cap
        c.Rect(0, 1, 30, 4, K.Roof[2]); c.HL(0, 1, 30, K.Roof[4]); c.HL(0, 4, 30, K.Roof[0]);
        // pinned papers with squiggles (not readable)
        var sheets = new[] { (5, 8, 7, 8, "F4EAD2"), (14, 7, 6, 6, "E8D9B5"), (21, 9, 5, 9, "FBF3DE"), (8, 16, 6, 5, "E8D9B5"), (16, 15, 5, 6, "F4EAD2") };
        foreach (var (x, y, w, h, hex) in sheets)
        {
            c.Rect(x, y, w, h, H(hex)); c.VL(x + w - 1, y, h, H("CDBFA0")); c.HL(x, y + h - 1, w, H("CDBFA0"));
            for (int yy = y + 2; yy < y + h - 1; yy += 2) for (int xx = x + 1; xx < x + w - 2; xx += 2) c.Set(xx + ((yy / 2) & 1), yy, K.Brown2);
            c.Set(x + w / 2, y, K.Terra);
        }
        c.Rect(23, 16, 3, 4, K.Terra); c.HL(23, 16, 3, K.RedR[3]);
        c.SelOutline(0.40, false);
        return c;
    }

    static Canvas SignPost()
    {
        var c = new Canvas(28, 40);
        Draw.Cyl(c, 12, 6, 4, 34, K.Wood, 4);
        c.Rect(10, 38, 8, 2, K.Wood[0]);
        // two arrow boards
        c.Poly(new[] { (2.0, 8.0), (18.0, 8.0), (18.0, 18.0), (2.0, 18.0), (-1.0, 13.0) }, K.Pine[2]);
        c.Poly(new[] { (11.0, 20.0), (26.0, 20.0), (29.0, 25.0), (26.0, 30.0), (11.0, 30.0) }, K.Pine[3]);
        c.HL(2, 8, 16, K.Pine[4]); c.HL(11, 20, 15, K.Pine[4]); c.HL(2, 17, 16, K.Pine[0]); c.HL(11, 29, 15, K.Pine[1]);
        for (int x = 4; x < 15; x += 2) c.Set(x, 12 + (x / 2 % 2), K.Brown2);
        for (int x = 14; x < 24; x += 2) c.Set(x, 24 + (x / 2 % 2), K.Brown2);
        c.Set(13, 7, K.Iron[3]); c.Set(13, 19, K.Iron[3]);
        c.SelOutline(0.40, false);
        return c;
    }

    static Canvas Planter(int seed)
    {
        var c = new Canvas(34, 24);
        // stone box
        for (int y = 11; y <= 22; y++)
            for (int x = 1; x < 33; x++)
            {
                int tone = y == 11 ? 5 : y >= 21 ? 1 : 3;
                if (x < 3) tone++; if (x > 30) tone--;
                if ((y - 11) % 4 == 3 && y < 21) tone = 2;
                if (((x + (((y - 11) / 4) & 1) * 5) % 10) == 0) tone = 2;
                c.Set(x, y, K.Stone[Math.Clamp(tone, 0, 5)]);
            }
        c.Rect(0, 9, 34, 3, K.Stone[4]); c.HL(0, 9, 34, K.Stone[5]); c.HL(0, 11, 34, K.Stone[2]);
        c.Rect(2, 8, 30, 2, K.Soil);
        // plants
        var cols = new[] { K.Cream, K.Gold, H("EAA5B5"), H("F4B07A"), H("B8D2DC") };
        for (int i = 0; i < 9; i++)
        {
            int px = 3 + i * 3 + (int)(Fx.R(i, 1, seed) * 2), h = 5 + (int)(Fx.R(i, 2, seed) * 6);
            for (int y = 0; y < h; y++) c.Set(px, 8 - y, K.Leaf[2 + (y % 2)]);
            if (i % 2 == 0) { c.Set(px - 1, 8 - h / 2, K.Leaf[4]); c.Set(px + 1, 8 - h / 2 - 1, K.Leaf[3]); }
            var fc = cols[(i + seed) % cols.Length];
            c.Set(px, 8 - h, fc); c.Set(px - 1, 8 - h, fc); c.Set(px + 1, 8 - h, fc); c.Set(px, 7 - h, fc); c.Set(px, 9 - h, K.Gold);
        }
        c.SelOutline(0.40, false);
        return c;
    }

    // ---- bunting ---------------------------------------------------------------------------------------------------------------
    static Canvas Bunting(int w, int frame, int seed)
    {
        var c = new Canvas(w, 18);
        var cols = new[] { K.RedR, K.Mustard, K.TealR, K.Linen, K.BlueR };
        double sag = 5;
        int Ly(int x) => 1 + (int)Math.Round(sag * (1 - Math.Pow(2.0 * x / (w - 1) - 1, 2)));
        for (int x = 0; x < w; x++) c.Set(x, Ly(x), K.Brown2);
        for (int x = 4; x < w - 4; x += 8)
        {
            var r = cols[(x / 8 + seed) % cols.Length];
            int y0 = Ly(x) + 1, sw = ((x / 8 + frame) & 1) == 0 ? 1 : 0;
            for (int j = 0; j < 7; j++)
            {
                int half = Math.Max(0, 3 - j / 2);
                for (int i = -half; i <= half; i++) c.Set(x + i + (j > 3 ? sw : 0), y0 + j, r[i < 0 ? 3 : i > 0 ? 1 : 2]);
            }
            c.HL(x - 3, y0, 7, r[4 == r.Length ? 3 : 3]);
        }
        c.SelOutline(0.45, false);
        return c;
    }

    // ---- fx ---------------------------------------------------------------------------------------------------------------------
    static Canvas Steam(int frame)
    {
        var c = new Canvas(14, 22);
        var a = H("FBF3DE"); var b = H("E8D9B5"); var d = H("D4C7A8");
        for (int i = 0; i < 3; i++)
        {
            double ph = (frame + i * 1.4) % 4.0;
            int y = 20 - (int)(ph / 4.0 * 19);
            int x = 3 + i * 4 + (int)Math.Round(Math.Sin(ph * 1.6 + i) * 1.5);
            int sz = ph < 1 ? 1 : ph < 3 ? 2 : 1;
            for (int j = 0; j < sz + 1; j++) for (int k2 = 0; k2 < sz + 1; k2++) c.Set(x + k2, y + j, ((j + k2) & 1) == 0 ? a : b);
            c.Set(x + sz, y + sz, d);
            if (ph > 0.8 && ph < 3.2) c.Set(x - 1 + (i & 1) * 3, y + 2, b);
        }
        return c;
    }

    static Canvas Bubble(int frame)
    {
        var c = new Canvas(20, 18);
        int dy = frame;
        c.Rect(2, 1 + dy, 16, 11, K.Cream); c.Rect(1, 2 + dy, 18, 9, K.Cream);
        c.Set(1, 2 + dy, Rgba.Clear); c.Set(18, 2 + dy, Rgba.Clear); c.Set(1, 10 + dy, Rgba.Clear); c.Set(18, 10 + dy, Rgba.Clear);
        c.Tri(7, 11 + dy, 11, 11 + dy, 8, 15 + dy, K.Cream);
        c.HL(3, 11 + dy, 14, K.Parch);
        var h1 = K.RedR[3]; var h2 = K.RedR[2]; var h3 = K.RedR[1];
        foreach (var (x, y) in new[] { (7, 4), (8, 4), (11, 4), (12, 4), (6, 5), (7, 5), (8, 5), (9, 5), (10, 5), (11, 5), (12, 5), (13, 5), (6, 6), (7, 6), (8, 6), (9, 6), (10, 6), (11, 6), (12, 6), (13, 6), (7, 7), (8, 7), (9, 7), (10, 7), (11, 7), (12, 7), (8, 8), (9, 8), (10, 8), (11, 8), (9, 9), (10, 9) })
            c.Set(x, y + dy, y <= 5 ? h1 : y >= 8 ? h3 : h2);
        c.Set(7, 5 + dy, K.RedR[4]);
        c.SelOutline(0.45, false);
        return c;
    }

    static Canvas CropBottom(Canvas c)
    {
        var b = c.Bounds()!.Value;
        return c.Crop(0, 0, c.W, b.y + b.h);
    }

    static Canvas Smoke(int frame)
    {
        var c = new Canvas(14, 18);
        var a = H("E8E2EC"); var b = H("CFC8D6"); var d = H("B0A8BC");
        for (int i = 0; i < 3; i++)
        {
            double ph = (frame + i * 1.3) % 4.0;
            int y = 16 - (int)(ph / 4.0 * 15);
            int x = 4 + (int)Math.Round(Math.Sin(ph * 1.3 + i) * 2) + i;
            int sz = ph < 1 ? 2 : ph < 3 ? 3 : 4;
            c.Disc(x + sz / 2.0, y + sz / 2.0, sz / 2.0 + 0.3, ph > 2.6 ? d : b);
            c.Set(x, y, a);
        }
        return c;
    }

    static Canvas Haybale()
    {
        var c = new Canvas(26, 18);
        c.Rect(1, 3, 24, 13, H("D8BC6C"));
        for (int y = 3; y < 16; y++) for (int x = 1; x < 25; x++) { if (Fx.R(x, y, 12) > .72) c.Set(x, y, H("C09A4A")); else if (Fx.R(x, y, 13) > .86) c.Set(x, y, H("F0DC96")); }
        c.HL(1, 3, 24, H("EBD68A")); c.HL(1, 15, 24, H("A88442"));
        c.VL(8, 3, 13, Rope); c.VL(17, 3, 13, Rope);
        c.VL(1, 3, 13, H("EBD68A")); c.VL(24, 3, 13, H("A88442"));
        c.Set(1, 3, Rgba.Clear); c.Set(24, 3, Rgba.Clear);
        c.SelOutline(0.40, false);
        return c;
    }

    public static void Build()
    {
        S.Cur = "props";
        S.Add("prop_c2_barrel", Barrel(false, 1));
        S.Add("prop_c2_barrel_open", Barrel(true, 2));
        S.Add("prop_c2_sack", SackClosed(1));
        S.Add("prop_c2_sack_b", SackClosed(5));
        S.Add("prop_c2_sack_open", SackOpen(3));
        S.Add("prop_c2_market_basket", MarketBasket());
        S.Add("prop_c2_handcart", Handcart());
        S.Add("prop_c2_cart_loaded", CropBottom(LoadedCart()));
        S.Add("prop_c2_bundles", Bundles());
        S.Add("prop_c2_noticeboard", NoticeBoard());
        S.Add("prop_c2_signpost", SignPost());
        S.Add("prop_c2_planter_a", Planter(1));
        S.Add("prop_c2_planter_b", Planter(4));
        S.Add("prop_c2_haybale", CropBottom(Haybale()));
        foreach (var kind in new[] { "carrot", "cabbage", "tomato", "apple", "orange", "pear", "empty" })
            S.Add($"prop_c2_crate_{kind}", Stalls.FloorCrate(kind, false, 3));
        foreach (var kind in new[] { "carrot", "cabbage", "tomato" })
            S.Add($"prop_c2_crate_{kind}_thin", Stalls.FloorCrate(kind, true, 3));
        S.Add("prop_c2_crate_empty_b", Stalls.FloorCrate("empty", false, 1));
        for (int f = 0; f < 2; f++)
        {
            S.Add($"prop_c2_bunting_long_{f}", Bunting(112, f, 0));
            S.Add($"prop_c2_bunting_mid_{f}", Bunting(80, f, 2));
            S.Add($"prop_c2_bunting_short_{f}", Bunting(48, f, 1));
        }
        S.Cur = "fx";
        for (int f = 0; f < 4; f++) S.Add($"fx_c2_steam_{f}", Steam(f));
        for (int f = 0; f < 2; f++) S.Add($"fx_c2_bubble_{f}", Bubble(f));
        for (int f = 0; f < 4; f++) S.Add($"fx_c2_smoke_{f}", Smoke(f));
    }
}
