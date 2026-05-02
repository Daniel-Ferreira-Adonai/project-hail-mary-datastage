using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PickCardRewards : PanelContainer
{
    [Signal] public delegate void CardChosenEventHandler();
    [Signal] public delegate void AnimationFinishedEventHandler();
    [Signal] public delegate void BackPressedEventHandler();
    public List<CardData> CardsToDisplay { get; set; } = new();


    List<CardDisplay> _cardDisplays = [];
    
    public override void _Ready()
{
    var displays = GetCardDisplays();

    for (int i = 0; i < displays.Count && i < CardsToDisplay.Count; i++)
    {
        displays[i].SetCard(CardsToDisplay[i]);
    }

    foreach(var display in displays)
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