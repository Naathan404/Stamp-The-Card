using UnityEngine;

public class CalculatePhaseHandler : PhaseHandler
{
    private CalculationSystem _calculationSystem = new CalculationSystem();
    public CalculatePhaseHandler(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Execute()
    {
        if (!gameManager.Runner.IsServer) return;
        TableVisualManager.Instance.SyncStampsToCardSlots();

        CardSlot[] hostSlots = TableVisualManager.Instance.GetHostCardSlots();
        CardSlot[] clientSlots = TableVisualManager.Instance.GetClientCardSlots();

        // reset tất cả card và clear flags
        ResetAllCard(hostSlots);
        ResetAllCard(clientSlots);

        _calculationSystem.Run(hostSlots, clientSlots, GameStateManager.Instance.CurrentTurn);

        TableVisualManager.Instance.UpdateBoardScores(hostSlots, clientSlots);

        GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.EndPhase);
    }


    /// <summary>
    /// Hàm reset các card về trạng thái ban đầu cho một hand người chơi
    /// </summary>
    /// <param name="cardSlots"></param>
    private void ResetAllCard(CardSlot[] cardSlots)
    {
        for(int i = 0; i < 3; i++)
        {
            cardSlots[i].Reset();
        }
    }
}
