using System.Collections.Generic;
using UnityEngine;


public class CardSlot : MonoBehaviour
{
    [Header("Card Data")]
    public int Index;
    public CardData Data;
    public int Score;
    public List<BaseStampData> Stamps = new List<BaseStampData>();

    [Header("Components")]
    public List<SpriteRenderer> StampRenderers = new List<SpriteRenderer>(); 

    [Header("Flags")]
    public bool IsIgnored = false;              // Hỏa Thiêu: lá bài bị loại khỏi trận lượt này
    public bool StampsDisabled = false;         // Thẩm Phán: toàn bộ stamp trên lá bị vô hiệu
    public bool IsImmuneLowerScore = false;     // Áo Choàng: miễn nhiễm hiệu ứng trừ điểm
    public bool IsReverseBalance = false;       // Đảo Ngược Cán Cân: bên cao hơn bị trừ máu
    public bool IsKingOfToughness = false;      // Vua Lì Đòn: sát thương nhận vào = 0 nếu thua cột này
    public bool HasPeaceAmulet = false;         // Bùa Bình An: nếu máu về 0 thì kích hoạt

    [Header("Networked Values")]
    public int LastRandomValue = 0;             // Ăn May: nhận từ Host, tránh desync

 
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

    public void Reset()
    {
        Score = Data.BaseScore;
        IsIgnored = false;              
        StampsDisabled = false;        
        IsImmuneLowerScore = false;     
        IsReverseBalance = false;       
        IsKingOfToughness = false;      
        HasPeaceAmulet = false;   
        LastRandomValue = 0;
    }
}
