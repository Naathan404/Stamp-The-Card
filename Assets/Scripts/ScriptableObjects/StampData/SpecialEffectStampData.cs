using UnityEngine;
using System.Collections.Generic;

public enum EffectType 
{
    NULLIFY_STAMP_EFFECT,       //Vo hieu hoa stamp (Tham Phan, Ke nhanh nhau)
    COPY,                       //(Guong)
    IMMUNITY,                   //(Ao Choang)
    RESET_SCORE,                //Reset score ve basescore (Rua Toi, Thanh Tay)
    RANDOM_SCORE_CHANGE         //Thay doi score random (An May)
}

[CreateAssetMenu(fileName = "New Stamp", menuName = "Stamp The Card/Stamp Data/SepcialEffectStamp")]
public class SpecialEffectStampData : BaseStampData
{
    public EffectType effectType;

    public override void ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        if (!isEnabled) return;

        switch (effectType)
        {
            case EffectType.NULLIFY_STAMP_EFFECT:
                foreach (var target in targets)
                {
                    var targetToCheck = FindTargetToCheck(target, myCards, enemyCards, currentCardIndex);
                    if (targetToCheck != null)
                        NullifyStampEffect(targetToCheck);
                }
                break;

            case EffectType.COPY:
                List<BaseStampData> targetStampList = enemyCards[2 - currentCardIndex].Stamps;      //Lay danh sach stamp dong tren card doi dien
                BaseStampData targetToCopy = targetStampList[targetStampList.Count - 1];
                if (targetToCopy != null && targetToCopy.stampName != this.stampName)
                {
                    targetToCopy.ApplyEffect(myCards, enemyCards, currentCardIndex);
                }
                break;

            case EffectType. IMMUNITY:
                myCards[currentCardIndex].IsImmuneLowerScore = true;
                break;

            case EffectType.RESET_SCORE:
                foreach (var target in targets)
                {
                    var targetToCheck = FindTargetToCheck(target, myCards, enemyCards, currentCardIndex);
                    if (targetToCheck != null)
                        ResetScore(targetToCheck);
                }
                break;

            case EffectType.RANDOM_SCORE_CHANGE:
                CardSlot currentCard = myCards[currentCardIndex];
                currentCard.Score += currentCard.LastRandomValue;
                break;
        }
    }

    private void NullifyStampEffect(CardSlot currentCard)
    {
        var stampList = currentCard.Stamps;

        foreach (var stamp in stampList)
        {
            if (stamp != this)                  //Dam bao khong tu vo hieu hoa ban than
                stamp.isEnabled = false;
        }
    }

    private void ResetScore(CardSlot currentCard)
    {
        currentCard.Score = currentCard.Data.BaseScore;
    }
}
