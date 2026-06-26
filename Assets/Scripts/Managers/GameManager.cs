using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DG.Tweening;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public class GameManager : NetworkSingleton<GameManager>
{
    [Header("DEBUG")]
    public int StampNum = 20;

    [Header("MAIN DECK")]
    public int[] MainDeck = new int[GameConstants.MAINDECK_SIZE];
    public int CurrentCardIndexFromMainDeck = 0;

    [Header("STAMP DECK")]
    [HideInInspector] public List<int> HostStampDeck = new List<int>();
    [HideInInspector] public List<int> ClientStampDeck = new List<int>();
    [Networked] public int NetworkedHostStampCount { get; set; }
    [Networked] public int NetworkedClientStampCount { get; set; }
    [Networked] public NetworkBool AreStampsReady { get; set; }
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

    [Header("STAMPS ON CARDS")]
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
    

    public static event Action OnDrawPhaseEntered;
    public static event Action OnMainPhaseEntered;
    public static event Action OnCalculatePhaseEntered;
    public static event Action OnEndPhaseEntered;

    // Bitmask đánh dấu card bị vô hiệu stamps
    [Networked] public int PermanentlyDisabledCardsBitmask { get; set; }

    // Hàm đánh dấu lá bài này bị phế võ công vĩnh viễn
    public void DisableCardStampsPermanently(int cardID)
    {
        PermanentlyDisabledCardsBitmask |= (1 << cardID);
    }

    // Hàm kiểm tra xem lá bài này có đang bị phế hay không
    public bool IsCardStampsPermanentlyDisabled(int cardID)
    {
        return (PermanentlyDisabledCardsBitmask & (1 << cardID)) != 0;
    }


    [Header("Change Detector")]
    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        
        /// Init data cho bàn chơi
        HostHP = GameConstants.PLAYER_STARTING_HP;
        ClientHP = GameConstants.PLAYER_STARTING_HP;
        //UIManager.Instance.UpdateHpTexts(Runner.IsServer);
        
        Debug.Log("chuan bi set up du lieu");
        if (HasStateAuthority) // Chỉ Host mới được quyền set up bàn chơi
        {
            Debug.Log("Dang set up du liue");
            // NẠP MAIN DECK (Từ 0 đến 25)
            for (int i = 0; i < GameConstants.MAINDECK_SIZE; i++)
            {
                MainDeck[i] = i; 
            }

            HostStampDeck.Clear();
            ClientStampDeck.Clear();

            ////---- --CHEAT
            //List<int> my8NewStamps = new List<int> { 28, 29, 30, 31, 32, 33, 34, 35 }; 
            List<int> my8NewStamps = new List<int> { 36, 37, 38, 39, 32, 33, 34, 35 }; 

            HostStampDeck.AddRange(my8NewStamps);
            ClientStampDeck.AddRange(my8NewStamps);
            //// ------------- 
            /// 

            StartCoroutine(WaitAndLoadStampsCoroutine());

            // SET BÀI TRÊN TAY LÀ -1 
            for(int i = 0; i < GameConstants.PLAYER_HAND_SIZE; i++)
            {
                HostHand.Set(i, -1);
                ClientHand.Set(i, -1);
                HostStampChoices.Set(i, -1);
                ClientStampChoices.Set(i, -1);
            }

            //  RESET MẢNG TEM TRÊN BÀI
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

    private System.Collections.IEnumerator WaitAndLoadStampsCoroutine()
    {
        PlayerNetworkData hostData = null;
        PlayerNetworkData clientData = null;

        while (hostData == null || clientData == null)
        {
            foreach (var playerData in FindObjectsByType<PlayerNetworkData>(FindObjectsSortMode.None))
            {
                // Ai có InputAuthority là chính bản thân Server thì người đó là Host
                if (playerData.Object.InputAuthority == Runner.LocalPlayer)
                    hostData = playerData;
                else
                    clientData = playerData;
            }
            yield return null;
        }

        yield return new WaitUntil(() => hostData.IsStampSynced && clientData.IsStampSynced);

        // HostStampDeck.Clear();
        // ClientStampDeck.Clear();

        HostStampDeck.AddRange(hostData.GetPlayerStampIDs().Take(9));
        ClientStampDeck.AddRange(clientData.GetPlayerStampIDs().Take(9));

        HostStampDeck = HostStampDeck.OrderBy(x => Guid.NewGuid()).ToList();
        ClientStampDeck = ClientStampDeck.OrderBy(x => Guid.NewGuid()).ToList();

        NetworkedHostStampCount = HostStampDeck.Count;
        NetworkedClientStampCount = ClientStampDeck.Count;

        Debug.Log($"[GameManager] Đã nạp xong bộ Tem thực tế! Host: {HostStampDeck.Count} tem | Client: {ClientStampDeck.Count} tem.");
        AreStampsReady = true;
    }

    #region EXECUTING
    public void ExecuteDrawPhase()
    {
        OnDrawPhaseEntered?.Invoke();
        Debug.Log("thực thi draw phase");
        _drawHandler.Execute();
        DebugPlayerHand();
    }

    public void ExecuteMainPhase()
    {
        OnMainPhaseEntered?.Invoke();
        Debug.Log("thực thi main phase");
        _mainHandler.Execute();
    }

    public void ExecuteCalcutePhase()
    {
        OnCalculatePhaseEntered?.Invoke();
        Debug.Log("thực thi calculate phase");
        _calculateHandler.Execute();
    }

    public void ExecuteEndPhase()
    {
        OnEndPhaseEntered?.Invoke();
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

                case nameof(HostHP):
                case nameof(ClientHP):
                    UIManager.Instance.UpdateHpTexts(Runner.IsServer);
                    break;
                
                case nameof(NetworkedHostStampCount):
                case nameof(NetworkedClientStampCount):
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.UpdateStampCount(Runner.IsServer);
                    }
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
        if (isHostAction) HostStampDeck.Remove(stampID);
        else ClientStampDeck.Remove(stampID);
        NetworkedHostStampCount = HostStampDeck.Count;
        NetworkedClientStampCount = ClientStampDeck.Count;
        
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SyncAndShowScores(int h0, int h1, int h2, int c0, int c1, int c2)
    {
        CardSlot[] hSlots = TableVisualManager.Instance.GetHostCardSlots();
        CardSlot[] cSlots = TableVisualManager.Instance.GetClientCardSlots();

        hSlots[0].Score = h0; hSlots[1].Score = h1; hSlots[2].Score = h2;
        cSlots[0].Score = c0; cSlots[1].Score = c1; cSlots[2].Score = c2;

        TableVisualManager.Instance.UpdateBoardScores();
        Debug.Log("[Client/Host] Đã nhận và cập nhật điểm số lên màn hình!");
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_HideAllStamps()
    {
        TableVisualManager.Instance.HideAllStamps();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayEndPhaseCinematic(int hostScore, int clientScore, int hostReverseDmg = 0, int clientReverseDmg = 0, bool hostTough = false, bool clientTough = false)
    {
        Debug.Log("[RPC] Tất cả client bắt đầu chạy Cinematic End Phase!");
        UIManager.Instance.StartCoroutine(UIManager.Instance.CinematicEndPhaseRoutine(hostScore, clientScore, hostReverseDmg, clientReverseDmg, hostTough, clientTough));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ProcessMatchEnd(bool isHostWinner)
    {
        bool isMeHost = Runner.IsServer;
        bool didIWin = (isMeHost && isHostWinner) || (!isMeHost && !isHostWinner);

        int oldRank = LocalPlayerData.RankPoints;
        int oldSouls = LocalPlayerData.Souls;
        int eloChange = 0;
        int earnedSouls = 0;

        if (didIWin)
        {
            LocalPlayerData.TotalWins++;
            earnedSouls = UnityEngine.Random.Range(15, 20);
            if (oldRank < 500)
                eloChange = UnityEngine.Random.Range(30, 36);
            else if (oldRank < 1000)
                eloChange = UnityEngine.Random.Range(20, 26);
            else if (oldRank < 1500)
                eloChange = UnityEngine.Random.Range(10, 16);
            Debug.Log($"[MatchResult] BẠN ĐÃ THẮNG! Rank: {LocalPlayerData.RankPoints}");
        }
        else
        {
            LocalPlayerData.TotalLoses++;
            earnedSouls = UnityEngine.Random.Range(2, 6);
            if (oldRank < 500)
                eloChange = UnityEngine.Random.Range(-10, -8);
            else if (oldRank < 1000)
                eloChange = UnityEngine.Random.Range(-15, -12);
            else if (oldRank < 1500)
                eloChange = UnityEngine.Random.Range(-20, -17);
            Debug.Log($"[MatchResult] BẠN ĐÃ THUA! Rank: {LocalPlayerData.RankPoints}");
        }

        LocalPlayerData.RankPoints = Mathf.Max(LocalPlayerData.RankPoints + eloChange, 0);

        if (PlayfabManager.Instance != null)
        {
            float delayTime = Runner.IsServer ? 0f : 0.5f;
            DOVirtual.DelayedCall(delay: delayTime, () =>
            {
                PlayfabManager.Instance.UpdateStatistics(
                    LocalPlayerData.TotalWins, 
                    LocalPlayerData.TotalLoses, 
                    LocalPlayerData.RankPoints
                );

                PlayfabManager.Instance.AddSoul(earnedSouls);
            });
        }

        UIManager.Instance.ShowGameOverUI(isHostWinner, oldRank: oldRank, eloChange: eloChange, oldSouls: oldSouls, earnedSouls: earnedSouls);
    }
    
    #endregion

    public void DebugPlayerHand()
    {
        Debug.Log($"Host Hand: {HostHand[0]}, {HostHand[1]}, {HostHand[2]}");
        Debug.Log($"Client Hand: {ClientHand[0]}, {ClientHand[1]}, {ClientHand[2]}");
    }

    #endregion

}
