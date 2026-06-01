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
	[Export] private float _playThresholdY = 0.7f; 

	[Export] private PackedScene _cardScene;  
	[Export] private Hand _handNode;  

	public int cardsDrawedPerTurn = 5;

	private List<Card> _deck = new List<Card>();  
	public List<Card> _handList = new List<Card>();      
	private List<Card> _discard = new List<Card>();  

	private List<Card> _exhausted = new List<Card>();

	public Dictionary<Card, float> _originalRotations = new Dictionary<Card, float>();
	public Dictionary<Card, Vector2> _originalPositions = new Dictionary<Card, Vector2>();
	public Dictionary<Card, Vector2> _originalScales = new Dictionary<Card, Vector2>();

	private Dictionary<Card, Tween> _activeTweens = new();

	public CombatManager _combatManager;
	public bool IsArranging { get; set; } = false;
	private Card _currentHoveredCard = null;

	private AudioStreamPlayer _clickSound;
	private AudioStreamPlayer _unclickSound;
	private AudioStreamPlayer _drawSound;

	private TargetingOverlay _targetingOverlay;
	private Enemy            _highlightedEnemy;
	private bool             _dragNeedsEnemy;

	private DrawPileUI   _drawPileUI;
	private CanvasLayer  _uiLayer;

	public override void _Ready()
	{
		_mouse = GetNode<MouseInputTracker>("/root/MouseTracker");
		_combatManager = GetParent<CombatManager>();
		_clickSound   = GetNodeOrNull<AudioStreamPlayer>("ClickSound");
		_unclickSound = GetNodeOrNull<AudioStreamPlayer>("UnclickSound");
		_drawSound    = GetNodeOrNull<AudioStreamPlayer>("DrawSound");

		CallDeferred(nameof(SetupCombatUI));
	}

	private void SetupCombatUI()
	{
		bool combatVisible = _combatManager.Visible;

		_targetingOverlay = new TargetingOverlay { ZAsRelative = false, ZIndex = 100, Visible = combatVisible };
		GetParent().AddChild(_targetingOverlay);

		_uiLayer = new CanvasLayer { Layer = 10, Visible = combatVisible };
		GetParent().AddChild(_uiLayer);

		var uiRoot = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
		uiRoot.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_uiLayer.AddChild(uiRoot);

		_drawPileUI = new DrawPileUI();
		uiRoot.AddChild(_drawPileUI);

		RefreshDrawPileUI();
	}

	public void SetCombatHUDVisible(bool visible)
	{
		if (_uiLayer          != null) _uiLayer.Visible          = visible;
		if (_targetingOverlay != null) _targetingOverlay.Visible = visible;
	}

	private void RefreshDrawPileUI()
	{
		_drawPileUI?.SetCount(_deck.Count);
	}

public async void DrawCard(int count)
{
   

    for (int i = 0; i < count; i++)
    {
		 if (_deck.Count <= 0)
        ShuffleDeck();

        if (_deck.Count == 0) return;
        if (_handList.Count >= 9) return;

        Card card = _deck[0];
        _deck.RemoveAt(0);
        RefreshDrawPileUI();

        _handNode.AddChild(card);
        card.Setup(card.Data);
        card.LoadVisuals();
        _handList.Add(card);

        card.UpdateDamagePreview(_combatManager.Player, _combatManager.RaycastCheckForEnemy());
        card.UpdateBlockPreview(_combatManager.Player);
        card.UpdatePlayableVisual(_combatManager.currentEnergy >= card.Data.EnergyCost);

        Vector2 fromPos = new Vector2(-500, 50f);
        if (_drawPileUI != null && IsInstanceValid(_drawPileUI))
            fromPos = _handNode.ToLocal(_drawPileUI.GetCenter());

        _handNode.AddCard(card, fromPos);
        _originalScales[card] = card.Scale;
        _drawSound?.Play();

        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
    }
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
		RefreshDrawPileUI();
	}
	public void DiscardHand()
{
    for (int i = _handList.Count - 1; i >= 0; i--)
    {
        Card card = _handList[i];
        
        
        handleCardDeckTurn(card);
    }

		_handList.Clear();
		_originalPositions.Clear();
		_originalRotations.Clear();
		_originalScales.Clear();
}
	public override void _Input(InputEvent @event)
{
    if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
    {
        if (CardBeingDraged != null)
        {
            FinishDrag();
            return;
        }

        Card card = RaycastCheckForCard();
        if (card is not null)
            StartDragging(card);
    }
}

	// public void ConnectCardSignals(Card card)
	// {
	// 	card.Connect(Card.SignalName.CardHovered, Callable.From<Card>(OnCardHovered));
	// 	card.Connect(Card.SignalName.CardUnhovered, Callable.From<Card>(OnCardUnhovered));
	// }

	// private void OnCardHovered(Card card)
	// {
	// 	if (!IsHoveringOnCard && !IsArranging)
	// 	{
	// 		IsHoveringOnCard = true;
	// 		HighlightCard(card, true);
	// 	}
	// }

	// private void OnCardUnhovered(Card card)
	// {
	// 	if (CardBeingDraged is null)
	// 	{
	// 		HighlightCard(card, false);
	// 		Card newCard = RaycastCheckForCard();
	// 		if (newCard != null)
	// 			HighlightCard(newCard, true);
	// 		else
	// 			IsHoveringOnCard = false;
	// 	}
	// }

public override void _Process(double delta)
{
    if (CardBeingDraged != null)
    {
        var dd = CardBeingDraged.Data;
        bool isInspect = (dd.tipoCarta == CardData.CardType.Attack
                       || dd.tipoCarta == CardData.CardType.SkillWithEnemyEffect)
                       && !dd.IsAoe;
        if (!isInspect)
            DragLogic(delta);

        Enemy hoveredEnemy = _combatManager.RaycastCheckForEnemy();
        CardBeingDraged.UpdateDamagePreview(_combatManager.Player, hoveredEnemy);
        UpdateTargetingOverlay(hoveredEnemy);
        return;
    }
	
    Card hovered = RaycastCheckForCard();

    if (_currentHoveredCard != null && (!GodotObject.IsInstanceValid(_currentHoveredCard) || _currentHoveredCard.IsQueuedForDeletion()))
    {
        _currentHoveredCard = null;
    }

    if (hovered != _currentHoveredCard)
    {
        if (GodotObject.IsInstanceValid(_currentHoveredCard))
            HighlightCard(_currentHoveredCard, false);

        _currentHoveredCard = hovered;

        if (GodotObject.IsInstanceValid(_currentHoveredCard))
            HighlightCard(_currentHoveredCard, true);
    }
}
	private void HighlightCard(Card card, bool hovered)
{
    if (!GodotObject.IsInstanceValid(card) || card.IsQueuedForDeletion())
    {
        _activeTweens.Remove(card);
        return;
    }

    if (_activeTweens.TryGetValue(card, out var existing))
    {
        if (GodotObject.IsInstanceValid(existing)) 
        {
            existing.Kill();
        }
        _activeTweens.Remove(card);
    }

    var shadow = card.GetNodeOrNull<Node2D>("CardShadow");
    if (shadow == null) return; 

    var tween = CreateTween().SetParallel();
    tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
    _activeTweens[card] = tween;

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

        tween.TweenProperty(card, "position", new Vector2(originalPos.X, originalPos.Y - moveUp), 0.2f);
        tween.TweenProperty(card, "rotation_degrees", 0f, 0.2f);
        tween.TweenProperty(card, "scale", new Vector2(scale.X * _hoverScale, scale.Y * _hoverScale), 0.2f);

        tween.TweenProperty(shadow, "position", new Vector2(18f, 0f), 0.2f);
        tween.TweenProperty(shadow, "modulate:a", 0.6f, 0.2f);
        tween.TweenProperty(shadow, "scale", new Vector2(1.1f, 1.1f), 0.2f);

        card.ZIndex = 11;
    }
    else
    {
        if (_originalPositions.ContainsKey(card))
        {
            tween.TweenProperty(card, "position", _originalPositions[card], 0.15f);
            tween.TweenProperty(card, "rotation_degrees", _originalRotations[card], 0.15f);
        }

        Vector2 scale = _originalScales.ContainsKey(card) ? _originalScales[card] : card.Scale;
        tween.TweenProperty(card, "scale", scale, 0.15f);

        tween.TweenProperty(shadow, "position", new Vector2(5f, -10f), 0.15f);
        tween.TweenProperty(shadow, "modulate:a", 0.25f, 0.15f);
        tween.TweenProperty(shadow, "scale", Vector2.One, 0.15f);

        int index = _handList.IndexOf(card);
        card.ZIndex = index >= 0 ? index + 1 : 1;
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
		KillActiveTween(card);
		CardBeingDraged = card;
		_clickSound?.Play();

		var d = card.Data;
		bool isInspect = (d.tipoCarta == CardData.CardType.Attack
		               || d.tipoCarta == CardData.CardType.SkillWithEnemyEffect)
		               && !d.IsAoe;

		if (isInspect)
		{
			Vector2 baseScale = _originalScales[card];

			float centerX, lowY;
			if (_originalPositions.Count > 0)
			{
				float minX = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
				foreach (var pos in _originalPositions.Values)
				{
					minX = Mathf.Min(minX, pos.X);
					maxX = Mathf.Max(maxX, pos.X);
					maxY = Mathf.Max(maxY, pos.Y);
				}
				centerX = (minX + maxX) * 0.5f;
				lowY    = maxY;
			}
			else { centerX = card.Position.X; lowY = card.Position.Y; }

			Vector2 inspectPos = new Vector2(centerX, lowY - 160f);
			card.ZIndex = 50;
			var t = CreateTween().SetParallel().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
			t.TweenProperty(card, "position", inspectPos, 0.15f);
			t.TweenProperty(card, "rotation_degrees", 0f, 0.15f);
			t.TweenProperty(card, "scale", baseScale * 1.4f, 0.15f);
		}
		else
		{
			card.Scale = _originalScales[card];
		}

		UpdateTargetingOverlay(_combatManager.RaycastCheckForEnemy());
	}
	public void UpdateAllCardPreviews(Player player, Enemy target = null)
{
    foreach (Card card in _handNode.getHandCards())
    {
		GD.Print(card.GetNode<RichTextLabel>("Descricao").Text);
        card.UpdateDamagePreview(player, target);
        card.UpdateBlockPreview(player);
		GD.Print(card.GetNode<RichTextLabel>("Descricao").Text);
    }
    RefreshPlayableVisuals();
}

public void RefreshPlayableVisuals()
{
    int energy = _combatManager.currentEnergy;
    foreach (var card in _handList)
    {
        if (GodotObject.IsInstanceValid(card) && !card.IsQueuedForDeletion())
            card.UpdatePlayableVisual(energy >= card.Data.EnergyCost);
    }
}
	private void FinishDrag()
	{
		if (CardBeingDraged is null) return;
		ClearTargeting();

		Vector2 scale = _originalScales[CardBeingDraged];
		CardBeingDraged.Scale = new Vector2(scale.X, scale.Y);
		CardBeingDraged.Rotation = 0f;
		_unclickSound?.Play();
		TryToPlayCard(CardBeingDraged);

		CardBeingDraged = null;

		if (_currentHoveredCard != null)
	{
		HighlightCard(_currentHoveredCard, false);
		_currentHoveredCard = null;
	}
		_originalPositions.Clear();
		_originalRotations.Clear();
		IsHoveringOnCard = false;
		_handNode.ArrangeFan();
	}
public async void TryToPlayCard(Card card)
{
    if (!IsInPlayZone() || CardBeingDraged is null)
    {
        return;
    }

	foreach (var e in CombatManager.Instance._activeEnemies)
    e.TriggerPowers(p => p.OnPlayerCardPlayed(e, CardBeingDraged.Data));

    if(CardBeingDraged.Data.tipoCarta == CardData.CardType.Attack
        || CardBeingDraged.Data.tipoCarta == CardData.CardType.SkillWithEnemyEffect)
    {
        Enemy Enemy = null;

        if(CardBeingDraged.Data.IsAoe)
		{
			if(!_combatManager.canPlayCard(CardBeingDraged)) return; 
			Enemy = _combatManager.RaycastCheckForEnemy() ?? _combatManager.GetFirstEnemy();
			_combatManager.handleCardPlayed(CardBeingDraged);
		}
        else
        {
            Enemy = _combatManager.getEnemy(CardBeingDraged);
        }

        GD.Print(Enemy);
        if(Enemy is Enemy enemy)
		{
		var playedCard = CardBeingDraged;
		PlayerManager.Instance.Player.PlayAttackAnimation(enemy);
		GD.Print(_combatManager.getPlayer() + " aaaaaaaa");
		_combatManager.Player.TriggerRelics(r => r.BeforeCardIsPlayed(_combatManager.Player, playedCard.Data)); 
		playedCard.Play(enemy, _combatManager.getPlayer());
		_combatManager.Player.TriggerRelics(r => r.OnCardPlayed(_combatManager.Player, playedCard.Data)); 
		UpdateAllCardPreviews(PlayerManager.Instance.Player, null);
		handleCardDeckTurn(card);
	} 
	return;
    }
    
    if(CardBeingDraged.Data.tipoCarta != CardData.CardType.Attack)
    {
        Player player = _combatManager.getPlayer(card);
        if(player is Player Player)
        {
            GD.Print("entrei aq");
            _combatManager.Player.TriggerRelics(r => r.BeforeCardIsPlayed(_combatManager.Player, CardBeingDraged.Data)); 
            CardBeingDraged.Play(null, Player);
            _combatManager.Player.TriggerRelics(r => r.OnCardPlayed(_combatManager.Player, CardBeingDraged.Data)); 
            UpdateAllCardPreviews(PlayerManager.Instance.Player, null);
            handleCardDeckTurn(card);
        }
    }
}
	public void handleCardDeckTurn(Card card)
{
    if (card == _currentHoveredCard) _currentHoveredCard = null;
    if (card == CardBeingDraged) CardBeingDraged = null;

    if (_activeTweens.TryGetValue(card, out var tween))
    {
        tween.Kill();
        _activeTweens.Remove(card);
    }

    _handList.Remove(card);
	_handNode.RemoveCard(card);
    _handNode.RemoveChild(card);
	
	if (card.Data is PowerData power)
	{
		card.Data.IsExhausted = true;
	}
	if (card.Data.IsExhausted)
        _exhausted.Add(card); 
    else
        _discard.Add(card); 

    _originalPositions.Remove(card);
    _originalRotations.Remove(card);
    _originalScales.Remove(card);

}
		private bool IsInPlayZone()
	{
		return _mouse.ScreenPosition.Y < ScreenSize.Y * _playThresholdY;
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

		
		var shadow = CardBeingDraged.GetNode<Node2D>("CardShadow");

		Vector2 baseOffset = new Vector2(12f, 0f);

		float rotationFactor = Mathf.Abs(CardBeingDraged.Rotation) / _maxCardRotation;
		float liftAmount = Mathf.Lerp(0f, 8f, rotationFactor);

		Vector2 targetShadowPos = baseOffset + new Vector2(0f, liftAmount);

		shadow.Position = shadow.Position.Lerp(targetShadowPos, (float)delta * 20f);

		float targetOpacity = Mathf.Lerp(0.35f, 0.55f, rotationFactor);
		shadow.Modulate = shadow.Modulate.Lerp(
			new Color(0f, 0f, 0f, targetOpacity),
			(float)delta * 20f
		);
	}
	public void OrganizeHand()
	{
		_handNode.ArrangeFan();
	}
public void StartDeck()
{
    var playerDeck = PlayerManager.Instance.Player.GetDeck();

    foreach (var cardData in playerDeck)
    {
        Card card = _cardScene.Instantiate<Card>();
        card.Data = cardData;
        _deck.Add(card);
    }

    RefreshDrawPileUI();
    DrawCard(cardsDrawedPerTurn + PlayerManager.Instance.Player.BonusCardsToDraw);
    PlayerManager.Instance.Player.BonusCardsToDraw = 0;
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
	private void KillActiveTween(Card card)
{
    if (_activeTweens.TryGetValue(card, out var tween))
    {
        if (GodotObject.IsInstanceValid(tween))
        {
            tween.Kill();
        }
        _activeTweens.Remove(card);
    }
}

private void UpdateTargetingOverlay(Enemy hoveredEnemy)
{
    if (_targetingOverlay is null || CardBeingDraged is null) return;
    var d = CardBeingDraged.Data;
    bool enemyCard = d.tipoCarta == CardData.CardType.Attack
                  || d.tipoCarta == CardData.CardType.SkillWithEnemyEffect;

    if (enemyCard && d.IsAoe)
    {
        _targetingOverlay.HideArrow();
        SetHighlightedEnemy(null);
        _targetingOverlay.ShowReticles(BuildEnemyReticles());
    }
    else if (enemyCard)
    {
        Vector2 origin = CardBeingDraged.GlobalPosition - new Vector2(0, 90f);
        _targetingOverlay.ShowArrow(origin, _mouse.ScreenPosition);
        SetHighlightedEnemy(hoveredEnemy);
        _targetingOverlay.ShowReticles(hoveredEnemy != null
            ? new List<Rect2> { ReticleFor(hoveredEnemy, 220f, 240f, -40f) }
            : new List<Rect2>());
    }
    else
    {
        _targetingOverlay.HideArrow();
        SetHighlightedEnemy(null);
        _targetingOverlay.ShowReticles(new List<Rect2> { ReticleForPlayer() });
    }
}

private Rect2 ReticleFor(Node2D unit, float w, float h, float yOff)
{
    Vector2 c = unit.GlobalPosition + new Vector2(0, yOff);
    return new Rect2(c - new Vector2(w, h) * 0.5f, new Vector2(w, h));
}

private List<Rect2> BuildEnemyReticles()
{
    var list = new List<Rect2>();
    foreach (var e in _combatManager._activeEnemies)
        if (e != null && IsInstanceValid(e)) list.Add(ReticleFor(e, 220f, 240f, -40f));
    return list;
}

private Rect2 ReticleForPlayer() => ReticleFor(_combatManager.Player, 180f, 280f, -40f);

private void SetHighlightedEnemy(Enemy next)
{
    if (next == _highlightedEnemy) return;

    if (_highlightedEnemy is not null && IsInstanceValid(_highlightedEnemy))
        _highlightedEnemy.Modulate = _highlightedEnemy.Data?.Tint ?? Colors.White;

    _highlightedEnemy = next;

    if (_highlightedEnemy is not null && IsInstanceValid(_highlightedEnemy))
        _highlightedEnemy.Modulate = new Color(1.25f, 1.18f, 0.8f, 1f);
}

private void ClearTargeting()
{
    _targetingOverlay?.HideArrow();
    _targetingOverlay?.HideReticles();
    SetHighlightedEnemy(null);
}
public void ResetDeck()
{
    foreach (var card in _handNode.getHandCards())
        if (GodotObject.IsInstanceValid(card))
            card.QueueFree();

    _handNode.ClearList();

    _deck.Clear();
    _discard.Clear();
    _handList.Clear();
	_exhausted.Clear();
    _originalPositions.Clear();
    _originalRotations.Clear();
    _originalScales.Clear();
    _activeTweens.Clear();
    _currentHoveredCard = null;
    CardBeingDraged = null;

    PlayerManager.Instance.Player.ActivePowers.Clear(); // aqui
    PlayerManager.Instance.Player.UpdateLabelValues();
    RefreshDrawPileUI();
}
}