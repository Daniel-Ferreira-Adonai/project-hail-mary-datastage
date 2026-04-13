using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardManager : Node2D
{
	private MouseInputTracker _mouse;
	public Vector2 ScreenSize => GetViewport().GetVisibleRect().Size;
	public Card CardBeingDraged {get; set;}
	public bool IsHoveringOnCard {get; set;} = false;
	private Vector2 _lastCardPosition;
	private float _maxCardRotation = 0.3f;
	private float _hoverScale = 1.30f;
	[Export] private PackedScene _cardScene;  
	[Export] private Hand _handNode;  

	private List<CardData> _deck = new List<CardData>();  
	private List<Card> _handList = new List<Card>();      
	private List<CardData> _discard = new List<CardData>();  
	public Dictionary<Card, float> _originalRotations = new Dictionary<Card, float>();
	public Dictionary<Card, Vector2> _originalPositions = new Dictionary<Card, Vector2>();
	public Dictionary<Card, Vector2> _originalScales = new Dictionary<Card, Vector2>();
	public bool IsArranging { get; set; } = false;

	public override void _Ready()
	{
		_mouse = GetNode<MouseInputTracker>("/root/MouseTracker");

		CardData strike = GD.Load<CardData>("res://Data/Cards/StrikeCard.tres");

		for (int i = 0; i < 8; i++)
			_deck.Add(strike);

		for (int i = 0; i < 5; i++)
		{
			var timer = GetTree().CreateTimer(i * 0.6f);
			timer.Timeout += DrawCard;
		}
	}

	public void DrawCard()
	{
		if (_deck.Count == 0) return;
		if (_handList.Count >= 9) return;

		CardData currentCardData = _deck[0];
		_deck.RemoveAt(0);

		Card card = _cardScene.Instantiate<Card>();
		_handNode.AddChild(card);
		card.Setup(currentCardData);
		_handList.Add(card);
		_handNode.AddCard(card);
		_originalScales[card] = card.Scale;
	}

	public void ShuffleDeck()
	{
		_deck.AddRange(_discard);
		_discard.Clear();

		for (int i = _deck.Count - 1; i > 0; i--)
		{
			int j = (int)(GD.Randi() % (uint)(i + 1));
			(_deck[i], _deck[j]) = (_deck[j], _deck[i]);
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
		{
			if (mb.Pressed)
			{
				Card card = RaycastCheckForCard();
				if (card is not null)
					StartDragging(card);
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
		if (!IsHoveringOnCard && !IsArranging)
		{
			IsHoveringOnCard = true;
			HighlightCard(card, true);
		}
	}

	private void OnCardUnhovered(Card card)
	{
		if (CardBeingDraged is null)
		{
			HighlightCard(card, false);
			Card newCard = RaycastCheckForCard();
			if (newCard != null)
				HighlightCard(newCard, true);
			else
				IsHoveringOnCard = false;
		}
	}

	public override void _Process(double delta)
	{
		if (CardBeingDraged != null)
			DragLogic(delta);
	}

	private void HighlightCard(Card card, bool hovered)
	{
		var tween = CreateTween().SetParallel();

		if (hovered)
		{
			if (!_originalPositions.ContainsKey(card))
			{
				_originalPositions[card] = card.Position;
				_originalRotations[card] = card.RotationDegrees;
			}

			Vector2 scale = _originalScales[card];
			float moveUp = GetPositionToMoveUpRelativeToBottom(card);
			Vector2 originalPos = _originalPositions[card];

			tween.TweenProperty(card, "position", new Vector2(originalPos.X, originalPos.Y - moveUp), 0.15f);
			tween.TweenProperty(card, "rotation_degrees", 0f, 0.15f);
			tween.TweenProperty(card, "scale", new Vector2(scale.X * _hoverScale, scale.Y * _hoverScale), 0.15f);
			card.ZIndex = 2;
		}
		else
		{
			if (_originalPositions.ContainsKey(card))
			{
				tween.TweenProperty(card, "position", _originalPositions[card], 0.15f);
				tween.TweenProperty(card, "rotation_degrees", _originalRotations[card], 0.15f);
			}
			Vector2 scale = _originalScales[card];
			tween.TweenProperty(card, "scale", new Vector2(scale.X, scale.Y), 0.15f);
			card.ZIndex = 1;
		}
	}

	public Card RaycastCheckForCard()
	{
		var spaceState = GetWorld2D().DirectSpaceState;
		var parameters = new PhysicsPointQueryParameters2D
		{
			Position = _mouse.ScreenPosition,
			CollideWithAreas = true,
			CollisionMask = 1
		};

		var results = spaceState.IntersectPoint(parameters);
		List<Card> cards = new List<Card>();

		if (results.Count > 0)
		{
			foreach (var result in results)
				cards.Add(result["collider"].As<Area2D>().GetParent<Card>());
			return GetCardWithHighestZIndex(cards);
		}
		return null;
	}

	private Card GetCardWithHighestZIndex(List<Card> cards)
	{
		cards.Sort((x, y) => y.ZIndex.CompareTo(x.ZIndex));
		return cards.FirstOrDefault();
	}

	private void StartDragging(Card card)
	{
		Vector2 scale = _originalScales[card];
		CardBeingDraged = card;
		card.Scale = new Vector2(scale.X, scale.Y);
	}

	private void FinishDrag()
	{
		if (CardBeingDraged is null) return;

		Vector2 scale = _originalScales[CardBeingDraged];
		CardBeingDraged.Scale = new Vector2(scale.X, scale.Y);
		CardBeingDraged.Rotation = 0f;
		CardBeingDraged = null;
		_originalPositions.Clear();
		_originalRotations.Clear();
		IsHoveringOnCard = false;
		_handNode.ArrangeFan();
	}

	private void DragLogic(double delta)
	{
		Vector2 targetPos = _mouse.ScreenPosition.Clamp(Vector2.Zero, ScreenSize);

		CardBeingDraged.GlobalPosition = CardBeingDraged.GlobalPosition.Lerp(
			targetPos,
			(float)delta * 15f
		);

		float desiredRotation = Mathf.Clamp(
			(CardBeingDraged.GlobalPosition.X - _lastCardPosition.X) * 0.75f,
			-_maxCardRotation,
			_maxCardRotation
		);

		CardBeingDraged.Rotation = Mathf.Lerp(
			CardBeingDraged.Rotation,
			desiredRotation,
			(float)delta * 12f
		);

		_lastCardPosition = CardBeingDraged.GlobalPosition;
	}

	private float GetPositionToMoveUpRelativeToBottom(Card card)
	{
		float originalScaleY = _originalScales[card].Y;
		float hoverScaleY = originalScaleY * _hoverScale;

		Sprite2D fundo = card.GetNode<Sprite2D>("FinalCarta");
		float cardHeight = fundo.GetRect().Size.Y * hoverScaleY;

		Vector2 originalPos = _originalPositions[card];
		float cardGlobalBottom = (_handNode.GlobalPosition.Y + originalPos.Y) + cardHeight / 2f;
		float overflow = cardGlobalBottom - ScreenSize.Y;
		float moveUp = overflow > 0 ? overflow + 10f : 0f;
		return moveUp;
	}
}