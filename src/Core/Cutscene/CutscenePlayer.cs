using Godot;
using System.Threading.Tasks;

public partial class CutscenePlayer : CanvasLayer
{
    [Signal] public delegate void CutsceneFinishedEventHandler();

    private TextureRect _imageRect;
    private Panel _textPanel;
    private Label _narrativeLabel;
    private Button _skipButton;
    private ColorRect _fadeRect;

    public string PendingSeenId { get; set; } = "";

    private CutsceneData _data;
    private bool _skipping;
    private Tween _kenBurnsTween;

    private float _panelH;
    private float _vpY;

    public override void _Ready()
    {
        Layer = 50;
        BuildUI();
        _skipButton.Pressed += OnSkip;
    }

    private void BuildUI()
    {
        var vp = GetViewport().GetVisibleRect().Size;
        _panelH = 160f;
        _vpY    = vp.Y;

        var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(root);

        // ── background ────────────────────────────────────────────────────────
        var bg = new ColorRect { Color = Colors.Black, MouseFilter = Control.MouseFilterEnum.Ignore };
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(bg);

        // ── slide image ───────────────────────────────────────────────────────
        _imageRect = new TextureRect
        {
            ExpandMode  = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            Size        = vp,
            Position    = Vector2.Zero,
            PivotOffset = vp / 2f,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        root.AddChild(_imageRect);

        // ── fade rect — added BEFORE text so text always renders on top ───────
        _fadeRect = new ColorRect { Color = Colors.Black, MouseFilter = Control.MouseFilterEnum.Ignore };
        _fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(_fadeRect);

        // ── text panel — starts above the screen, slides down per slide ────────
        _textPanel = new Panel
        {
            Size        = new Vector2(vp.X, _panelH),
            Position    = new Vector2(0f, -_panelH),  // off-screen initially
            Modulate    = Colors.Transparent,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        var panelStyle = new StyleBoxFlat { BgColor = new Color(0f, 0f, 0f, 0.72f) };
        panelStyle.SetContentMarginAll(20);
        _textPanel.AddThemeStyleboxOverride("panel", panelStyle);
        root.AddChild(_textPanel);

        var font = GD.Load<Font>("res://Data/Fonte/citadel_of_blackrose/Enchanted Land.otf");
        _narrativeLabel = new Label
        {
            AutowrapMode        = TextServer.AutowrapMode.Word,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            Size                = new Vector2(vp.X - 40f, _panelH - 40f),
            Position            = new Vector2(20f, 20f),
            MouseFilter         = Control.MouseFilterEnum.Ignore,
        };
        if (font is not null) _narrativeLabel.AddThemeFontOverride("font", font);
        _narrativeLabel.AddThemeFontSizeOverride("font_size", 30);
        _narrativeLabel.AddThemeColorOverride("font_color", new Color(0.91f, 0.835f, 0.639f));
        _textPanel.AddChild(_narrativeLabel);

        // ── skip button ───────────────────────────────────────────────────────
        _skipButton = new Button
        {
            Text              = "[ PULAR ]",
            CustomMinimumSize = new Vector2(130f, 38f),
            Position          = new Vector2(vp.X - 150f, 20f),
        };
        if (font is not null) _skipButton.AddThemeFontOverride("font", font);
        _skipButton.AddThemeFontSizeOverride("font_size", 14);
        _skipButton.AddThemeColorOverride("font_color", new Color(0.667f, 0.667f, 0.667f));
        var skipStyle = new StyleBoxFlat
        {
            BgColor     = new Color(0f, 0f, 0f, 0.5f),
            BorderColor = new Color(0.5f, 0.5f, 0.5f, 0.8f),
        };
        skipStyle.SetBorderWidthAll(1);
        _skipButton.AddThemeStyleboxOverride("normal", skipStyle);
        root.AddChild(_skipButton);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Play(CutsceneData data)
    {
        _data = data;
        PlaySlidesAsync();
    }

    // ── Slide loop ────────────────────────────────────────────────────────────

    private async void PlaySlidesAsync()
    {
        for (int i = 0; i < _data.Slides.Length; i++)
        {
            if (_skipping || !IsInsideTree()) return;
            await PlaySlide(_data.Slides[i], i);
        }

        if (!_skipping && IsInsideTree())
            await FinalizeCutscene();
    }

    private async Task PlaySlide(CutsceneSlideData slide, int index)
    {
        // Reset image and hide text before revealing
        _imageRect.Texture   = slide.Image;
        _narrativeLabel.Text = slide.NarrativeText;
        _imageRect.Scale     = Vector2.One;
        _imageRect.Position  = Vector2.Zero;
        _textPanel.Modulate  = Colors.Transparent;
        _textPanel.Position  = new Vector2(0f, -_panelH);

        // Fade image in (fadeRect: opaque → transparent)
        await FadeRect(1f, 0f, 0.4f);
        if (_skipping || !IsInsideTree()) return;

        // Ken Burns runs for the full slide duration
        StartKenBurns(index, slide.Duration);

        // Text slides down from above (over the image, above fadeRect)
        var textIn = CreateTween().SetParallel();
        textIn.TweenProperty(_textPanel, "modulate:a", 1f, 0.3f)
              .SetTrans(Tween.TransitionType.Sine);
        textIn.TweenProperty(_textPanel, "position:y", 0f, 0.3f)
              .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        await ToSignal(textIn, Tween.SignalName.Finished);
        if (_skipping || !IsInsideTree()) return;

        // Hold for slide duration
        await ToSignal(GetTree().CreateTimer(slide.Duration), SceneTreeTimer.SignalName.Timeout);
        if (_skipping || !IsInsideTree()) return;

        // Text slides back up
        var textOut = CreateTween().SetParallel();
        textOut.TweenProperty(_textPanel, "modulate:a", 0f, 0.25f)
               .SetTrans(Tween.TransitionType.Sine);
        textOut.TweenProperty(_textPanel, "position:y", -_panelH, 0.25f)
               .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
        await ToSignal(textOut, Tween.SignalName.Finished);
        if (_skipping || !IsInsideTree()) return;

        _kenBurnsTween?.Kill();

        // Fade to black
        await FadeRect(0f, 1f, 0.4f);
    }

    private void StartKenBurns(int slideIndex, float duration)
    {
        _kenBurnsTween?.Kill();
        _kenBurnsTween = CreateTween().SetParallel();

        float dir = (slideIndex % 2 == 0) ? 1f : -1f;

        _kenBurnsTween
            .TweenProperty(_imageRect, "scale", new Vector2(1.08f, 1.08f), duration)
            .SetTrans(Tween.TransitionType.Linear);

        _kenBurnsTween
            .TweenProperty(_imageRect, "position:x", dir * 15f, duration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    // ── Finalization ──────────────────────────────────────────────────────────

    private async Task FinalizeCutscene()
    {
        await FadeRect(0f, 1f, 0.6f);
        if (!IsInsideTree()) return;

        if (!string.IsNullOrEmpty(PendingSeenId))
            MetaProgress.MarkCutsceneSeen(PendingSeenId);

        EmitSignal(SignalName.CutsceneFinished);
        SceneLoader.Instance?.GoTo(_data.NextScene);
    }

    // ── Skip ──────────────────────────────────────────────────────────────────

    private async void OnSkip()
    {
        if (_skipping || !IsInsideTree()) return;
        _skipping = true;
        _kenBurnsTween?.Kill();
        _textPanel.Modulate = Colors.Transparent;

        await FadeRect(_fadeRect.Color.A, 1f, 0.4f);
        if (!IsInsideTree()) return;

        if (!string.IsNullOrEmpty(PendingSeenId))
            MetaProgress.MarkCutsceneSeen(PendingSeenId);

        SceneLoader.Instance?.GoTo(_data.NextScene);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task FadeRect(float fromA, float toA, float duration)
    {
        if (!IsInsideTree()) return;
        var c = _fadeRect.Color;
        c.A = fromA;
        _fadeRect.Color = c;

        var tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", toA, duration)
             .SetTrans(Tween.TransitionType.Sine);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}
