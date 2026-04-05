using System.Linq;
using System.Net.Security;
using Fusion;
using UnityEngine;

public class GameStateManager : NetworkSingleton<GameStateManager>
{
    #region Game Phase Enum
    public enum GamePhase
    {
        Waiting,
        DrawPhase,
        MainPhase,
        CalculatePhase,
        EndPhase,
        GameOver
    }
    #endregion



    [Networked] public GamePhase CurrentGameState { get; set; } = GamePhase.Waiting;
    [Networked] public bool IsHostPriority { get; set; } = true;
    [Networked] public NetworkBool HasStartedFirstTurn { get; set; } = false;
    [Networked] private TickTimer _startDelayTimer { get; set; }
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
                newPhase = GamePhase.DrawPhase;
                break;
            case 2:
                newPhase = GamePhase.MainPhase;
                break;
            case 3:
                newPhase = GamePhase.CalculatePhase;
                break;
            case 4:
                newPhase = GamePhase.EndPhase;
                break;
            default:
                newPhase = GamePhase.Waiting;
                break;
        }
        ChangePhase(newPhase);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (HasStartedFirstTurn) return;

        if (Runner.ActivePlayers.Count() == 2 && !_startDelayTimer.IsRunning)
        {
            _startDelayTimer = TickTimer.CreateFromSeconds(Runner, 1.5f);
        }

        if (_startDelayTimer.Expired(Runner))
        {
            _startDelayTimer = TickTimer.None; 
            HasStartedFirstTurn = true; 
            ChangePhase(GamePhase.DrawPhase);
        }
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
