using Godot;

public partial class DrawPileUI : Control
{
    [Export] public Texture2D PileIcon;
    private const string IconPath = "res://Test/TestImagesSprites/art/DeckPile.png";

    private TextureRect _iconRect;
    private Panel _placeholder;
    private Label _countLabel;

    private const int IconW = 92, IconH = 56, Badge = 28;

    public override void _Ready()
    {
        AnchorLeft = 0; AnchorTop = 1; AnchorRight = 0; AnchorBottom = 1;
        OffsetLeft = 20; OffsetTop = -(IconH + 20); OffsetRight = 20 + IconW; OffsetBottom = -20;
        MouseFilter = MouseFilterEnum.Ignore;

        _placeholder = new Panel { MouseFilter = MouseFilterEnum.Ignore };
        _placeholder.SetAnchorsPreset(LayoutPreset.FullRect);
        var sb = new StyleBoxFlat
        {
            BgColor = new Color(0.06f, 0.04f, 0.02f, 0.95f),
            BorderColor = new Color(1f, 0.75f, 0.15f),
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
        };
        sb.SetBorderWidthAll(2);
        _placeholder.AddThemeStyleboxOverride("panel", sb);
        AddChild(_placeholder);

        _iconRect = new TextureRect
        {
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
            Visible = false,
        };
        _iconRect.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(_iconRect);

        Texture2D icon = PileIcon;
        if (icon == null && ResourceLoader.Exists(IconPath))
            icon = GD.Load<Texture2D>(IconPath);
        if (icon != null)
        {
            _iconRect.Texture = icon;
            _iconRect.Visible = true;
            _placeholder.Visible = false;
        }

        var badge = new Panel
        {
            Size = new Vector2(Badge, Badge),
            Position = new Vector2(IconW - Badge + 8, -8),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        var bsb = new StyleBoxFlat
        {
            BgColor = new Color(0.72f, 0.12f, 0.12f),
            BorderColor = new Color(0.1f, 0.05f, 0.05f),
            CornerRadiusTopLeft = Badge / 2, CornerRadiusTopRight = Badge / 2,
            CornerRadiusBottomLeft = Badge / 2, CornerRadiusBottomRight = Badge / 2,
        };
        bsb.SetBorderWidthAll(2);
        badge.AddThemeStyleboxOverride("panel", bsb);
        AddChild(badge);

        _countLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
            Text = "0",
        };
        _countLabel.SetAnchorsPreset(LayoutPreset.FullRect);
        _countLabel.AddThemeFontSizeOverride("font_size", 14);
        _countLabel.AddThemeColorOverride("font_color", Colors.White);
        badge.AddChild(_countLabel);
    }

    public void SetCount(int n)
    {
        if (_countLabel != null)
            _countLabel.Text = n.ToString();
    }

    public Vector2 GetCenter()
    {
        var r = GetGlobalRect();
        if (r.Size == Vector2.Zero)
            return GlobalPosition + new Vector2(IconW, IconH) * 0.5f;
        return r.GetCenter();
    }
}
