using Godot;

public partial class MainMenu : Control
{
    [Export] private PackedScene _characterSelectScene;

    private Control _titleGroup;
    private Control _buttonsGroup;

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

        PlayIntroAnimation();
        AnimateTitle();
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
