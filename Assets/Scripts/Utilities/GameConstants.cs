using UnityEngine;

public static class GameConstants
{
    public const int MAINDECK_SIZE = 26;
    public const int MAX_STAMP_CAPACITY = 9;
    public const int PLAYER_HAND_SIZE = 3;
    public const int PLAYER_STARTING_HP = 15;

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
