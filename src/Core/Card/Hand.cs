using Godot;
using System;
using System.Collections.Generic;

public partial class Hand : Node2D
{
	[Export] private Curve _handCurve;
	[Export] private Curve _rotationCurve;
	[Export] private float _fanHeight = 20f;

	[Export] private float _xSep = 50f;
	[Export] private float _yMin = 130f;
	[Export] private float _yMax = 40f;
	[Export] private float _maxRotationDegrees = 10f;
    public Vector2 ScreenSize => GetViewport().GetVisibleRect().Size;	public Card CardBeingDraged {get; set;}


	private List<Card> _cards = new List<Card>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
    	Position = new Vector2(ScreenSize.X / 2f, ScreenSize.Y * 0.8f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void AddCard(Card card, Vector2? fromPosition = null)
{
    _cards.Add(card);

    card.Position = fromPosition ?? new Vector2(-500, 50f);
    card.Modulate = new Color(1, 1, 1, 0f);

    ArrangeFan();

    // fade in starts after position tween (0.2s) to avoid racing with modulate
    var fadeTween = card.CreateTween();
    fadeTween.TweenInterval(0.2f);
    fadeTween.TweenProperty(card, "modulate", new Color(1, 1, 1, 1f), 0.2f);
}
	public void RemoveCard(Card card)
	{
		_cards.Remove(card);
		ArrangeFan();
	}
    public void ClearList()
    {
        _cards.Clear();
    }
    public List<Card> getHandCards()
    {
        return _cards;
    }
public void ArrangeFan()
{
    int count = _cards.Count;
    if (count == 0) return;

    var manager = GetParent<CardManager>();
    manager.IsArranging = true;
    manager._originalPositions.Clear(); 
    manager._originalRotations.Clear();  

    Vector2 screenSize = GetViewport().GetVisibleRect().Size;
    float cardWidth = 100f;
    float allCardsSize = cardWidth * count + _xSep * (count - 1);
    float finalXSep = _xSep;

    if (allCardsSize > screenSize.X)
    {
        finalXSep = (screenSize.X - cardWidth * count) / (count - 1);
        allCardsSize = screenSize.X;
    }

    float offset = (screenSize.X - allCardsSize) / 2f - Position.X + cardWidth / 2f;

    if (count == 1)
    {
        _cards[0].Position = new Vector2(offset, _yMin);
        _cards[0].RotationDegrees = 0f;
        _cards[0].ZIndex = 1;
        manager._originalPositions[_cards[0]] = new Vector2(offset, _yMin);
        manager._originalRotations[_cards[0]] = 0f;
        manager.IsArranging = false;
        return;
    }

    Tween lastTween = null;

    for (int i = 0; i < count; i++)
    {
        float t = count > 1 ? (1.0f / (count - 1)) * i : 0f;
        float yMultiplier = count > 1 ? _handCurve.Sample(t) : 0f;
        float rotMultiplier = count > 1 ? _rotationCurve.Sample(t) : 0f;

        float x = offset + cardWidth * i + finalXSep * i;
        float y = _yMin - _yMax * yMultiplier;
        float angle = _maxRotationDegrees * rotMultiplier;

        // Salva a posição FINAL antes do tween rodar
        manager._originalPositions[_cards[i]] = new Vector2(x, y);
        manager._originalRotations[_cards[i]] = angle;
        _cards[i].ZIndex = i + 1;

        var tween = _cards[i].CreateTween().SetParallel();
        tween.TweenProperty(_cards[i], "position", new Vector2(x, y), 0.2f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_cards[i], "rotation_degrees", angle, 0.2f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        lastTween = tween;
    }

    lastTween.Finished += () => manager.IsArranging = false;
}
}
