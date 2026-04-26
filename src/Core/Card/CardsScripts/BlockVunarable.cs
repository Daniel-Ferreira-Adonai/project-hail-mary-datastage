using Godot;
using System;

[GlobalClass]
public partial class BlockVunarable : CardData
{
	public override void Execute(object target = null, object aux = null)
	{
		if (target is Enemy enemy && aux is Player player)
		{
			enemy.Vulnerable += 1;
			player.BlockValue += this.Block;
			player.UpdateLabelValues();
		}
	}
  }
