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
    [HideInInspector] public NetworkArray<int> HostStampChoices => default;
    // stamp cua client
    [Networked, Capacity(3)] 
    [HideInInspector] public NetworkArray<int> ClientStampChoices => default; 

    [Header("STAMPS ON CARRDS")]
    //// lá bài 0 (idx 0, 1, 2) và tương tự với lá bài 1, 2,...
    [Networked, Capacity(GameConstants.MAINDECK_SIZE * 3)] 
    [HideInInspector] public NetworkArray<int> CardAttachedStamps => default;

    [Header("MAIN PHASE STATS")]
    [Networked] public NetworkBool IsHostDone { get; set; }
    [Networked] public NetworkBool IsClientDone { get; set; }

    [Header("PLAYER HP")]
    [Networked] public int HostHP { get; set; }
    [Networked] public int ClientHP { get; set; }

    /// <summary>
    /// ================ HANDLERS ========================
    /// </summary>
    private DrawPhaseHandler _drawHandler;
    private MainPhaseHandler _mainHandler;
    private CalculatePhaseHandler _calculateHandler;
    private EndPhaseHandler _endHandler;
    /// <summary>
    ///  ================================================
    /// </summary>


    [Header("Change Detector")]
    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        
        /// Init data cho bàn chơi
        HostHP = GameConstants.PLAYER_STARTING_HP;
        ClientHP = GameConstants.PLAYER_STARTING_HP;
        
        Debug.Log("chuan bi set up du lieu");
        if (HasStateAuthority) // Chỉ Host mới được quyền set up bàn chơi
        {
            Debug.Log("Dang set up du liue");
            // NẠP MAIN DECK (Từ 0 đến 25)
            for (int i = 0; i < GameConstants.MAINDECK_SIZE; i++)
            {
                MainDeck[i] = i; 
            }

            // NẠP STAMP DECK (Tránh việc toàn số 0)
            for (int i = 0; i < GameConstants.MAX_STAMP_CAPACITY; i++)
            {
                HostStampDeck[i] = i + 1; 
                ClientStampDeck[i] = i + 1;
            }

            // SET BÀI TRÊN TAY LÀ -1 
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

        _drawHandler = new DrawPhaseHandler(this);
        _mainHandler = new MainPhaseHandler(this);
        _calculateHandler = new CalculatePhaseHandler(this);
        _endHandler = new EndPhaseHandler(this);

    }

    #region EXECUTING
    public void ExecuteDrawPhase()
    {
        Debug.Log("thực thi draw phase");
        _drawHandler.Execute();
        DebugPlayerHand();
    }

    public void ExecuteMainPhase()
    {
        Debug.Log("thực thi main phase");
        _mainHandler.Execute();
    }

    public void ExecuteCalcutePhase()
    {
        Debug.Log("thực thi calculate phase");
        _calculateHandler.Execute();
    }

    public void ExecuteEndPhase()
    {
        Debug.Log("thực thi endphase");
        _endHandler.Execute();
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
