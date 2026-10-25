namespace WorldArtCh5;

/// <summary>Shared colour ramps and small helpers for the camp art.</summary>
public static class C
{
    static Rgba[] R(params string[] h) => h.Select(Rgba.Hex).ToArray();

    // dry-country ground
    public static readonly Rgba[] Earth = R("98714D", "AA8059", "BC9267", "CCA676", "DABA89", "E6CB9E");
    public static readonly Rgba[] Packed = R("A67F58", "B8926A", "C8A578", "D6B88B", "E3CA9F", "EDD8B2");
    public static readonly Rgba[] Worn = R("B9946A", "C9A678", "D8B98C", "E5CBA0", "EFD9B3");
    public static readonly Rgba[] Dry = R("6E6738", "807841", "938A4B", "A79D58", "BBAF68", "CDC17D");
    public static readonly Rgba[] Fresh = R("4F6B3A", "5F7F45", "729552", "88AA5E", "A0BF70", "B9D184");
    public static readonly Rgba[] Rock = R("5C4D47", "75645A", "8F7B6C", "A99480", "C3AE98", "DBC8AE");
    public static readonly Rgba[] Scrub = R("4A4A2E", "5C5A36", "726D40", "8A8350", "A29A62", "BAB078");
    public static readonly Rgba[] ScrubDark = R("383628", "46432C", "575238", "6B6540");
    public static readonly Rgba[] Mud = R("5A4234", "6E5340", "866650", "9F7F62");
    public static readonly Rgba[] Water = R("46787C", "568E92", "6CA3A3", "8CBDB9", "B2D6CC", "DAEFE2");
    public static readonly Rgba[] Pebble = R("8E7C68", "A8937B", "C2AE94", "D8C8AE");

    // dry-tinted tree foliage
    public static readonly Rgba[] Olive = R("46512F", "58653A", "6E7C48", "879356", "A3AB6B", "BFC283");

    // tarp / cloth families, 4 tones dark -> light
    public static readonly Rgba[] Ochre = R("93733F", "B08E54", "CAA96E", "DEC48C");
    public static readonly Rgba[] Blue = R("46607A", "5B7A94", "7897AD", "9BB4C3");
    public static readonly Rgba[] Drab = R("525C38", "697648", "859060", "A2AC7E");
    public static readonly Rgba[] Clay = R("7A4036", "955245", "B16A56", "C98772");
    public static readonly Rgba[] Cream = R("A99C80", "C4B796", "DCD1B3", "EEE6CE");
    public static readonly Rgba[] Rust = R("65463A", "81604C", "9D7A60", "B8967A");
    public static readonly Rgba[] Teal = R("3B6462", "507D79", "6C9992", "8DB5AB");
    public static readonly Rgba[] Mustard = R("9A7633", "B8923F", "D1AE58", "E5C97F");
    public static readonly Rgba[] Plum = R("5F4B5C", "786175", "937B8F", "AE98AA");
    public static readonly Rgba[] Grey = R("5E5B58", "78746F", "948F88", "B1AB9F");

    public static readonly Rgba Out = Rgba.Hex("3B2A28");
    public static readonly Rgba Rope = Rgba.Hex("B59E72"), RopeD = Rgba.Hex("8A7450");
    public static readonly Rgba Inside = Rgba.Hex("2E2325"), Inside2 = Rgba.Hex("463436");

    // skin tones (dark -> light)
    public static readonly Rgba[] SkinA = R("B87F5B", "D29A70", "E3B38A");
    public static readonly Rgba[] SkinB = R("94603F", "AF7A52", "C69167");
    public static readonly Rgba[] SkinC = R("D09873", "E5B590", "F1CCAA");
    public static readonly Rgba[] HairDk = R("2A201E", "3E2F2A", "56433A");
    public static readonly Rgba[] HairBr = R("4A3428", "63473A", "7E5D4A");
    public static readonly Rgba[] HairGr = R("77706A", "9A938C", "BDB6AD");
    public static readonly Rgba[] HairBl = R("7E6038", "9E7C4A", "BD9A60");

    public static Rgba Mix(Rgba a, Rgba b, double t) =>
        new((byte)(a.R + (b.R - a.R) * t), (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t));
}

public static class Tints
{
    /// <summary>Re-colour foliage greens into dusty dry olive, preserving luminance structure.</summary>
    public static Canvas Dry(Canvas src, double dust = 0.30, double yellow = 1.0)
    {
        var o = new Canvas(src.W, src.H);
        for (int y = 0; y < src.H; y++)
            for (int x = 0; x < src.W; x++)
            {
                var p = src.Get(x, y);
                if (p.IsClear) continue;
                double r = p.R, g = p.G, b = p.B;
                bool green = g > r + 6 && g >= b;
                if (green)
                {
                    double lum = 0.3 * r + 0.59 * g + 0.11 * b;
                    // map to olive/khaki ramp keeping lightness
                    double nr = lum * 1.06 + 12 * yellow, ng = lum * 1.0 + 4 * yellow, nb = lum * 0.62;
                    r = nr; g = ng; b = nb;
                    // dust: pull towards warm cream
                    r = r + (212 - r) * dust * 0.4; g = g + (196 - g) * dust * 0.4; b = b + (150 - b) * dust * 0.4;
                }
                else if (g > b && r > g && p.R > 120)
                {
                    // warm things (fruit, trunk highlights) just get duller
                    r = r * 0.9 + 14; g = g * 0.9 + 10; b = b * 0.9;
                }
                o.Set(x, y, new Rgba((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255)));
            }
        return o;
    }

    public static Canvas Warm(Canvas src, double k = 1.0)
    {
        var o = new Canvas(src.W, src.H);
        for (int y = 0; y < src.H; y++)
            for (int x = 0; x < src.W; x++)
            {
                var p = src.Get(x, y);
                if (p.IsClear) continue;
                o.Set(x, y, new Rgba((byte)Math.Clamp(p.R * (1 + 0.06 * k) + 6 * k, 0, 255), (byte)Math.Clamp(p.G * (1 - 0.01 * k) + 1 * k, 0, 255), (byte)Math.Clamp(p.B * (1 - 0.12 * k), 0, 255)));
            }
        return o;
    }

    /// <summary>Replace red fruit-like pixels with the nearby leaf tone (for olive trees).</summary>
    public static Canvas RemoveRed(Canvas src)
    {
        var o = src.Clone();
        for (int y = 0; y < o.H; y++)
            for (int x = 0; x < o.W; x++)
            {
                var p = o.Get(x, y);
                if (p.IsClear) continue;
                if (p.R > p.G + 35 && p.R > p.B + 35 && y < o.H * 0.75) o.Set(x, y, C.Olive[1 + (x + y) % 3]);
            }
        return o;
    }
}
