namespace PixelArt;

public static class Preview
{
    static readonly Rgba Bg = new(0x80, 0x80, 0x80);

    public static readonly Dictionary<string, int> Borders = new();

    static Canvas Scaled(Canvas c, int k)
    {
        var o = new Canvas(c.W * k, c.H * k);
        for (int y = 0; y < c.H; y++) for (int x = 0; x < c.W; x++) { var p = c.Get(x, y); if (!p.IsClear) o.Rect(x * k, y * k, k, k, p); }
        return o;
    }

    /// <summary>Stretch a 9-slice sprite to a new size (corners fixed, edges and centre stretched nearest-neighbour).</summary>
    public static Canvas Slice(Canvas s, int b, int nw, int nh)
    {
        var o = new Canvas(nw, nh);
        int Map(int v, int n, int sn) => v < b ? v : v >= n - b ? sn - (n - v) : b + (int)((v - b) * (double)(sn - 2 * b) / (n - 2 * b));
        for (int y = 0; y < nh; y++) for (int x = 0; x < nw; x++) o.Set(x, y, s.Get(Map(x, nw, s.W), Map(y, nh, s.H)));
        return o;
    }

    static void Sheet(IReadOnlyList<Canvas> items, int k, int cols, string path, int pad = 8)
    {
        int cw = items.Max(i => i.W * k) + pad, ch = items.Max(i => i.H * k) + pad;
        int rows = (items.Count + cols - 1) / cols;
        var sheet = new Canvas(cols * cw + pad, rows * ch + pad);
        sheet.Fill(Bg);
        for (int n = 0; n < items.Count; n++)
        {
            int cx = pad + (n % cols) * cw, cy = pad + (n / cols) * ch;
            sheet.Blit(Scaled(items[n], k), cx, cy);
        }
        File.WriteAllBytes(path, Png.Encode(sheet));
    }

    public static void Zoom(List<Sprite> sprites, int k, string path) =>
        Sheet(sprites.Select(s => s.C).ToList(), k, Math.Min(sprites.Count, Math.Max(1, 1400 / (sprites.Max(s => s.C.W) * k + 8))), path);

    public static void BuildAll(string dir)
    {
        foreach (var f in Directory.GetFiles(dir, "*.png")) File.Delete(f);
        foreach (var g in Registry.All.GroupBy(s => s.Group))
        {
            var list = g.ToList();
            int w = list[0].C.W;
            int k = w <= 32 ? 6 : 4;
            int cols = Math.Max(1, 1500 / (list.Max(s => s.C.W) * k + 8));
            for (int part = 0; part * 24 < list.Count; part++)
            {
                var chunk = list.Skip(part * 24).Take(24).ToList();
                Sheet(chunk.Select(s => s.C).ToList(), k, Math.Min(cols, 6), Path.Combine(dir, $"{g.Key}_{part + 1}.png"));
            }
        }

        // 9-slice check: each frame stretched to 80x36 and 36x60 (x4)
        var frames = Registry.All.Where(s => Borders.ContainsKey(s.Name)).ToList();
        var stretched = new List<Canvas>();
        foreach (var f in frames) { stretched.Add(Slice(f.C, Borders[f.Name], 72, 32)); }
        Sheet(stretched, 4, 4, Path.Combine(dir, "A_frames_stretched.png"));
    }
}
