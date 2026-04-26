using Godot;
using System;
using System.Collections.Generic;

public partial class PickCardRewards : PanelContainer
{
    [Signal] public delegate void CardChosenEventHandler();
    [Signal] public delegate void AnimationFinishedEventHandler();
    [Signal] public delegate void BackPressedEventHandler();

    List<CardDisplay> _cardDisplays = [];
    
    public override void _Ready()
    {
        foreach(var display in GetCardDisplays())
        {
            display.CardChosen += (cardData) =>
            {
                PlayerManager.Instance.Player.AddCardToDeck(cardData);
                EmitSignal(SignalName.CardChosen);
            };
        }
    }

    public void OnSkipPressed()
    {
        EmitSignal(SignalName.AnimationFinished);
        QueueFree();
    }

    public List<CardDisplay> GetCardDisplays()
    {
        var result = new List<CardDisplay>();
        var hbox = GetNode<VBoxContainer>("VBoxContainer").GetNode<HBoxContainer>("HBoxContainer");
        
        foreach(var child in hbox.GetChildren())
        {
            if(child is CardDisplay display)
                result.Add(display);
        }
        return result;
    }
    public void OnBackPressed()
    {
        EmitSignal(SignalName.BackPressed);
        QueueFree(); 
        
    }
}