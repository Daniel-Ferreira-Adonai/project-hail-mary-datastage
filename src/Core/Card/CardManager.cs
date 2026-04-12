using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardManager : Node2D
{
	private MouseInputTracker _mouse;
	public Vector2 ScreenSize {get; private set;}
	public Card CardBeingDraged {get; set;}
	public bool IsHoveringOnCard {get; set;} = false;
	public override void _Ready()
	{
		_mouse = GetNode<MouseInputTracker>("/root/MouseTracker");
		ScreenSize = GetViewport().GetVisibleRect().Size;
	}
    public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left )
		{
			if (mb.Pressed)
			{
				Card card = RaycastCheckForCard();
				if(card is not null)
				{
					StartDragging(card);
				}
			}
			else FinishDrag();
		}
	
	}
		public void ConnectCardSignals(Card card)
	{
		card.Connect(Card.SignalName.CardHovered, Callable.From<Card>(OnCardHovered));
		card.Connect(Card.SignalName.CardUnhovered, Callable.From<Card>(OnCardUnhovered));
	}
	private void OnCardHovered(Card card)
	{
		if (!IsHoveringOnCard)
		{

			IsHoveringOnCard = true;
			GD.Print(IsHoveringOnCard);

			HighlightCard(card, true);

		}
	}

	private void OnCardUnhovered(Card card)
	{
		if(CardBeingDraged is null)
		{
			HighlightCard(card, false);
		Card newCard = RaycastCheckForCard();

		if(newCard != null)
		{
			HighlightCard(newCard,true);

		}
		else
		IsHoveringOnCard = false;
		}
		
	}
	public override void _Process(double delta)
	{
		if (CardBeingDraged != null)
		CardBeingDraged.GlobalPosition = CardBeingDraged.GlobalPosition.Lerp(
			_mouse.ScreenPosition.Clamp(Vector2.Zero, ScreenSize), 
			(float)delta * 15f
		);	
		}
	private void HighlightCard(Card card, bool hovered)
	{	
		if(hovered)
		{
			card.Scale = new Vector2(1.05f,1.05f);
			card.ZIndex = 2;
		}
		else
		{
			card.Scale = new Vector2(1f,1f);
			card.ZIndex = 1;
		}

	}
	public  Card RaycastCheckForCard()
	{
		var SpaceState =  GetWorld2D().DirectSpaceState; 

		var parameters = new PhysicsPointQueryParameters2D();
		parameters.Position = _mouse.ScreenPosition;
		parameters.CollideWithAreas = true;
		parameters.CollisionMask = 1;

		var results = SpaceState.IntersectPoint(parameters);
		List<Card> cards = new List<Card>();
		
		if(results.Count > 0)
		{
			foreach(var result in results)
				{
				cards.Add(result["collider"].As<Area2D>().GetParent<Card>());			
					
				}
			return GetCardWithHighestZIndex(cards);
		} 
		return null;
	}

	private Card GetCardWithHighestZIndex(List<Card> cards)
	{
		cards.Sort((x,y) => y.ZIndex.CompareTo(x.ZIndex));
		return cards.FirstOrDefault();
	}
	private void StartDragging(Card card)
	{
		CardBeingDraged = card;
		card.Scale = new Vector2(1.0f,1.0f);
		
	}
	private void FinishDrag()
	{
		CardBeingDraged.Scale = new Vector2(1.05f,1.05f);

		CardBeingDraged = null;
	}
}
