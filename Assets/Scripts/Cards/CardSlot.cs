using System.Collections.Generic;
using UnityEngine;


public class CardSlot : MonoBehaviour
{
    public CardData data;
    public int score;
    public List<BaseStampData> stamps = new List<BaseStampData>();
    public bool isImmuneLowerScore = false;             //Kiem tra xem co mien nhiem viec tru so nut --> Dung cho stamp "Ao Choang"
    public int lastRandomValue = 0;                     //Luu gia tri random nhan tu Network --> Dung cho stamp "An May"
    public bool isIgnored = false;                      //La bai bi vo hieu --> Dung cho stamp "Hoa thieu"

    //Ham cap nhat so nut cua la bai
    public void UpdateUI()
    {
    
    }
}
