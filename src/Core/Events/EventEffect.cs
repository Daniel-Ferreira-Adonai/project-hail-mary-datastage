using Godot;
using System;

public enum EffectType { GainGold, LoseGold, GainHP, LoseHP, GainCard, RemoveCard, GainRelic }

[GlobalClass]
public partial class EventEffect : Resource
{
    [Export] public EffectType Type { get; set; }
    [Export] public int Value { get; set; }
}