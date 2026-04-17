using UnityEngine;


[CreateAssetMenu(fileName = "New Stamp", menuName = "Stamp The Card/Stamp Data/ScoreTransferStamp")]
public class ScoreTransferStampData : BaseStampData
{
    public int amountToTransfer;

    public override void ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex) 
    {
        if (!isEnabled || targets == null || targets.Length < 2) return;

        //Mac dinh target dau tien receive score va cac target con lai bi steal score
        CardSlot targetToReceive = FindTargetToCheck(targets[0], myCards, enemyCards, currentCardIndex);
        if (targetToReceive == null) return;
        CardSlot targetToSteal = null;

        for (int i = 1; i < targets.Length; i++)
        {
            targetToSteal = FindTargetToCheck(targets[i], myCards, enemyCards, currentCardIndex);
            if (targetToSteal != null && !targetToSteal.isImmuneLowerScore)
            {
                targetToSteal.score -= amountToTransfer;
                targetToReceive.score += amountToTransfer;
            }
        }
    }
}
