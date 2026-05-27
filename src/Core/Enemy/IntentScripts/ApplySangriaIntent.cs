using Godot;

[GlobalClass]
public partial class ApplySangriaIntent : IntentData
{
    public override void Execute(Enemy enemy, Player player)
    {
        player.ApplyDebuff("Sangria", this.Value);
    }
}