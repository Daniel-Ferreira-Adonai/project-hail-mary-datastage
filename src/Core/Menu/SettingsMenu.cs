using Godot;

public partial class SettingsMenu : CanvasLayer
{
    private CheckButton _fullscreenToggle;
    private HSlider     _musicSlider, _sfxSlider;
    private Label       _musicValueLabel, _sfxValueLabel;
    private Button      _backToMenuBtn, _closeBtn;
    private bool        _inGame;

    private const string CfgPath = "user://settings.cfg";
    private const string Sect    = "settings";

    // ── Bootstrap (called from MusicManager at startup) ───────────────────────

    public static void Bootstrap()
    {
        EnsureBus("Music");
        EnsureBus("SFX");
        ApplyStoredSettings();
    }

    private static void EnsureBus(string name)
    {
        if (AudioServer.GetBusIndex(name) >= 0) return;
        int idx = AudioServer.BusCount;
        AudioServer.AddBus(idx);
        AudioServer.SetBusName(idx, name);
        AudioServer.SetBusSend(idx, "Master");
    }

    private static void ApplyStoredSettings()
    {
        var cfg = new ConfigFile();
        if (cfg.Load(CfgPath) != Error.Ok) return;

        SetBusVolume("Music", (float)cfg.GetValue(Sect, "music_vol", 1.0f));
        SetBusVolume("SFX",   (float)cfg.GetValue(Sect, "sfx_vol",   1.0f));

        bool fullscr = (bool)cfg.GetValue(Sect, "fullscreen", true);
        var  target  = fullscr
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed;
        if (DisplayServer.WindowGetMode() != target)
            DisplayServer.WindowSetMode(target);
    }

    public static void SetBusVolume(string busName, float linear)
    {
        int idx = AudioServer.GetBusIndex(busName);
        if (idx < 0) return;
        AudioServer.SetBusVolumeDb(idx,
            linear <= 0.001f ? -80f : Mathf.LinearToDb(linear));
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override void _Ready()
    {
        Layer       = 50;
        ProcessMode = ProcessModeEnum.Always;
        Visible     = false;
        BuildUI();
    }

    private void BuildUI()
    {
        var overlay = new ColorRect
        {
            Color       = new Color(0f, 0f, 0f, 0.65f),
            MouseFilter = Control.MouseFilterEnum.Stop,
        };
        overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(overlay);

        var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(root);

        const float pw = 560f, ph = 490f;
        var panel = new Panel
        {
            Size     = new Vector2(pw, ph),
            Position = new Vector2((1920f - pw) * 0.5f, (1080f - ph) * 0.5f),
        };
        var psb = new StyleBoxFlat
        {
            BgColor              = new Color(0.06f, 0.04f, 0.02f, 0.97f),
            BorderColor          = new Color(1f, 0.75f, 0.15f),
            CornerRadiusTopLeft  = 10, CornerRadiusTopRight     = 10,
            CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10,
        };
        psb.SetBorderWidthAll(3);
        panel.AddThemeStyleboxOverride("panel", psb);
        root.AddChild(panel);

        float y = 28f;

        var title = new Label
        {
            Text                = "CONFIGURAÇÕES",
            HorizontalAlignment = HorizontalAlignment.Center,
            Position            = new Vector2(0, y),
            Size                = new Vector2(pw, 48f),
        };
        title.AddThemeFontSizeOverride("font_size", 30);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.82f, 0.2f));
        panel.AddChild(title);
        y += 62f;

        panel.AddChild(Divider(pw, y)); y += 18f;

        // Fullscreen
        panel.AddChild(RowLabel("Tela Cheia", y));
        _fullscreenToggle = new CheckButton
        {
            Position      = new Vector2(pw - 130f, y - 4f),
            Size          = new Vector2(120f, 36f),
            ButtonPressed = IsFullscreen(),
        };
        _fullscreenToggle.AddThemeColorOverride("font_color",        Colors.White);
        _fullscreenToggle.AddThemeColorOverride("font_pressed_color", new Color(1f, 0.82f, 0.2f));
        _fullscreenToggle.Toggled += on =>
        {
            if (on)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
            else
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                var screen  = DisplayServer.ScreenGetSize();
                var winSize = new Vector2I(screen.X * 3 / 4, screen.Y * 3 / 4);
                DisplayServer.WindowSetSize(winSize);
                DisplayServer.WindowSetPosition(new Vector2I(
                    (screen.X - winSize.X) / 2,
                    (screen.Y - winSize.Y) / 2));
            }
        };
        panel.AddChild(_fullscreenToggle);
        y += 54f;

        panel.AddChild(Divider(pw, y)); y += 18f;

        // Music
        panel.AddChild(RowLabel("Volume Música", y));
        _musicValueLabel = ValueLabel(new Vector2(pw - 72f, y));
        panel.AddChild(_musicValueLabel);
        y += 34f;
        _musicSlider = MakeSlider(new Vector2(40f, y), pw - 80f);
        _musicSlider.ValueChanged += v =>
        {
            SetBusVolume("Music", (float)v);
            RefreshLabel(_musicValueLabel, (float)v);
        };
        panel.AddChild(_musicSlider);
        y += 52f;

        // SFX
        panel.AddChild(RowLabel("Volume SFX", y));
        _sfxValueLabel = ValueLabel(new Vector2(pw - 72f, y));
        panel.AddChild(_sfxValueLabel);
        y += 34f;
        _sfxSlider = MakeSlider(new Vector2(40f, y), pw - 80f);
        _sfxSlider.ValueChanged += v =>
        {
            SetBusVolume("SFX", (float)v);
            RefreshLabel(_sfxValueLabel, (float)v);
        };
        panel.AddChild(_sfxSlider);
        y += 58f;

        panel.AddChild(Divider(pw, y)); y += 18f;

        _backToMenuBtn = StyledButton("Voltar ao Menu", new Vector2(40f, y), 200f,
            new Color(0.60f, 0.10f, 0.10f));
        _backToMenuBtn.Pressed += OnBackToMenu;
        panel.AddChild(_backToMenuBtn);

        _closeBtn = StyledButton("Fechar", new Vector2(pw - 178f, y), 138f,
            new Color(0.15f, 0.48f, 0.15f));
        _closeBtn.Pressed += OnClose;
        panel.AddChild(_closeBtn);
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    private static ColorRect Divider(float pw, float y) => new ColorRect
    {
        Color    = new Color(1f, 0.75f, 0.15f, 0.3f),
        Position = new Vector2(30f, y),
        Size     = new Vector2(pw - 60f, 2f),
    };

    private static Label RowLabel(string text, float y) => new Label
    {
        Text     = text,
        Position = new Vector2(40f, y),
        Size     = new Vector2(300f, 32f),
    };

    private static Label ValueLabel(Vector2 pos) => new Label
    {
        Text                = "100%",
        Position            = pos,
        Size                = new Vector2(60f, 30f),
        HorizontalAlignment = HorizontalAlignment.Right,
    };

    private static HSlider MakeSlider(Vector2 pos, float width) => new HSlider
    {
        MinValue = 0.0,
        MaxValue = 1.0,
        Step     = 0.01,
        Value    = 1.0,
        Position = pos,
        Size     = new Vector2(width, 30f),
    };

    private static Button StyledButton(string text, Vector2 pos, float w, Color bg)
    {
        var btn = new Button { Text = text, Position = pos, Size = new Vector2(w, 44f) };
        var sb  = new StyleBoxFlat
        {
            BgColor                = bg,
            CornerRadiusTopLeft    = 6, CornerRadiusTopRight    = 6,
            CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6,
        };
        sb.SetBorderWidthAll(0);
        btn.AddThemeStyleboxOverride("normal",  sb);
        var sbH = (StyleBoxFlat)sb.Duplicate(); sbH.BgColor = bg.Lightened(0.2f);
        btn.AddThemeStyleboxOverride("hover",   sbH);
        var sbP = (StyleBoxFlat)sb.Duplicate(); sbP.BgColor = bg.Darkened(0.15f);
        btn.AddThemeStyleboxOverride("pressed", sbP);
        btn.AddThemeFontSizeOverride("font_size", 16);
        btn.AddThemeColorOverride("font_color", Colors.White);
        return btn;
    }

    private static void RefreshLabel(Label lbl, float v) =>
        lbl.Text = $"{Mathf.RoundToInt(v * 100f)}%";

    private static bool IsFullscreen() =>
        DisplayServer.WindowGetMode() is DisplayServer.WindowMode.Fullscreen
                                      or DisplayServer.WindowMode.ExclusiveFullscreen;

    // ── Open / Close ──────────────────────────────────────────────────────────

    public void Open(bool inGame = false)
    {
        _inGame = inGame;

        float musicVol = GetStoredVolume("music_vol", 1.0f);
        float sfxVol   = GetStoredVolume("sfx_vol",   1.0f);

        _musicSlider.SetValueNoSignal(musicVol);
        _sfxSlider.SetValueNoSignal(sfxVol);
        _fullscreenToggle.SetPressedNoSignal(IsFullscreen());

        RefreshLabel(_musicValueLabel, musicVol);
        RefreshLabel(_sfxValueLabel,   sfxVol);

        _backToMenuBtn.Visible = inGame;
        _closeBtn.Text         = inGame ? "Continuar" : "Fechar";

        Visible = true;
        if (inGame) GetTree().Paused = true;
    }

    private void SaveAndClose()
    {
        var cfg = new ConfigFile();
        cfg.SetValue(Sect, "music_vol",  (float)_musicSlider.Value);
        cfg.SetValue(Sect, "sfx_vol",    (float)_sfxSlider.Value);
        cfg.SetValue(Sect, "fullscreen", _fullscreenToggle.ButtonPressed);
        cfg.Save(CfgPath);

        Visible = false;
        if (_inGame) GetTree().Paused = false;
    }

    private static float GetStoredVolume(string key, float fallback)
    {
        var cfg = new ConfigFile();
        return cfg.Load(CfgPath) == Error.Ok
            ? (float)cfg.GetValue(Sect, key, fallback)
            : fallback;
    }

    private void OnClose()    => SaveAndClose();

    private void OnBackToMenu()
    {
        SaveAndClose();
        GameManager.Instance?.GoToMainMenu();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!Visible) return;
        if (e is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape })
        {
            GetViewport().SetInputAsHandled();
            SaveAndClose();
        }
    }
}
