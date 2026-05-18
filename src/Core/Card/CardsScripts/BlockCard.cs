using Godot;
using System;

[GlobalClass]
public partial class BlockCard : CardData
{
	public override void Execute(object target = null, object aux = null)
	{
		if (target is Player player)
		{
			player.BlockValue += CombatManager.CalculateBlock(this.Block,player,this);
			player.UpdateLabelValues();

		}
	}
}
