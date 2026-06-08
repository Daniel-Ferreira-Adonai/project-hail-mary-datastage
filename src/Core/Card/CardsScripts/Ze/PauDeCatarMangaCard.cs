using Godot;

[GlobalClass]
public partial class PauDeCatarMangaCard : CardData
{
    public override void Execute(object target = null, object aux = null)
    {
        if (target is Enemy enemy && aux is Player player)
        {
            int damage = CombatManager.Calculate(this.Damage, player, enemy, this);
            enemy.TakeDamage(damage);
        }
    }

    public override void UpgradeCard()
    {
        this.Damage += 3;
    }
}
