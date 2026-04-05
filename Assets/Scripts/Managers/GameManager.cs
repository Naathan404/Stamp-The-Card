using Fusion;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public class GameManager : NetworkSingleton<GameManager>
{
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
        Debug.Log($"HostHand: {HostHand[0]} - {HostHand[1]} - {HostHand[2]}");
        Debug.Log($"ClientHand: {ClientHand[0]} - {ClientHand[1]} - {ClientHand[2]}");
    }

    #endregion

}
