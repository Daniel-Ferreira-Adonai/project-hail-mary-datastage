using Godot;
using System;

public partial class BackgroundVideo : VideoStreamPlayer
{
    [Export] public Godot.Collections.Array<Resource> Videos = new();

    public override void _Ready()
    {
        if (Videos.Count == 0) return;

        Stream = Videos[new Random().Next(Videos.Count)] as VideoStream;

        if (MusicManager.Instance != null)
        {
            string path = (Stream?.ResourcePath ?? "").ToLower();
            MusicManager.Instance.CurrentTheme = path.Contains("e_ce_d_f_e")
                ? MusicManager.MenuTheme.DrPeste
                : MusicManager.MenuTheme.PortaoTorre;
            MusicManager.Instance.PlayThemeMusic();
        }

        Play();
    }
}