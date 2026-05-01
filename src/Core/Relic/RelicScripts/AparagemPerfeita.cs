using Godot;
using System;

[GlobalClass]

public partial class AparagemPerfeita : RelicData
{
	// Called when the node enters the scene tree for the first time.
	
		public override void OnTakeDamage(Player player, ref int damage)
	{
		 if(player.BlockValue - damage == 0)
		{
			 var enemies = player.GetTree().GetNodesInGroup("enemies");
			foreach (var node in enemies)
			{
				if (node is Enemy enemy)
					{
						enemy.TakeDamage(5);
					}
	}
		}
	}
}
