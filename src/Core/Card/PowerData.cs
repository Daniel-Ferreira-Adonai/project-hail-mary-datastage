using Godot;

[GlobalClass]
public partial class PowerData : CardData
{
    public virtual void OnDebuffApplied(Player player, string debuff, int value, Enemy enemy) {}
    public virtual void OnTurnStart(Player player) {}
    public virtual void OnTurnEnd(Player player) {}
    public virtual void OnDamageTaken(Player player, int damage) {}
}