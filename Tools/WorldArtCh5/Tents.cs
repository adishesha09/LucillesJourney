namespace WorldArtCh5;

/// <summary>Patchwork tents, tarps and the lean-to frame (bare and finished).</summary>
public static class Tents
{
    static int FDiv(int a, int b) => (int)Math.Floor(a / (double)b);

    static Rgba Tone(Rgba[] ramp, int i) => ramp[Math.Clamp(i, 0, ramp.Length - 1)];

    /// <summary>Patchwork colour of a cloth plane at plane coordinates (u,v).</summary>
    static Rgba Patch(int u, int v, int seed, Rgba[][] pal, int baseTone, int pw, int ph)
    {
        int row = FDiv(v, ph);
        int off = (int)(Fx.R(row, 0, seed) * pw);
        int col = FDiv(u + off, pw);
        uint h = Fx.H(col, row, seed + 3);
        double r = (h & 255) / 256.0;
        var ramp = r < 0.46 ? pal[0] : pal[1 + (int)((h >> 8) % (uint)(pal.Length - 1))];
        int idx = baseTone;
        if (Fx.R(u, v / 2, seed + 9) > 0.93) idx--;
        if (Fx.R(u / 2, v, seed + 11) > 0.95) idx++;
        bool vseam = FDiv(u + off, pw) != FDiv(u + off - 1, pw);
        bool hseam = FDiv(v, ph) != FDiv(v - 1, ph);
        if (vseam || hseam)
        {
            bool stitch = ((u + v) & 1) == 0;
            return Tone(ramp, idx - (stitch ? 2 : 1));
        }
        return Tone(ramp, idx);
    }

    static void Pole(Canvas c, int x0, int y0, int x1, int y1, bool thick, Rgba lit, Rgba shade)
    {
        c.Line(x0, y0, x1, y1, lit);
        if (thick) c.Line(x0 + 1, y0, x1 + 1, y1, shade);
    }

    /// <summary>A-frame tent seen with its gable towards the viewer. door: 0 none, 1 closed flap, 2 open.</summary>
    public static Canvas GableTent(int w, int h, int ds, int seed, Rgba[][] pal, int door, int doorShift = 0, bool open = false)
    {
        int mx = 5, W = w + 2 * mx, Hh = h + ds + 3, yb = Hh - 1;
        var c = new Canvas(W, Hh);
        double midx = mx + w / 2.0;
        int dw = (int)(w * 0.36) | 1, dh = (int)(h * 0.68);
        double dcx = midx + doorShift;
        for (int x = mx; x < mx + w; x++)
        {
            double dxn = Math.Abs(x + 0.5 - midx) / (w / 2.0);
            double yEdge = yb + 1 - h * (1 - dxn);
            int ye = (int)Math.Ceiling(yEdge);
            bool left = x + 0.5 < midx;
            for (int y = ye - ds; y <= yb; y++)
            {
                if (y < 0) continue;
                Rgba col;
                if (y >= ye)
                {
                    // gable face
                    col = Patch(x - mx, y - (yb - h), seed, pal, 2, 11 + seed % 4, 9 + seed % 3);
                    // slightly darker lower part, lit top
                    if (y - ye < 1) col = Tone(pal[0], 0);
                    else if (y - ye == 1) col = Tone(pal[0], 3);
                }
                else
                {
                    int v = y - (ye - ds);
                    col = Patch(x - mx, v + (left ? 0 : 5), seed + (left ? 1 : 2), pal, left ? 3 : 1, 12 + seed % 3, 7 + seed % 3);
                    if (y == ye - ds) col = Tone(pal[0], left ? 3 : 0);       // far edge catches the light
                    // fold creases
                    if ((x + seed * 3) % 13 == 0 && ((y + x) & 1) == 0) col = Tone(pal[0], left ? 1 : 0);
                }
                c.Set(x, y, col);
            }
        }
        // ridge seam and pole tip
        int rx = (int)Math.Round(midx - 0.5);
        for (int y = yb - h - ds; y < yb - h + 1; y++) if (c.Opaque(rx, y)) c.Set(rx, y, Tone(pal[0], 0));
        c.Set(rx, yb - h - ds - 1, C.Rope); c.Set(rx + 1, yb - h - ds - 1, C.RopeD);
        c.Set(rx, yb - h - 1, K.Wood[3]); c.Set(rx + 1, yb - h - 1, K.Wood[1]);
        // door
        if (door > 0)
        {
            for (int y = yb - dh; y <= yb; y++)
                for (int x = (int)(dcx - dw / 2.0 - 1); x <= (int)(dcx + dw / 2.0 + 1); x++)
                {
                    double t = (y - (yb - dh)) / (double)dh;      // 0 apex .. 1 base
                    double half = dw / 2.0 * t;
                    if (Math.Abs(x + 0.5 - dcx) > half) continue;
                    if (door == 2)
                    {
                        // open: dark interior, floor strip lighter
                        c.Set(x, y, y >= yb - 1 ? C.Inside2 : (t < 0.4 ? C.Inside : Rgba.Mul(C.Inside, 1.05, 1.0, 1.05)));
                    }
                    else
                    {
                        // closed flap: two panels meeting at a slit
                        bool slit = x == (int)dcx || x == (int)dcx - 1 && (y & 1) == 0;
                        var ramp = pal[0];
                        c.Set(x, y, slit ? Tone(ramp, 0) : Tone(ramp, x < dcx ? 3 : 2));
                    }
                }
            // door trim
            for (int y = yb - dh; y <= yb; y++)
            {
                double t = (y - (yb - dh)) / (double)dh;
                int lx = (int)Math.Round(dcx - dw / 2.0 * t) - 1, rxx = (int)Math.Round(dcx + dw / 2.0 * t);
                if (c.Opaque(lx, y)) c.Set(lx, y, Tone(pal[0], 0));
                if (c.Opaque(rxx, y)) c.Set(rxx, y, Tone(pal[0], 0));
            }
            if (door == 1)
            {
                // ties
                for (int k = 1; k <= 2; k++) { int ty = yb - dh + k * dh / 3; c.Set((int)dcx - 1, ty, C.Rope); c.Set((int)dcx, ty, C.RopeD); }
            }
            else
            {
                // pulled-back flap bunched at one side
                int fx = (int)(dcx - dw / 2.0) - 2;
                for (int y = yb - dh / 2; y <= yb; y++) { c.Set(fx, y, Tone(pal[0], 3)); c.Set(fx + 1, y, Tone(pal[0], 2)); }
            }
        }
        // pegs and guy ropes
        c.Line(mx + 2, yb - 4, 1, yb, C.Rope); c.Line(mx + w - 3, yb - 4, W - 2, yb, C.Rope);
        c.Set(1, yb, K.Wood[1]); c.Set(1, yb - 1, K.Wood[3]); c.Set(W - 2, yb, K.Wood[1]); c.Set(W - 2, yb - 1, K.Wood[3]);
        // weighting stones along the lower hem
        for (int i = 0; i < w / 14; i++)
        {
            int sx = mx + 3 + (int)(Fx.R(i, 1, seed + 40) * (w - 8));
            if (Math.Abs(sx - dcx) < dw / 2.0 + 2) continue;
            c.Set(sx, yb, C.Rock[3]); c.Set(sx + 1, yb, C.Rock[2]);
        }
        c.SelOutline(0.36);
        return c;
    }

    /// <summary>Lean tarp strung from poles over a few belongings (side-on, wide).</summary>
    public static Canvas Tarp(int w, int h, int seed, Rgba[][] pal, bool bundles)
    {
        int mx = 4, W = w + 2 * mx, Hh = h + 10, yb = Hh - 1;
        var c = new Canvas(W, Hh);
        int top = 1, front = yb - 8;
        // pale ground-shadow beneath the sheet
        for (int x = mx + 2; x < mx + w - 2; x++)
            for (int y = front; y <= yb; y++) c.Set(x, y, (y == yb) ? C.Inside2 : C.Inside);
        // under-sheet belongings
        if (bundles)
        {
            c.Disc(mx + 12, yb - 3, 4, C.Cream[1]); c.Disc(mx + 12, yb - 4, 3, C.Cream[2]); c.Set(mx + 12, yb - 7, C.RopeD);
            c.Rect(mx + 20, yb - 6, 9, 6, C.Rust[1]); c.Rect(mx + 20, yb - 6, 9, 2, C.Rust[2]); c.HL(mx + 20, yb - 3, 9, C.Rust[0]);
            c.Disc(mx + w - 14, yb - 3, 4, C.Clay[1]); c.Disc(mx + w - 14, yb - 4, 3, C.Clay[2]);
        }
        // sheet: sags slightly at the front edge
        for (int x = mx; x < mx + w; x++)
        {
            double t = (x - mx) / (double)(w - 1);
            int sag = (int)Math.Round(Math.Sin(t * Math.PI * 3) * 1.0 + Math.Sin(t * Math.PI) * 2);
            int yTop = top + (int)(Math.Abs(t - 0.5) * 2 * 2), yBot = front + sag - 1;
            for (int y = yTop; y <= yBot; y++)
            {
                int v = y - yTop;
                var col = Patch(x - mx, v, seed, pal, 2 + (y < yTop + 3 ? 1 : 0) - (y > yBot - 3 ? 1 : 0), 13 + seed % 4, 8 + seed % 3);
                if (y == yTop) col = Tone(pal[0], 3);
                if (y == yBot) col = Tone(pal[0], 0);
                if (x == mx + w / 2 + seed % 5 && ((y & 1) == 0)) col = Tone(pal[0], 1);
                c.Set(x, y, col);
            }
        }
        // poles at both ends, rope from the top corners to pegs
        for (int y = top - 1; y <= yb; y++) { c.Set(mx - 1, y, K.Wood[3]); c.Set(mx, y, K.Wood[1]); c.Set(mx + w, y, K.Wood[3]); c.Set(mx + w + 1, y, K.Wood[1]); }
        c.Line(mx + w + 1, top + 1, W - 1, yb, C.Rope);
        c.Line(mx - 2, top + 1, 0, yb, C.Rope);
        c.SelOutline(0.36);
        return c;
    }

    // ---- lean-to frame, bare and finished (identical canvases) -----------------------------------------------------
    public const int ShW = 56, ShH = 36, ShDs = 16;
    public static int ShelterCanvasW => ShW + 10;
    public static int ShelterCanvasH => ShH + ShDs + 3;

    public static Canvas ShelterBare()
    {
        int mx = 5, w = ShW, h = ShH, ds = ShDs, W = w + 2 * mx, Hh = h + ds + 3, yb = Hh - 1;
        var c = new Canvas(W, Hh);
        double midx = mx + w / 2.0;
        int cx = (int)Math.Round(midx - 0.5);
        var lit = K.Wood[4]; var mid = K.Wood[3]; var dk = K.Wood[1]; var dk2 = K.Wood[0];
        int lx = mx + 1, rx = mx + w - 2;
        // back A-frame (further away, darker)
        Pole(c, lx, yb - ds, cx, yb - h - ds, false, dk, dk);
        Pole(c, rx, yb - ds, cx + 1, yb - h - ds, false, dk2, dk2);
        // ridge pole running back
        for (int y = yb - h - ds - 1; y <= yb - h; y++) { c.Set(cx, y, mid); c.Set(cx + 1, y, dk); }
        // side poles leaning from ridge-back to ground at the sides (lean-to bracing)
        c.Line(cx - 1, yb - h - ds + 2, lx - 2, yb - 2, dk);
        c.Line(cx + 2, yb - h - ds + 2, rx + 3, yb - 2, dk2);
        // cross-bar of the back frame
        c.HL(lx + 6, yb - ds - h / 3, w - 14, dk);
        // front A-frame: thick, lit from the left
        Pole(c, lx, yb, cx, yb - h, true, lit, dk);
        Pole(c, lx - 1, yb, cx - 1, yb - h, false, mid, dk);
        Pole(c, rx, yb, cx + 1, yb - h, true, mid, dk);
        Pole(c, rx + 1, yb, cx + 2, yb - h, false, dk, dk);
        // front cross-bar with lashings
        int cb = yb - (int)(h * 0.38);
        int cl = lx + (int)((yb - cb) / (double)h * (cx - lx));
        int cr = rx - (int)((yb - cb) / (double)h * (rx - cx));
        c.HL(cl, cb, cr - cl + 2, lit); c.HL(cl, cb + 1, cr - cl + 2, dk);
        foreach (var jx in new[] { cl, cr })
        {
            c.Rect(jx - 1, cb - 1, 3, 3, C.Rope); c.Set(jx, cb + 1, C.RopeD); c.Set(jx - 1, cb, C.RopeD);
        }
        // lashing at the apex, with loose rope ends hanging
        c.Rect(cx - 1, yb - h - 1, 4, 3, C.Rope); c.Set(cx, yb - h + 1, C.RopeD); c.Set(cx + 2, yb - h, C.RopeD);
        c.Line(cx, yb - h + 2, cx - 3, yb - h + 8, C.Rope);
        c.Line(cx + 2, yb - h + 2, cx + 5, yb - h + 6, C.Rope);
        // pegs and tie-down ropes
        c.Line(lx + 1, yb - 4, 1, yb, C.Rope); c.Line(rx, yb - 4, W - 2, yb, C.Rope);
        foreach (var px in new[] { 1, W - 2 }) { c.Set(px, yb, dk); c.Set(px, yb - 1, mid); }
        // folded tarp and spare poles waiting on the ground (swapped out for the finished tent)
        for (int y = yb - 4; y <= yb; y++)
            for (int x = cx - 8; x <= cx + 9; x++)
            {
                if (y == yb - 4 && (x < cx - 5 || x > cx + 6)) continue;
                if (y >= yb - 1 && (x < cx - 8 || x > cx + 9)) continue;
                var ramp = C.Blue;
                int idx = y == yb - 4 ? 3 : (y == yb ? 0 : 2 - ((x + y) % 7 == 0 ? 1 : 0));
                c.Set(x, y, ramp[idx]);
            }
        c.HL(cx - 8, yb - 2, 18, C.Blue[1]);
        c.Line(cx - 11, yb - 1, cx + 12, yb - 3, K.Wood[2]);
        c.Line(cx - 11, yb, cx + 12, yb - 2, K.Wood[1]);
        c.SelOutline(0.38);
        return c;
    }

    public static Canvas ShelterDone()
    {
        var pal = new[] { C.Ochre, C.Blue, C.Drab, C.Clay, C.Cream };
        var c = GableTent(ShW, ShH, ShDs, 4, pal, 2, 0);
        if (c.W != ShelterCanvasW || c.H != ShelterCanvasH) throw new Exception($"shelter size {c.W}x{c.H}");
        int yb = c.H - 1, cx = c.W / 2;
        // bedroll and blanket inside the doorway
        c.Rect(cx - 6, yb - 6, 12, 5, C.Cream[2]); c.HL(cx - 6, yb - 6, 12, C.Cream[3]); c.HL(cx - 6, yb - 2, 12, C.Cream[0]);
        c.Rect(cx - 7, yb - 5, 2, 3, C.Cream[1]); c.Set(cx - 3, yb - 4, C.RopeD); c.Set(cx + 1, yb - 4, C.RopeD);
        c.Rect(cx - 8, yb - 1, 16, 2, C.Inside2);
        for (int x = cx - 6; x <= cx + 5; x++) { c.Set(x, yb - 1, x % 4 < 2 ? C.Clay[2] : C.Clay[1]); c.Set(x, yb, C.Clay[0]); }
        // lantern on a stake beside the door
        int sx = cx + 12;
        for (int y = yb - 12; y <= yb; y++) { c.Set(sx, y, K.Wood[3]); c.Set(sx + 1, y, K.Wood[1]); }
        c.Rect(sx - 2, yb - 19, 6, 7, C.Out);
        c.Rect(sx - 1, yb - 18, 4, 5, K.Gold); c.Set(sx, yb - 17, Rgba.Hex("FFF1B8")); c.Set(sx + 1, yb - 16, K.Orange);
        c.HL(sx - 2, yb - 20, 6, K.Brown2); c.Set(sx, yb - 21, K.Brown2); c.Set(sx + 1, yb - 21, K.Brown2);
        return c;
    }
}
