using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    [SerializeField] private CardData[] _allCards;
    [SerializeField] private BaseStampData[] _allStamps;

    private Dictionary<int, CardData> _cardDataDict = new Dictionary<int, CardData>();
    private Dictionary<int, BaseStampData> _stampDataDict = new Dictionary<int, BaseStampData>();

    private void Start()
    {
        InitDatabase();
    }

    private void InitDatabase()
    {
        foreach (var card in _allCards)
        {
            _cardDataDict[card.CardID] = card;
        }

        foreach (var stamp in _allStamps)
        {
            _stampDataDict[stamp.GetInstanceID()] = stamp;
        }
    }

    public CardData GetCardDataByID(int id)
    {
        if (_cardDataDict.TryGetValue(id, out var cardData))
        {
            return cardData;
        }
        else
        {
            Debug.LogError($"Card with ID {id} not found!");
            return null;
        }
    }

    public BaseStampData GetStampDataByID(int id)
    {
        if (_stampDataDict.TryGetValue(id, out var stampData))
        {
            return stampData;
        }
        else
        {
            Debug.LogError($"Stamp with ID {id} not found!");
            return null;
        }
    }
}
