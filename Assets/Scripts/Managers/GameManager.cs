using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    #region Game State
    public enum GameState
    {
        WaitingForPlayers,
        Dealing,
        Playing,
        CheckResults,
        Ending
    }
    #endregion

    

}
