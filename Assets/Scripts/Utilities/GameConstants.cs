using UnityEngine;

public static class GameConstants
{
    public const int MAINDECK_SIZE = 26;
    public const int MAX_STAMP_CAPACITY = 9;
    public const int PLAYER_HAND_SIZE = 3;
    public const int PLAYER_STARTING_HP = 15;

    public static readonly Color STAMP_COMMON_COLOR = new Color32(255, 255, 255, 255);
    public static readonly Color STAMP_RARE_COLOR = new Color32(20, 110, 255, 255);
    public static readonly Color STAMP_EPIC_COLOR = new Color32(170, 20, 255, 255);
    public static readonly Color STAMP_LEGENDARY_COLOR = new Color32(255, 200, 20, 255);

    // Khai báo bằng chuỗi luôn cho TextMeshPro xài
    public const string STAMP_RARE_HEX = "#146eff";
    public const string STAMP_EPIC_HEX = "#a914ff";
    public const string STAMP_LEGENDARY_HEX = "#ffc814";

    public const string SCENE_LOBBY = "Lobby";

    public static readonly int[] JOKER_STAMP_IDS = { 10, 23 };

    public static bool IsJokerStamp(int stampId)
    {
        foreach (int jokerId in JOKER_STAMP_IDS)
        {
            if (stampId == jokerId) return true;
        }
        return false;
    }
}
