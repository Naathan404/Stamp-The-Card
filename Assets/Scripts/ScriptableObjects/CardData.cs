using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public enum CardType
{
    Card_A = 1, Card_2 = 2, Card_3 = 3, Card_4 = 4,
    Card_5 = 5, Card_6 = 6, Card_7 = 7, Card_8 = 8,
    Card_9 = 9, Card_10 = 10, Card_J = 10,Card_Q = 10,
    Card_K = 10, Card_Joker = 0
}
public enum CardColor
{
    RED,
    BLACK
}


[CreateAssetMenu(fileName = "Card", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    private const int defaultCardNumber = -1000;                                        //Giá trị mặc định của card number

    [Header("Thông tin card")]
    [SerializeField] private CardType _cardType;
    [SerializeField] private CardColor _cardColor;
    private int _cardNumber = defaultCardNumber;

    [Header("Stamp đóng vào card")]
    [SerializeField] private List<StampData> _stampList = new List<StampData>(); 


    public CardType cardType => _cardType;
    public CardColor cardColor => _cardColor;
    public int cardNumber                                                               //Số nút của lá bài
    {
        get
        {
            if (_cardNumber == defaultCardNumber)
            {
                _cardNumber = (int)_cardType;
            }
            return _cardNumber;
        }
        set
        {
            if (_cardNumber != value)
            {
                _cardNumber = value;
            }
        }
    }
    public List<StampData> stampList => _stampList;

    
    // Hàm reset card number khi chỉnh sửa trong Unity Editor
    private void OnValidate()
    {
        _cardNumber = (int)_cardType;
    }

    //Hàm thêm stamp đóng vào card
    public bool AddStamp (StampData newStamp)
    {
        if (_cardType == CardType.Card_Joker)
        {
            if (_stampList.Count >= 1)
            {
                Debug.LogWarning("Lá joker này đã có stamp, vui lòng xóa stamp trước khi thêm mới");
                return false;
            }
        }    

        if (_stampList.Count >= 3)
        {
            Debug.LogWarning("Card này đã đóng đủ 3 stamp");
            return false;
        }

        _stampList.Add(newStamp);
        return true;
    }

    //Hàm xóa stamp khỏi card theo index
    public bool RemoveStamp (int index)
    {
        if (_stampList.Count <= 0)
        {
            Debug.LogWarning("Card này chưa có stamp nào!");
            return false;
        }

        if (index >= 0 && index < _stampList.Count)
        {
            _stampList.RemoveAt(index);
            return true;
        }
        
        return false;
    }
}
