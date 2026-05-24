using Godot;
using System;
using System.Runtime.Serialization;

[GlobalClass]
public partial class DebuffWeakIntent : IntentData
{
    public override void Execute(Enemy enemy, Player player)
    {
        player.ApplyDebuff("Weak", this.Value);
    }
}