using Godot;
using System;
using System.Runtime.Serialization;

[GlobalClass]
public partial class DebuffFrailIntent : IntentData
{
	public override void Execute(Enemy enemy, Player player)
	{
		player.Frail = this.Value;
	}
}
