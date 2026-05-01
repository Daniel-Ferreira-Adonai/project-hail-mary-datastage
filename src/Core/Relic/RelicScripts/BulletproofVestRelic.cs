using Godot;
using System;

[GlobalClass]
public partial class BulletproofVestRelic : RelicData
{
    private int _damageThisTurn = 0;

    public override void OnTurnStart(Player player)
    {
        _damageThisTurn = 0;
    }

    public override void OnTakeDamage(Player player, ref int damage)
    {
         int damageToHp = Math.Max(0, damage - player.BlockValue);
        _damageThisTurn += damageToHp;

        if (_damageThisTurn >= 10)
        {
            player.BonusCardsToDraw += 4;
            _damageThisTurn = 0;
        }
    }
}