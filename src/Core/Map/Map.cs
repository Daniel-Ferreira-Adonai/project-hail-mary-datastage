
using Godot;
using System;
using System.Linq;
public partial class Map : Node2D
{
    public const int SCROLL_SPEED = 15;

    private static readonly PackedScene MAP_ROOM = GD.Load<PackedScene>("res://src/Core/Map/MapRoom.tscn");
    private static readonly PackedScene MAP_LINE = GD.Load<PackedScene>("res://src/Core/Map/MapLine.tscn");
	
    private MapGenerator _mapGenerator;
    private Node2D _lines;
    private Node2D _rooms;
    private Node2D _visuals;
    private Camera2D _camera2D;

    private Room[][] _mapData;
    public int _floorsClimbed;
    private Room _lastRoom;
    private float _cameraEdgeY;
	public override void _Ready()
{
    _mapGenerator = GetNode<MapGenerator>("MapGenerator");
    _lines        = GetNode<Node2D>("%Lines");
    _rooms        = GetNode<Node2D>("%Rooms");
    _visuals      = GetNode<Node2D>("Visuals");
    _camera2D     = GetNode<Camera2D>("Camera2D");

    _cameraEdgeY = MapGenerator.Y_DIST * (MapGenerator.FLOORS - 1);

    GenerateNewMap();
    UnlockFloor(0);
    
}

public override void _Input(InputEvent @event)
{
    if (@event.IsActionPressed("scroll_up"))
        _camera2D.Position = new Vector2(_camera2D.Position.X, _camera2D.Position.Y - SCROLL_SPEED);
    else if (@event.IsActionPressed("scroll_down"))
        _camera2D.Position = new Vector2(_camera2D.Position.X, _camera2D.Position.Y + SCROLL_SPEED);

    _camera2D.Position = new Vector2(
        _camera2D.Position.X,
        Mathf.Clamp(_camera2D.Position.Y, -_cameraEdgeY, 0)
    );
}

public void GenerateNewMap()
{
    _floorsClimbed = 0;
    RunStats.CurrentFloor = 0;
    foreach (var c in _rooms.GetChildren()) { _rooms.RemoveChild(c); c.QueueFree(); }
    foreach (var c in _lines.GetChildren()) { _lines.RemoveChild(c); c.QueueFree(); }
    _mapData = _mapGenerator.GenerateMap();
    CreateMap();
    _camera2D.Position = new Vector2(_camera2D.Position.X, 0);
}

private void CreateMap()
{
    foreach (Room[] currentFloor in _mapData)
        foreach (Room room in currentFloor)
            if (room.NextRooms.Length > 0)
                _SpawnRoom(room);

    int middle = Mathf.FloorToInt(MapGenerator.MAP_WIDTH * 0.5f);
    _SpawnRoom(_mapData[MapGenerator.FLOORS - 1][middle]);

    int mapWidthPixels = MapGenerator.X_DIST * (MapGenerator.MAP_WIDTH - 1);
    _visuals.Position = new Vector2(
        (GetViewportRect().Size.X - mapWidthPixels) / 2,
        GetViewportRect().Size.Y / 2
    );
}

public void UnlockFloor(int whichFloor = -1)
{
    if (whichFloor == -1) whichFloor = _floorsClimbed;

    foreach (MapRoom mapRoom in _rooms.GetChildren().Cast<MapRoom>())
        if (mapRoom.Room.Row == whichFloor)
            mapRoom.Available = true;
}

public void UnlockNextRooms()
{
    foreach (MapRoom mapRoom in _rooms.GetChildren().Cast<MapRoom>())
        if (Array.Exists(_lastRoom.NextRooms, r => r == mapRoom.Room))
            mapRoom.Available = true;
}

public void ShowMap()
{
    Show();
    _camera2D.Enabled = true;
    ZIndex = 10; // acima do combate
    MusicManager.Instance?.PlayThemeMusic();
}

public void HideMap()
{
    Hide();
    _camera2D.Enabled = false;
    ZIndex = 0;
}

private void _SpawnRoom(Room room)
{
    var newMapRoom = MAP_ROOM.Instantiate<MapRoom>();
    _rooms.AddChild(newMapRoom);
    newMapRoom.Room = room;
    newMapRoom.Selected += _OnMapRoomSelected;
    _ConnectLines(room);

    if (room.Select && room.Row < _floorsClimbed)
        newMapRoom.ShowSelected();
}

private void _ConnectLines(Room room)
{
    if (room.NextRooms.Length == 0)
        return;

    foreach (Room next in room.NextRooms)
    {
        var newMapLine = MAP_LINE.Instantiate<Line2D>();
        newMapLine.AddPoint(room.Position);
        newMapLine.AddPoint(next.Position);
        _lines.AddChild(newMapLine);
    }
}

private void _OnMapRoomSelected(Room room)
{
    foreach (MapRoom mapRoom in _rooms.GetChildren().Cast<MapRoom>())
        if (mapRoom.Room.Row == room.Row)
            mapRoom.Available = false;

    _lastRoom = room;
    _floorsClimbed += 1;
    RunStats.CurrentFloor = _floorsClimbed;
    GameManager.Instance.OnMapRoomSelected(room);
}

// ── Save / Load ───────────────────────────────────────────────────────────────

public MapSave BuildMapSave()
{
    int middle = Mathf.FloorToInt(MapGenerator.MAP_WIDTH * 0.5f);
    var save = new MapSave
    {
        Floor         = _floorsClimbed,
        CurrentNodeId = _lastRoom != null
            ? _lastRoom.Row * MapGenerator.MAP_WIDTH + _lastRoom.Column
            : -1,
    };

    for (int i = 0; i < MapGenerator.FLOORS; i++)
    {
        for (int j = 0; j < MapGenerator.MAP_WIDTH; j++)
        {
            var room = _mapData[i][j];
            bool isBoss = (i == MapGenerator.FLOORS - 1 && j == middle);
            if (room.NextRooms.Length == 0 && !isBoss && !room.Select) continue;

            var rs = new RoomSave
            {
                Row           = room.Row,
                Column        = room.Column,
                PosX          = room.Position.X,
                PosY          = room.Position.Y,
                RoomType      = (int)room.EnumRoomType,
                EncounterPath = room.Encounter?.ResourcePath ?? "",
                EventPath     = room.Event?.ResourcePath    ?? "",
                Select        = room.Select,
            };
            foreach (var next in room.NextRooms)
                rs.NextRoomIds.Add(new[] { next.Row, next.Column });
            save.Rooms.Add(rs);
        }
    }
    return save;
}

public void LoadFromSave(MapSave save)
{
    // Clear existing visuals
    foreach (var c in _rooms.GetChildren()) { _rooms.RemoveChild(c); c.QueueFree(); }
    foreach (var c in _lines.GetChildren()) { _lines.RemoveChild(c); c.QueueFree(); }

    // Fresh grid (all rooms initialised with row/column so unused slots are valid)
    _mapData = new Room[MapGenerator.FLOORS][];
    for (int i = 0; i < MapGenerator.FLOORS; i++)
    {
        _mapData[i] = new Room[MapGenerator.MAP_WIDTH];
        for (int j = 0; j < MapGenerator.MAP_WIDTH; j++)
            _mapData[i][j] = new Room { Row = i, Column = j };
    }

    // Fill saved rooms
    foreach (var rs in save.Rooms)
    {
        var room = _mapData[rs.Row][rs.Column];
        room.Position     = new Vector2(rs.PosX, rs.PosY);
        room.EnumRoomType = (Room.RoomType)rs.RoomType;
        room.Select       = rs.Select;
        room.Encounter    = !string.IsNullOrEmpty(rs.EncounterPath) ? GD.Load<EncounterData>(rs.EncounterPath) : null;
        room.Event        = !string.IsNullOrEmpty(rs.EventPath)     ? GD.Load<EventData>(rs.EventPath)         : null;
    }

    // Link NextRooms by coordinate
    foreach (var rs in save.Rooms)
    {
        var room  = _mapData[rs.Row][rs.Column];
        var nexts = new System.Collections.Generic.List<Room>();
        foreach (var id in rs.NextRoomIds)
            nexts.Add(_mapData[id[0]][id[1]]);
        room.NextRooms = nexts.ToArray();
    }

    _floorsClimbed        = save.Floor;
    RunStats.CurrentFloor = _floorsClimbed;

    CreateMap();

    // Scroll camera to current floor
    float camY = -_floorsClimbed * MapGenerator.Y_DIST;
    _camera2D.Position = new Vector2(_camera2D.Position.X, Mathf.Clamp(camY, -_cameraEdgeY, 0));

    if (save.CurrentNodeId >= 0)
    {
        int row = save.CurrentNodeId / MapGenerator.MAP_WIDTH;
        int col = save.CurrentNodeId % MapGenerator.MAP_WIDTH;
        _lastRoom = _mapData[row][col];
        UnlockNextRooms();
    }
    else
    {
        UnlockFloor(0);
    }
}
}
