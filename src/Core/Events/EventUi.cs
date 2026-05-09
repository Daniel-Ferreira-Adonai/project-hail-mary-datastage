using Godot;
using System;

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

    private void OnChoiceSelected(EventChoice choice)
{
    _description.Text = choice.ResultText;

    foreach (Node child in _choicesContainer.GetChildren())
        child.QueueFree();

    ApplyEffects(choice.Effects);

    var continueBtn = new Button();
    continueBtn.Text = "Continuar";
    _choicesContainer.AddChild(continueBtn);
    continueBtn.Pressed += () => EmitSignal(SignalName.ExitRequested);
}
    private void ApplyEffects(EventEffect[] effects)
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
                case EffectType.GainCard:
                    break;
            }
        }
    }
}