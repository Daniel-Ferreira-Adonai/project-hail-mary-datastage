using Godot;
using System;
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
                    var relic = GetRandomRelic();
                    if (relic != null)
                    {
                        PlayerManager.Instance.Player.AddRelic(relic);
                    }
                    break;
				case EffectType.RandomHpSwing:
					hpSwingEvent(effect, choice);
					break;
				case EffectType.RandomGoldSwing:
					GoldSwingEvent(effect, choice);
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
        var allRelics = new System.Collections.Generic.List<RelicData>();

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

        var rng = new Random();
        return allRelics[rng.Next(allRelics.Count)];
    }
}