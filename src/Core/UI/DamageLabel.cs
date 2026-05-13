using Godot;
using System;

public partial class DamageLabel : Label
{
    [Export] AnimationPlayer animationPlayer;

    public override void _Ready()
    {
    }

    public void Setup(int damage)
    {
        Text = damage.ToString();
        animationPlayer.Play("Damage");
		GD.Print("fui chamado");
        animationPlayer.AnimationFinished += (_) => QueueFree();

    }
}