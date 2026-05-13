using Godot;
using System;

public partial class ChestButton : TextureButton
{
    [Export] AnimationPlayer _animationPlayer;
    [Export] RelicForShopOrChest _relic;
    
    private bool _opened = false;

    public override void _Ready()
    {
        Pressed += OnPressed;
    }

private async void OnPressed()
{
    if (_opened) return;
    _opened = true;
    
    Disabled = true;
    TextureNormal = TexturePressed; // mantém textura de aberto

    _animationPlayer.Play("relic_appear");
    await ToSignal(_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);

    _relic.Enable();
}
}