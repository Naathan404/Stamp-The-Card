using UnityEngine;
using System.Collections.Generic;

public enum EffectType 
{
    NULLIFY_STAMP_EFFECT,       //Vo hieu hoa stamp (Tham Phan, Ke nhanh nhau)
    COPY,                       //(Guong)
    IMMUNITY,                   //(Ao Choang)
    RESET_SCORE,                //Reset score ve basescore (Rua Toi, Thanh Tay)
    RANDOM_SCORE_CHANGE,         //Thay doi score random (An May)
    BALANCE_SCORE               //(Can Can Cong Bang)
}

[CreateAssetMenu(fileName = "New Stamp", menuName = "Stamp The Card/Stamp Data/SepcialEffectStamp")]
public class SpecialEffectStampData : BaseStampData
{
    public EffectType effectType;

    public override string ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        if (!isEnabled) return "NoEffect";

        switch (effectType)
        {
            case EffectType.NULLIFY_STAMP_EFFECT:
                foreach (var target in targets)
                {
                    var targetToCheck = FindTargetToCheck(target, myCards, enemyCards, currentCardIndex);
                    if (targetToCheck != null)
                        NullifyStampEffect(targetToCheck);
                }
                return "SILENCED";

            case EffectType.COPY:
                CardSlot oppositeCard = enemyCards[2 - currentCardIndex];
                int startIndex = oppositeCard.Data.CardID * 3;
                BaseStampData targetToCopy = null;
                
                // Quét ngược từ ô số 2 về ô số 0 để lấy con tem cuối cùng được đóng
                for (int i = 2; i >= 0; i--)
                {
                    int sID = GameManager.Instance.CardAttachedStamps[startIndex + i];
                    if (sID > 0)
                    {
                        targetToCopy = DataManager.Instance.GetStampDataByID(sID);
                        break;
                    }
                }

                if (targetToCopy != null && targetToCopy.stampName != this.stampName)
                {
                    return targetToCopy.ApplyEffect(myCards, enemyCards, currentCardIndex);
                }
                break;

            case EffectType.IMMUNITY:
                myCards[currentCardIndex].IsImmuneLowerScore = true;
                myCards[currentCardIndex].UpdateStatusUI();
                return "IMMUNE";

            case EffectType.RESET_SCORE:
                foreach (var target in targets)
                {
                    var targetToCheck = FindTargetToCheck(target, myCards, enemyCards, currentCardIndex);
                    if (targetToCheck != null)
                        ResetScore(targetToCheck);
                }
                return "RESET";

            case EffectType.RANDOM_SCORE_CHANGE:
                CardSlot currentCard = myCards[currentCardIndex];
                currentCard.Score += currentCard.LastRandomValue;
                return "ScoreChanged";

            case EffectType.BALANCE_SCORE:
                myCards[currentCardIndex].Score = enemyCards[2 - currentCardIndex].Data.BaseScore;
                return "ScoreChanged";
        }

        return "NoEffect";
    }

    private void NullifyStampEffect(CardSlot currentCard)
    {
        int startIndex = currentCard.Data.CardID * 3;
        bool hasDisabledAny = false;
        for (int i = 0; i < 3; i++)
        {
            int stampID = GameManager.Instance.CardAttachedStamps[startIndex + i];
            if (stampID > 0)
            {
                BaseStampData stampData = DataManager.Instance.GetStampDataByID(stampID);
                if (stampData != null && stampData != this)
                {
                    //stampData.isEnabled = false;
                    hasDisabledAny = true;
                }
            }
        }
        if (hasDisabledAny)
        {
            currentCard.StampsDisabled = true;
            currentCard.UpdateStatusUI();
        }
    }

    private void ResetScore(CardSlot currentCard)
    {
        currentCard.Score = currentCard.Data.BaseScore;
    }
}
