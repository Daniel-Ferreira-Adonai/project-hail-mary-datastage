using Godot;
using System;

[GlobalClass]
public partial class EffectDatabase : Resource
{
    [Export] public Godot.Collections.Dictionary<string, EffectData> Effects;
}