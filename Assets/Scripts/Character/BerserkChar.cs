using UnityEngine;

[CreateAssetMenu(fileName = "BerserkChar", menuName = "Stamp The Card/Character Data/BerserkChar")]
public class BerserkChar : BaseCharacter
{
    public override bool ApplySkills(BaseCharacter target, GameManager gameManager, GameStateManager gameStateManager)
    {
        throw new System.NotImplementedException();
    }
    public override void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            isDead = true;
        }
    }
}
