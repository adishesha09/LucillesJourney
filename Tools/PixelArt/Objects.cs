namespace PixelArt;

/// <summary>Groups C, D, G: keepsake illustrations, achievement medals, final-choice illustrations.</summary>
public static class Objects
{
    // ------------------------------------------------------------------ helpers
    static Canvas Outlined(int w, int h, Action<Canvas> draw)
    {
        var c = new Canvas(w, h); draw(c); c.Outline(P.Ink); return c;
    }

    /// <summary>Object on a hard-edged ground shadow.</summary>
    static Canvas OnGround(int size, Action<Canvas> draw, double shy, double shrx, double shry, double shx = -1)
    {
        var obj = Outlined(size, size, draw);
        var c = new Canvas(size, size);
        c.Shadow(shx < 0 ? size / 2.0 : shx, shy, shrx, shry, P.Brown);
        c.Blit(obj, 0, 0);
        return c;
    }

    /// <summary>Woven basket body (trapezoid) with a rim; items/cloth are drawn by the caller.</summary>
    static void Basket(Canvas c, double cx, int top, int bottom, double halfTop, double halfBot)
    {
        for (int y = top; y < bottom; y++)
        {
            double t = (y + .5 - top) / (bottom - top), half = halfTop + (halfBot - halfTop) * t;
            for (int x = 0; x < c.W; x++)
            {
                double dx = x + .5 - cx;
                if (Math.Abs(dx) > half) continue;
                int row = (y - top) / 2;
                bool strandTop = (y - top) % 2 == 0;
                Rgba col = strandTop ? P.Sand : P.Tan;
                if (((x + 2 * row) & 3) == 3) col = P.MidBrown;
                if (dx > half - 1.6 && col != P.MidBrown) col = strandTop ? P.Tan : P.MidBrown;
                c.Set(x, y, col);
            }
        }
        for (int x = 0; x < c.W; x++)
        {
            double dx = x + .5 - cx;
            if (Math.Abs(dx) <= halfTop + 1.2) { c.Set(x, top - 1, P.Sand); c.Set(x, top, P.Tan); }
            if (Math.Abs(dx) <= halfBot) c.Set(x, bottom - 1, P.MidBrown);
        }
    }

    // ------------------------------------------------------------------ C: keepsakes
    static Canvas WateringCan() => OnGround(32, c =>
    {
        // top carry handle (arch) and rear D-handle, drawn before the body
        c.EllipseRing(14.5, 12.5, 6.5, 9, 4.6, 7, P.Teal);
        c.Paint((x, y) => y > 12, Rgba.Clear, 0, 0, 32, 32);
        c.EllipseRing(8.5, 19.5, 6.2, 7.5, 3.8, 5.2, P.Teal);
        c.Paint((x, y) => y > 19 && c.Get(x, y) == P.Teal, P.DarkTeal);
        // spout + rose
        c.Poly(new[] { (19.0, 25.0), (19.0, 20.0), (25.0, 13.5), (28.0, 15.2) }, P.Teal);
        c.Poly(new[] { (24.8, 11.4), (29.8, 15.3), (28.2, 17.3), (23.2, 13.4) }, P.LightTeal);
        c.Set(26, 13, P.DarkTeal); c.Set(27, 14, P.DarkTeal); c.Set(28, 15, P.DarkTeal);
        // body
        c.Rect(8, 12, 13, 14, P.Teal);
        c.Set(8, 25, Rgba.Clear); c.Set(20, 25, Rgba.Clear);
        c.Rect(17, 13, 4, 12, P.DarkTeal);
        c.Rect(9, 14, 1, 10, P.LightTeal);
        c.Rect(8, 18, 13, 1, P.DarkTeal);
        c.Rect(8, 24, 12, 1, P.DarkTeal);
        c.Rect(7, 11, 15, 2, P.LightTeal);
        c.Rect(7, 12, 15, 1, P.Teal);
        c.Set(7, 11, Rgba.Clear); c.Set(21, 11, Rgba.Clear);
        c.Set(10, 15, P.White); c.Set(10, 16, P.White);
    }, 28.3, 11, 2.3, 17.5);

    static Canvas BasketKs() => OnGround(32, c =>
    {
        c.EllipseRing(16, 15, 10.5, 12, 8.3, 9.8, P.MidBrown);
        c.Paint((x, y) => y > 15, Rgba.Clear, 0, 0, 32, 32);
        c.Set(6, 15, P.MidBrown); c.Set(25, 15, P.MidBrown);
        Basket(c, 16, 16, 27, 10, 8);
        // gingham cloth draped over the top, hem hangs over the rim
        c.Poly(new[] { (6.0, 17.5), (8.0, 10.5), (24.0, 10.5), (26.0, 17.5) }, P.Terracotta);
        c.Paint((x, y) => y >= 11 && y <= 17 && ((x / 2 + y / 2) & 1) == 0 && c.Get(x, y) == P.Terracotta, P.Cream, 0, 11, 32, 18);
        c.Paint((x, y) => y == 17 && c.Get(x, y) == P.Terracotta, P.DarkTerra);
        c.Paint((x, y) => y == 17 && c.Get(x, y) == P.Cream, P.Sand);
        c.Rect(8, 10, 16, 1, P.DarkTerra);
        // hem scallops
        foreach (var x in new[] { 8, 12, 16, 20 }) c.Rect(x + 1, 18, 2, 1, P.DarkTerra);
    }, 28.4, 11, 2.2);

    static Canvas Blanket() => OnGround(32, c =>
    {
        // three folded layers, each with its own ink outline so the folds read as lines
        void Layer(int x0, int x1, int y0, int y1, bool fringe)
        {
            var l = new Canvas(32, 32);
            l.RectN(x0, y0, x1 - x0 + 1, y1 - y0 + 1, P.Orange);
            l.Rect(x0 + 1, y0 + 1, x1 - x0 - 1, 1, P.LightOrange);
            l.Rect(x0 + 1, y1 - 1, x1 - x0 - 1, 1, P.DarkOrange);
            foreach (int sx in new[] { x0 + 3, x1 - 6 })
            {
                l.Rect(sx, y0 + 1, 1, y1 - y0 - 1, P.Cream);
                l.Rect(sx + 1, y0 + 1, 2, y1 - y0 - 1, P.Terracotta);
            }
            l.Outline(P.Ink);
            c.Blit(l, 0, 0);
            if (fringe) for (int y = y0 + 1; y < y1; y += 2) { c.Set(x1 + 2, y, P.Sand); c.Set(x1 + 3, y, P.Sand); }
        }
        Layer(3, 28, 20, 26, true);
        Layer(4, 27, 14, 20, true);
        Layer(6, 25, 8, 14, false);
    }, 28.3, 12.5, 2.2);

    static Canvas Bag() => OnGround(32, c =>
    {
        c.Ellipse(16, 20.2, 10, 7.6, P.Sand);
        c.Poly(new[] { (12.2, 9.5), (19.8, 9.5), (21.5, 15.5), (10.5, 15.5) }, P.Sand);
        c.Poly(new[] { (11.0, 11.0), (12.0, 7.2), (13.8, 10.0), (16.0, 6.8), (18.2, 10.0), (20.0, 7.2), (21.0, 11.0) }, P.Sand);
        c.Paint((x, y) => y >= 14 && x + y > 36 && c.Get(x, y) == P.Sand, P.Tan);
        foreach (var (x, y) in new[] { (8, 18), (8, 19), (9, 17), (8, 20), (10, 16) }) c.Set(x, y, P.Cream);
        foreach (var x in new[] { 13, 16, 19 }) c.Rect(x, 10, 1, 3, P.Tan);
        // patch with stitches
        c.Rect(13, 19, 7, 5, P.Teal);
        c.Rect(13, 23, 7, 1, P.DarkTeal);
        foreach (var (x, y) in new[] { (13, 19), (19, 19), (13, 23), (19, 23) }) c.Set(x, y, P.Cream);
        // drawstring tied at the neck
        c.Rect(11, 14, 10, 2, P.Terracotta);
        c.Rect(15, 14, 2, 3, P.DarkTerra);
        foreach (var (x, y) in new[] { (14, 16), (13, 17), (13, 18), (18, 16), (19, 17), (20, 18) }) c.Set(x, y, P.Terracotta);
    }, 28.3, 11, 2.2);

    static Canvas Jug() => OnGround(32, c =>
    {
        c.EllipseRing(21.5, 14, 5.5, 5.5, 3.2, 3.2, P.DarkOrange);
        c.Paint((x, y) => x < 20, Rgba.Clear, 0, 0, 32, 32);
        c.Ellipse(16, 20, 9.2, 8, P.Orange);
        c.Poly(new[] { (12.5, 8.0), (19.5, 8.0), (21.5, 14.0), (10.5, 14.0) }, P.Orange);
        c.Rect(11, 6, 10, 3, P.DarkOrange);
        c.Rect(12, 5, 8, 1, P.DarkOrange);
        c.Rect(12, 6, 8, 1, P.LightOrange);
        c.Paint((x, y) => x + y > 33 && c.Get(x, y) == P.Orange, P.DarkOrange, 0, 9, 32, 32);
        c.Paint((x, y) => x < 12 && y > 15 && y < 24 && x + y < 31 && c.Get(x, y) == P.Orange, P.LightOrange);
        c.Paint((x, y) => y >= 12 && y <= 13 && x >= 10 && x <= 21 && c.Get(x, y) == P.Orange, P.Terracotta);
        // zig-zag band in cream, blue dots below
        for (int x = 8; x < 25; x++) { int y = 18 + ((x / 2) % 2); if (c.Get(x, y) == P.Orange || c.Get(x, y) == P.DarkOrange) c.Set(x, y, P.Cream); }
        for (int x = 9; x < 24; x += 3) if (c.Get(x, 22).A != 0) c.Set(x, 22, P.Blue);
    }, 28.4, 10, 2.2);

    static Canvas Key() => OnGround(32, c =>
    {
        var ring = new Canvas(32, 32);
        ring.EllipseRing(16, 9.5, 8.8, 8.3, 5.9, 5.4, P.LightGrey);
        ring.Paint((x, y) => x + y > 26 && ring.Get(x, y) == P.LightGrey, P.Grey);
        c.Blit(ring, 0, 0);
        // key: bow, shaft, teeth
        c.Ring(16, 14.5, 5.8, 2.7, P.Gold);
        c.Paint((x, y) => x + y > 31 && c.Get(x, y) == P.Gold, P.DarkGold, 8, 8, 24, 21);
        c.Rect(15, 20, 3, 8, P.Gold);
        c.Rect(17, 20, 1, 8, P.DarkGold);
        c.Rect(18, 23, 3, 2, P.Gold); c.Rect(18, 26, 2, 2, P.Gold);
        c.Rect(15, 22, 1, 5, P.LightGold);
        c.Set(12, 11, P.LightGold); c.Set(13, 10, P.LightGold); c.Set(11, 12, P.LightGold);
        // the ring passes through the bow hole, in front of the key
        c.Paint((x, y) => ring.Get(x, y).A != 0 && (x + .5 - 16) * (x + .5 - 16) + (y + .5 - 14.5) * (y + .5 - 14.5) <= 2.7 * 2.7, P.LightGrey);
    }, 28.4, 8, 2.0);

    // ------------------------------------------------------------------ D: achievements
    static Canvas Medal(Canvas motif)
    {
        var c = new Canvas(32, 32);
        c.Disc(16, 16, 16, P.Ink);
        c.Disc(16, 16, 15, P.Gold);
        c.Paint((x, y) => x + y > 38 && c.Get(x, y) == P.Gold, P.DarkGold);
        c.Paint((x, y) => x + y < 26 && c.Get(x, y) == P.Gold, P.LightGold);
        c.Disc(16, 16, 12.8, P.DarkGold);
        c.Disc(16, 16, 12.0, P.Tan);
        c.Disc(16, 16, 11.0, P.Parchment);
        c.Paint((x, y) => x + y < 21 && c.Get(x, y) == P.Parchment, P.Cream);
        foreach (var (x, y) in new[] { (15, 1), (16, 1), (15, 30), (16, 30), (1, 15), (1, 16), (30, 15), (30, 16) }) c.Set(x, y, P.DarkGold);
        c.Blit(motif, 8, 8);
        return c;
    }

    static Canvas Circular(Action<Canvas> scene)
    {
        var c = new Canvas(16, 16); scene(c);
        c.Paint((x, y) => (x + .5 - 8) * (x + .5 - 8) + (y + .5 - 8) * (y + .5 - 8) > 7.4 * 7.4, Rgba.Clear);
        c.Outline(P.Ink); return c;
    }

    static Canvas MotifFirstMemory()
    {
        var c = new Canvas(16, 16);
        c.Rect(3, 1, 10, 14, P.Cream);
        c.Rect(4, 2, 8, 12, P.DarkTeal);
        c.Rect(4, 2, 8, 1, P.Teal);
        c.Star(8, 8, 4.6, 1.2, 4, P.Gold);
        c.Paint((x, y) => x >= 8 && y >= 8 && c.Get(x, y) == P.Gold, P.DarkGold);
        c.Set(8, 7, P.LightGold);
        c.Set(5, 3, P.Gold); c.Set(11, 12, P.Gold);
        c.Outline(P.Ink); return c;
    }

    static Canvas MotifBook()
    {
        var c = new Canvas(16, 16);
        c.Rect(0, 8, 16, 7, P.Terracotta);
        c.Rect(0, 14, 16, 1, P.DarkTerra);
        c.Rect(1, 6, 6, 7, P.Cream); c.Rect(9, 6, 6, 7, P.Cream);
        c.Rect(7, 7, 2, 7, P.Sand);
        c.Rect(1, 12, 6, 1, P.Sand); c.Rect(9, 12, 6, 1, P.Sand);
        foreach (var y in new[] { 8, 10 }) { c.Rect(2, y, 4, 1, P.Tan); c.Rect(10, y, 4, 1, P.Tan); }
        c.Set(1, 6, Rgba.Clear); c.Set(14, 6, Rgba.Clear);
        c.Star(8, 3.2, 3.2, 0.9, 4, P.Gold);
        c.Set(8, 3, P.LightGold);
        c.Outline(P.Ink); return c;
    }

    static Canvas MotifAlbumStar()
    {
        var c = new Canvas(16, 16);
        c.Rect(1, 4, 13, 11, P.Gold);
        c.Rect(14, 5, 1, 10, P.DarkGold); c.Rect(2, 14, 12, 1, P.DarkGold);
        c.Rect(3, 6, 9, 7, P.Brown);
        c.Rect(4, 7, 7, 5, P.Blue);
        c.Tri(3.8, 12, 6.5, 7.5, 9.3, 12, P.DarkGreen);
        c.Rect(4, 11, 7, 1, P.Green);
        c.Set(9, 8, P.LightGold);
        c.Outline(P.Ink);
        var star = new Canvas(16, 16);
        star.Star(11, 5.3, 5.4, 2.3, 5, P.Gold);
        star.Paint((x, y) => x >= 11 && star.Get(x, y) == P.Gold, P.DarkGold);
        star.Set(9, 4, P.LightGold); star.Set(10, 3, P.LightGold);
        star.Outline(P.Ink);
        c.Blit(star, 0, 0);
        return c;
    }

    static Canvas MotifBasket()
    {
        var c = new Canvas(16, 16);
        c.EllipseRing(8, 8, 5.2, 7, 3.8, 5.6, P.MidBrown);
        c.Paint((x, y) => y > 8, Rgba.Clear);
        // items peeking out
        c.Ellipse(5.2, 6.6, 3.2, 2.2, P.Orange);
        c.Set(4, 5, P.Gold); c.Set(6, 7, P.DarkOrange); c.Set(5, 6, P.DarkOrange);
        c.Disc(10.6, 6.2, 2.5, P.Terracotta);
        c.Set(10, 5, P.Orange); c.Rect(10, 3, 1, 2, P.DarkGreen); c.Rect(11, 3, 2, 1, P.Green);
        c.Rect(7, 3, 2, 3, P.Teal);
        // body
        c.Poly(new[] { (1.5, 9.0), (14.5, 9.0), (12.5, 15.0), (3.5, 15.0) }, P.Tan);
        for (int y = 10; y < 15; y++) for (int x = 0; x < 16; x++)
                if (c.Get(x, y) == P.Tan && ((x + (y / 2) * 2) & 3) == 3) c.Set(x, y, P.MidBrown);
        c.Rect(1, 9, 14, 1, P.Sand);
        c.Outline(P.Ink); return c;
    }

    static Canvas MotifRituals()
    {
        var c = new Canvas(16, 16);
        // sun with rays
        foreach (var (x, y) in new[] { (8, 0), (4, 1), (12, 1), (2, 4), (3, 4), (13, 4), (14, 4) }) c.Set(x, y, P.Gold);
        c.Disc(8, 4.4, 2.9, P.Gold);
        c.Paint((x, y) => x + y > 12 && c.Get(x, y) == P.Gold, P.DarkGold);
        c.Set(7, 3, P.LightGold); c.Set(6, 4, P.LightGold);
        // cup on a saucer
        c.Rect(4, 8, 7, 5, P.Cream);
        c.Rect(4, 8, 7, 1, P.Brown);
        c.Rect(4, 11, 7, 1, P.Teal);
        c.Rect(10, 9, 1, 3, P.Sand); c.Rect(10, 12, 1, 1, P.Sand);
        c.Frame(11, 9, 3, 4, P.Cream);
        c.Set(11, 10, P.Cream); c.Set(11, 11, P.Cream);
        c.Rect(2, 13, 11, 2, P.Sand); c.Rect(2, 14, 11, 1, P.Tan);
        c.Outline(P.Ink); return c;
    }

    static Canvas MotifLeave()
    {
        var c = new Canvas(16, 16);
        c.Rect(9, 1, 6, 13, P.MidBrown); c.Set(9, 1, Rgba.Clear);
        c.Rect(10, 2, 4, 12, P.Orange);
        c.Rect(10, 2, 1, 12, P.LightOrange);
        c.Rect(13, 8, 1, 2, P.Gold);
        // light spilling on the sill
        c.Rect(0, 14, 16, 1, P.Tan);
        // packed bag by the door
        c.Ellipse(5, 10.2, 4.3, 3.8, P.Sand);
        c.Poly(new[] { (3.0, 6.5), (7.0, 6.5), (7.5, 8.5), (2.5, 8.5) }, P.Sand);
        c.Paint((x, y) => x + y > 14 && c.Get(x, y) == P.Sand, P.Tan, 0, 7, 9, 14);
        c.Rect(2, 7, 6, 1, P.Terracotta);
        c.Set(4, 5, P.Sand); c.Set(5, 5, P.Sand); c.Set(3, 5, P.Sand);
        c.Rect(4, 8, 2, 1, P.DarkTerra);
        c.Outline(P.Ink); return c;
    }

    static Canvas MotifJourneyEnd() => Circular(c =>
    {
        c.Rect(0, 0, 16, 16, P.LightBlue);
        c.Rect(0, 0, 16, 4, P.Blue);
        c.Dither(0, 4, 16, 1, P.Blue);
        c.Rect(0, 6, 16, 4, P.Parchment);
        c.Dither(0, 6, 16, 1, P.LightBlue);
        c.Disc(8, 9.5, 3.6, P.Gold);
        c.Disc(8, 9.5, 2.2, P.LightGold);
        foreach (var (x, y) in new[] { (8, 3), (3, 5), (13, 5), (5, 3), (11, 3) }) c.Set(x, y, P.Gold);
        c.Rect(0, 9, 16, 7, P.Green);
        c.Poly(new[] { (0.0, 10.5), (4.0, 9.0), (6.0, 10.0), (0.0, 13.0) }, P.DarkGreen);
        c.Poly(new[] { (16.0, 10.5), (12.0, 9.0), (10.0, 10.0), (16.0, 13.0) }, P.DarkGreen);
        c.Poly(new[] { (7.2, 9.5), (8.8, 9.5), (13.0, 16.0), (3.0, 16.0) }, P.Sand);
        c.Poly(new[] { (8.0, 9.5), (8.8, 9.5), (13.0, 16.0), (10.0, 16.0) }, P.Tan);
        c.Rect(8, 11, 1, 1, P.Cream); c.Rect(8, 13, 1, 1, P.Cream);
    });

    static Canvas MotifCompass()
    {
        var c = new Canvas(16, 16);
        c.Disc(8, 8, 7.4, P.Gold);
        c.Paint((x, y) => x + y > 17 && c.Get(x, y) == P.Gold, P.DarkGold);
        c.Disc(8, 8, 5.8, P.Cream);
        // small diagonal points
        c.Poly(new[] { (8.0, 8.0), (4.2, 4.2), (6.2, 8.0) }, P.Sand);
        c.Poly(new[] { (8.0, 8.0), (11.8, 4.2), (9.8, 8.0) }, P.Sand);
        c.Poly(new[] { (8.0, 8.0), (4.2, 11.8), (6.2, 8.0) }, P.Tan);
        c.Poly(new[] { (8.0, 8.0), (11.8, 11.8), (9.8, 8.0) }, P.Tan);
        // cardinal points
        c.Poly(new[] { (8.0, 1.8), (9.8, 8.0), (6.2, 8.0) }, P.Terracotta);
        c.Poly(new[] { (8.0, 14.2), (9.8, 8.0), (6.2, 8.0) }, P.MidBrown);
        c.Poly(new[] { (1.8, 8.0), (8.0, 6.2), (8.0, 9.8) }, P.Brown);
        c.Poly(new[] { (14.2, 8.0), (8.0, 6.2), (8.0, 9.8) }, P.MidBrown);
        c.Rect(7, 7, 2, 2, P.Gold);
        c.Outline(P.Ink); return c;
    }

    // ------------------------------------------------------------------ G: final choices (48x48)
    static Canvas SchoolFees() => OnGround(48, c =>
    {
        // schoolhouse (upper left)
        c.Rect(6, 17, 20, 16, P.Cream);
        c.Rect(6, 32, 20, 1, P.Sand); c.Rect(25, 17, 1, 16, P.Sand);
        c.Poly(new[] { (3.5, 18.0), (16.0, 7.0), (28.5, 18.0) }, P.Terracotta);
        c.Poly(new[] { (16.0, 7.0), (28.5, 18.0), (16.0, 18.0) }, P.DarkTerra);
        c.Rect(4, 17, 24, 1, P.DarkTerra);
        c.Disc(16, 13.2, 2.1, P.Cream); c.Disc(16, 13.2, 1.0, P.Brown);
        c.Rect(13, 24, 6, 9, P.MidBrown); c.Set(13, 24, Rgba.Clear); c.Set(18, 24, Rgba.Clear);
        c.Rect(14, 25, 4, 8, P.Orange); c.Rect(16, 29, 1, 2, P.Gold);
        foreach (var x in new[] { 8, 20 }) { c.Rect(x, 21, 4, 5, P.Blue); c.Rect(x, 23, 4, 1, P.Cream); c.Rect(x + 1, 21, 1, 5, P.Cream); c.Rect(x, 21, 4, 1, P.LightBlue); }
        c.Rect(12, 33, 8, 2, P.Sand);
        c.Rect(16, 1, 1, 6, P.Brown); c.Rect(17, 1, 5, 3, P.Gold); c.Rect(17, 3, 5, 1, P.DarkGold);
        // closed book lying in front (lower left)
        c.Rect(3, 37, 22, 7, P.Green);
        c.Rect(3, 37, 22, 1, P.LightGreen); c.Rect(3, 37, 3, 7, P.DarkGreen);
        c.Rect(4, 42, 20, 2, P.Cream); c.Rect(4, 43, 20, 1, P.Sand);
        c.Rect(9, 39, 10, 2, P.Gold); c.Rect(9, 40, 10, 1, P.DarkGold);
        // satchel (right)
        c.EllipseRing(36.5, 30, 6, 8, 4.2, 6, P.Brown);
        c.Paint((x, y) => y > 30, Rgba.Clear, 28, 20, 48, 48);
        c.Rect(28, 29, 17, 15, P.MidBrown);
        c.Rect(28, 29, 17, 1, P.LightBrown);
        c.Poly(new[] { (28.0, 30.0), (45.0, 30.0), (45.0, 37.0), (36.5, 40.5), (28.0, 37.0) }, P.Brown);
        c.Rect(28, 30, 17, 1, P.MidBrown);
        c.Rect(34, 36, 5, 4, P.Gold); c.Rect(35, 37, 3, 2, P.DarkGold);
        c.Rect(28, 43, 17, 1, P.Brown);
        c.Set(28, 43, Rgba.Clear); c.Set(44, 43, Rgba.Clear);
    }, 45.4, 21, 1.8, 24);

    static Canvas Utilities() => OnGround(48, c =>
    {
        // bulb
        c.Disc(15, 18, 9.5, P.Gold);
        c.Poly(new[] { (9.5, 24.0), (20.5, 24.0), (19.0, 30.5), (11.0, 30.5) }, P.Gold);
        c.Paint((x, y) => x + y > 33 && x < 25 && c.Get(x, y) == P.Gold, P.DarkGold, 0, 0, 26, 32);
        c.Disc(15, 18, 7.6, P.LightGold);
        c.Disc(12.5, 15.5, 2.4, P.Cream);
        // filament
        foreach (var (x, y) in new[] { (12, 25), (12, 23), (13, 21), (14, 20), (15, 21), (16, 20), (17, 21), (18, 23), (18, 25) }) c.Set(x, y, P.Orange);
        c.Set(12, 24, P.Orange); c.Set(18, 24, P.Orange);
        // screw base
        c.Rect(11, 31, 8, 7, P.LightGrey);
        c.Rect(11, 33, 8, 1, P.Grey); c.Rect(11, 36, 8, 1, P.Grey);
        c.Rect(18, 31, 1, 7, P.Grey);
        c.Rect(13, 38, 4, 2, P.DarkGrey);
        // water drop
        c.Poly(new[] { (35.0, 11.0), (43.0, 29.0), (27.0, 29.0) }, P.Blue);
        c.Disc(35, 31, 8.8, P.Blue);
        c.Paint((x, y) => x + y > 66 && x > 34 && c.Get(x, y) == P.Blue, P.DarkBlue, 27, 11, 48, 44);
        c.Paint((x, y) => x < 33 && y > 24 && y < 36 && c.Get(x, y) == P.Blue && x + y < 60, P.LightBlue);
        c.Rect(30, 28, 2, 5, P.White);
        c.Set(34, 17, P.LightBlue); c.Set(33, 19, P.LightBlue);
    }, 44.4, 22, 1.8, 24);

    static Canvas Groceries() => OnGround(48, c =>
    {
        // bottle, baguette, vegetables behind the basket
        c.Rect(31, 12, 5, 6, P.Teal); c.Rect(32, 5, 3, 8, P.Teal);
        c.Rect(31, 18, 5, 14, P.Teal); c.Rect(35, 12, 1, 20, P.DarkTeal);
        c.Rect(32, 2, 3, 3, P.Tan); c.Set(32, 2, P.Sand);
        c.Rect(32, 20, 3, 6, P.Cream); c.Rect(32, 22, 3, 1, P.Terracotta);
        c.Rect(32, 14, 1, 4, P.LightTeal);
        // baguette (diagonal)
        c.Poly(new[] { (8.0, 24.0), (10.5, 20.0), (27.0, 9.0), (28.5, 12.5), (13.0, 25.0) }, P.Orange);
        c.Poly(new[] { (10.5, 20.0), (27.0, 9.0), (28.0, 10.8), (12.0, 22.0) }, P.LightOrange);
        foreach (var (x, y) in new[] { (14, 19), (18, 16), (22, 13) }) { c.Set(x, y, P.DarkOrange); c.Set(x + 1, y - 1, P.DarkOrange); c.Set(x + 1, y, P.DarkOrange); }
        // carrot + tomato
        c.Poly(new[] { (14.0, 22.0), (19.0, 20.0), (12.0, 31.0) }, P.Orange);
        c.Rect(18, 17, 2, 3, P.Green); c.Rect(20, 18, 2, 2, P.Green); c.Rect(16, 18, 2, 2, P.DarkGreen);
        c.Disc(23.5, 23, 4.2, P.Terracotta); c.Set(22, 21, P.Cream); c.Set(21, 22, P.Cream);
        c.Rect(23, 18, 2, 2, P.DarkGreen); c.Rect(21, 19, 2, 1, P.Green); c.Rect(25, 19, 2, 1, P.Green);
        c.Paint((x, y) => x + y > 48 && c.Get(x, y) == P.Terracotta, P.DarkTerra);
        c.Disc(14.5, 27.5, 4.4, P.Green);
        c.Paint((x, y) => x + y > 42 && c.Get(x, y) == P.Green, P.DarkGreen, 9, 22, 20, 32);
        c.Set(12, 25, P.LightGreen); c.Set(13, 24, P.LightGreen); c.Set(12, 26, P.LightGreen);
        c.Rect(14, 24, 1, 4, P.DarkGreen);
        // basket
        Basket(c, 24, 28, 44, 17, 13);
        c.Rect(7, 26, 35, 1, P.Sand);
    }, 45.6, 18, 1.8, 24);

    // ------------------------------------------------------------------ build
    public static void Build()
    {
        void K(string n, Canvas c) => Registry.Add("C_keepsakes", n, c, 32, 32);
        K("ks_watering_can", WateringCan());
        K("ks_basket", BasketKs());
        K("ks_blanket", Blanket());
        K("ks_bag", Bag());
        K("ks_jug", Jug());
        K("ks_key", Key());

        void A(string n, Canvas motif) => Registry.Add("D_achievements", n, Medal(motif), 32, 32);
        A("ach_first_memory", MotifFirstMemory());
        A("ach_curious_observer", Icons.Magnifier());
        A("ach_keeper_of_memories", MotifBook());
        A("ach_life_remembered", MotifAlbumStar());
        A("ach_first_keepsake", Icons.Locket());
        A("ach_treasured", MotifBasket());
        A("ach_everyday_rituals", MotifRituals());
        A("ach_ready_to_leave", MotifLeave());
        A("ach_journey_complete", MotifJourneyEnd());
        A("ach_full_picture", MotifCompass());

        void G(string n, Canvas c) => Registry.Add("G_choices", n, c, 48, 48);
        G("choice_school_fees", SchoolFees());
        G("choice_utilities", Utilities());
        G("choice_groceries", Groceries());
    }
}
