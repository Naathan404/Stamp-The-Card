using System.Linq;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public class GameManager : NetworkSingleton<GameManager>
{

    [Header("DATABASE (Từ điển gốc - Không xáo trộn)")]
    public CardData[] CardDatabase = new CardData[GameConstants.MAINDECK_SIZE];    

    [Header("MAIN DECK")]
    public int[] MainDeck = new int[GameConstants.MAINDECK_SIZE];
    public int CurrentCardIndexFromMainDeck = 0;

    [Header("PLAYER HAND")]
    // Your hand
    [Networked, Capacity(GameConstants.PLAYER_HAND_SIZE)] 
    private NetworkArray<int> HostHand => default;
    // Opponent hand 
    [Networked, Capacity(GameConstants.PLAYER_HAND_SIZE)] 
    private NetworkArray<int> ClientHand => default;

    [Header("Change Detector")]
    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    
    public void ExecuteDrawPhase()
    {
        Debug.Log("thực thi draw phase");

        // gọi hàm trộn bộ bài chung
        ShuffleMainDeck();
        DealCards();
        DebugPlayerHand();
    }

    public void ExecuteMainPhase()
    {
        Debug.Log("thực thi main phase");
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
        foreach(var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(HostHand):
                    TriggerDealAnimation(HostHand, true);
                    break;
                case nameof(ClientHand):
                    TriggerDealAnimation(ClientHand, false);
                    break;
            }
        }
    }

    private void TriggerDealAnimation(NetworkArray<int> cardIDs, bool isHost)
    {   
        CardData[] cardDatas = new CardData[3];
        for(int i = 0; i < 3; i++)
        {
            cardDatas[i] = CardDatabase[cardIDs[i]]; 
        }
        TableVisualManager.Instance.PlayDealAnimation(cardDatas, isHost);
    }

    #endregion

    #region Xử lý Logic
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


    /// <summary>
    /// deal 3 card for each player
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

    public void DebugPlayerHand()
    {
        Debug.Log($"Host Hand: {HostHand[0]}, {HostHand[1]}, {HostHand[2]}");
        Debug.Log($"Client Hand: {ClientHand[0]}, {ClientHand[1]}, {ClientHand[2]}");
    }

    #endregion

}
