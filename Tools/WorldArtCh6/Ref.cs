namespace WorldArtCh6;

/// <summary>Debug aid: contact sheet of existing sprites at high zoom.</summary>
public static class Ref
{
    public static void Run(string[] names, int scale, string file)
    {
        int x = 6, y = 6, rowH = 0, maxW = 1700;
        var pos = new List<(Canvas c, int x, int y)>();
        foreach (var n in names)
        {
            var c = S.Ext(n);
            int w = c.W * scale + 10, h = c.H * scale + 10;
            if (x + w > maxW) { x = 6; y += rowH; rowH = 0; }
            pos.Add((c, x, y)); x += w; rowH = Math.Max(rowH, h);
        }
        var sheet = new Canvas(maxW, y + rowH + 6);
        sheet.Fill(Rgba.Hex("6C7A8A"));
        foreach (var (c, px, py) in pos) sheet.Blit(c.Scale(scale), px, py);
        File.WriteAllBytes(file, Png.Encode(sheet));
    }
}
