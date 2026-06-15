using System.Collections.Generic;
using UnityEngine;


public class CardSlot : MonoBehaviour
{
    [Header("Card Data")]
    public int Index = 0;
    public CardData Data = null;
    public int Score = 0;
    public bool IsBottom = true;

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
