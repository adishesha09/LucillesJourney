namespace PixelArt;

/// <summary>Group A: 9-slice UI frames and small UI parts.</summary>
public static class UiFrames
{
    static Canvas Box(int w, int h, int notch, Func<int, int, int, Rgba> colour)
    {
        var c = new Canvas(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int dx = Math.Min(x, w - 1 - x), dy = Math.Min(y, h - 1 - y);
                if (dx + dy < notch) continue;
                c.Set(x, y, colour(Math.Min(dx, dy), x, y));
            }
        return c;
    }

    // Standard bevel: two outer rings, one inner edge ring, then fill.
    static Canvas Std(int w, int h, Rgba outer, Rgba inner, Rgba fill, int notch = 2) =>
        Box(w, h, notch, (d, x, y) => d < 2 ? outer : d == 2 ? inner : fill);

    static void Add(Canvas c, string name, int border)
    {
        Registry.Add("A_frames", name, c, c.W, c.H);
        Preview.Borders[name] = border;
    }

    static void Rivets(Canvas c, Rgba col)
    {
        foreach (var (x, y) in new[] { (4, 4), (c.W - 5, 4), (4, c.H - 5), (c.W - 5, c.H - 5) }) c.Set(x, y, col);
    }

    // Horizontal highlight row across the 9-slice interior at row y (inside the 8px border band).
    static void HRow(Canvas c, int y, Rgba col) { for (int x = 3; x < c.W - 3; x++) c.Set(x, y, col); }

    public static void Build()
    {
        // --- panels
        var panel = Std(24, 24, P.Sand, P.Brown, P.DeepBrown);
        Rivets(panel, P.Tan);
        Add(panel, "ui_panel", 8);

        var parch = Std(24, 24, P.Brown, P.Tan, P.Parchment);
        Rivets(parch, P.Tan);
        Add(parch, "ui_panel_parchment", 8);

        // --- buttons
        var btn = Std(24, 24, P.Sand, P.DeepBrown, P.Brown);
        HRow(btn, 3, P.LightBrown); HRow(btn, 20, P.DeepBrown);
        Add(btn, "ui_button", 8);

        var hover = Std(24, 24, P.Gold, P.Brown, P.MidBrown);
        HRow(hover, 3, P.LightBrown); HRow(hover, 20, P.Brown);
        Add(hover, "ui_button_hover", 8);

        var pressed = Std(24, 24, P.Sand, P.Ink, P.DeepBrown);
        HRow(pressed, 3, P.Ink); HRow(pressed, 20, P.MidBrown);
        Add(pressed, "ui_button_pressed", 8);

        var disabled = Std(24, 24, Rgba.Hex("6A645B"), Rgba.Hex("433F3B"), Rgba.Hex("5A554F"));
        HRow(disabled, 3, Rgba.Hex("66605A"));
        Add(disabled, "ui_button_disabled", 8);

        var sel = Std(24, 24, P.LightGold, P.DarkGold, P.SelFill);
        for (int y = 0; y < 24; y++) for (int x = 0; x < 24; x++)
            {
                int d = Math.Min(Math.Min(x, 23 - x), Math.Min(y, 23 - y));
                if (d == 1 && sel.Opaque(x, y)) sel.Set(x, y, P.Gold);
            }
        HRow(sel, 3, Rgba.Hex("7A6048")); HRow(sel, 20, P.Brown);
        Add(sel, "ui_button_selected", 8);

        // --- tabs
        var tab = Std(24, 24, P.Tan, P.Brown, P.MidBrown);
        HRow(tab, 3, P.LightBrown);
        Add(tab, "ui_tab", 8);

        var tabA = Std(24, 24, P.Tan, P.Sand, P.Cream);
        for (int x = 0; x < 24; x++)
        {
            if (tabA.Opaque(x, 0)) tabA.Set(x, 0, P.Gold);
            if (tabA.Opaque(x, 1)) tabA.Set(x, 1, P.Gold);
            if (tabA.Opaque(x, 2) && x > 1 && x < 22) tabA.Set(x, 2, P.DarkGold);
        }
        tabA.Set(1, 2, P.Gold); tabA.Set(22, 2, P.Gold);
        Add(tabA, "ui_tab_active", 8);

        // --- card slots
        var slot = Box(24, 24, 3, (d, x, y) =>
        {
            int dx = Math.Min(x, 23 - x), dy = Math.Min(y, 23 - y);
            if (d == 0) return P.Brown;
            if (d == 1) return P.Ink;
            if (d == 2) return (dx < 8 && dy < 8 && ((dx + dy) & 1) == 1) ? P.Brown : P.Ink;
            return P.Ink;
        });
        Add(slot, "ui_slot", 8);

        var slotOpen = Box(24, 24, 2, (d, x, y) => d < 2 ? P.Brown : d == 2 ? P.Tan : d == 3 ? P.DarkParch : P.Parchment);
        Add(slotOpen, "ui_slot_open", 8);

        // --- toast: gold accent on top/left, dim sand-brown bottom/right
        var toast = Box(24, 24, 2, (d, x, y) =>
        {
            bool accent = (x <= 23 - x ? x : 99) == d || (y <= 23 - y ? y : 99) == d;
            if (d < 2) return accent ? P.Gold : P.Brown;
            if (d == 2) return accent ? P.DarkGold : P.Ink;
            return P.DeepBrown;
        });
        Add(toast, "ui_toast", 8);

        // --- bars and slider
        var barFrame = Box(12, 12, 1, (d, x, y) =>
        {
            if (d == 0) return P.Sand;
            if (d == 1) return (x == 1 || y == 1) ? P.Ink : P.Brown;
            return P.DeepBrown;
        });
        Add(barFrame, "ui_bar_frame", 4);

        var barFill = new Canvas(8, 8);
        barFill.Rect(0, 0, 8, 2, P.White);
        barFill.Rect(0, 2, 8, 5, Rgba.Hex("E6E6E6"));
        barFill.Rect(0, 7, 8, 1, Rgba.Hex("D8D8D8"));
        Registry.Add("A_frames", "ui_bar_fill", barFill, 8, 8);

        var slider = Box(12, 12, 1, (d, x, y) =>
        {
            if (d == 0) return P.MidBrown;
            if (d == 1) return (x == 1 || y == 1) ? P.Ink : P.Brown;
            return P.DeepBrown;
        });
        Add(slider, "ui_slider_bg", 4);

        // --- knob (ink outline added after a notched body)
        var knobBody = Box(14, 14, 2, (d, x, y) =>
        {
            bool top = y == d && x >= d && x <= 13 - d, left = x == d && y >= d && y <= 13 - d;
            bool bot = y == 13 - d && x >= d, right = x == 13 - d && y >= d;
            if (d <= 1) return (top || left) ? P.Cream : P.Sand;
            if (d == 2) return (bot || right) ? P.Tan : P.Sand;
            if (d == 3) return P.MidBrown;
            // gold centre with a top highlight and a bottom shade
            return y == 4 ? P.LightGold : (y == 9 ? P.DarkGold : P.Gold);
        });
        var knob = new Canvas(16, 16); knob.Blit(knobBody, 1, 1); knob.Outline(P.Ink);
        Registry.Add("A_frames", "ui_knob", knob, 16, 16);

        var tbox = Box(14, 14, 2, (d, x, y) => d < 2 ? P.Sand : d == 2 ? ((x == 2 || y == 2) ? P.Ink : P.Brown) : P.DeepBrown);
        var toggleBox = new Canvas(16, 16); toggleBox.Blit(tbox, 1, 1); toggleBox.Outline(P.Ink);
        Registry.Add("A_frames", "ui_toggle_box", toggleBox, 16, 16);

        // --- tick: 3px-wide stroke, darker gold underside on the right
        var tick = new Canvas(16, 16);
        for (int i = 0; i < 4; i++) { tick.Set(1 + i, 8 + i, P.Gold); tick.Set(2 + i, 8 + i, P.Gold); tick.Set(3 + i, 8 + i, P.DarkGold); tick.Set(2 + i, 9 + i, P.DarkGold); }
        for (int i = 0; i < 8; i++) { tick.Set(4 + i, 11 - i, P.Gold); tick.Set(5 + i, 11 - i, P.Gold); tick.Set(6 + i, 11 - i, P.DarkGold); tick.Set(7 + i, 11 - i, P.DarkGold); }
        for (int i = 1; i < 8; i++) tick.Set(4 + i, 11 - i, P.LightGold);
        Registry.Add("A_frames", "ui_toggle_check", tick, 16, 16);

        // --- path dot
        var dot = Canvas.FromMap(8, 8, new[]
        {
            "........",
            "..tttt..",
            ".tccsst.",
            ".tcssst.",
            ".tssstt.",
            ".tsstt..",
            "..tttt..",
            "........",
        }, "ui_path_dot");
        dot.Replace(P.Tan, P.Tan);
        Registry.Add("A_frames", "ui_path_dot", dot, 8, 8);

        // --- "new" badge: 8-point gold burst, terracotta outline
        var badge = new Canvas(16, 16);
        badge.Star(8, 8, 6.6, 4.1, 8, P.Gold, -90);
        badge.Disc(8, 8, 3.3, P.LightGold);
        badge.Set(6, 6, P.Cream); badge.Set(7, 5, P.Cream);
        badge.Outline(P.Terracotta, true);
        Registry.Add("A_frames", "ui_badge_new", badge, 16, 16);
    }
}
