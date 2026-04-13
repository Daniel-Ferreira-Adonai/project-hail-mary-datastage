using Godot;
using System;

public partial class Card : Node2D
{
	private MouseInputTracker _mouse;

	private CardData Data {get; set;}
	
	[Signal] public delegate void CardHoveredEventHandler(Card card);
	[Signal] public delegate void CardUnhoveredEventHandler(Card card);
	
	public override void _Ready()
	{
   		 GetParent().GetParent<CardManager>().ConnectCardSignals(this);
		_mouse = GetNode<MouseInputTracker>("/root/MouseTracker");
	}

	public override void _Process(double delta)
	{
		
	}
	public void _on_area_2d_mouse_entered()
	{
		EmitSignal(SignalName.CardHovered,this);
	}
	public void _on_area_2d_mouse_exited()
	{
		EmitSignal(SignalName.CardUnhovered, this);
	}
	public void Setup(CardData data)
	{
		Data = data;
		 GetNode<Label>("Nome").Text = data.CardName;
		GetNode<Label>("Custo").Text = data.EnergyCost.ToString();
		GetNode<Label>("Descricao").Text = data.Description;
		}
}
