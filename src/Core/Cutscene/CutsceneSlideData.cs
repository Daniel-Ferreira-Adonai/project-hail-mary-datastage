using Godot;

[GlobalClass]
public partial class CutsceneSlideData : Resource
{
    [Export] public Texture2D Image { get; set; }
    [Export] public string NarrativeText { get; set; } = "";
    [Export] public float Duration { get; set; } = 5.0f;
    [Export] public AudioStream Music { get; set; }
}
