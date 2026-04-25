using UnityEngine;


[CreateAssetMenu(fileName = "New Stamp", menuName = "Stamp The Card/Stamp Data/VariableScoreModifierStamp")]
public class VariableScoreModifierStampData : SimpleScoreModifierStampData
{

    public override void ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        if (!isEnabled) return;

        float valueToChange = 0;
        CardSlot targetToCount = FindTargetToCheck(targets[0], myCards, enemyCards, currentCardIndex);
        if (targetToCount != null && targetToCount.data != null)
        {
            valueToChange = targetToCount.stamps.Count;
            if (amountToChange < 0) valueToChange = -valueToChange;                 //Danh dau de biet day la phep tru
        }


        if (targets[1] == Target.ALL_ENEMY_CARDS)
        {
            foreach (var card in enemyCards)
            {
                ApplyScoreOperator(card, valueToChange, scoreOperator);
            }
        }
        else
        {
            CardSlot targetToChange = FindTargetToCheck(targets[1], myCards, enemyCards, currentCardIndex);
            if (targetToChange != null && targetToChange.data != null)
            {
                ApplyScoreOperator(targetToChange, valueToChange, scoreOperator);
            }
        }
    }
}
