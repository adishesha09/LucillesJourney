namespace WorldArtCh6;

/// <summary>Evening colours and small helpers shared by the ground and the sprites.</summary>
public static class E
{
    static Rgba H(string h) => Rgba.Hex(h);

    // dusk sky for the window, horizon -> zenith
    public static readonly Rgba[] Sky = { H("F2B27A"), H("E59A7E"), H("C98A92"), H("9C7FA0"), H("6F76A6"), H("4A5C93"), H("34457A") };
    // lamp light
    public static readonly Rgba Warm = H("FFD58A"), WarmHi = H("FFF0C2"), WarmLow = H("F2A65A");
    // cool dusk street: written as warm daylight greys, the outdoor grade turns them blue
    public static readonly Rgba[] Cobble = { H("4A4640"), H("5E5850"), H("746C62"), H("8A8174"), H("A0968A") };
    public static readonly Rgba[] Paving = { H("827C72"), H("9A9486"), H("B0AA9A"), H("C6C0AE"), H("DAD4C2") };
    public static readonly Rgba[] DuskRoof = { H("3A404C"), H("4B5262"), H("5E6678"), H("737B8E"), H("8A92A4") };           // slate
    public static readonly Rgba[] DuskRoofWarm = { H("55383A"), H("6B4A4A"), H("825D5A"), H("987069"), H("AE857A") };       // muted terracotta
    public static readonly Rgba[] DuskRoofGreen = { H("33463F"), H("445850"), H("566C63"), H("6A8277"), H("80988C") };      // old copper
    public static readonly Rgba[] Lobby = { H("8E8A8A"), H("A9A5A4"), H("C2BDB8"), H("D6D1C6"), H("E8E3D6") };

    /// <summary>Multiply a sprite by a colour cast, keeping outlines readable.</summary>
    public static Canvas Tinted(Canvas src, double r, double g, double b, double lift = 0.0)
    {
        var o = new Canvas(src.W, src.H);
        for (int y = 0; y < src.H; y++)
            for (int x = 0; x < src.W; x++)
            {
                var p = src.Get(x, y);
                if (p.A == 0) continue;
                o.Set(x, y, new Rgba(
                    (byte)Math.Clamp(p.R * r + lift * 255, 0, 255),
                    (byte)Math.Clamp(p.G * g + lift * 255, 0, 255),
                    (byte)Math.Clamp(p.B * b + lift * 255, 0, 255)));
            }
        return o;
    }

    public static Rgba Cool(Rgba c, double k = 1.0)
    {
        double lum = (c.R * 0.3 + c.G * 0.55 + c.B * 0.15);
        double r = c.R * (1 - 0.30 * k) + 0.0, g = c.G * (1 - 0.20 * k), b = c.B * (1 - 0.06 * k) + 14 * k;
        // keep a little of the luminance so highlights do not go muddy
        r += lum * 0.03 * k; g += lum * 0.05 * k; b += lum * 0.10 * k;
        return new Rgba((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255));
    }

    public static Rgba Lerp(Rgba a, Rgba b, double t) =>
        new((byte)Math.Clamp(a.R + (b.R - a.R) * t, 0, 255), (byte)Math.Clamp(a.G + (b.G - a.G) * t, 0, 255), (byte)Math.Clamp(a.B + (b.B - a.B) * t, 0, 255));

    /// <summary>Light tint towards a lamp colour; t in 0..1.</summary>
    public static Rgba Glow(Rgba c, Rgba lamp, double t)
    {
        var l = new Rgba((byte)Math.Clamp(c.R + 26 * t, 0, 255), (byte)Math.Clamp(c.G + 20 * t, 0, 255), (byte)Math.Clamp(c.B + 2 * t, 0, 255));
        return Lerp(l, lamp, 0.20 * t);
    }
}
