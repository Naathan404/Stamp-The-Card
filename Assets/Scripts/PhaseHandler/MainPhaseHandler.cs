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
        for(int i = 0; i < 3; i++)
        {
            /// Pick stamps for host
            if(gameManager.HostCurrentStampIndex < GameConstants.MAX_STAMP_CAPACITY)
            {
                gameManager.HostStampChoices.Set(i, gameManager.HostStampDeck[gameManager.HostCurrentStampIndex]);
                gameManager.HostCurrentStampIndex++;
            }
            else
            {
                gameManager.HostStampChoices.Set(i, -1);
            }

            // Pick stamps for client   
            if(gameManager.ClientCurrentStampIndex < GameConstants.MAX_STAMP_CAPACITY)
            {
                gameManager.ClientStampChoices.Set(i, gameManager.ClientStampDeck[gameManager.ClientCurrentStampIndex]);
                gameManager.ClientCurrentStampIndex++;
            }
            else
            {
                gameManager.ClientStampChoices.Set(i, -1);
            }
        }
    }
}
