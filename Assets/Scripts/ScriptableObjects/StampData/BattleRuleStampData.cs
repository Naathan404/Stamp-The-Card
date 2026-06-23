using UnityEngine;

public enum BattleRuleType 
{
    JUDGE,                  // Thẩm Phán
    KING_OF_TOUGHNESS,      // Vua Lì Đòn
    REVERSE_BALANCE,        // Đảo Ngược Cán Cân
    PEACE_AMULET,           // Bùa Bình An
    CREMATION               // Hỏa Thiêu
}

[CreateAssetMenu(fileName = "New Stamp", menuName = "Stamp The Card/Stamp Data/BattleRuleStamp")]
public class BattleRuleStampData : BaseStampData
{
    public BattleRuleType battleRuleType;

    public override string ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        if (!isEnabled) return "NoEffect";

        switch (battleRuleType)
        {
            case BattleRuleType.JUDGE:
                ApplyJudge(myCards, enemyCards, currentCardIndex);
                return "JUDGED";

            case BattleRuleType.KING_OF_TOUGHNESS:
                myCards[currentCardIndex].IsKingOfToughness = true;
                Debug.Log($"[Vua Lì Đòn] Slot {currentCardIndex} được miễn sát thương nếu thua cột này");
                return "TOUGH";

            case BattleRuleType.REVERSE_BALANCE:
                myCards[currentCardIndex].IsReverseBalance = true;
                Debug.Log($"[Đảo Ngược Cán Cân] Slot {currentCardIndex} - bên điểm cao hơn sẽ bị trừ máu");
                return "REVERSE";

            case BattleRuleType.PEACE_AMULET:
                myCards[currentCardIndex].HasPeaceAmulet = true;
                Debug.Log($"[Bùa Bình An] Slot {currentCardIndex} sẽ được cứu nếu máu về 0");
                return "AMULET";

            case BattleRuleType.CREMATION:
                ApplyCremation(enemyCards, currentCardIndex);
                return "BURN";
        }

        return "NoEffect";
    }

/// Thẩm Phán: vô hiệu toàn bộ stamp cả 2 lá trên cột này -> về so sánh điểm gốc
    private void ApplyJudge(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        CardSlot mySlot    = myCards[currentCardIndex];
        CardSlot enemySlot = enemyCards[2 - currentCardIndex];

        // Tra cứu Sổ Cái và vô hiệu hóa (trừ chính cái tem Thẩm Phán này)
        NullifyStampsOnCard(mySlot, this);
        NullifyStampsOnCard(enemySlot, null);

        // Reset điểm về gốc
        mySlot.Score = mySlot.Data.BaseScore;
        enemySlot.Score = enemySlot.Data.BaseScore;

        // Đánh dấu để các stamp sau không chạy nữa
        mySlot.StampsDisabled    = true;
        enemySlot.StampsDisabled = true;

        Debug.Log($"[Thẩm Phán] Cột {currentCardIndex} bị vô hiệu toàn bộ stamp, về điểm gốc");
    }

    /// Hỏa Thiêu: chọn 1 lá random của đối thủ → đánh dấu IsIgnored lượt này
    private void ApplyCremation(CardSlot[] enemyCards, int currentCardIndex)
    {
        System.Collections.Generic.List<int> validTargets = new System.Collections.Generic.List<int>();
        for (int i = 0; i < 3; i++)
        {
            if (!enemyCards[i].IsIgnored) validTargets.Add(i);
        }

        if (validTargets.Count == 0) return;

        int randomIndex = validTargets[Random.Range(0, validTargets.Count)];
        enemyCards[randomIndex].IsIgnored = true;

        NullifyStampsOnCard(enemyCards[randomIndex], null);

        Debug.Log($"[Hỏa Thiêu] Đốt lá {randomIndex} của đối thủ");
    }

    private void NullifyStampsOnCard(CardSlot cardSlot, BaseStampData exceptionStamp)
    {
        int startIndex = cardSlot.Data.CardID * 3;
        for (int i = 0; i < 3; i++)
        {
            int stampID = GameManager.Instance.CardAttachedStamps[startIndex + i];
            if (stampID > 0)
            {
                BaseStampData stampData = DataManager.Instance.GetStampDataByID(stampID);
                if (stampData != null && stampData != exceptionStamp)
                {
                    stampData.isEnabled = false;
                }
            }
        }
    }
}