using UnityEngine;

public abstract class PhaseHandler
{
    protected GameManager gameManager;
    public PhaseHandler(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public abstract void Execute();
}
