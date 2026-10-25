using System.IO.Compression;

namespace PixelArt;

public readonly record struct Rgba(byte R, byte G, byte B, byte A = 255)
{
    public static readonly Rgba Clear = new(0, 0, 0, 0);
    public bool IsClear => A == 0;

    public static Rgba Hex(string h) =>
        new(Convert.ToByte(h.Substring(0, 2), 16), Convert.ToByte(h.Substring(2, 2), 16), Convert.ToByte(h.Substring(4, 2), 16));

    public override string ToString() => A == 0 ? "clear" : $"#{R:X2}{G:X2}{B:X2}";
}

/// <summary>Project palette: the brief's colours plus a few derived tints/shades.</summary>
public static class P
{
    static Rgba H(string h) => Rgba.Hex(h);

    // Brief palette
    public static readonly Rgba Ink = H("1A2224"), DeepBrown = H("2B2321"), Brown = H("4A3B32"), MidBrown = H("6A5444"),
        Tan = H("A8895E"), Sand = H("C8B58A"), Cream = H("F4EAD2"), Parchment = H("E8D9B5"), Gold = H("F2D26B"),
        Orange = H("D98F4B"), Terracotta = H("C05A4A"), Green = H("6FA86B"), DarkGreen = H("3F6B45"), Blue = H("6B8FA8"),
        DarkBlue = H("3E5A6E"), Teal = H("4B8F8A"), Grey = H("7A7268"), MutedBlue = H("5F6F78"), White = H("FFFFFF");

    // Derived warm tints / shades
    public static readonly Rgba DarkGold = H("B8893A"), LightGold = H("FBE9A8"), DarkTerra = H("8E3F36"), LightBrown = H("8A7058"),
        DarkTeal = H("2F6360"), LightBlue = H("A8C4CF"), PaleSky = H("D6DCCB"), DarkOrange = H("A96A33"), LightOrange = H("E9AE72"),
        DarkParch = H("CDBB92"), SelFill = H("5C4838"), LightTeal = H("8FC1B8"), LightGreen = H("93C48E");

    // Derived muted tones (chapters 3-5)
    public static readonly Rgba DarkGrey = H("4F4A45"), LightGrey = H("A39C8E"), DimOchre = H("8C7A55"), DarkSlate = H("36424A"),
        LightSlate = H("8E9CA3"), Night = H("232C33"), PaleMuted = H("C9C3B3"), DeepSlate = H("2A343B");

    public static readonly Dictionary<char, Rgba> Legend = new()
    {
        ['.'] = Rgba.Clear,
        ['k'] = Ink, ['d'] = DeepBrown, ['b'] = Brown, ['m'] = MidBrown, ['t'] = Tan, ['s'] = Sand, ['c'] = Cream,
        ['p'] = Parchment, ['y'] = Gold, ['Y'] = DarkGold, ['o'] = Orange, ['r'] = Terracotta, ['R'] = DarkTerra,
        ['g'] = Green, ['G'] = DarkGreen, ['u'] = Blue, ['U'] = DarkBlue, ['e'] = Teal, ['E'] = DarkTeal, ['h'] = Grey,
        ['H'] = MutedBlue, ['w'] = White, ['L'] = LightGold, ['i'] = LightGrey, ['j'] = DarkGrey, ['n'] = DimOchre,
        ['N'] = DarkSlate, ['l'] = LightSlate, ['B'] = LightBrown, ['f'] = LightBlue, ['O'] = DarkOrange, ['q'] = LightOrange,
        ['a'] = DarkParch, ['z'] = LightTeal, ['v'] = LightGreen, ['x'] = Night, ['P'] = PaleMuted, ['D'] = DeepSlate, ['S'] = PaleSky, ['F'] = SelFill,
    };
}

/// <summary>RGBA canvas with hard-edged pixel primitives. Continuous coords: pixel (x,y) covers [x,x+1) x [y,y+1).</summary>
public sealed class Canvas
{
    public readonly int W, H;
    readonly Rgba[] px;

    public Canvas(int w, int h)
    {
        W = w; H = h; px = new Rgba[w * h];
    }

    public bool In(int x, int y) => x >= 0 && y >= 0 && x < W && y < H;
    public Rgba Get(int x, int y) => In(x, y) ? px[y * W + x] : Rgba.Clear;
    public bool Opaque(int x, int y) => In(x, y) && px[y * W + x].A != 0;
    public void Set(int x, int y, Rgba c) { if (In(x, y)) px[y * W + x] = c; }
    public IEnumerable<Rgba> Pixels => px;

    public Canvas Clone() { var c = new Canvas(W, H); Array.Copy(px, c.px, px.Length); return c; }

    public void Fill(Rgba c) => Array.Fill(px, c);

    public void Rect(int x, int y, int w, int h, Rgba c)
    {
        for (int j = y; j < y + h; j++) for (int i = x; i < x + w; i++) Set(i, j, c);
    }

    /// <summary>Rect with the four corner pixels removed (cheap rounded corner).</summary>
    public void RectN(int x, int y, int w, int h, Rgba c)
    {
        Rect(x, y, w, h, c);
        Set(x, y, Rgba.Clear); Set(x + w - 1, y, Rgba.Clear); Set(x, y + h - 1, Rgba.Clear); Set(x + w - 1, y + h - 1, Rgba.Clear);
    }

    public void Frame(int x, int y, int w, int h, Rgba c)
    {
        for (int i = x; i < x + w; i++) { Set(i, y, c); Set(i, y + h - 1, c); }
        for (int j = y; j < y + h; j++) { Set(x, j, c); Set(x + w - 1, j, c); }
    }

    public void Line(int x0, int y0, int x1, int y1, Rgba c)
    {
        int dx = Math.Abs(x1 - x0), dy = -Math.Abs(y1 - y0), sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, err = dx + dy;
        while (true)
        {
            Set(x0, y0, c);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    public void Paint(Func<int, int, bool> pred, Rgba c, int x0 = 0, int y0 = 0, int x1 = -1, int y1 = -1)
    {
        if (x1 < 0) x1 = W; if (y1 < 0) y1 = H;
        for (int y = Math.Max(0, y0); y < Math.Min(H, y1); y++)
            for (int x = Math.Max(0, x0); x < Math.Min(W, x1); x++)
                if (pred(x, y)) px[y * W + x] = c;
    }

    public void Ellipse(double cx, double cy, double rx, double ry, Rgba c) =>
        Paint((x, y) => { double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry; return dx * dx + dy * dy <= 1.0; }, c);

    public void Disc(double cx, double cy, double r, Rgba c) => Ellipse(cx, cy, r, r, c);

    public void Ring(double cx, double cy, double rOut, double rIn, Rgba c) =>
        Paint((x, y) => { double dx = x + .5 - cx, dy = y + .5 - cy, d = dx * dx + dy * dy; return d <= rOut * rOut && d > rIn * rIn; }, c);

    public void EllipseRing(double cx, double cy, double rxO, double ryO, double rxI, double ryI, Rgba c) =>
        Paint((x, y) =>
        {
            double ax = (x + .5 - cx), ay = (y + .5 - cy);
            double o = (ax / rxO) * (ax / rxO) + (ay / ryO) * (ay / ryO);
            double i = (ax / rxI) * (ax / rxI) + (ay / ryI) * (ay / ryI);
            return o <= 1 && i > 1;
        }, c);

    public void Poly(IReadOnlyList<(double x, double y)> pts, Rgba c)
    {
        int n = pts.Count;
        double minY = pts.Min(p => p.y), maxY = pts.Max(p => p.y), minX = pts.Min(p => p.x), maxX = pts.Max(p => p.x);
        for (int y = Math.Max(0, (int)Math.Floor(minY)); y <= Math.Min(H - 1, (int)Math.Ceiling(maxY)); y++)
            for (int x = Math.Max(0, (int)Math.Floor(minX)); x <= Math.Min(W - 1, (int)Math.Ceiling(maxX)); x++)
            {
                double qx = x + .5, qy = y + .5; bool inside = false;
                for (int i = 0, j = n - 1; i < n; j = i++)
                {
                    var (xi, yi) = pts[i]; var (xj, yj) = pts[j];
                    if ((yi > qy) != (yj > qy) && qx < (xj - xi) * (qy - yi) / (yj - yi) + xi) inside = !inside;
                }
                if (inside) px[y * W + x] = c;
            }
    }

    public void Tri(double x0, double y0, double x1, double y1, double x2, double y2, Rgba c) =>
        Poly(new[] { (x0, y0), (x1, y1), (x2, y2) }, c);

    public static (double x, double y)[] StarPts(double cx, double cy, double outer, double inner, int points, double rotDeg = -90)
    {
        var l = new (double, double)[points * 2];
        for (int i = 0; i < points * 2; i++)
        {
            double a = (rotDeg + i * 180.0 / points) * Math.PI / 180.0, r = i % 2 == 0 ? outer : inner;
            l[i] = (cx + r * Math.Cos(a), cy + r * Math.Sin(a));
        }
        return l;
    }

    public void Star(double cx, double cy, double outer, double inner, int points, Rgba c, double rotDeg = -90) =>
        Poly(StarPts(cx, cy, outer, inner, points, rotDeg), c);

    /// <summary>Checkerboard dither of colour c over the rect (phase 0/1); the other cells are untouched.</summary>
    public void Dither(int x, int y, int w, int h, Rgba c, int phase = 0)
    {
        for (int j = y; j < y + h; j++) for (int i = x; i < x + w; i++) if (((i + j) & 1) == phase) Set(i, j, c);
    }

    public void DitherDisc(double cx, double cy, double r, Rgba c, int phase = 0) =>
        Paint((x, y) => ((x + y) & 1) == phase && (x + .5 - cx) * (x + .5 - cx) + (y + .5 - cy) * (y + .5 - cy) <= r * r, c);

    /// <summary>Draw an ASCII map using the palette legend ('.' leaves the pixel untouched).</summary>
    public void Map(int x0, int y0, string[] rows, string name = "map")
    {
        int w = rows[0].Length;
        for (int j = 0; j < rows.Length; j++)
        {
            if (rows[j].Length != w) throw new Exception($"{name}: row {j} has length {rows[j].Length}, expected {w}");
            for (int i = 0; i < w; i++)
            {
                char ch = rows[j][i];
                if (ch == '.') continue;
                if (!P.Legend.TryGetValue(ch, out var col)) throw new Exception($"{name}: unknown legend '{ch}' at row {j}");
                Set(x0 + i, y0 + j, col);
            }
        }
    }

    public static Canvas FromMap(int w, int h, string[] rows, string name)
    {
        if (rows.Length != h || rows[0].Length != w) throw new Exception($"{name}: map is {rows[0].Length}x{rows.Length}, expected {w}x{h}");
        var c = new Canvas(w, h); c.Map(0, 0, rows, name); return c;
    }

    public void Replace(Rgba from, Rgba to) { for (int i = 0; i < px.Length; i++) if (px[i] == from) px[i] = to; }

    /// <summary>Adds a 1px outline on transparent pixels touching opaque ones.</summary>
    public void Outline(Rgba c, bool diagonals = false)
    {
        var snap = (Rgba[])px.Clone();
        bool O(int x, int y) => x >= 0 && y >= 0 && x < W && y < H && snap[y * W + x].A != 0;
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                if (snap[y * W + x].A != 0) continue;
                bool hit = O(x - 1, y) || O(x + 1, y) || O(x, y - 1) || O(x, y + 1);
                if (!hit && diagonals) hit = O(x - 1, y - 1) || O(x + 1, y - 1) || O(x - 1, y + 1) || O(x + 1, y + 1);
                if (hit) px[y * W + x] = c;
            }
    }

    public void Blit(Canvas s, int x, int y)
    {
        for (int j = 0; j < s.H; j++) for (int i = 0; i < s.W; i++) { var c = s.Get(i, j); if (c.A != 0) Set(x + i, y + j, c); }
    }

    public Canvas FlipH()
    {
        var c = new Canvas(W, H);
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) c.px[y * W + x] = px[y * W + (W - 1 - x)];
        return c;
    }

    /// <summary>Hard-edged ground shadow: solid core plus a dithered fringe (kept fully opaque / fully clear).</summary>
    public void Shadow(double cx, double cy, double rx, double ry, Rgba c)
    {
        Ellipse(cx, cy, rx, ry, c);
        Paint((x, y) =>
        {
            double dx = (x + .5 - cx) / (rx + 2), dy = (y + .5 - cy) / (ry + 1.2);
            return !Opaque(x, y) && ((x + y) & 1) == 0 && dx * dx + dy * dy <= 1;
        }, c);
    }

    /// <summary>Horizontal sky-style bands with a 1-row checker transition between bands.</summary>
    public void Bands(int x, int y, int w, int[] heights, Rgba[] colours)
    {
        int yy = y;
        for (int b = 0; b < heights.Length; b++)
        {
            Rect(x, yy, w, heights[b], colours[b]);
            if (b > 0) Dither(x, yy, w, 1, colours[b - 1], 0);
            yy += heights[b];
        }
    }
}

public static class Png
{
    static readonly uint[] Table = BuildTable();

    static uint[] BuildTable()
    {
        var t = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            t[n] = c;
        }
        return t;
    }

    static uint Crc(byte[] type, byte[] data)
    {
        uint c = 0xFFFFFFFFu;
        foreach (var b in type) c = Table[(c ^ b) & 0xFF] ^ (c >> 8);
        foreach (var b in data) c = Table[(c ^ b) & 0xFF] ^ (c >> 8);
        return c ^ 0xFFFFFFFFu;
    }

    static void Chunk(Stream s, string type, byte[] data)
    {
        var t = System.Text.Encoding.ASCII.GetBytes(type);
        var be = new byte[4];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(be, (uint)data.Length); s.Write(be);
        s.Write(t); s.Write(data);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(be, Crc(t, data)); s.Write(be);
    }

    public static byte[] Encode(Canvas c)
    {
        using var ms = new MemoryStream();
        ms.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var ihdr = new byte[13];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(0), (uint)c.W);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(4), (uint)c.H);
        ihdr[8] = 8; ihdr[9] = 6; // 8-bit RGBA
        Chunk(ms, "IHDR", ihdr);

        using var raw = new MemoryStream();
        using (var z = new ZLibStream(raw, CompressionLevel.Optimal, leaveOpen: true))
        {
            var row = new byte[1 + c.W * 4];
            for (int y = 0; y < c.H; y++)
            {
                row[0] = 0;
                for (int x = 0; x < c.W; x++)
                {
                    var p = c.Get(x, y);
                    bool clear = p.A == 0;
                    row[1 + x * 4] = clear ? (byte)0 : p.R; row[2 + x * 4] = clear ? (byte)0 : p.G;
                    row[3 + x * 4] = clear ? (byte)0 : p.B; row[4 + x * 4] = clear ? (byte)0 : (byte)255;
                }
                z.Write(row);
            }
        }
        Chunk(ms, "IDAT", raw.ToArray());
        Chunk(ms, "IEND", Array.Empty<byte>());
        return ms.ToArray();
    }
}

public sealed record Sprite(string Group, string Name, Canvas C);

public static class Registry
{
    public static readonly List<Sprite> All = new();

    public static void Add(string group, string name, Canvas c, int w, int h)
    {
        if (c.W != w || c.H != h) throw new Exception($"{name}: is {c.W}x{c.H}, expected {w}x{h}");
        if (All.Any(s => s.Name == name)) throw new Exception($"duplicate sprite {name}");
        All.Add(new Sprite(group, name, c));
    }
}
