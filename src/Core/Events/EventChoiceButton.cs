using Godot;
using System;

public partial class EventChoiceButton : PanelContainer
{
    [Export] private Label _choiceText;
    [Export] private RichTextLabel _effectText;
    
    private Action<EventChoice> _onSelected;
    private EventChoice _choice;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse && 
            mouse.ButtonIndex == MouseButton.Left && 
            mouse.Pressed)
        {
            _onSelected?.Invoke(_choice);
        }
    }

    public void Setup(EventChoice choice, Action<EventChoice> onSelected)
    {
        _choice = choice;
        _onSelected = onSelected;

        _choiceText.Text = choice.ChoiceText;
        
        string effectText = BuildEffectText(choice.Effects);
        _effectText.ParseBbcode(effectText);
        _effectText.Visible = !string.IsNullOrEmpty(effectText);
    }

    private string BuildEffectText(EventEffect[] effects)
    {
        if (effects == null || effects.Length == 0) return "";

        string result = "";
        foreach (var effect in effects)
        {
            result += effect.Type switch
            {
                EffectType.GainGold => $"[color=#FFD700]Gain {effect.Value} Gold[/color] ",
                EffectType.LoseGold => $"[color=#FF4444]Lose {effect.Value} Gold[/color] ",
                EffectType.LoseHP   => $"[color=#FF4444]Lose {effect.Value} HP[/color] ",
                EffectType.GainHP   => $"[color=#44FF44]Gain {effect.Value} HP[/color] ",
                _ => ""
            };
        }
        return result;
    }
}