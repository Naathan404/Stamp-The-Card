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

    public override void ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        if (!isEnabled) return;

        switch (battleRuleType)
        {
            case BattleRuleType.JUDGE:
                ApplyJudge(myCards, enemyCards, currentCardIndex);
                break;

            case BattleRuleType.KING_OF_TOUGHNESS:
                myCards[currentCardIndex].IsKingOfToughness = true;
                Debug.Log($"[Vua Lì Đòn] Slot {currentCardIndex} được miễn sát thương nếu thua cột này");
                break;

            case BattleRuleType.REVERSE_BALANCE:
                myCards[currentCardIndex].IsReverseBalance = true;
                Debug.Log($"[Đảo Ngược Cán Cân] Slot {currentCardIndex} - bên điểm cao hơn sẽ bị trừ máu");
                break;

            case BattleRuleType.PEACE_AMULET:
                myCards[currentCardIndex].HasPeaceAmulet = true;
                Debug.Log($"[Bùa Bình An] Slot {currentCardIndex} sẽ được cứu nếu máu về 0");
                break;

            case BattleRuleType.CREMATION:
                ApplyCremation(enemyCards, currentCardIndex);
                break;
        }
    }

    /// Thẩm Phán: vô hiệu toàn bộ stamp cả 2 lá trên cột này -> về so sánh điểm gốc
    private void ApplyJudge(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        CardSlot mySlot    = myCards[currentCardIndex];
        CardSlot enemySlot = enemyCards[2 - currentCardIndex];

        // Vô hiệu tất cả stamp trên cả 2 lá (trừ chính stamp Thẩm Phán này)
        foreach (var stamp in mySlot.Stamps)
        {
            if (stamp != this) stamp.isEnabled = false;
        }
        foreach (var stamp in enemySlot.Stamps)
        {
            stamp.isEnabled = false;
        }

        // Reset điểm về gốc
        mySlot.Score = mySlot.Data.BaseScore;
        enemySlot.Score = enemySlot.Data.BaseScore;

        // Đánh dấu để các stamp sau không chạy nữa
        mySlot.StampsDisabled    = true;
        enemySlot.StampsDisabled = true;

        Debug.Log($"[Thẩm Phán] Cột {currentCardIndex} bị vô hiệu toàn bộ stamp, về điểm gốc");
    }

    /// Hỏa Thiêu: chọn 1 lá random của đối thủ → đánh dấu IsIgnored lượt này
    /// Lượt sau lá đó và lá bài này vẫn chiếm slot nhưng không tác dụng
    private void ApplyCremation(CardSlot[] enemyCards, int currentCardIndex)
    {
        // Lọc những lá chưa bị Ignored
        System.Collections.Generic.List<int> validTargets = new System.Collections.Generic.List<int>();
        for (int i = 0; i < 3; i++)
        {
            if (!enemyCards[i].IsIgnored)
                validTargets.Add(i);
        }

        if (validTargets.Count == 0)
        {
            Debug.Log("[Hỏa Thiêu] Không còn lá nào để đốt");
            return;
        }

        int randomIndex = validTargets[Random.Range(0, validTargets.Count)];
        enemyCards[randomIndex].IsIgnored = true;

        // Vô hiệu toàn bộ stamp trên lá bị đốt
        foreach (var stamp in enemyCards[randomIndex].Stamps)
        {
            stamp.isEnabled = false;
        }

        Debug.Log($"[Hỏa Thiêu] Đốt lá {randomIndex} của đối thủ");
    }
}