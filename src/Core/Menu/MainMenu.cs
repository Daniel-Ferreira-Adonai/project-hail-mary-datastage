using Godot;

public partial class MainMenu : Control
{
    [Export] private PackedScene _characterSelectScene;

    private Control _titleGroup;
    private Control _buttonsGroup;

    public override void _Ready()
    {
        _titleGroup   = GetNodeOrNull<Control>("Background/TitleGroup");
        _buttonsGroup = GetNodeOrNull<Control>("Background/ButtonsGroup");

        PlayIntroAnimation();
        AnimateBackground();
    }

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

    private void AnimateBackground()
    {
        var bg = GetNodeOrNull<ColorRect>("Background/GradientOverlay");
        if (bg is null) return;

        var tween = CreateTween().SetLoops();
        tween.TweenProperty(bg, "color", new Color(0.05f, 0.02f, 0.12f, 0.85f), 4f)
             .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(bg, "color", new Color(0.12f, 0.03f, 0.08f, 0.85f), 4f)
             .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
    }

    public void OnPlayPressed()
    {
        _characterSelectScene ??= GD.Load<PackedScene>("res://src/Core/CharacterSelect/CharacterSelect.tscn");

        if (_characterSelectScene is null)
        {
            GD.PushError("MainMenu: CharacterSelect.tscn não encontrado em res://src/Core/CharacterSelect/CharacterSelect.tscn");
            return;
        }

        GetTree().ChangeSceneToPacked(_characterSelectScene);
    }

    public void OnSettingsPressed()
    {
        var comingSoonScene = GD.Load<PackedScene>("res://src/Core/Menu/ComingSoon.tscn");
        if (comingSoonScene is null) return;

        var popup = comingSoonScene.Instantiate<ComingSoon>();
        AddChild(popup);
    }

    public void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
