using static MenuArt.Kit;

namespace MenuArt;

public static class EndingSky
{
    public const int SunStartX = 124, SunStartY = 124, SunEndY = 84;

    public static Canvas Sky()
    {
        var c = new Canvas(384, 216);
        var stops = new (double, Rgba)[]
        {
            (0, C("6B8FA8")), (22, C("7B9AAF")), (44, C("97A9B3")), (64, C("B3ADAB")), (82, C("CDB59F")),
            (98, C("DDBE9C")), (108, C("EBD2AA")), (116, C("F4EAD2")), (123, C("F6E0A0")), (129, C("F2D26B")),
            (142, C("EEC670")), (216, C("DDB474")),
        };
        for (int y = 0; y < 216; y++)
            for (int x = 0; x < 384; x++)
            {
                double lift = 14 * Gauss(x - SunStartX, 90) * Smooth(60, 125, y + 20);
                double wob = (N(x / 80.0, y / 3.0, 7) - 0.5) * 2.6;
                c.Set(x, y, Grad(stops, y + lift + wob, x, y, 1.9));
            }
        return c;
    }

    public static Canvas Sun()
    {
        var c = new Canvas(64, 64);
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                double d = Math.Sqrt((x - 31.5) * (x - 31.5) + (y - 31.5) * (y - 31.5));
                if (d <= 8.5)
                {
                    double t = d / 8.5;
                    c.Set(x, y, t < 0.5 ? C("FFF6DC") : t < 0.82 ? C("FBEBB8") : C("F6DC8E"));
                    continue;
                }
                double v = (d - 8.5) / 23.0;
                if (v >= 1) continue;
                double dens = Math.Floor(Math.Pow(1 - v, 1.9) * 0.9 * 8 + 0.5) / 8.0;
                if (dens <= Bay8(x, y)) continue;
                c.Set(x, y, v < 0.28 ? C("F8E6A8") : v < 0.6 ? C("F4DC96") : C("F0CF84"));
            }
        return c;
    }

    public static Canvas Clouds()
    {
        var c = new Canvas(768, 72);
        var ramp = Ramp("FAF3E2", "F4E8D2", "EEDCC4", "E8CAB2", "DDB4A2");
        var rim = C("F2C8A6"); var rim2 = C("F6DBBE");
        var defs = new (double cx, double cy, double hw, double th, double bh)[]
        {
            (80, 22, 112, 3.4, 2.4), (300, 46, 90, 3, 2.2), (452, 14, 70, 2.6, 1.8), (566, 36, 124, 3.6, 2.4), (708, 58, 56, 2.2, 1.6), (190, 62, 46, 2, 1.5),
        };
        int s = 3;
        foreach (var d in defs) MenuSky.Cloud(c, d.cx, d.cy, d.hw, d.th, d.bh, s++ * 5, ramp, rim, rim2);
        return c;
    }

    public static Canvas Bird(bool up)
    {
        var c = new Canvas(9, 5);
        var d = C("3F4452"); var d2 = C("5A5C6A");
        string[] a = up
            ? new[] { "#.......#", ".##...##.", "..##.##..", "...###...", "....#...." }
            : new[] { "....#....", "..#####..", ".##.#.##.", "##..#..##", "........." };
        for (int y = 0; y < 5; y++) for (int x = 0; x < 9; x++) if (a[y][x] == '#') c.Set(x, y, (x == 4 && y > 0) ? d2 : d);
        return c;
    }

    public static Canvas Dust2()
    {
        var c = new Canvas(2, 2);
        c.Set(0, 0, C("F4EAD2")); c.Set(1, 0, C("F4EAD2")); c.Set(0, 1, C("F4EAD2"));
        return c;
    }

    public static Canvas Dust3()
    {
        var c = new Canvas(3, 3);
        c.Set(1, 1, C("FBF1D2"));
        foreach (var (x, y) in new[] { (1, 0), (0, 1), (2, 1), (1, 2) }) c.Set(x, y, C("EFDDB8"));
        return c;
    }

    /// <summary>Soft additive glow around the town (and the sun's start position). Coordinates are the 384x216 screen frame.</summary>
    public static Canvas Glow(IEnumerable<(int x, int y)> townWindows)
    {
        var g = new Canvas(384, 216);
        var bands = Ramp("F6E4A8", "F2D26B", "EFC27A", "E8A878");
        for (int y = 0; y < 216; y++)
            for (int x = 0; x < 384; x++)
            {
                double dx = (x - 160) / 128.0, dy = (y - 124) / 52.0;
                double d = Math.Sqrt(dx * dx + dy * dy);
                if (d >= 1) continue;
                double dens = Math.Floor(Math.Pow(1 - d, 2.0) * 0.8 * 8 + 0.5) / 8.0;
                if (dens <= Bay8(x, y)) continue;
                g.Set(x, y, bands[Math.Min(3, (int)(d * 4.2))]);
            }
        foreach (var (wx, wy) in townWindows)
            for (int y = wy - 4; y <= wy + 4; y++)
                for (int x = wx - 4; x <= wx + 4; x++)
                {
                    double d = Math.Sqrt((x - wx) * (x - wx) + (y - wy) * (y - wy) * 1.3);
                    if (d < 0.5 || d > 4.2) continue;
                    double dens = Math.Floor(Math.Pow(1 - d / 4.2, 1.4) * 0.95 * 6 + 0.5) / 6.0;
                    if (dens > Bay8(x, y)) g.Set(x, y, d < 2.0 ? C("F2D26B") : C("E8A858"));
                }
        return g;
    }
}
