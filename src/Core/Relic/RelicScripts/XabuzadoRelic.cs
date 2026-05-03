using Godot;
using System;

[GlobalClass]
public partial class XabuzadoRelic : RelicData
{
    private int _debuffCount = 0;

    public override void OnDebuffApplied(Player player, string debuff) 
    {
        _debuffCount++;
        if (_debuffCount % 5 == 0)
            player.NextDebuffDoubled = true; 
    }
}