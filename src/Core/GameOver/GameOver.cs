using Godot;
using System.Collections.Generic;

public partial class GameOver : CanvasLayer
{
    [Export] public string CorvoFlavor = "O médico sucumbiu à praga que jurou combater.";
    [Export] public string SeuZeFlavor = "A feira fechou as portas para sempre.";

    public override void _Ready()
    {
        Layer = 50;
        ProcessMode = ProcessModeEnum.Always;

        bool isSeuZe = RunData.SelectedCharacter?.CharacterName == "Seu Zé";
        string flavor = isSeuZe ? SeuZeFlavor : CorvoFlavor;

        var font = GD.Load<Font>("res://Data/Fonte/citadel_of_blackrose/Enchanted Land.otf");

        // Collect stats now while player is still alive in the tree
        int relics = PlayerManager.Instance?.Player?.Relics?.Count ?? 0;
        int gold    = PlayerManager.Instance?.Player?.Gold ?? 0;
        int kills   = RunStats.EnemiesKilled;
        int tower   = RunStats.CurrentTower;
        int floor   = RunStats.CurrentFloor;

        // Root control blocks input to game behind it
        var root = new Control { ProcessMode = ProcessModeEnum.Always };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.MouseFilter = Control.MouseFilterEnum.Stop;
        AddChild(root);

        var bgBlack = new ColorRect { Color = Colors.Black };
        bgBlack.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(bgBlack);

        // Main VBox centrado verticalmente
        var vbox = new VBoxContainer { ProcessMode = ProcessModeEnum.Always };
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 18);
        root.AddChild(vbox);

        vbox.AddChild(Spacer(expand: true));

        // Título
        var title = new Label
        {
            Text = "Você tombou!",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        if (font is not null) title.AddThemeFontOverride("font", font);
        title.AddThemeFontSizeOverride("font_size", 72);
        title.AddThemeColorOverride("font_color",        new Color(0.85f, 0.10f, 0.10f, 1f));
        title.AddThemeColorOverride("font_shadow_color", new Color(0f,    0f,    0f,    1f));
        title.AddThemeConstantOverride("shadow_offset_x", 3);
        title.AddThemeConstantOverride("shadow_offset_y", 3);
        vbox.AddChild(title);

        // Flavor
        var flavorLabel = new Label
        {
            Text = flavor,
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.Word,
        };
        if (font is not null) flavorLabel.AddThemeFontOverride("font", font);
        flavorLabel.AddThemeFontSizeOverride("font_size", 28);
        flavorLabel.AddThemeColorOverride("font_color", new Color(0.88f, 0.78f, 0.60f, 1f));
        vbox.AddChild(flavorLabel);

        vbox.AddChild(Spacer(height: 32));

        // Painel de estatísticas (oculto por padrão) — centralizado com largura fixa
        var statsPanel = BuildStatsPanel(font, tower, floor, relics, gold, kills);
        statsPanel.Visible = false;
        var statsPanelRow = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        statsPanelRow.AddChild(statsPanel);
        vbox.AddChild(statsPanelRow);

        // Botão "Ver estatísticas"
        var statsBtn = BuildButton(font, "Ver estatísticas");
        var btnRowStats = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        btnRowStats.AddChild(statsBtn);
        vbox.AddChild(btnRowStats);

        bool statsVisible = false;
        statsBtn.Pressed += () =>
        {
            statsVisible = !statsVisible;
            statsPanelRow.Visible = statsVisible;
            statsBtn.Text = statsVisible ? "Esconder" : "Ver estatísticas";
        };

        vbox.AddChild(Spacer(height: 8));

        // Botão "Voltar ao Menu"
        var menuBtn = BuildButton(font, "Voltar ao Menu");
        var btnRowMenu = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        btnRowMenu.AddChild(menuBtn);
        vbox.AddChild(btnRowMenu);
        menuBtn.Pressed += OnBackToMenu;

        vbox.AddChild(Spacer(expand: true));

        // Fade de entrada
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

    // ── Stats panel ───────────────────────────────────────────────────────────

    private static PanelContainer BuildStatsPanel(
        Font font, int tower, int floor, int relics, int gold, int kills)
    {
        var panel = new PanelContainer();

        var panelStyle = new StyleBoxFlat
        {
            BgColor     = new Color(0.06f, 0.04f, 0.02f, 0.90f),
            BorderColor = new Color(0.55f, 0.42f, 0.12f),
            CornerRadiusTopLeft     = 8, CornerRadiusTopRight    = 8,
            CornerRadiusBottomLeft  = 8, CornerRadiusBottomRight = 8,
        };
        panelStyle.SetBorderWidthAll(2);
        panelStyle.SetContentMarginAll(20);
        panel.AddThemeStyleboxOverride("panel", panelStyle);

        var innerVBox = new VBoxContainer();
        innerVBox.AddThemeConstantOverride("separation", 10);
        panel.AddChild(innerVBox);

        bool miserable = floor <= 1 && kills == 0;

        var lines = miserable
            ? new[] { "Você falhou miseravelmente!" }
            : new[]
            {
                $"Torre:                        {tower} / 3",
                $"Andar:                       {floor}",
                $"Relíquias:                 {relics}",
                $"Ouro:                          {gold}",
                $"Inimigos eliminados:  {kills}",
            };

        foreach (var line in lines)
        {
            var lbl = new Label
            {
                Text = line,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            if (font is not null) lbl.AddThemeFontOverride("font", font);
            lbl.AddThemeFontSizeOverride("font_size", 24);
            lbl.AddThemeColorOverride("font_color", new Color(0.88f, 0.78f, 0.60f, 1f));
            innerVBox.AddChild(lbl);
        }

        panel.CustomMinimumSize = new Vector2(380, 0);
        panel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        return panel;
    }

    // ── Button builder ────────────────────────────────────────────────────────

    private static Button BuildButton(Font font, string text)
    {
        var btn = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(300, 60),
            ProcessMode = ProcessModeEnum.Always,
        };
        if (font is not null) btn.AddThemeFontOverride("font", font);
        btn.AddThemeFontSizeOverride("font_size", 26);
        btn.AddThemeColorOverride("font_color", new Color(0.95f, 0.88f, 0.65f, 1f));

        var sb = new StyleBoxFlat
        {
            BgColor     = new Color(0.10f, 0.07f, 0.03f, 0.93f),
            BorderColor = new Color(0.70f, 0.55f, 0.15f),
            CornerRadiusTopLeft     = 8, CornerRadiusTopRight    = 8,
            CornerRadiusBottomLeft  = 8, CornerRadiusBottomRight = 8,
        };
        sb.SetBorderWidthAll(2);

        var sbH = (StyleBoxFlat)sb.Duplicate();
        sbH.BgColor     = new Color(0.18f, 0.12f, 0.04f, 0.97f);
        sbH.BorderColor = new Color(1.00f, 0.82f, 0.28f);

        var sbP = (StyleBoxFlat)sb.Duplicate();
        sbP.BgColor = new Color(0.06f, 0.04f, 0.02f, 0.93f);

        btn.AddThemeStyleboxOverride("normal",  sb);
        btn.AddThemeStyleboxOverride("hover",   sbH);
        btn.AddThemeStyleboxOverride("pressed", sbP);
        return btn;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Control Spacer(bool expand = false, int height = 0)
    {
        var c = new Control();
        if (expand)  c.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        if (height > 0) c.CustomMinimumSize = new Vector2(0, height);
        return c;
    }

    private void OnBackToMenu()
    {
        QueueFree(); // remove before scene change so it doesn't linger on main menu
        if (GodotObject.IsInstanceValid(GameManager.Instance))
            GameManager.Instance.GoToMainMenu();
        else
            GetTree().ChangeSceneToFile("res://src/Core/Menu/MainMenu.tscn");
    }
}
