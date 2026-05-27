using Godot;

[GlobalClass]
public partial class EnemyPowerData : Resource
{
    [Export] public string Id;

    [Export] public int EffectValue;

   public string GetDescription()
{
    var effectData = EffectManager.Instance.GetEffect(Id);
    return effectData?.GetDescription(EffectValue) ?? "";
}
    public virtual void OnTurnStart(Enemy enemy) {}
    public virtual void OnTurnEnd(Enemy enemy) {}
    public virtual void OnDamageTaken(Enemy enemy, int damage) {}
    public virtual void OnDebuffApplied(Enemy enemy, string debuff, int value) {}

    public virtual void OnPlayerCardPlayed(Enemy enemy, CardData card) {}

}