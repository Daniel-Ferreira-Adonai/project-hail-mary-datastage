using Godot;
using System;

public partial class endTurn : Button
{
	CombatManager _combatManager;
	public override void _Ready()
	{
		_combatManager = GetParent().GetNode<CombatManager>("CardManager");
	}

	public void _on_button_down2()
	{
		GD.Print("tste");
		_combatManager.EndTurn();
	}
	public override void _Process(double delta)
	{
	}
}
