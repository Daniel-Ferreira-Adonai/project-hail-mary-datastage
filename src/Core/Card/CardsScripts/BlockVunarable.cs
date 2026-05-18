using Godot;
using System;

[GlobalClass]
public partial class BlockVunarable : CardData
{
	public override void Execute(object target = null, object aux = null)
	{
		if (target is Enemy enemy && aux is Player player)
		{
			GD.Print("card jogado");
			enemy.ApplyDebuff("Vulnerable",1);
			player.BlockValue += CombatManager.CalculateBlock(this.Block,player,this);
			player.UpdateLabelValues();
		}
	}
  }
