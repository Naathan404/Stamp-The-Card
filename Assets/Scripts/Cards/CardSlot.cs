using System.Collections.Generic;
using UnityEngine;


public class CardSlot : MonoBehaviour
{
    public CardData Data;
    public int Score;
    public List<BaseStampData> Stamps = new List<BaseStampData>();
    public List<SpriteRenderer> StampRenderers = new List<SpriteRenderer>(); 
    public bool IsImmuneLowerScore = false;             //Kiem tra xem co mien nhiem viec tru so nut --> Dung cho stamp "Ao Choang"
    public int LastRandomValue = 0;                     //Luu gia tri random nhan tu Network --> Dung cho stamp "An May"
    public bool IsIgnored = false;                      //La bai bi vo hieu --> Dung cho stamp "Hoa thieu"

    //Ham cap nhat so nut cua la bai
    public void UpdateUI()
    {
        for (int i = 0; i < StampRenderers.Count; i++)
        {
            if (i < Stamps.Count)
            {
                StampRenderers[i].sprite = Stamps[i].stampArt;
                StampRenderers[i].enabled = true;
            }
            else
            {
                StampRenderers[i].enabled = false;
            }
        }
    }

    public void ApplyStamp(BaseStampData stamp)
    {
        Stamps.Add(stamp);
        UpdateUI();
    }
}
