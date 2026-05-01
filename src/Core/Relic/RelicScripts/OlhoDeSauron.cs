using Godot;
using System;

[GlobalClass]

public partial class OlhoDeSauron : RelicData
{
	// Called when the node enters the scene tree for the first time.
	
	public override void BeforeOnCombatStart(Player player)
	{
		player.BonusCardsToDraw += 2;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
}
