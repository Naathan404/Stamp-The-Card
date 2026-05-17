using UnityEngine;

[CreateAssetMenu(fileName = "AegisChar", menuName = "Stamp The Card/Character Data/AegisChar")]
public class AegisChar : BaseCharacter
{
    private bool isNotDamaged = false;

    public override bool ApplySkills(BaseCharacter target, GameManager gameManager, GameStateManager gameStateManager)
    {
        if (canActivateSkill)
        {
            if (gameStateManager.CurrentGameState == GameStateManager.GamePhase.CalculatePhase)
            {
                isNotDamaged = true;
                canActivateSkill = false;
                return true;
            }
        }
        return false;
    }

    public override void TakeDamage(int damage)
    {
        if (!isNotDamaged)
        {
            health -= damage;
            isNotDamaged = false;
        }
        if (health <= 0)
        {
            isDead = true;
        }
    }
}
