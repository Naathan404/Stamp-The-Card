public class CalculationSystem
{
    /// <summary>
    /// Entry để gọi từ CalculatePhaseHandler.
    /// </summary>
    public void Run(CardSlot[] hostSlots, CardSlot[] clientSlots, int currentTurn)
    {
        bool hostFirst = (currentTurn % 2 == 1);

        // thực hiện PreResolve -> chạy các stampe Tier0
        // set flags (IsIgnored, StampsDisabled, v.v.) trước khi tính điểm
        if (hostFirst)
        {
            ApplyTier(ExecutionTier.Tier0_RuleSetting, hostSlots, clientSlots);
            ApplyTier(ExecutionTier.Tier0_RuleSetting, clientSlots, hostSlots);
        }
        else
        {
            ApplyTier(ExecutionTier.Tier0_RuleSetting, clientSlots, hostSlots);
            ApplyTier(ExecutionTier.Tier0_RuleSetting, hostSlots, clientSlots);
        }

        //  Main Resolve -> chạy Tier1 đến Tier4 theo thứ tự turn
        if (hostFirst)
        {
            ResolveMainStamps(hostSlots, clientSlots);
            ResolveMainStamps(clientSlots, hostSlots);
        }
        else
        {
            ResolveMainStamps(clientSlots, hostSlots);
            ResolveMainStamps(hostSlots, clientSlots);
        }
    }

    /// Chạy tất cả stamp thuộc 1 tier cụ thể của 1 bên
    private void ApplyTier(ExecutionTier tier, CardSlot[] mySlots, CardSlot[] enemySlots)
    {
        for (int i = 0; i < 3; i++)
        {
            if (mySlots[i].IsIgnored || mySlots[i].StampsDisabled) continue;

            foreach (var stamp in mySlots[i].Stamps)
            {
                if (stamp.ExeTier == tier && stamp.isEnabled)
                    stamp.ApplyEffect(mySlots, enemySlots, i);
            }
        }
    }

    /// Chạy stamp Tier1–4 theo đúng thứ tự slot trên mỗi lá bài
    private void ResolveMainStamps(CardSlot[] mySlots, CardSlot[] enemySlots)
    {
        for (int i = 0; i < 3; i++)
        {
            if (mySlots[i].IsIgnored || mySlots[i].StampsDisabled) continue;

            foreach (var stamp in mySlots[i].Stamps)
            {
                if (stamp.ExeTier == ExecutionTier.Tier0_RuleSetting) continue; // đã xử lý ở PreResolve
                if (!stamp.isEnabled) continue;

                stamp.ApplyEffect(mySlots, enemySlots, i);
            }
        }
    }
}