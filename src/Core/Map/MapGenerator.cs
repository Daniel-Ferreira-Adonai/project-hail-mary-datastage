using Godot;
using System;
using System.Collections.Generic;

public partial class MapGenerator : Node
{
    public const int X_DIST = 200;
    public const int Y_DIST = 120;
    public const int PLACEMENT_RANDOMNESS = 20;
    public const int FLOORS = 15;
    public const int MAP_WIDTH = 7;
    public const int PATHS = 6;

    public const float MONSTER_ROOM_WEIGHT = 10.0f;
    public const float SHOP_ROOM_WEIGHT = 2.5f;
    public const float CAMPFIRE_ROOM_WEIGHT = 4.0f;
    [Export] public EncounterData[] EasyEncounters = new EncounterData[0];   
	[Export] public EncounterData[] MediumEncounters = new EncounterData[0]; 
	[Export] public EncounterData[] HardEncounters = new EncounterData[0];   
	[Export] public EncounterData[] BossEncounters = new EncounterData[0];   
    private Dictionary<Room.RoomType, float> _roomWeights = new()
    {
        { Room.RoomType.Combat, MONSTER_ROOM_WEIGHT },
        { Room.RoomType.Shop, SHOP_ROOM_WEIGHT },
        { Room.RoomType.CampFire, CAMPFIRE_ROOM_WEIGHT },
    };

    private float _randomRoomTypeTotalWeight = 0f;
    public Room[][] MapData = new Room[FLOORS][];

    public override void _Ready()
    {
        GenerateMap();
    }

    public override void _Process(double delta) { }

    // Equivalent to GDScript: func generate_map() -> Array[Array]
public Room[][] GenerateMap()
{
    MapData = _GenerateInitialGrid();

    int[] startingPoints = GetRandomStartingPoints();

    foreach (int j in startingPoints)
    {
        int currentJ = j;
        for (int i = 0; i < FLOORS - 1; i++)
            currentJ = _SetupConnection(i, currentJ);
    }

    _SetupBossRoom();
    _SetupRandomRoomWeights();
    _SetupRoomTypes();

    int floorIndex = 0;
    foreach (Room[] floor in MapData)
    {
        GD.Print($"floor {floorIndex}");

        Room[] usedRooms = Array.FindAll(floor, room => room.NextRooms.Length > 0);
        foreach (Room room in usedRooms)
            GD.Print(room.Column);

        floorIndex++;
    }

    return MapData;
}

	private Room[][] _GenerateInitialGrid()
	{
		var result = new Room[FLOORS][];

		for (int i = 0; i < FLOORS; i++)
		{
			var adjacentRooms = new List<Room>();

			for (int j = 0; j < MAP_WIDTH; j++)
			{
				var currentRoom = new Room();

				var offset = new Vector2(
					(float)GD.Randf() * PLACEMENT_RANDOMNESS,
					(float)GD.Randf() * PLACEMENT_RANDOMNESS
				);

				currentRoom.Position  = new Vector2(j * X_DIST, i * -Y_DIST) + offset;
				currentRoom.Row       = i;
				currentRoom.Column    = j;
				currentRoom.NextRooms = new Room[0];

				if (i == FLOORS - 1)
				{
					currentRoom.Position = new Vector2(currentRoom.Position.X, (i + 1) * -Y_DIST);
				}

				adjacentRooms.Add(currentRoom); 
			}

			result[i] = adjacentRooms.ToArray(); 
		}

		return result;
	}

    // Returns a random RoomType based on weighted probabilities
    private Room.RoomType _GetRandomRoomType()
    {
        float roll       = (float)GD.Randf() * _randomRoomTypeTotalWeight;
        float cumulative = 0f;

        foreach (var kvp in _roomWeights)
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
                return kvp.Key;
        }

        return Room.RoomType.Combat;
    }
	private int[] GetRandomStartingPoints()
{
    List<int> yCoordinates = new();
    int uniquePoints = 0;

    while (uniquePoints < 2)
    {
        uniquePoints = 0;
        yCoordinates.Clear();

        for (int i = 0; i < PATHS; i++)
        {
            int startingPoint = (int)GD.RandRange(0, MAP_WIDTH - 1);

            if (!yCoordinates.Contains(startingPoint))
                uniquePoints++;

            yCoordinates.Add(startingPoint);
        }
    }

    return yCoordinates.ToArray();
}
private int _SetupConnection(int i, int j)
{
    Room nextRoom = null;
    Room currentRoom = MapData[i][j];

    while (nextRoom == null || _WouldCrossExistingPath(i, j, nextRoom))
    {
        int randomJ = Mathf.Clamp((int)GD.RandRange(j - 1, j + 1), 0, MAP_WIDTH - 1);
        nextRoom = MapData[i + 1][randomJ];
    }

    var nextRooms = new List<Room>(currentRoom.NextRooms);
    nextRooms.Add(nextRoom);
    currentRoom.NextRooms = nextRooms.ToArray();

    return nextRoom.Column;
}
private bool _WouldCrossExistingPath(int i, int j, Room room)
{
    Room leftNeighbour  = null;
    Room rightNeighbour = null;

    if (j > 0)
        leftNeighbour = MapData[i][j - 1];

    if (j < MAP_WIDTH - 1)
        rightNeighbour = MapData[i][j + 1];

    // can't cross in right dir if right neighbour goes to left
    if (rightNeighbour != null && room.Column > j)
    {
        foreach (Room nextRoom in rightNeighbour.NextRooms)
        {
            if (nextRoom.Column < room.Column)
                return true;
        }
    }

    // can't cross in left dir if left neighbour goes to right
    if (leftNeighbour != null && room.Column < j)
    {
        foreach (Room nextRoom in leftNeighbour.NextRooms)
        {
            if (nextRoom.Column > room.Column)
                return true;
        }
    }

    return false;
}
private void _SetupBossRoom()
{
    int middle = Mathf.FloorToInt(MAP_WIDTH * 0.5f);
    Room bossRoom = MapData[FLOORS - 1][middle];

    for (int j = 0; j < MAP_WIDTH; j++)
    {
        Room currentRoom = MapData[FLOORS - 2][j];

        if (currentRoom.NextRooms.Length > 0)
        {
            currentRoom.NextRooms = new Room[] { bossRoom };
        }
    }

    bossRoom.EnumRoomType = Room.RoomType.Boss;
}

private void _SetupRandomRoomWeights()
{
    _roomWeights[Room.RoomType.Combat]   = MONSTER_ROOM_WEIGHT;
    _roomWeights[Room.RoomType.CampFire] = MONSTER_ROOM_WEIGHT + CAMPFIRE_ROOM_WEIGHT;
    _roomWeights[Room.RoomType.Shop]     = MONSTER_ROOM_WEIGHT + CAMPFIRE_ROOM_WEIGHT + SHOP_ROOM_WEIGHT;

    _randomRoomTypeTotalWeight = _roomWeights[Room.RoomType.Shop];
}

private void _SetupRoomTypes()
{
    // first floor is always a battle
    foreach (Room room in MapData[0])
        if (room.NextRooms.Length > 0)
        {
            room.EnumRoomType = Room.RoomType.Combat;
            room.Encounter = _GetRandomEncounterForRow(room.Row); // <- aqui
        }

    // 9th floor is always a treasure
    foreach (Room room in MapData[8])
        if (room.NextRooms.Length > 0)
            room.EnumRoomType = Room.RoomType.Chest;

    // last floor before boss is always a campfire
    foreach (Room room in MapData[13])
        if (room.NextRooms.Length > 0)
            room.EnumRoomType = Room.RoomType.CampFire;

    // boss room
    int middle = Mathf.FloorToInt(MAP_WIDTH * 0.5f);
    MapData[FLOORS - 1][middle].Encounter = BossEncounters.Length > 0
        ? BossEncounters[(int)GD.RandRange(0, BossEncounters.Length - 1)]
        : null; // <- aqui

    // rest of rooms
    foreach (Room[] currentFloor in MapData)
        foreach (Room room in currentFloor)
            foreach (Room nextRoom in room.NextRooms)
                if (nextRoom.EnumRoomType == Room.RoomType.NotAssigned)
                    _SetRoomRandomly(nextRoom);
}

private void _SetRoomRandomly(Room roomToSet)
{
    bool campfireBelow4      = true;
    bool consecutiveCampfire = true;
    bool consecutiveShop     = true;
    bool campfireOn13        = true;

    Room.RoomType typeCandidate = Room.RoomType.NotAssigned;

    while (campfireBelow4 || consecutiveCampfire || consecutiveShop || campfireOn13)
    {
        typeCandidate = _GetRandomRoomTypeByWeight();

        bool isCampfire        = typeCandidate == Room.RoomType.CampFire;
        bool hasCampfireParent = _RoomHasParentOfType(roomToSet, Room.RoomType.CampFire);
        bool isShop            = typeCandidate == Room.RoomType.Shop;
        bool hasShopParent     = _RoomHasParentOfType(roomToSet, Room.RoomType.Shop);

        campfireBelow4      = isCampfire && roomToSet.Row < 3;
        consecutiveCampfire = isCampfire && hasCampfireParent;
        consecutiveShop     = isShop     && hasShopParent;
        campfireOn13        = isCampfire && roomToSet.Row == 12;
    }

    roomToSet.EnumRoomType = typeCandidate;

    if (typeCandidate == Room.RoomType.Combat) // <- faltava isso
        roomToSet.Encounter = _GetRandomEncounterForRow(roomToSet.Row);
}
private bool _RoomHasParentOfType(Room room, Room.RoomType type)
{
    var parents = new List<Room>();

    if (room.Column > 0 && room.Row > 0)
    {
        Room parentCandidate = MapData[room.Row - 1][room.Column - 1];
        if (Array.Exists(parentCandidate.NextRooms, r => r == room))
            parents.Add(parentCandidate);
    }

    if (room.Row > 0)
    {
        Room parentCandidate = MapData[room.Row - 1][room.Column];
        if (Array.Exists(parentCandidate.NextRooms, r => r == room))
            parents.Add(parentCandidate);
    }

    if (room.Column < MAP_WIDTH - 1 && room.Row > 0)
    {
        Room parentCandidate = MapData[room.Row - 1][room.Column + 1];
        if (Array.Exists(parentCandidate.NextRooms, r => r == room))
            parents.Add(parentCandidate);
    }

    foreach (Room parent in parents)
        if (parent.EnumRoomType == type)
            return true;

    return false;
}

private Room.RoomType _GetRandomRoomTypeByWeight()
{
    float roll = (float)GD.RandRange(0.0, _randomRoomTypeTotalWeight);

    foreach (var kvp in _roomWeights)
        if (kvp.Value > roll)
            return kvp.Key;

    return Room.RoomType.Combat;
}
private EncounterData _GetRandomEncounterForRow(int row)
{
    EncounterData[] pool;

    if (row <= 4)
        pool = EasyEncounters;
    else if (row <= 9)
        pool = MediumEncounters;
    else
        pool = HardEncounters;

    if (pool.Length == 0) return null;
    return pool[(int)GD.RandRange(0, pool.Length - 1)];
}
}