using System;
using System.Threading.Tasks;
using UnityEditor.Build;
using UnityEngine;

public class EndPhaseHandler : PhaseHandler
{
    public static event Action<bool> OnPlayerHPModified;
    public EndPhaseHandler(GameManager gameManager) : base(gameManager)
    {
    }

    public async override void Execute()
    {
        if(!gameManager.Runner.IsServer) return;

        CardSlot[] hostSlots = TableVisualManager.Instance.GetHostCardSlots();
        CardSlot[] clientSlots = TableVisualManager.Instance.GetClientCardSlots();

        bool hostHasToughness = false;
        bool clientHasToughness = false;

        int tempHostTotal = 0;
        int tempClientTotal = 0;

        int hostReverseDmg = 0;   
        int clientReverseDmg = 0;

        for(int i = 0; i < 3; i++)
        {
            if (hostSlots[i].IsKingOfToughness) hostHasToughness = true;
            if (clientSlots[i].IsKingOfToughness) clientHasToughness = true;
        }

        for(int i = 0; i < 3; i++)
        {
            if (hostSlots[i].IsReverseBalance || clientSlots[i].IsReverseBalance) 
            {
                int diff = Mathf.Abs(hostSlots[i].Score - clientSlots[i].Score);
                if (hostSlots[i].IsReverseBalance && clientSlots[i].IsReverseBalance) 
                {
                    diff *= 2;
                }
                
                if (hostSlots[i].Score > clientSlots[i].Score && !hostHasToughness) 
                    hostReverseDmg += diff;
                else if (clientSlots[i].Score > hostSlots[i].Score && !clientHasToughness) 
                    clientReverseDmg += diff;
                continue;
            }

            tempHostTotal += hostSlots[i].Score;
            tempClientTotal += clientSlots[i].Score;
        }

        gameManager.RPC_PlayEndPhaseCinematic(tempHostTotal, tempClientTotal, hostReverseDmg, clientReverseDmg, hostHasToughness, clientHasToughness);

        int dynamicDelay = CalculateCinematicDelay(tempHostTotal, tempClientTotal, hostReverseDmg, clientReverseDmg, hostHasToughness, clientHasToughness);

        await Task.Delay(dynamicDelay);
        CalculateAndApplyDamage(hostSlots, clientSlots);

        await Task.Delay(1500);
        if(CheckWinCondition()) 
            return;

        PrepareNextTurn();
    }

        /// <summary>
    /// Hàm tính toán tổng thời gian Cinematic 
    /// </summary>
    private int CalculateCinematicDelay(int hostTotal, int clientTotal, int hostRev, int clientRev, bool hostTough, bool clientTough)
    {
        int hostReal = hostTotal % 10;
        int clientReal = clientTotal % 10;

        int hostBase = (hostReal < clientReal && !hostTough) ? (clientReal - hostReal) : 0;
        int clientBase = (clientReal < hostReal && !clientTough) ? (hostReal - clientReal) : 0;

        float timeInSeconds = 5.5f; 

        if (hostBase == 0 && clientBase == 0 && hostRev == 0 && clientRev == 0)
        {
            timeInSeconds += 2.0f; 
        }
        else
        {
            if (hostBase > 0) timeInSeconds += 2.0f; 
            if (clientBase > 0)
            {
                if (hostBase > 0) timeInSeconds += 0.4f; 
                timeInSeconds += 2.0f;
            }

            if (hostRev > 0 || clientRev > 0)
            {
                timeInSeconds += 0.4f; 
                if (hostRev > 0) timeInSeconds += 2.0f;
                if (clientRev > 0)
                {
                    if (hostRev > 0) timeInSeconds += 0.4f; 
                    timeInSeconds += 2.0f;
                }
            }
        }
        return (int)(timeInSeconds * 1000); 
    }

    private (int, int) CalculateAndApplyDamage(CardSlot[] hostSlots, CardSlot[] clientSlots)
    {
        int hostTotal = 0;
        int clientTotal = 0;
        bool hostHasToughness = false;
        bool clientHasToughness = false;

        for(int i = 0; i < 3; i++)
        {
            if (hostSlots[i].IsKingOfToughness) hostHasToughness = true;
            if (clientSlots[i].IsKingOfToughness) clientHasToughness = true;

            if (hostSlots[i].IsReverseBalance || clientSlots[i].IsReverseBalance)
            {
                ApplyReverseBalanceDamage(hostSlots[i], clientSlots[i], hostHasToughness, clientHasToughness);
                continue; // Bỏ qua, KHÔNG cộng vào tổng
            }

            hostTotal += hostSlots[i].Score;
            clientTotal += clientSlots[i].Score;
        }

        int hostFinal = hostTotal % 10;
        int clientFinal = clientTotal % 10;

        Debug.Log($"[EndPhase] Host: {hostTotal} → {hostFinal} | Client: {clientTotal} → {clientFinal}");

        int damage = Mathf.Abs(hostFinal - clientFinal);

        if (hostFinal < clientFinal)
        {
            // ApplyDamageToHost(damage, hostSlots);
            // Debug.Log($"[EndPhase] Host nhận {damage} sát thương");
            if (hostHasToughness) 
            {
                Debug.Log("[Vua Lì Đòn] Host thua điểm TỔNG LƯỢT nhưng được miễn toàn bộ sát thương!");
            }
            else 
            {
                ApplyDamageToHost(damage, hostSlots);
                Debug.Log($"[EndPhase] Host nhận {damage} sát thương");
            }
        }
        else if (clientFinal < hostFinal)
        {
            // ApplyDamageToClient(damage, clientSlots);
            // Debug.Log($"[EndPhase] Client nhận {damage} sát thương");
            if (clientHasToughness) 
            {
                Debug.Log("[Vua Lì Đòn] Client thua điểm TỔNG LƯỢT nhưng được miễn toàn bộ sát thương!");
            }
            else 
            {
                ApplyDamageToClient(damage, clientSlots);
                Debug.Log($"[EndPhase] Client nhận {damage} sát thương");
            }
        }
        else
        {
            Debug.Log("[EndPhase] Hòa — không ai bị trừ máu");
        }

        return (hostTotal, clientTotal);
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

        GameManager.Instance.HostHP = Math.Clamp(GameManager.Instance.HostHP - damage, 0, GameConstants.PLAYER_STARTING_HP);
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
                    TriggerPeaceAmulet(false);
                    return;
                }
            }
        }

        GameManager.Instance.ClientHP = Math.Clamp(GameManager.Instance.ClientHP - damage, 0, GameConstants.PLAYER_STARTING_HP);
    }

    private void TriggerPeaceAmulet(bool isHost)
    {
        // TODO: đánh dấu player này đã kích hoạt Bùa Bình An
        // → stamp của họ vô hiệu các lượt còn lại
        // → lượt này an toàn, không bị trừ máu    
        CardSlot[] mySlots = isHost ? TableVisualManager.Instance.GetHostCardSlots() : TableVisualManager.Instance.GetClientCardSlots();
        bool hasAmulet = false;
        int amuletCardSlotIndex = -1;

        for (int i = 0; i < mySlots.Length; i++)
        {
            if (mySlots[i].HasPeaceAmulet)
            {
                hasAmulet = true;
                amuletCardSlotIndex = i;
                break;
            }
        }

        if (hasAmulet)
        {
            string ownerName = isHost ? "Host" : "Client";
            Debug.Log($"[Bùa Bình An] Đã kích hoạt cứu mạng cho {ownerName}! Tiến hành áp dụng hình phạt luật chơi.");

            if (isHost) GameManager.Instance.HostHP = 1;
            else GameManager.Instance.ClientHP = 1;

            // hạ điểm các bài về 0 và vô hiệu hóa stamps
            for (int i = 0; i < mySlots.Length; i++)
            {
                mySlots[i].Score = 0;
                mySlots[i].StampsDisabled = true; 
            }

            TableVisualManager.Instance.UpdateBoardScores();

            // Tận diệt con tem Bùa Bình An khỏi lá bài này trên Server (để tránh lỗi reset/dùng lại)
            // if (GameManager.Instance.Runner.IsServer && mySlots[amuletCardSlotIndex].Data != null)
            // {
            //     int cardID = mySlots[amuletCardSlotIndex].Data.CardID;
            //     int startIndex = cardID * 3;
            //     for (int s = 0; s < 3; s++)
            //     {
            //         // Xóa sạch bộ tem dán trên lá bài hộ mệnh này                                               
            //         GameManager.Instance.CardAttachedStamps.Set(startIndex + s, -1);
            //     }
            // }

            if (mySlots[amuletCardSlotIndex].Data != null)
            {
                int targetCardID = mySlots[amuletCardSlotIndex].Data.CardID;

                GameManager.Instance.DisableCardStampsPermanently(targetCardID);
            }
        }
        Debug.Log($"[Bùa Bình An] {"Host hoặc Client"} an toàn lượt này, stamps vô hiệu từ đây");
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
            gameManager.RPC_ProcessMatchEnd(false);
            GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.GameOver);
            return true;
        }
        if (gameManager.ClientHP <= 0)
        {
            Debug.Log("[GameOver] Host thắng!");
            gameManager.RPC_ProcessMatchEnd(true);
            GameStateManager.Instance.ChangePhase(GameStateManager.GamePhase.GameOver);
            return true;
        }
        return false;
    }


    #region LOGIC của các STAMPS đặc biệt

    // Stamp Đảo nược cán cân: bên nào điểm cao hơn sẽ bị trừ máu bằng đúng chênh lệch điểm
    private void ApplyReverseBalanceDamage(CardSlot hostSlot, CardSlot clientSlot, bool hostTough, bool clientTough)
    {
        int diff = Mathf.Abs(hostSlot.Score - clientSlot.Score);
        if (diff == 0) return;

        if (hostSlot.Score > clientSlot.Score)
        {
            // ApplyDamageToHost(diff, null);
            // Debug.Log($"[Đảo Ngược Cán Cân] Host điểm cao hơn -> nhận {diff} sát thương");
            if (hostTough) 
                Debug.Log($"[Vua Lì Đòn] Host bị Đảo Ngược Cán Cân nhưng được MIỄN {diff} sát thương");
            else 
            {
                ApplyDamageToHost(diff, null);
                Debug.Log($"[Đảo Ngược Cán Cân] Host điểm cao hơn -> nhận {diff} sát thương");
            }
        }
        else
        {
            // ApplyDamageToClient(diff, null);
            // Debug.Log($"[Đảo Ngược Cán Cân] Client điểm cao hơn -> nhận {diff} sát thương");
            if (clientTough) 
                Debug.Log($"[Vua Lì Đòn] Client bị Đảo Ngược Cán Cân nhưng được MIỄN {diff} sát thương");
            else 
            {
                ApplyDamageToClient(diff, null);
                Debug.Log($"[Đảo Ngược Cán Cân] Client điểm cao hơn -> nhận {diff} sát thương");
            }
        }
    }

    // /// stamp Vua Lì Đòn: nếu thua cột này thì sát thương nhận vào = 0
    // private void ApplyKingOfToughnessDamage(CardSlot hostSlot, CardSlot clientSlot)
    // {
    //     int diff = Mathf.Abs(hostSlot.Score - clientSlot.Score);
    //     if (diff == 0) return;

    //     if (hostSlot.Score < clientSlot.Score && hostSlot.IsKingOfToughness)
    //     {
    //         Debug.Log("[Vua Lì Đòn] Host thua cột này nhưng được miễn sát thương");
    //         return;
    //     }
    //     if (clientSlot.Score < hostSlot.Score && clientSlot.IsKingOfToughness)
    //     {
    //         Debug.Log("[Vua Lì Đòn] Client thua cột này nhưng được miễn sát thương");
    //         return;
    //     }

    //     // Không có Vua Lì Đòn ở bên thua -> tính bình thường
    //     if (hostSlot.Score < clientSlot.Score)
    //         ApplyDamageToHost(diff, null);
    //     else
    //         ApplyDamageToClient(diff, null);
    // }

    /// stamp Bùa Bình An: nếu máu về 0 thì kích hoạt, tránh chết và vô hiệu stamp từ đây
    /// 
    #endregion
}
