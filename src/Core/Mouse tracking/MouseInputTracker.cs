using Godot;
using System;

public partial class MouseInputTracker : Node
{	
	public static MouseInputTracker Instance {get; private set;}

	public Vector2 ScreenPosition {get; private set;}
	
	public bool LeftHeld {get; private set;}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}
	public override void _Input(InputEvent @event)
	{
		if(@event is InputEventMouseMotion  motion)
			ScreenPosition = motion.Position;
		if(@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
			LeftHeld = mb.Pressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
