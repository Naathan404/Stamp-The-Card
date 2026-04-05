using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public enum CardType
{
    CARD_A = 1, CARD_2 = 2, CARD_3 = 3, 
    CARD_4 = 4, CARD_5 = 5, CARD_6 = 6, 
    CARD_7 = 7, CARD_8 = 8, CARD_9 = 9, 
    Card_J = 11,Card_Q = 12, Card_K = 13, 
    CARD_JOKER = 0
}
public enum CardColor
{
    RED,
    BLACK
}


[CreateAssetMenu(fileName = "New Card", menuName = "Stamp The Card/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    [SerializeField] private CardType _cardType;
    [SerializeField] private CardColor _cardColor;
    [SerializeField] private int _baseScore;
    public Sprite Artwork;

    public CardType CardType => _cardType;
    public CardColor CardColor => _cardColor;
    public int BaseScore => _baseScore;                            //Số nút mặc định của lá bài
}
