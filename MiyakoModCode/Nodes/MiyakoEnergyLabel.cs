using Godot;

namespace Tsukiyuki_Miyako.MiyakoModCode.Nodes;

/// <summary>
/// Label that draws its own text outline via _Draw(), bypassing the need for MSDF font support.
/// The built-in Label text is made transparent; all rendering is done manually.
/// </summary>
public partial class MiyakoEnergyLabel : Godot.Label
{
    private const int OutlineThickness = 2;

    public override void _Ready()
    {
        // Hide the Label's built-in text rendering
        AddThemeColorOverride("font_color", new Color(0, 0, 0, 0));
        AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0));
    }

    public override void _Draw()
    {
        if (string.IsNullOrEmpty(Text)) return;

        Font font = GetThemeFont("font");
        int fontSize = GetThemeFontSize("font_size");

        // Read colors from theme overrides that the scene already sets
        Color textColor = GetThemeColor("font_color");
        Color outlineColor = GetThemeColor("font_outline_color");

        // Fallback if the theme override is also transparent
        if (textColor.A < 0.01f) textColor = new Color(1.0f, 0.95686f, 0.87843f, 1.0f);
        if (outlineColor.A < 0.01f) outlineColor = new Color(0.102f, 0.102f, 0.8f, 1.0f);

        Vector2 labelSize = Size;

        // Vertical center offset
        float textHeight = font.GetHeight(fontSize);
        float yOffset = (labelSize.Y - textHeight) / 2.0f;

        Rid ci = GetCanvasItem();

        // Draw outline — 8‑direction stroke
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                Vector2 pos = new(dx * OutlineThickness, dy * OutlineThickness + yOffset);
                font.DrawString(ci, pos, Text,
                    HorizontalAlignment.Center, labelSize.X, fontSize, outlineColor);
            }
        }

        // Draw main text on top
        font.DrawString(ci, new Vector2(0, yOffset), Text,
            HorizontalAlignment.Center, labelSize.X, fontSize, textColor);
    }
}
