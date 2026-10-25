namespace WorldArtCh5;

/// <summary>Bakes ch5_ground.png: dry earth, worn paths, stream, rock face, scrub border, shadows and warm low-sun grade.</summary>
public static class Ground
{
    public static Canvas Cv = new(G.W, G.H);
    public static readonly List<(double x, double y, double r, double k)> Glows = new();
    public static readonly List<(int x, int y, int rx, int ry)> Scorch = new();
    static readonly bool[,] blockedMass = new bool[G.W, G.H];

    static Rgba Pick(Rgba[] ramp, int i) => ramp[Math.Clamp(i, 0, ramp.Length - 1)];
    static double Smooth(double a, double b, double v) { double t = Math.Clamp((v - a) / (b - a), 0, 1); return t * t * (3 - 2 * t); }

    public static void Build()
    {
        for (int y = 0; y < G.H; y++) for (int x = 0; x < G.W; x++) blockedMass[x, y] = Terrain.Rise[x, y] || Terrain.ScrubM[x, y];
        RenderLand();
        RenderPaths();
        RenderStream();
        RenderFord();
        Decorate();
        RenderRise();
        RenderScrub();
        ApplyLight();
        Grade();
        AddColliders();
    }

    // ---- dry earth & grass ----------------------------------------------------------------------------------------
    static double Grassiness(int x, int y)
    {
        double dx = (x - 300) / 330.0, dy = (y - 350) / 270.0;
        double r = Math.Sqrt(dx * dx + dy * dy);
        double g = Smooth(0.62, 1.35, r);
        if (x > 745) g = Math.Max(g, 0.62);
        return g;
    }

    static void RenderLand()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                double wd = Terrain.WD[x, y];
                double g = Grassiness(x, y);
                double fresh = Smooth(34, 4, wd);                                 // moisture near the stream
                double n1 = Fx.Fbm(x / 16.0, y / 15.0, 41), n2 = Fx.Fbm(x / 8.0, y / 6.0, 47), n3 = Fx.VN(x / 3.5, y / 3.0, 53);
                double bay = Fx.Bay(x, y);
                double mix = g + (n2 - 0.5) * 0.9 + fresh * 0.45;
                double camp = 1 - Smooth(0.3, 0.95, Math.Sqrt(Math.Pow((x - 300) / 300.0, 2) + Math.Pow((y - 350) / 250.0, 2)));
                Rgba col;
                if (mix > 0.66)
                {
                    bool moist = fresh > 0.25 + (n3 - 0.5) * 0.3;
                    var ramp = moist ? C.Fresh : C.Dry;
                    double v = n1 * 0.75 + n3 * 0.25 + bay * 0.08;
                    int idx = v < 0.34 ? 0 : v < 0.44 ? 1 : v < 0.56 ? 2 : v < 0.66 ? 3 : v < 0.76 ? 4 : 5;
                    col = ramp[idx];
                }
                else if (mix > 0.57 && ((x + y) & 1) == 0)
                {
                    col = C.Dry[1 + (n3 > 0.5 ? 1 : 0)];
                }
                else
                {
                    var ramp = camp > 0.35 ? C.Packed : C.Earth;
                    double v = n1 * 0.78 + n3 * 0.22 + bay * 0.07;
                    int idx = v < 0.33 ? 0 : v < 0.44 ? 1 : v < 0.56 ? 2 : v < 0.67 ? 3 : v < 0.78 ? 4 : 5;
                    col = ramp[idx];
                    // cracked dry-earth hairlines
                    if (camp < 0.8 && Fx.VN(x / 5.0, y / 5.0, 59) > 0.8 && Fx.R(x, y, 7) > 0.35) col = ramp[Math.Max(0, idx - 2)];
                }
                Cv.Set(x, y, col);
            }
    }

    static void RenderPaths()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (Terrain.WD[x, y] < 1) continue;
                double nz = (Fx.Fbm(x / 10.0, y / 10.0, 141) - 0.5) * 5;
                double d = Terrain.PathDist(x + .5, y + .5, out int pi) + nz;
                bool track = pi == 4 || pi == 0;
                if (d > 0)
                {
                    // dusty shoulder dither on the rim
                    if (d < 3 && ((x + y) & 1) == 0 && Fx.R(x, y, 143) > 0.35) Cv.Set(x, y, Cv.Get(x, y).Light(0.7));
                    continue;
                }
                double v = Fx.Fbm(x / 9.0, y / 9.0, 145) * 0.8 + Fx.VN(x / 3.0, y / 2.5, 147) * 0.2 + Fx.Bay(x, y) * 0.07;
                int idx = v < 0.38 ? 0 : v < 0.5 ? 1 : v < 0.62 ? 2 : v < 0.74 ? 3 : 4;
                if (d > -2) idx = Math.Max(0, idx - 1);
                var col = C.Worn[idx];
                if (pi == 3 || pi == 2) col = Rgba.Mul(col, 0.97, 0.95, 0.92);            // fainter tracks
                Cv.Set(x, y, col);
            }
        // cart ruts and footprints on the main routes
        for (int y = 2; y < G.H - 2; y++)
            for (int x = 2; x < G.W - 2; x++)
            {
                if (Terrain.WD[x, y] < 1) continue;
                double d = Terrain.PathDist(x + .5, y + .5, out int pi);
                if (pi != 0 && pi != 4) continue;
                if (d < -4 && d > -9 && Fx.VN(x / 14.0, y / 14.0, 149) > 0.42 && Fx.R(x, y, 151) > 0.62) Cv.Set(x, y, C.Worn[1]);
            }
    }

    // ---- stream -------------------------------------------------------------------------------------------------------
    static void RenderStream()
    {
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                double wd = Terrain.WD[x, y];
                if (wd >= 5) continue;
                if (wd >= 0)
                {
                    // wet mud bank, dithered towards dry earth
                    if (wd > 3.2 && ((x + y) & 1) == 0) continue;
                    if (wd > 4.2) continue;
                    double v = Fx.VN(x / 4.0, y / 3.0, 153) + Fx.Bay(x, y) * 0.1;
                    Cv.Set(x, y, C.Mud[v < 0.35 ? 0 : v < 0.62 ? 1 : v < 0.85 ? 2 : 3]);
                    if (wd > 2.4 && Fx.R(x, y, 154) > 0.7) Cv.Set(x, y, C.Pebble[1]);
                    continue;
                }
                double depth = Math.Clamp(-wd / 9.0, 0, 1);
                double flow = Fx.VN(x / 10.0, y / 2.4, 155);
                double ripple = Fx.VN(x / 5.0, y / 1.6, 157);
                int idx = depth < 0.12 ? 3 : depth < 0.35 ? 2 : depth < 0.7 ? 2 : 1;
                if (depth > 0.55 && Fx.VN(x / 20.0, y / 14.0, 158) > 0.55) idx = 1;
                if (flow > 0.7 && ((x + y) & 1) == 0) idx++;
                else if (ripple > 0.82) idx++;
                if (ripple < 0.12 && depth > 0.3) idx--;
                var col = C.Water[Math.Clamp(idx, 0, 5)];
                // pebbles showing through the shallows
                if (depth < 0.32 && Fx.R(x / 2, y / 2, 159) > 0.78 && ((x + y) & 1) == 0) col = C.Mix(C.Pebble[1], col, 0.35);
                if (wd > -1.4) col = C.Mix(C.Water[5], col, 0.35);   // bright shore line
                Cv.Set(x, y, col);
            }
        // flat rocks breaking the surface and a spring crevice at the head of the stream
        var rocks = new (int x, int y, int rx, int ry)[] { (738, 262, 5, 3), (700, 390, 6, 3), (716, 340, 4, 3), (730, 150, 4, 2), (687, 540, 5, 3), (676, 500, 4, 3) };
        foreach (var (rx0, ry0, rx, ry) in rocks)
        {
            if (!Terrain.Water[rx0, ry0]) continue;
            RenderStone(rx0, ry0, rx, ry);
        }
        int sy = Terrain.EdgeY[748];
        for (int y = sy - 7; y <= sy + 3; y++)
            for (int x = 738; x <= 758; x++)
            {
                double dx = (x - 748) / 8.0, dy = (y - (sy - 1)) / 6.0;
                if (dx * dx + dy * dy < 1 && y < sy + 1) Cv.Set(x, y, ((x + y) & 1) == 0 ? Rgba.Hex("2F2928") : Rgba.Hex("3E3433"));
            }
    }

    static void RenderStone(int sx, int sy, int rx, int ry)
    {
        for (int y = sy - ry - 2; y <= sy + ry + 3; y++)
            for (int x = sx - rx - 2; x <= sx + rx + 3; x++)
            {
                double dx = (x + .5 - sx) / rx, dy = (y + .5 - sy) / ry;
                double dsx = (x + .5 - sx - 1.5) / rx, dsy = (y + .5 - sy - 1.6) / ry;
                if (dsx * dsx + dsy * dsy <= 1 && dx * dx + dy * dy > 1) Cv.Set(x, y, Cv.Get(x, y).Shade(0.9));
            }
        for (int y = sy - ry - 1; y <= sy + ry + 1; y++)
            for (int x = sx - rx - 1; x <= sx + rx + 1; x++)
            {
                double dx = (x + .5 - sx) / rx, dy = (y + .5 - sy + 0.3) / ry;
                if (dx * dx + dy * dy > 1) continue;
                double t = 0.55 - 0.3 * dx - 0.35 * dy + (Fx.R(x, y, 5) - 0.5) * 0.2;
                int idx = Math.Clamp((int)(t * 6) + 1, 1, 5);
                if (dy > 0.55) idx = Math.Max(1, idx - 1);
                Cv.Set(x, y, Rgba.Mul(C.Rock[idx], 1.03, 0.98, 0.92));
            }
    }

    static void RenderFord()
    {
        var f = Terrain.Ford;
        double midY = f.Y + f.H / 2.0;
        // shallow gravel bar across the stream
        for (int y = f.Y; y < f.Y + f.H; y++)
            for (int x = f.X; x < f.X + f.W; x++)
            {
                if (!Terrain.Water[x, y]) continue;
                double ed = Math.Abs(y - midY);
                if (ed > 11.5) continue;
                double v = Fx.VN(x / 2.5, y / 2.0, 161);
                var col = v < 0.3 ? C.Pebble[0] : v < 0.6 ? C.Pebble[1] : v < 0.85 ? C.Pebble[2] : C.Pebble[3];
                // thin water film over the gravel
                if (((x + y * 2) & 3) == 0) col = C.Mix(col, C.Water[3], 0.45);
                if (ed > 9.5 && ((x + y) & 1) == 0) col = C.Water[3];
                Cv.Set(x, y, col);
            }
        // stepping stones in a loose row
        int y0 = (int)midY;
        var xs = new List<int>();
        for (int x = f.X + 6; x < f.X + f.W - 4; x += 10) xs.Add(x);
        int k = 0;
        foreach (int x in xs)
        {
            if (!Terrain.Water[x, y0] && !Terrain.Water[x + 3, y0] && !Terrain.Water[x - 3, y0]) continue;
            RenderStone(x + (int)(Fx.R(k, 1, 163) * 3), y0 + (int)((Fx.R(k, 2, 163) - 0.5) * 5), 6, 4);
            k++;
        }
    }

    // ---- decoration -------------------------------------------------------------------------------------------------
    static bool Free(int x, int y) => x > 2 && y > 2 && x < G.W - 3 && y < G.H - 3 && Terrain.WD[x, y] > 5 && !blockedMass[x, y];

    static void Decorate()
    {
        // grass tufts on grassy ground
        for (int gy = 0; gy < G.H; gy += 6)
            for (int gx = 0; gx < G.W; gx += 6)
            {
                int x = gx + (int)(Fx.R(gx, gy, 1) * 6), y = gy + (int)(Fx.R(gx, gy, 2) * 6);
                if (!Free(x, y)) continue;
                double g = Grassiness(x, y) + Smooth(30, 8, Terrain.WD[x, y]) * 0.5;
                if (Fx.R(gx, gy, 3) > 0.12 + g * 0.38) continue;
                if (Terrain.PathDist(x, y, out _) < 2) continue;
                bool moist = Terrain.WD[x, y] < 28;
                var ramp = moist ? C.Fresh : C.Dry;
                int bi = 2 + (int)(Fx.R(gx, gy, 4) * 2);
                int h = 2 + (int)(Fx.R(gx, gy, 5) * 3);
                for (int b = -1; b <= 1; b++)
                {
                    int lean = b + (Fx.R(gx + b, gy, 6) > 0.5 ? 1 : 0);
                    for (int j = 0; j < h - Math.Abs(b); j++) Cv.Set(x + b + (lean * j) / 3, y - j, ramp[Math.Clamp(bi + (j > h / 2 ? 1 : 0) - (b == 0 ? 0 : 1), 0, 5)]);
                }
                Cv.Set(x, y + 1, ramp[0]);
            }
        // pebbles and small flat stones everywhere dry
        for (int gy = 0; gy < G.H; gy += 8)
            for (int gx = 0; gx < G.W; gx += 8)
            {
                int x = gx + (int)(Fx.R(gx, gy, 71) * 7), y = gy + (int)(Fx.R(gx, gy, 72) * 7);
                if (!Free(x, y) || Fx.R(gx, gy, 73) > 0.40) continue;
                int t = (int)(Fx.R(gx, gy, 74) * 3);
                var hi = new[] { Rgba.Hex("E6DBC4"), Rgba.Hex("D8CCB2"), Rgba.Hex("CDB99A") }[t];
                Cv.Set(x, y, hi); Cv.Set(x + 1, y, hi);
                if (t != 2) Cv.Set(x + 1, y + 1, C.Earth[0]);
                Cv.Set(x, y + 1, C.Earth[1]); Cv.Set(x + 2, y + 1, C.Earth[1]);
                if (Fx.R(gx, gy, 75) > 0.8) { Cv.Set(x + 3, y + 1, hi); Cv.Set(x + 4, y + 2, C.Earth[1]); }
            }
        // scorch marks and trampled dust
        foreach (var (sx, sy, rx, ry) in Scorch)
            for (int y = sy - ry - 2; y <= sy + ry + 2; y++)
                for (int x = sx - rx - 2; x <= sx + rx + 2; x++)
                {
                    double dx = (x + .5 - sx) / (rx + 1.5), dy = (y + .5 - sy) / (ry + 1.0);
                    double d = dx * dx + dy * dy;
                    if (d > 1.0) continue;
                    if (d > 0.55 && ((x + y) & 1) != 0 && Fx.R(x, y, 91) > 0.4) continue;
                    var c0 = Cv.Get(x, y);
                    Cv.Set(x, y, Rgba.Mul(c0, 0.62, 0.55, 0.52));
                }
        foreach (var (cx, cy, rx, ry) in Terrain.Trample)
            for (int y = cy - ry; y <= cy + ry; y++)
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    if (!Free(x, y)) continue;
                    double dx = (x - cx) / (double)rx, dy = (y - cy) / (double)ry;
                    double d = dx * dx + dy * dy;
                    if (d > 1) continue;
                    double n = Fx.Fbm(x / 7.0, y / 7.0, 171);
                    if (d + (n - 0.5) * 0.6 > 0.8) continue;
                    double v = Fx.VN(x / 3.0, y / 2.5, 173) + Fx.Bay(x, y) * 0.1;
                    Cv.Set(x, y, C.Worn[v < 0.4 ? 1 : v < 0.72 ? 2 : 3]);
                }
    }

    // ---- rocky rise ----------------------------------------------------------------------------------------------
    static void RenderRise()
    {
        for (int x = 0; x < G.W; x++)
        {
            int ey = Terrain.EdgeY[x];
            int faceTop = ey - 30 - (int)(8 * Fx.VN(x / 17.0, 7, 181));
            for (int y = Math.Max(0, faceTop); y < ey; y++)
            {
                double stratum = (y + 5 * Fx.VN(x / 40.0, y / 7.0, 183)) / 5.5;
                int sid = (int)Math.Floor(stratum);
                double within = stratum - sid;
                double h = Fx.R(sid, 0, 185);
                int idx = 2 + (int)(h * 2);                              // layer tone
                if (within < 0.18) idx += 1;                              // lit upper lip of each layer
                else if (within > 0.8) idx -= 2;                          // shaded underside
                double vcrack = Fx.VN(x / 3.0, y / 11.0, 187);
                if (vcrack > 0.83) idx -= 2;
                double depth = (y - faceTop) / (double)Math.Max(1, ey - faceTop);
                idx += (int)((1 - depth) * 1.2) - (depth > 0.85 ? 1 : 0);
                idx += (Fx.Bay(x, y) > 0 ? 0 : -0) + (Fx.R(x, y, 189) > 0.9 ? 1 : 0);
                var col = Pick(C.Rock, idx);
                Cv.Set(x, y, Rgba.Mul(col, 1.02, 0.97, 0.92));
            }
            // scree and fallen stones at the foot, dithered into the earth
            for (int j = 0; j < 7; j++)
            {
                int y = ey + j;
                if (y >= G.H || Terrain.WD[x, y] < 1) continue;
                double chance = 0.7 - j * 0.1;
                if (Fx.R(x, y, 191) < chance * 0.55 && ((x + y) & 1) == 0) Cv.Set(x, y, Pick(C.Rock, 1 + (int)(Fx.R(x, y, 193) * 3)));
            }
            Cv.Set(x, ey - 1, C.Rock[0]);
        }
    }

    // ---- dry scrub mass (plateau on the rise and the border all around) --------------------------------------------------
    static void RenderScrub()
    {
        const int cellW = 14, cellH = 12;
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                bool plateau = false;
                if (Terrain.Rise[x, y])
                {
                    int ey = Terrain.EdgeY[x];
                    int faceTop = ey - 30 - (int)(8 * Fx.VN(x / 17.0, 7, 181));
                    plateau = y < faceTop;
                    if (!plateau) continue;
                }
                else if (!Terrain.ScrubM[x, y]) continue;
                double bestD = 1e9, second = 1e9, bcx = 0, bcy = 0;
                int gx0 = x / cellW, gy0 = y / cellH;
                double jx = (Fx.VN(x / 5.0, y / 5.0, 311) - 0.5) * 5.5, jy = (Fx.VN(x / 5.0, y / 5.0, 312) - 0.5) * 5.5;
                for (int gy = gy0 - 2; gy <= gy0 + 2; gy++)
                    for (int gx = gx0 - 2; gx <= gx0 + 2; gx++)
                    {
                        double cx = gx * cellW + ((gy & 1) * cellW / 2.0) + (Fx.R(gx, gy, 301) - 0.5) * 9;
                        double cy = gy * cellH + (Fx.R(gx, gy, 302) - 0.5) * 7;
                        double dx = x + .5 + jx - cx, dy = (y + .5 + jy - cy) * 1.15;
                        double d = Math.Sqrt(dx * dx + dy * dy) * (0.85 + 0.3 * Fx.R(gx, gy, 303));
                        if (d < bestD) { second = bestD; bestD = d; bcx = cx; bcy = cy; }
                        else if (d < second) second = d;
                    }
                double R = 9.0;
                double nx = (x + .5 - bcx) / R, ny = (y + .5 - bcy) / R;
                double light = -(nx * 0.6 + ny * 0.85);
                double t = 0.5 + 0.32 * light - 0.14 * Math.Min(1, bestD / R);
                double edge = Math.Min(Math.Min(x, G.W - 1 - x), Math.Min(y, G.H - 1 - y));
                t -= Math.Clamp((30 - edge) / 30.0, 0, 1) * 0.22;
                t += (Fx.R(x / 3, y / 2, 303) - 0.5) * 0.2 + Fx.Bay(x, y) * 0.09;
                if (bestD / Math.Max(0.1, second) > 0.86) t -= 0.12;
                // sun-bleached tops
                if (Fx.VN(x / 25.0, y / 18.0, 305) > 0.6) t += 0.1;
                if (Fx.R(x, y, 307) > 0.965) t += 0.3;
                int idx = Math.Clamp((int)(t * C.Scrub.Length), 0, C.Scrub.Length - 1);
                Cv.Set(x, y, C.Scrub[Math.Min(idx, 5)]);
            }
        // the track leaves the map through the gap
        for (int y = G.H - 40; y < G.H; y++)
            for (int x = 70; x < 132; x++)
                if (!Terrain.ScrubM[x, y] && Terrain.WD[x, y] > 5 && Terrain.PathDist(x, y, out _) < 6 && Fx.R(x, y, 197) > 0.55 && ((x + y) & 1) == 0) Cv.Set(x, y, C.Worn[1]);
    }

    // ---- light --------------------------------------------------------------------------------------------------
    static bool Shadable(int x, int y) => x >= 0 && y >= 0 && x < G.W && y < G.H && !blockedMass[x, y] && Terrain.WD[x, y] > -0.5;

    public static void ShadeEll(double cx, double cy, double rx, double ry, double k = 1.0)
    {
        for (int y = (int)(cy - ry - 2); y <= cy + ry + 2; y++)
            for (int x = (int)(cx - rx - 2); x <= cx + rx + 2; x++)
            {
                if (!Shadable(x, y)) continue;
                double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry, d = dx * dx + dy * dy;
                if (d <= 1.0 || (d <= 1.35 && ((x + y) & 1) == 0)) Cv.Set(x, y, Cv.Get(x, y).Shade(k));
            }
    }
    public static void ShadeRect(int x0, int y0, int w, int h, double k = 1.0, bool fringe = true)
    {
        for (int y = y0 - 1; y <= y0 + h; y++)
            for (int x = x0 - 1; x <= x0 + w; x++)
            {
                if (!Shadable(x, y)) continue;
                bool inside = x >= x0 && y >= y0 && x < x0 + w && y < y0 + h;
                if (inside || (fringe && ((x + y) & 1) == 0)) Cv.Set(x, y, Cv.Get(x, y).Shade(k));
            }
    }

    static void ApplyLight()
    {
        // low sun from the north-west: the rise and the western scrub throw shadows onto the camp
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (blockedMass[x, y] || Terrain.WD[x, y] < -0.5) continue;
                int hit = 0;
                for (int t = 1; t <= 14; t++)
                {
                    bool n = y - t >= 0 && Terrain.Rise[x, y - t];
                    bool w = x - t >= 0 && Terrain.ScrubM[x - t, y] && x - t < 80;
                    bool nw = x - t >= 0 && y - t >= 0 && (Terrain.Rise[x - t, y - t]);
                    if (n || w || nw) { hit = t; break; }
                }
                if (hit == 0) continue;
                if (hit <= 5 || (((x + y) & 1) == 0 && hit <= 11)) Cv.Set(x, y, Cv.Get(x, y).Shade(hit <= 4 ? 1.0 : 0.75));
            }
        foreach (var s in Layout.Shadows)
        {
            if (s.Kind == 'e') ShadeEll(s.A, s.B, s.C, s.D, s.K);
            else ShadeRect((int)s.A, (int)s.B, (int)s.C, (int)s.D, s.K);
        }
        // warm firelight pools (a gentle orange wash with a dithered rim)
        var warm = new Rgba(255, 178, 96);
        foreach (var (gx, gy, gr, gk) in Glows)
            for (int y = (int)(gy - gr); y <= gy + gr; y++)
                for (int x = (int)(gx - gr); x <= gx + gr; x++)
                {
                    if (!Shadable(x, y)) continue;
                    double d = Math.Sqrt((x - gx) * (x - gx) + (y - gy) * (y - gy) * 1.7) / gr;
                    if (d > 1) continue;
                    double a = d < 0.4 ? 0.20 : d < 0.7 ? 0.12 : 0.06;
                    if (d >= 0.7 && ((x + y) & 1) == 0) continue;
                    var p = Cv.Get(x, y);
                    a *= gk / 12.0;
                    Cv.Set(x, y, new Rgba((byte)(p.R + (warm.R - p.R) * a), (byte)(p.G + (warm.G - p.G) * a), (byte)(p.B + (warm.B - p.B) * a)));
                }
    }

    // ---- final grade: dust haze + low-sun warmth --------------------------------------------------------------------
    static void Grade()
    {
        var haze = Rgba.Hex("F0D6A6");
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                var p = Cv.Get(x, y);
                double a = 0.05 + 0.05 * (y / (double)G.H);
                double sun = 1.04 - 0.07 * (x / (double)G.W);
                double r = p.R + (haze.R - p.R) * a, g = p.G + (haze.G - p.G) * a, b = p.B + (haze.B - p.B) * a;
                r *= sun * 1.02; g *= sun * 0.99; b *= sun * 0.95;
                Cv.Set(x, y, new Rgba((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255)));
            }
    }

    static void AddColliders()
    {
        // rock face, plateau and scrub border
        Terrain.MaskToColliders((x, y) => blockedMass[x, y], 4);
        // the map edge itself
        Layout.Col(0, G.H - 8, G.W, 8);
        // stream, except the ford
        Terrain.MaskToColliders((x, y) => Terrain.Water[x, y] && !Terrain.InFord(x, y) && Terrain.WD[x, y] < -0.6, 4);
    }
}
