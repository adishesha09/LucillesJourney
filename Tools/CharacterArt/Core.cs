using System.IO.Compression;

namespace CharacterArt;

public readonly record struct Rgba(byte R, byte G, byte B, byte A = 255)
{
    public static readonly Rgba Clear = new(0, 0, 0, 0);
    public bool IsClear => A == 0;

    public static Rgba Hex(string h) =>
        new(Convert.ToByte(h.Substring(0, 2), 16), Convert.ToByte(h.Substring(2, 2), 16), Convert.ToByte(h.Substring(4, 2), 16));

    public double Luma => 0.299 * R + 0.587 * G + 0.114 * B;
}

/// <summary>RGBA canvas (hard-edged pixels only).</summary>
public sealed class Canvas
{
    public readonly int W, H;
    readonly Rgba[] px;

    public Canvas(int w, int h) { W = w; H = h; px = new Rgba[w * h]; }

    public bool In(int x, int y) => x >= 0 && y >= 0 && x < W && y < H;
    public Rgba Get(int x, int y) => In(x, y) ? px[y * W + x] : Rgba.Clear;
    public bool Opaque(int x, int y) => In(x, y) && px[y * W + x].A != 0;
    public void Set(int x, int y, Rgba c) { if (In(x, y)) px[y * W + x] = c; }
    public void Fill(Rgba c) => Array.Fill(px, c);

    public void Rect(int x, int y, int w, int h, Rgba c)
    {
        for (int j = y; j < y + h; j++) for (int i = x; i < x + w; i++) Set(i, j, c);
    }

    public void Blit(Canvas s, int x, int y)
    {
        for (int j = 0; j < s.H; j++) for (int i = 0; i < s.W; i++) { var c = s.Get(i, j); if (c.A != 0) Set(x + i, y + j, c); }
    }

    public Canvas Scaled(int k)
    {
        var o = new Canvas(W * k, H * k);
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) { var p = Get(x, y); if (!p.IsClear) o.Rect(x * k, y * k, k, k, p); }
        return o;
    }

    /// <summary>Tight bounding box of opaque pixels (x0,y0,x1,y1 inclusive) or null if empty.</summary>
    public (int x0, int y0, int x1, int y1)? Bounds()
    {
        int x0 = W, y0 = H, x1 = -1, y1 = -1;
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
                if (Opaque(x, y)) { x0 = Math.Min(x0, x); y0 = Math.Min(y0, y); x1 = Math.Max(x1, x); y1 = Math.Max(y1, y); }
        return x1 < 0 ? null : (x0, y0, x1, y1);
    }

    public bool HardAlphaOnly() { for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) { var a = px[y * W + x].A; if (a != 0 && a != 255) return false; } return true; }
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
