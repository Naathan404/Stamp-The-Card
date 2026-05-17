using UnityEngine;

public class EndPhaseHandler : PhaseHandler
{
    public EndPhaseHandler(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Execute()
    {
        if(!gameManager.Runner.IsServer) return;

        CardSlot[] hostSlots = TableVisualManager.Instance.GetHostCardSlots();
        CardSlot[] clientSlots = TableVisualManager.Instance.GetClientCardSlots();

        CalculateAndApplyDamage(hostSlots, clientSlots);

        ClearJokerStamps();

        if(CheckWinCondition()) return;

        PrepareNextTurn();
    }

    private void CalculateAndApplyDamage(CardSlot[] hostSlots, CardSlot[] clientSlots)
    {
        int hostTotal = 0;
        int clientTotal = 0;

        for(int i = 0; i < 3; i++)
        {
            hostTotal += hostSlots[i].Score;
            clientTotal += clientSlots[i].Score;

            // Cột có Đảo Ngược Cán Cân -> tính riêng, không cộng vào tổng chung
            if (hostSlots[i].IsReverseBalance || clientSlots[i].IsReverseBalance)
            {
                ApplyReverseBalanceDamage(hostSlots[i], clientSlots[i]);
                continue;
            }

            // Cột có Vua Lì Đòn -> tính riêng, không cộng vào tổng chung
            if (hostSlots[i].IsKingOfToughness || clientSlots[i].IsKingOfToughness)
            {
                ApplyKingOfToughnessDamage(hostSlots[i], clientSlots[i]);
                continue;
            }
        }

        int hostFinal = hostTotal % 9;
        int clientFinal = clientTotal % 9;

        Debug.Log($"[EndPhase] Host: {hostTotal} → {hostFinal} | Client: {clientTotal} → {clientFinal}");

        int damage = Mathf.Abs(hostFinal - clientFinal);

        if (hostFinal < clientFinal)
        {
            ApplyDamageToHost(damage, hostSlots);
            Debug.Log($"[EndPhase] Host nhận {damage} sát thương");
        }
        else if (clientFinal < hostFinal)
        {
            ApplyDamageToClient(damage, clientSlots);
            Debug.Log($"[EndPhase] Client nhận {damage} sát thương");
        }
        else
        {
            Debug.Log("[EndPhase] Hòa — không ai bị trừ máu");
        }
    }

    private void ApplyDamageToHost(int damage, CardSlot[] cardSlots)
    {
        if (damage <= 0) return;

        if(cardSlots != null && gameManager.HostHP - damage <= 0)
        {
            for(int i = 0; i < 3; i++)
            {
                if (cardSlots[i].HasPeaceAmulet)
                {
                    Debug.Log("[EndPhase] Host kích hoạt Bùa Bình An để tránh chết");
                    TriggerPeaceAmulet(true);
                    return;
                }
            }
        }
    }

    private void ApplyDamageToClient(int damage, CardSlot[] cardSlots)
    {
        if (damage <= 0) return;

        if(cardSlots != null && gameManager.ClientHP - damage <= 0)
        {
            for(int i = 0; i < 3; i++)
            {
                if (cardSlots[i].HasPeaceAmulet)
                {
                    Debug.Log("[EndPhase] Client kích hoạt Bùa Bình An để tránh chết");
                    TriggerPeaceAmulet(true);
                    return;
                }
            }
        }
    }

    private void TriggerPeaceAmulet(bool isHost)
    {
        // TODO: đánh dấu player này đã kích hoạt Bùa Bình An
        // → stamp của họ vô hiệu các lượt còn lại
        // → lượt này an toàn, không bị trừ máu    
        Debug.Log($"[Bùa Bình An] {"Host hoặc Client"} an toàn lượt này, stamps vô hiệu từ đây");
    }                      

    // ===================== HELPERS =====================

    private void ClearJokerStamps()
    {
        foreach (int jokerID in GameConstants.JOKER_STAMP_IDS)
        {
            int startIndex = jokerID * 3;
            for (int i = 0; i < 3; i++)
                gameManager.CardAttachedStamps.Set(startIndex + i, -1);
        }

        Debug.Log("[EndPhase] Đã xóa stamp của các lá Joker");
    }

    private void PrepareNextTurn()
    {
        GameStateManager.Instance.CurrentTurn++;
        gameManager.IsHostDone   = false;
        gameManager.IsClientDone = false;

        Debug.Log($"[EndPhase] Sang turn {GameStateManager.Instance.CurrentTurn}");
        GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.DrawPhase);
    }

    private bool CheckWinCondition()
    {
        if (gameManager.HostHP <= 0)
        {
            Debug.Log("[GameOver] Client thắng!");
            GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.GameOver);
            return true;
        }
        if (gameManager.ClientHP <= 0)
        {
            Debug.Log("[GameOver] Host thắng!");
            GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.GameOver);
            return true;
        }
        return false;
    }


    #region LOGIC của các STAMPS đặc biệt

    // Stamp Đảo nược cán cân: bên nào điểm cao hơn sẽ bị trừ máu bằng đúng chênh lệch điểm
    private void ApplyReverseBalanceDamage(CardSlot hostSlot, CardSlot clientSlot)
    {
        int diff = Mathf.Abs(hostSlot.Score - clientSlot.Score);
        if (diff == 0) return;

        if (hostSlot.Score > clientSlot.Score)
        {
            ApplyDamageToHost(diff, null);
            Debug.Log($"[Đảo Ngược Cán Cân] Host điểm cao hơn -> nhận {diff} sát thương");
        }
        else
        {
            ApplyDamageToClient(diff, null);
            Debug.Log($"[Đảo Ngược Cán Cân] Client điểm cao hơn -> nhận {diff} sát thương");
        }
    }

    /// stamp Vua Lì Đòn: nếu thua cột này thì sát thương nhận vào = 0
    private void ApplyKingOfToughnessDamage(CardSlot hostSlot, CardSlot clientSlot)
    {
        int diff = Mathf.Abs(hostSlot.Score - clientSlot.Score);
        if (diff == 0) return;

        if (hostSlot.Score < clientSlot.Score && hostSlot.IsKingOfToughness)
        {
            Debug.Log("[Vua Lì Đòn] Host thua cột này nhưng được miễn sát thương");
            return;
        }
        if (clientSlot.Score < hostSlot.Score && clientSlot.IsKingOfToughness)
        {
            Debug.Log("[Vua Lì Đòn] Client thua cột này nhưng được miễn sát thương");
            return;
        }

        // Không có Vua Lì Đòn ở bên thua -> tính bình thường
        if (hostSlot.Score < clientSlot.Score)
            ApplyDamageToHost(diff, null);
        else
            ApplyDamageToClient(diff, null);
    }

    /// stamp Bùa Bình An: nếu máu về 0 thì kích hoạt, tránh chết và vô hiệu stamp từ đây
    /// 
    #endregion
}
