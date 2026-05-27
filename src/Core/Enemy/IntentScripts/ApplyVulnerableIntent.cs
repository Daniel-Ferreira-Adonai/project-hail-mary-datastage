using Godot;
using System;


[GlobalClass]
public partial class ApplyVulnerableIntent : IntentData
{
    public override void Execute(Enemy enemy, Player player)
    {
        player.ApplyDebuff("Vulnerable", this.Value);
    }
}