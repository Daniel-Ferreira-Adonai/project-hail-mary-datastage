using Godot;
using System;

public partial class endTurn : Button
{
	GameManager _gamaManager;
	public override void _Ready()
	{
		_gamaManager = GetParent().GetNode<GameManager>("GameManager");
	}

	public void _on_button_down2()
	{
		GD.Print("tste");
		_gamaManager.EndTurn();
	}
	public override void _Process(double delta)
	{
	}
}
