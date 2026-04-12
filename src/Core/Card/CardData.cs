using Godot;
using System;

public partial class CardData : Resource
{
	public enum CardType{Attack, Skill, Power}
	[Export] public string CardName { get; set; }
    [Export] public int EnergyCost { get; set; }
    [Export] public int Damage { get; set; }
    [Export] public int Block { get; set; }
    [Export] public string Description { get; set; }
    [Export] public Texture2D Art { get; set; }
}
