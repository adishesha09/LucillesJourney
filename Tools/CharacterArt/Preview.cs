namespace CharacterArt;

/// <summary>Hard-edged contact sheets (8x nearest-neighbour on mid grey with 1px grid) for visual QA.</summary>
public static class Preview
{
    static readonly Rgba Bg = new(0x80, 0x80, 0x80), Grid = new(0x76, 0x76, 0x76), Bounds = new(0x9A, 0x9A, 0x9A), Axis = new(0xB8, 0x6A, 0x6A);

    /// <summary>Rows of sprites, each aligned bottom-centre inside a uniform cell (the pivot), with canvas bounds outlined.</summary>
    public static Canvas Sheet(List<List<Canvas>> rows, int scale = 8, int pad = 2, bool bounds = true, bool gridLines = true)
    {
        int cw = rows.SelectMany(r => r).Max(c => c.W) + pad * 2, ch = rows.SelectMany(r => r).Max(c => c.H) + pad * 2;
        int cols = rows.Max(r => r.Count);
        var o = new Canvas(cols * cw * scale, rows.Count * ch * scale);
        o.Fill(Bg);
        if (gridLines)
        {
            for (int x = 0; x < o.W; x += scale) o.Rect(x, 0, 1, o.H, Grid);
            for (int y = 0; y < o.H; y += scale) o.Rect(0, y, o.W, 1, Grid);
        }
        for (int r = 0; r < rows.Count; r++)
            for (int c = 0; c < rows[r].Count; c++)
            {
                var spr = rows[r][c];
                int px = c * cw + (cw - spr.W) / 2, py = r * ch + ch - pad - spr.H;
                if (bounds) Frame(o, px * scale - 1, py * scale - 1, spr.W * scale + 2, spr.H * scale + 2, Bounds);
                // pivot marker: bottom-centre
                o.Rect((c * cw + cw / 2) * scale - 1, (py + spr.H) * scale + 1, 2, 3, Axis);
                o.Blit(spr.Scaled(scale), px * scale, py * scale);
            }
        return o;
    }

    static void Frame(Canvas c, int x, int y, int w, int h, Rgba col)
    {
        c.Rect(x, y, w, 1, col); c.Rect(x, y + h - 1, w, 1, col); c.Rect(x, y, 1, h, col); c.Rect(x + w - 1, y, 1, h, col);
    }

    /// <summary>Frames side by side with no gaps on a common baseline (flip-book strip).</summary>
    public static Canvas Strip(List<List<Canvas>> rows, int scale = 8)
    {
        int w = rows.Max(r => r.Sum(c => c.W + 1)) + 1, h = rows.Sum(r => r.Max(c => c.H) + 2) + 1;
        var o = new Canvas(w * scale, h * scale);
        o.Fill(Bg);
        int y = 1;
        foreach (var r in rows)
        {
            int x = 1, rh = r.Max(c => c.H);
            foreach (var c in r) { o.Blit(c.Scaled(scale), x * scale, (y + rh - c.H) * scale); x += c.W + 1; }
            o.Rect(0, (y + rh) * scale, o.W, 1, Axis); // ground line
            y += rh + 2;
        }
        return o;
    }

    /// <summary>Sprites at ~4x on a grass-green ground, the way they appear in game.</summary>
    public static Canvas InGame(List<List<Canvas>> rows, int scale = 4, int gap = 6)
    {
        int cw = rows.SelectMany(r => r).Max(c => c.W) + gap, ch = rows.SelectMany(r => r).Max(c => c.H) + gap;
        var o = new Canvas(rows.Max(r => r.Count) * cw * scale + gap * scale, rows.Count * ch * scale + gap * scale);
        o.Fill(Rgba.Hex("7FB06F"));
        var dark = Rgba.Hex("73A464");
        for (int y = 0; y < o.H; y += 16) for (int x = (y / 16 % 2) * 16; x < o.W; x += 32) o.Rect(x, y, 16, 16, dark);
        for (int r = 0; r < rows.Count; r++)
            for (int c = 0; c < rows[r].Count; c++)
            {
                var spr = rows[r][c];
                o.Blit(spr.Scaled(scale), (gap + c * cw + (cw - gap - spr.W) / 2) * scale, (gap + r * ch + ch - gap - spr.H) * scale);
            }
        return o;
    }
}
