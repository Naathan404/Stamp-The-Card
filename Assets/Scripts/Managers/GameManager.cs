using Fusion;
using Unity.Mathematics;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    #region Game State
    public enum GameState
    {
        WaitingForPlayers,
        DrawPhase,
        MainPhase,
        CalculatePhase,
        EndPhase
    }
    #endregion


    [Networked]
    public GameState CurrentGameState { get; set; }
    

}
