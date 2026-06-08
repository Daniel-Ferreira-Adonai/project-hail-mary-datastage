using Godot;

public partial class VictoryScreen : CanvasLayer
{
    public override void _Ready()
    {
        Layer       = 50;
        ProcessMode = ProcessModeEnum.Always;

        var font = GD.Load<Font>("res://Data/Fonte/citadel_of_blackrose/Enchanted Land.otf");

        var root = new Control { ProcessMode = ProcessModeEnum.Always };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.MouseFilter = Control.MouseFilterEnum.Stop;
        AddChild(root);

        var bgBlack = new ColorRect { Color = Colors.Black };
        bgBlack.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(bgBlack);

        var vbox = new VBoxContainer { ProcessMode = ProcessModeEnum.Always };
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 18);
        root.AddChild(vbox);

        vbox.AddChild(Spacer(expand: true));

        var title = new Label
        {
            Text = "Você venceu!",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        if (font is not null) title.AddThemeFontOverride("font", font);
        title.AddThemeFontSizeOverride("font_size", 72);
        title.AddThemeColorOverride("font_color",        new Color(1f,   0.82f, 0.15f, 1f));
        title.AddThemeColorOverride("font_shadow_color", new Color(0f,   0f,    0f,    1f));
        title.AddThemeConstantOverride("shadow_offset_x", 3);
        title.AddThemeConstantOverride("shadow_offset_y", 3);
        vbox.AddChild(title);

        vbox.AddChild(Spacer(height: 8));

        var msg = new Label
        {
            Text = "Parabéns! Você conquistou o poder supremo de controlar o loop temporal.",
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.Word,
        };
        if (font is not null) msg.AddThemeFontOverride("font", font);
        msg.AddThemeFontSizeOverride("font_size", 28);
        msg.AddThemeColorOverride("font_color", new Color(0.88f, 0.78f, 0.60f, 1f));
        vbox.AddChild(msg);

        vbox.AddChild(Spacer(height: 40));

        var encerrarBtn  = BuildButton(font, "Encerrar",         new Color(0.55f, 0.10f, 0.10f));
        var continuarBtn = BuildButton(font, "Continuar jogando", new Color(0.12f, 0.45f, 0.12f));
        continuarBtn.CustomMinimumSize = new Vector2(340, 60);

        var btnRow = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        btnRow.AddThemeConstantOverride("separation", 24);
        btnRow.AddChild(encerrarBtn);
        btnRow.AddChild(continuarBtn);
        vbox.AddChild(btnRow);

        encerrarBtn.Pressed  += OnEncerrar;
        continuarBtn.Pressed += () => OnContinueEndless(encerrarBtn, continuarBtn);

        vbox.AddChild(Spacer(expand: true));

        // Fade-in from black (same pattern as GameOver)
        var fadeRect = new ColorRect
        {
            Color       = Colors.Black,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(fadeRect);

        var tween = CreateTween();
        tween.TweenProperty(fadeRect, "color", new Color(0f, 0f, 0f, 0f), 1.0f)
             .SetTrans(Tween.TransitionType.Sine);
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────

    private void OnEncerrar() => GetTree().Quit();

    private void OnContinueEndless(Button encerrar, Button continuar)
    {
        encerrar.Disabled  = true;
        continuar.Disabled = true;
        RunData.Endless    = true;
        QueueFree();
        GameManager.Instance?.GenerateNextTower();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Button BuildButton(Font font, string text, Color bg)
    {
        var btn = new Button
        {
            Text              = text,
            CustomMinimumSize = new Vector2(240, 60),
            ProcessMode       = ProcessModeEnum.Always,
        };
        if (font is not null) btn.AddThemeFontOverride("font", font);
        btn.AddThemeFontSizeOverride("font_size", 26);
        btn.AddThemeColorOverride("font_color", new Color(0.95f, 0.88f, 0.65f, 1f));

        var sb = new StyleBoxFlat
        {
            BgColor                = bg,
            BorderColor            = bg.Lightened(0.3f),
            CornerRadiusTopLeft    = 8, CornerRadiusTopRight    = 8,
            CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
        };
        sb.SetBorderWidthAll(2);

        var sbH = (StyleBoxFlat)sb.Duplicate(); sbH.BgColor = bg.Lightened(0.2f);
        var sbP = (StyleBoxFlat)sb.Duplicate(); sbP.BgColor = bg.Darkened(0.15f);

        btn.AddThemeStyleboxOverride("normal",  sb);
        btn.AddThemeStyleboxOverride("hover",   sbH);
        btn.AddThemeStyleboxOverride("pressed", sbP);
        return btn;
    }

    private static Control Spacer(bool expand = false, int height = 0)
    {
        var c = new Control();
        if (expand)     c.SizeFlagsVertical  = Control.SizeFlags.ExpandFill;
        if (height > 0) c.CustomMinimumSize  = new Vector2(0, height);
        return c;
    }
}
