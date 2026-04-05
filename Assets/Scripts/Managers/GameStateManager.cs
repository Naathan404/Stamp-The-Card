using System.Linq;
using System.Net.Security;
using DG.Tweening;
using Fusion;
using TMPro;
using Unity.Mathematics.Geometry;
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



    [Networked] public GamePhase CurrentGameState { get; set; }
    [Networked] public bool IsHostPriority { get; set; } = true;
    [Networked] private NetworkBool HasStartedFirstTurn { get; set; }
    [Networked] private TickTimer _startDelayTimer { get; set; }
    [Networked] private TickTimer _mainPhaseTimer { get; set; }
    
    [Header("UI")]
    [SerializeField] private GameObject _timerZone;
    [SerializeField] private TextMeshProUGUI _timer;
    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        CurrentGameState = GamePhase.Waiting;
        HasStartedFirstTurn = false;
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

        if(CurrentGameState == GamePhase.Waiting)
        {
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
        else if(CurrentGameState == GamePhase.MainPhase)
        {
            if(_mainPhaseTimer.Expired(Runner))
            {
                _mainPhaseTimer = TickTimer.None;
                ChangePhase(GamePhase.CalculatePhase);
            }
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
                _mainPhaseTimer = TickTimer.CreateFromSeconds(Runner, 60f);
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
                    if(CurrentGameState == GamePhase.MainPhase)
                    {
                        _timerZone.gameObject.SetActive(true);
                        _timerZone.transform.DOScaleY(1f, 0.4f).SetEase(Ease.OutQuart);
                    }
                    if(CurrentGameState == GamePhase.CalculatePhase)
                    {
                        _timerZone.transform.DOScaleY(0f, 0.4f).OnComplete(() =>
                        {
                            _timer.text = "0";
                            _timerZone.gameObject.SetActive(false); 
                        });
                    }
                    if(CurrentGameState == GamePhase.DrawPhase)
                    {
                        _timerZone.transform.DOScaleY(0f, 0.4f).OnComplete(() =>
                        {
                            _timer.text = "0";
                            _timerZone.gameObject.SetActive(false); 
                        });                        
                    }
                    // call UI, sound, bla bla, etc
                    break;
            }
        }
        if (!Object.IsValid) return;
        if (CurrentGameState == GamePhase.MainPhase)
        {
            if (_mainPhaseTimer.IsRunning)
            {
                float? timeRemaining = _mainPhaseTimer.RemainingTime(Runner);
                if (timeRemaining.HasValue)
                {
                    int displayTime = Mathf.CeilToInt(timeRemaining.Value);
                    _timer.text = displayTime.ToString();
                }
            }
        }
    }
}
