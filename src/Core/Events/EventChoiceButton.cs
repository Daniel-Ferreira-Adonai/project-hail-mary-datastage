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
			 if (effect.secret == true)
			{
				result += "[color=#FFD700]???[/color] ";
				continue;
			}
            result += effect.Type switch
            {
                EffectType.GainGold => $"[color=#FFD700]Ganhe {effect.Value} Gold[/color] ",
				EffectType.LoseGold => $"[color=#FF4444]Perca {effect.Value} Gold[/color] ",
				EffectType.GainHP => $"[color=#44FF44]Ganhe {effect.Value} HP[/color] ",
				EffectType.LoseHP => $"[color=#FF4444]Perca {effect.Value} HP[/color] ",
				EffectType.GainCard => $"[color=#44FF44]Ganhe uma carta[/color] ",
				EffectType.RemoveCard => $"[color=#FFAA44]Remova uma carta[/color] ",
				EffectType.GainRelic => $"[color=#AA44FF]Ganhe {effect.Value} relíquia(s)[/color] ",
				EffectType.GainRandomCard => $"[color=#44FF44]Ganhe uma carta aleatória[/color] ",
				EffectType.UpgradeCard => $"[color=#44FF44]Melhore uma carta[/color] ",
				EffectType.UpgradeRandomCard => $"[color=#44FF44]Melhore uma carta aleatória[/color] ",
				EffectType.RemoveRandomCard => $"[color=#FF4444]Remova uma carta aleatória[/color] ",
				EffectType.RandomHpSwing => $"[color=#FFAA44]Ganhe ou Perca {effect.Value} HP (50/50)[/color] ",
				EffectType.RandomGoldSwing => $"[color=#FFAA44]Ganhe ou Perca {effect.Value} Gold (50/50)[/color] ",
                EffectType.RandomCardSwing => $"[color=#FFAA44]Melhore ou Remova uma carta aleatória (50/50)[/color] ",
                EffectType.DuplicateRandomCard => $"[color=#44FF44]Duplique uma carta aleatória[/color] ",
                EffectType.TransformRandomCard => $"[color=#AA44FF]Transforme uma carta aleatória[/color] ",
				EffectType.nothing => "",
				_ => ""
            };
        }
        return result;
    }
}