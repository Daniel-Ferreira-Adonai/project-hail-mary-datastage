using Godot;
using System;

public partial class TestLabel : Label
{
	private MouseInputTracker _mouse;
	public override void _Ready()
	{
		_mouse = GetNode<MouseInputTracker>("/root/MouseTracker");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Text = _mouse.ScreenPosition.ToString();
	}
}
