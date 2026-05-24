using Godot;

public partial class EffectManager : Node
{
    public static EffectManager Instance { get; private set; }

    [Export] public EffectDatabase EffectDB;

    public override void _Ready()
    {
        Instance = this;
		 if (EffectDB == null)
        EffectDB = new EffectDatabase(); // 🔥 cria automaticamente

		    LoadAllEffects();

    }

    public EffectData GetEffect(string id)
    {
        if (EffectDB.Effects.TryGetValue(id, out var data))
            return data;

        GD.PrintErr($"Effect não encontrado: {id}");
        return null;
    }
private void LoadAllEffects()
{
    if (EffectDB.Effects == null)
        EffectDB.Effects = new Godot.Collections.Dictionary<string, EffectData>();

    EffectDB.Effects.Clear();

    var files = DirAccess.GetFilesAt("res://Data/Effects/");

    foreach (var file in files)
    {
        if (!file.EndsWith(".tres")) continue;

        var effect = GD.Load<EffectData>($"res://Data/Effects/{file}");

        if (effect != null && !string.IsNullOrEmpty(effect.Id))
        {
            EffectDB.Effects[effect.Id] = effect;
        }
    }
}
}