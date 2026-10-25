namespace WorldArtCh4;

/// <summary>Contact sheets of the registered sprites (debug aid, written to out_preview).</summary>
public static class Sheet
{
    public static void Write(string dir, int scale = 3, int maxW = 1500)
    {
        foreach (var grp in S.Group.Values.Distinct())
        {
            var names = S.Group.Where(kv => kv.Value == grp).Select(kv => kv.Key).ToList();
            int x = 4, y = 4, rowH = 0;
            var pos = new List<(string n, int x, int y)>();
            int totalH = 0;
            foreach (var n in names)
            {
                var c = S.Get(n);
                int w = c.W * scale + 8, h = c.H * scale + 8;
                if (x + w > maxW) { x = 4; y += rowH; rowH = 0; }
                pos.Add((n, x, y)); x += w; rowH = Math.Max(rowH, h);
                totalH = y + rowH;
            }
            var sheet = new Canvas(maxW, totalH + 8);
            sheet.Fill(Rgba.Hex("7C9A74"));
            foreach (var (n, px, py) in pos)
            {
                var c = S.Get(n);
                sheet.Blit(c.Scale(scale), px, py);
            }
            File.WriteAllBytes(Path.Combine(dir, $"sheet_{grp}.png"), Png.Encode(sheet));
        }
    }
}
