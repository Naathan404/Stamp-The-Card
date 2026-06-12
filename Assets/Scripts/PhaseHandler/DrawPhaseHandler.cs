using Unity.VisualScripting;
using UnityEngine;

public class DrawPhaseHandler : PhaseHandler
{
    public DrawPhaseHandler(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Execute()
    {
        bool isHost = gameManager.Runner.IsServer;
        /// Update HP cho người chơi
        UIManager.Instance.UpdateHpTexts(isHost);
        
        ShuffleMainDeck();
        DealCards();
    }

    #region DRAW PHASE
    /// <summary>
    /// Shuffle the main dekc
    /// </summary>
    /// <param name="mainDeck"></param>
    /// <param name="n"></param>
    private void ShuffleMainDeck()
    {
        Debug.Log("Trộn bài");
        for(int i = GameConstants.MAINDECK_SIZE - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            
            // swap
            int a = gameManager.MainDeck[i];
            gameManager.MainDeck[i] = gameManager.MainDeck[j];
            gameManager.MainDeck[j] = a;
        }

        // in thử bộ bài sau khi trộn
        // foreach(var i in gameManager.MainDeck)
        // {
        //     Debug.Log($"card: => {i}");
        // }

        gameManager.CurrentCardIndexFromMainDeck = 0;
    }

    /// <summary>
    /// deal 3 cards for each player
    /// </summary>
    private void DealCards()
    {
        Debug.Log("Chia bài");

        for (int i = 0; i < GameConstants.PLAYER_HAND_SIZE; i++)
        {
            gameManager.HostHand.Set(i, gameManager.MainDeck[gameManager.CurrentCardIndexFromMainDeck]);
            gameManager.CurrentCardIndexFromMainDeck++;
        }

        for (int i = 0; i < GameConstants.PLAYER_HAND_SIZE; i++)
        {
            gameManager.ClientHand.Set(i, gameManager.MainDeck[gameManager.CurrentCardIndexFromMainDeck]);
            gameManager.CurrentCardIndexFromMainDeck++;
        }
    }
    #endregion
}
