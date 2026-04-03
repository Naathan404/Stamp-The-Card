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
    [Header("Thông tin card")]
    [SerializeField] private CardType _cardType;
    [SerializeField] private CardColor _cardColor;

    public CardType cardType => _cardType;
    public CardColor cardColor => _cardColor;
    public int defaultCardNumber => (int)_cardType;                            //Số nút mặc định của lá bài
  
}
