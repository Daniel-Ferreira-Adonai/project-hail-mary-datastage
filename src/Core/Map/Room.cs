using Godot;
using System;
using System.Collections.Generic;

public partial class Room : Resource
{
   public enum RoomType
{
    NotAssigned, 
    Combat,
    Shop,
    Boss,
    Event,
    Chest,
    CampFire
}
    [Export] public RoomType EnumRoomType {get; set;}
    [Export] public int Row {get; set;}
    [Export] public int Column {get; set;}
    [Export] public Vector2 Position {get; set;}
    [Export] public Room[] NextRooms = new Room[0];
    [Export] public EncounterData Encounter { get; set; } 
    [Export] public EventData Event { get; set; }
    [Export] public bool Select {get; set;} = false;

    public override string ToString()
    {
        return $"{Column} e {EnumRoomType}";
    }

}
