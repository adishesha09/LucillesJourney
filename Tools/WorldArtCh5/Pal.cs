namespace WorldArtCh5;

public static class K
{
    static Rgba H(string h) => Rgba.Hex(h);

    // brief palette
    public static readonly Rgba Cream = H("F4EAD2"), Parch = H("E8D9B5"), Sand = H("C8B58A"), Tan = H("A8895E"),
        Brown = H("6A5444"), Brown2 = H("4A3B32"), Brown3 = H("2B2321"), Terra = H("C05A4A"), Orange = H("D98F4B"),
        Gold = H("F2D26B"), Green = H("6FA86B"), DGreen = H("3F6B45"), Teal = H("4B8F8A"), Blue = H("6B8FA8");

    // warm outlines / shadows
    public static readonly Rgba Line = H("3B2A28"), Line2 = H("4F3A34"), ShadowP = H("5A4250");

    // grass ramp (dark blue-green -> yellow-green)
    public static readonly Rgba[] Grass = { H("3F6B45"), H("4F8A58"), H("5E9A5F"), H("6FA86B"), H("82B570"), H("98C47A"), H("B4D283") };

    // dirt
    public static readonly Rgba[] Dirt = { H("8F7552"), H("A18560"), H("B39A6C"), H("C4AC7C"), H("D2BE8E"), H("DECDA0") };

    // plaster
    public static readonly Rgba[] Plaster = { H("B9A47C"), H("D4C197"), H("E8D9B5"), H("F4EAD2"), H("FBF3DE") };
    public static readonly Rgba[] Peach = { H("B98F6C"), H("D2A982"), H("E3C09A"), H("EED2AE"), H("F7E2C4") };
    public static readonly Rgba[] BlueWall = { H("8FA8B0"), H("A8C0C4"), H("BFD3D2"), H("D3E2DC"), H("E6EFE6") };

    // wood
    public static readonly Rgba[] Wood = { H("5E3B25"), H("7F5232"), H("A0693C"), H("B87F47"), H("D09A5C"), H("E6B97A") };
    public static readonly Rgba[] Pine = { H("8E6A44"), H("AD8556"), H("C79E68"), H("DDB67B"), H("EDCB94") };
    public static readonly Rgba[] Dark = { H("2B2321"), H("3A2D29"), H("4A3B32"), H("5E4A3C"), H("6A5444") };

    // stone
    public static readonly Rgba[] Stone = { H("5E564F"), H("7A7066"), H("9A9082"), H("B8AE9C"), H("D3CAB8"), H("E6DFCF") };
    public static readonly Rgba Mortar = H("6B6057");

    // roof terracotta
    public static readonly Rgba[] Roof = { H("7F3A36"), H("A44A40"), H("C05A4A"), H("D46F55"), H("E58C66") };

    // teal / blue / red / mustard
    public static readonly Rgba[] TealR = { H("2B5654"), H("3A726F"), H("4B8F8A"), H("6FB0A6"), H("9FD0C4") };
    public static readonly Rgba[] BlueR = { H("3E5A6E"), H("4F7189"), H("6B8FA8"), H("8FB0C2"), H("B8D2DC") };
    public static readonly Rgba[] RedR = { H("7A3535"), H("A04545"), H("C05A4A"), H("D9786A"), H("EE9A86") };
    public static readonly Rgba[] Mustard = { H("A8772A"), H("C9952F"), H("E0B040"), H("F2D26B"), H("FBE9A8") };
    public static readonly Rgba[] Linen = { H("B8A88A"), H("D4C7A8"), H("E8DEC4"), H("F4EDDA"), H("FFFAEC") };
    public static readonly Rgba[] Iron = { H("2E2A2E"), H("453F45"), H("5E5660"), H("7B727C"), H("9D94A0") };
    public static readonly Rgba[] Pink = { H("B5667A"), H("D4849A"), H("EAA5B5"), H("F6C6CF") };
    public static readonly Rgba[] Purple = { H("5E4E80"), H("7C6AA3"), H("9F8CC4"), H("C0B2DF") };

    // foliage ramps (dark -> light)
    public static readonly Rgba[] Leaf = { H("2A4A38"), H("35603F"), H("447A4B"), H("5A9458"), H("78AF63"), H("9CC873"), H("BFDC8A") };
    public static readonly Rgba[] LeafWarm = { H("31503A"), H("3F6B45"), H("558A4F"), H("6FA86B"), H("8FBF6B"), H("B1D27A") };
    public static readonly Rgba[] LeafDeep = { H("1F3A32"), H("2A4D3E"), H("35614A"), H("447A55"), H("5A9262") };
    public static readonly Rgba[] Cypress = { H("1F3D33"), H("2A5240"), H("37694C"), H("478259"), H("5E9A66") };
    public static readonly Rgba[] Fruit = { H("5E7A3A"), H("739446"), H("8DB052"), H("A9C860"), H("C6DC7A") };

    public static readonly Rgba Soil = H("6B4A38"), SoilDark = H("4E362C"), SoilLight = H("8A6448");
    public static readonly Rgba WetSoil = H("45302A"), WetSoil2 = H("33231F"), WetSoilHi = H("6A4C43");
    public static readonly Rgba Water = H("8FC9D6"), WaterHi = H("D6F1F2");
}

/// <summary>Reusable shading helpers for hand-made-looking pixel art.</summary>
public static class Draw
{
    /// <summary>Dome-shaded blob: light from top-left, clumpy leaf texture, ordered-dither tone transitions.</summary>
    public static void Blob(Canvas c, double cx, double cy, double rx, double ry, Rgba[] ramp, int seed, double bias = 0.0, int cell = 3)
    {
        int n = ramp.Length;
        c.Paint((x, y) =>
        {
            double nx = (x + .5 - cx) / rx, ny = (y + .5 - cy) / ry, r2 = nx * nx + ny * ny;
            if (r2 > 1) return false;
            return true;
        }, ramp[0], (int)(cx - rx - 1), (int)(cy - ry - 1), (int)(cx + rx + 2), (int)(cy + ry + 2));
        for (int y = Math.Max(0, (int)(cy - ry - 1)); y < Math.Min(c.H, (int)(cy + ry + 2)); y++)
            for (int x = Math.Max(0, (int)(cx - rx - 1)); x < Math.Min(c.W, (int)(cx + rx + 2)); x++)
            {
                double nx = (x + .5 - cx) / rx, ny = (y + .5 - cy) / ry, r2 = nx * nx + ny * ny;
                if (r2 > 1) continue;
                double light = -(nx * 0.6 + ny * 0.8);
                double t = 0.5 + 0.42 * light - 0.18 * r2 + bias;
                int bx = x / cell, by = (y + ((x / cell) & 1) * 1) / 2;
                t += (Fx.R(bx, by, seed) - 0.5) * 0.26;
                t += Fx.Bay(x, y) * 0.10;
                int idx = (int)Math.Floor(t * n);
                c.Set(x, y, ramp[Math.Clamp(idx, 0, n - 1)]);
            }
    }

    /// <summary>Cylinder-like vertical shading (trunks, posts, barrels): left light, right dark.</summary>
    public static void Cyl(Canvas c, int x, int y, int w, int h, Rgba[] ramp, int seed = 0, bool grain = true)
    {
        int n = ramp.Length;
        for (int i = 0; i < w; i++)
        {
            double t = w == 1 ? 0.5 : 1.0 - (i + 0.5) / w; // left=1
            t = Math.Clamp(t * 1.05 + (t > 0.7 ? 0.1 : 0), 0, 0.999);
            int idx = Math.Clamp((int)(t * n), 0, n - 1);
            for (int j = 0; j < h; j++)
            {
                int k = idx;
                if (grain && Fx.R(i, j / 3, seed) > 0.86 && idx > 0) k = idx - 1;
                c.Set(x + i, y + j, ramp[k]);
            }
        }
    }

    /// <summary>Horizontal wood-plank face (front of boxes): rows lit from top.</summary>
    public static void Planks(Canvas c, int x, int y, int w, int h, Rgba[] ramp, int seed, int rowH = 4)
    {
        for (int j = 0; j < h; j++)
        {
            int row = j / rowH;
            int tone = 2 + (int)(Fx.R(row, 0, seed) * 2);
            for (int i = 0; i < w; i++)
            {
                int k = tone;
                if (j % rowH == 0) k = Math.Min(ramp.Length - 1, tone + 1);
                if (j % rowH == rowH - 1) k = Math.Max(0, tone - 2);
                c.Set(x + i, y + j, ramp[k]);
            }
        }
    }

    /// <summary>Ground ellipse shadow baked into a sprite as dark purple-brown, dithered edge.</summary>
    public static void Gloss(Canvas c, int x, int y, Rgba col) => c.Set(x, y, col);

    public static Rgba Pick(Rgba[] ramp, double t) => ramp[Math.Clamp((int)(t * ramp.Length), 0, ramp.Length - 1)];
}
