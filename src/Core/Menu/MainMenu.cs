using Godot;

public partial class MainMenu : Control
{
    [Export] private PackedScene _characterSelectScene;

    private Control _titleGroup;
    private Control _buttonsGroup;
    private Godot.Button _continueButton;

    private record MenuEntry(Godot.Button Btn, Godot.Panel Highlight);

    private MenuEntry[] _entries;

    public override void _Ready()
    {
        _titleGroup   = GetNodeOrNull<Control>("Background/TitleGroup");
        _buttonsGroup = GetNodeOrNull<Control>("Background/ButtonsGroup");

        _entries = new[]
        {
            new MenuEntry(
                GetNode<Godot.Button> ("Background/LoginPanel/VBox/PlayContainer/PlayButton"),
                GetNode<Godot.Panel>  ("Background/LoginPanel/VBox/PlayContainer/PlayHighlight")),
            new MenuEntry(
                GetNode<Godot.Button> ("Background/LoginPanel/VBox/SettingsContainer/SettingsButton"),
                GetNode<Godot.Panel>  ("Background/LoginPanel/VBox/SettingsContainer/SettingsHighlight")),
            new MenuEntry(
                GetNode<Godot.Button> ("Background/LoginPanel/VBox/QuitContainer/QuitButton"),
                GetNode<Godot.Panel>  ("Background/LoginPanel/VBox/QuitContainer/QuitHighlight")),
        };

        foreach (var e in _entries)
        {
            var entry = e;
            entry.Btn.MouseEntered += () => OnHoverEnter(entry);
            entry.Btn.MouseExited  += () => OnHoverExit(entry);
            entry.Btn.ButtonDown   += () => OnButtonDown(entry);
        }

        AddContinueButton();
        PlayIntroAnimation();
        AnimateTitle();
    }

    // ── Continue button ──────────────────────────────────────────────────────

    private void AddContinueButton()
    {
        var vbox = GetNodeOrNull<VBoxContainer>("Background/LoginPanel/VBox");
        if (vbox is null) return;

        bool hasSave = SaveManager.Instance?.HasSave() ?? false;

        var font = GD.Load<Font>("res://Data/Fonte/citadel_of_blackrose/Enchanted Land.otf");

        var container = new Control
        {
            CustomMinimumSize   = new Vector2(450, 64),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };

        var highlightStyle = new StyleBoxFlat
        {
            BgColor     = new Color(0.04f, 0.02f, 0f, 0.45f),
            BorderColor = new Color(1f, 0.75f, 0.15f, 1f),
            CornerRadiusTopLeft     = 10, CornerRadiusTopRight    = 10,
            CornerRadiusBottomLeft  = 10, CornerRadiusBottomRight = 10,
        };
        highlightStyle.SetBorderWidthAll(2);

        var highlight = new Godot.Panel
        {
            Modulate    = new Color(1, 1, 1, 0),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        highlight.AddThemeStyleboxOverride("panel", highlightStyle);
        container.AddChild(highlight);
        highlight.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        var btn = new Godot.Button { Text = "Continuar", Flat = true, Disabled = !hasSave };
        btn.AddThemeColorOverride("font_color",          new Color(0.95f, 0.88f, 0.65f, 1f));
        btn.AddThemeColorOverride("font_focus_color",    new Color(0.95f, 0.88f, 0.65f, 1f));
        btn.AddThemeColorOverride("font_pressed_color",  new Color(0.95f, 0.88f, 0.65f, 1f));
        btn.AddThemeColorOverride("font_hover_color",    new Color(0.95f, 0.88f, 0.65f, 1f));
        btn.AddThemeColorOverride("font_disabled_color", new Color(0.60f, 0.55f, 0.40f, 0.55f));
        if (font is not null) btn.AddThemeFontOverride("font", font);
        btn.AddThemeFontSizeOverride("font_size", 52);
        var empty = new StyleBoxEmpty();
        foreach (var state in new[] { "normal", "pressed", "hover", "disabled", "focus" })
            btn.AddThemeStyleboxOverride(state, empty);
        container.AddChild(btn);
        btn.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        var playContainer = GetNodeOrNull<Node>("Background/LoginPanel/VBox/PlayContainer");
        vbox.AddChild(container);
        if (playContainer is not null)
            vbox.MoveChild(container, playContainer.GetIndex() + 1);

        _continueButton = btn;

        if (!hasSave) return;

        var entry = new MenuEntry(btn, highlight);
        btn.MouseEntered += () => OnHoverEnter(entry);
        btn.MouseExited  += () => OnHoverExit(entry);
        btn.ButtonDown   += () => OnButtonDown(entry);
        btn.Pressed      += OnContinuePressed;
    }

    private void OnContinuePressed()
    {
        var save = SaveManager.Instance?.LoadRun();
        if (save is null) return;

        RunData.SelectedCharacter = ResolveCharacterByName(save.CharacterName);
        RunData.PendingLoad       = save;

        if (save.CharacterName == "Seu Zé")
            CursorManager.Instance?.SetSeuZe();
        else
            CursorManager.Instance?.SetCorvo();

        GetTree().ChangeSceneToFile("res://src/Core/GameManager/GameManager.tscn");
    }

    private static CharacterData ResolveCharacterByName(string name)
    {
        using var dir = DirAccess.Open("res://Data/Characters");
        if (dir is not null)
        {
            dir.ListDirBegin();
            var file = dir.GetNext();
            while (!string.IsNullOrEmpty(file))
            {
                if (!dir.CurrentIsDir() && file.EndsWith(".tres"))
                {
                    var c = GD.Load<CharacterData>($"res://Data/Characters/{file}");
                    if (c?.CharacterName == name) { dir.ListDirEnd(); return c; }
                }
                file = dir.GetNext();
            }
            dir.ListDirEnd();
        }
        return new CharacterData { CharacterName = name };
    }

    // ── Hover effects ────────────────────────────────────────────────────────

    private void OnHoverEnter(MenuEntry e)
    {
        e.Btn.PivotOffset = e.Btn.Size / 2;

        var t = CreateTween().SetParallel();
        t.TweenProperty(e.Highlight, "modulate:a", 1.0f, 0.15f)
         .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        t.TweenProperty(e.Btn, "scale", new Vector2(1.08f, 1.08f), 0.12f)
         .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        t.TweenProperty(e.Btn, "modulate", new Color(1.0f, 0.82f, 0.25f, 1f), 0.12f);
    }

    private void OnHoverExit(MenuEntry e)
    {
        var t = CreateTween().SetParallel();
        t.TweenProperty(e.Highlight, "modulate:a", 0.0f, 0.18f)
         .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        t.TweenProperty(e.Btn, "scale", Vector2.One, 0.15f)
         .SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        t.TweenProperty(e.Btn, "modulate", Colors.White, 0.15f);
    }

    private void OnButtonDown(MenuEntry e)
    {
        e.Btn.PivotOffset = e.Btn.Size / 2;
        var t = CreateTween();
        t.TweenProperty(e.Btn, "scale", new Vector2(0.92f, 0.92f), 0.06f);
        t.TweenProperty(e.Btn, "scale", new Vector2(1.08f, 1.08f), 0.06f);
    }

    // ── Title pulse ──────────────────────────────────────────────────────────

    private void AnimateTitle()
    {
        var title = GetNodeOrNull<Label>("Background/LoginPanel/VBox/GameTitle");
        if (title is null) return;

        var tween = CreateTween().SetLoops();
        tween.TweenProperty(title, "modulate", new Color(1f, 0.95f, 0.7f, 1f), 1.8f)
             .SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(title, "modulate", Colors.White, 1.8f)
             .SetTrans(Tween.TransitionType.Sine);
    }

    // ── Intro animation ──────────────────────────────────────────────────────

    private async void PlayIntroAnimation()
    {
        if (_titleGroup is not null)
        {
            _titleGroup.Modulate = new Color(1, 1, 1, 0);
            _titleGroup.Position += new Vector2(0, -30);

            var tween = CreateTween();
            tween.TweenProperty(_titleGroup, "modulate:a", 1f, 0.8f)
                 .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
            tween.Parallel().TweenProperty(_titleGroup, "position:y",
                 _titleGroup.Position.Y + 30, 0.8f)
                 .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        if (_buttonsGroup is not null)
        {
            _buttonsGroup.Modulate = new Color(1, 1, 1, 0);
            var tween2 = CreateTween();
            tween2.TweenProperty(_buttonsGroup, "modulate:a", 1f, 0.5f)
                  .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        }
    }

    // ── Button callbacks ─────────────────────────────────────────────────────

    public void OnPlayPressed()
    {
        _characterSelectScene ??= GD.Load<PackedScene>("res://src/Core/CharacterSelect/CharacterSelect.tscn");

        if (_characterSelectScene is null)
        {
            GD.PushError("MainMenu: CharacterSelect.tscn não encontrado.");
            return;
        }

        GetTree().ChangeSceneToPacked(_characterSelectScene);
    }

    public void OnSettingsPressed()
    {
        UI.Instance.Settings.Open(false);
    }

    public void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
