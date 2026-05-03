using Godot;
using System;

[GlobalClass]

public partial class OvoVermelho : RelicData
{
	// Called when the node enters the scene tree for the first time.
	
	public override void OnCombatStart(Player player)
	{
		player.PerCombatTemporaryStrength += 1;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
}
