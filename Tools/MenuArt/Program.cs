using System.Text;

namespace MenuArt;

public static class Program
{
    static string Root()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d != null && !(Directory.Exists(Path.Combine(d.FullName, "Assets")) && Directory.Exists(Path.Combine(d.FullName, "Tools")))) d = d.Parent;
        return d?.FullName ?? @"c:\Users\Adishesha\LucillesJourney";
    }

    public static string OutDir = "", PreviewDir = "";

    public static void Save(string name, Canvas c)
    {
        File.WriteAllBytes(Path.Combine(OutDir, name), Png.Encode(c));
        Console.WriteLine($"  {name} ({c.W}x{c.H})");
    }

    static int Opaque(Canvas c, int x0, int y0, int x1, int y1)
    {
        int n = 0;
        for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) if (c.Opaque(x, y)) n++;
        return n;
    }

    public static int Main(string[] args)
    {
        string root = Root();
        OutDir = Path.Combine(root, "Assets", "Resources", "Menu");
        PreviewDir = Path.Combine(root, "Tools", "MenuArt", "out_preview");
        Directory.CreateDirectory(OutDir); Directory.CreateDirectory(PreviewDir);

        // ---- MAIN MENU ----
        var sky = MenuSky.Sky(); Save("menu_sky.png", sky);
        var (s0, s1) = MenuSky.Stars(); Save("menu_stars_0.png", s0); Save("menu_stars_1.png", s1);
        var glow = MenuSky.SunGlow(); Save("menu_sun_glow.png", glow);
        var clouds = MenuSky.Clouds(); Save("menu_clouds.png", clouds);
        var far = MenuHills.Far(); Save("menu_hills_far.png", far);
        var mid = MenuHills.Mid(); Save("menu_hills_mid.png", mid);
        var ground = MenuGarden.Ground(); Save("menu_ground.png", ground);
        var wglow = MenuGarden.WindowGlow(); Save("menu_window_glow.png", wglow);
        var fg = MenuGarden.ForegroundGrass(); Save("menu_foreground_grass.png", fg);
        var vig = MenuSky.Vignette(); Save("menu_vignette.png", vig);
        var fly = MenuSky.Firefly(); var flyGlow = MenuSky.FireflyGlow();
        Save("menu_firefly.png", fly); Save("menu_firefly_glow.png", flyGlow);
        var orn = MenuSky.Ornament(); Save("menu_title_ornament.png", orn);

        // ---- ENDING ----
        var eSky = EndingSky.Sky(); Save("ending_sky.png", eSky);
        var eSun = EndingSky.Sun(); Save("ending_sun.png", eSun);
        var eClouds = EndingSky.Clouds(); Save("ending_clouds.png", eClouds);
        var eFar = EndingLand.Far(); Save("ending_hills_far.png", eFar);
        var eMid = EndingLand.Mid(); Save("ending_hills_mid.png", eMid);
        var eRoad = EndingLand.Road(); Save("ending_road.png", eRoad);
        var fig0 = Figures.Frame(0); var fig1 = Figures.Frame(1);
        Save("ending_figures_0.png", fig0); Save("ending_figures_1.png", fig1);
        var bird0 = EndingSky.Bird(true); var bird1 = EndingSky.Bird(false);
        Save("ending_bird_0.png", bird0); Save("ending_bird_1.png", bird1);
        var eGlow = EndingSky.Glow(EndingLand.TownWindows.Select(w => (w.x - 32, w.y)));
        Save("ending_glow.png", eGlow);
        var dust2 = EndingSky.Dust2(); var dust3 = EndingSky.Dust3();
        Save("ending_dust.png", dust2); Save("ending_dust_big.png", dust3);

        // ---- anchors ----
        int figY = 188, figX = (int)Math.Round(EndingLand.RoadX(figY));
        var winCentres = MenuGarden.Lights.Select(l => (l.X + l.W / 2, l.Y + l.H / 2)).Concat(MenuHills.FarWindows).ToList();
        string json = Anchors(winCentres, figX, figY);
        File.WriteAllText(Path.Combine(OutDir, "menu_anchors.json"), json, new UTF8Encoding(false));
        Console.WriteLine("  menu_anchors.json");
        Console.WriteLine(json);

        // ---- previews + sanity checks ----
        var menu = new MenuSet(sky, s0, s1, glow, clouds, far, mid, ground, wglow, fg, vig, fly, flyGlow, orn);
        Previews.Menu(menu, 32, "menu_composite.png");
        Previews.Menu(menu, 64, "menu_composite_offset.png");
        Previews.Menu(menu, 0, "menu_composite_offset_plus.png");
        var ending = new EndingSet(eSky, eSun, eClouds, eGlow, eFar, eMid, eRoad, fig0, fig1, bird0, bird1, dust2, dust3, vig, figX, figY);
        Previews.Ending(ending, 32, "ending_composite.png");
        Previews.Ending(ending, 64, "ending_composite_offset.png");
        Previews.Crop("menu_ground_crop.png", ground, 4);
        Previews.Save("ending_road_crop.png", eRoad.Crop(0, 100, 448, 116), 4);
        var tc = new Canvas(448, 216); Previews.Over(tc, eSky, -32, 0); Previews.Over(tc, eFar, 0, 0); Previews.Over(tc, eMid, 0, 0); Previews.Over(tc, eRoad, 0, 0);
        Previews.Save("ending_town_crop.png", tc.Crop(150, 84, 150, 56), 8);
        Previews.Seam("menu_clouds_seam.png", clouds); Previews.Seam("ending_clouds_seam.png", eClouds);
        var figSheet = new Canvas(96, 32); figSheet.Fill(Kit.C("A8B8A0")); Previews.Over(figSheet, fig0, 2, 2); Previews.Over(figSheet, fig1, 50, 2);
        Previews.Save("ending_figures_crop.png", figSheet, 8);
        var ob = new Canvas(160, 14); ob.Fill(Kit.C("2B3A3F")); Previews.Over(ob, orn, 0, 0); Previews.Save("menu_ornament_crop.png", ob, 6);

        Console.WriteLine($"title-zone check (opaque px): menu_ground x112..336,y12..70 = {Opaque(ground, 112, 12, 336, 70)}; ending_road x112..336,y8..60 = {Opaque(eRoad, 112, 8, 336, 60)}");
        Console.WriteLine($"fill check: far bottom row opaque = {Opaque(far, 0, 215, 447, 215)}/448, mid = {Opaque(mid, 0, 215, 447, 215)}/448, ending far = {Opaque(eFar, 0, 215, 447, 215)}/448, ending mid = {Opaque(eMid, 0, 215, 447, 215)}/448");
        return 0;
    }

    static string Anchors(List<(int x, int y)> win, int figX, int figY)
    {
        var sb = new StringBuilder();
        sb.Append("{\n");
        sb.Append("  \"menu\": {\n");
        sb.Append("    \"title_x\": 192,\n    \"title_y\": 40,\n");
        sb.Append("    \"windowGlow\": [\n");
        for (int i = 0; i < win.Count; i++) sb.Append($"      {{\"x\": {win[i].x}, \"y\": {win[i].y}}}{(i < win.Count - 1 ? "," : "")}\n");
        sb.Append("    ],\n");
        sb.Append($"    \"chimney\": {{\"x\": {MenuGarden.Chimney.x}, \"y\": {MenuGarden.Chimney.y}}},\n");
        sb.Append("    \"fireflyRect\": {\"x\": 20, \"y\": 134, \"w\": 400, \"h\": 66}\n");
        sb.Append("  },\n");
        sb.Append("  \"ending\": {\n");
        sb.Append($"    \"figuresX\": {figX},\n    \"figuresY\": {figY},\n");
        sb.Append($"    \"sunStartX\": {EndingSky.SunStartX},\n    \"sunStartY\": {EndingSky.SunStartY},\n    \"sunEndY\": {EndingSky.SunEndY},\n");
        sb.Append("    \"dustRect\": {\"x\": 40, \"y\": 36, \"w\": 304, \"h\": 130},\n");
        sb.Append("    \"birdPath\": {\"x0\": 24, \"y0\": 84, \"x1\": 344, \"y1\": 46}\n");
        sb.Append("  }\n");
        sb.Append("}\n");
        return sb.ToString();
    }
}

public record MenuSet(Canvas Sky, Canvas Stars0, Canvas Stars1, Canvas Glow, Canvas Clouds, Canvas Far, Canvas Mid, Canvas Ground, Canvas WGlow, Canvas Fg, Canvas Vig, Canvas Fly, Canvas FlyGlow, Canvas Orn);
public record EndingSet(Canvas Sky, Canvas Sun, Canvas Clouds, Canvas Glow, Canvas Far, Canvas Mid, Canvas Road, Canvas Fig0, Canvas Fig1, Canvas Bird0, Canvas Bird1, Canvas Dust2, Canvas Dust3, Canvas Vig, int FigX, int FigY);
