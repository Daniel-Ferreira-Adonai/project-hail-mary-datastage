using Godot;
using System;

public partial class CardDisplay : SubViewportContainer
{
    [Export] SubViewport _viewport;
    [Export] public bool PlayRewardChosenAnimation { get; set; } = true;
    
    [Signal] public delegate void CardChosenEventHandler(CardData cardData);
    
    private CardData _cardData;

    public override void _Ready()
    {
		PivotOffset = new Vector2(Size.X / 2, Size.Y);

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        GuiInput += OnGuiInput;
    }

    

private void OnMouseEntered()
{
    var tween = CreateTween();
    tween.TweenProperty(this, "scale", new Vector2(1.15f, 1.15f), 0.15f)
         .SetTrans(Tween.TransitionType.Cubic)
         .SetEase(Tween.EaseType.Out);
}

private void OnMouseExited()
{
    var tween = CreateTween();
    tween.TweenProperty(this, "scale", Vector2.One, 0.15f)
         .SetTrans(Tween.TransitionType.Cubic)
         .SetEase(Tween.EaseType.Out);
}

    public void SetCard(CardData cardData)
    {
        _cardData = cardData;
        _viewport ??= GetNode<SubViewport>("SubViewport");
        var card = _viewport.GetChild<Card>(0);
        card.Position = _viewport.Size2DOverride / 2;
        card.Setup(cardData);
    }
	private void OnGuiInput(InputEvent @event)
	{
		if(@event is InputEventMouseButton mouse && 
		mouse.ButtonIndex == MouseButton.Left && 
		mouse.Pressed)
		{
			EmitSignal(SignalName.CardChosen, _cardData);
            if (PlayRewardChosenAnimation)
            {
			    PlayChosenAnimation();
            }
		}
	}

	private async void PlayChosenAnimation()
	{
        var picker = GetParent()?.GetParent()?.GetParentOrNull<PickCardRewards>();
        if (picker is null)
        {
            return;
        }

		MouseFilter = MouseFilterEnum.Ignore;
		
		var tween = CreateTween();
		tween.TweenProperty(this, "scale", new Vector2(1.3f, 1.3f), 0.4f)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
		tween.TweenInterval(0.15f);
		tween.TweenProperty(this, "position", Position + new Vector2(0, -150), 0.5f)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.In);
		tween.Parallel().TweenProperty(this, "modulate:a", 0.0f, 0.5f);

		foreach(var display in picker.GetCardDisplays())
		{
			if(display == this) continue;
			
			var randomX = (float)GD.RandRange(-200, 200);
			var randomRotation = (float)GD.RandRange(-0.5f, 0.5f);
			
			var otherTween = display.CreateTween();
			otherTween.TweenProperty(display, "position", 
				display.Position + new Vector2(randomX, 200), 0.4f)
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.In);
			otherTween.Parallel().TweenProperty(display, "rotation", randomRotation, 0.4f);
			otherTween.Parallel().TweenProperty(display, "modulate:a", 0.0f, 0.4f);
		}

		await ToSignal(tween, Tween.SignalName.Finished);
		
		picker.EmitSignal(PickCardRewards.SignalName.AnimationFinished); 
		picker.QueueFree();
	}
}
