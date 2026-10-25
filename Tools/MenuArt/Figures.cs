using static MenuArt.Kit;

namespace MenuArt;

/// <summary>Lucille and the two children seen from behind, walking away. Colours are the in-game character sprite colours.</summary>
public static class Figures
{
    // hair
    static readonly Rgba H = C("73594A"), Hs = C("4A3B32"), Hj = C("33292A"), Hl = C("8A6A52");
    // skin
    static readonly Rgba Sk = C("D19A6D"), SkD = C("B0795A"), SkL = C("E8BC93");
    // terracotta dress
    static readonly Rgba Q = C("D8705C"), R = C("C05A4A"), RD = C("A04838"), D = C("7E3A32");
    // apron
    static readonly Rgba Ac = C("F4EAD2"), AcD = C("DCCBA4"), AcK = C("B5A07A");
    // sandals
    static readonly Rgba Sa = C("A8895E"), SaD = C("7A5F48");

    public static Canvas Frame(int f)
    {
        var c = new Canvas(44, 28);
        int dy = f == 0 ? 0 : -1;                       // 1 px bob
        bool leftFwd = f == 0;                           // which leg is planted (lower) for Lucille
        Lucille(c, dy, leftFwd);
        Girl(c, dy, !leftFwd);
        Boy(c, dy, leftFwd);
        c.SelOutline(0.38);
        return c;
    }

    static void Px(Canvas c, int x, int y, Rgba col) => c.Set(x, y, col);
    static void Row(Canvas c, int x0, int x1, int y, Rgba col) { for (int x = x0; x <= x1; x++) c.Set(x, y, col); }

    static void Lucille(Canvas c, int dy, bool leftPlanted)
    {
        int y0 = 2 + dy;                                  // r0
        int X(int col) => 16 + col;
        // bun and hair
        Row(c, X(5), X(6), y0, H); Px(c, X(5), y0, Hl);
        Row(c, X(4), X(7), y0 + 1, H); Px(c, X(7), y0 + 1, Hs); Px(c, X(4), y0 + 1, Hl); Px(c, X(5), y0 + 1, Hl);
        Row(c, X(4), X(7), y0 + 2, Hs); Px(c, X(4), y0 + 2, H); Px(c, X(5), y0 + 2, C("F2D26B"));
        Row(c, X(3), X(8), y0 + 3, H); Px(c, X(8), y0 + 3, Hs); Px(c, X(4), y0 + 3, Hl);
        for (int r = 4; r <= 6; r++) { Row(c, X(3), X(8), y0 + r, H); Px(c, X(7), y0 + r, Hs); Px(c, X(8), y0 + r, Hs); Px(c, X(5), y0 + r, Hs); }
        Row(c, X(4), X(7), y0 + 7, Hs); Px(c, X(4), y0 + 7, H);
        // neck
        Px(c, X(5), y0 + 7, Sk); Px(c, X(6), y0 + 7, SkD);
        // shoulders, torso (dress) with crossed apron straps and a waist bow
        for (int r = 8; r <= 12; r++) { Row(c, X(3), X(8), y0 + r, R); Px(c, X(3), y0 + r, Q); Px(c, X(4), y0 + r, Q); Px(c, X(8), y0 + r, RD); }
        Row(c, X(2), X(2), y0 + 8, Q); Row(c, X(9), X(9), y0 + 8, R); Row(c, X(2), X(2), y0 + 9, Q); Row(c, X(9), X(9), y0 + 9, RD);
        foreach (var (sx, sr, col) in new[] { (4, 8, Ac), (4, 9, Ac), (5, 10, Ac), (5, 11, AcD), (6, 12, AcD), (7, 8, Ac), (7, 9, AcD), (6, 10, Ac), (6, 11, AcD), (5, 12, AcD) })
            Px(c, X(sx), y0 + sr, col);
        Row(c, X(3), X(8), y0 + 13, R); Px(c, X(3), y0 + 13, Q);
        Row(c, X(4), X(7), y0 + 13, Ac); Px(c, X(4), y0 + 13, AcD); Px(c, X(7), y0 + 13, AcD);
        Px(c, X(3), y0 + 14, Ac); Px(c, X(4), y0 + 14, AcD); Px(c, X(7), y0 + 14, Ac); Px(c, X(8), y0 + 14, AcD); // bow tails
        Px(c, X(4), y0 + 15, AcD); Px(c, X(7), y0 + 15, AcK);
        // skirt: flared bell, folds and a darker hem
        int[] left = { 3, 3, 2, 2, 1, 1, 0, 0 };
        for (int i = 0; i < 8; i++)
        {
            int r = 14 + i; int a = left[i], b = 11 - left[i];
            for (int col = a; col <= b; col++)
            {
                Rgba k = col < a + 2 ? Q : col > b - 3 ? RD : R;
                if (i >= 3 && (col == 3 || col == 6 || col == 9) && ((i + col) & 1) == 0) k = k == RD ? D : (col == 9 ? D : RD);
                if (i == 7) k = col < a + 2 ? R : col > b - 3 ? D : RD;
                if (r >= 14 && r <= 15 && col >= 3 && col <= 8 && ((col == 3 && r == 14) || (col == 8 && r == 14))) continue; // keep the bow tails readable
                Px(c, X(col), y0 + r, k);
            }
        }
        // apron tails over the skirt
        Px(c, X(4), y0 + 15, AcD); Px(c, X(7), y0 + 15, Ac); Px(c, X(4), y0 + 16, AcK); Px(c, X(7), y0 + 16, AcD);
        // legs and sandals (walk cycle)
        int ly = y0 + 21;
        void Leg(int x, bool planted)
        {
            int len = planted ? 3 : 2;
            for (int j = 1; j <= len - 1; j++) { Px(c, x, ly + j, Sk); Px(c, x + 1, ly + j, j == len - 1 ? SkD : SkD); }
            int fy = ly + len; Px(c, x - 1, fy, SaD); Px(c, x, fy, Sa); Px(c, x + 1, fy, SaD);
        }
        Leg(X(3), leftPlanted); Leg(X(7), !leftPlanted);
        // arms: sleeve, then forearm to the held hands
        Line(c, X(2), y0 + 9, 14 + 0, 16 + dy, Sk, SkD);
        Line(c, X(9), y0 + 9, 29, 16 + dy, Sk, SkD);
        Px(c, X(2), y0 + 9, Q); Px(c, X(2), y0 + 10, Q); Px(c, X(9), y0 + 9, R); Px(c, X(9), y0 + 10, RD);
    }

    static void Line(Canvas c, int x0, int y0, int x1, int y1, Rgba a, Rgba b)
    {
        int dx = Math.Abs(x1 - x0), dyy = -Math.Abs(y1 - y0), sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, err = dx + dyy; int i = 0;
        while (true)
        {
            c.Set(x0, y0, i % 2 == 0 ? a : b);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err; if (e2 >= dyy) { err += dyy; x0 += sx; } if (e2 <= dx) { err += dx; y0 += sy; } i++;
        }
    }

    static void Girl(Canvas c, int dy, bool leftPlanted)
    {
        int y0 = 7 + dy;
        int X(int col) => 3 + col;
        var gH = C("8A664C"); var gHs = C("5E4334"); var gHj = C("3E2D26");
        var g1 = C("F8DE8C"); var g2 = C("F2D26B"); var g3 = C("C9983F");
        var s4 = C("D8705C"); var s5 = C("C05A4A"); var s6 = C("9E4638");
        // hair cap with a centre parting, two plaited braids on the back
        Row(c, X(3), X(6), y0, gH);
        Row(c, X(2), X(7), y0 + 1, gH); Row(c, X(2), X(7), y0 + 2, gH); Row(c, X(2), X(7), y0 + 3, gH); Row(c, X(2), X(7), y0 + 4, gHs);
        Px(c, X(4), y0 + 1, gHs); Px(c, X(4), y0 + 2, gHs); Px(c, X(3), y0, C("A47E60")); Px(c, X(2), y0 + 2, C("A47E60")); Px(c, X(7), y0 + 2, gHs); Px(c, X(7), y0 + 3, gHs);
        Px(c, X(4), y0 + 5, Sk); Px(c, X(5), y0 + 5, SkD);
        for (int r = 5; r <= 12; r++)
        {
            var b = ((r / 2) & 1) == 0 ? gH : gHs;
            Px(c, X(2), y0 + r, b); Px(c, X(7), y0 + r, b == gH ? gHs : gH);
        }
        Px(c, X(2), y0 + 13, s5); Px(c, X(7), y0 + 13, s6); Px(c, X(2), y0 + 14, gHj); Px(c, X(7), y0 + 14, gHj);
        // gold top
        for (int r = 6; r <= 10; r++) { Row(c, X(2), X(7), y0 + r, g2); Px(c, X(3), y0 + r, g1); Px(c, X(7), y0 + r, g3); }
        Px(c, X(2), y0 + 6, g1); Px(c, X(1), y0 + 6, g1); Px(c, X(8), y0 + 6, g3); Px(c, X(1), y0 + 7, g2); Px(c, X(8), y0 + 7, g3);
        for (int r = 6; r <= 10; r++) { Px(c, X(4), y0 + r, ((r + 1) & 1) == 0 ? g2 : Mix(g2, g1, 0.5)); }
        // terracotta skirt
        int[] left = { 2, 2, 1, 1, 0 };
        for (int i = 0; i < 5; i++)
        {
            int r = 11 + i;
            for (int col = left[i]; col <= 9 - left[i]; col++)
                Px(c, X(col), y0 + r, i == 4 ? (col < 3 ? s5 : s6) : col < left[i] + 2 ? s4 : col > 7 - left[i] ? s6 : s5);
        }
        // legs + sandals
        int ly = y0 + 15;
        void Leg(int x, bool planted)
        {
            int len = planted ? 4 : 3;
            for (int j = 1; j <= len - 1; j++) { Px(c, x, ly + j, Sk); Px(c, x + 1, ly + j, SkD); }
            Px(c, x, ly + len, Sa); Px(c, x + 1, ly + len, SaD);
        }
        Leg(X(3), leftPlanted); Leg(X(5), !leftPlanted);
        // arms: outer arm swings, inner arm reaches for Lucille's hand
        Px(c, X(1), y0 + 8, g2); Px(c, X(1), y0 + 9, Sk); Px(c, X(0), y0 + 10 + (leftPlanted ? 0 : 1), Sk); Px(c, X(0), y0 + 11 + (leftPlanted ? 0 : 1), SkD);
        Line(c, X(8), y0 + 8, 13, 16 + dy, Sk, SkD);
        Px(c, X(8), y0 + 7, g3);
        Px(c, 13, 16 + dy, SkL); Px(c, 14, 16 + dy, Sk);
    }

    static void Boy(Canvas c, int dy, bool rightPlanted)
    {
        int y0 = 12 + dy;
        int X(int col) => 32 + col;
        var bH = C("5E4A40"); var bHs = C("3A2E2A"); var bHj = C("261E1C");
        var b1 = C("8FB0C4"); var b2 = C("6B8FA8"); var b3 = C("4C6E86");
        var t4 = C("CDB88F"); var t5 = C("A8895E"); var t6 = C("7A5F48");
        Row(c, X(3), X(6), y0, bH);
        Row(c, X(2), X(7), y0 + 1, bH); Row(c, X(2), X(7), y0 + 2, bH); Row(c, X(2), X(7), y0 + 3, bHs);
        Px(c, X(3), y0 + 1, C("7A6252")); Px(c, X(7), y0 + 2, bHs); Px(c, X(2), y0 + 3, bH); Px(c, X(5), y0 + 1, bHs);
        Px(c, X(4), y0 + 4, Sk); Px(c, X(5), y0 + 4, SkD);
        // shirt
        for (int r = 5; r <= 9; r++) { Row(c, X(2), X(7), y0 + r, b2); Px(c, X(2), y0 + r, b1); Px(c, X(3), y0 + r, r < 8 ? b1 : b2); Px(c, X(7), y0 + r, b3); }
        Px(c, X(1), y0 + 5, b1); Px(c, X(1), y0 + 6, b2); Px(c, X(8), y0 + 5, b3); Px(c, X(8), y0 + 6, b3);
        for (int x = X(3); x <= X(7); x += 2) Px(c, x, y0 + 9, b3);
        // shorts
        for (int r = 10; r <= 11; r++) { Row(c, X(2), X(7), y0 + r, t5); Px(c, X(2), y0 + r, t4); Px(c, X(3), y0 + r, t4); Px(c, X(7), y0 + r, t6); }
        Row(c, X(2), X(7), y0 + 11, t6); Px(c, X(2), y0 + 11, t5); Px(c, X(3), y0 + 11, t5);
        // legs
        int ly = y0 + 11;
        void Leg(int x, bool planted)
        {
            int len = planted ? 3 : 2;
            for (int j = 1; j <= len - 1; j++) { Px(c, x, ly + j, Sk); Px(c, x + 1, ly + j, SkD); }
            Px(c, x, ly + len, Sa); Px(c, x + 1, ly + len, SaD);
        }
        Leg(X(2), !rightPlanted); Leg(X(5), rightPlanted);
        // arms: inner arm reaches Lucille's hand, outer swings
        Line(c, X(1), y0 + 6, 30, 16 + dy, Sk, SkD);
        Px(c, 30, 16 + dy, SkL); Px(c, 29, 16 + dy, Sk);
        Px(c, X(8), y0 + 7, Sk); Px(c, X(8), y0 + 8, SkD); Px(c, X(9), y0 + 8 + (rightPlanted ? 1 : 0), SkD);
    }
}
