using Godot;
using System;
using System.Runtime.Serialization;

[GlobalClass]
public partial class BuffForcaIntent : IntentData
{
	public override void Execute(Enemy enemy, Player player)
	{
		enemy.Strength += this.Value;
	}
}
