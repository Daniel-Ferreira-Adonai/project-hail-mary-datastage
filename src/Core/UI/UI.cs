using Godot;
using System;
using System.Threading.Tasks;

public partial class UI : CanvasLayer
{
    public static UI Instance { get; private set; }
    public TopHud TopHud { get; private set; }

    [Export] private ColorRect _fadeRect;

    public override void _Ready()
    {
        Instance = this;
        Layer = 10;
        FollowViewportEnabled = false;

        TopHud = GetNodeOrNull<TopHud>("TopHud");
    }

    public async Task FadeOut(float duration = 0.4f)
    {
        _fadeRect.MouseFilter = Control.MouseFilterEnum.Stop;

        var tween = CreateTween();

        tween.TweenProperty(
            _fadeRect,
            "color",
            new Color(0, 0, 0, 1),
            duration
        )
        .SetTrans(Tween.TransitionType.Sine)
        .SetEase(Tween.EaseType.InOut);

        await ToSignal(tween, Tween.SignalName.Finished);
    }

    public async void FadeIn(float duration = 0.4f)
    {
        var tween = CreateTween();

        tween.TweenProperty(
            _fadeRect,
            "color",
            new Color(0, 0, 0, 0),
            duration
        )
        .SetTrans(Tween.TransitionType.Sine)
        .SetEase(Tween.EaseType.InOut);

        await ToSignal(tween, Tween.SignalName.Finished);

        _fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    public void AddUI(Control control)
    {
        AddChild(control);
    }
}
