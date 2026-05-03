using Godot;
using System;
[GlobalClass]
public partial class GritoDaMasculinidade : RelicData
{
		public override void OnCombatStart(Player player)
	{
			 var enemies = player.GetTree().GetNodesInGroup("enemies");
			foreach (var node in enemies)
			{
				if (node is Enemy enemy)
					{
						enemy.ApplyDebuff("Vulnerable",1);
					}
	}
		
	}
}
