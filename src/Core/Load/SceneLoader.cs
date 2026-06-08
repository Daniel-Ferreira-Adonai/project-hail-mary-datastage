using Godot;

public partial class SceneLoader : Node
{
    public static SceneLoader Instance { get; private set; }

    private string _pending;

    public override void _Ready() => Instance = this;

    public void StartLoad(string scenePath)
    {
        _pending = scenePath;
        ResourceLoader.LoadThreadedRequest(scenePath);
    }

    public async void GoTo(string scenePath, bool showLoading = false)
    {
        CanvasLayer overlay = showLoading ? CreateLoadingOverlay() : null;

        if (_pending != scenePath) StartLoad(scenePath);

        while (ResourceLoader.LoadThreadedGetStatus(scenePath) == ResourceLoader.ThreadLoadStatus.InProgress)
            await ToSignal(GetTree().CreateTimer(0.05), SceneTreeTimer.SignalName.Timeout);

        if (ResourceLoader.LoadThreadedGetStatus(scenePath) == ResourceLoader.ThreadLoadStatus.Loaded)
            GetTree().ChangeSceneToPacked((PackedScene)ResourceLoader.LoadThreadedGet(scenePath));
        else
        {
            GD.PushError($"SceneLoader: falha ao carregar {scenePath}");
            GetTree().ChangeSceneToFile(scenePath);
        }

        overlay?.QueueFree();
    }

    private CanvasLayer CreateLoadingOverlay()
    {
        var cl  = new CanvasLayer { Layer = 200 };
        var bg  = new ColorRect { Color = new Color(0f, 0f, 0f, 0.88f) };
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        var lbl = new Label { Text = "Carregando..." };
        lbl.SetAnchorsPreset(Control.LayoutPreset.Center);
        cl.AddChild(bg);
        cl.AddChild(lbl);
        GetTree().Root.AddChild(cl);
        return cl;
    }
}
