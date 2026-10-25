namespace WorldArtCh3;

/// <summary>Light, smoke, dust and mote sprites (semi-transparent, drawn in final colours).</summary>
public static partial class Sprites
{
    static Canvas Glow(int w, int h, Rgba col, double kMax, double power = 1.6, int steps = 9, double ditherAmt = 0.06)
    {
        var c = C(w, h);
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                double dx = (i + .5 - w / 2.0) / (w / 2.0), dy = (j + .5 - h / 2.0) / (h / 2.0);
                double d = Math.Sqrt(dx * dx + dy * dy);
                if (d >= 1) continue;
                double f = Math.Pow(1 - d, power) * kMax + Fx.Bay(i, j) * ditherAmt;
                int lv = (int)Math.Floor(f * steps);
                if (lv <= 0) continue;
                c.Set(i, j, new Rgba(col.R, col.G, col.B, (byte)Math.Min(255, lv * 255 / steps)));
            }
        return c;
    }

    static void Effects()
    {
        var warm = new Rgba(255, 196, 108);
        RegRaw("fx_c3_lampglow", Glow(104, 48, warm, 0.34));
        RegRaw("fx_c3_lamphalo", Glow(34, 30, new Rgba(255, 214, 130), 0.62, 1.2));
        RegRaw("fx_c3_lantern_glow_0", Glow(176, 100, new Rgba(255, 186, 96), 0.55));
        RegRaw("fx_c3_lantern_glow_1", Glow(172, 98, new Rgba(255, 178, 90), 0.48));
        RegRaw("fx_c3_lantern_halo", Glow(30, 30, new Rgba(255, 214, 130), 0.7, 1.2));
        RegRaw("fx_c3_door_glow", Glow(64, 84, new Rgba(255, 190, 100), 0.42));
        var f0 = C(16, 22); var f1 = C(16, 22);
        for (int j = 0; j < 22; j++)
            for (int i = 0; i < 16; i++)
            {
                if (j < 9 && ((i + j) & 1) == 0) f0.Set(i, j, new Rgba(255, 240, 170, 70));
                if (j > 12 && ((i + j) & 1) == 0) f1.Set(i, j, new Rgba(140, 70, 40, 70));
            }
        RegRaw("fx_c3_window_flicker_0", f0); RegRaw("fx_c3_window_flicker_1", f1);
        for (int f = 0; f < 4; f++) Plume($"fx_c3_smoke_{f}", f, 46, 110, false);
        for (int f = 0; f < 4; f++) Plume($"fx_c3_plume_far_{f}", f, 40, 96, true);
        for (int f = 0; f < 4; f++) ChimneySmoke(f);
        for (int f = 0; f < 3; f++) DustCloud(f);
        for (int f = 0; f < 3; f++) Motes(f, 72, 48, 12, $"fx_c3_motes_{f}");
        for (int f = 0; f < 3; f++) CeilingDust(f);
    }

    static void Plume(string name, int f, int w, int h, bool far)
    {
        var c = C(w, h);
        var dark = far ? new Rgba(74, 62, 98) : new Rgba(62, 54, 78);
        var light = far ? new Rgba(120, 100, 130) : new Rgba(118, 104, 132);
        double ph = f * Math.PI / 2;
        for (int y = 0; y < h; y++)
        {
            double t = (double)y / h;                              // 0 at the top, 1 at the base
            for (int x = 0; x < w; x++)
            {
                double u = 1 - t;                                   // grows towards the top
                double cx = w / 2.0 + Math.Sin(u * 4.5 + ph) * (2 + 7 * u);
                double half = 4.5 + u * (w / 2.0 - 8);
                double dx = Math.Abs(x + .5 - cx) / half;
                if (dx >= 1) continue;
                double n = Fx.Fbm(x / 5.0, (y + f * 6) / 5.0, 400);
                double dens = (1 - dx * dx) * (0.55 + n * 0.8) * (far ? 0.7 : 1.0) * (1 - u * u * 0.8);
                if (y > h - 6) dens *= (h - y) / 6.0;
                dens += Fx.Bay(x, y) * 0.12;
                if (dens < 0.2) continue;
                int a = dens > 0.62 ? 170 : dens > 0.42 ? 120 : 70;
                var col = Dusk.Mix(dark, light, Math.Clamp(u * 0.8 + (n - 0.5) * 0.5, 0, 1));
                if (t > 0.85) col = Dusk.Mix(col, new Rgba(214, 112, 64), (t - 0.85) / 0.15 * 0.7);
                c.Set(x, y, new Rgba(col.R, col.G, col.B, (byte)(far ? a * 0.8 : a)));
            }
        }
        RegRaw(name, c);
    }

    static void ChimneySmoke(int f)
    {
        var c = C(16, 34);
        for (int k = 0; k < 4; k++)
        {
            double t = ((f + k * 1.0) % 4) / 4.0;
            int cy = 31 - (int)(t * 28);
            double r = 2.6 + t * 4.5;
            double cx = 8 + Math.Sin(t * 6 + k) * (1 + t * 2.5);
            int a = (int)(150 * (1 - t));
            for (int y = (int)(cy - r); y <= cy + r; y++)
                for (int x = (int)(cx - r); x <= cx + r; x++)
                {
                    double dx = (x + .5 - cx) / r, dy = (y + .5 - cy) / r;
                    if (dx * dx + dy * dy > 1 || ((x + y) & 1) == 0 && dx * dx + dy * dy > 0.6) continue;
                    var col = dx + dy < 0 ? new Rgba(176, 170, 196, (byte)a) : new Rgba(126, 118, 150, (byte)a);
                    c.Over(x, y, col);
                }
        }
        RegRaw($"fx_c3_chimney_smoke_{f}", c);
    }

    static void DustCloud(int f)
    {
        int w = 176, h = 60;
        var c = C(w, h);
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++)
            {
                double dx = (i + .5 - w / 2.0) / (w / 2.0), dy = (j + .5 - h / 2.0) / (h / 2.0);
                double d = dx * dx * 0.8 + dy * dy * 1.3;
                if (d >= 1) continue;
                double n = Fx.Fbm((i + f * 9) / 11.0, (j - f * 2) / 7.0, 600);
                double dens = (1 - d) * 1.1 * (0.35 + n) + Fx.Bay(i, j) * 0.1;
                if (dens < 0.26) continue;
                int a = dens > 0.7 ? 96 : dens > 0.5 ? 70 : 44;
                c.Set(i, j, new Rgba(186, 172, 170, (byte)(a * 0.8)));
            }
        RegRaw($"fx_c3_dust_cloud_{f}", c);
    }

    static void Motes(int f, int w, int h, int n, string name)
    {
        var c = C(w, h);
        var rnd = new Random(900);
        for (int k = 0; k < n; k++)
        {
            double bx = rnd.NextDouble() * w, by = rnd.NextDouble() * h;
            double sp = 0.6 + rnd.NextDouble() * 0.8, ph = rnd.NextDouble() * 6.28;
            double t = f / 3.0 * Math.PI * 2;
            int x = (int)(bx + Math.Sin(t * sp + ph) * 4), y = (int)(by + Math.Cos(t * sp * 0.8 + ph) * 3 - f * 1.5);
            var col = k % 3 == 0 ? new Rgba(255, 236, 190, 230) : new Rgba(236, 214, 176, 160);
            c.Set(((x % w) + w) % w, ((y % h) + h) % h, col);
            if (k % 4 == 0) c.Set(((x + 1) % w + w) % w, ((y % h) + h) % h, new Rgba(255, 244, 210, 120));
        }
        RegRaw(name, c);
    }

    static void CeilingDust(int f)
    {
        var c = C(16, 44);
        var rnd = new Random(33);
        for (int k = 0; k < 9; k++)
        {
            int x = 1 + rnd.Next(14);
            int y0 = rnd.Next(44);
            int y = (y0 + f * 14 + k * 5) % 44;
            c.Set(x, y, new Rgba(226, 214, 196, 210));
            if (k % 2 == 0) c.Set(x, (y + 43) % 44, new Rgba(190, 178, 164, 140));
        }
        RegRaw($"fx_c3_ceiling_dust_{f}", c);
    }
}
