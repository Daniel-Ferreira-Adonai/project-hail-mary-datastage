using Godot;

public partial class IntroScene : Control
{
    private VideoStreamPlayer _video;
    private bool              _transitioning;

    private const string VideoPath    = "res://Test/TestImagesSprites/art/IntroDoGame.mp4";
    private const string MainMenuPath = "res://src/Core/Menu/MainMenu.tscn";

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        var bg = new ColorRect { Color = Colors.Black };
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        var stream = ResourceLoader.Load(VideoPath) as VideoStream;
        if (stream != null)
        {
            _video = new VideoStreamPlayer { Stream = stream, Expand = true, Autoplay = false };
            _video.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(_video);
            _video.Finished += GoToMainMenu;
            _video.Play();

            var hint = new Label
            {
                Text                = "SPACE  —  pular",
                HorizontalAlignment = HorizontalAlignment.Center,
                AnchorLeft          = 0f,
                AnchorTop           = 1f,
                AnchorRight         = 1f,
                AnchorBottom        = 1f,
                OffsetTop           = -54f,
                OffsetBottom        = -14f,
            };
            hint.AddThemeFontSizeOverride("font_size", 20);
            hint.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.4f));
            AddChild(hint);
        }
        else
        {
            CallDeferred(nameof(GoToMainMenu));
        }
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Space })
            GoToMainMenu();
    }

    private void GoToMainMenu()
    {
        if (_transitioning) return;
        _transitioning = true;
        _video?.Stop();
        GetTree().ChangeSceneToFile(MainMenuPath);
    }
}
