namespace CharacterArt;

/// <summary>Ginger tabby curled up asleep, 14x10 (content 12x8 at offset 1,1).</summary>
public static class Cat
{
    static char Tone(double dx, double dy, double lit = 0.5, double dark = -0.2)
    {
        double t = -0.65 * dx - 0.75 * dy;
        return t > lit ? 'q' : t < dark ? 'O' : 'o';
    }

    static void Shaded(Spr s, double cx, double cy, double rx, double ry)
    {
        s.Paint((x, y) =>
        {
            double dx = (x + .5 - 1 - cx) / rx, dy = (y + .5 - 1 - cy) / ry;
            return dx * dx + dy * dy <= 1.0 ? Tone(dx, dy) : null;
        });
    }

    static Spr Build(int frame)
    {
        var s = new Spr(14, 10);
        void P(int x, int y, char c) => s.Put(x + 1, y + 1, c);
        double lift = frame == 1 ? 0.5 : 0;

        // body mound (breathing lifts the top by 1px)
        Shaded(s, 4.6, 5.1 - lift, 4.7, 2.9 + lift);
        // tabby stripes on the back
        for (int x = 1; x <= 7; x += 2)
            for (int y = 0; y < 8; y++)
            {
                char c = s.Get(x + 1, y + 1);
                if (c == ' ') continue;
                if (y <= (frame == 1 ? 4 : 3) && (c is 'o' or 'q' or 'O')) { P(x, y, 'n'); break; }
            }

        // tail wraps along the front/bottom
        for (int x = 0; x <= 8; x++) P(x, 7, (x == 2 || x == 4 || x == 6) ? 'n' : x == 8 ? 'n' : 'O');
        for (int x = 1; x <= 7; x++) if (x != 2 && x != 4 && x != 6) P(x, 6, s.Get(x + 1, 7) == ' ' ? 'o' : s.Get(x + 1, 7) == 'o' ? 'o' : s.Get(x + 1, 7));
        P(0, 6, 'o'); P(0, 7, 'O');

        // head resting on the paws
        for (int y = 3; y <= 7; y++) if (s.Get(7 + 1, y + 1) != ' ') P(6, y, 'O');
        Shaded(s, 9.0, 5.0, 2.65, 2.2);
        // ears
        P(7, 1, 'n'); P(7, 2, 'o'); P(8, 2, 'p');
        P(11, 1, 'n'); P(11, 2, 'o'); P(10, 2, 'p');
        // cream muzzle / chest / paws
        P(8, 6, 'c'); P(9, 6, 'c'); P(10, 6, 'c'); P(8, 7, 'c'); P(9, 7, 'c'); P(10, 7, 'c'); P(11, 7, 'C');
        P(9, 5, 'p');
        // closed eyes (dark ginger, softer than black)
        P(8, 4, 'n'); P(10, 4, 'n');
        return s;
    }

    public static void Register(Action<string, Canvas, int, int> add, Action<string, Canvas> save)
    {
        var frames = new List<Canvas>();
        for (int f = 0; f < 2; f++)
        {
            var c = Build(f).Render(Pal.Base());
            add($"cat_sleep_{f}", c, 14, 10);
            frames.Add(c);
        }
        save("cat_sheet", Preview.Sheet(new() { frames }, 16, 2, true, true));
    }
}
