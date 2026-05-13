using Godot;
using System;

public partial class SaveImage : Node2D
{
    public override void _Ready()
    {
        // Espera dois frames para garantir que o Viewport renderizou os sprites
        Callable.From(SaveImageA).CallDeferred();
    }

    private async void SaveImageA()
    {
        // Aguarda um tempinho para o motor renderizar tudo
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Pega a textura do Viewport
        var viewport = GetNode<SubViewport>("SubViewportContainer/SubViewport");
        Image img = viewport.GetTexture().GetImage();

        // Salva na pasta do seu projeto
        string path = "res://SombraCartaUnificada.png";
        img.SavePng(path);

        GD.Print("Sombra salva com sucesso em: " + path);
        // Pode fechar o jogo após salvar
        GetTree().Quit();
    }
}