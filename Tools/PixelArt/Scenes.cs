namespace PixelArt;

/// <summary>Group E: 96x56 chapter thumbnails (plus shared scene helpers used by the memory vignettes).</summary>
public static class Scenes
{
    // ------------------------------------------------------------------ helpers
    internal static void Layer(Canvas dst, Action<Canvas> draw, Rgba? outline = null)
    {
        var l = new Canvas(dst.W, dst.H); draw(l);
        if (outline.HasValue) l.Outline(outline.Value);
        dst.Blit(l, 0, 0);
    }

    internal static void Tent(Canvas c, double cx, int baseY, int w, int h, Rgba body, Rgba shade, Rgba door, Rgba? outline = null)
    {
        Layer(c, l =>
        {
            l.Tri(cx - w / 2.0, baseY, cx, baseY - h, cx + w / 2.0, baseY, body);
            l.Tri(cx, baseY - h, cx + w / 2.0, baseY, cx, baseY, shade);
            l.Tri(cx - w * 0.13, baseY, cx, baseY - h * 0.58, cx + w * 0.13, baseY, door);
            l.Set((int)Math.Floor(cx), baseY - h, shade);
        }, outline);
    }

    /// <summary>Concentric hard-edged glow; rings are listed outermost first.</summary>
    internal static void Glow(Canvas c, double cx, double cy, params (double r, Rgba col, bool dither)[] rings)
    {
        foreach (var (r, col, dither) in rings)
        {
            if (dither) c.DitherDisc(cx, cy, r, col, 0); else c.Disc(cx, cy, r, col);
        }
    }

    internal static void Person(Canvas c, int x, int baseY, int h, Rgba col)
    {
        c.Disc(x + .5, baseY - h + 1.8, 1.8, col);
        c.Rect(x - 1, baseY - h + 3, 3, h - 6, col);
        c.Rect(x - 1, baseY - 3, 1, 3, col); c.Rect(x + 1, baseY - 3, 1, 3, col);
    }

    static void Add(string n, Canvas c) { c.Frame(0, 0, 96, 56, P.Ink); Registry.Add("E_chapters", n, c, 96, 56); }

    // ------------------------------------------------------------------ chapter 1: warm morning
    static Canvas Ch1()
    {
        var c = new Canvas(96, 56);
        c.Bands(0, 0, 96, new[] { 9, 8, 8, 10 }, new[] { P.LightBlue, P.PaleSky, P.Cream, P.Parchment });
        // clouds
        foreach (var (x, y, r) in new[] { (16, 9, 4.0), (21, 8, 3.0), (11, 10, 3.0), (52, 6, 3.5), (57, 7, 2.5), (47, 7, 2.5) })
            c.Ellipse(x, y, r * 1.5, r * .7, P.Cream);
        // sun with short rays
        c.Disc(74, 21, 6.5, P.Gold); c.Disc(74, 21, 4.6, P.LightGold);
        for (int i = 0; i < 8; i++)
        {
            double a = i * Math.PI / 4;
            c.Line((int)Math.Round(74 + 9.5 * Math.Cos(a)), (int)Math.Round(21 + 9.5 * Math.Sin(a)),
                   (int)Math.Round(74 + 12 * Math.Cos(a)), (int)Math.Round(21 + 12 * Math.Sin(a)), P.Gold);
        }
        // hills and meadow
        c.Poly(new[] { (0.0, 36.0), (0.0, 31.0), (12.0, 28.0), (26.0, 31.0), (42.0, 28.0), (58.0, 32.0), (76.0, 29.0), (96.0, 32.0), (96.0, 36.0) }, P.Green);
        c.Dither(0, 31, 96, 1, P.Cream, 1);
        c.Poly(new[] { (0.0, 36.0), (0.0, 34.0), (20.0, 32.0), (50.0, 34.0), (70.0, 32.0), (96.0, 34.0), (96.0, 36.0) }, P.DarkGreen);
        c.Rect(0, 36, 96, 20, P.Green);
        c.Dither(0, 36, 96, 2, P.DarkGreen, 0);
        foreach (var (x, y) in new[] { (4, 40), (14, 45), (30, 49), (38, 42), (8, 52), (40, 53), (52, 39), (88, 40), (70, 41) }) { c.Set(x, y, P.LightGreen); c.Set(x + 1, y, P.LightGreen); c.Set(x, y - 1, P.LightGreen); }
        // sand path from the door
        c.Poly(new[] { (22.0, 40.0), (27.0, 40.0), (36.0, 55.0), (14.0, 55.0) }, P.Sand);
        c.Poly(new[] { (27.0, 40.0), (28.0, 40.0), (37.0, 55.0), (36.0, 55.0) }, P.Tan);
        // tree
        Layer(c, l =>
        {
            l.Rect(84, 32, 3, 9, P.Brown);
            l.Disc(85.5, 26, 8.5, P.DarkGreen); l.Disc(82, 23, 4.5, P.Green); l.Disc(88.5, 28, 3.5, P.Green);
            l.Set(81, 21, P.LightGreen); l.Set(82, 21, P.LightGreen);
        }, P.Ink);
        // garden rows with sprouts, low fence
        for (int r = 0; r < 3; r++)
        {
            int y = 45 + r * 4;
            c.Rect(44, y, 48, 2, P.MidBrown); c.Rect(44, y + 1, 48, 1, P.Brown);
            for (int x = 46; x < 91; x += 4) { c.Set(x, y - 1, P.LightGreen); c.Set(x, y - 2, P.DarkGreen); c.Set(x + 1, y - 2, P.Green); c.Set(x - 1, y - 2, P.Green); }
        }
        c.Rect(42, 42, 52, 1, P.Tan);
        for (int x = 42; x < 94; x += 6) c.Rect(x, 39, 1, 5, P.Sand);
        // house
        Layer(c, l =>
        {
            l.Rect(31, 14, 4, 9, P.Brown); l.Rect(30, 13, 6, 2, P.MidBrown);
            l.Rect(9, 25, 28, 15, P.Cream);
            l.Rect(33, 25, 4, 15, P.Parchment);
            l.Rect(9, 39, 28, 1, P.Sand);
            l.Poly(new[] { (6.0, 26.5), (23.0, 12.5), (40.0, 26.5) }, P.Terracotta);
            l.Poly(new[] { (23.0, 12.5), (40.0, 26.5), (23.0, 26.5) }, P.DarkTerra);
            l.Rect(7, 26, 32, 1, P.DarkTerra);
            l.Rect(20, 30, 6, 10, P.MidBrown); l.Rect(21, 31, 4, 9, P.Orange); l.Set(24, 35, P.Gold);
            foreach (var wx in new[] { 12, 29 })
            {
                l.Rect(wx, 29, 6, 6, P.Brown); l.Rect(wx + 1, 30, 4, 4, P.LightBlue);
                l.Rect(wx + 2, 30, 1, 4, P.Brown); l.Rect(wx + 1, 31, 4, 1, P.Brown);
                l.Set(wx + 1, 30, P.White); l.Rect(wx - 1, 35, 8, 2, P.Terracotta);
                l.Set(wx, 34, P.Gold); l.Set(wx + 2, 34, P.Cream); l.Set(wx + 4, 34, P.Gold); l.Set(wx + 1, 34, P.Green); l.Set(wx + 3, 34, P.Green);
            }
        }, P.Ink);
        // chimney smoke
        c.DitherDisc(33.5, 10, 2.4, P.Cream, 0); c.DitherDisc(36, 6.5, 2.0, P.Cream, 1);
        return c;
    }

    // ------------------------------------------------------------------ chapter 2: market
    static Canvas Ch2()
    {
        var c = new Canvas(96, 56);
        c.Bands(0, 0, 96, new[] { 7, 7, 8 }, new[] { P.LightBlue, P.PaleSky, P.Cream });
        // back buildings
        c.Rect(0, 8, 28, 26, P.Parchment); c.Rect(0, 8, 28, 3, P.Terracotta);
        c.Rect(28, 12, 40, 22, P.Sand); c.Rect(28, 12, 40, 3, P.Orange);
        c.Rect(68, 6, 28, 28, P.Parchment); c.Rect(68, 6, 28, 3, P.Teal);
        foreach (var x in new[] { 5, 15, 33, 45, 57, 73, 85 }) { c.Rect(x, 18, 5, 7, P.Brown); c.Rect(x + 1, 19, 3, 5, P.DarkBlue); c.Rect(x - 1, 25, 7, 1, P.Tan); }
        c.Rect(0, 30, 96, 5, P.Tan);
        // bunting
        for (int i = 0; i < 16; i++)
        {
            int x = 2 + i * 6; int y = 11 + (int)Math.Round(2.5 * Math.Sin(i * Math.PI / 5.0));
            var col = new[] { P.Terracotta, P.Gold, P.Teal, P.Cream }[i % 4];
            c.Tri(x, y, x + 5, y, x + 2.5, y + 4, col);
            c.Rect(x, y - 1, 6, 1, P.Brown);
        }
        // street
        c.Rect(0, 35, 96, 21, P.Sand);
        c.Dither(0, 35, 96, 21, P.Parchment, 0);
        foreach (var (x, y) in new[] { (6, 52), (20, 50), (42, 53), (60, 51), (80, 53), (90, 49), (32, 47) }) { c.Rect(x, y, 3, 1, P.Tan); c.Set(x + 1, y + 1, P.Tan); }
        // stalls
        var cols = new[] { (P.Terracotta, P.Cream), (P.Teal, P.Cream), (P.Orange, P.Cream) };
        for (int s = 0; s < 3; s++)
        {
            int x = 3 + s * 32;
            var (A, B) = cols[s];
            Layer(c, l =>
            {
                l.Rect(x + 1, 22, 2, 22, P.Brown); l.Rect(x + 23, 22, 2, 22, P.Brown);
                l.Rect(x + 2, 37, 22, 2, P.LightBrown);                      // table top
                l.Rect(x + 2, 39, 22, 6, A);                                  // cloth front
                l.Rect(x + 2, 39, 22, 1, B);
                for (int px = x + 2; px < x + 24; px += 4) { l.Rect(px, 45, 2, 1, A); }
                // awning
                l.Poly(new[] { (x + 1.0, 14.0), (x + 25.0, 14.0), (x + 28.0, 23.0), (x - 2.0, 23.0) }, A);
                l.Paint((px, py) => ((px - x + 3) / 3) % 2 == 1 && l.Get(px, py) == A, B);
                l.Rect(x - 2, 23, 30, 1, A);
                l.Paint((px, py) => py == 23 && ((px - x + 3) / 3) % 2 == 1, B);
                for (int px = x - 2; px < x + 28; px++) if (((px - x + 3) / 3) % 2 == 0 && px % 3 == 1) l.Set(px, 24, A); else if (((px - x + 3) / 3) % 2 == 1 && px % 3 == 1) l.Set(px, 24, B);
                l.Rect(x + 1, 13, 25, 1, P.Brown);
            }, P.Ink);
        }
        // goods: apples, greens, loaves
        for (int i = 0; i < 4; i++) { c.Disc(8 + i * 5, 34, 2.6, i % 2 == 0 ? P.Terracotta : P.Orange); c.Set(7 + i * 5, 33, P.Cream); }
        c.Rect(7, 35, 22, 2, P.Brown);
        for (int i = 0; i < 5; i++) { c.Disc(41 + i * 4, 34, 2.4, i % 2 == 0 ? P.Green : P.DarkGreen); }
        c.Rect(39, 35, 22, 2, P.Brown);
        for (int i = 0; i < 3; i++) { c.Ellipse(73 + i * 7, 34, 3.5, 2.5, P.Orange); c.Set(72 + i * 7, 33, P.LightOrange); c.Set(74 + i * 7, 34, P.DarkOrange); }
        c.Rect(71, 35, 22, 2, P.Brown);
        // crates and sacks in the foreground
        Layer(c, l =>
        {
            for (int k = 0; k < 2; k++)
            {
                int x = k == 0 ? 8 : 70;
                l.Rect(x, 47, 14, 8, P.Tan);
                l.Rect(x, 47, 14, 1, P.Sand);
                l.Rect(x, 50, 14, 1, P.MidBrown); l.Rect(x, 53, 14, 1, P.MidBrown);
                l.Rect(x, 47, 1, 8, P.MidBrown); l.Rect(x + 13, 47, 1, 8, P.MidBrown); l.Rect(x + 6, 47, 2, 8, P.MidBrown);
                for (int i = 0; i < 4; i++) l.Disc(x + 2.5 + i * 3, 46, 1.8, k == 0 ? P.Orange : P.Green);
            }
            l.Rect(44, 49, 12, 6, P.Tan); l.Rect(44, 49, 12, 1, P.Sand); l.Rect(44, 52, 12, 1, P.MidBrown);
            l.Disc(50, 47, 3.5, P.Parchment);
        }, P.Ink);
        return c;
    }

    // ------------------------------------------------------------------ chapter 3: dusk, dim (muted)
    static Canvas Ch3()
    {
        var c = new Canvas(96, 56);
        c.Bands(0, 0, 96, new[] { 9, 9, 9, 10 }, new[] { P.Night, P.DeepSlate, P.DarkSlate, P.MutedBlue });
        foreach (var (x, y) in new[] { (8, 4), (20, 9), (41, 3), (60, 7), (77, 2), (88, 11), (31, 12), (52, 15) }) c.Set(x, y, P.LightSlate);
        // far skyline, uneven rooflines
        var far = new (int x, int w, int top, int slope)[] { (0, 9, 28, 0), (9, 7, 22, 2), (16, 10, 30, -2), (26, 8, 24, 0), (34, 12, 19, 3), (46, 7, 27, 0), (53, 9, 23, -3), (62, 10, 29, 0), (72, 8, 21, 2), (80, 7, 26, 0), (87, 9, 24, -2) };
        foreach (var (x, w, top, s) in far)
            c.Poly(new[] { (x * 1.0, top + (s > 0 ? s : 0.0)), (x + w * 1.0, top + (s < 0 ? -s : 0.0)), (x + w * 1.0, 46.0), (x * 1.0, 46.0) }, P.DarkSlate);
        // near skyline
        var near = new (int x, int w, int top, int slope)[] { (0, 13, 31, 0), (13, 8, 37, 2), (21, 9, 28, -3), (30, 8, 36, 0), (38, 10, 32, 3), (48, 7, 39, 0), (86, 10, 33, -2), (80, 6, 38, 0) };
        foreach (var (x, w, top, s) in near)
        {
            c.Poly(new[] { (x * 1.0, top + (s > 0 ? s : 0.0)), (x + w * 1.0, top + (s < 0 ? -s : 0.0)), (x + w * 1.0, 47.0), (x * 1.0, 47.0) }, P.Night);
            for (int wy = top + 5; wy < 44; wy += 6) for (int wx = x + 2; wx < x + w - 2; wx += 4) c.Rect(wx, wy, 2, 3, P.Ink);
        }
        // ground
        c.Rect(0, 46, 96, 10, P.Ink);
        c.Dither(0, 46, 96, 1, P.Night, 0);
        foreach (var (x, y, w) in new[] { (4, 50, 6), (24, 52, 8), (46, 49, 5), (88, 51, 6), (14, 54, 4) }) c.Rect(x, y, w, 1, P.Night);
        // low shelter with a tiny lit window
        c.Poly(new[] { (54.0, 40.0), (60.0, 34.0), (80.0, 34.0), (85.0, 40.0) }, P.DeepBrown);
        c.Rect(57, 40, 25, 7, P.DarkGrey);
        c.Rect(57, 40, 25, 1, P.Brown);
        c.Rect(62, 42, 6, 5, P.Ink);
        c.Dither(70, 40, 8, 6, P.DimOchre, 0);
        c.Rect(72, 41, 4, 3, P.Orange); c.Rect(73, 42, 2, 1, P.Gold);
        c.Rect(71, 40, 6, 1, P.Brown); c.Rect(71, 44, 6, 1, P.Brown);
        c.Dither(69, 47, 8, 2, P.Brown, 0); // faint spill on the ground
        return c;
    }

    // ------------------------------------------------------------------ chapter 4: road at dusk (muted)
    static Canvas Ch4()
    {
        var c = new Canvas(96, 56);
        c.Bands(0, 0, 96, new[] { 7, 7, 7, 9 }, new[] { P.DeepSlate, P.DarkSlate, P.MutedBlue, P.DimOchre });
        c.Poly(new[] { (0.0, 30.0), (0.0, 26.0), (14.0, 24.0), (30.0, 28.0), (50.0, 23.0), (72.0, 27.0), (96.0, 24.0), (96.0, 30.0) }, P.DarkSlate);
        c.Rect(0, 30, 96, 26, P.DarkGrey);
        c.Dither(0, 31, 96, 24, P.Brown, 0);
        c.Rect(0, 30, 96, 1, P.Grey);
        // road converging on the horizon
        c.Poly(new[] { (58.0, 30.0), (63.0, 30.0), (96.0, 56.0), (24.0, 56.0) }, P.Grey);
        c.Poly(new[] { (58.0, 30.0), (59.5, 30.0), (36.0, 56.0), (24.0, 56.0) }, P.DarkGrey);
        c.Paint((x, y) => ((x + y) & 1) == 0 && c.Get(x, y) == P.Grey && x < 70 + (y - 30) * 0.9 && y > 38, P.LightGrey);
        foreach (var (x, y, w) in new[] { (60, 34, 1), (60, 39, 2), (61, 45, 2), (61, 52, 3) }) c.Rect(x, y, w, 2, P.LightGrey);
        // small house left
        Layer(c, l =>
        {
            l.Rect(7, 27, 18, 12, P.Grey);
            l.Rect(21, 27, 4, 12, P.DarkGrey);
            l.Poly(new[] { (5.0, 28.0), (16.0, 19.0), (27.0, 28.0) }, P.DarkGrey);
            l.Poly(new[] { (16.0, 19.0), (27.0, 28.0), (16.0, 28.0) }, P.Brown);
            l.Rect(13, 31, 5, 8, P.Brown);
            l.Rect(9, 30, 3, 3, P.Gold); l.Rect(9, 30, 3, 1, P.Orange);      // the one warm window
            l.Rect(21, 19, 3, 6, P.DarkGrey);
        }, P.Ink);
        c.Rect(2, 40, 28, 1, P.DarkGrey);
        for (int x = 3; x < 30; x += 5) c.Rect(x, 38, 1, 3, P.Grey);
        // bare tree
        Layer(c, l =>
        {
            l.Rect(84, 22, 2, 14, P.Ink);
            l.Line(85, 25, 90, 19, P.Ink); l.Line(85, 28, 80, 22, P.Ink); l.Line(85, 23, 83, 17, P.Ink); l.Line(86, 27, 91, 25, P.Ink);
        });
        // lone figure with a bag, walking away
        Layer(c, l =>
        {
            l.Disc(62.5, 37.2, 1.9, P.Ink);
            l.Poly(new[] { (60.0, 39.0), (65.0, 39.0), (66.0, 47.0), (59.0, 47.0) }, P.Ink);
            l.Rect(60, 47, 2, 4, P.Ink); l.Rect(63, 47, 2, 4, P.Ink);
            l.Rect(66, 41, 3, 5, P.DarkSlate); l.Rect(66, 40, 1, 2, P.Ink);
        });
        c.Rect(58, 51, 10, 1, P.DarkGrey);
        return c;
    }

    // ------------------------------------------------------------------ chapter 5: tents on dry ground (muted)
    static Canvas Ch5()
    {
        var c = new Canvas(96, 56);
        c.Bands(0, 0, 96, new[] { 9, 9, 8 }, new[] { P.LightSlate, P.PaleMuted, P.LightGrey });
        c.Rect(0, 26, 96, 30, P.DimOchre);
        c.Dither(0, 26, 96, 30, P.LightGrey, 0);
        c.Rect(0, 26, 96, 2, P.Grey);
        c.Dither(0, 28, 96, 6, P.Grey, 1);
        foreach (var (x, y, w) in new[] { (6, 48, 9), (28, 53, 12), (70, 50, 10), (84, 54, 8), (52, 47, 6), (14, 38, 7), (60, 40, 5) })
        { c.Rect(x, y, w, 1, P.DarkGrey); c.Set(x + 2, y + 1, P.DarkGrey); c.Set(x + w - 1, y - 1, P.DarkGrey); }
        // far row then near row of tents
        var back = new[] { (14, P.Grey, P.DarkGrey), (46, P.LightSlate, P.MutedBlue), (78, P.Grey, P.DarkGrey) };
        foreach (var (cx, b, s) in back) Tent(c, cx, 38, 20, 11, b, s, P.Ink, P.Ink);
        var front = new[] { (10, P.MutedBlue, P.DarkSlate), (34, P.PaleMuted, P.LightGrey), (62, P.MutedBlue, P.DarkSlate), (88, P.PaleMuted, P.LightGrey) };
        foreach (var (cx, b, s) in front) Tent(c, cx, 49, 24, 15, b, s, P.Ink, P.Ink);
        // small cooking fire (the one warm accent)
        c.Rect(46, 51, 8, 2, P.DarkGrey);
        foreach (var x in new[] { 45, 47, 49, 51, 53 }) c.Rect(x, 52, 2, 2, P.Grey);
        c.Tri(46.5, 52, 50, 44.5, 53.5, 52, P.Orange);
        c.Tri(48, 52, 50, 47, 52, 52, P.Gold);
        c.Rect(49, 49, 1, 3, P.LightGold);
        c.DitherDisc(50, 40, 2.2, P.PaleMuted, 0); c.DitherDisc(51, 35, 2.6, P.LightGrey, 1);
        return c;
    }

    // ------------------------------------------------------------------ chapter 6: a room, light returning
    static Canvas Ch6()
    {
        var c = new Canvas(96, 56);
        c.Rect(0, 0, 96, 40, P.Tan);
        c.Dither(0, 0, 96, 38, P.Sand, 0);
        for (int x = 0; x < 96; x += 8) c.Rect(x, 0, 1, 38, P.Tan);
        // lamp glow
        Glow(c, 63, 27, (21, P.Sand, true), (15, P.Sand, false), (15, P.Parchment, true), (9, P.Parchment, false), (9, P.Cream, true));
        // floor
        c.Rect(0, 40, 96, 16, P.MidBrown);
        for (int y = 44; y < 56; y += 4) c.Rect(0, y, 96, 1, P.Brown);
        for (int x = 6; x < 96; x += 17) c.Rect(x, 40, 1, 4, P.Brown);
        c.Rect(0, 37, 96, 3, P.Brown); c.Rect(0, 37, 96, 1, P.LightBrown);
        // window with a dawn glow inside
        c.Rect(7, 7, 28, 28, P.Brown);
        c.Rect(9, 9, 24, 24, P.DarkBlue);
        c.Bands(9, 9, 24, new[] { 6, 6, 6, 6 }, new[] { P.DarkBlue, P.Blue, P.Orange, P.Gold });
        c.Disc(25, 25, 3.2, P.LightGold);
        c.Rect(9, 25, 24, 8, P.DarkGreen); c.Poly(new[] { (9.0, 26.0), (17.0, 24.0), (26.0, 27.0), (33.0, 25.0), (33.0, 33.0), (9.0, 33.0) }, P.DarkTeal);
        c.Rect(20, 9, 2, 24, P.Brown); c.Rect(9, 20, 24, 2, P.Brown);
        c.Rect(5, 35, 32, 3, P.Sand); c.Rect(5, 35, 32, 1, P.Cream);
        // window light on the floor
        c.Poly(new[] { (9.0, 40.0), (33.0, 40.0), (42.0, 55.0), (12.0, 55.0) }, P.Sand);
        c.Paint((x, y) => ((x + y) & 1) == 0 && y >= 40 && c.Get(x, y) == P.Sand, P.MidBrown);
        // door, closed
        Layer(c, l =>
        {
            l.Rect(73, 9, 20, 31, P.Brown);
            l.Rect(75, 11, 16, 29, P.MidBrown);
            l.Rect(77, 13, 5, 12, P.Brown); l.Rect(84, 13, 5, 12, P.Brown);
            l.Rect(77, 28, 5, 10, P.Brown); l.Rect(84, 28, 5, 10, P.Brown);
            l.Rect(75, 11, 16, 1, P.LightBrown);
            l.Rect(87, 26, 2, 3, P.Gold); l.Set(87, 26, P.LightGold);
        }, P.Ink);
        // desk with books and lamp
        Layer(c, l =>
        {
            l.Rect(38, 34, 34, 3, P.LightBrown);
            l.Rect(38, 37, 34, 4, P.MidBrown);
            l.Rect(40, 41, 3, 12, P.Brown); l.Rect(67, 41, 3, 12, P.Brown);
            l.Rect(43, 29, 14, 5, P.Terracotta); l.Rect(43, 29, 14, 1, P.Orange); l.Rect(45, 31, 10, 1, P.Gold);
            l.Rect(44, 24, 12, 5, P.Teal); l.Rect(44, 24, 12, 1, P.LightTeal); l.Rect(46, 26, 8, 1, P.Cream);
            l.Rect(45, 20, 10, 4, P.Gold); l.Rect(45, 20, 10, 1, P.LightGold); l.Rect(47, 22, 6, 1, P.DarkGold);
            l.Rect(59, 31, 8, 3, P.Brown); l.Rect(62, 24, 2, 8, P.Brown);
            l.Poly(new[] { (57.0, 24.0), (69.0, 24.0), (66.0, 17.0), (60.0, 17.0) }, P.Gold);
            l.Rect(60, 17, 6, 1, P.LightGold); l.Rect(57, 24, 12, 1, P.DarkGold);
        }, P.Ink);
        // small rug
        c.Rect(28, 49, 42, 5, P.DarkTerra); c.Rect(29, 50, 40, 3, P.Terracotta);
        for (int x = 31; x < 68; x += 6) c.Rect(x, 51, 3, 1, P.Cream);
        c.Rect(70, 50, 2, 3, P.Cream);
        return c;
    }

    public static void Build()
    {
        Add("chapter_1", Ch1());
        Add("chapter_2", Ch2());
        Add("chapter_3", Ch3());
        Add("chapter_4", Ch4());
        Add("chapter_5", Ch5());
        Add("chapter_6", Ch6());
        Memories.Build();
    }
}
