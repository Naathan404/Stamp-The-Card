using UnityEngine;

public enum StampRank
{
    COMMON,
    RARE,
    EPIC,
    LEGENDARY
}
public enum Target
{
    SELF,
    RIGHT,
    LEFT,
    OPPOSITE,
    ALL_ENEMY_CARDS,
}

public abstract class BaseStampData : ScriptableObject
{
    [Header("Stamp Info")]
    [SerializeField] private StampRank _stampRank;
    [SerializeField] private string _stampName;
    [TextArea(3, 10)]
    [SerializeField] private string _stampEffect;
    [SerializeField] protected Target[] targets;

    public Sprite stampArt;
    public StampRank stampRank => _stampRank;
    public string stampName => _stampName;
    public string stampEffect => _stampEffect;
    public bool isEnabled = true;                           //De dam bao isEnabled hoat dong dung --> Moi lan co stamp moi dong vao card, reset card ve ban dau va ApplyEffect lai tu dau


    // Ham goi khi kich hoat stamp
    public abstract void ApplyEffect(CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex);

    protected CardSlot FindTargetToCheck(Target target, CardSlot[] myCards, CardSlot[] enemyCards, int currentCardIndex)
    {
        switch (target)
        {
            case Target.SELF:
                return myCards[currentCardIndex];
            case Target.OPPOSITE:
                return enemyCards[2 - currentCardIndex];
            case Target.RIGHT:
                if (currentCardIndex < myCards.Length - 1)
                    return myCards[currentCardIndex + 1];
                else
                    return null;
            case Target.LEFT:
                if (currentCardIndex > 0)
                    return myCards[currentCardIndex - 1];
                else
                    return null;
            default:
                return null;
        }
    }
}
