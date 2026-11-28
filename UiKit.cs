using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.ComponentModel;

namespace YMH219_SpeechApp;

internal sealed class SurfacePanel : Panel
{
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 16;
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color OutlineColor { get; set; } = Color.FromArgb(224, 229, 231);
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DrawShadow { get; set; } = true;

    public SurfacePanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.White;
        Padding = new Padding(16);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? SystemColors.Control);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(3, 3, Math.Max(1, Width - 7), Math.Max(1, Height - 8));
        if (DrawShadow)
        {
            for (var i = 3; i >= 1; i--)
            {
                using var shadow = new SolidBrush(Color.FromArgb(9 + i * 3, 15, 25, 32));
                using var path = Rounded(new Rectangle(bounds.X, bounds.Y + i, bounds.Width, bounds.Height), CornerRadius);
                g.FillPath(shadow, path);
            }
        }

        using var surface = new SolidBrush(BackColor);
        using var shape = Rounded(bounds, CornerRadius);
        g.FillPath(surface, shape);
        using var outline = new Pen(OutlineColor, 1);
        g.DrawPath(outline, shape);
        base.OnPaint(e);
    }

    private static GraphicsPath Rounded(Rectangle rect, int radius)
    {
        var r = Math.Clamp(radius, 1, Math.Min(rect.Width, rect.Height) / 2);
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
        path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
        path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
        path.CloseFigure();
        return path;
    }
}

internal sealed class DepthButton : Button
{
    private bool _hovered;
    private bool _pressed;

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 11;

    public DepthButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;
        Cursor = Cursors.Hand;
        Height = 42;
        Font = new Font("Segoe UI", 9, FontStyle.Bold);
        AccessibleRole = AccessibleRole.PushButton;
        TabStop = true;
    }

    protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hovered = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        var shift = _pressed ? 1 : 0;
        var bounds = new Rectangle(1, 1 + shift, Math.Max(1, Width - 3), Math.Max(1, Height - 5));
        var baseColor = Enabled ? BackColor : Color.FromArgb(205, 210, 215);
        if (!_pressed && Enabled)
        {
            using var shadow = new SolidBrush(Color.FromArgb(42, 18, 29, 38));
            using var shadowPath = Rounded(new Rectangle(bounds.X, bounds.Y + 3, bounds.Width, bounds.Height), CornerRadius);
            g.FillPath(shadow, shadowPath);
        }

        var top = _hovered && Enabled ? ControlPaint.Light(baseColor, .13f) : ControlPaint.Light(baseColor, .06f);
        var bottom = _pressed ? ControlPaint.Dark(baseColor, .06f) : baseColor;
        using (var fill = new LinearGradientBrush(bounds, top, bottom, LinearGradientMode.Vertical))
        using (var path = Rounded(bounds, CornerRadius))
        {
            g.FillPath(fill, path);
            using var edge = new Pen(Color.FromArgb(45, 255, 255, 255), 1);
            g.DrawPath(edge, path);
        }

        var textColor = Enabled ? ForeColor : Color.FromArgb(116, 124, 132);
        TextRenderer.DrawText(g, Text, Font, bounds, textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);

        if (Focused && ShowFocusCues)
        {
            var luminance = .2126 * BackColor.R + .7152 * BackColor.G + .0722 * BackColor.B;
            var focusColor = luminance > 150 ? Color.FromArgb(235, 26, 47, 62) : Color.FromArgb(235, 255, 255, 255);
            using var focus = new Pen(focusColor, 2) { DashStyle = DashStyle.Dot };
            using var focusPath = Rounded(Rectangle.Inflate(bounds, -5, -5), Math.Max(2, CornerRadius - 4));
            g.DrawPath(focus, focusPath);
        }
    }

    private static GraphicsPath Rounded(Rectangle rect, int radius)
    {
        var r = Math.Clamp(radius, 1, Math.Min(rect.Width, rect.Height) / 2);
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
        path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
        path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
        path.CloseFigure();
        return path;
    }
}

