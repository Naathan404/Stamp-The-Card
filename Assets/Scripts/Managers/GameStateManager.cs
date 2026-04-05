using Fusion;
using UnityEngine;

public class GameStateManager : NetworkSingleton<GameStateManager>
{
    #region Game Phase Enum
    public enum GamePhase
    {
        DrawPhase,
        MainPhase,
        CalculatePhase,
        EndPhase
    }
    #endregion



    [Networked] public GamePhase CurrentGameState { get; set; } = GamePhase.DrawPhase;
    [Networked] public bool IsHostPriority { get; set; } = true;
    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public void testChangePhase(int n)
    {
        GamePhase newPhase;
        switch(n)
        {
            case 1:
                newPhase = GamePhase.MainPhase;
                break;
            case 2:
                newPhase = GamePhase.CalculatePhase;
                break;
            case 3:
                newPhase = GamePhase.EndPhase;
                break;
            default:
                newPhase = GamePhase.DrawPhase;
                break;
        }
        ChangePhase(newPhase);
    }

    /// <summary>
    /// Change Current Game Phase
    /// </summary>
    /// <param name="newPhase"></param>
    public void ChangePhase(GamePhase newPhase)
    {
        if(!Object.HasStateAuthority) return;

        CurrentGameState = newPhase;

        switch (newPhase)
        {
            case GamePhase.DrawPhase:
                GameManager.Instance.ExecuteDrawPhase();
                break;

            case GamePhase.MainPhase:
                GameManager.Instance.ExecuteMainPhase();
                break;

            case GamePhase.CalculatePhase:
                GameManager.Instance.ExecuteCalcutePhase();
                break;

            case GamePhase.EndPhase:
                GameManager.Instance.ExecuteEndPhase();
                break;
            
            default:
                Debug.Log("Game Phase mới truyền vào không hợp lệ");
                break;
        }
    }

    public override void Render()
    {
        foreach(var change in _changeDetector.DetectChanges(this))
        {
            switch(change)
            {
                case nameof(CurrentGameState):
                    Debug.Log($"change to phase: [{CurrentGameState.ToString()}]");
                    // call UI, sound, bla bla, etc
                    break;
            }
        }
    }
}
