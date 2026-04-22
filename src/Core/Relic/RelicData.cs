using Godot;
using System;

public partial class RelicData : Resource
{
    [Export] public string RelicName { get; set; }
    [Export] public string Description { get; set; }
    [Export] public Texture2D Icon { get; set; }

    public virtual void OnCombatStart(Player player) { }
    public virtual void OnTurnStart(Player player) { }
    public virtual void OnTurnEnd(Player player) { }
    public virtual void OnCardPlayed(Player player, CardData card) { }
    public virtual void OnTakeDamage(Player player, ref int damage) { }
    public virtual void OnKillEnemy(Player player, Enemy enemy) { }
}