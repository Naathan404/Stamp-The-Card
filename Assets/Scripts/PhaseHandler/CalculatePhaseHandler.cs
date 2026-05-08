using UnityEngine;

public class CalculatePhaseHandler : PhaseHandler
{
    public CalculatePhaseHandler(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Execute()
    {
        CardSlot[] hostSlots = TableVisualManager.Instance.GetBottomCardSlots();
        CardSlot[] clientSlots = TableVisualManager.Instance.GetTopCardSlots();
        ResetAllCard(hostSlots);
        ResetAllCard(clientSlots);
    }

    private void ResetAllCard(CardSlot[] cardSlots)
    {
        for(int i = 0; i < 3; i++)
        {
            cardSlots[i].Reset();
        }
    }
}
