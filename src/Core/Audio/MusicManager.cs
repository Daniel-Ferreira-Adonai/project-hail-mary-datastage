using Godot;

public partial class MusicManager : Node
{
    public static MusicManager Instance { get; private set; }

    public enum MenuTheme { DrPeste, PortaoTorre }
    public MenuTheme CurrentTheme { get; set; } = MenuTheme.DrPeste;

    private AudioStreamPlayer _a, _b, _active;
    private AudioStream _currentStream;
    private Tween _fadeTween;

    private const float FullDb = -5f;
    private const float MuteDb = -60f;

    private AudioStream _menuDrPeste, _menuPortaoTorre, _combatGeral, _loja;

    public AudioStream CombatGeral => _combatGeral;

    private readonly System.Collections.Generic.Dictionary<AudioStream, float> _savedPositions = new();

    public override void _Ready()
    {
        Instance = this;
        SettingsMenu.Bootstrap();
        string bus = AudioServer.GetBusIndex("Music") != -1 ? "Music" : "Master";
        _a = new AudioStreamPlayer { Bus = bus, VolumeDb = MuteDb };
        _b = new AudioStreamPlayer { Bus = bus, VolumeDb = MuteDb };
        AddChild(_a); AddChild(_b);
        _active = _a;

        _menuDrPeste     = GD.Load<AudioStream>("res://Audio/Music/MusicaMenuDrpeste.mp3");
        _menuPortaoTorre = GD.Load<AudioStream>("res://Audio/Music/MenuPortãoTorre.mp3");
        _combatGeral     = GD.Load<AudioStream>("res://Audio/Music/CombateGeral.mp3");
        _loja            = GD.Load<AudioStream>("res://Audio/Music/MusicaLoja.mp3");
    }

    public void PlayThemeMusic(float fade = 1f)
        => PlayTrack(CurrentTheme == MenuTheme.DrPeste ? _menuDrPeste : _menuPortaoTorre, fade);

    public void PlayShop(float fade = 1f) => PlayTrack(_loja, fade);

    public void PlayTrack(AudioStream stream, float fade = 1.0f, float volumeDb = FullDb)
    {
        if (stream is null) return;
        if (stream == _currentStream && _active.Playing) return;

        // Salva posição da faixa atual antes de trocar
        if (_currentStream != null && _active.Playing)
            _savedPositions[_currentStream] = _active.GetPlaybackPosition();

        if (stream is AudioStreamMP3 mp3) mp3.Loop = true;

        _currentStream = stream;
        var next = _active == _a ? _b : _a;
        var prev = _active;

        next.Stream = stream;
        next.VolumeDb = MuteDb;
        next.Play();

        // Retoma de onde parou (se houver posição salva)
        if (_savedPositions.TryGetValue(stream, out float pos) && pos > 0f)
            next.Seek(pos);

        _fadeTween?.Kill();
        _fadeTween = CreateTween().SetParallel();
        _fadeTween.TweenProperty(next, "volume_db", volumeDb, fade);
        _fadeTween.TweenProperty(prev, "volume_db", MuteDb, fade);
        _fadeTween.Chain().TweenCallback(Callable.From(() => { if (prev != next) prev.Stop(); }));

        _active = next;
    }

    public void Stop(float fade = 0.5f)
    {
        _currentStream = null;
        _fadeTween?.Kill();
        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(_active, "volume_db", MuteDb, fade);
        _fadeTween.TweenCallback(Callable.From(() => _active.Stop()));
    }
}
