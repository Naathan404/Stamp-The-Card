using UnityEngine;

public class CalculatePhaseHandler : PhaseHandler
{
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
