using UnityEngine;

[CreateAssetMenu(fileName = "TwistedFateChar", menuName = "Stamp The Card/Character Data/TwistedFateChar")]
public class TwistedFateChar : BaseCharacter
{
    public override void TakeDamage(int damage)
    {
        
    }

    public override bool ApplySkills(BaseCharacter target, GameManager gameManager, GameStateManager gameStateManager)
    {
        throw new System.NotImplementedException();
    }
}
