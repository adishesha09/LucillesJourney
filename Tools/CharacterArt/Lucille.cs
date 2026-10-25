namespace CharacterArt;

public enum Dir { Down, Up, Left, Right }

/// <summary>Lucille, 16x24. Content origin is (1,2) so the outline fits and a 1px bob never clips the head.</summary>
public static class Lucille
{
    public const int W = 16, H = 24;
    const int OX = 1, OY = 2;

    // Content rows r0..r17 (14 wide): head, torso, skirt. Legs/arms are layered per frame.
    static readonly string[] FrontRows =
    {
        "....hHHhhj....",
        "...hHHHhhhj...",
        "...hhaasshj...",
        "...hsesseSj...",
        "...hsissiSj...",
        "...hPssssPj...",
        "....sSmmSs....",
        ".....ssSS.....",
        "....QcsScR....",
        "...QQcrrCRR...",
        "...QQcccCRR...",
        "...QrcccCRR...",
        "...QrcccCRR...",
        "...QQcccCRR...",
        "..QQrcccCRRR..",
        ".QQrrcccCRRRR.",
        ".rrrrKKKKRRRR.",
    };

    static readonly string[] BackRows =
    {
        "....hHHhhj....",
        "...hHHHhhhj...",
        "...hHhhhhhj...",
        "...hHhhhhhj...",
        "...hhhhhhhj...",
        "...hhhyyhhj...",
        "...hjHHhhjj...",
        "....jHhhhj....",
        "....QchjcR....",
        "...QQcrrCRR...",
        "...QQcrrCRR...",
        "...QQcrrCRR...",
        "...QcccccCR...",
        "...QQrccrRR...",
        "..QQrrcCrRRR..",
        ".QQrrrRrrRRRR.",
        ".rrrrrrrrRRRR.",
    };

    static readonly string[] SideRows =
    {
        "....hHHhh.....",
        "...hHHHhhhh...",
        "...hHhhhhas...",
        "...hhhXsses...",
        ".HhhhhXssis...",
        "HHhyhhsssPsa..",
        "hhjYhhsssxs...",
        ".jj.hjssSS....",
        "....QrcsSR....",
        "....rrRRcC....",
        "....rrRRcC....",
        "....rrRRcC....",
        "...crrRRcC....",
        "..CCrrrRccC...",
        "...QrrrRccC...",
        "..QQrrrrRccC..",
        "..rrrrrRRKKK..",
    };

    public static Canvas Build(Dir d, int frame) => BuildSpr(d, frame).Render(Pal.Base());

    public static Spr BuildSpr(Dir d, int frame)
    {
        if (d == Dir.Left) return BuildSpr(Dir.Right, frame).Mirror();
        var s = new Spr(W, H);
        bool pass = frame == 1 || frame == 3;
        int bob = pass ? -1 : 0;
        if (d == Dir.Right) Side(s, frame, bob); else FrontBack(s, d == Dir.Up, frame, bob);
        return s;
    }

    static void FrontBack(Spr s, bool back, int frame, int bob)
    {
        void Put(int x, int y, char c) => s.Put(x + OX, y + OY, c);

        // foot rows: lower = planted / forward, higher = lifted / back
        (int l, int r) feet = frame switch { 0 => (20, 19), 1 => (20, 19), 2 => (19, 20), 3 => (19, 20), _ => (20, 20) };
        bool pass = frame == 1 || frame == 3;

        void Leg(int x, int foot, bool left)
        {
            for (int y = 15; y < foot; y++)
            {
                bool shade = y == 17 + bob;
                Put(x, y, shade ? 'S' : left ? 'a' : 's');
                Put(x + 1, y, shade ? 'S' : left ? 's' : 'S');
            }
            if (left) { Put(x - 1, foot, 't'); Put(x, foot, 't'); Put(x + 1, foot, 'T'); }
            else { Put(x, foot, 't'); Put(x + 1, foot, 't'); Put(x + 2, foot, 'T'); }
        }
        bool lLift = feet.l == 19, rLift = feet.r == 19;
        Leg(lLift && pass ? 6 : 5, feet.l, true);
        Leg(rLift && pass ? 7 : 8, feet.r, false);

        s.Map(OX, OY + bob, back ? BackRows : FrontRows, back ? "back" : "front");

        // arms: opposite arm swings with the forward leg
        (int l, int r) arm = frame switch { 0 => (1, 3), 2 => (3, 1), _ => (2, 2) };
        void Arm(int x, bool left, int len)
        {
            // 2px wide: sleeve rows 9-10, then forearm/hand
            Put(x, 9 + bob, left ? 'Q' : 'r'); Put(x + 1, 9 + bob, left ? 'r' : 'R');
            Put(x, 10 + bob, left ? 'r' : 'R'); Put(x + 1, 10 + bob, left ? 'R' : 'D');
            for (int i = 0; i < len; i++)
            {
                Put(x, 11 + i + bob, left ? (i == 0 ? 'a' : 's') : 's');
                Put(x + 1, 11 + i + bob, left ? 's' : 'S');
            }
        }
        Arm(3, true, arm.l);
        Arm(9, false, arm.r);
    }

    static void Side(Spr s, int frame, int bob)
    {
        void Put(int x, int y, char c) => s.Put(x + OX, y + OY, c);

        // (offset, foot row) for near / far leg; arm swing for the near arm
        ((int o, int f) near, (int o, int f) far, int armDx) p = frame switch
        {
            0 => ((2, 20), (-2, 20), -2),
            1 => ((0, 20), (1, 19), 0),
            2 => ((-2, 20), (2, 20), 2),
            3 => ((1, 19), (0, 20), 0),
            _ => ((1, 20), (-1, 20), 0),
        };

        void Leg((int o, int f) l, bool far)
        {
            const int hip = 6;
            for (int y = 15; y < l.f; y++)
            {
                double t = (y - 15) / (double)(l.f - 15);
                int x = hip + (int)Math.Round(l.o * t, MidpointRounding.AwayFromZero);
                bool shade = y == 17 + bob;
                Put(x, y, far || shade ? 'S' : 'a');
                Put(x + 1, y, far ? 'x' : shade ? 'S' : 's');
            }
            int fx = hip + l.o;
            if (far) { Put(fx, l.f, 'T'); Put(fx + 1, l.f, 'T'); Put(fx + 2, l.f, 'T'); Put(fx + 3, l.f, 'T'); }
            else { Put(fx, l.f, 't'); Put(fx + 1, l.f, 't'); Put(fx + 2, l.f, 't'); Put(fx + 3, l.f, 'T'); }
        }

        void Arm(int dx, bool far)
        {
            const int bx = 6;
            string[] rows = far
                ? new[] { "RD", "RD", "Sx", "Sx", "xx" }
                : new[] { "Qr", "Qr", "as", "as", "sS" };
            for (int i = 0; i < rows.Length; i++)
            {
                int x = bx + (int)Math.Round(dx * i / 4.0, MidpointRounding.AwayFromZero);
                Put(x, 9 + i + bob, rows[i][0]);
                Put(x + 1, 9 + i + bob, rows[i][1]);
            }
        }

        Arm(-p.armDx * 3 / 2, true);
        Leg(p.far, true);
        Leg(p.near, false);
        s.Map(OX, OY + bob, SideRows, "side");
        Arm(p.armDx, false);
    }
}
