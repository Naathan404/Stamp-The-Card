using UnityEngine;

public enum BattleRuleType 
{
    JUDGE,                  //Tham Phan
    KING_OF_TOUGHNESS,      //Vua Li Don
    REVERSE_BALANCE,        //Dao Nguoc Can Can
    PEACE_AMULET,           //Bua Binh An
    CREMATION               //Hoa Thieu
}

[CreateAssetMenu(fileName = "New Stamp", menuName = "Stamp The Card/Stamp Data/BattleRuleStamp")]
public class BattleRuleStampData : BaseStampData
{
    public BattleRuleType battleRuleType;

    public override void ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
    switch (battleRuleType)
    {
        case BattleRuleType.JUDGE: // Thẩm Phán
            // Vô hiệu toàn bộ stamp trên cột này (cả 2 bên)
            myCards[currentCardIndex].StampsDisabled = true;
            enemyCards[2 - currentCardIndex].StampsDisabled = true;
            // Reset về BaseScore
            myCards[currentCardIndex].Score = myCards[currentCardIndex].Data.BaseScore;
            enemyCards[2 - currentCardIndex].Score = enemyCards[2 - currentCardIndex].Data.BaseScore;
            break;

        // case BattleRuleType.KING_OF_TOUGHNESS: // Vua Lì Đòn
        //     myCards[currentCardIndex].IsKingOfToughness = true; // check ở End Phase
        //     break;

        // case BattleRuleType.REVERSE_BALANCE: // Đảo Ngược Cán Cân
        //     myCards[currentCardIndex].IsReverseBalance = true; // check ở End Phase
        //     break;

        // case BattleRuleType.PEACE_AMULET: // Bùa Bình An — resolve ở End Phase, không làm gì ở đây
        //     myCards[currentCardIndex].HasPeaceAmulet = true;
        //     break;

        // case BattleRuleType.CREMATION: // Hỏa Thiêu — cần xử lý riêng vì chọn lá random
        //     // Xử lý ở pre-resolve trước khi Calculate bắt đầu
    }
    }
}
