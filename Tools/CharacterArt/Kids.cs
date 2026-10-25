namespace CharacterArt;

public static class Kids
{
    sealed record Def(Dictionary<char, Rgba> Pal, bool Braids, int LegLen, bool Reach, bool Swing);

    static Rgba H(string h) => Rgba.Hex(h);

    static Dictionary<char, Rgba> PalA()
    {
        var p = Pal.Base();
        p['H'] = H("8A664C"); p['h'] = H("5E4334"); p['j'] = H("3E2D26");
        p['1'] = H("F8DE8C"); p['2'] = H("F2D26B"); p['3'] = H("C9983F");   // gold top
        p['4'] = H("D8705C"); p['5'] = H("C05A4A"); p['6'] = H("9E4638");   // terracotta skirt
        return p;
    }

    static Dictionary<char, Rgba> PalB()
    {
        var p = Pal.Base();
        p['H'] = H("5E4A40"); p['h'] = H("3A2E2A"); p['j'] = H("261E1C");
        p['1'] = H("8FB0C4"); p['2'] = H("6B8FA8"); p['3'] = H("4C6E86");   // blue shirt
        p['4'] = H("CDB88F"); p['5'] = H("A8895E"); p['6'] = H("7A5F48");   // tan shorts
        return p;
    }

    static readonly Def A = new(PalA(), true, 2, true, false);
    static readonly Def B = new(PalB(), false, 1, false, true);

    static Spr Seated(Def d, int frame)
    {
        var s = new Spr(12, 16);
        const int ox = 1, oy = 1;
        void P(int x, int y, char c) => s.Put(x + ox, y + oy, c);
        int dy = frame == 1 ? 1 : 0;

        // chair: tall back post, one-row seat plank, legs
        P(0, 3, 'B');
        for (int y = 4; y <= 11; y++) P(0, y, 'M');
        for (int x = 1; x <= 6; x++) P(x, 11, 'B');
        P(0, 11, 'B');
        P(0, 12, 'M'); P(0, 13, 'b'); P(6, 12, 'M'); P(6, 13, 'b');

        if (d.Braids)
        {
            string[] br = { "hj", "jh", "hj", "yy" };
            for (int i = 0; i < br.Length; i++) { P(1, 5 + i, br[i][0]); P(2, 5 + i, br[i][1]); }
        }

        // torso (cols 3..6)
        for (int y = 6; y <= 8; y++) { P(3, y, '1'); P(4, y, '2'); P(5, y, '2'); P(6, y, '3'); }

        // lap
        for (int x = 3; x <= 8; x++) P(x, 9, x == 3 ? '4' : x == 8 ? '6' : '5');
        for (int x = 3; x <= 7; x++) P(x, 10, x == 3 ? '4' : x == 7 ? '6' : '5');
        P(8, 10, 's');

        // lower leg + shoe
        bool swing = d.Swing && frame == 1;
        int foot = 11 + d.LegLen - (swing ? 1 : 0);
        int lx = 7 + (swing ? 1 : 0);
        for (int y = 11; y < foot; y++) { P(lx, y, 's'); P(lx + 1, y, 'S'); }
        P(lx, foot, 't'); P(lx + 1, foot, 't'); if (lx + 2 <= 9) P(lx + 2, foot, 'T');

        // head (profile facing right)
        string[] head =
        {
            "...hHHh...",
            "..hHHhhh..",
            "..hhhhas..",
            "..hhXses..",
            "..hhsPssa.",
            "...hssxs..",
        };
        if (!d.Braids) head[0] = "...hHHhh..";
        s.Map(ox, oy + dy, head, "kid head");

        // near arm
        int ay = 6 + dy;
        P(4, ay, '2'); P(5, ay, '3');
        P(4, ay + 1, '2'); P(5, ay + 1, '3');
        if (d.Reach && frame == 1)
        {
            P(6, ay + 1, 'a'); P(7, ay + 1, 's'); P(8, ay + 1, 's'); P(9, ay + 1, 's');
        }
        else
        {
            P(4, ay + 2, '2'); P(5, ay + 2, '3');
            P(5, 9, 'a'); P(6, 9, 's'); P(7, 9, 's');
        }
        return s;
    }

    static Spr Standing(Def d, bool girl)
    {
        int h = girl ? 18 : 16;
        var s = new Spr(12, h + 2);
        const int ox = 1, oy = 1;
        string[] head = girl
            ? new[]
            {
                "...hHHh...",
                "..hHHhhj..",
                ".hhaasshj.",
                ".hsesseSj.",
                ".hsissiSj.",
                ".hPssssPj.",
                "..sSmmSs..",
            }
            : new[]
            {
                "...hHHh...",
                "..hHHHhj..",
                ".hhaasshj.",
                ".hsesseSj.",
                ".XsissiSX.",
                ".sPssssPS.",
                "..sSmmSs..",
            };
        s.Map(ox, oy, head, "kid front head");
        string[] top =
        {
            "..12sS23..",
            girl ? ".11222233." : ".1GGGGEE3.",
            ".a122223S.",
        };
        s.Map(ox, oy + 7, top, "kid top");
        if (girl)
        {
            s.Map(ox, oy + 10, new[]
            {
                ".a455556S.",
                "..455556..",
                ".45555566.",
                ".45555566.",
                ".46666666.",
                "..as..sS..",
                "..as..sS..",
                ".ttT..ttT.",
            }, "girl lower");
            for (int y = 3; y <= 8; y++) { s.Put(ox + 0, oy + y, y % 2 == 1 ? 'h' : 'j'); s.Put(ox + 9, oy + y, y % 2 == 1 ? 'j' : 'h'); }
            s.Put(ox + 0, oy + 9, 'y'); s.Put(ox + 9, oy + 9, 'y');
        }
        else
        {
            s.Map(ox, oy + 10, new[]
            {
                ".a455556S.",
                ".45555566.",
                "..as..sS..",
                "..as..sS..",
                "..as..sS..",
                ".ttT..ttT.",
            }, "boy lower");
        }
        return s;
    }

    public static void Register(Action<string, Canvas, int, int> add, Action<string, Canvas> save)
    {
        var sheet = new List<Canvas>();
        var seat = new List<Canvas>();
        for (int f = 0; f < 2; f++)
        {
            var a = Seated(A, f).Render(A.Pal);
            add($"child_a_sit_right_{f}", a, 12, 16);
            seat.Add(a);
        }
        for (int f = 0; f < 2; f++)
        {
            var b = Seated(B, f).Mirror().Render(B.Pal);
            add($"child_b_sit_left_{f}", b, 12, 16);
            seat.Add(b);
        }
        var sa = Standing(A, true).Render(A.Pal);
        var sb = Standing(B, false).Render(B.Pal);
        add("child_a_stand_down", sa, 12, 20);
        add("child_b_stand_down", sb, 12, 18);

        save("children_sheet", Preview.Sheet(new() { seat, new() { sa, sb } }, 12, 2, true, true));
    }
}
