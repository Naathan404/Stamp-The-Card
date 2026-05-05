using System.Linq;
using System.Security.Cryptography;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public class GameManager : NetworkSingleton<GameManager>
{
    [Header("MAIN DECK")]
    public int[] MainDeck = new int[GameConstants.MAINDECK_SIZE];
    public int CurrentCardIndexFromMainDeck = 0;

    [Header("STAMP DECK")]
    [HideInInspector] public int[] HostStampDeck = new int[GameConstants.MAX_STAMP_CAPACITY];
    [HideInInspector] public int[] ClientStampDeck = new int[GameConstants.MAX_STAMP_CAPACITY];
    public int HostCurrentStampIndex = 0;
    public int ClientCurrentStampIndex = 0;

    [Header("PLAYER HAND")]
    // Your hand
    [Networked, Capacity(GameConstants.PLAYER_HAND_SIZE)] 
    [HideInInspector] public NetworkArray<int> HostHand => default;
    // Opponent hand 
    [Networked, Capacity(GameConstants.PLAYER_HAND_SIZE)] 
    [HideInInspector] public NetworkArray<int> ClientHand => default;

    [Header("PLAYER STAMPS")]
    // stamp cua hosst
    [Networked, Capacity(3)]
    private NetworkArray<int> HostStampChoices => default;
    // stamp cua client
    [Networked, Capacity(3)] 
    private NetworkArray<int> ClientStampChoices => default; 

    [Header("STAMPS ON CARRDS")]
    //// lá bài 0 (idx 0, 1, 2) và tương tự với lá bài 1, 2,...
    [Networked, Capacity(GameConstants.MAINDECK_SIZE * 3)] 
    [HideInInspector] public NetworkArray<int> CardAttachedStamps => default;

    [Header("MAIN PHASE STATS")]
    [Networked] public NetworkBool IsHostDone { get; set; }
    [Networked] public NetworkBool IsClientDone { get; set; }


    [Header("Change Detector")]
    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        // if (HasStateAuthority)
        // {
        //     /// set ô stamp tất cả bài đều trống
        //     for (int i = 0; i < GameConstants.MAINDECK_SIZE * 3; i++)
        //     {
        //         CardAttachedStamps.Set(i, -1);
        //     }
        // }
        
        Debug.Log("chuan bi set up du lieu");
        if (HasStateAuthority) // Chỉ Host mới được quyền set up bàn chơi
        {
            Debug.Log("Dang set up du liue");
            // 1. NẠP ĐẠN CHO MAIN DECK (Từ 0 đến 25)
            for (int i = 0; i < GameConstants.MAINDECK_SIZE; i++)
            {
                MainDeck[i] = i; 
            }

            // 2. NẠP ĐẠN CHO STAMP DECK (Tránh việc toàn số 0)
            for (int i = 0; i < GameConstants.MAX_STAMP_CAPACITY; i++)
            {
                HostStampDeck[i] = i + 1; 
                ClientStampDeck[i] = i + 1; // Cố tình cho Client tem khác Host để dễ test
            }

            // 3. SET BÀI TRÊN TAY LÀ -1 (Để lát nữa chia bài nó mới nhận diện là "Có thay đổi")
            for(int i = 0; i < GameConstants.PLAYER_HAND_SIZE; i++)
            {
                HostHand.Set(i, -1);
                ClientHand.Set(i, -1);
                HostStampChoices.Set(i, -1);
                ClientStampChoices.Set(i, -1);
            }

            // 4. RESET MẢNG TEM TRÊN BÀI
            for (int i = 0; i < GameConstants.MAINDECK_SIZE * 3; i++)
            {
                CardAttachedStamps.Set(i, -1);
            }
        }
    }

    #region EXECUTING
    public void ExecuteDrawPhase()
    {
        Debug.Log("thực thi draw phase");

        // gọi hàm trộn bộ bài chung
        ShuffleMainDeck();
        ShuffleStampDeck();
        DealCards();
        DebugPlayerHand();
    }

    public void ExecuteMainPhase()
    {
        IsHostDone = IsClientDone = false;
        Debug.Log("thực thi main phase");

        // Rút stamp cho host
        for(int i = 0; i < 3; i++)
        {
            if(HostCurrentStampIndex < GameConstants.MAX_STAMP_CAPACITY)
            {
                HostStampChoices.Set(i, HostStampDeck[HostCurrentStampIndex]);
                HostCurrentStampIndex++;
            }
            else
            {
                HostStampChoices.Set(i, -1);
            }
        }

        // Rút 3 Stamp cho Client
        for (int i = 0; i < 3; i++)
        {
            if (ClientCurrentStampIndex < GameConstants.MAX_STAMP_CAPACITY)
            {
                ClientStampChoices.Set(i, ClientStampDeck[ClientCurrentStampIndex]);
                ClientCurrentStampIndex++;
            }
            else
            {
                ClientStampChoices.Set(i, -1); // Hết tem
            }
        }
    }

    public void ExecuteCalcutePhase()
    {
        Debug.Log("thực thi calculate phase");
        
    }

    public void ExecuteEndPhase()
    {
        Debug.Log("thực thi endphase");
    }

#region RENDER
    public override void Render()
    {
        bool triggerDeal = false;

        foreach(var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                // GOM CHUNG 2 SỰ KIỆN NÀY LẠI
                case nameof(HostHand):
                case nameof(ClientHand):
                    triggerDeal = true;
                    break;

                case nameof(HostStampChoices):
                    TableVisualManager.Instance.SpawnStampChoices(HostStampChoices, true);
                    break;

                case nameof(ClientStampChoices):
                    TableVisualManager.Instance.SpawnStampChoices(ClientStampChoices, false);
                    break;

                case nameof(CardAttachedStamps):
                    TableVisualManager.Instance.RenderStampsOnBoard(CardAttachedStamps);
                    break;
            }
        }

        // CHỈ GỌI ANIMATION 1 LẦN DUY NHẤT KHI CẢ 2 ĐÃ NHẬN BÀI
        if (triggerDeal && HostHand[0] != -1 && ClientHand[0] != -1)
        {
            TriggerDealAnimation(HostHand, ClientHand);
        }
    }

    // Truyền cả 2 bộ bài vào cùng lúc
    private void TriggerDealAnimation(NetworkArray<int> hostCards, NetworkArray<int> clientCards)
    {   
        CardData[] hData = new CardData[3];
        CardData[] cData = new CardData[3];
        
        for(int i = 0; i < 3; i++)
        {
            hData[i] = DataManager.Instance.GetCardDataByID(hostCards[i]); 
            cData[i] = DataManager.Instance.GetCardDataByID(clientCards[i]); 
        }
        
        TableVisualManager.Instance.PlayDealAnimation(hData, cData);
    }
    #endregion

    #region DRAW PHASE
    /// <summary>
    /// Shuffle the main dekc
    /// </summary>
    /// <param name="MainDeck"></param>
    /// <param name="n"></param>
    public void ShuffleMainDeck()
    {
        Debug.Log("Trộn bài");
        for(int i = GameConstants.MAINDECK_SIZE - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            
            // swap
            int a = MainDeck[i];
            MainDeck[i] = MainDeck[j];
            MainDeck[j] = a;
        }

        // in thử bộ bài sau khi trộn
        foreach(var i in MainDeck)
        {
            Debug.Log($"card: => {i}");
        }

        CurrentCardIndexFromMainDeck = 0;
    }

    public void ShuffleStampDeck()
    {
        Debug.Log("Trộn stamps");
        for(int i = GameConstants.MAX_STAMP_CAPACITY - 1; i >  0; i--)
        {
            int j = Random.Range(0, i + 1);
            int k = Random.Range(0, i + 1);
            // swap host
            int a = HostStampDeck[i];
            HostStampDeck[i] = HostStampDeck[j];
            HostStampDeck[j] = a;
            // swap client
            int b = ClientStampDeck[i];
            ClientStampDeck[i] = ClientStampDeck[k];
            ClientStampDeck[k] = b;
        }
    }


    /// <summary>
    /// deal 3 cards for each player
    /// </summary>
    public void DealCards()
    {
        Debug.Log("Chia bài");

        for (int i = 0; i < GameConstants.PLAYER_HAND_SIZE; i++)
        {
            HostHand.Set(i, MainDeck[CurrentCardIndexFromMainDeck]);
            CurrentCardIndexFromMainDeck++;
        }

        for (int i = 0; i < GameConstants.PLAYER_HAND_SIZE; i++)
        {
            ClientHand.Set(i, MainDeck[CurrentCardIndexFromMainDeck]);
            CurrentCardIndexFromMainDeck++;
        }
    }
    #endregion

    #region MAIN PHASE
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_PlayStamp(int slotIndex, int stampID, bool isHostAction)
    {
        int targetCardID = isHostAction ? HostHand[slotIndex] : ClientHand[slotIndex];

        int startIndex = targetCardID * 3;
        bool stampAdded = false;

        for(int  i = 0; i < 3; i++)
        {
            if(CardAttachedStamps[startIndex + i] == -1) // nếu găp ô trống
            {
                CardAttachedStamps.Set(startIndex + i, stampID);
                stampAdded = true;

                if(isHostAction) IsHostDone = true;
                else    IsClientDone = true;

                break;
            }
        }

        if (stampAdded)
        {
            Debug.Log($"[Server] Đã đóng Tem {stampID} lên lá bài số {slotIndex} của {(isHostAction ? "Host" : "Client")}");
        }
        else
        {
            Debug.LogWarning($"[Server] Lá bài số {slotIndex} đã đầy 3 Tem, không thể đóng thêm!");
        }

        // dummy
        // - kiểm tra slotIndex đã đầy stamp chưa
        // - lưu stampId vào mảng stamp của slot index
        // - cập nhật hình ảnh stamp vào slot index
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_EndPhase(bool isHostAction)
    {
        if(isHostAction)
        {
            IsHostDone = true;
            Debug.Log("Host bấm end phase");
        }
        else
        {
            IsClientDone = true;
            Debug.Log("Client bấm end phase");
        }
    }
    
    #endregion

    public void DebugPlayerHand()
    {
        Debug.Log($"Host Hand: {HostHand[0]}, {HostHand[1]}, {HostHand[2]}");
        Debug.Log($"Client Hand: {ClientHand[0]}, {ClientHand[1]}, {ClientHand[2]}");
    }

    #endregion

}
