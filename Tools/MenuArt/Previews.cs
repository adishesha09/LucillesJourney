namespace MenuArt;

public static class Previews
{
    public static void Over(Canvas d, Canvas s, int ox, int oy)
    {
        for (int y = 0; y < s.H; y++) for (int x = 0; x < s.W; x++) { var p = s.Get(x, y); if (p.A != 0) d.Set(x + ox, y + oy, p); }
    }

    public static void Screen(Canvas d, Canvas s, int ox, int oy)
    {
        for (int y = 0; y < s.H; y++)
            for (int x = 0; x < s.W; x++)
            {
                var p = s.Get(x, y); if (p.A == 0 || !d.In(x + ox, y + oy)) continue;
                var q = d.Get(x + ox, y + oy);
                byte S(byte a, byte b) => (byte)(255 - (255 - a) * (255 - b) / 255);
                d.Set(x + ox, y + oy, new Rgba(S(q.R, p.R), S(q.G, p.G), S(q.B, p.B)));
            }
    }

    public static void Save(string name, Canvas c, int scale)
    {
        File.WriteAllBytes(Path.Combine(Program.PreviewDir, name), Png.Encode(scale == 1 ? c : c.Scale(scale)));
    }

    public static void Seam(string name, Canvas clouds)
    {
        var c = new Canvas(clouds.W + 200, clouds.H); c.Fill(new Rgba(80, 100, 130));
        Over(c, clouds, 0, 0);
        // second copy appended; the join sits at x = clouds.W
        for (int y = 0; y < clouds.H; y++) for (int x = 0; x < 200; x++) { var p = clouds.Get(x, y); if (p.A != 0) c.Set(clouds.W + x, y, p); }
        Save(name, c.Crop(clouds.W - 200, 0, 400, clouds.H), 3);
    }

    public static void Ending(EndingSet m, int shift, string name)
    {
        var c = new Canvas(384, 216);
        Over(c, m.Sky, 0, 0);
        Over(c, m.Clouds, 0, 6);
        Over(c, m.Sun, EndingSky.SunStartX - 32, EndingSky.SunStartY - 32);
        Screen(c, m.Glow, 0, 0);
        Over(c, m.Far, -shift, 0); Over(c, m.Mid, -shift, 0); Over(c, m.Road, -shift, 0);
        Over(c, m.Fig0, m.FigX - shift - 22, m.FigY - 28);
        Over(c, m.Bird0, 120, 62); Over(c, m.Bird1, 144, 56); Over(c, m.Bird0, 170, 50);
        var rnd = new Random(5);
        for (int i = 0; i < 34; i++)
        {
            int x = 50 + rnd.Next(290), y = 44 + rnd.Next(120);
            Over(c, i % 4 == 0 ? m.Dust3 : m.Dust2, x, y);
        }
        Over(c, m.Vig, 0, 0);
        Save(name, c, 3);
    }

    public static void Crop(string name, Canvas c, int scale) => Save(name, c.Crop(0, 100, c.W, 116), scale);

    /// <param name="shift">x at which the 448px layers are placed is -shift (32 = centred rest position, 64 = slid fully left)</param>
    public static void Menu(MenuSet m, int shift, string name)
    {
        var c = new Canvas(384, 216);
        Over(c, m.Sky, 0, 0); Over(c, m.Stars0, 0, 0); Screen(c, m.Glow, 0, 0); Over(c, m.Clouds, 0, 26);
        Over(c, m.Far, -shift, 0); Over(c, m.Mid, -shift, 0); Over(c, m.Ground, -shift, 0); Screen(c, m.WGlow, -shift, 0);
        // fireflies scattered over the garden
        var rnd = new Random(9);
        for (int i = 0; i < 16; i++)
        {
            int fx = 20 + rnd.Next(380) - shift, fy = 134 + rnd.Next(66);
            if (fx + shift > 262 && fx + shift < 380 && fy < 186) continue; // keep the house readable in the preview
            Screen(c, m.FlyGlow, fx - 3, fy - 3); Over(c, m.Fly, fx - 1, fy - 1);
        }
        Over(c, m.Fg, -shift, 216 - 32);
        Over(c, m.Orn, 112, 70);
        Over(c, m.Vig, 0, 0);
        Save(name, c, 3);
        if (name == "menu_composite.png") Save("menu_horizon_crop.png", c.Crop(0, 84, 384, 90), 4);
    }
}
