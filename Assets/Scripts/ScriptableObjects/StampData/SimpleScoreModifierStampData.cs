using System;
using System.Linq;
using UnityEngine;

public enum ScoreOperator 
{ 
    ADD,            //Phep cong
    MULTIPLY,       //Phep nhan
}

[CreateAssetMenu(fileName = "New Stamp", menuName = "Stamp The Card/Stamp Data/SimpleScoreModifierStamp")]

public class SimpleScoreModifierStampData : BaseStampData
{
    [Header("The score operator")]
    public float amountToChange;
    public ScoreOperator scoreOperator;

    public override string ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        if (!isEnabled) return "NoEffect";

        ApplyToTargets(myCards, enemyCards, currentCardIndex);
        return "ScoreChanged";
    }
    

    protected void ApplyToTargets(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex, float? valueOverride = null)
    {
        var uniqueTargets = targets.Distinct();                 //Dam bao cac target phan biet nhau trong truong hop nhap du lieu trung lap
        
        float finalValue = valueOverride.HasValue ? valueOverride.Value : amountToChange;

        foreach (var target in uniqueTargets)
        {
            CardSlot targetSlot = null;

            switch (target)
            {
                case Target.SELF:
                    targetSlot = myCards[currentCardIndex]; break;
                case Target.RIGHT:
                    if (currentCardIndex < myCards.Length - 1)
                        targetSlot = myCards[currentCardIndex + 1]; break;
                case Target.LEFT:
                    if (currentCardIndex > 0)
                        targetSlot = myCards[currentCardIndex - 1]; break;
                case Target.OPPOSITE:
                    targetSlot = enemyCards[2 - currentCardIndex]; break;
                case Target.ALL_ENEMY_CARDS:
                    foreach (var card in enemyCards)
                    {
                        ApplyScoreOperator(card, finalValue, scoreOperator);
                    }
                    continue;
                case Target.ALL_ALLY_CARDS:
                    foreach (var card in myCards)
                    {
                        ApplyScoreOperator(card, finalValue, scoreOperator);
                    }
                    continue;
            }

            if (targetSlot != null)
                ApplyScoreOperator(targetSlot, finalValue, scoreOperator);
        }
    }

    protected void ApplyScoreOperator(CardSlot targetSlot, float value, ScoreOperator op)
    {
        if (targetSlot == null || targetSlot.Data == null) return;

        if (targetSlot.IsImmuneLowerScore)
        {
            if (op == ScoreOperator.ADD && value < 0) return;
            if (op == ScoreOperator.MULTIPLY && value < 1) return;
        }

        float result;
        switch (op)
        {
            case ScoreOperator.ADD:
                result = targetSlot.Score + value;
                targetSlot.Score = (int)result; break;
            case ScoreOperator.MULTIPLY:
                result = targetSlot.Score * value;
                targetSlot.Score = (int)Math.Round(result, MidpointRounding.AwayFromZero); break;
        }
    }
}
