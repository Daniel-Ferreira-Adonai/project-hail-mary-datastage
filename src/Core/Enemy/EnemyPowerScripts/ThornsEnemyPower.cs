using Godot;

[GlobalClass]
public partial class ThornsEnemyPower : EnemyPowerData
{
    public override void OnDamageTaken(Enemy enemy, int damage)
    {
        var player = PlayerManager.Instance.Player;
        if (player != null)
            player.TakeDamage(EffectValue);
    }
}