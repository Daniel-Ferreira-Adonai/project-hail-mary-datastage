using Godot;
using System;

[GlobalClass]
public partial class EspinhosRelic : RelicData
{
    private const int DamageReturn = 3;

    public override void OnTakeDamage(Player player, ref int damage)
    {
        var enemy = CombatManager.Instance.GetFirstEnemy();
        if (enemy == null) return;
        enemy.TakeDamage(DamageReturn);
    }
}