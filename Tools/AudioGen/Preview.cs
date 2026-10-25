using System.IO.Compression;

namespace AudioGen;

static class Preview
{
    const int W = 900, EnvH = 56, SpecH = 64, SeamH = 34, Gap = 6;

    sealed class Img
    {
        public readonly int W, H;
        public readonly byte[] Px;
        public Img(int w, int h) { W = w; H = h; Px = new byte[w * h * 3]; Fill(0, 0, w, h, 16, 16, 20); }
        public void Set(int x, int y, byte r, byte g, byte b)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return;
            int i = (y * W + x) * 3;
            Px[i] = r; Px[i + 1] = g; Px[i + 2] = b;
        }
        public void Fill(int x0, int y0, int w, int h, byte r, byte g, byte b)
        {
            for (int y = y0; y < y0 + h; y++) for (int x = x0; x < x0 + w; x++) Set(x, y, r, g, b);
        }
        public void VLine(int x, int y0, int y1, byte r, byte g, byte b)
        {
            if (y0 > y1) (y0, y1) = (y1, y0);
            for (int y = y0; y <= y1; y++) Set(x, y, r, g, b);
        }
    }

    public static void RenderGroups(List<FileInfoRow> rows, string outDir)
    {
        Directory.CreateDirectory(outDir);
        var groups = new (string file, Func<FileInfoRow, bool> pick)[]
        {
            ("preview_music.png", r => r.Kind == "music"),
            ("preview_amb.png", r => r.Kind == "amb"),
            ("preview_sfx.png", r => r.Name.StartsWith("sfx_")),
            ("preview_ui.png", r => r.Name.StartsWith("ui_")),
        };
        foreach (var (file, pick) in groups)
        {
            var sel = rows.Where(pick).ToList();
            if (sel.Count == 0) continue;
            int panelH = EnvH + SpecH + Gap;
            int total = sel.Sum(r => panelH + (r.Kind != "sfx" ? SeamH : 0) + Gap);
            var img = new Img(W, total);
            int y = 0;
            Console.WriteLine($"  {file}: " + string.Join(", ", sel.Select(r => r.Name)));
            foreach (var r in sel)
            {
                var (d, sr, _, _) = Wav.Read(r.Path);
                var x = new float[d.Length];
                for (int i = 0; i < x.Length; i++) x[i] = d[i] / 32768f;
                DrawEnvelope(img, x, y);
                DrawSpectrogram(img, x, sr, y + EnvH + 2);
                y += EnvH + SpecH + Gap;
                if (r.Kind != "sfx") { DrawSeam(img, x, sr, y); y += SeamH + Gap; }
                y += Gap;
                img.Fill(0, y - Gap, W, 1, 70, 70, 90);
            }
            Png.Save(Path.Combine(outDir, file), img.W, img.H, img.Px);
        }
    }

    static void DrawEnvelope(Img img, float[] x, int y0)
    {
        img.Fill(0, y0, W, EnvH, 24, 24, 30);
        int mid = y0 + EnvH / 2;
        img.Fill(0, mid, W, 1, 50, 50, 60);
        int dbLine = (int)(Math.Sqrt(0.708) * (EnvH / 2 - 1));
        for (int xx = 0; xx < W; xx += 4) { img.Set(xx, mid - dbLine, 90, 60, 60); img.Set(xx, mid + dbLine, 90, 60, 60); }
        for (int c = 0; c < W; c++)
        {
            int a = (int)((long)c * x.Length / W), b = Math.Max(a + 1, (int)((long)(c + 1) * x.Length / W));
            float mn = 0, mx = 0;
            for (int i = a; i < b && i < x.Length; i++) { if (x[i] < mn) mn = x[i]; if (x[i] > mx) mx = x[i]; }
            double h = EnvH / 2 - 1;
            int ya = mid - (int)(Math.Sqrt(mx) * h), yb = mid + (int)(Math.Sqrt(-mn) * h);
            bool clip = mx >= 0.99 || -mn >= 0.99;
            img.VLine(c, ya, yb, clip ? (byte)255 : (byte)210, clip ? (byte)40 : (byte)220, clip ? (byte)40 : (byte)230);
        }
    }

    static void DrawSpectrogram(Img img, float[] x, int sr, int y0)
    {
        const int N = 1024;
        var win = new double[N];
        for (int i = 0; i < N; i++) win[i] = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / N);
        var re = new double[N];
        var im = new double[N];
        double maxHz = 8000;
        for (int c = 0; c < W; c++)
        {
            long center = (long)((c + 0.5) * x.Length / W);
            for (int i = 0; i < N; i++)
            {
                long idx = center - N / 2 + i;
                re[i] = idx >= 0 && idx < x.Length ? x[idx] * win[i] : 0;
                im[i] = 0;
            }
            Fft(re, im);
            for (int row = 0; row < SpecH; row++)
            {
                double hz = maxHz * (SpecH - 1 - row) / (SpecH - 1);
                int bin = Math.Min(N / 2 - 1, (int)(hz / sr * N));
                double mag = Math.Sqrt(re[bin] * re[bin] + im[bin] * im[bin]) / (N * 0.25);
                double db = 20 * Math.Log10(Math.Max(mag, 1e-9));
                double t = Math.Clamp((db + 100) / 80.0, 0, 1);
                var (r, g, b) = Color(t);
                img.Set(c, y0 + row, r, g, b);
            }
        }
    }

    static void DrawSeam(Img img, float[] x, int sr, int y0)
    {
        img.Fill(0, y0, W, SeamH, 24, 24, 30);
        int w = sr / 10;
        var s = new float[2 * w];
        for (int i = 0; i < w; i++) { s[i] = x[x.Length - w + i]; s[w + i] = x[i]; }
        float pk = Math.Max(Dsp.Peak(s), 1e-6f);
        int mid = y0 + SeamH / 2;
        img.Fill(0, mid, W, 1, 50, 50, 60);
        int prevY = mid;
        for (int i = 0; i < s.Length; i++)
        {
            int xx = (int)((long)i * W / s.Length);
            int yy = mid - (int)(s[i] / pk * (SeamH / 2 - 2));
            if (i == 0) prevY = yy;
            img.VLine(xx, prevY, yy, 120, 220, 140);
            prevY = yy;
        }
        img.VLine(W / 2, y0, y0 + SeamH - 1, 220, 60, 60);
    }

    static (byte, byte, byte) Color(double t)
    {
        double[][] stops = { new[] { 0.0, 0, 0, 0 }, new[] { 0.33, 60, 20, 110 }, new[] { 0.66, 220, 80, 60 }, new[] { 1.0, 255, 240, 160 } };
        for (int i = 1; i < stops.Length; i++)
        {
            if (t <= stops[i][0])
            {
                double u = (t - stops[i - 1][0]) / (stops[i][0] - stops[i - 1][0]);
                return ((byte)(stops[i - 1][1] + (stops[i][1] - stops[i - 1][1]) * u),
                        (byte)(stops[i - 1][2] + (stops[i][2] - stops[i - 1][2]) * u),
                        (byte)(stops[i - 1][3] + (stops[i][3] - stops[i - 1][3]) * u));
            }
        }
        return (255, 240, 160);
    }

    static void Fft(double[] re, double[] im)
    {
        int n = re.Length;
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) { (re[i], re[j]) = (re[j], re[i]); (im[i], im[j]) = (im[j], im[i]); }
        }
        for (int len = 2; len <= n; len <<= 1)
        {
            double ang = -2 * Math.PI / len;
            double wr = Math.Cos(ang), wi = Math.Sin(ang);
            for (int i = 0; i < n; i += len)
            {
                double cr = 1, ci = 0;
                for (int k = 0; k < len / 2; k++)
                {
                    int a = i + k, b = i + k + len / 2;
                    double tr = re[b] * cr - im[b] * ci, ti = re[b] * ci + im[b] * cr;
                    re[b] = re[a] - tr; im[b] = im[a] - ti;
                    re[a] += tr; im[a] += ti;
                    double ncr = cr * wr - ci * wi; ci = cr * wi + ci * wr; cr = ncr;
                }
            }
        }
    }
}

static class Png
{
    static readonly uint[] Table = MakeTable();

    static uint[] MakeTable()
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
        var tb = System.Text.Encoding.ASCII.GetBytes(type);
        var len = BitConverter.GetBytes((uint)data.Length); Array.Reverse(len);
        s.Write(len);
        s.Write(tb);
        s.Write(data);
        var crc = BitConverter.GetBytes(Crc(tb, data)); Array.Reverse(crc);
        s.Write(crc);
    }

    public static void Save(string path, int w, int h, byte[] rgb)
    {
        using var fs = File.Create(path);
        fs.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var ihdr = new byte[13];
        var wb = BitConverter.GetBytes((uint)w); Array.Reverse(wb);
        var hb = BitConverter.GetBytes((uint)h); Array.Reverse(hb);
        Array.Copy(wb, 0, ihdr, 0, 4); Array.Copy(hb, 0, ihdr, 4, 4);
        ihdr[8] = 8; ihdr[9] = 2;
        Chunk(fs, "IHDR", ihdr);
        var raw = new byte[h * (w * 3 + 1)];
        for (int y = 0; y < h; y++)
        {
            raw[y * (w * 3 + 1)] = 0;
            Buffer.BlockCopy(rgb, y * w * 3, raw, y * (w * 3 + 1) + 1, w * 3);
        }
        using var ms = new MemoryStream();
        using (var z = new ZLibStream(ms, CompressionLevel.Optimal, true)) z.Write(raw);
        Chunk(fs, "IDAT", ms.ToArray());
        Chunk(fs, "IEND", Array.Empty<byte>());
    }
}
