using Godot;
using System;

public enum EffectType { GainGold, LoseGold, GainHP, LoseHP, GainCard, RemoveCard, GainRelic, GainRandomCard, UpgradeCard, UpgradeRandomCard, RemoveRandomCard, nothing, RandomHpSwing, RandomGoldSwing }



[GlobalClass]
public partial class EventEffect : Resource
{
    [Export] public EffectType Type { get; set; }
    [Export] public int Value { get; set; }

	[Export] public bool secret { get; set; } = false;

}