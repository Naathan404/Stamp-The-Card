using System.Linq;
using UnityEngine;

public enum Condition 
{
    IS_EVEN,                            //Kiem tra so nut mac dinh la so chan (Dao Phu, Ke Do Te)
    IS_ODD,                             //Kiem tra so nut mac dinh la so le (Tho San, Ke Hanh Quyet)
    IS_STAMPED,                         //Kiem tra card da dong stamp chua (Am Sat)
    IS_NOT_HIGHER_THAN_5,               //Kiem tra so nut <= 5 (Ke yeu kem)
    IS_CENTERED,                        //Kiem tra vi tri card nam giua (Tran thu)
    IS_NOT_CENTERED,                    //Kiem tra vi tri card khong nam giua (Dot kich)
    IS_MYSCORE_LOWER_THAN_ENEMYSCORE,   //Kiem tra tong so nut ta < tong so nut dich (Hiep si)
    DISABLE_OTHER_STAMPS                //Vo hieu cac stamp khac (Xung kich, Boc Thep)
}

[CreateAssetMenu(fileName = "New Stamp", menuName = "Stamp The Card/Stamp Data/ConditionalScoreModifierStamp")]
public class ConditionalScoreModifierStampData : SimpleScoreModifierStampData
{
    [Header("Condition to trigger stamp effect")]
    public Condition Condition;

    public override void ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        if (!isEnabled) return;

        if (CheckCondition(myCards, enemyCards, currentCardIndex))
            ApplyToTargets(myCards, enemyCards, currentCardIndex);
    }

    private bool CheckCondition(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        CardSlot targetToCheck = FindTargetToCheck(targets[0], myCards, enemyCards, currentCardIndex);

        if (targetToCheck == null || targetToCheck.Data == null) return false;

        switch (Condition)
        {
            case Condition.IS_EVEN:
                return targetToCheck.Data.BaseScore % 2 == 0;

            case Condition.IS_ODD:
                return targetToCheck.Data.BaseScore % 2 != 0;

            case Condition.IS_STAMPED:
                bool isStamped = GetStampCount(targetToCheck) > 0;
                amountToChange = isStamped ? -1 : -3;
                return true;

            case Condition.IS_NOT_HIGHER_THAN_5:
                 return targetToCheck.Data.BaseScore <= 5;

             case Condition.IS_CENTERED:
                 return currentCardIndex == 1;

             case Condition.IS_NOT_CENTERED:
                 return currentCardIndex != 1;

             case Condition.IS_MYSCORE_LOWER_THAN_ENEMYSCORE:
                 int myScore = 0;
                 int enemyScore = 0;
                 foreach (var card in myCards)
                     myScore += card.Score;
                 foreach (var card in enemyCards)
                     enemyScore += card.Score;
                 return myScore < enemyScore;

             case Condition.DISABLE_OTHER_STAMPS:
                  // Sửa lỗi: Dò Server để vô hiệu hóa tem
                  int startIndex = targetToCheck.Data.CardID * 3;
                  for (int i = 0; i < 3; i++)
                  {
                      int stampID = GameManager.Instance.CardAttachedStamps[startIndex + i];
                      if (stampID > 0)
                      {
                          BaseStampData stampData = DataManager.Instance.GetStampDataByID(stampID);
                          if (stampData != null && stampData.stampName != this.stampName)
                          {
                              stampData.isEnabled = false;
                          }
                      }
                  }
                  return true;

             default:
                  return false;
             }
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