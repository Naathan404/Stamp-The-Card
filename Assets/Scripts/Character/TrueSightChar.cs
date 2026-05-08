using UnityEngine;

[CreateAssetMenu(fileName = "TrueSightChar", menuName = "Stamp The Card/Character Data/TrueSightChar")]
public class TrueSightChar : BaseCharacter
{
    public override void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            isDead = true;
        }
    }

    public override bool ApplySkills(BaseCharacter target, GameManager gameManager, GameStateManager gameStateManager)
    {
        if (canActivateSkill)
        {
            if (gameStateManager.CurrentGameState == GameStateManager.GamePhase.MainPhase)
            {
                skillChecked = true;
                canActivateSkill = false;
                return true;
            }
        }
        return false;
    }
}
