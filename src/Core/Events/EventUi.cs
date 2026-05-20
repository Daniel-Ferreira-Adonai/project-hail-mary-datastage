using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class EventUi  : Control
{
    [Export] private TextureRect _background;
    [Export] private Label _title;
    [Export] private RichTextLabel _description;
    [Export] private VBoxContainer _choicesContainer;
    [Export] private PackedScene _choiceButtonScene;
[Signal] public delegate void ExitRequestedEventHandler();

    private EventData _currentEvent;

    public override void _Ready()
    {
        EventData testEvent = GD.Load<EventData>("res://Data/Events/MercadorSuspeitoEvento.tres");
        LoadEvent(testEvent);
    }

    public void LoadEvent(EventData eventData)
    {
        _currentEvent = eventData;
        _background.Texture = eventData.Art;
        _title.Text = eventData.EventName;
        _description.Text = eventData.Description;

        foreach (Node child in _choicesContainer.GetChildren())
            child.QueueFree();

        foreach (var choice in eventData.Choices)
        {
            var btn = _choiceButtonScene.Instantiate<EventChoiceButton>();
            _choicesContainer.AddChild(btn);
            btn.Setup(choice, OnChoiceSelected);
        }
    }

private async void OnChoiceSelected(EventChoice choice)
	{
		_description.Text = choice.ResultText;

		foreach (Node child in _choicesContainer.GetChildren())
			child.QueueFree();

		await ApplyEffects(choice.Effects, choice); 

		var continueBtn = new Button();
		continueBtn.Text = "Continuar";
		_choicesContainer.AddChild(continueBtn);
		continueBtn.Pressed += () => EmitSignal(SignalName.ExitRequested);
	}
    private async Task ApplyEffects(EventEffect[] effects, EventChoice choice)
    {
        if (effects == null) return;

        foreach (var effect in effects)
        {
            switch (effect.Type)
            {
                case EffectType.GainGold:
                    PlayerManager.Instance.Player.Gold += effect.Value;
                    break;
				case EffectType.LoseGold:
                    PlayerManager.Instance.Player.Gold -= effect.Value;
                    break;
                case EffectType.LoseHP:
                    PlayerManager.Instance.Player.TakeDamage(effect.Value);
                    break;
                case EffectType.GainHP:
                    PlayerManager.Instance.Player.TryToHeal(effect.Value);
                    break;
				case EffectType.UpgradeRandomCard:
                    var upgradedData = PlayerManager.Instance.Player.UpgradeRandomCard();
					if (upgradedData != null)
					{
						await ShowUpgradeAnimation(upgradedData);
					}
                    break;
				 case EffectType.GainRelic:
                    for(int i = 0; i < effect.Value; i++)
                    {
                    var relic = GetRandomRelic();
                    if (relic != null)
                    {
                        PlayerManager.Instance.Player.AddRelic(relic);
                    }
                    }
                    break;
				case EffectType.RandomHpSwing:
					hpSwingEvent(effect, choice);
					break;
				case EffectType.RandomGoldSwing:
					GoldSwingEvent(effect, choice);
					break;
				case EffectType.RemoveRandomCard:
				var removedData = PlayerManager.Instance.Player.RemoveRandomCard();
				if (removedData != null)
					await ShowRemoveAnimation(removedData);
				break;
				case EffectType.RandomCardSwing:
					await CardSwingEvent(effect, choice);
					break;
                case EffectType.DuplicateRandomCard:
                var duplicated = PlayerManager.Instance.Player.DuplicateRandomCard();
                if (duplicated != null)
                    await ShowDuplicateAnimation(duplicated); // reusa a animação existente
                break;
                case EffectType.GainCard:
                    break;

            }
        }
    }
private async Task ShowUpgradeAnimation(CardData data)
	{
		var cardScene = GD.Load<PackedScene>("res://src/Core/Card/Card.tscn");
		var card = cardScene.Instantiate<Card>();
		
		// Control container centralizado
		var container = new Control();
		container.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		
		UI.Instance.AddUI(container);
		container.AddChild(card);
		
		card.Setup(data);
		
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		
		var viewportSize = GetViewport().GetVisibleRect().Size;
		card.GlobalPosition = new Vector2(
			viewportSize.X / 2,
			viewportSize.Y / 2
		);
		card.ZIndex = 100;
		card.Data.UpgradeCard(); 
		card.Setup(data); 
		await card.PlayUpgradeAnimation();
		
		
		container.QueueFree();
	}
	private void hpSwingEvent(EventEffect effect, EventChoice choice)
{
    bool isGood = GD.Randf() > 0.5f;
    string[] texts = choice.ResultText.Split("|");

    if (isGood)
    {
        PlayerManager.Instance.Player.TryToHeal(effect.Value);
        _description.Text = texts[0];
    }
    else
    {
        PlayerManager.Instance.Player.TakeDamage(effect.Value);
        _description.Text = texts.Length > 1 ? texts[1] : texts[0];
    }
}
	private void GoldSwingEvent(EventEffect effect, EventChoice choice)
{
    bool isGood = GD.Randf() > 0.5f;
    string[] texts = choice.ResultText.Split("|");

    if (isGood)
    {
        PlayerManager.Instance.Player.Gold += effect.Value;
        _description.Text = texts[0];
    }
    else
    {
        PlayerManager.Instance.Player.Gold -= effect.Value;
        _description.Text = texts.Length > 1 ? texts[1] : texts[0];
    }
}
private RelicData GetRandomRelic()
{
    var allRelics = new List<RelicData>();

    var files = DirAccess.GetFilesAt("res://Data/Relics/");
    foreach (var file in files)
    {
        if (file.EndsWith(".tres"))
        {
            var relic = GD.Load<RelicData>($"res://Data/Relics/{file}");
            if (relic != null)
                allRelics.Add(relic);
        }
    }

    if (allRelics.Count == 0) return null;

    var validRelics = new List<RelicData>();

    Player player = PlayerManager.Instance.Player;

    foreach (var relic in allRelics)
    {
        if (relic.PlayerEnum == PlayerEnum.any || relic.PlayerEnum == player.playerEnum)
            validRelics.Add(relic);
    }

    if (validRelics.Count == 0) return null;

    var rng = new Random();
    return validRelics[rng.Next(validRelics.Count)];
}
	private async Task CardSwingEvent(EventEffect effect, EventChoice choice)
{
    bool isGood = GD.Randf() > 0.5f;
    string[] texts = choice.ResultText.Split("|");

    if (isGood)
    {
        var upgradedData = PlayerManager.Instance.Player.UpgradeRandomCard();
        if (upgradedData != null)
        {
            _description.Text = texts[0];
            await ShowUpgradeAnimation(upgradedData);
        }
    }
    else
    {
        var removedData = PlayerManager.Instance.Player.RemoveRandomCard();
        if (removedData != null)
        {
            _description.Text = texts.Length > 1 ? texts[1] : texts[0];
            await ShowRemoveAnimation(removedData);
        }
    }
}
	private async Task ShowRemoveAnimation(CardData data)
{
    var cardScene = GD.Load<PackedScene>("res://src/Core/Card/Card.tscn");
    var card = cardScene.Instantiate<Card>();
    
    var container = new Control();
    container.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    
    UI.Instance.AddUI(container);
    container.AddChild(card);
    
    card.Setup(data);
    
    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    
    var viewportSize = GetViewport().GetVisibleRect().Size;
    card.GlobalPosition = new Vector2(
        viewportSize.X / 2,
        viewportSize.Y / 2
    );
    card.ZIndex = 100;
    
    await card.PlayRemoveAnimation();
    
    container.QueueFree();
}
private async Task ShowTransformAnimation(CardData removedData, CardData newData)
{
    var cardScene = GD.Load<PackedScene>("res://src/Core/Card/Card.tscn");
    
    var container = new Control();
    container.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    UI.Instance.AddUI(container);

    var viewportSize = GetViewport().GetVisibleRect().Size;
    Vector2 center = new Vector2(viewportSize.X / 2, viewportSize.Y / 2);

    // carta antiga sendo transformada
    var oldCard = cardScene.Instantiate<Card>();
    container.AddChild(oldCard);
    oldCard.Setup(removedData);
    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    oldCard.GlobalPosition = center;
    oldCard.ZIndex = 100;
    await oldCard.PlayTransformCardStart();
    oldCard.QueueFree();

    // carta nova aparecendo
    var newCard = cardScene.Instantiate<Card>();
    container.AddChild(newCard);
    newCard.Setup(newData);
    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    newCard.GlobalPosition = center;
    newCard.ZIndex = 100;
    await newCard.PlayTransformCardEnd();

    container.QueueFree();
}
private async Task ShowDuplicateAnimation(CardData data)
{
    var cardScene = GD.Load<PackedScene>("res://src/Core/Card/Card.tscn");
    var card = cardScene.Instantiate<Card>();

    var container = new Control();
    container.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

    UI.Instance.AddUI(container);
    container.AddChild(card);

    card.Setup(data);

    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

    var viewportSize = GetViewport().GetVisibleRect().Size;
    card.GlobalPosition = new Vector2(
        viewportSize.X / 2,
        viewportSize.Y / 2
    );
    card.ZIndex = 100;

    await card.PLayShowCard();

    container.QueueFree();
}
}