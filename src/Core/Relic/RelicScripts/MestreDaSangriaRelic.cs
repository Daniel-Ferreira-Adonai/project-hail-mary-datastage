using Godot;
using System;

[GlobalClass]
public partial class MestreDaSangriaRelic : RelicData
{
    public override void OnDebuffApplied(Player player, string debuff, Enemy enemy)
    {
        if (debuff != "Sangria") return;
        enemy.Debuffs["Sangria"] *= 2;
    }
}
