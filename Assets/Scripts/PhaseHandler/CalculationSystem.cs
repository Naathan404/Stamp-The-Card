using System.Collections.Generic;
using UnityEngine;

public class CalculationSystem : MonoBehaviour
{
    public void RunCalculationPhase(CardSlot[] hostSlots, CardSlot[] clientSlots, int[] hostCards, int[] clientCards, int[] attachedStamps)
    {
        List<StampAction> allStamps = new List<StampAction>();

        // napj toàn bộ stamp vào list allStamps
        allStamps.AddRange(CollectStamps(hostCards, attachedStamps, true));
        allStamps.AddRange(CollectStamps(clientCards, attachedStamps, false));
    }


    private List<StampAction> CollectStamps(int[] hand, int[] attachedStamps, bool isHost)
    {
        List<StampAction> actions = new List<StampAction>();
        for (int slot = 0; slot < 3; slot++)
        {
            int cardID = hand[slot];
            if (cardID == -1) continue;

            int startIndex = cardID * 3;
            for (int s = 0; s < 3; s++)
            {
                int stampID = attachedStamps[startIndex + s];
                if (stampID > 0)
                {
                    BaseStampData data = DataManager.Instance.GetStampDataByID(stampID);
                    if (data != null)
                    {
                        // Luôn reset tem về trạng thái bật trước mỗi pha tính toán
                        data.isEnabled = true; 
                        actions.Add(new StampAction(data, slot, isHost));
                    }
                }
            }
        }
        return actions;
    }

}


public class StampAction
{
    public BaseStampData StampData;
    public int SlotIndex;
    public bool IsHostOwned;

    public StampAction(BaseStampData data, int slot, bool isHost)
    {
        StampData = data;
        SlotIndex = slot;
        IsHostOwned = isHost;
    }
}