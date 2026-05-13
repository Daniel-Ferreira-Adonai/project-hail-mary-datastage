using Godot;
using System;

[GlobalClass]

public partial class PocaoDeVida : RelicData
{
	// Called when the node enters the scene tree for the first time.
	
	public override void OnCombatStart(Player player)
	{
		player.TryToHeal(2);
		player.UpdateLabelValues();
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
}
