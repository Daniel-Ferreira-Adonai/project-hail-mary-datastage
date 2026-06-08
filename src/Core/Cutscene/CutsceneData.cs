using Godot;
using System;

[GlobalClass]
public partial class CutsceneData : Resource
{
    [Export] public CutsceneSlideData[] Slides { get; set; } = Array.Empty<CutsceneSlideData>();
    [Export] public string NextScene { get; set; } = "";
}
