using System.IO.Compression;

namespace WorldArtCh3;

public readonly record struct Rgba(byte R, byte G, byte B, byte A = 255)
{
    public static readonly Rgba Clear = new(0, 0, 0, 0);
    public bool IsClear => A == 0;
    public static Rgba Hex(string h) =>
        new(Convert.ToByte(h.Substring(0, 2), 16), Convert.ToByte(h.Substring(2, 2), 16), Convert.ToByte(h.Substring(4, 2), 16));
    public static Rgba Mul(Rgba c, double r, double g, double b) =>
        new((byte)Math.Clamp(c.R * r, 0, 255), (byte)Math.Clamp(c.G * g, 0, 255), (byte)Math.Clamp(c.B * b, 0, 255));
    /// <summary>Hue-shifted shadow: darker, cooler purple-ish (never black).</summary>
    public Rgba Shade(double k = 1.0)
    {
        double f = 1 - 0.30 * k;
        return new((byte)Math.Clamp(R * f * 0.98 + 6 * k, 0, 255), (byte)Math.Clamp(G * (1 - 0.34 * k) + 2 * k, 0, 255), (byte)Math.Clamp(B * (1 - 0.22 * k) + 12 * k, 0, 255));
    }
    /// <summary>Warm tint towards sunlight.</summary>
    public Rgba Light(double k = 1.0) =>
        new((byte)Math.Clamp(R + 18 * k, 0, 255), (byte)Math.Clamp(G + 14 * k, 0, 255), (byte)Math.Clamp(B + 4 * k, 0, 255));
    public override string ToString() => A == 0 ? "clear" : $"#{R:X2}{G:X2}{B:X2}";
}

public static class Fx
{
    public static uint H(int x, int y, int s)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + s * 1274126177);
            h = (h ^ (h >> 13)) * 1274126177u;
            return h ^ (h >> 16);
        }
    }
    public static double R(int x, int y, int s) => (H(x, y, s) & 0xFFFFFF) / 16777216.0;
    public static double VN(double x, double y, int s)
    {
        int xi = (int)Math.Floor(x), yi = (int)Math.Floor(y);
        double fx = x - xi, fy = y - yi;
        fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
        double a = R(xi, yi, s), b = R(xi + 1, yi, s), c = R(xi, yi + 1, s), d = R(xi + 1, yi + 1, s);
        return (a + (b - a) * fx) + ((c + (d - c) * fx) - (a + (b - a) * fx)) * fy;
    }
    public static double Fbm(double x, double y, int s) => VN(x, y, s) * 0.6 + VN(x * 2.1, y * 2.1, s + 7) * 0.28 + VN(x * 4.3, y * 4.3, s + 13) * 0.12;
    static readonly int[,] Bayer = { { 0, 2 }, { 3, 1 } };
    public static double Bay(int x, int y) => (Bayer[y & 1, x & 1] + 0.5) / 4.0 - 0.5;
}

public sealed class Canvas
{
    public readonly int W, H;
    readonly Rgba[] px;
    public Canvas(int w, int h) { W = w; H = h; px = new Rgba[w * h]; }
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
    public void HL(int x, int y, int w, Rgba c) => Rect(x, y, w, 1, c);
    public void VL(int x, int y, int h, Rgba c) => Rect(x, y, 1, h, c);
    public void RectN(int x, int y, int w, int h, Rgba c)
    {
        Rect(x, y, w, h, c);
        Set(x, y, Rgba.Clear); Set(x + w - 1, y, Rgba.Clear); Set(x, y + h - 1, Rgba.Clear); Set(x + w - 1, y + h - 1, Rgba.Clear);
    }
    /// <summary>Rect with highlight on top/left and shade on bottom/right.</summary>
    public void Bevel(int x, int y, int w, int h, Rgba fill, Rgba hi, Rgba lo)
    {
        Rect(x, y, w, h, fill);
        HL(x, y, w, hi); VL(x, y, h, hi);
        HL(x, y + h - 1, w, lo); VL(x + w - 1, y, h, lo);
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
        Paint((x, y) => { double dx = (x + .5 - cx) / rx, dy = (y + .5 - cy) / ry; return dx * dx + dy * dy <= 1.0; },
            c, (int)(cx - rx - 1), (int)(cy - ry - 1), (int)(cx + rx + 2), (int)(cy + ry + 2));
    public void Disc(double cx, double cy, double r, Rgba c) => Ellipse(cx, cy, r, r, c);
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
    public void Tri(double x0, double y0, double x1, double y1, double x2, double y2, Rgba c) => Poly(new[] { (x0, y0), (x1, y1), (x2, y2) }, c);
    public void Dither(int x, int y, int w, int h, Rgba c, int phase = 0)
    {
        for (int j = y; j < y + h; j++) for (int i = x; i < x + w; i++) if (((i + j) & 1) == phase) Set(i, j, c);
    }
    public void Blit(Canvas s, int x, int y)
    {
        for (int j = 0; j < s.H; j++) for (int i = 0; i < s.W; i++) { var c = s.Get(i, j); if (c.A != 0) Over(x + i, y + j, c); }
    }
    /// <summary>Alpha-composite c over the pixel (plain overwrite when c is opaque).</summary>
    public void Over(int x, int y, Rgba c)
    {
        if (!In(x, y) || c.A == 0) return;
        if (c.A == 255) { px[y * W + x] = c; return; }
        var d = px[y * W + x];
        if (d.A == 0) { px[y * W + x] = c; return; }
        double a = c.A / 255.0;
        px[y * W + x] = new Rgba((byte)(c.R * a + d.R * (1 - a)), (byte)(c.G * a + d.G * (1 - a)), (byte)(c.B * a + d.B * (1 - a)), Math.Max(d.A, c.A));
    }
    public Canvas FlipH()
    {
        var c = new Canvas(W, H);
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) c.px[y * W + x] = px[y * W + (W - 1 - x)];
        return c;
    }
    public Canvas Crop(int x, int y, int w, int h)
    {
        var c = new Canvas(w, h);
        for (int j = 0; j < h; j++) for (int i = 0; i < w; i++) c.px[j * w + i] = Get(x + i, y + j);
        return c;
    }
    public Canvas Scale(int k)
    {
        var o = new Canvas(W * k, H * k);
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) { var p = Get(x, y); if (!p.IsClear) o.Rect(x * k, y * k, k, k, p); }
        return o;
    }
    public (int x, int y, int w, int h)? Bounds()
    {
        int x0 = W, y0 = H, x1 = -1, y1 = -1;
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) if (px[y * W + x].A != 0) { x0 = Math.Min(x0, x); y0 = Math.Min(y0, y); x1 = Math.Max(x1, x); y1 = Math.Max(y1, y); }
        return x1 < 0 ? null : (x0, y0, x1 - x0 + 1, y1 - y0 + 1);
    }
    public Canvas Trim()
    {
        var b = Bounds(); if (b is null) return this;
        return Crop(b.Value.x, b.Value.y, b.Value.w, b.Value.h);
    }
    /// <summary>Selective outline: each outline pixel is a dark, warm version of its brightest-neighbour hue.</summary>
    public void SelOutline(double k = 0.38, bool diagonals = false)
    {
        var snap = (Rgba[])px.Clone();
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                if (snap[y * W + x].A != 0) continue;
                int n = 0, r = 0, g = 0, b = 0;
                void Add(int xx, int yy)
                {
                    if (xx < 0 || yy < 0 || xx >= W || yy >= H) return;
                    var p = snap[yy * W + xx]; if (p.A == 0) return;
                    n++; r += p.R; g += p.G; b += p.B;
                }
                Add(x - 1, y); Add(x + 1, y); Add(x, y - 1); Add(x, y + 1);
                if (n == 0 && diagonals) { Add(x - 1, y - 1); Add(x + 1, y - 1); Add(x - 1, y + 1); Add(x + 1, y + 1); }
                if (n == 0) continue;
                px[y * W + x] = new Rgba((byte)Math.Clamp(r / n * k + 22, 0, 255), (byte)Math.Clamp(g / n * k * 0.92 + 14, 0, 255), (byte)Math.Clamp(b / n * k * 0.95 + 16, 0, 255));
            }
    }
    public void Outline(Rgba c)
    {
        var snap = (Rgba[])px.Clone();
        bool O(int x, int y) => x >= 0 && y >= 0 && x < W && y < H && snap[y * W + x].A != 0;
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                if (snap[y * W + x].A == 0 && (O(x - 1, y) || O(x + 1, y) || O(x, y - 1) || O(x, y + 1))) px[y * W + x] = c;
    }
    public void Replace(Rgba from, Rgba to) { for (int i = 0; i < px.Length; i++) if (px[i] == from) px[i] = to; }
    /// <summary>Make width even by padding one transparent column on the right.</summary>
    public Canvas EvenW()
    {
        if (W % 2 == 0) return this;
        var c = new Canvas(W + 1, H); c.Blit(this, 0, 0); return c;
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
        ihdr[8] = 8; ihdr[9] = 6;
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
                    row[3 + x * 4] = clear ? (byte)0 : p.B; row[4 + x * 4] = clear ? (byte)0 : p.A;
                }
                z.Write(row);
            }
        }
        Chunk(ms, "IDAT", raw.ToArray());
        Chunk(ms, "IEND", Array.Empty<byte>());
        return ms.ToArray();
    }

    /// <summary>Minimal decoder (8-bit, non-interlaced, colour types 2/3/6) for placeholder-replacement in previews.</summary>
    public static Canvas? Decode(byte[] data)
    {
        try
        {
            int pos = 8, w = 0, h = 0, ct = 0, depth = 0, interlace = 0;
            var idat = new MemoryStream(); byte[]? plte = null; byte[]? trns = null;
            while (pos < data.Length)
            {
                int len = (int)System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(pos)); pos += 4;
                string type = System.Text.Encoding.ASCII.GetString(data, pos, 4); pos += 4;
                var body = data.AsSpan(pos, len);
                if (type == "IHDR") { w = (int)System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(body); h = (int)System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(body[4..]); depth = body[8]; ct = body[9]; interlace = body[12]; }
                else if (type == "IDAT") idat.Write(body);
                else if (type == "PLTE") plte = body.ToArray();
                else if (type == "tRNS") trns = body.ToArray();
                pos += len + 4;
            }
            if (depth != 8 || interlace != 0) return null;
            int bpp = ct switch { 6 => 4, 2 => 3, 3 => 1, 0 => 1, 4 => 2, _ => 0 };
            if (bpp == 0) return null;
            idat.Position = 0;
            using var z = new ZLibStream(idat, CompressionMode.Decompress);
            var raw = new MemoryStream(); z.CopyTo(raw); var rb = raw.ToArray();
            int stride = w * bpp; var cur = new byte[stride]; var prev = new byte[stride];
            var cv = new Canvas(w, h);
            for (int y = 0; y < h; y++)
            {
                int ft = rb[y * (stride + 1)];
                Array.Copy(rb, y * (stride + 1) + 1, cur, 0, stride);
                for (int i = 0; i < stride; i++)
                {
                    int a = i >= bpp ? cur[i - bpp] : 0, b = prev[i], c = i >= bpp ? prev[i - bpp] : 0, v = cur[i];
                    switch (ft)
                    {
                        case 1: v += a; break;
                        case 2: v += b; break;
                        case 3: v += (a + b) / 2; break;
                        case 4: { int p = a + b - c, pa = Math.Abs(p - a), pb = Math.Abs(p - b), pc = Math.Abs(p - c); v += pa <= pb && pa <= pc ? a : pb <= pc ? b : c; break; }
                    }
                    cur[i] = (byte)v;
                }
                for (int x = 0; x < w; x++)
                {
                    Rgba p = ct switch
                    {
                        6 => new Rgba(cur[x * 4], cur[x * 4 + 1], cur[x * 4 + 2], cur[x * 4 + 3]),
                        2 => new Rgba(cur[x * 3], cur[x * 3 + 1], cur[x * 3 + 2]),
                        3 => new Rgba(plte![cur[x] * 3], plte[cur[x] * 3 + 1], plte[cur[x] * 3 + 2], trns != null && cur[x] < trns.Length ? trns[cur[x]] : (byte)255),
                        0 => new Rgba(cur[x], cur[x], cur[x]),
                        _ => new Rgba(cur[x * 2], cur[x * 2], cur[x * 2], cur[x * 2 + 1]),
                    };
                    cv.Set(x, y, p.A < 128 ? Rgba.Clear : new Rgba(p.R, p.G, p.B));
                }
                (prev, cur) = (cur, prev);
            }
            return cv;
        }
        catch { return null; }
    }
}
