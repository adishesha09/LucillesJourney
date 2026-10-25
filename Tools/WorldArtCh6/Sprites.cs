namespace WorldArtCh6;

/// <summary>Registration of re-tinted copies of Chapter 1 sprites plus all new sprites.</summary>
public static class Sprites
{
    public static string Re(string src, double r, double g, double b, string? key = null, double lift = 0.0)
    {
        string name = "prop_c6_" + (key ?? src.Replace("prop_", ""));
        if (!S.All.ContainsKey(name)) S.Add(name, E.Tinted(S.Ext(src), r, g, b, lift));
        return name;
    }

    /// <summary>Furniture indoors keeps its warm daylight shading, only a little dimmer and rosier for lamp-lit evening.</summary>
    public static string In(string src, string? key = null) => Re(src, 0.95, 0.90, 0.90, key);
    /// <summary>Outdoor things take the blue evening cast.</summary>
    public static string Out(string src, string? key = null) => Re(src, 0.80, 0.88, 1.06, key);

    public static void Build()
    {
        S.Cur = "furniture"; Furniture.Build();
        S.Cur = "decor"; Decor.Build();
        S.Cur = "street"; StreetProps.Build();
        S.Cur = "npc"; Npcs.Build();
        S.Cur = "fx"; Fx6.Build();
    }
}

public static class Kit
{
    /// <summary>Shaded box: lit top face, mid front, with highlights on the left edge and shade on the right edge.</summary>
    public static void Prism(Canvas c, int x, int y, int w, int topH, int frontH, Rgba[] ramp, int tone = 3)
    {
        for (int j = 0; j < topH; j++)
            for (int i = 0; i < w; i++)
            {
                int idx = Math.Min(ramp.Length - 1, tone + 1);
                if (j == 0 || i == 0) idx = Math.Min(ramp.Length - 1, tone + 2);
                c.Set(x + i, y + j, ramp[idx]);
            }
        for (int j = 0; j < frontH; j++)
            for (int i = 0; i < w; i++)
            {
                int idx = tone - 1;
                if (j == 0) idx = tone;
                if (i == w - 1) idx = tone - 2;
                if (i == 0) idx = tone;
                if (j == frontH - 1) idx = Math.Max(0, tone - 2);
                c.Set(x + i, y + topH + j, ramp[Math.Clamp(idx, 0, ramp.Length - 1)]);
            }
    }

    public static Rgba Dk(Rgba c, double k = 0.45) => new((byte)(c.R * k + 18), (byte)(c.G * k * 0.95 + 12), (byte)(c.B * k * 0.95 + 14));

    /// <summary>Outline a sprite with a dark warm version of its own colours (adds a 1px margin).</summary>
    public static Canvas Outlined(Canvas c, double k = 0.40)
    {
        var o = new Canvas(c.W + 2, c.H + 2);
        o.Blit(c, 1, 1);
        o.SelOutline(k);
        return o;
    }

    public static void Ellipse(Canvas c, double cx, double cy, double rx, double ry, Rgba mid, Rgba hi, Rgba lo)
    {
        for (int y = (int)(cy - ry - 1); y <= cy + ry + 1; y++)
            for (int x = (int)(cx - rx - 1); x <= cx + rx + 1; x++)
            {
                double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry;
                double d = dx * dx + dy * dy;
                if (d > 1) continue;
                double light = -(dx * 0.6 + dy * 0.8);
                c.Set(x, y, light > 0.35 ? hi : light < -0.35 ? lo : mid);
            }
    }
}
