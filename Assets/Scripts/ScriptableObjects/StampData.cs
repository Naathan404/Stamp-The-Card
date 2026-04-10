using UnityEngine;

public enum StampRank 
{
    COMMON,
    RARE,
    EPIC,
    LEGENDARY
}
public enum StampType
{
    GREEN,
    RED
}


[CreateAssetMenu(fileName = "New Stamp", menuName = "Stamp The Card/Stamp Data")]
public class StampData : ScriptableObject
{
    [Header("Stamp Info")]
    [SerializeField] private StampRank _stampRank;
    [SerializeField] private string _stampName;
    [SerializeField] private StampType _stampType;
    [TextArea(3, 10)]
    [SerializeField] private string _stampEffect;

    public Sprite stampArt;
    public StampRank stampRank => _stampRank;
    public string stampName => _stampName;
    public StampType stampType => _stampType;
    public string stampEffect => _stampEffect;
}
