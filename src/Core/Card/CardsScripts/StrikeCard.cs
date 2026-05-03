using Godot;
using System;

[GlobalClass]
public partial class StrikeCard : CardData
{
	public override void Execute(object target = null, object aux = null)
	{
		if (target is Enemy enemy && aux is Player player)
		{
        int damage = CombatManager.Calculate(this.Damage, player, enemy);
        enemy.TakeDamage(damage);
		}
	}
  }
