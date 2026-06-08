public static class RunStats
{
    public static int EnemiesKilled { get; set; } = 0;
    public static int CurrentFloor  { get; set; } = 0;

    public static void Reset()
    {
        EnemiesKilled  = 0;
        CurrentFloor   = 0;
        RunData.Tower   = 1;
        RunData.Endless = false;
    }
}
