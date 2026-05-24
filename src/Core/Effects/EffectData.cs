using Godot;

[GlobalClass]
public partial class EffectData : Resource
{
    [Export] public string Id; 
    [Export] public string Name;
    [Export] public Texture2D Icon;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public EffectCategory Category;

	public string GetDescription(int value)
{
    if (string.IsNullOrEmpty(Description))
        return "";

    return Description.Replace("{value}", value.ToString());
}
}