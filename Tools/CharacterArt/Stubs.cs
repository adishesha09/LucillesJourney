namespace CharacterArt;

/// <summary>Lucille bust portrait, 48x48, drawn with the same palette chars as the in-game sprite.</summary>
public static class Portrait
{
    public const int W = 48, H = 48;

    public static void Register(Action<string, Canvas, int, int> add, Action<string, Canvas> save)
    {
        var c = Build().Render(Pal.Base());
        add("lucille_portrait", c, W, H);

        var front = Lucille.Build(Dir.Down, -1);
        save("portrait_sheet", Preview.Sheet(new() { new() { front, c } }, 8, 2, true, true));
        save("portrait_zoom", Preview.Sheet(new() { new() { c } }, 14, 1, false, true));
    }

    // Face half-width per row (face spans x = 24-hw .. 23+hw).
    static int FaceHw(int y) => y switch
    {
        13 => 8, 14 => 9, 15 => 10, 16 => 10, 17 => 11, >= 18 and <= 25 => 11, 26 => 10, 27 => 10, 28 => 9, 29 => 8, 30 => 7, 31 => 6, 32 => 5, 33 => 3, _ => 0,
    };

    static int HairHw(int y) => y switch
    {
        3 => 6, 4 => 9, 5 => 11, 6 => 13, 7 => 14, 8 => 15, <= 27 and >= 9 => 16, 28 => 14, 29 => 12, 30 => 10, _ => 0,
    };

    static int ShoulderHw(int y) => y switch { 36 => 8, 37 => 10, 38 => 12, 39 => 16, 40 => 18, 41 => 19, >= 42 => 20, _ => 0 };

    static int Hairline(int x) => 13 + (int)Math.Round(Math.Pow((x - 23.5) / 11.0, 2) * 5.5);

    public static Spr Build()
    {
        var s = new Spr(W, H);

        Bun(s);
        Hair(s);
        Body(s);
        Neck(s);
        Face(s);
        Features(s);
        return s;
    }

    static void Bun(Spr s)
    {
        s.Ellipse(38, 35, 5.2, 4.8, 'h');
        s.Paint((x, y) =>
        {
            if (s.Get(x, y) != 'h') return null;
            if (x + y * 0.7 >= 66) return 'j';
            return null;
        });
        foreach (var (x, y) in new[] { (36, 36), (37, 36), (36, 37), (38, 35) }) s.Put(x, y, 'H');
        // swirl of the wrapped hair
        foreach (var (x, y) in new[] { (37, 38), (38, 38), (39, 37), (40, 36), (40, 34), (41, 35) }) if (s.Get(x, y) != ' ') s.Put(x, y, 'j');
        // gold clip where the bun is tied off
        s.Rect(34, 31, 3, 3, 'y');
        s.Put(34, 31, 'L');
        s.Rect(34, 33, 3, 1, 'Y');
        s.Put(36, 32, 'Y');
    }

    static void Hair(Spr s)
    {
        for (int y = 3; y <= 30; y++)
        {
            int hw = HairHw(y);
            for (int x = 24 - hw; x < 24 + hw; x++)
            {
                double d = Math.Sqrt((x - 23.5) * (x - 23.5) + (y - 19.0) * (y - 19.0) * 1.1);
                char c = 'h';
                // curved sheen across the crown on the lit (left) side
                if (d >= 12.0 && d < 13.8 && y <= 15 && x <= 26) c = 'H';
                if (d >= 12.6 && d < 13.2 && y <= 9 && x >= 14 && x <= 19) c = 'B';
                if (x >= 36 && y >= 7 || y >= 26 && x >= 25) c = 'j';
                if (x < 13 && y >= 11 && y <= 23 && (x == 10 || x == 12 && y >= 14 && y <= 20)) c = 'H';
                s.Put(x, y, c);
            }
        }
    }

    static void Body(Spr s)
    {
        for (int y = 36; y < H; y++)
        {
            int hw = ShoulderHw(y);
            for (int x = 24 - hw; x < 24 + hw; x++)
            {
                double n = (x - 23.5) / 20.0 + (y - 38) * 0.012;
                char c = n < -0.5 ? 'Q' : n < 0.12 ? 'r' : n < 0.74 ? 'R' : 'D';
                s.Put(x, y, c);
            }
        }
        // apron straps and bib
        for (int y = 37; y < H; y++)
        {
            bool bib = y >= 43;
            for (int x = 16; x < 32; x++)
            {
                bool strap = x < 20 || x >= 28;
                if (!bib && !strap) continue;
                char c = x < 27 ? 'c' : 'C';
                if (bib && y == 43) c = x < 24 ? 'c' : 'C';
                if (y >= 47 && bib) c = x < 24 ? 'C' : 'K';
                s.Put(x, y, c);
            }
        }
        // neckline: skin chest between the straps, then the dress band above the bib
        for (int y = 37; y <= 40; y++)
            for (int x = 20; x < 28; x++)
            {
                if (y == 40 && (x == 20 || x == 27)) continue;
                s.Put(x, y, x < 25 ? 's' : 'S');
            }
        s.Put(20, 40, 'r'); s.Put(27, 40, 'R');
    }

    static void Neck(Spr s)
    {
        for (int y = 33; y <= 38; y++)
            for (int x = 20; x < 28; x++)
            {
                char c = x < 25 ? 's' : 'S';
                if (y == 34) c = x < 25 ? 'S' : 'x';
                if (y >= 37 && x < 22) c = 'a';
                s.Put(x, y, c);
            }
    }

    static void Face(Spr s)
    {
        for (int y = 13; y <= 33; y++)
        {
            int hw = FaceHw(y);
            for (int x = 24 - hw; x < 24 + hw; x++)
            {
                if (y < Hairline(x)) continue;
                char c = 's';
                if (y <= 17 && Math.Pow((x - 20.0) / 6.5, 2) + Math.Pow((y - 15.0) / 2.6, 2) < 1.0) c = 'a';
                if (x >= 23 + hw - 2 && y >= 15 || y >= 31 && x >= 24 || y == 33) c = 'S';
                s.Put(x, y, c);
            }
        }
        // soft shadow under the fringe
        for (int x = 12; x < 36; x++)
        {
            int hl = Hairline(x);
            if (s.Get(x, hl) is 's' or 'a') s.Put(x, hl, x > 20 ? 'S' : 's');
        }
        // side locks framing the face, like the sprite's hair columns
        for (int y = 16; y <= 25; y++) { s.Put(13, y, y < 20 ? 'H' : 'h'); s.Put(34, y, 'j'); }
        s.Put(14, 16, 'h'); s.Put(33, 16, 'h'); s.Put(13, 26, 'h'); s.Put(34, 26, 'j');
        // hair edge facing the shade side gets the darkest tone
        for (int y = 14; y <= 30; y++)
            for (int x = 24; x < 40; x++)
                if (s.Get(x, y) is 'h' or 'H' && s.Get(x - 1, y) is 'a' or 's' or 'S') s.Put(x, y, 'j');
    }

    static void Features(Spr s)
    {
        void Row(int x, int y, string row) { for (int i = 0; i < row.Length; i++) if (row[i] != '.') s.Put(x + i, y, row[i]); }

        // brows: gentle arch, inner end a touch higher (tired but warm)
        Row(16, 18, "hh..");
        Row(18, 17, "hhh");
        Row(27, 17, "hhh");
        Row(28, 18, "..hh");

        // eyes: 4x4 with a tiny highlight; lower lid softened
        foreach (int ex in new[] { 17, 27 })
        {
            Row(ex, 20, "eeee");
            Row(ex, 21, "ewie");
            Row(ex, 22, "eiie");
            Row(ex, 23, ".ee.");
            s.Put(ex + 1, 24, 'X'); s.Put(ex + 2, 24, 'X');
        }
        // blush
        Row(15, 25, "PPP");
        Row(15, 26, "PPP");
        Row(30, 25, "PPP");
        Row(30, 26, "PPP");

        // nose
        s.Put(23, 25, 'a'); s.Put(24, 26, 'S'); s.Put(23, 26, 'X');

        // smile: corners lifted, soft lower lip
        s.Put(20, 27, 'S'); s.Put(27, 27, 'S');
        s.Put(21, 28, 'm'); s.Put(26, 28, 'm');
        Row(22, 29, "mmmm");
        s.Put(23, 30, 'P'); s.Put(24, 30, 'P');
    }
}
