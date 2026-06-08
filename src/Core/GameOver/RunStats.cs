public static class RunStats
{
    public static int EnemiesKilled { get; set; } = 0;
    public static int CurrentTower  { get; set; } = 1;
    public static int CurrentFloor  { get; set; } = 0;

    public static void Reset()
    {
        EnemiesKilled = 0;
        CurrentTower  = 1;
        CurrentFloor  = 0;
    }
}
