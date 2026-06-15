using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class MainPhaseHandler : PhaseHandler
{
    public MainPhaseHandler(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Execute()
    {
        gameManager.IsHostDone = false;
        gameManager.IsClientDone = false;

        StampChoices();

    }

    private void StampChoices()
    {
        PickRandomStamps(
            gameManager.HostStampDeck,
            gameManager.HostStampChoices
        );
        PickRandomStamps(
            gameManager.ClientStampDeck,
            gameManager.ClientStampChoices
        );
    }

    private void PickRandomStamps(List<int> deck, NetworkArray<int> choices)
    {
        List<int> available = new List<int>(deck);

        for (int i = available.Count - 1; i >= 1; i--)
        {
            int rnd = Random.Range(0, i + 1);
            int temp = available[rnd];
            available[rnd] = available[i];
            available[i] = temp;
        }

        for (int i = 0; i < 3; i++)
        {
            choices.Set(i, i < available.Count ? available[i] : -1); 
        }
    }
}
