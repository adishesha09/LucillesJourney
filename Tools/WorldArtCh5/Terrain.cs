namespace WorldArtCh5;

/// <summary>Fixed terrain geometry for the camp: stream, rocky rise, scrub border, worn paths. Computed before any placement.</summary>
public static class Terrain
{
    // stream centre line: x, y, half-width
    public static readonly (double x, double y, double hw)[] Stream =
    {
        (748, 78, 6), (742, 128, 9), (754, 180, 12), (768, 232, 14), (752, 288, 15), (722, 332, 18),
        (706, 372, 24), (708, 420, 18), (722, 470, 15), (700, 524, 16), (670, 574, 18), (652, 640, 20),
    };

    public static readonly (double x, double y)[][] Paths =
    {
        new[] { (390.0, 372.0), (470, 376), (540, 372), (610, 372), (648, 372) },              // camp -> water
        new[] { (545.0, 374.0), (580, 420), (630, 452), (700, 470), (752, 470) },              // camp -> ford
        new[] { (560.0, 374.0), (585, 320), (640, 270), (700, 240), (735, 232) },              // -> jug on the north bank
        new[] { (752.0, 470.0), (790, 440), (800, 380), (788, 300), (790, 220), (772, 140) },  // goat track on the far bank
        new[] { (98.0, 640.0), (100, 608), (170, 604), (260, 602), (340, 598), (404, 580), (428, 520), (430, 456), (412, 416), (396, 394) }, // way in from the south-west
    };
    public static readonly double[] PathW = { 24, 17, 12, 9, 22 };

    public static readonly (int cx, int cy, int rx, int ry)[] Trample =
    {
        (380, 350, 100, 56),   // central fire
        (150, 534, 74, 36),    // Lucille's spot
        (302, 548, 46, 28),    // neighbour family
        (650, 376, 30, 24),    // water landing
        (530, 314, 56, 34),    // cart & donkey
        (160, 224, 62, 30),    // north yards
        (254, 214, 40, 22),
        (330, 214, 36, 20),
        (410, 214, 48, 22),
        (110, 320, 58, 32),    // west yards
        (184, 372, 50, 30),
        (120, 436, 54, 24),
        (264, 456, 46, 26),
        (486, 566, 56, 28),    // south-east fire
        (554, 478, 44, 24),
    };

    public static readonly Rc Ford = new(692, 456, 78, 30);

    public static bool[,] Water = new bool[G.W, G.H];
    public static bool[,] Rise = new bool[G.W, G.H];        // everything north of the rock face foot (blocked)
    public static bool[,] ScrubM = new bool[G.W, G.H];      // border mass of dry scrub (blocked)
    public static double[,] WD = new double[G.W, G.H];      // signed distance to the stream edge (negative = inside)
    public static int[] EdgeY = new int[G.W];                // foot of the rock face per column

    public static double SegDist(double px, double py, double ax, double ay, double bx, double by, out double t)
    {
        double dx = bx - ax, dy = by - ay, l2 = dx * dx + dy * dy;
        t = l2 == 0 ? 0 : Math.Clamp(((px - ax) * dx + (py - ay) * dy) / l2, 0, 1);
        double qx = ax + t * dx - px, qy = ay + t * dy - py;
        return Math.Sqrt(qx * qx + qy * qy);
    }

    /// <summary>Centre x of the stream at a given y.</summary>
    public static double StreamX(double y)
    {
        for (int i = 0; i + 1 < Stream.Length; i++)
            if (y >= Stream[i].y && y <= Stream[i + 1].y)
            {
                double t = (y - Stream[i].y) / (Stream[i + 1].y - Stream[i].y);
                return Stream[i].x + (Stream[i + 1].x - Stream[i].x) * t;
            }
        return Stream[^1].x;
    }
    public static double StreamHw(double y)
    {
        for (int i = 0; i + 1 < Stream.Length; i++)
            if (y >= Stream[i].y && y <= Stream[i + 1].y)
            {
                double t = (y - Stream[i].y) / (Stream[i + 1].y - Stream[i].y);
                return Stream[i].hw + (Stream[i + 1].hw - Stream[i].hw) * t;
            }
        return Stream[^1].hw;
    }

    public static (double d, double hw) StreamField(double px, double py)
    {
        double best = 1e9, bhw = 0;
        for (int i = 0; i + 1 < Stream.Length; i++)
        {
            var a = Stream[i]; var b = Stream[i + 1];
            double d = SegDist(px, py, a.x, a.y, b.x, b.y, out double t);
            if (d < best) { best = d; bhw = a.hw + (b.hw - a.hw) * t; }
        }
        return (best, bhw);
    }

    public static double PathDist(double px, double py, out int idx)
    {
        double best = 1e9; idx = -1;
        for (int i = 0; i < Paths.Length; i++)
            for (int s = 0; s + 1 < Paths[i].Length; s++)
            {
                double d = SegDist(px, py, Paths[i][s].x, Paths[i][s].y, Paths[i][s + 1].x, Paths[i][s + 1].y, out _) - PathW[i] / 2.0;
                if (d < best) { best = d; idx = i; }
            }
        return best;
    }

    public static bool InFord(int x, int y) => x >= Ford.X && x < Ford.X + Ford.W && y >= Ford.Y && y < Ford.Y + Ford.H;

    public static void Init()
    {
        for (int x = 0; x < G.W; x++) EdgeY[x] = (int)(64 + 30 * Fx.Fbm(x / 80.0, 3.3, 120) + 4 * Fx.VN(x / 9.0, 2, 121));
        for (int y = 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                var (d, hw) = StreamField(x + .5, y + .5);
                double nz = (Fx.Fbm(x / 13.0, y / 13.0, 131) - 0.45) * 7;
                double wd = d - (hw + nz);
                // a small pool bulges out of the stream at the water landing
                double pdx = (x + .5 - 702) / 34.0, pdy = (y + .5 - 372) / 54.0;
                double pool = Math.Sqrt(pdx * pdx + pdy * pdy);
                if (pool < 1.25) wd = Math.Min(wd, (pool - 1.0) * 28 + nz * 0.5);
                if (y < EdgeY[x] + 4) wd = Math.Max(wd, 1);
                WD[x, y] = wd;
                Water[x, y] = wd < 0;
                Rise[x, y] = y < EdgeY[x];
                double nL = x - (26 + (Fx.Fbm(4, y / 18.0, 93) - 0.45) * 16);
                double nR = (G.W - 1 - x) - (28 + (Fx.Fbm(8, y / 18.0, 94) - 0.45) * 16);
                double nB = (G.H - 1 - y) - (26 + (Fx.Fbm(x / 18.0, 9, 95) - 0.45) * 16);
                bool gap = x > 68 && x < 134;
                bool sc = nL < 0 || nR < 0 || (!gap && nB < 0);
                if (gap && y > G.H - 8) sc = false;
                ScrubM[x, y] = sc && !Rise[x, y];
            }
    }

    // ---- turn masks into collider rectangles ---------------------------------------------------------------------------
    public static void MaskToColliders(Func<int, int, bool> mask, int band = 4, int xMin = 0, int xMax = G.W)
    {
        var open = new Dictionary<(int x0, int x1), Rc>();
        bool Any(int x, int y) { for (int k = 0; k < band && y + k < G.H; k++) if (mask(x, y + k)) return true; return false; }
        for (int y = 0; y <= G.H; y += band)
        {
            var runs = new List<(int, int)>();
            if (y < G.H)
            {
                int x = xMin;
                while (x < xMax)
                {
                    if (!Any(x, y)) { x++; continue; }
                    int s = x; while (x < xMax && Any(x, y)) x++;
                    runs.Add((s, x));
                }
            }
            var next = new Dictionary<(int, int), Rc>();
            foreach (var r in runs)
            {
                if (open.TryGetValue(r, out var rc)) { next[r] = rc with { H = rc.H + band }; open.Remove(r); }
                else next[r] = new Rc(r.Item1, y, r.Item2 - r.Item1, Math.Min(band, G.H - y));
            }
            foreach (var kv in open) Layout.Col(kv.Value.X, kv.Value.Y, kv.Value.W, kv.Value.H);
            open = next;
        }
    }
}
