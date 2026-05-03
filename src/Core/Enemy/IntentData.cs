using Godot;

[GlobalClass]
public partial class IntentData : Resource
{
    public enum IntentType { Attack, Defend, Buff, Debuff }

    [Export] public IntentType Type;
    [Export] public int Value;
    [Export] public Texture2D Icon;
    [Export] public string Description; 
	public virtual void Execute(Enemy user, Player target)
    {
        GD.Print("Executando ação genérica");
    }
}