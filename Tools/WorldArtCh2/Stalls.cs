namespace WorldArtCh2;

/// <summary>Market stalls: striped awning + counter ("front", animated awning), rear cloth/shelf ("back"), and the goods on the counter.</summary>
public static class Stalls
{
    static Rgba H(string h) => Rgba.Hex(h);
    public const int W = 60, Ht = 56;

    // ---- small drawing helpers -------------------------------------------------------------------------------
    public static void Disc(Canvas c, double cx, double cy, double r, Rgba[] ramp, int seed = 0)
    {
        // dome-shaded little ball, light from top-left
        c.Paint((x, y) =>
        {
            double dx = x + .5 - cx, dy = y + .5 - cy; return dx * dx + dy * dy <= r * r;
        }, ramp[ramp.Length / 2], (int)(cx - r - 1), (int)(cy - r - 1), (int)(cx + r + 2), (int)(cy + r + 2));
        for (int y = (int)(cy - r - 1); y <= cy + r + 1; y++)
            for (int x = (int)(cx - r - 1); x <= cx + r + 1; x++)
            {
                double dx = x + .5 - cx, dy = y + .5 - cy;
                if (dx * dx + dy * dy > r * r) continue;
                double t = 0.5 - (dx * 0.5 + dy * 0.7) / r * 0.5 + (Fx.R(x, y, seed) - .5) * 0.08;
                c.Set(x, y, ramp[Math.Clamp((int)(t * ramp.Length), 0, ramp.Length - 1)]);
            }
    }

    static void Mound(Canvas c, int x, int y, int w, int h, Rgba[] ramp, int seed)
    {
        // heap of loose goods sitting on y (bottom row = y)
        for (int j = 0; j < h; j++)
        {
            double half = w / 2.0 * Math.Sqrt(Math.Max(0, 1 - Math.Pow((double)(h - 1 - j) / h, 2)));
            for (int i = 0; i < w; i++)
            {
                double dx = i + .5 - w / 2.0; if (Math.Abs(dx) > half) continue;
                double t = 0.45 + (h - j) / (double)h * 0.3 - dx / w * 0.5 + (Fx.R(x + i, y - j, seed) - .5) * 0.5;
                c.Set(x + i, y - h + 1 + j, ramp[Math.Clamp((int)(t * ramp.Length), 0, ramp.Length - 1)]);
            }
        }
    }

    /// <summary>Open wooden crate seen 3/4 from the front: x,y = top-left, contents are painted by the caller before calling Rim.</summary>
    public static void CrateFront(Canvas c, int x, int y, int w, int h)
    {
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                int tone = j == 0 ? 4 : j == h - 1 ? 1 : (j % 3 == 2 ? 2 : 3);
                if (i == 0) tone = Math.Min(5, tone + 1);
                if (i == w - 1) tone = Math.Max(0, tone - 1);
                c.Set(x + i, y + j, K.Wood[Math.Clamp(tone, 0, 5)]);
            }
        c.VL(x + 1, y + 1, h - 1, K.Wood[1]); c.VL(x + w - 2, y + 1, h - 1, K.Wood[1]);
    }

    static void Pine(Canvas c, int x, int y, int w, int h)
    {
        for (int j = 0; j < h; j++) for (int i = 0; i < w; i++)
            {
                int t = j == h - 1 ? 4 : j == 0 ? 1 : 3;
                if (j > 0 && j < h - 1 && (j == 2 && Fx.R(i / 7, 0, 13) > .3)) t = 2;
                if ((i + 3) % 11 == 0 && j > 0) t = Math.Max(1, t - 1);
                c.Set(x + i, y + j, K.Pine[t]);
            }
    }

    // ---- awning ------------------------------------------------------------------------------------------------
    static void Awning(Canvas c, Rgba[] col, Rgba[] cream, int frame, bool plain = false)
    {
        const int stripes = 8;
        for (int y = 2; y <= 15; y++)
        {
            double t = (y - 2) / 13.0;
            double xl = 7 * (1 - t), xr = W - 7 * (1 - t);
            for (int x = (int)xl; x < (int)Math.Ceiling(xr); x++)
            {
                double u = (x + .5 - xl) / (xr - xl); if (u < 0 || u > 1) continue;
                int idx = (int)(u * stripes); if (idx >= stripes) idx = stripes - 1;
                var ramp = (plain ? idx % 2 == 0 : idx % 2 == 0) ? col : cream;
                int tone = 3;
                if (u < 0.22) tone++;
                if (u > 0.72) tone--;
                if (y >= 14) tone--;
                if (y <= 4) tone++;
                double dth = Fx.Bay(x, y) * 0.9;
                if (dth > 0.25 && tone < 4) tone += 0;
                // subtle fabric sag creases at stripe seams
                double su = u * stripes - idx;
                if (su < 0.09 || su > 0.93) tone--;
                c.Set(x, y, ramp[Math.Clamp(tone, 0, ramp.Length - 1)]);
            }
        }
        // valance with scalloped hem; alternating scallops lift by 1px between frames = gentle flapping
        double sw = W / (double)(stripes * 2);   // scallop width
        int scal = (int)Math.Round(W / sw);
        for (int x = 0; x < W; x++)
        {
            int k = Math.Min(scal - 1, (int)((x + .5) / sw));
            double u = (x + .5 - k * sw) / sw;
            int stripe = Math.Min(stripes - 1, (int)((x + .5) / (W / (double)stripes)));
            var ramp = stripe % 2 == 0 ? col : cream;
            int wob = ((k + frame) & 1) == 0 ? 1 : 0;
            int bottom = 17 + (int)Math.Round(2.4 * Math.Sqrt(Math.Max(0, 1 - Math.Pow(2 * u - 1, 2)))) + wob;
            for (int y = 16; y <= bottom; y++)
            {
                int tone = 2;
                if (x < 8) tone++;
                if (x > W - 10) tone--;
                if (y == 16) tone--;
                if (y == bottom && y > 18) tone--;
                c.Set(x, y, ramp[Math.Clamp(tone, 0, ramp.Length - 1)]);
            }
        }
    }

    static void Posts(Canvas c, int bottom)
    {
        foreach (int px in new[] { 1, 56 })
        {
            Draw.Cyl(c, px, 19, 3, bottom - 19 + 1, K.Wood, px);
            c.Set(px, 19, K.Wood[4]); c.Set(px + 1, 19, K.Wood[3]);
        }
    }

    // ---- counter -----------------------------------------------------------------------------------------------------
    static void Counter(Canvas c, string kind)
    {
        Pine(c, 5, 36, 50, 6);                                       // top surface rows 36..41
        c.HL(5, 36, 50, K.Pine[0]);
        // front face
        Draw.Planks(c, 5, 42, 50, 14, K.Wood, 17, 5);
        c.HL(5, 42, 50, K.Wood[5]);
        for (int x = 5; x < 55; x++) c.Set(x, 55, K.Wood[0]);
        c.VL(5, 42, 14, K.Wood[4]); c.VL(54, 42, 14, K.Wood[1]);
        // cloth skirts on some stalls
        if (kind is "bread" or "spice" or "cloth" or "pots")
        {
            Rgba[] a = kind switch { "bread" => K.RedR, "spice" => K.Mustard, "cloth" => K.Purple, _ => K.BlueR };
            for (int x = 6; x < 54; x++)
            {
                int k = (x - 6) / 6, lx = (x - 6) % 6;
                int bot = 52 + (lx == 0 || lx == 5 ? 0 : 1) + (lx == 2 || lx == 3 ? 1 : 0);
                for (int y = 43; y <= bot; y++)
                {
                    int tone = (kind == "bread" ? ((x / 3 + y / 3) & 1) == 0 : (k & 1) == 0) ? 3 : 2;
                    if (kind == "bread") { tone = ((x / 3 + y / 3) & 1) == 0 ? 3 : 1; }
                    c.Set(x, y, kind == "bread" && tone == 1 ? K.Linen[3] : a[Math.Clamp(tone, 0, a.Length - 1)]);
                }
            }
        }
    }

    // ---- per kind goods ---------------------------------------------------------------------------------------------
    static readonly Rgba[] Carrot = { H("A8501F"), H("C8692A"), H("E0843A"), H("F0A458"), H("F8C380") };
    static readonly Rgba[] Tomato = { H("8A2F2F"), H("B03E3A"), H("D2554A"), H("E5705C"), H("F2927C") };
    static readonly Rgba[] Onion = { H("8C6A5A"), H("A9826C"), H("C79C7C"), H("DDB898"), H("EBCDB0") };
    static readonly Rgba[] Plum = { H("4E3A6A"), H("66508A"), H("7E68A6"), H("9984C0"), H("B6A4D8") };
    static readonly Rgba[] Pear = { H("86923C"), H("A6B04A"), H("C4CA5C"), H("DDE07C"), H("EEF0A0") };
    static readonly Rgba[] Orng = { H("C4601F"), H("DE7A28"), H("EE9A3C"), H("F8B85C"), H("FFD58C") };
    static readonly Rgba[] Bread = { H("7E4A24"), H("A0642F"), H("C0823F"), H("D9A056"), H("EBC17A") };
    static readonly Rgba[] Grain = { H("B8923F"), H("D2AE55"), H("E6C86E"), H("F2DC8C"), H("FAEBB0") };
    static readonly Rgba[] Sack = { H("9C8458"), H("B89E72"), H("D0B88C"), H("E2D0A8"), H("F0E2C0") };
    static readonly Rgba[] Clay = { H("8A4B2B"), H("A85F38"), H("C27A4B"), H("D8946A"), H("E8B088") };
    static readonly Rgba[] Purple5 = { H("4A3C6A"), H("5E4E80"), H("7C6AA3"), H("9F8CC4"), H("C0B2DF") };

    static void VegCrate(Canvas c, int x, int baseY, int w, string kind, bool thin, int seed, int faceH = 5)
    {
        // contents (behind front rim), then rim
        int topY = baseY - faceH;            // crate rim row
        if (!thin)
        {
            switch (kind)
            {
                case "carrot":
                    for (int i = 0; i < w - 2; i += 2) { for (int j = 0; j < 6; j++) c.Set(x + 1 + i, topY - j + 1, Carrot[Math.Clamp(2 + (j > 3 ? 1 : 0) - (i > w - 6 ? 1 : 0), 0, 4)]); c.Set(x + 2 + i, topY - 4 + (i % 4 == 0 ? 0 : 1), Carrot[1]); c.Set(x + 1 + i, topY - 6, K.Leaf[4]); c.Set(x + i, topY - 7, K.Leaf[3]); c.Set(x + 2 + i, topY - 7, K.Leaf[5]); }
                    break;
                case "cabbage":
                    Disc(c, x + 3.5, topY - 1.5, 3.6, K.LeafWarm, seed); Disc(c, x + w - 4, topY - 1.5, 3.6, K.Leaf.Skip(1).ToArray(), seed + 1); Disc(c, x + w / 2.0, topY - 4, 3.6, K.Fruit, seed + 2);
                    break;
                case "tomato":
                    for (int r = 0; r < 3; r++) for (int i = 0; i < (w - 2) / 3; i++) Disc(c, x + 2.5 + i * 3 + (r & 1) * 1.5, topY - 0.5 - r * 2.4, 1.9, Tomato, seed + r * 7 + i);
                    break;
                case "onion":
                    Mound(c, x + 1, topY + 1, w - 2, 6, Onion, seed); for (int i = 0; i < 3; i++) c.Set(x + 3 + i * 3, topY - 5, K.Leaf[4]);
                    break;
                case "apple":
                    for (int r = 0; r < 3; r++) for (int i = 0; i < (w - 2) / 3; i++) Disc(c, x + 2.5 + i * 3 + (r & 1) * 1.5, topY - 0.5 - r * 2.4, 1.9, K.RedR.Concat(new[] { H("F4A592") }).ToArray(), seed + r * 5 + i);
                    break;
                case "pear":
                    for (int r = 0; r < 3; r++) for (int i = 0; i < (w - 2) / 3; i++) Disc(c, x + 2.5 + i * 3 + (r & 1) * 1.5, topY - 0.5 - r * 2.4, 1.9, Pear, seed + r * 3 + i);
                    break;
                case "orange":
                    for (int r = 0; r < 3; r++) for (int i = 0; i < (w - 2) / 3; i++) Disc(c, x + 2.5 + i * 3 + (r & 1) * 1.5, topY - 0.5 - r * 2.4, 1.9, Orng, seed + r * 9 + i);
                    break;
                case "plum":
                    for (int r = 0; r < 2; r++) for (int i = 0; i < (w - 2) / 3; i++) Disc(c, x + 2.5 + i * 3 + (r & 1) * 1.5, topY - 0.5 - r * 2.4, 1.9, Plum, seed + r * 4 + i);
                    break;
            }
        }
        else
        {
            // thinned: a handful at the bottom, straw / planks showing
            c.HL(x + 1, topY + 1, w - 2, H("C9A862")); c.HL(x + 2, topY, w - 5, H("D8BC78"));
            switch (kind)
            {
                case "carrot": for (int i = 0; i < 2; i++) { for (int j = 0; j < 4; j++) c.Set(x + 3 + i * 3, topY - j + 1, Carrot[2]); c.Set(x + 3 + i * 3, topY - 3, K.Leaf[4]); } break;
                case "cabbage": Disc(c, x + w - 4.5, topY - 0.8, 3.2, K.LeafWarm, seed); break;
                case "tomato": Disc(c, x + 3, topY, 1.9, Tomato, seed); Disc(c, x + 6, topY + 0.5, 1.9, Tomato, seed + 1); break;
                default: Disc(c, x + w / 2.0, topY, 2.2, Onion, seed); break;
            }
        }
        CrateFront(c, x, topY + 1, w, faceH);
        c.HL(x, topY + 1, w, K.Wood[5]);
    }

    /// <summary>Standalone floor crate sprite with produce (or empty).</summary>
    public static Canvas FloorCrate(string kind, bool thin, int seed)
    {
        var c = new Canvas(22, 20);
        // back rim and inner wall
        c.Rect(1, 8, 20, 4, K.Wood[1]); c.HL(1, 8, 20, K.Wood[3]);
        if (kind != "empty") VegCrate(c, 1, 19, 20, kind, thin, seed, 8);
        else
        {
            CrateFront(c, 1, 11, 20, 8);
            c.HL(2, 9, 18, K.Wood[0]); c.HL(3, 10, 16, H("C9A862"));
            if (seed % 2 == 0) { c.Set(6, 9, H("D8BC78")); c.Set(14, 10, H("D8BC78")); }
        }
        c.Set(1, 19, K.Wood[0]);
        c.SelOutline(0.40, false);
        return c;
    }

    static void Rail(Canvas c, int y, int x0, int x1) { c.HL(x0, y, x1 - x0, K.Dark[3]); c.HL(x0, y + 1, x1 - x0, K.Dark[1]); }

    static void Hang(Canvas c, int x, int y, int len, Rgba col, Rgba hi) { c.VL(x, y, len, K.Dark[2]); c.Set(x, y + len, col); c.Set(x - 1, y + len, hi); c.Set(x + 1, y + len, col); c.Set(x, y + len + 1, col); }

    static void Lantern(Canvas c, int x, int y)
    {
        c.VL(x, y, 2, K.Iron[1]);
        c.Rect(x - 1, y + 2, 3, 4, K.Iron[2]);
        c.Rect(x, y + 3, 1, 2, K.Gold); c.Set(x, y + 3, H("FFF3B0"));
        c.Set(x - 1, y + 6, K.Iron[1]); c.Set(x, y + 6, K.Iron[1]); c.Set(x + 1, y + 6, K.Iron[1]);
    }

    static void Goods(Canvas c, string kind, bool thin, int frame)
    {
        switch (kind)
        {
            case "veg":
                // crates stood along the counter: carrots | cabbages | tomatoes
                VegCrate(c, 7, 41, 14, "carrot", thin, 3);
                VegCrate(c, 23, 41, 14, "cabbage", thin, 5);
                VegCrate(c, 39, 41, 14, "tomato", thin, 7);
                if (!thin)
                {
                    // onion braids hung from the awning + a basket of greens
                    for (int i = 0; i < 2; i++) { int hx = 14 + i * 28; c.VL(hx, 21, 6, K.Dark[2]); for (int j = 0; j < 4; j++) { Disc(c, hx - 1.5 + (j & 1) * 3, 26 + j * 1.2, 1.6, Onion, 30 + j); } }
                }
                else { c.VL(14, 21, 5, K.Dark[2]); Disc(c, 14, 27, 1.7, Onion, 31); }
                break;
            case "grain":
                for (int i = 0; i < 3; i++)
                {
                    int sx = 8 + i * 16, top = 31;
                    c.Rect(sx, top + 4, 13, 7, Sack[2]); c.VL(sx, top + 4, 7, Sack[3]); c.VL(sx + 12, top + 4, 7, Sack[1]); c.HL(sx, top + 10, 13, Sack[1]);
                    // rolled-down mouth with the grain heap
                    Mound(c, sx + 1, top + 5, 11, thin && i == 2 ? 2 : 5, Grain, 40 + i);
                    c.HL(sx, top + 4, 13, Sack[4]); c.HL(sx + 1, top + 5, 11, Sack[3]);
                    c.Set(sx, top + 4, Rgba.Clear);
                }
                // hanging scale
                Rail(c, 22, 8, 52);
                c.VL(30, 24, 3, K.Iron[2]); c.HL(24, 27, 13, K.Iron[3]); c.VL(24, 28, 2, K.Iron[2]); c.VL(36, 28, 2, K.Iron[2]);
                c.HL(22, 30, 5, K.Wood[3]); c.HL(34, 30, 5, K.Wood[3]);
                if (!thin) { c.Set(24, 29, K.Gold); c.Set(36, 29, K.Gold); }
                break;
            case "bread":
                // wicker trays: baguettes (upright), round loaves, rolls
                for (int i = 0; i < 3; i++)
                {
                    int bx = 7 + i * 16;
                    c.Rect(bx, 36, 14, 5, K.Pine[1]); c.HL(bx, 36, 14, K.Pine[3]);
                    for (int xx = 0; xx < 14; xx += 2) c.VL(bx + xx, 37, 4, K.Pine[0]);
                    if (i == 0) for (int b = 0; b < 4; b++) { int xx = bx + 1 + b * 3; for (int j = 0; j < 8; j++) c.Set(xx + (j > 3 ? 1 : 0), 36 - j, Bread[2 + (b & 1) - (j % 4 == 0 ? 1 : 0)]); c.Set(xx, 29, Bread[4]); }
                    else for (int b = 0; b < 3; b++)
                        {
                            Disc(c, bx + 3.5 + b * 4.2, 35 - (b & 1) * 0.8, 2.8 + (i == 2 ? 0 : 0.3), Bread, 50 + i * 3 + b);
                            c.Set(bx + 2 + b * 4, 33 - (b & 1), Bread[4]); c.Set(bx + 4 + b * 4, 34 - (b & 1), Bread[0]);
                        }
                }
                break;
            case "fruit":
                VegCrate(c, 7, 41, 14, "apple", false, 11);
                VegCrate(c, 23, 41, 14, "orange", false, 12);
                VegCrate(c, 39, 41, 14, "pear", false, 13);
                for (int j = 0; j < 3; j++) Disc(c, 28 + j * 3, 23, 1.6, Plum, 70 + j);
                c.VL(28, 21, 2, K.Dark[2]); c.HL(28, 21, 7, K.Leaf[2]);
                break;
            case "pots":
                // stacked jugs & bowls
                for (int i = 0; i < 4; i++)
                {
                    int jx = 8 + i * 12;
                    var ramp = i % 2 == 0 ? Clay : K.BlueR;
                    c.Ellipse(jx + 4, 37, 4.2, 3.6, ramp[2]);
                    Disc(c, jx + 4, 36.5, 3.8, ramp, 80 + i);
                    c.Rect(jx + 2, 31, 5, 3, ramp[2]); c.HL(jx + 2, 31, 5, ramp[4]); c.VL(jx + 6, 32, 2, ramp[1]);
                    c.Set(jx + 1, 34, ramp[0]); c.Set(jx + 7, 34, ramp[1]);
                    if (i % 2 == 1) { c.HL(jx + 1, 36, 7, ramp[4]); }
                }
                Rail(c, 22, 6, 54);
                for (int i = 0; i < 5; i++) { int hx = 10 + i * 9; c.VL(hx, 24, 2, K.Dark[2]); c.Rect(hx - 1, 26, 3, 3, i % 2 == 0 ? Clay[2] : K.BlueR[2]); c.Set(hx - 1, 26, i % 2 == 0 ? Clay[4] : K.BlueR[4]); }
                break;
            case "cloth":
                // bolts of cloth rolled and stacked, one drape hung
                Rgba[][] cols = { Purple5, K.TealR, K.Mustard, K.RedR };
                for (int i = 0; i < 4; i++)
                {
                    int bx = 7 + i * 12; var r = cols[i];
                    c.Rect(bx, 33, 10, 8, r[2]); c.HL(bx, 33, 10, r[3]); c.HL(bx, 40, 10, r[1]);
                    c.Rect(bx + 1, 30 + (i & 1), 8, 3, cols[(i + 1) % 4][2]); c.HL(bx + 1, 30 + (i & 1), 8, cols[(i + 1) % 4][3]);
                    c.Set(bx + 9, 36, r[1]); c.Set(bx, 36, r[4]);
                    for (int yy = 34; yy < 40; yy += 2) c.HL(bx + 1, yy, 8, ((yy / 2) & 1) == 0 ? r[3] : r[2]);
                }
                for (int i = 0; i < 3; i++) { int dx = 12 + i * 16; var r = cols[(i + 2) % 4]; for (int y = 21; y < 29 + (i == 1 ? 3 : 0) + (frame == 1 && i == 1 ? 1 : 0); y++) { c.Set(dx, y, r[3]); c.Set(dx + 1, y, r[2]); c.Set(dx + 2, y, r[1]); } }
                break;
            case "spice":
                // low bowls of coloured spices, small sacks behind
                Rgba[][] sp = { Tomato, K.Mustard, K.LeafWarm, Carrot, K.Stone, Tomato };
                for (int i = 0; i < 5; i++)
                {
                    int bx = 6 + i * 10;
                    Mound(c, bx, 37, 8, 4, sp[i % sp.Length].Length >= 5 ? sp[i % sp.Length] : sp[0], 90 + i);
                    c.Rect(bx, 38, 8, 3, K.Wood[2]); c.HL(bx, 38, 8, K.Wood[4]); c.HL(bx, 40, 8, K.Wood[0]); c.VL(bx, 38, 3, K.Wood[3]);
                }
                for (int i = 0; i < 2; i++) { int sx = 14 + i * 24; c.Rect(sx, 28, 8, 8, Sack[2]); c.HL(sx, 28, 8, Sack[4]); c.VL(sx + 7, 29, 7, Sack[1]); c.Set(sx, 28, Rgba.Clear); c.Set(sx + 3, 27, Sack[3]); c.Set(sx + 4, 27, Sack[2]); Mound(c, sx + 1, 29, 6, 2, i == 0 ? Tomato : K.Mustard, 95 + i); }
                // dried chilli / garlic strings
                for (int i = 0; i < 4; i++) { int hx = 10 + i * 13; c.VL(hx, 21, 3, K.Dark[2]); for (int j = 0; j < 3; j++) { c.Set(hx - 1 + (j & 1) * 2, 24 + j, i % 2 == 0 ? Tomato[2] : Onion[4]); c.Set(hx, 24 + j, i % 2 == 0 ? Tomato[1] : Onion[3]); } }
                break;
        }
    }

    // ---- public builders -----------------------------------------------------------------------------------------------
    public static (Rgba[] col, Rgba[] cream) Colours(string kind) => kind switch
    {
        "veg" => (K.LeafWarm.Concat(new[] { K.Leaf[5] }).ToArray(), K.Linen),
        "grain" => (K.Mustard, K.Linen),
        "bread" => (K.RedR, K.Linen),
        "fruit" => (K.TealR, K.Linen),
        "pots" => (K.BlueR, K.Linen),
        "cloth" => (Purple5, K.Linen),
        "spice" => (K.Roof, K.Linen),
        _ => (K.Linen, K.Linen),
    };

    public static Canvas Front(string kind, int frame, bool thin = false)
    {
        var c = new Canvas(W, Ht);
        var (col, cream) = Colours(kind);
        Posts(c, 55);
        Counter(c, kind);
        Goods(c, kind, thin, frame);
        // lantern on one post
        Awning(c, col, cream, frame);
        if (kind is "veg" or "bread" or "spice") Lantern(c, 58, 20);
        c.SelOutline(0.40, false);
        return c;
    }

    public static Canvas Back(string kind)
    {
        var c = new Canvas(W, 36);
        var (col, _) = Colours(kind);
        // hanging backcloth between the rear posts, plank shelf below
        for (int y = 4; y < 36; y++)
            for (int x = 4; x < 56; x++)
            {
                bool cloth = y < 22;
                Rgba px;
                if (cloth)
                {
                    int tone = 1 + ((x / 6) & 1);
                    if (y > 18) tone = Math.Max(0, tone - 1);
                    px = col[Math.Clamp(tone, 0, col.Length - 1)];
                    px = Rgba.Mul(px, 0.78, 0.74, 0.84);
                }
                else px = Rgba.Mul(K.Wood[1 + (y % 6 < 3 ? 1 : 0)], 0.86, 0.82, 0.9);
                c.Set(x, y, px);
            }
        c.HL(4, 22, 52, K.Wood[3]); c.HL(4, 23, 52, K.Wood[0]);
        Draw.Cyl(c, 3, 8, 2, 28, K.Wood, 5); Draw.Cyl(c, 55, 8, 2, 28, K.Wood, 6);
        // shelf stock
        for (int i = 0; i < 8; i++) { int sx = 8 + i * 6; c.Rect(sx, 19, 3, 3, Rgba.Mul(i % 2 == 0 ? Clay[2] : Sack[2], 0.8, 0.76, 0.86)); }
        c.SelOutline(0.4, false);
        return c;
    }

    /// <summary>The stall that stands unattended: awning rolled up on its frame, bare counter, a few bundled crates.</summary>
    public static Canvas ClosedFront(int frame)
    {
        var c = new Canvas(W, Ht);
        Posts(c, 55);
        Counter(c, "closed");
        // frame beam + rolled-up cloth
        c.Rect(2, 14, 56, 3, K.Wood[2]); c.HL(2, 14, 56, K.Wood[4]); c.HL(2, 16, 56, K.Wood[0]);
        for (int x = 6; x < 54; x++)
        {
            for (int y = 8; y <= 13; y++)
            {
                int tone = y <= 9 ? 4 : y >= 12 ? 1 : 3;
                var r = ((x / 6) & 1) == 0 ? K.Linen : K.Stone;
                c.Set(x, y, r[Math.Clamp(tone, 0, r.Length - 1)]);
            }
        }
        for (int x = 8; x < 54; x += 12) { c.VL(x, 8, 6, K.Brown); }
        c.Set(5, 10, Rgba.Clear);
        // bare counter: one folded cloth and an empty crate, a tied board "closed" sign
        c.Rect(10, 31, 14, 3, K.Linen[3]); c.HL(10, 31, 14, K.Linen[4]); c.HL(10, 33, 14, K.Linen[1]);
        c.Rect(11, 28, 12, 3, K.Linen[2]); c.HL(11, 28, 12, K.Linen[4]);
        CrateFront(c, 36, 34, 14, 7); c.HL(37, 35, 12, K.Wood[0]);
        // hanging board with squiggle
        c.VL(31, 17, 4, K.Dark[2]); c.VL(41, 17, 4, K.Dark[2]);
        c.Rect(29, 21, 14, 8, K.Wood[3]); c.HL(29, 21, 14, K.Wood[5]); c.HL(29, 28, 14, K.Wood[1]);
        for (int x = 31; x < 41; x += 2) { c.Set(x, 24 + (x % 4 == 1 ? 0 : 1), K.Dark[1]); c.Set(x + 1, 24 + (x % 4 == 1 ? 1 : 0), K.Dark[1]); }
        c.SelOutline(0.40, false);
        return c;
    }

    public static void Build()
    {
        S.Cur = "stalls";
        foreach (var kind in new[] { "veg", "grain", "bread", "fruit", "pots", "cloth", "spice" })
        {
            for (int f = 0; f < 2; f++) S.Add($"prop_c2_stall_{kind}_{f}", Front(kind, f));
            S.Add($"prop_c2_stallback_{kind}", Back(kind));
        }
        for (int f = 0; f < 2; f++) S.Add($"prop_c2_stall_veg_thin_{f}", Front("veg", f, true));
        for (int f = 0; f < 2; f++) S.Add($"prop_c2_stall_closed_{f}", ClosedFront(f));
        S.Add("prop_c2_stallback_closed", Back("closed"));
    }
}
