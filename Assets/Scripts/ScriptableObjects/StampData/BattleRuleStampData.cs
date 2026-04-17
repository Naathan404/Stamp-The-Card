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

    public override void ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex) { }
}
