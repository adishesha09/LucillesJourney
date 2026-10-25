namespace PixelArt;

/// <summary>Group F: 64x40 memory vignettes (no text anywhere).</summary>
public static class Memories
{
    static Canvas M() => new(64, 40);
    static void Obj(Canvas c, Action<Canvas> draw) => Scenes.Layer(c, draw, P.Ink);

    static void Add(string n, Canvas c, Rgba? border = null)
    {
        c.Frame(0, 0, 64, 40, border ?? P.Ink);
        Registry.Add("F_memories", n, c, 64, 40);
    }

    static void Sack(Canvas c, int cx, int baseY, int w, int h)
    {
        Obj(c, l =>
        {
            l.Ellipse(cx, baseY - h + 5, w / 2.0, 4.5, P.Parchment);
            l.Rect(cx - w / 2, baseY - h + 5, w, h - 5, P.Parchment);
            l.Set(cx - w / 2, baseY - 1, Rgba.Clear); l.Set(cx + w / 2 - 1, baseY - 1, Rgba.Clear);
            l.Rect(cx + w / 2 - 3, baseY - h + 6, 3, h - 7, P.DarkParch);
            l.Rect(cx - 2, baseY - h, 4, 3, P.Parchment);
            l.Rect(cx - 3, baseY - h + 2, 6, 1, P.Brown);
            l.Rect(cx - 3, baseY - h - 1, 2, 1, P.Parchment); l.Rect(cx + 1, baseY - h - 1, 2, 1, P.Parchment);
            l.Set(cx - w / 2 + 2, baseY - h + 7, P.Cream); l.Set(cx - w / 2 + 2, baseY - h + 8, P.Cream);
        });
    }

    // ------------------------------------------------------------------ chapter 1
    static Canvas Ch1Photo()
    {
        var c = M();
        c.Rect(0, 0, 64, 29, P.Sand); c.Dither(0, 0, 64, 29, P.Parchment, 0);
        c.Rect(0, 29, 64, 11, P.MidBrown); c.Rect(0, 29, 64, 1, P.LightBrown);
        c.Rect(0, 34, 64, 1, P.Brown); c.Rect(0, 38, 64, 1, P.Brown);
        c.Rect(17, 30, 30, 2, P.Brown);
        Obj(c, l =>
        {
            l.Rect(17, 4, 30, 26, P.Brown);
            l.Rect(18, 5, 28, 24, P.Tan);
            l.Rect(18, 5, 28, 1, P.Sand); l.Rect(18, 5, 1, 24, P.Sand);
            l.Rect(20, 7, 24, 20, P.Cream);
            l.Rect(22, 9, 20, 16, P.Parchment);
            l.Poly(new[] { (22.0, 20.0), (30.0, 17.0), (38.0, 19.0), (42.0, 18.0), (42.0, 25.0), (22.0, 25.0) }, P.Sand);
            // family: two adults and a child, darker than the paper so they read as people
            l.Disc(26.5, 14.2, 2.2, P.LightBrown); l.Rect(24, 16, 5, 6, P.Brown); l.Rect(24, 22, 2, 3, P.Brown); l.Rect(27, 22, 2, 3, P.Brown);
            l.Disc(33.5, 14.6, 2.2, P.LightBrown); l.Rect(31, 12, 5, 1, P.Brown); l.Set(31, 13, P.Brown); l.Set(35, 13, P.Brown);
            l.Poly(new[] { (31.0, 16.5), (36.0, 16.5), (37.5, 25.0), (29.5, 25.0) }, P.MidBrown);
            l.Disc(38.8, 19.4, 1.6, P.LightBrown); l.Rect(38, 21, 3, 4, P.Brown);
            l.Line(22, 21, 26, 25, P.Cream); l.Line(22, 22, 25, 25, P.Cream);
            // creased (dog-eared) top-right corner
            l.Tri(37, 9, 42, 9, 42, 14, P.Cream);
            l.Tri(37, 9, 42, 14, 37, 14, P.White);
            l.Line(37, 9, 42, 14, P.DarkParch);
            l.Set(38, 10, P.White);
        });
        c.Rect(37, 9, 1, 1, P.Cream);
        // teacup on the left, small plant on the right
        Obj(c, l =>
        {
            l.Rect(4, 25, 8, 5, P.Cream); l.Rect(4, 27, 8, 1, P.Teal); l.Rect(4, 29, 8, 1, P.Sand);
            l.Frame(11, 26, 3, 3, P.Cream);
            l.Rect(3, 30, 11, 1, P.Sand);
            l.Poly(new[] { (52.0, 24.0), (60.0, 24.0), (59.0, 30.0), (53.0, 30.0) }, P.Terracotta);
            l.Rect(51, 23, 10, 2, P.DarkTerra);
            l.Tri(53, 23, 49.5, 15, 56, 22, P.Green); l.Tri(56, 23, 56, 10.5, 58.5, 23, P.DarkGreen);
            l.Tri(59, 23, 63, 15.5, 57, 22, P.Green); l.Tri(55, 23, 53, 12, 56.5, 23, P.DarkGreen);
            l.Set(56, 11, P.Gold); l.Set(57, 10, P.Gold);
        });
        return c;
    }

    static Canvas Ch1Shelf()
    {
        var c = M();
        c.Rect(0, 0, 64, 40, P.Parchment);
        for (int y = 0; y < 40; y += 8) c.Rect(0, y, 64, 1, P.DarkParch);
        for (int r = 0; r < 5; r++) for (int x = (r % 2) * 4; x < 64; x += 8) c.Rect(x, r * 8, 1, 8, P.DarkParch);
        // shelf board and brackets
        Obj(c, l =>
        {
            l.Rect(4, 27, 56, 3, P.LightBrown); l.Rect(4, 30, 56, 2, P.MidBrown);
            l.Poly(new[] { (9.0, 32.0), (14.0, 32.0), (9.0, 38.0) }, P.Brown);
            l.Poly(new[] { (50.0, 32.0), (55.0, 32.0), (55.0, 38.0) }, P.Brown);
        });
        // honey jar
        Obj(c, l =>
        {
            l.Rect(10, 12, 12, 15, P.LightBlue); l.Rect(10, 12, 12, 1, P.White);
            l.Rect(11, 17, 10, 10, P.Gold); l.Rect(11, 17, 10, 1, P.DarkGold);
            l.Rect(19, 17, 2, 10, P.Orange);
            l.Rect(9, 9, 14, 4, P.Terracotta); l.Rect(9, 9, 14, 1, P.Orange); l.Rect(9, 12, 14, 1, P.DarkTerra);
            l.Rect(12, 13, 1, 12, P.White); l.Set(13, 19, P.LightGold);
        });
        // cup
        Obj(c, l =>
        {
            l.Rect(27, 17, 10, 10, P.Cream); l.Set(27, 26, Rgba.Clear); l.Set(36, 26, Rgba.Clear);
            l.Rect(27, 20, 10, 2, P.Teal); l.Rect(35, 17, 2, 10, P.Sand); l.Rect(27, 17, 10, 1, P.Brown);
            l.Frame(37, 19, 4, 5, P.Cream); l.Set(37, 20, P.Cream); l.Set(37, 22, P.Cream);
            l.Rect(28, 23, 1, 3, P.White);
        });
        // bowl
        Obj(c, l =>
        {
            l.Paint((x, y) => y >= 18 && (x + .5 - 50) * (x + .5 - 50) / 90.0 + (y + .5 - 18) * (y + .5 - 18) / 81.0 <= 1, P.Terracotta, 38, 17, 62, 28);
            l.Rect(40, 18, 20, 1, P.DarkTerra);
            l.Rect(41, 17, 18, 1, P.Cream);
            l.Rect(44, 22, 12, 1, P.Cream); l.Rect(46, 24, 8, 1, P.Cream);
            l.Rect(51, 20, 7, 5, P.DarkTerra);
            l.Rect(42, 19, 2, 4, P.Orange);
        });
        // hanging cloth rail
        Obj(c, l => { l.Rect(36, 32, 7, 7, P.Teal); l.Rect(36, 32, 7, 1, P.DarkTeal); l.Rect(38, 34, 3, 5, P.Cream); l.Rect(36, 38, 7, 1, P.DarkTeal); });
        return c;
    }

    // ------------------------------------------------------------------ chapter 2
    static Canvas Ch2Rumor()
    {
        var c = M();
        c.Rect(0, 0, 64, 31, P.Sand); c.Dither(0, 0, 64, 31, P.DarkParch, 0);
        c.Rect(0, 31, 64, 9, P.Tan); c.Dither(0, 31, 64, 9, P.Sand, 0);
        c.Rect(0, 31, 64, 1, P.MidBrown);
        Sack(c, 36, 36, 12, 13);
        Sack(c, 48, 37, 14, 15);
        Sack(c, 59, 36, 10, 12);
        // two figures leaning towards each other
        void Fig(Canvas dst, double hx, Rgba skin, Rgba hair, Rgba cloth, Rgba shade, (double, double)[] body, int footX)
        {
            Obj(dst, l =>
            {
                l.Poly(body, cloth);
                l.Paint((x, y) => y > 21 && l.Get(x, y) == cloth && x + .5 > body[1].Item1 + (body[2].Item1 - body[1].Item1) * (y - body[1].Item2) / (body[2].Item2 - body[1].Item2) - 3, shade);
                l.Rect(footX, 34, 5, 2, P.Brown);
                l.Rect((int)hx - 1, 18, 3, 2, skin);
                l.Disc(hx, 15, 3.3, skin);
                l.Ellipse(hx, 13.3, 3.4, 1.9, hair);
                l.Set((int)hx + (hx < 24 ? 2 : -3), 15, P.Ink);
            });
        }
        Fig(c, 20.5, P.LightBrown, P.DeepBrown, P.MutedBlue, P.DarkSlate, new[] { (17.0, 20.0), (24.0, 20.0), (22.0, 34.0), (12.0, 34.0) }, 11);
        Fig(c, 28.5, P.Tan, P.Brown, P.DimOchre, P.Brown, new[] { (25.0, 20.0), (32.0, 20.0), (35.5, 34.0), (26.0, 34.0) }, 29);
        c.Rect(18, 31, 8, 1, P.MidBrown);
        // speech-mark shapes, empty
        Obj(c, l =>
        {
            l.Ellipse(12, 7.5, 8, 4.4, P.Cream);
            l.Tri(14, 10.5, 19, 10.5, 18.5, 14, P.Cream);
            l.Ellipse(41, 8, 6, 3.6, P.Parchment);
            l.Tri(34, 10, 38, 10.5, 32.5, 14, P.Parchment);
            l.Rect(7, 9, 8, 1, P.Sand); l.Rect(37, 9, 7, 1, P.DarkParch);
        });
        return c;
    }

    static Canvas Ch2Bread()
    {
        var c = M();
        c.Rect(0, 12, 64, 18, P.MidBrown); c.Dither(0, 12, 64, 18, P.Brown, 0);
        c.Rect(0, 0, 64, 12, P.Terracotta);
        for (int x = 0; x < 64; x += 8) c.Rect(x, 0, 4, 12, P.Cream);
        for (int x = 0; x < 64; x += 4)
        {
            var col = (x / 4) % 2 == 0 ? P.Cream : P.Terracotta;
            c.Ellipse(x + 2, 11.5, 2, 2.6, col);
        }
        c.Rect(0, 0, 64, 1, P.DarkTerra); c.Rect(0, 12, 64, 1, P.Ink);
        // table and cloth
        c.Rect(0, 29, 64, 3, P.Parchment); c.Rect(0, 29, 64, 1, P.Cream);
        c.Rect(0, 32, 64, 8, P.Teal); c.Rect(0, 32, 64, 1, P.DarkTeal);
        for (int x = 0; x < 64; x += 6) c.Rect(x, 36, 3, 1, P.Cream);
        // baguette basket (right)
        Obj(c, l =>
        {
            foreach (var (x0, y0, x1, y1) in new[] { (47.0, 14.0, 52.0, 29.0), (52.0, 13.0, 56.0, 29.0), (43.0, 17.0, 49.0, 29.0) })
            {
                l.Poly(new[] { (x0, y0), (x0 + 3.5, y0 + 0.5), (x1 + 2.0, y1), (x1 - 2.0, y1) }, P.Orange);
                l.Rect((int)x0 + 1, (int)y0 + 3, 1, 3, P.LightOrange);
                foreach (var dy in new[] { 5, 10 }) { l.Set((int)(x0 + (x1 - x0) * dy / 15.0) + 1, (int)y0 + dy, P.DarkOrange); l.Set((int)(x0 + (x1 - x0) * dy / 15.0) + 2, (int)y0 + dy, P.DarkOrange); }
            }
            l.Poly(new[] { (42.0, 24.0), (60.0, 24.0), (58.0, 29.0), (44.0, 29.0) }, P.Tan);
            l.Rect(42, 24, 18, 1, P.Sand);
        });
        // boules and rolls
        Obj(c, l =>
        {
            l.Ellipse(13, 25.2, 9.5, 5, P.Orange);
            l.Paint((x, y) => y >= 27 && l.Get(x, y) == P.Orange, P.DarkOrange);
            l.Ellipse(11, 22.6, 4.2, 1.5, P.LightOrange);
            foreach (var x in new[] { 8, 12, 16 }) { l.Set(x, 24, P.DarkOrange); l.Set(x + 1, 25, P.DarkOrange); l.Set(x + 2, 26, P.DarkOrange); }
            l.Ellipse(29, 26, 6.5, 3.7, P.Orange);
            l.Paint((x, y) => y >= 27 && l.Get(x, y) == P.Orange, P.DarkOrange);
            l.Set(26, 24, P.LightOrange); l.Set(27, 24, P.LightOrange);
            l.Set(29, 25, P.DarkOrange); l.Set(30, 26, P.DarkOrange);
            foreach (var cx in new[] { 37.0 })
            {
                l.Ellipse(cx, 27, 3.2, 2.2, P.Orange); l.Set((int)cx - 1, 26, P.LightOrange); l.Set((int)cx + 1, 28, P.DarkOrange);
            }
        });
        return c;
    }

    // ------------------------------------------------------------------ chapter 3 (muted)
    static Canvas Ch3Wait()
    {
        var c = M();
        c.Rect(0, 0, 64, 30, P.DeepSlate); c.Dither(0, 0, 64, 30, P.Night, 0);
        c.Rect(0, 30, 64, 10, P.Night); c.Rect(0, 30, 64, 1, P.DarkSlate);
        // small high window and the pale light it lets in
        c.Rect(47, 4, 12, 13, P.DarkSlate); c.Rect(48, 5, 10, 11, P.MutedBlue);
        c.Rect(52, 5, 1, 11, P.DarkSlate); c.Rect(48, 10, 10, 1, P.DarkSlate);
        c.Poly(new[] { (46.0, 31.0), (59.0, 31.0), (63.0, 40.0), (40.0, 40.0) }, P.Night);
        c.Dither(40, 31, 23, 9, P.DarkSlate, 0);
        // table with the dark space beneath
        c.Rect(13, 21, 38, 17, P.Night);
        Obj(c, l =>
        {
            l.Rect(8, 17, 48, 3, P.Grey); l.Rect(8, 17, 48, 1, P.LightGrey); l.Rect(8, 20, 48, 2, P.DarkGrey);
            l.Rect(10, 22, 3, 16, P.DarkGrey); l.Rect(51, 22, 3, 16, P.DarkGrey);
        });
        // huddled shapes under it: a blanket over two adults and a child
        Scenes.Layer(c, l =>
        {
            l.Ellipse(31, 33.5, 9, 5.6, P.DarkGrey);
            l.Rect(22, 33, 18, 4, P.DarkGrey);
            l.Line(26, 31, 36, 30, P.Grey);
            l.Disc(26, 29.3, 3, P.Ink); l.Disc(36, 28.6, 3, P.Ink);
            l.Ellipse(21, 35, 3.8, 3, P.Ink); l.Disc(21, 31.4, 2.3, P.Ink);
            l.Ellipse(43, 35.5, 3.8, 2.8, P.Ink); l.Disc(43, 32.3, 2.3, P.Ink);
        }, P.DarkSlate);
        return c;
    }

    static Canvas Ch3Counting()
    {
        var c = M();
        c.Rect(0, 0, 64, 40, P.Night);
        c.Rect(0, 30, 64, 10, P.Ink);
        c.Rect(0, 30, 64, 1, P.DarkSlate);
        Scenes.Glow(c, 32, 25, (21, P.DeepBrown, true), (16, P.DeepBrown, false), (16, P.Brown, true), (11, P.Brown, false), (11, P.DimOchre, true), (6.5, P.DimOchre, false));
        // two small sitting silhouettes
        void Sitter(int cx, int dir)
        {
            var f = new Canvas(64, 40);
            f.Disc(cx - dir * 0.5, 23.8, 3.4, P.Ink);
            f.Ellipse(cx, 31.2, 5.5, 5.6, P.Ink);
            f.Rect(cx - 6, 34, 12, 3, P.Ink);
            for (int y = 0; y < 40; y++) for (int x = 0; x < 64; x++)
                    if (f.Opaque(x, y) && !f.Opaque(x + dir, y)) f.Set(x, y, P.DimOchre);
            c.Blit(f, 0, 0);
        }
        Sitter(14, 1); Sitter(50, -1);
        // candle on a small dish
        c.Rect(29, 34, 7, 2, P.DarkGrey); c.Rect(29, 34, 7, 1, P.Grey);
        c.Rect(31, 27, 3, 7, P.PaleMuted); c.Rect(33, 27, 1, 7, P.LightGrey);
        c.Rect(32, 25, 1, 2, P.Ink);
        c.Rect(32, 22, 1, 3, P.Orange); c.Set(32, 23, P.Gold); c.Set(32, 24, P.LightGold);
        c.Set(31, 24, P.Orange); c.Set(33, 24, P.Orange);
        // a few pebbles being counted
        foreach (var x in new[] { 22, 25, 27, 38, 40, 43 }) c.Set(x, 36, P.DimOchre);
        foreach (var x in new[] { 23, 26, 39, 42 }) c.Set(x, 37, P.Brown);
        return c;
    }

    // ------------------------------------------------------------------ chapter 4 (muted)
    static Canvas Ch4Farewell()
    {
        var c = M();
        c.Rect(0, 0, 64, 40, P.DeepSlate); c.Dither(0, 0, 64, 40, P.Night, 0);
        // bright doorway
        c.Bands(18, 4, 32, new[] { 7, 7, 6 }, new[] { P.PaleMuted, P.Parchment, P.PaleMuted });
        c.Rect(18, 4, 32, 20, P.PaleMuted);
        c.Dither(18, 14, 32, 6, P.LightGrey, 0);
        c.Poly(new[] { (18.0, 24.0), (28.0, 21.0), (40.0, 23.0), (50.0, 20.0), (50.0, 36.0), (18.0, 36.0) }, P.LightGrey);
        c.Poly(new[] { (30.0, 36.0), (36.0, 36.0), (40.0, 24.0), (38.0, 24.0) }, P.PaleMuted);
        c.Poly(new[] { (22.0, 24.0), (28.0, 21.0), (33.0, 24.0) }, P.DimOchre);
        // frame
        c.Rect(14, 0, 4, 40, P.Brown); c.Rect(50, 0, 4, 40, P.Brown);
        c.Rect(14, 0, 40, 4, P.Brown);
        c.Rect(14, 0, 1, 40, P.MidBrown); c.Rect(50, 0, 1, 40, P.MidBrown); c.Rect(14, 0, 40, 1, P.MidBrown);
        c.Rect(14, 36, 40, 4, P.DarkGrey); c.Rect(14, 36, 40, 1, P.Grey);
        // light spilling in across the floor
        c.Poly(new[] { (18.0, 36.0), (50.0, 36.0), (62.0, 40.0), (6.0, 40.0) }, P.DarkSlate);
        c.Rect(14, 36, 40, 1, P.Grey);
        // hand resting on the frame (silhouette)
        Scenes.Layer(c, l =>
        {
            l.Rect(0, 22, 10, 9, P.Ink);
            l.Rect(9, 19, 7, 12, P.Ink);
            l.Rect(11, 16, 5, 3, P.Ink); l.Set(11, 16, Rgba.Clear);
            l.Rect(16, 19, 6, 2, P.Ink); l.Rect(16, 22, 7, 2, P.Ink); l.Rect(16, 25, 6, 2, P.Ink); l.Rect(16, 28, 5, 2, P.Ink);
            foreach (var (x, y) in new[] { (21, 19), (22, 22), (21, 25), (20, 28) }) { l.Set(x, y, Rgba.Clear); }
        }, P.DarkSlate);
        return c;
    }

    static Canvas Ch4Shelf()
    {
        var c = M();
        c.Rect(0, 0, 64, 40, P.DarkGrey); c.Dither(0, 0, 64, 40, P.Brown, 0);
        // pale rectangle where a frame used to hang
        c.Rect(21, 4, 22, 19, P.Grey);
        c.Dither(21, 4, 22, 19, P.LightGrey, 0);
        c.Frame(21, 4, 22, 19, P.Grey);
        c.Rect(22, 5, 20, 17, P.Grey); c.Dither(22, 5, 20, 17, P.LightGrey, 1);
        c.Rect(32, 5, 1, 1, P.Ink); c.Rect(31, 4, 3, 1, P.Brown);
        c.Line(32, 5, 27, 9, P.DarkGrey); c.Line(32, 5, 37, 9, P.DarkGrey);
        // the empty shelf
        Obj(c, l =>
        {
            l.Rect(6, 28, 52, 3, P.DimOchre); l.Rect(6, 31, 52, 3, P.Brown);
            l.Poly(new[] { (11.0, 34.0), (16.0, 34.0), (11.0, 39.0) }, P.Brown);
            l.Poly(new[] { (48.0, 34.0), (53.0, 34.0), (53.0, 39.0) }, P.Brown);
        });
        // ghost marks of a small object that stood there
        c.Rect(46, 20, 8, 8, P.Grey); c.Dither(46, 20, 8, 8, P.LightGrey, 0);
        c.Rect(46, 28, 8, 1, P.DarkGrey);
        c.Dither(6, 29, 52, 1, P.Grey, 0);
        return c;
    }

    // ------------------------------------------------------------------ chapter 5 (muted)
    static Canvas Ch5Share()
    {
        var c = M();
        c.Rect(0, 0, 64, 34, P.PaleMuted); c.Dither(0, 0, 64, 12, P.LightSlate, 0);
        c.Rect(0, 34, 64, 6, P.DimOchre); c.Dither(0, 34, 64, 6, P.Grey, 0);
        Scenes.Tent(c, 32, 36, 54, 30, P.MutedBlue, P.DarkSlate, P.Ink, P.Ink);
        // arms and hands
        Obj(c, l =>
        {
            // left arm and hand
            l.Rect(0, 23, 12, 7, P.DarkSlate);
            l.Rect(12, 24, 9, 5, P.LightBrown);
            l.Ellipse(24, 26, 5, 3.6, P.LightBrown);
            l.Poly(new[] { (23.0, 24.0), (28.0, 22.0), (28.0, 24.5), (25.0, 25.5) }, P.LightBrown);
            // right arm and hand
            l.Rect(52, 25, 12, 7, P.DimOchre);
            l.Rect(42, 26, 10, 5, P.Tan);
            l.Ellipse(40, 28, 5, 3.6, P.Tan);
            l.Poly(new[] { (37.0, 30.0), (41.0, 32.0), (38.0, 33.0), (35.0, 31.0) }, P.Tan);
        });
        // piece of bread between them
        Obj(c, l =>
        {
            l.Ellipse(32, 26.5, 6.5, 3.6, P.Sand);
            l.Paint((x, y) => y >= 28 && l.Get(x, y) == P.Sand, P.DimOchre);
            l.Rect(28, 24, 4, 1, P.Cream);
            l.Set(33, 26, P.Tan); l.Set(35, 27, P.Tan); l.Set(30, 27, P.Tan);
        });
        // thumbs over the bread
        Obj(c, l =>
        {
            l.Ellipse(27.5, 24.6, 2.8, 1.5, P.LightBrown);
            l.Ellipse(36.5, 27.7, 2.8, 1.6, P.Tan);
        });
        return c;
    }

    static Canvas Ch5Neighbours()
    {
        var c = M();
        c.Bands(0, 0, 64, new[] { 8, 8, 6 }, new[] { P.LightSlate, P.PaleMuted, P.LightGrey });
        c.Rect(0, 22, 64, 18, P.DimOchre); c.Dither(0, 22, 64, 18, P.LightGrey, 0);
        c.Rect(0, 22, 64, 1, P.Grey);
        foreach (var (cx, b, s) in new[] { (7, P.Grey, P.DarkGrey), (23, P.LightSlate, P.MutedBlue), (41, P.Grey, P.DarkGrey), (57, P.LightSlate, P.MutedBlue) })
            Scenes.Tent(c, cx, 28, 15, 9, b, s, P.Ink, P.Ink);
        Scenes.Tent(c, 10, 38, 22, 14, P.MutedBlue, P.DarkSlate, P.Ink, P.Ink);
        Scenes.Tent(c, 54, 38, 22, 14, P.PaleMuted, P.LightGrey, P.Ink, P.Ink);
        // fire in the middle with tiny figures around it
        foreach (var x in new[] { 29, 31, 33, 35 }) c.Rect(x, 36, 2, 2, P.Grey);
        c.Tri(29.5, 36, 32.5, 30, 35.5, 36, P.Orange);
        c.Tri(30.5, 36, 32.5, 32, 34.5, 36, P.Gold);
        c.DitherDisc(32.5, 27, 1.8, P.PaleMuted, 0);
        foreach (var (x, y, col) in new[] { (24, 38, P.Ink), (41, 38, P.Ink), (19, 39, P.DarkSlate) })
        {
            Scenes.Layer(c, l => { l.Disc(x + .5, y - 6.2, 1.8, col); l.Rect(x - 1, y - 5, 3, 4, col); l.Rect(x - 2, y - 2, 5, 2, col); });
        }
        return c;
    }

    // ------------------------------------------------------------------ chapter 6 (warm)
    static Canvas Ch6Lock()
    {
        var c = M();
        c.Rect(0, 0, 64, 40, P.Tan); c.Dither(0, 0, 64, 40, P.Sand, 0);
        Scenes.Glow(c, 7, 12, (20, P.Sand, true), (14, P.Sand, false), (14, P.Parchment, true), (8, P.Parchment, false), (8, P.Cream, true));
        c.Rect(0, 34, 64, 6, P.Brown); c.Rect(0, 34, 64, 1, P.LightBrown);
        // wall lamp
        Obj(c, l =>
        {
            l.Rect(3, 12, 2, 6, P.Brown);
            l.Poly(new[] { (1.0, 12.0), (11.0, 12.0), (9.0, 6.0), (3.0, 6.0) }, P.Gold);
            l.Rect(3, 6, 6, 1, P.LightGold); l.Rect(1, 12, 10, 1, P.DarkGold);
        });
        // door in its frame
        Obj(c, l =>
        {
            l.Rect(16, 0, 34, 40, P.Brown);
            l.Rect(19, 0, 28, 37, P.MidBrown);
            foreach (var x in new[] { 26, 33, 40 }) l.Rect(x, 0, 1, 37, P.Brown);
            foreach (var x in new[] { 19, 27, 34, 41 }) l.Rect(x, 0, 1, 37, P.LightBrown);
            l.Rect(19, 7, 28, 3, P.DarkGrey); l.Rect(19, 7, 28, 1, P.Grey);
            l.Rect(19, 27, 28, 3, P.DarkGrey); l.Rect(19, 27, 28, 1, P.Grey);
            foreach (var x in new[] { 21, 29, 37, 44 }) { l.Set(x, 8, P.LightGrey); l.Set(x, 28, P.LightGrey); }
            l.Rect(19, 37, 28, 3, P.DarkGrey);
        });
        // sturdy box lock with the key still in it
        Obj(c, l =>
        {
            l.Rect(33, 14, 12, 12, P.Grey); l.Rect(33, 14, 12, 1, P.LightGrey); l.Rect(33, 14, 1, 12, P.LightGrey);
            l.Rect(44, 15, 1, 11, P.DarkGrey); l.Rect(34, 25, 11, 1, P.DarkGrey);
            foreach (var (x, y) in new[] { (35, 16), (42, 16), (35, 23), (42, 23) }) l.Set(x, y, P.Cream);
            l.Rect(36, 11, 6, 3, P.LightGrey); l.Rect(36, 11, 1, 5, P.LightGrey); l.Rect(41, 11, 1, 5, P.LightGrey);
            l.Rect(37, 12, 4, 1, P.Ink);
        });
        c.Rect(37, 19, 4, 4, P.Ink);
        Obj(c, l =>
        {
            l.Ring(39, 20, 5.2, 2.4, P.Gold);
            l.Rect(38, 24, 3, 8, P.Gold); l.Rect(41, 28, 3, 2, P.Gold); l.Rect(41, 31, 2, 2, P.Gold);
            l.Paint((x, y) => x + y > 62 && l.Get(x, y) == P.Gold, P.DarkGold);
            l.Set(35, 18, P.LightGold); l.Set(36, 17, P.LightGold);
        });
        // doormat
        Obj(c, l => { l.Rect(20, 36, 24, 3, P.Teal); l.Rect(22, 37, 20, 1, P.Cream); });
        return c;
    }

    static Canvas Ch6Window()
    {
        var c = M();
        c.Rect(0, 0, 64, 40, P.Brown); c.Dither(0, 0, 64, 40, P.DeepBrown, 0);
        // window at night
        Obj(c, l =>
        {
            l.Rect(14, 2, 36, 26, P.MidBrown);
            l.Rect(16, 4, 32, 22, P.DarkBlue);
        });
        c.Bands(16, 4, 32, new[] { 8, 8, 6 }, new[] { P.Night, P.DarkBlue, P.DarkSlate });
        c.Dither(16, 4, 32, 1, P.Night, 0);
        foreach (var (x, y) in new[] { (20, 7), (27, 9), (34, 6), (44, 11), (23, 16), (40, 18) }) c.Set(x, y, P.Gold);
        foreach (var (x, y) in new[] { (30, 8), (21, 12), (37, 14) }) c.Set(x, y, P.Cream);
        c.Disc(41, 10, 3.4, P.Cream); c.Disc(42.5, 9, 3.0, P.DarkBlue);
        c.Disc(40, 11, 2.2, P.Cream); c.Disc(41.5, 10, 2.0, P.DarkBlue);
        c.Disc(40.5, 10.5, 3.2, P.Cream); c.Disc(42.2, 9.4, 2.9, P.DarkBlue);
        c.Poly(new[] { (16.0, 26.0), (16.0, 21.0), (24.0, 18.0), (31.0, 22.0), (38.0, 19.0), (48.0, 23.0), (48.0, 26.0) }, P.Night);
        foreach (var (x, y) in new[] { (21, 23), (35, 22) }) c.Rect(x, y, 2, 2, P.Gold);
        c.Rect(31, 4, 2, 22, P.MidBrown); c.Rect(16, 14, 32, 2, P.MidBrown);
        c.Rect(12, 28, 40, 3, P.Tan); c.Rect(12, 28, 40, 1, P.Sand);
        // desk surface
        c.Rect(0, 31, 64, 9, P.MidBrown); c.Rect(0, 31, 64, 1, P.LightBrown);
        c.Rect(0, 36, 64, 1, P.Brown);
        // lamp glow, then lamp
        Scenes.Glow(c, 55, 22, (16, P.MidBrown, true), (11, P.LightBrown, true), (7, P.Tan, true));
        Obj(c, l =>
        {
            l.Rect(51, 30, 9, 2, P.Brown); l.Rect(55, 22, 2, 8, P.Brown);
            l.Poly(new[] { (49.0, 22.0), (61.0, 22.0), (58.0, 15.0), (52.0, 15.0) }, P.Gold);
            l.Rect(52, 15, 6, 1, P.LightGold); l.Rect(49, 22, 12, 1, P.DarkGold);
        });
        // school books and an open notebook
        Obj(c, l =>
        {
            l.Rect(4, 25, 20, 6, P.Terracotta); l.Rect(4, 25, 20, 1, P.Orange); l.Rect(6, 28, 16, 1, P.Gold);
            l.Rect(5, 20, 17, 5, P.Teal); l.Rect(5, 20, 17, 1, P.LightTeal); l.Rect(7, 22, 13, 1, P.Cream);
            l.Rect(6, 16, 14, 4, P.Cream); l.Rect(6, 18, 14, 1, P.Sand); l.Rect(8, 16, 1, 4, P.Sand);
            l.Rect(27, 32, 17, 6, P.Cream); l.Rect(35, 32, 1, 6, P.Sand);
            foreach (var y in new[] { 34, 36 }) { l.Rect(29, y, 5, 1, P.Tan); l.Rect(37, y, 5, 1, P.Tan); }
            l.Rect(28, 38, 16, 1, P.Sand);
            l.Line(46, 36, 58, 33, P.Gold); l.Line(46, 37, 58, 34, P.DarkGold); l.Set(45, 36, P.Ink); l.Set(45, 37, P.Ink);
        });
        return c;
    }

    // ------------------------------------------------------------------ unknown / locked placeholder
    static Canvas Unknown()
    {
        var c = M();
        c.Rect(0, 0, 64, 40, P.Ink);
        for (int y = 0; y < 40; y++) for (int x = 0; x < 64; x++) if ((x + y) % 4 == 0) c.Set(x, y, P.Night);
        // small padlock, centred
        c.Rect(27, 13, 2, 8, P.MutedBlue); c.Rect(35, 13, 2, 8, P.MutedBlue);
        c.Rect(29, 11, 6, 2, P.MutedBlue);
        c.Rect(27, 12, 1, 1, P.Ink); c.Rect(36, 12, 1, 1, P.Ink);
        c.Rect(25, 20, 14, 11, P.DarkSlate);
        c.Rect(25, 20, 14, 1, P.MutedBlue); c.Rect(25, 20, 1, 11, P.MutedBlue);
        c.Rect(38, 21, 1, 10, P.Night); c.Rect(26, 30, 13, 1, P.Night);
        c.Rect(31, 23, 2, 3, P.Ink); c.Rect(31, 26, 2, 2, P.Ink);
        c.Set(25, 20, Rgba.Clear); c.Set(38, 20, Rgba.Clear);
        return c;
    }

    public static void Build()
    {
        Add("mem_ch1_photo", Ch1Photo());
        Add("mem_ch1_shelf", Ch1Shelf());
        Add("mem_ch2_rumor", Ch2Rumor());
        Add("mem_ch2_bread", Ch2Bread());
        Add("mem_ch3_wait", Ch3Wait());
        Add("mem_ch3_counting", Ch3Counting());
        Add("mem_ch4_farewell", Ch4Farewell());
        Add("mem_ch4_shelf", Ch4Shelf());
        Add("mem_ch5_share", Ch5Share());
        Add("mem_ch5_neighbours", Ch5Neighbours());
        Add("mem_ch6_lock", Ch6Lock());
        Add("mem_ch6_window", Ch6Window());
        Add("mem_unknown", Unknown(), P.DarkSlate);
    }
}
