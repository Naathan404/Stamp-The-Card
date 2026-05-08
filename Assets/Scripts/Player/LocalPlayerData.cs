public static class LocalPlayerData
{
    public static string Username = "Player";
    public static string DisplayName;

    public static int Souls;

    public static int TotalWins;
    public static int TotalLoses;
    public static int RankPoints;

    public static void Clear()
    {
        Username = "Player";
        DisplayName = "";
        Souls = 0;
        TotalWins = 0;
        TotalLoses = 0;
        RankPoints = 0;
    }
}