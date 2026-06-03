using System.Collections.Generic;
using UnityEngine;

public static class LocalPlayerData
{
    //Danh sach tat ca stamp data trong game hien tai
    public static List<BaseStampData> AllStampsDatabase;

    public static string Username = "Player";
    public static string DisplayName;

    public static int Souls;

    public static int TotalWins;
    public static int TotalLoses;
    public static int RankPoints;

    //Danh sach stamp player dang so huu
    public static List<StampInstance> StampInInventory = new List<StampInstance>();

    static LocalPlayerData()
    {
        BaseStampData[] loadedStamps = Resources.LoadAll<BaseStampData>("Stamps");
        AllStampsDatabase = new List<BaseStampData>(loadedStamps);

        Debug.Log($"[LocalPlayerData] da tai thanh cong {AllStampsDatabase.Count} Stamps vao Database.");
    }

    public static void Clear()
    {
        Username = "Player";
        DisplayName = "";
        Souls = 0;
        TotalWins = 0;
        TotalLoses = 0;
        RankPoints = 0;
        StampInInventory.Clear();
    }

}