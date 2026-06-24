using UnityEngine;

[CreateAssetMenu(fileName = "New Stamp", menuName = "Stamp The Card/Stamp Data/VariableScoreModifierStamp")]
public class VariableScoreModifierStampData : SimpleScoreModifierStampData
{
    public override string ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        if (!isEnabled) return "NoEffect";

        float valueToChange = 0;
        CardSlot targetToCount = FindTargetToCheck(targets[0], myCards, enemyCards, currentCardIndex);
        
        if (targetToCount != null && targetToCount.Data != null)
        {
            valueToChange = GetStampCount(targetToCount);
            if (amountToChange < 0) valueToChange = -valueToChange;                 
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
            if (targetToChange != null && targetToChange.Data != null)
            {
                ApplyScoreOperator(targetToChange, valueToChange, scoreOperator);
            }
        }

        return "ScoreChanged";
    }

    private int GetStampCount(CardSlot currentCard)
    {
        int count = 0;
        int startIndex = currentCard.Data.CardID * 3;
        for(int i = 0; i < 3; i++)
        {
            if(GameManager.Instance.CardAttachedStamps[startIndex + i] > 0)
            {
                count++;
            }
        }
        return count;
    }
}