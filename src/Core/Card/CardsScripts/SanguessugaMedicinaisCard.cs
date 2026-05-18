using Godot;
using System;

[GlobalClass]
public partial class SanguessugaMedicinaisCard : CardData
{
	public override void Execute(object target = null, object aux = null)
	{
		if (target is Player player)
		{
			player.BlockValue += CombatManager.CalculateBlock(this.Block,player,this);
			player.UpdateLabelValues();
			 var enemies = player.GetTree().GetNodesInGroup("enemies");
			foreach (var node in enemies)
			{
				if (node is Enemy enemy)
					{
						enemy.ApplyDebuff("Sangria",this.EffectValue);
					}
			}
		}
	}
	public override int GetSpecialModifierValue(object target = null, object aux = null)
{
    if (aux is Player player)
		{
			var enemies = player.GetTree().GetNodesInGroup("enemies");
			int extraBlock = 0;
			foreach (var node in enemies)
			{
				if (node is Enemy enemy)
					{
						extraBlock = enemy.GetDebuffValue("Sangria");
					}
			}
		}
	return 0;
}
}
