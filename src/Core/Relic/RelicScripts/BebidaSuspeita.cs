using Godot;
using System;

[GlobalClass]

public partial class BebidaSuspeita : RelicData
{
	// Called when the node enters the scene tree for the first time.
	
	public override void OnCombatStart(Player player)
	{
		
		player.MaxHp += 1;
		player.UpdateLabelValues();

	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
}
