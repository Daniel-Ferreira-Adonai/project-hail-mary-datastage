using Godot;

[GlobalClass]
public partial class MangaComLeiteCard : CardData
{
    public override void Execute(object target = null, object aux = null)
    {
        if (target is Enemy && aux is Player player)
        {
            var enemies = player.GetTree().GetNodesInGroup("enemies");
            foreach (var node in enemies)
            {
                if (node is Enemy e)
                {
                    int damage = CombatManager.Calculate(this.Damage, player, e, this);
                    e.TakeDamage(damage);
                }
            }
        }
    }

    public override void UpgradeCard()
    {
        this.Damage += 2;
    }
}
