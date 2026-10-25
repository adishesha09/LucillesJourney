namespace WorldArtCh6;

public static class Street
{
    static Canvas Cv => Ground.Cv;

    public static void Paint()
    {
        for (int y = Geo.FrontBot - 0; y < G.H; y++)
            for (int x = 0; x < G.W; x++)
            {
                if (y < Geo.SideY1) Ground.PxPublic(x, y, Ground.PaveSlab(x, y), Mat.Pave, true);
                else if (y < Geo.RoadY0) Ground.PxPublic(x, y, Kerb(x, y), Mat.Kerb, true);
                else if (y < Geo.RoadY1) Ground.PxPublic(x, y, Road(x, y), Mat.Cobble, true);
                else if (y < Geo.RoadY1 + 4) Ground.PxPublic(x, y, Kerb(x, y, true), Mat.Kerb, true);
                else if (y < Geo.FarY1) Ground.PxPublic(x, y, Ground.PaveSlab(x, y), Mat.Pave, true);
                else Ground.PxPublic(x, y, OppositeRoof(x, y), Mat.Roof, true);
            }
        FrontStep();
        Patch(150, 384, 44, 14);
        Patch(560, 372, 30, 18);
        Puddles();
        Grate(96); Grate(300); Grate(520); Grate(690);
        Manhole(318, 410);
        Manhole(612, 424);
        Litter();
    }

    static Rgba Kerb(int x, int y, bool far = false)
    {
        int v = far ? y - Geo.RoadY1 : y - Geo.SideY1;       // 0..3 (near: 358..361)
        int jx = x % 24;
        if (!far)
        {
            if (v == 0) return E.Paving[4];
            if (v == 1) return E.Paving[3];
            if (v == 2) return jx == 0 ? E.Paving[0] : E.Paving[1];
            return E.Cobble[0];
        }
        if (v == 0) return E.Cobble[0];
        if (v == 1) return jx == 0 ? E.Paving[0] : E.Paving[2];
        if (v == 2) return E.Paving[3];
        return E.Paving[4];
    }

    static Rgba Road(int x, int y)
    {
        var c = Ground.CobbleDark(x, y);
        // smooth worn centre track
        double lane = Math.Abs(y - 400) / 38.0;
        double wear = Fx.Fbm(x / 24.0, y / 10.0, 231);
        if (wear > 0.62 && lane < 0.9 && c.R > 0x40) c = Rgba.Mul(c, 1.08, 1.06, 1.04);
        // gutter darkening along the kerb
        int dk = y - Geo.RoadY0;
        if (dk < 3) c = c.Shade(0.25 - dk * 0.07);
        int df = Geo.RoadY1 - 1 - y;
        if (df < 2) c = c.Shade(0.2);
        // faint dashed centre line
        if (y == 399 || y == 400)
            if ((x / 14) % 2 == 0 && Fx.R(x, y, 233) > 0.25) c = Rgba.Mul(E.Cobble[4], 1.05, 1.02, 0.95);
        return c;
    }

    static readonly int[] OppEdges = { 0, 130, 262, 402, 540, 662, 768 };

    static Rgba OppositeRoof(int x, int y)
    {
        int b = 0;
        while (b + 2 < OppEdges.Length && x >= OppEdges[b + 1]) b++;
        var ramp = (b % 3) switch { 0 => E.DuskRoofGreen, 1 => E.DuskRoofWarm, _ => E.DuskRoof };
        int v = y - Geo.FarY1;
        if (v < 3) return ramp[v == 0 ? 0 : 1];             // eave shadow
        return Ground.RoofBlockPx(x, y, OppEdges[b], Geo.FarY1 + 3, OppEdges[b + 1], G.H + 40, G.H + 20, ramp, 181 + b, 11, 8);
    }

    static void FrontStep()
    {
        // two stone steps below the entrance, wider than the doorway
        for (int s = 0; s < 2; s++)
        {
            int y0 = 336 + s * 7, w = 52 - s * 0, x0 = 376 - w / 2 - 0;
            for (int j = 0; j < 7; j++)
                for (int x = x0; x < x0 + w; x++)
                {
                    int lx = x - x0;
                    int idx = j == 0 ? 4 : j < 5 ? 3 : 1;
                    if (lx == 0 || lx == w - 1) idx = 1;
                    if (j >= 5 && lx % 11 == 3) idx = 0;
                    Ground.PxPublic(x, y0 + j, Rgba.Mul(K.Stone[Math.Clamp(idx, 0, 5)], 0.98, 1.0, 1.05), Mat.Step, true);
                }
        }
    }

    static readonly (int x, int y, int rx, int ry)[] PuddleList = { (212, 392, 14, 4), (470, 380, 18, 5), (688, 404, 12, 4), (130, 420, 10, 3), (548, 372, 9, 3) };

    static void Puddles()
    {
        foreach (var (px, py, rx, ry) in PuddleList)
            for (int y = py - ry; y <= py + ry; y++)
                for (int x = px - rx; x <= px + rx; x++)
                {
                    double dx = (x - px) / (double)rx, dy = (y - py) / (double)ry;
                    double d = dx * dx + dy * dy + (Fx.R(x, y, 241) - 0.5) * 0.25;
                    if (d > 1) continue;
                    var c = d > 0.75 ? E.Cobble[0] : Rgba.Hex("4F5F8A");
                    if (d < 0.5 && y < py) c = Rgba.Hex("6479A6");
                    if (d < 0.25 && x < px) c = Rgba.Hex("A9BEDC");
                    Ground.PxPublic(x, y, c, Mat.Cobble, true);
                }
    }

    static void Manhole(int cx, int cy)
    {
        for (int y = -5; y <= 5; y++)
            for (int x = -10; x <= 10; x++)
            {
                double dx = x / 10.0, dy = y / 5.0, d = dx * dx + dy * dy;
                if (d > 1) continue;
                Rgba c;
                if (d > 0.80) c = K.Iron[0];                                   // rim
                else if (d > 0.70) c = K.Iron[3];
                else
                {
                    c = K.Iron[2];
                    if ((x + 20) % 4 == 0 || Math.Abs(y) == 1 && x % 2 == 0) c = K.Iron[1];      // cast pattern
                    if (dx + dy < -0.35) c = K.Iron[3];
                }
                Ground.PxPublic(cx + x, cy + y, c, Mat.Cobble, true);
            }
    }

    static void Grate(int cx)
    {
        for (int j = 0; j < 5; j++)
            for (int i = -8; i < 8; i++)
                Ground.PxPublic(cx + i, Geo.RoadY0 + 1 + j, (i + 8) % 3 == 0 ? K.Iron[3] : K.Iron[0], Mat.Cobble, true);
    }

    /// <summary>Straight-edged patch of smooth repair surface in the cobbles.</summary>
    static void Patch(int x0, int y0, int w, int h)
    {
        for (int y = y0; y < y0 + h; y++)
            for (int x = x0; x < x0 + w; x++)
            {
                double n = Fx.VN(x / 3.0, y / 3.0, 251);
                var c = n > 0.66 ? E.Cobble[2] : n > 0.3 ? E.Cobble[1] : E.Cobble[0];
                if (x == x0 || y == y0) c = E.Cobble[0];
                if (x == x0 + w - 1 || y == y0 + h - 1) c = E.Cobble[3];
                Ground.PxPublic(x, y, c, Mat.Cobble, true);
            }
    }

    static void Litter()
    {
        var cols = new[] { K.Orange, K.Gold, K.Terra, K.Tan, K.Cream };
        for (int gy = Geo.FrontBot + 4; gy < Geo.FarY1; gy += 9)
            for (int gx = 4; gx < G.W - 4; gx += 11)
            {
                int x = gx + (int)(Fx.R(gx, gy, 261) * 10), y = gy + (int)(Fx.R(gx, gy, 262) * 8);
                if (Fx.R(gx, gy, 263) > 0.16 || x >= G.W - 2 || y >= Geo.FarY1) continue;
                if (y >= Geo.SideY1 && y < Geo.RoadY0) continue;
                var c = cols[(int)(Fx.R(gx, gy, 264) * cols.Length)];
                Ground.PxPublic(x, y, c, Ground.M[x, y], true);
                if (Fx.R(gx, gy, 265) > 0.5) Ground.PxPublic(x + 1, y, Rgba.Mul(c, 0.8, 0.8, 0.8), Ground.M[x + 1, y], true);
            }
        // weeds pushing up along the kerb
        for (int x = 3; x < G.W - 3; x += 7)
        {
            if (Fx.R(x, 0, 266) > 0.4) continue;
            int y = Geo.RoadY0 + 2;
            Ground.PxPublic(x, y, K.Grass[3], Mat.Cobble, true); Ground.PxPublic(x, y - 1, K.Grass[5], Mat.Cobble, true); Ground.PxPublic(x + 1, y, K.Grass[1], Mat.Cobble, true);
        }
    }

    public static void Emit() { }
}
