using System.Threading.Tasks;
using UnityEngine;

public class CalculatePhaseHandler : PhaseHandler
{
    private CalculationSystem _calculationSystem = new CalculationSystem();
    public CalculatePhaseHandler(GameManager gameManager) : base(gameManager)
    {
    }

    public async override void Execute()
    {
        
        if (!gameManager.Runner.IsServer) return;

        gameManager.NetworkedHostStampCount = gameManager.HostStampDeck.Count;
        gameManager.NetworkedClientStampCount = gameManager.ClientStampDeck.Count;
        
        // TableVisualManager.Instance.SyncStampsToCardSlots();

        CardSlot[] hostSlots = TableVisualManager.Instance.GetHostCardSlots();
        CardSlot[] clientSlots = TableVisualManager.Instance.GetClientCardSlots();

        // reset tất cả card và clear flags
        ResetAllCard(hostSlots);
        ResetAllCard(clientSlots);

        Debug.Log("[CAL PHASE HANDLER] Gọi Calculate System để tính toán");
        //_calculationSystem.Run(hostSlots, clientSlots, GameStateManager.Instance.CurrentTurn);

        // gameManager.RPC_SyncAndShowScores(
        //     hostSlots[0].Score, hostSlots[1].Score, hostSlots[2].Score,
        //     clientSlots[0].Score, clientSlots[1].Score, clientSlots[2].Score
        // );
        
        // await Task.Delay(5000);
        // GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.EndPhase);
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
